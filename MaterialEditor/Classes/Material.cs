using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MaterialEditor.Annotations;

namespace MaterialEditor
{
    public class Material : INotifyPropertyChanged
    {
        public string DisplayName
        {
            get { return Name + (IsDirty ? "*" : ""); }
        }

        public string Name { get; set; } = "New Material";
        public string ID { get; set; }

        public string SaveLocation { get; set; }

        private bool isDirty = false;
        public bool IsDirty
        {
            get => isDirty;
            set
            {
                isDirty = value;
                OnPropertyChanged(nameof(IsDirty));
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public Dictionary<string, object> Properties = new Dictionary<string, object>();
        public Dictionary<string, Type> PropertyTypes = new Dictionary<string, Type>();

        private string shaderLocation = "";

        public string ShaderLocation
        {
            get => shaderLocation;
            set
            {
                shaderLocation = value;
                GetProperties();
                OnPropertyChanged(nameof(ShaderLocation));
            }
        }

        public Material(string name)
        {
            Name = name;
            ID = Guid.NewGuid().ToString();
        }

        public Material(string path, string name)
        {
            Name = name;
            ID = Guid.NewGuid().ToString();
            SaveLocation = path;
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if(line.StartsWith("shader"))
                {
                    ShaderLocation = Path.Combine(Settings.Instance.ProjectLocation, line.Split(' ')[1]);
                }

                if (line.StartsWith("uniform"))
                {
                    var vals = line.Split(' ');
                    var type = vals[1];
                    var nme = vals[2];
                    if (Properties.ContainsKey(nme))
                    {
                        var actualType = PropertyTypes[nme];
                        switch (actualType)
                        {
                            case var f when f == typeof(float):
                                Properties[nme] = float.Parse(vals[3]);
                                break;
                            case var v when v == typeof(vec2):
                                var nums2 = vals[3].Split(',');
                                Properties[nme] = new vec2()
                                {
                                    x = float.Parse(nums2[0]),
                                    y = float.Parse(nums2[1])
                                };
                                break;
                            case var v when v == typeof(vec3):
                                var nums3 = vals[3].Split(',');
                                Properties[nme] = new vec3()
                                {
                                    x = float.Parse(nums3[0]),
                                    y = float.Parse(nums3[1]),
                                    z = float.Parse(nums3[2])
                                };
                                break;
                            case var v when v == typeof(vec4):
                                var nums4 = vals[3].Split(',');
                                Properties[nme] = new vec4()
                                {
                                    x = float.Parse(nums4[0]),
                                    y = float.Parse(nums4[1]),
                                    z = float.Parse(nums4[2]),
                                    w = float.Parse(nums4[3])
                                };
                                break;
                        }
                    }
                }
            }
        }

        public void Save()
        {
            List<string> lines= new List<string>();
            lines.Add("//Generated material file: " + DateTime.Now.ToString());
            lines.Add("//Valid material type: number, mat4, vec2, vec3, vec4");
            string localShaderLocation = shaderLocation.Remove(0, Settings.Instance.ProjectLocation.Length + 1);
            lines.Add("shader " + localShaderLocation);
            lines.Add("\n//Uniforms");
            IsDirty = false;

            foreach (var key in Properties.Keys)
            {
                string type = "number";
                if (PropertyTypes[key] != typeof(float))
                {
                    type = PropertyTypes[key].Name;
                }
                lines.Add("uniform " + type + " " + key + " " + Properties[key].ToString());
            }

            File.WriteAllLines(SaveLocation, lines);
        }

        void GetProperties()
        {
            if (string.IsNullOrEmpty(ShaderLocation)) return;

            var lines = File.ReadAllLines(ShaderLocation).Where(x => x.StartsWith("uniform"));
            foreach (var line in lines)
            {
                var split = line.Split(' ');
                var type = split[1];
                var name = split[2].TrimEnd(';');

                var nameLower = name.ToLower();
                if (nameLower == "u_world" | nameLower == "u_src" | nameLower == "u_depth")
                    continue;

                if (PropertyTypes.ContainsKey(name))
                    continue;

                switch (type)
                {
                    case "float":
                        PropertyTypes.Add(name, typeof(float));
                        Properties.Add(name, 0.0f);
                        break;
                    case "vec2":
                        PropertyTypes.Add(name, typeof(vec2));
                        Properties.Add(name, new vec2());
                        break;
                    case "vec3":
                        PropertyTypes.Add(name, typeof(vec3));
                        Properties.Add(name, new vec3());
                        break;
                    case "vec4":
                        PropertyTypes.Add(name, typeof(vec4));
                        Properties.Add(name, new vec4());
                        break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
