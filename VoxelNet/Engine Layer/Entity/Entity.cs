using System.Collections.Generic;
using System.Linq;
using OpenTK;
using VoxelNet.Misc;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;

namespace VoxelNet
{
    public class Entity
    {
        protected const float UnderWaterDrag = 25;

        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; } = Vector3.One;

        public Vector3 WorldPosition
        {
            get { return ModelMatrix.ExtractTranslation(); }
        }

        public Vector3 WorldRotation
        {
            get { return Parent != null ? Parent.WorldRotation + Rotation : Rotation; }
        }

        public Mesh Mesh { get; set; }

        public bool IgnoreFrustumCulling { get; set; } = false;

        public Material Material { get; set; }

        public Matrix4 ModelMatrix { get; private set; }

        private Entity _parent;
        public Entity Parent
        {
            get => _parent;
            set
            {
                if (_parent != null)
                    _parent.RemoveChild(this);

                _parent = value;

                _parent.AddChild(this);
            }
        }

        List<Entity> _children = new List<Entity>();

        void RemoveChild(Entity entity)
        {
            if(_children.Contains(entity))
                _children.Remove(entity);
        }

        void AddChild(Entity entity)
        {
            if(!_children.Contains(entity))
                _children.Add(entity);
        }

        public int GetChildCount()
        {
            return _children.Count;
        }

        public Entity FindChild(int index)
        {
            return _children[index];
        }

        public Entity FindChild(string name)
        {
            return _children.FirstOrDefault(x => x.Name == name);
        }


        public virtual void Begin()
        {
        }

        public virtual void End()
        {
        }

        public virtual void Update()
        {
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Update();
            }
        }

        public void Render()
        {
            var localMatrix = Matrix4.CreateScale(Scale) *
                              Matrix4.CreateFromQuaternion(new Quaternion(Rotation.ToRadians()).Inverted()) * 
                              Matrix4.CreateTranslation(Position);

            if (Parent == null)
            {
                ModelMatrix = localMatrix;
            }
            else
            {
                ModelMatrix = localMatrix * Parent.ModelMatrix;
            }

            if (Mesh != null)
            {
                Renderer.DrawRequest(Mesh, IgnoreFrustumCulling, Material, ModelMatrix);
            }

            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Render();
            }
        }

        public virtual void Destroyed()
        {

        }

        public virtual void RenderGUI()
        {
        }

        //Physics Virtual Functions
        public virtual void OnPreVoxelCollisionEnter()
        {

        }  
        
        public virtual void OnPostVoxelCollisionEnter()
        {

        }

        public Vector3 GetForwardVector()
        {
            return Maths.GetForwardFromRotation(WorldRotation);
        }

        public Vector3 GetRightVector()
        {
            return Maths.GetRightFromRotation(WorldRotation);
        }

        public Vector3 GetUpVector()
        {
            return Maths.GetUpFromRotation(WorldRotation);
        }

        public Vector2 GetChunk()
        {
            var pos = Position.ToChunkPosition();
            return new Vector2(pos.X, pos.Z);
        }

        public Vector3 GetPositionInChunk()
        {
            return Position.ToChunkSpace();
        }
    }
}
