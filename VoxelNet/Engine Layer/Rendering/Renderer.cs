using System.Collections.Generic;
using System.Collections.Specialized;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VoxelNet.Assets;
using VoxelNet.Buffers;

namespace VoxelNet.Rendering
{
    public static class Renderer
    {
        public static int DrawCalls { get; set; }
        public static int ClippedCount { get; set; }

        static OrderedDictionary drawQueue = new OrderedDictionary();

        static FrameBufferObject fbo = new FrameBufferObject(Program.Window.Width, Program.Window.Height, FBOType.DepthTexture);
        public static FrameBufferObject FrameBuffer { get; private set; }= new FrameBufferObject(Program.Window.Width, Program.Window.Height, FBOType.DepthTexture);

        static Vector3[] meshPoses = { new Vector3(-1, -1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0) };
        static Vector2[] meshUvs = { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

        public static Mesh BlitMesh = new Mesh(new VertexContainer(meshPoses, meshUvs), new uint[] { 1, 2, 0, 3, 2, 1 });

        private static Material.Material FBOMaterial =
            AssetDatabase.GetAsset<Material.Material>("Resources/Materials/FBO.mat");

        static Renderer()
        {
            Program.Window.Resize += (sender, args) =>
            {
                FrameBuffer = new FrameBufferObject(Program.Window.Width, Program.Window.Height, FBOType.DepthTexture);
                fbo = new FrameBufferObject(Program.Window.Width, Program.Window.Height, FBOType.DepthTexture);
            };
        }

        public static void DrawRequest(Mesh mesh, bool ignoreFrustumCulling, Material.Material material, Matrix4 worldMatrix = default)
        {
            if (mesh.IndexBuffer.Length == 0)
                 return;

            if (worldMatrix == default)
                worldMatrix = Matrix4.Identity;

            if (World.GetInstance() != null && !ignoreFrustumCulling && mesh.FrustumCull && 
                !World.GetInstance().WorldCamera.Frustum.Intersects(mesh.Bounds.Transform(worldMatrix.ExtractTranslation(), Vector3.One)))
            {
                ClippedCount++;
                return;
            }

            if (material == null)
                material = AssetDatabase.GetAsset<Material.Material>("Resources/Materials/Fallback.mat");

            if (drawQueue.Contains(material.Name))
            {
                var batch = (BatchDraw)drawQueue[material.Name];
                batch.draws.Add(new QueuedDraw()
                {
                    mesh = mesh,
                    worldMatrix = worldMatrix
                });

            }
            else if (material.Shader.IsTransparent)
            {
                drawQueue.Add(material.Name, new BatchDraw()
                {
                    draws = new List<QueuedDraw>()
                    {
                        new QueuedDraw()
                        {
                            mesh = mesh,
                            worldMatrix = worldMatrix
                        }
                    },
                    material = material,
                });
            }
            else
            {
                drawQueue.Insert(0, material.Name, new BatchDraw()
                {
                    draws = new List<QueuedDraw>()
                    {
                        new QueuedDraw()
                        {
                            mesh = mesh,
                            worldMatrix = worldMatrix
                        }
                    },
                    material = material,
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

            UniformBuffers.BindAll(material.Shader.Handle);
            material.Bind();
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
                bool drawFBO = false;

                var call = (BatchDraw) drawQueue[i];

                if (!isLast)
                {
                    var callPlusOne = (BatchDraw)drawQueue[i + 1];
                    if (!call.material.Shader.IsTransparent && callPlusOne.material.Shader.IsTransparent)
                    {
                        drawFBO = true;
                    }
                }
                else
                    drawFBO = true;

                //Try to set this...
                call.material.Bind();
                UniformBuffers.BindAll(call.material.Shader.Handle);

                for (int j = 0; j < call.draws.Count; j++)
                {
                    if (call.draws[j].worldMatrix != default)
                    {
                        call.material.Shader.SetUniform("u_World", call.draws[j].worldMatrix);
                        call.material.Shader.BindUniform("u_World");
                    }

                    call.draws[j].mesh.VertexArray.Bind();
                    call.draws[j].mesh.IndexBuffer.Bind();

                    if (call.material.Shader.IsTransparent)
                    {
                        if (call.material.Shader.ContainsUniform("u_Src"))
                        {
                            call.material.SetScreenSourceTexture("u_Src", fbo.ColorHandle, 1);
                            call.material.Shader.BindUniform("u_Src");
                        }

                        if (call.material.Shader.ContainsUniform("u_Depth"))
                        {
                            call.material.SetScreenSourceTexture("u_Depth", fbo.DepthHandle, 2);
                            call.material.Shader.BindUniform("u_Depth");
                        }
                    }

                    FrameBuffer.Bind();

                    try
                    {
                        var length = call.draws[j].mesh.IndexBuffer.Length;
                        GL.DrawElements(PrimitiveType.Triangles, length,
                            DrawElementsType.UnsignedInt, 0);
                    }
                    catch
                    {
                        Debug.Log("Error when trying to render an object", DebugLevel.Warning);
                    }

                    call.draws[j].mesh.VertexArray.Unbind();
                    call.draws[j].mesh.IndexBuffer.Unbind();
                    FrameBuffer.Unbind();
                }
                call.draws.Clear();

                if (isLast)
                {
                    UniformBuffers.UnbindAll();
                    call.material.Unbind();
                }

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

        struct BatchDraw
        {
            public List<QueuedDraw> draws;
            public Material.Material material;
        }

        struct QueuedDraw
        {
            public Mesh mesh;
            public Matrix4 worldMatrix;
        }
    }
}
