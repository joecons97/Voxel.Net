using System;
using System.Drawing.Drawing2D;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using VoxelNet;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;

//NOTE: OPENGL FORWARD IS 0,0,-1

namespace VoxelNet
{
    public class Window : GameWindow
    {   
        private Vector3[] positions =
        {
            new Vector3(-0.5f, -0.5f, 0.5f), // front
            new Vector3(0.5f, -0.5f,  0.5f),  // front
            new Vector3(0.5f, 0.5f,   0.5f),   // front
            new Vector3(-0.5f, 0.5f,  0.5f),  // front

            new Vector3(-0.5f, -0.5f, 0.5f),  // left
            new Vector3(-0.5f, -0.5f, -.5f),  // left
            new Vector3(-0.5f, 0.5f,  -.5f),   // left
            new Vector3(-0.5f, 0.5f,  0.5f),   // left
                                           
            new Vector3(0.5f, -0.5f,0.5f),   // right
            new Vector3(0.5f, -0.5f,-.5f),   // right
            new Vector3(0.5f, 0.5f, -.5f),   // right
            new Vector3(0.5f, 0.5f, 0.5f),   // right

            new Vector3(-0.5f, -0.5f, -.5f), // back
            new Vector3(0.5f, -0.5f, -.5f),  // back
            new Vector3(0.5f, 0.5f, -.5f),   // back
            new Vector3(-0.5f, 0.5f, -.5f),   // back

            new Vector3(-0.5f,0.5f,  0.5f),   // top
            new Vector3(-0.5f,0.5f,  -.5f),   // top
            new Vector3(0.5f, 0.5f,  -.5f),   // top
            new Vector3(0.5f, 0.5f,  0.5f),   // top

            new Vector3(-0.5f,-0.5f,  0.5f),   // bottom
            new Vector3(-0.5f,-0.5f,  -.5f),   // bottom
            new Vector3(0.5f, -0.5f,  -.5f),   // bottom
            new Vector3(0.5f, -0.5f,  0.5f),   // bottom
        };

        private Vector2[] uvs =
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
        };
        
        private VertexContainer vertices;

        private uint[] indices =
        {
            0, 1, 2,
            2, 3, 0,

            4, 5, 6,
            6, 7, 4,

            8, 9, 10,
            10, 11, 8,

            12, 13, 14,
            14, 15, 12,

            16, 17, 18,
            18, 19, 16,

            20, 21, 22,
            22, 23, 20,
        };

        private Mesh mesh;
        private double time;
        private Material mat;
        private Camera cam;
        private Vector2 lastMousePos;
        private Vector2 mouseD;
        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorGrabbed = true;
            CursorVisible = false;
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(.39f, .58f, .92f, 1.0f);

            cam = new Camera();
            cam.Position = new Vector3(0,0,2);

            vertices = new VertexContainer(positions, uvs);

            mesh = AssetDatabase.GetAsset<Mesh>("Resources/Models/Tests/Teapot.obj");//new Mesh(vertices, indices);

            mat = AssetDatabase.GetAsset<Material>("Resources/Materials/TestMaterial.mat");//new Material();

            mat.Bind();

            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            CursorGrabbed = false;
            CursorVisible = true;
            mesh.Dispose();
            mat.Dispose();
            base.OnUnload(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState kbdState = Keyboard.GetState();
            MouseState mseState = Mouse.GetState();

            mouseD.X = (mseState.X - lastMousePos.X);
            mouseD.Y = (mseState.Y - lastMousePos.Y);

            lastMousePos.X = mseState.X;
            lastMousePos.Y = mseState.Y;

            if (kbdState.IsKeyDown(Key.Escape))
                Exit();

            cam.Rotation = new Vector3(cam.Rotation.X + (mouseD.Y * (float)e.Time) * 5, cam.Rotation.Y + (mouseD.X * (float)e.Time) * 5, cam.Rotation.Z);

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), (float)Width / (float)Height, 0.01f, 100.0f);
            var up = cam.GetUp();
            var forward = cam.GetForward();
            Matrix4 view = Matrix4.LookAt(cam.Position, cam.Position + forward, up);
            Matrix4 world = Matrix4.CreateRotationY((float)time) * Matrix4.CreateRotationX((float)time) * Matrix4.CreateTranslation(0,0, -1);

            mat.Shader.SetUniform("u_Projection", proj);
            mat.Shader.SetUniform("u_View", view);
            mat.Shader.SetUniform("u_World", world);

            Renderer.Draw(mesh.IndexBuffer, mesh.VertexArray, mat.Shader);

            Context.SwapBuffers();
            time += e.Time;
            string fps = (1 / e.Time).ToString("f1");
            Title = Program.PROGRAMTITLE + $" {fps}fps";
            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0,0,Width, Height);
            base.OnResize(e);
        }
    }
}
