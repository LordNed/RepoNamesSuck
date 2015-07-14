using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using WEditor.Maps;
using WEditor.WindWaker;

namespace WEditor.Rendering
{
    public class RenderSystem
    {
        private List<Camera> m_cameraList;
        private WWorld m_world;

        private Shader m_debugShader;

        public RenderSystem(WWorld world)
        {
            m_cameraList = new List<Camera>();
            m_world = world;
        }

        public void InitializeSystem()
        {
            // Create a Default camera
            Camera editorCamera = new Camera();
            editorCamera.ClearColor = new Color(0.4f, 0.1f, 1f, 1f);

            EditorCameraMovement camMovement = new EditorCameraMovement();
            camMovement.Camera = editorCamera;
            m_world.RegisterComponent(camMovement);

            m_cameraList.Add(editorCamera);

            // Create a shader for drawing debug primitives/instances.
            m_debugShader = new Shader("DebugPrimitives");
            m_debugShader.CompileSource(File.ReadAllText("RenderSystem/Shaders/frag.glsl"), ShaderType.FragmentShader);
            m_debugShader.CompileSource(File.ReadAllText("RenderSystem/Shaders/vert.glsl"), ShaderType.VertexShader);
            m_debugShader.LinkShader();
        }

        public void ShutdownSystem()
        {
            UnloadAll();
            m_debugShader.Dispose();
            m_debugShader = null;
        }

        internal void RenderFrame()
        {
            // Solid Fill the Back Buffer, until I can figure out what's going on with resizing
            // windows and partial camera viewport rects.
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.ScissorTest);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.PrimitiveRestart);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.CullFace);
            GL.PrimitiveRestartIndex(0xFFFF);
            GL.DepthMask(true);

            for (int i = 0; i < m_cameraList.Count; i++)
            {
                /* SETUP THE VIEWPORT FOR THE CAMERA */
                Camera camera = m_cameraList[i];

                Rect pixelRect = camera.PixelRect;
                GL.Viewport((int)pixelRect.X, (int)pixelRect.Y, (int)pixelRect.Width, (int)pixelRect.Height);
                GL.Scissor((int)pixelRect.X, (int)pixelRect.Y, (int)pixelRect.Width, (int)pixelRect.Height);

                // Clear the backbuffer
                Color clearColor = camera.ClearColor;
                GL.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                // Draw each room in the map
                DrawRoomsForMap(m_world.Map, camera);

                // Then draw objects from the scene in the map.
                DrawStageForMap(m_world.Map, camera);

                // Draw Debug Shapes
                DrawDebugShapes(camera);
            }

            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.PrimitiveRestart);
            GL.Disable(EnableCap.CullFace);

            //  Flush OpenGL commands to make them draw.
            GL.Flush();
        }

        public void SetOutputSize(float width, float height)
        {
            // Re-Calculate perspective camera ratios here.
            for (int i = 0; i < m_cameraList.Count; i++)
            {
                Camera camera = m_cameraList[i];
                camera.PixelWidth = width;
                camera.PixelHeight = height;
            }
        }

        public void UnloadAll()
        {
            // Reset Camera position
            foreach (Camera camera in m_cameraList)
            {
                camera.Transform.Position = Vector3.Zero;
                camera.Transform.Rotation = Quaternion.Identity;

                // Re-register it's movement component... yes this is a hack.
                EditorCameraMovement camMovement = new EditorCameraMovement();
                camMovement.Camera = camera;
                m_world.RegisterComponent(camMovement);
            }
        }

        private void DrawRoomsForMap(Map map, Camera camera)
        {
            if (map == null)
                return;

            Matrix4 viewMatrix = camera.ViewMatrix;
            Matrix4 projMatrix = camera.ProjectionMatrix;

            foreach (Room room in map.Rooms)
            {
                Matrix4 roomOffset = Matrix4.CreateTranslation(room.Translation) * Matrix4.CreateRotationY(room.YRotation);

                foreach (var obj in room.Objects)
                {
                    // Check if this layer is visible
                    if (!map.LayerIsVisible(obj.Layer))
                        continue;

                    var meshObj = obj as MeshSceneComponent;
                    if(meshObj != null)
                    {
                        DrawMesh(meshObj.Mesh, camera, roomOffset); 
                    }
                }
            }
        }

        private void DrawStageForMap(Map map, Camera camera)
        {
            if (map == null || map.Stage == null)
                return;

            Matrix4 viewMatrix = camera.ViewMatrix;
            Matrix4 projMatrix = camera.ProjectionMatrix;

            foreach (var obj in map.Stage.Objects)
            {
                // Check if this layer is visible
                if (!map.LayerIsVisible(obj.Layer))
                    continue;

                var meshObj = obj as MeshSceneComponent;
                if (meshObj != null)
                {
                    DrawMesh(meshObj.Mesh, camera, Matrix4.Identity);
                }
            }
        }

        private void DrawDebugShapes(Camera camera)
        {
            Matrix4 viewMatrix = camera.ViewMatrix;
            Matrix4 projMatrix = camera.ProjectionMatrix;

            foreach (var instance in m_world.Gizmos.GetInstanceList())
            {
                Matrix4 modelMatrix = Matrix4.CreateScale(instance.Scale) * Matrix4.CreateTranslation(instance.Position);

                // Bind the Debug Shader
                m_debugShader.Bind();

                // Upload uniforms to GPU
                GL.UniformMatrix4(m_debugShader.UniformModelMtx, false, ref modelMatrix);
                GL.UniformMatrix4(m_debugShader.UniformViewMtx, false, ref viewMatrix);
                GL.UniformMatrix4(m_debugShader.UniformProjMtx, false, ref projMatrix);

                // Bind our Mesh
                instance.Mesh.SubMeshes[0].Bind();

                // Draw our Mesh.
                GL.DrawElements(instance.Mesh.SubMeshes[0].PrimitveType, instance.Mesh.SubMeshes[0].Indexes.Length, DrawElementsType.UnsignedInt, 0);

                // Unbind the VAOs so that our VAO doesn't leak into the next drawcall.
                instance.Mesh.SubMeshes[0].Unbind();
            }
        }

        private void DrawMesh(Mesh mesh, Camera camera, Matrix4 additionalMatrix)
        {
            Matrix4 viewMatrix = camera.ViewMatrix;
            Matrix4 projMatrix = camera.ProjectionMatrix;
            for (int b = 0; b < mesh.SubMeshes.Count; b++)
            {
                MeshBatch batch = mesh.SubMeshes[b];

                // Bind the shader
                if (batch.Material != null)
                {
                    if (batch.Material.Shader != null)
                        batch.Material.Shader.Bind();
                }

                // ToDo: Get the model's position in the world from the entity it belongs to and create a model matrix from that.
                Matrix4 modelMatrix = additionalMatrix;

                // Before we draw it, we're going to do something incredibly stupid, and try to add bone support.
                Matrix4[] boneTransforms = new Matrix4[mesh.Skeleton.Count];
                for(int i = 0; i < mesh.Skeleton.Count; i++)
                {
                    SkeletonBone bone = mesh.Skeleton[i];
                    if (bone.Parent == null)
                        continue;

                    Vector3 rotatedPos = Vector3.Transform(bone.Translation, bone.Parent.Rotation);
                    Vector3 jntTrans = bone.Parent.Translation + rotatedPos;
                    Quaternion jntRot = bone.Rotation * bone.Parent.Rotation;
                    jntRot.Normalize();

                    boneTransforms[i] = boneTransforms[i] * Matrix4.CreateTranslation(jntTrans) * Matrix4.CreateFromQuaternion(jntRot);
                }

                //SkeletonBone[] boneCopy = new SkeletonBone[mesh.Skeleton.Count];
                //for (int s = 0; s < boneCopy.Length; s++)
                //    boneCopy[s] = new SkeletonBone(mesh.Skeleton[s]);

                //// For each bone, multiply it by its parent rotation/translation to go get its final position.
                //for (int bone = 0; bone < boneCopy.Length; bone++)
                //{
                //    SkeletonBone joint = boneCopy[bone];
                //    if (joint.Parent != null)
                //    {
                //        Vector3 rotatedPos = Vector3.Transform(joint.Translation, joint.Parent.Rotation);
                //        joint.Translation = joint.Parent.Translation + rotatedPos;
                //        joint.Rotation = joint.Rotation * joint.Parent.Rotation;
                //        joint.Rotation.Normalize();
                //    }
                //}

                //// Each boneCopy is now in it's final position, so we can apply that to the vertexes based on their bone weighting.
                //// However, vertex positions have already been uploaded once, so we're uh... going to hack it and re-upload them.
                //Vector3[] origVerts = batch.Vertices;
                //Vector3[] vertices = new Vector3[origVerts.Length];
                //Array.Copy(origVerts, vertices, origVerts.Length);

                //for (int v = 0; v < vertices.Length; v++)
                //{
                //    BoneWeight weights = batch.BoneWeights[v];
                //    Matrix4 translationMtx = Matrix4.Identity;
                //    Matrix4 rotationMtx = Matrix4.Identity;

                //    for (int w = 0; w < weights.BoneIndexes.Length; w++)
                //    {
                //        SkeletonBone bone = boneCopy[weights.BoneIndexes[w]];
                //        Matrix4 boneInfluence = boneTransforms[weights.BoneIndexes[w]];
                //        float weight = weights.BoneWeights[w];


                //        translationMtx = Matrix4.CreateTranslation(bone.Translation) * weight * translationMtx;
                //        rotationMtx = Matrix4.CreateFromQuaternion(bone.Rotation) * weight * rotationMtx;
                //    }

                //    Matrix4 finalMatrix = rotationMtx * translationMtx;
                //    vertices[v] = Vector3.TransformPosition(vertices[v], finalMatrix);
                //}

                // Each boneCopy is now in it's final position, so we can apply that to the vertexes based on their bone weighting.
                // However, vertex positions have already been uploaded once, so we're uh... going to hack it and re-upload them.
                Vector3[] origVerts = batch.Vertices;
                Vector3[] vertices = new Vector3[origVerts.Length];
                Array.Copy(origVerts, vertices, origVerts.Length);

                for (int v = 0; v < vertices.Length; v++)
                {
                    BoneWeight weights = batch.BoneWeights[v];
                    Matrix4 finalMatrix = Matrix4.Identity;

                    for (int w = 0; w < weights.BoneIndexes.Length; w++)
                    {
                        Matrix4 boneInfluence = boneTransforms[weights.BoneIndexes[w]];
                        float weight = weights.BoneWeights[w];

                        finalMatrix = finalMatrix + (boneInfluence * weight);
                    }

                    vertices[v] = Vector3.TransformPosition(vertices[v], finalMatrix);
                }

                // Now re-assign our Vertices to the mesh so they get uploaded to the GPU...
                batch.Vertices = vertices;

                // Bind the VAOs currently associated with this Mesh
                batch.Bind();

                // Bind our Textures
                GL.ActiveTexture(TextureUnit.Texture0);
                if (batch.Material.Textures[0] != null)
                    batch.Material.Textures[0].Bind();

                // Upload uniforms to the GPU
                GL.UniformMatrix4(batch.Material.Shader.UniformModelMtx, false, ref modelMatrix);
                GL.UniformMatrix4(batch.Material.Shader.UniformViewMtx, false, ref viewMatrix);
                GL.UniformMatrix4(batch.Material.Shader.UniformProjMtx, false, ref projMatrix);

                // Set our Blend, Cull, Depth and Dither states. Alpha Compare is
                // done in the pixel shader due to Nintendo having advanced AC options.
                GX2OpenGL.SetBlendState(batch.Material.BlendMode);
                GX2OpenGL.SetCullState(batch.Material.CullMode);
                GX2OpenGL.SetDepthState(batch.Material.ZMode);
                GX2OpenGL.SetDitherState(batch.Material.Dither);

                // Draw our Mesh.
                GL.DrawElements(batch.PrimitveType, batch.Indexes.Length, DrawElementsType.UnsignedInt, 0);

                // Unbind the VAOs so that our VAO doesn't leak into the next drawcall.
                batch.Unbind();

                // And finally restore our 'source' vertex positions
                batch.Vertices = origVerts;
            }
        }
    }
}
