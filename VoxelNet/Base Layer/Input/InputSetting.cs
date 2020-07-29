using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTK.Input;

namespace VoxelNet.Input
{
    public class InputSetting
    {
        public string Name;
        public Interaction Main;
        public Interaction Alt;

        [JsonIgnore]
        public Action KeyDown;
        [JsonIgnore]
        public Action KeyUp;

        public InputSetting(string name, Interaction main)
        {
            Name = name;
            Main = main;
        }

        public InputSetting() { }

    }
    public class Interaction
    {
        public Key? KeyButton;
        public MouseButton? MouseButton;
        public KeyModifiers? Modifiers;

        public Interaction(Key input, KeyModifiers? modifiers = null)
        {
            KeyButton = input;
            MouseButton = null;
            Modifiers = modifiers;
        }
        public Interaction(MouseButton input, KeyModifiers? modifiers = null)
        {
            KeyButton = null;
            MouseButton = input;
            Modifiers = modifiers;
        }

        public Interaction() { }
    }

}
