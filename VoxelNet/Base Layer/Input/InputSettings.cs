using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using VoxelNet.Input;

namespace VoxelNet.Input
{
    public class InputSettings
    {
        public List<InputSetting> Settings { get; private set; } = new List<InputSetting>();

        public InputSettings()
        {
            Settings.Add(new InputSetting("Jump", new Input(Key.Space)));
            Settings.Add(new InputSetting("Destroy Block", new Input(MouseButton.Left)));
            Settings.Add(new InputSetting("Interact", new Input(MouseButton.Right)));
        }

        public InputSetting GetSetting(string name)
        {
            return Settings.FirstOrDefault(x => x.Name == name);
        }
    }
}
