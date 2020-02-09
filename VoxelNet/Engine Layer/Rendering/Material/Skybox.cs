using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Assets;

namespace VoxelNet.Rendering
{
    public class Skybox
    {
        private Mesh skyMesh;
        private Material.Material skyMat;

        private Camera worldCam = World.GetInstance().WorldCamera;

        public Skybox(Material.Material mat)
        {
            skyMesh = AssetDatabase.GetAsset<Mesh>("Resources/Models/InvertedCube.obj");
            skyMat = mat;
        }

        public void Update()
        {

        }

        public void Render()
        {
            if (worldCam == null)
            {
                worldCam = World.GetInstance().WorldCamera;
            }

            Renderer.DrawRequest(skyMesh, skyMat, Matrix4.CreateScale(worldCam.FarPlane / 2) * Matrix4.CreateTranslation(worldCam.Position));
        }

        public void Dispose()
        {
            skyMesh.Dispose();
            skyMat.Dispose();
        }
    }
}
