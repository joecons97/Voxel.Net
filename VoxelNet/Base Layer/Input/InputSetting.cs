using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;

namespace VoxelNet.Input
{
    public class InputSetting
    {
        public string Name;
        public Input Main;
        public Input Alt;

        public Action KeyDown;
        public Action KeyUp;

        public InputSetting(string name, Input main)
        {
            Name = name;
            Main = main;
        }
    }

    public class Input
    {
        public Key? KeyButton;
        public MouseButton? MouseButton;
        public KeyModifiers? Modifiers;

        public Input(Key input, KeyModifiers? modifiers = null)
        {
            KeyButton = input;
            MouseButton = null;
            Modifiers = modifiers;
        }
        public Input(MouseButton input, KeyModifiers? modifiers = null)
        {
            KeyButton = null;
            MouseButton = input;
            Modifiers = modifiers;
        }
    }
}
