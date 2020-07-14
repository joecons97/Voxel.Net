using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VoxelNet.Assets;
using VoxelNet.Buffers;
using VoxelNet.Physics;

namespace VoxelNet.Rendering
{
    public static class Renderer
    {
        public static int DrawCalls { get; set; }
        public static int ClippedCount { get; set; }

        static List<QueuedDraw> drawQueue = new List<QueuedDraw>();

        static FrameBufferObject fbo = new FrameBufferObject(Program.Window.Width, Program.Window.Height, FBOType.DepthTexture);
        public static FrameBufferObject FrameBuffer { get; private set; }= new FrameBufferObject(Program.Window.Width, Program.Window.Height, FBOType.DepthTexture);

        static Vector3[] meshPoses = new[] { new Vector3(-1, -1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0) };
        static Vector2[] meshUvs = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

        public static Mesh BlitMesh = new Mesh(new VertexContainer(meshPoses, meshUvs), new uint[] { 1, 2, 0, 3, 2, 1 });

        private static Material.Material FBOMaterial =
            AssetDatabase.GetAsset<Material.Material>("Resources/Materials/FBO.mat");

        private static FrameBufferObject pass1Fbo = null;

        static Renderer()
        {
            Program.Window.Resize += (sender, args) =>
            {
                FrameBuffer = new FrameBufferObject(Program.Window.Width, Program.Window.Height, FBOType.DepthTexture);
                fbo = new FrameBufferObject(Program.Window.Width, Program.Window.Height, FBOType.DepthTexture);
            };
        }

        public static void DrawRequest(Mesh mesh, Material.Material material, Matrix4 worldMatrix = default)
        {
            if (mesh.IndexBuffer.Length == 0)
                 return;

            if (worldMatrix == default)
                worldMatrix = Matrix4.Identity;

            if (!World.GetInstance().WorldCamera.Frustum.Intersects(mesh.Bounds.Transform(worldMatrix.ExtractTranslation(), Vector3.One)))
            {
                ClippedCount++;
                return;
            }

            if (material == null)
                material = AssetDatabase.GetAsset<Material.Material>("Resources/Materials/Fallback.mat");

            if (material.Shader.IsTransparent)
            {
                drawQueue.Add(new QueuedDraw()
                {
                    mesh = mesh,
                    material = material,
                    worldMatrix = worldMatrix
                });
            }
            else
            {
                drawQueue.Insert(0, new QueuedDraw()
                {
                    mesh = mesh,
                    material = material,
                    worldMatrix = worldMatrix
                });
            }
            DrawCalls = drawQueue.Count;
        }

        public static void DrawNow(Mesh mesh, Material.Material material, Matrix4 worldMatrix = default)
        {
            if (material == null)
                material = AssetDatabase.GetAsset<Material.Material>("Resources/Materials/Fallback.mat");

            if (worldMatrix == default)
                worldMatrix = Matrix4.Identity;

            if (worldMatrix != default)
                material.Shader.SetUniform("u_World", worldMatrix);

            material.Bind();

            UniformBuffers.BindAll(material.Shader.Handle);

            mesh.VertexArray.Bind();
            mesh.IndexBuffer.Bind();

            GL.DrawElements(PrimitiveType.Triangles, mesh.IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);

            mesh.VertexArray.Unbind();
            mesh.IndexBuffer.Unbind();
            material.Unbind();
            UniformBuffers.UnbindAll();
        }

        public static void DrawQueue()
        {
            FrameBuffer.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            FrameBuffer.Unbind();

            fbo.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            fbo.Unbind();

            for (int i = 0; i < drawQueue.Count; i++)
            {
                bool isLast = i == drawQueue.Count - 1;
                bool isSecondToLast = i == drawQueue.Count - 2;
                bool renderToFBO = true;
                bool drawFBO = false;

                /*if(isLast || isSecondToLast)
                {
                    drawFBO = true;
                }
                else*/
                if (!isLast)
                {
                    if (!drawQueue[i].material.Shader.IsTransparent && drawQueue[i + 1].material.Shader.IsTransparent)
                    {
                        drawFBO = true;
                    }
                }
                else
                    drawFBO = true;

                if (drawQueue[i].worldMatrix != default)
                    drawQueue[i].material.Shader.SetUniform("u_World", drawQueue[i].worldMatrix);

                //Try to set this...
                drawQueue[i].material.Bind();

                UniformBuffers.BindAll(drawQueue[i].material.Shader.Handle);

                drawQueue[i].mesh.VertexArray.Bind();
                drawQueue[i].mesh.IndexBuffer.Bind();

                if (drawQueue[i].material.Shader.IsTransparent)
                {
                    if (drawQueue[i].material.Shader.ContainsUniform("u_Src"))
                    {
                        drawQueue[i].material
                            .SetScreenSourceTexture("u_Src", fbo.ColorHandle,
                                1); //.Shader.SetUniform("u_Src", FrameBuffer.ColorHandle);
                    }

                    if (drawQueue[i].material.Shader.ContainsUniform("u_Depth"))
                    {
                        drawQueue[i].material
                            .SetScreenSourceTexture("u_Depth", fbo.DepthHandle,
                                2); //.Shader.SetUniform("u_Src", FrameBuffer.ColorHandle);
                    }
                }

                if (renderToFBO)
                {
                    FrameBuffer.Bind();
                }

                GL.DrawElements(PrimitiveType.Triangles, drawQueue[i].mesh.IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);

                drawQueue[i].mesh.VertexArray.Unbind();
                drawQueue[i].mesh.IndexBuffer.Unbind();
                drawQueue[i].material.Unbind();
                UniformBuffers.UnbindAll();

                FrameBuffer.Unbind();

                if (drawFBO)
                {
                    if (!isLast)
                    {
                        GL.BlitNamedFramebuffer(FrameBuffer.Handle, fbo.Handle, 0,0,FrameBuffer.Width, FrameBuffer.Height,0,0,fbo.Width, fbo.Height, 
                            ClearBufferMask.ColorBufferBit| ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
                    }
                    else
                    {
                        GL.Disable(EnableCap.DepthTest);

                        FBOMaterial.SetScreenSourceTexture("u_Src", FrameBuffer.ColorHandle, 0);
                        DrawNow(BlitMesh, FBOMaterial);

                        GL.Enable(EnableCap.DepthTest);
                        fbo.Unbind();
                    }
                }
            }

            drawQueue.Clear();
        }

        struct QueuedDraw
        {
            public Mesh mesh;
            public Material.Material material;
            public Matrix4 worldMatrix;
        }
    }
}
