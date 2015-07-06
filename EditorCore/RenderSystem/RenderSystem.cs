﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace WEditor.Rendering
{
    public class RenderSystem : IDisposable
    {
        private List<Camera> m_cameraList;
        private WWorld m_world;
        public List<Mesh> m_meshList;
        private bool m_disposed;

        public RenderSystem(WWorld world)
        {
            m_cameraList = new List<Camera>();
            m_meshList = new List<Mesh>();
            m_world = world;

            // Create a Default camera
            Camera editorCamera = new Camera();
            editorCamera.ClearColor = new Color(0.8f, 0.2f, 1f, 1f);

            EditorCameraMovement camMovement = new EditorCameraMovement();
            camMovement.Camera = editorCamera;
            m_world.RegisterComponent(camMovement);

            m_cameraList.Add(editorCamera);
        }

        ~RenderSystem()
        {
            //Dispose(false);
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

                Matrix4 viewMatrix = camera.ViewMatrix;
                Matrix4 projMatrix = camera.ProjectionMatrix;

                for (int m = 0; m < m_meshList.Count; m++)
                {
                    Mesh mesh = m_meshList[m];
                    for (int b = 0; b < mesh.SubMeshes.Count; b++)
                    {
                        MeshBatch batch = mesh.SubMeshes[b];

                        // Bind the shader
                        batch.Material.Shader.Bind();

                        // ToDo: Get the model's position in the world from the entity it belongs to and create a model matrix from that.
                        Matrix4 modelMatrix = Matrix4.Identity;

                        // Before we draw it, we're going to do something incredibly stupid, and try to add bone support.
                        SkeletonBone[] boneCopy = new SkeletonBone[mesh.Skeleton.Count];
                        for (int s = 0; s < boneCopy.Length; s++)
                            boneCopy[s] = new SkeletonBone(mesh.Skeleton[s]);

                        // For each bone, multiply it by its parent rotation/translation to go get its final position.
                        for (int bone = 0; bone < boneCopy.Length; bone++)
                        {
                            SkeletonBone joint = boneCopy[bone];
                            if (joint.Parent != null)
                            {
                                Vector3 rotatedPos = Vector3.Transform(joint.Translation, joint.Parent.Rotation);
                                joint.Translation = joint.Parent.Translation + rotatedPos;
                                joint.Rotation = joint.Rotation * joint.Parent.Rotation;
                                joint.Rotation.Normalize();
                            }
                        }

                        // Each boneCopy is now in it's final position, so we can apply that to the vertexes based on their bone weighting.
                        // However, vertex positions have already been uploaded once, so we're uh... going to hack it and re-upload them.
                        Vector3[] origVerts = batch.Vertices;
                        Vector3[] vertices = new Vector3[origVerts.Length];
                        Array.Copy(origVerts, vertices, origVerts.Length);

                        for (int v = 0; v < vertices.Length; v++)
                        {
                            BoneWeight weights = batch.BoneWeights[v];
                            Matrix4 translationMtx = Matrix4.Identity;
                            Matrix4 rotationMtx = Matrix4.Identity;

                            for(int w = 0; w < weights.BoneIndexes.Length; w++)
                            {
                                SkeletonBone bone = boneCopy[weights.BoneIndexes[w]];
                                float weight = weights.BoneWeights[w];

                                translationMtx = Matrix4.CreateTranslation(bone.Translation) * weight * translationMtx;
                                rotationMtx *= Matrix4.CreateFromQuaternion(bone.Rotation) * weight;
                            }

                            Matrix4 finalMatrix = rotationMtx * translationMtx;
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
            foreach(Camera camera in m_cameraList)
            {
                camera.Transform.Position = Vector3.Zero;
                camera.Transform.Rotation = Quaternion.Identity;

                // Re-register it's movement component... yes this is a hack.
                EditorCameraMovement camMovement = new EditorCameraMovement();
                camMovement.Camera = camera;
                m_world.RegisterComponent(camMovement);
            }

            for (int i = 0; i < m_meshList.Count; i++)
                m_meshList[i].Dispose();

            // Free all of our meshes
            m_meshList.Clear();
        }

        public void RegisterMesh(Mesh model)
        {
            m_meshList.Add(model);
        }

        public void UnregisterMesh(Mesh model)
        {
            model.Dispose();
            m_meshList.Remove(model);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            if (disposing)
            {
                // Free any *managed* objects here.
                UnloadAll();
            }

            // Free any *unmanaged* objects here.
            m_disposed = true;
        }
    }
}
