using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;

namespace VoxelNet.Input
{
    /*public static class Input
    {
        private static List<KeyboardKeyEventArgs> inputs = new List<KeyboardKeyEventArgs>();
        private static List<KeyboardKeyEventArgs> inputsDownDetected = new List<KeyboardKeyEventArgs>();
        private static List<KeyboardKeyEventArgs> inputsReleased = new List<KeyboardKeyEventArgs>();

        public static void Init()
        {
            
        }

        /*public static bool GetButtonHeld(string input)
        {
            var val = Program.Settings.Input.Settings.FirstOrDefault(x => x.Name == input);
            if (val != null)
            {
                if (inputs.Any(i => i.Key == val.Key))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GetButtonPressed(string input)
        {
            var val = Program.Settings.Input.Settings.FirstOrDefault(x => x.Name == input);
            if (val != null)
            {
                var keyboardInput = inputs.FirstOrDefault(i => i.Key == val.Key && i.IsRepeat == false);
                if (keyboardInput != null && inputsDownDetected.All(i => i.Key != val.Key))
                {
                    inputsDownDetected.Add(keyboardInput);
                    Debug.Log("Pressed " + input);
                    return true;
                }
            }

            return false;
        }

        public static bool GetButtonReleased(string input)
        {
            var val = Program.Settings.Input.Settings.FirstOrDefault(x => x.Name == input);
            if (val != null)
            {
                var index = inputsReleased.FindIndex(i => i.Key == val.Key && i.IsRepeat == false);
                if (index != -1)
                {
                    inputsReleased.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }
        
    }
    */
}
