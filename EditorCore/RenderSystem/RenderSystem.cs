using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace WEditor.Rendering
{
    public class RenderSystem
    {
        public static RenderSystem HackInstance;

        private List<Camera> m_cameraList;
        private ShaderProgram m_shader;

        public List<Mesh> m_meshList;

        public RenderSystem()
        {
            m_cameraList = new List<Camera>();
            m_meshList = new List<Mesh>();
            m_shader = new ShaderProgram("RenderSystem/Shaders/vert.glsl", "RenderSystem/Shaders/frag.glsl");
            HackInstance = this;

            // Create a Default camera
            Camera editorCamera = new Camera();
            editorCamera.ClearColor = new Color(0.8f, 0.2f, 1f, 1f);
            m_cameraList.Add(editorCamera);

            Camera leftCamera = new Camera();
            leftCamera.ClearColor = new Color(1f, 0.5f, 0, 1f);
            leftCamera.ViewportRect = new Rect(0f, 0f, 0.5f, 1f);


            Camera rightCamera = new Camera();
            rightCamera.ViewportRect = new Rect(0.5f, 0f, 0.5f, 1f);
            rightCamera.ClearColor = new Color(0.5f, 0, 1f, 1f);

            //m_cameraList.Add(leftCamera);
            //m_cameraList.Add(rightCamera);

            /* Create a default cube */
            var testMesh = new MeshBatch();
            Vector3 size = new Vector3(2f, 2f, 2f);

            Vector3[] meshVerts =
            {
                new Vector3(-size.X / 2f, -size.Y / 2f,  -size.Z / 2f),
                new Vector3(size.X / 2f, -size.Y / 2f,  -size.Z / 2f),
                new Vector3(size.X / 2f, size.Y / 2f,  -size.Z / 2f),
                new Vector3(-size.X / 2f, size.Y / 2f,  -size.Z / 2f),
                new Vector3(-size.X / 2f, -size.Y / 2f,  size.Z / 2f),
                new Vector3(size.X / 2f, -size.Y / 2f,  size.Z / 2f),
                new Vector3(size.X / 2f, size.Y / 2f,  size.Z / 2f),
                new Vector3(-size.X / 2f, size.Y / 2f,  size.Z / 2f),
            };

            int[] meshIndexes =
            {
                //front
                0, 7, 3,
                0, 4, 7,
                //back
                1, 2, 6,
                6, 5, 1,
                //left
                0, 2, 1,
                0, 3, 2,
                //right
                4, 5, 6,
                6, 7, 4,
                //top
                2, 3, 6,
                6, 3, 7,
                //bottom
                0, 1, 5,
                0, 5, 4
            };

            testMesh.Vertices = meshVerts;
            testMesh.Indexes = meshIndexes;

            Color[] colors = new Color[meshVerts.Length];
            for (int i = 0; i < meshVerts.Length; i++)
                colors[i] = new Color(1f, 1f, 0f, 0.5f);
            testMesh.Color0 = colors;

            Mesh mesh = new Mesh();
            mesh.SubMeshes.Add(testMesh);

            //m_meshList.Add(mesh);
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
                        SetBlendState(batch.Material.BlendMode);
                        SetCullState(batch.Material.CullMode);
                        SetDepthState(batch.Material.ZMode);
                        SetDitherState(batch.Material.Dither);

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

        private void SetDitherState(bool enabled)
        {
            if (enabled)
                GL.Enable(EnableCap.Dither);
            else
                GL.Disable(EnableCap.Dither);
        }

        private void SetDepthState(Common.Nintendo.J3D.ZMode zMode)
        {
            if (zMode.Enable)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(GetOpenGLDepthFunc(zMode.Function));
                GL.DepthMask(zMode.UpdateEnable);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }
        }

        private DepthFunction GetOpenGLDepthFunc(GXCompareType gxCompare)
        {
            switch (gxCompare)
            {
                case GXCompareType.Never: return DepthFunction.Never;
                case GXCompareType.Less: return DepthFunction.Less;
                case GXCompareType.Equal: return DepthFunction.Equal;
                case GXCompareType.LEqual: return DepthFunction.Lequal;
                case GXCompareType.Greater: return DepthFunction.Greater;
                case GXCompareType.NEqual: return DepthFunction.Notequal;
                case GXCompareType.GEqual: return DepthFunction.Gequal;
                case GXCompareType.Always: return DepthFunction.Always;
                default:
                    WLog.Warning(LogCategory.Rendering, null, "Unsupported GXCompareType: \"{0}\" in GetOpenGLDepthFunc!", gxCompare);
                    return DepthFunction.Less;
            }
        }

        private void SetCullState(GXCullMode gXCullMode)
        {
            GL.Enable(EnableCap.CullFace);

            switch (gXCullMode)
            {
                case GXCullMode.None:
                    GL.Disable(EnableCap.CullFace);
                    break;
                case GXCullMode.Front:
                    GL.CullFace(CullFaceMode.Front);
                    break;
                case GXCullMode.Back:
                    GL.CullFace(CullFaceMode.Back);
                    break;
                case GXCullMode.All:
                    GL.CullFace(CullFaceMode.FrontAndBack);
                    break;
            }
        }

        private void SetBlendState(Common.Nintendo.J3D.BlendMode blendMode)
        {
            if (blendMode.Type == GXBlendMode.Blend)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(GetOpenGLBlendSrc(blendMode.SourceFact), GetOpenGLBlendDest(blendMode.DestinationFact));
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }
        }

        private BlendingFactorDest GetOpenGLBlendDest(GXBlendModeControl gxMode)
        {
            switch (gxMode)
            {
                case GXBlendModeControl.Zero: return BlendingFactorDest.Zero;
                case GXBlendModeControl.One: return BlendingFactorDest.One;
                case GXBlendModeControl.SrcColor: return BlendingFactorDest.SrcColor;
                case GXBlendModeControl.InverseSrcColor: return BlendingFactorDest.OneMinusSrcColor;
                case GXBlendModeControl.SrcAlpha: return BlendingFactorDest.SrcAlpha;
                case GXBlendModeControl.InverseSrcAlpha: return BlendingFactorDest.OneMinusSrcAlpha;
                case GXBlendModeControl.DstAlpha: return BlendingFactorDest.DstAlpha;
                case GXBlendModeControl.InverseDstAlpha: return BlendingFactorDest.OneMinusDstAlpha;
                default:
                    WLog.Warning(LogCategory.Rendering, null, "Unsupported GXBlendModeControl: \"{0}\" in GetOpenGLBlendDest!", gxMode);
                    return BlendingFactorDest.OneMinusSrcAlpha;
            }
        }

        private BlendingFactorSrc GetOpenGLBlendSrc(GXBlendModeControl gxMode)
        {
            switch (gxMode)
            {
                case GXBlendModeControl.Zero: return BlendingFactorSrc.Zero;
                case GXBlendModeControl.One: return BlendingFactorSrc.One;
                case GXBlendModeControl.SrcColor: return BlendingFactorSrc.SrcColor;
                case GXBlendModeControl.InverseSrcColor: return BlendingFactorSrc.OneMinusSrcColor;
                case GXBlendModeControl.SrcAlpha: return BlendingFactorSrc.SrcAlpha;
                case GXBlendModeControl.InverseSrcAlpha: return BlendingFactorSrc.OneMinusSrcAlpha;
                case GXBlendModeControl.DstAlpha: return BlendingFactorSrc.DstAlpha;
                case GXBlendModeControl.InverseDstAlpha: return BlendingFactorSrc.OneMinusDstAlpha;
                default:
                    WLog.Warning(LogCategory.Rendering, null, "Unsupported GXBlendModeControl: \"{0}\" in GetOpenGLBlendSrc!", gxMode);
                    return BlendingFactorSrc.SrcAlpha;
            };
        }

        internal void SetOutputSize(float width, float height)
        {
            // Re-Calculate perspective camera ratios here.
            for (int i = 0; i < m_cameraList.Count; i++)
            {
                Camera camera = m_cameraList[i];
                camera.PixelWidth = width;
                camera.PixelHeight = height;
            }
        }
    }
}
