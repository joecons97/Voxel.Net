using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialEditor.Controls;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace MaterialEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Material> OpenMaterials { get; set; }

        private StackPanel dynamicPanel;

        void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            Save_OnClicked(sender, args);
        }

        public MainWindow()
        {
            InitializeComponent();

            OpenMaterials = new ObservableCollection<Material>();

            TabControl.ItemsSource = OpenMaterials;
            TabControl.DataContext = OpenMaterials;
        }

        async void UpdateMaterialView(Material material)
        {
            while (dynamicPanel == null)
                await Task.Delay(100);

            dynamicPanel.Children.Clear();

            if (string.IsNullOrEmpty(material.ShaderLocation))
                return;

            foreach (var key in material.PropertyTypes.Keys)
            {
                var type = material.PropertyTypes[key];

                switch (type)
                {
                    case var f when f == typeof(float):
                        AddFloatField(key, material);
                        break;
                    case var f when f == typeof(vec2):
                        AddVec2Field(key, material);
                        break;
                    case var f when f == typeof(vec3):
                        AddVec3Field(key, material);
                        break;
                    case var f when f == typeof(vec4):
                        AddVec4Field(key, material);
                        break;
                }
            }
        }

        private void New_OnClick(object sender, RoutedEventArgs e)
        {
            var num = OpenMaterials.Count(x => x.Name.StartsWith("New Material"));
            string name = "New Material";
            if (num > 0)
                name += "(" + num.ToString() + ")";
            OpenMaterials.Add(new Material(name));
        }

        private void FilePicker_OnFileSelected(object sender, string e)
        {
            OpenMaterials[TabControl.SelectedIndex].ShaderLocation = e;
            UpdateMaterialView(OpenMaterials[TabControl.SelectedIndex]);
        }

        private void ProjectPrefs_OnClick(object sender, RoutedEventArgs e)
        {
            ProjectLocationWindow prefs = new ProjectLocationWindow();
            prefs.ShowDialog();
        }

        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedIndex == -1)
                return;

            Material selected = OpenMaterials[TabControl.SelectedIndex];
            UpdateMaterialView(selected);
        }

        private void DynamicPanel_OnLoaded(object sender, RoutedEventArgs e)
        {
            dynamicPanel = (StackPanel) sender;
        }

        private static readonly Regex numericTextBoxRegex = new Regex("[^0-9.-]+"); //regex that matches disallowed text

        void AddFloatField(string name, Material material)
        {
            DockPanel panel = new DockPanel();
            panel.LastChildFill = true;
            panel.Margin = new Thickness(0, 5, 0, 5);
            panel.Children.Add(new TextBlock() {Text = name, Margin = new Thickness(5, 0, 5, 0)});
            var textBox = new TextBox();
            textBox.Text = material.Properties[name].ToString();
            textBox.PreviewTextInput += (sender, args) => { args.Handled = numericTextBoxRegex.IsMatch(args.Text); };
            textBox.TextChanged += (sender, args) =>
            {
                if (float.TryParse(textBox.Text, out float f))
                {
                    material.Properties[name] = f;
                }
                material.IsDirty = true;
            };
            panel.Children.Add(textBox);

            dynamicPanel.Children.Add(panel);
        }
        void AddVec2Field(string name, Material material)
        {
            DockPanel panel = new DockPanel();
            panel.LastChildFill = true;
            panel.Margin = new Thickness(0, 5, 0, 5);
            panel.Children.Add(new TextBlock() { Text = name, Margin = new Thickness(5, 0, 5, 0) });
            StackPanel vecStack = new StackPanel();

            var x = new TextBox();
            x.Text = ((vec2)material.Properties[name]).x.ToString();
            x.PreviewTextInput += (sender, args) => { args.Handled = numericTextBoxRegex.IsMatch(args.Text); }; 
            x.TextChanged += (sender, args) =>
            {
                if (float.TryParse(x.Text, out float f))
                {
                    ((vec2)material.Properties[name]).x = f;
                }
                else
                    ((vec2)material.Properties[name]).x = 0;

                x.Text = ((vec2)material.Properties[name]).x.ToString();
                material.IsDirty = true;
            };
            vecStack.Children.Add(x);

            var y = new TextBox();
            y.Text = ((vec2)material.Properties[name]).y.ToString();
            y.PreviewTextInput += (sender, args) => { args.Handled = numericTextBoxRegex.IsMatch(args.Text); };
            y.TextChanged += (sender, args) =>
            {
                if (float.TryParse(y.Text, out float f))
                {
                    ((vec2)material.Properties[name]).y = f;
                }
                else
                    ((vec2)material.Properties[name]).y = 0;

                y.Text = ((vec2)material.Properties[name]).y.ToString();
                material.IsDirty = true;
            };
            vecStack.Children.Add(y);

            panel.Children.Add(vecStack);

            dynamicPanel.Children.Add(panel);
        }

        void AddVec3Field(string name, Material material)
        {
            DockPanel panel = new DockPanel();
            panel.LastChildFill = true;
            panel.Margin = new Thickness(0, 5, 0, 5);
            panel.Children.Add(new TextBlock() { Text = name, Margin = new Thickness(5, 0, 5, 0) });
            StackPanel vecStack = new StackPanel();

            var x = new TextBox();
            x.Text = ((vec3)material.Properties[name]).x.ToString();
            x.PreviewTextInput += (sender, args) => { args.Handled = numericTextBoxRegex.IsMatch(args.Text); };
            x.TextChanged += (sender, args) =>
            {
                if (float.TryParse(x.Text, out float f))
                {
                    ((vec3)material.Properties[name]).x = f;
                }
                else
                    ((vec3)material.Properties[name]).x = 0;

                x.Text = ((vec3)material.Properties[name]).x.ToString();
                material.IsDirty = true;
            };
            vecStack.Children.Add(x);

            var y = new TextBox();
            y.Text = ((vec3)material.Properties[name]).y.ToString();
            y.PreviewTextInput += (sender, args) => { args.Handled = numericTextBoxRegex.IsMatch(args.Text); };
            y.TextChanged += (sender, args) =>
            {
                if (float.TryParse(y.Text, out float f))
                {
                    ((vec3)material.Properties[name]).y = f;
                }
                else
                    ((vec3)material.Properties[name]).y = 0;

                y.Text = ((vec3)material.Properties[name]).y.ToString();
                material.IsDirty = true;
            };
            vecStack.Children.Add(y);

            var z = new TextBox();
            z.Text = ((vec3)material.Properties[name]).z.ToString();
            z.PreviewTextInput += (sender, args) => { args.Handled = numericTextBoxRegex.IsMatch(args.Text); };
            z.TextChanged += (sender, args) =>
            {
                if (float.TryParse(z.Text, out float f))
                {
                    ((vec3)material.Properties[name]).z = f;
                }
                else
                    ((vec3)material.Properties[name]).z = 0;

                z.Text = ((vec3)material.Properties[name]).z.ToString();
                material.IsDirty = true;
            };
            vecStack.Children.Add(z);

            panel.Children.Add(vecStack);

            dynamicPanel.Children.Add(panel);
        }

        void AddVec4Field(string name, Material material)
        {
            DockPanel panel = new DockPanel();
            panel.LastChildFill = true;
            panel.Margin = new Thickness(0, 5, 0, 5);
            panel.Children.Add(new TextBlock() { Text = name, Margin = new Thickness(5, 0, 5, 0) });
            StackPanel vecStack = new StackPanel();

            var x = new TextBox();
            x.Text = ((vec4)material.Properties[name]).x.ToString();
            x.PreviewTextInput += (sender, args) => { args.Handled = numericTextBoxRegex.IsMatch(args.Text); };
            x.TextChanged += (sender, args) =>
            {
                if (float.TryParse(x.Text, out float f))
                {
                    ((vec4)material.Properties[name]).x = f;
                }
                else
                {
                    ((vec4)material.Properties[name]).x = 0;
                }
                x.Text = ((vec4)material.Properties[name]).x.ToString();
                material.IsDirty = true;
            };
            vecStack.Children.Add(x);

            var y = new TextBox();
            y.Text = ((vec4)material.Properties[name]).y.ToString();
            y.PreviewTextInput += (sender, args) => { args.Handled = numericTextBoxRegex.IsMatch(args.Text); };
            y.TextChanged += (sender, args) =>
            {
                if (float.TryParse(y.Text, out float f))
                {
                    ((vec4)material.Properties[name]).y = f;
                }
                else
                {
                    ((vec4)material.Properties[name]).y = 0;
                }
                y.Text = ((vec4)material.Properties[name]).y.ToString();
                material.IsDirty = true;
            };
            vecStack.Children.Add(y);

            var z = new TextBox();
            z.Text = ((vec4)material.Properties[name]).z.ToString();
            z.PreviewTextInput += (sender, args) => { args.Handled = numericTextBoxRegex.IsMatch(args.Text); };
            z.TextChanged += (sender, args) =>
            {
                if (float.TryParse(z.Text, out float f))
                {
                    ((vec4)material.Properties[name]).z = f;
                }
                else
                {
                    ((vec4)material.Properties[name]).z = 0;
                }
                z.Text = ((vec4)material.Properties[name]).z.ToString();
                material.IsDirty = true;
            };
            vecStack.Children.Add(z);

            var w = new TextBox();
            w.Text = ((vec4)material.Properties[name]).w.ToString();
            w.PreviewTextInput += (sender, args) => { args.Handled = numericTextBoxRegex.IsMatch(args.Text); };
            w.TextChanged += (sender, args) =>
            {
                if (float.TryParse(w.Text, out float f))
                {
                    ((vec4)material.Properties[name]).w = f;
                }
                else
                {
                    ((vec4)material.Properties[name]).w = 0;
                }
                w.Text = ((vec4)material.Properties[name]).w.ToString();
                material.IsDirty = true;
            };
            vecStack.Children.Add(w);

            panel.Children.Add(vecStack);

            dynamicPanel.Children.Add(panel);
        }

        private void Open_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Settings.Instance.ProjectLocation;
            ofd.DefaultExt = "mat";
            ofd.Filter = "Voxel.Net Material file (*.mat)|*.mat";
            ofd.Multiselect = false;
            if (ofd.ShowDialog().GetValueOrDefault(false))
            {
                OpenMaterials.Add(new Material(ofd.FileName, Path.GetFileNameWithoutExtension(ofd.FileName)));
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var mat = OpenMaterials.FirstOrDefault(x => x.ID == ((Button) e.Source).CommandParameter as string);
            if (mat.IsDirty)
            {
                var result = MessageBox.Show("Changes have been made to " + mat.Name + ", would you like to save them?", "Alert", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                    return;

                if (result == MessageBoxResult.Yes)
                {
                    Save_OnClicked(sender, e);
                }
            }

            OpenMaterials.Remove(mat);
        }

        private void Save_OnClicked(object sender, RoutedEventArgs e)
        {
            if (TabControl.SelectedIndex == -1) return;

            var mat = OpenMaterials[TabControl.SelectedIndex];
            if (mat != null)
            {
                if (string.IsNullOrEmpty(mat.SaveLocation))
                {
                    SaveAs_OnClicked(sender, e);
                }
                else
                {
                    mat.Save();
                }
            }
        }

        private void SaveAs_OnClicked(object sender, RoutedEventArgs e)
        {
            if (TabControl.SelectedIndex == -1) return;

            var mat = OpenMaterials[TabControl.SelectedIndex];

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = Settings.Instance.ProjectLocation;
            sfd.DefaultExt = "material";
            sfd.Filter = "Voxel.Net Material file (*.mat)|*.mat";

            if (sfd.ShowDialog().GetValueOrDefault(false))
            {
                if (sfd.FileName.StartsWith(Settings.Instance.ProjectLocation))
                {
                    mat.SaveLocation = sfd.FileName;
                    mat.Name = Path.GetFileNameWithoutExtension(sfd.FileName);
                    mat.Save();
                }
            }
        }
    }
}

