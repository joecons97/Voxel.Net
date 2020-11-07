using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Buffers;
using VoxelNet.Misc;

namespace VoxelNet.Rendering
{
    public struct CameraUniformBuffer
    {
        public Matrix4 ProjectionMat;
        public Matrix4 ViewMat;
        public Vector4 Position;
    }

    public enum CameraProjectionType
    {
        Perspective,
        Orthographic
    }

    public class Camera
    {
        private Vector3 _position;

        public Vector3 Position
        {
            get
            {
                if (Parent == null)
                    return _position;
                else
                    return Parent.WorldPosition;
            }
            set
            {
                if (Parent == null)
                    _position = value;
                else
                    Debug.Log("You cannot set the Camera's position whilst is has a parent!");
            }
        }

        private Vector3 _rotation;

        public Vector3 Rotation
        {
            get
            {
                if (Parent == null)
                    return _rotation;
                else
                    return Parent.WorldRotation;
            }
            set
            {
                if (Parent == null)
                    _rotation = value;
                else
                    Debug.Log("You cannot set the Camera's position whilst is has a parent!");
            }
        }

        public Entity Parent { get; set; }

        public CameraProjectionType ProjectionType { get; set; } = CameraProjectionType.Perspective;

        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }

        /// <summary>
        /// This is only used when the camera is Orthographic
        /// </summary>
        public Vector2 CameraSize { get; set; } = Vector2.One;

        public float NearPlane { get; } = 0.1f;
        public float FarPlane { get; } = 900f;

        public Frustum Frustum { get; } = new Frustum(Matrix4.Identity);

        private CameraUniformBuffer bufferData = new CameraUniformBuffer();

        public Camera()
        {
            GenerateProjectionMatrix();

            Program.Window.Resize += (sender, args) => GenerateProjectionMatrix();
        }

        public void Update()
        {
            ViewMatrix = Matrix4.LookAt(Position, Position + GetForward(), GetUp());
            
            Frustum.UpdateMatrix(ViewMatrix * ProjectionMatrix);

            bufferData.ProjectionMat = ProjectionMatrix;
            bufferData.ViewMat = ViewMatrix;
            bufferData.Position = new Vector4(Position, 1);

            UniformBuffers.WorldCameraBuffer.Update(bufferData);
        }

        public void GenerateProjectionMatrix()
        {
            switch (ProjectionType)
            {
                case CameraProjectionType.Perspective:
                    ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                        MathHelper.DegreesToRadians(Program.Settings.FieldOfView),
                        (float)Window.WindowWidth / (float)Window.WindowHeight, NearPlane, FarPlane);
                    break;
                case CameraProjectionType.Orthographic:
                    ProjectionMatrix = Matrix4.CreateOrthographic(CameraSize.X * 2f,
                        CameraSize.Y *2f, NearPlane, FarPlane);
                    break;
            }
        }

        public Vector3 GetForward()
        {
            return Maths.GetForwardFromRotation(Rotation);
        }

        public Vector3 GetRight()
        {
            return Maths.GetRightFromRotation(Rotation);
        }

        public Vector3 GetUp()
        {
            return Maths.GetUpFromRotation(Rotation);
        }
    }
}
