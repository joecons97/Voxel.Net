using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Input;

namespace VoxelNet
{
    public class Settings
    {
        public int FPS { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public float FieldOfView { get; set; }

        public InputSettings Input { get; set; }
    }
}
