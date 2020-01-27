using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;

namespace VoxelNet.Input
{
    public static class Input
    {
        private static Vector2 mouseDelta = new Vector2(0,0);

       // public static Action<MouseMoveEventArgs> MouseMoved;

        private static MouseState currentMouseState, previousMouseState;

        static Input()
        {
            //Program.Window.MouseMove += (sender, args) =>
           // {
           //     Mouse.SetPosition(Program.Window.Width / 2f, Program.Window.Height / 2f);
           //     MouseMoved?.Invoke(args);
           // };
        }

        public static InputSetting GetSetting(string input)
        {
            return Program.Settings.Input.GetSetting(input);
        }

        public static void Update()
        {
            currentMouseState = Mouse.GetState();
            if (currentMouseState != previousMouseState)
            {
                mouseDelta = new Vector2(currentMouseState.X - previousMouseState.X, currentMouseState.Y - previousMouseState.Y);
            }
            else
            {
                mouseDelta = Vector2.Zero;
            }

            previousMouseState = currentMouseState;
        }

        public static Vector2 GetMouseDelta()
        {
            return mouseDelta;
        }
    }
    
}
