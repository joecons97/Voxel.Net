using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using UserControl = System.Windows.Controls.UserControl;

namespace MaterialEditor
{
    /// <summary>
    /// Interaction logic for FilePicker.xaml
    /// </summary>
    public partial class FilePicker : UserControl
    {
        public enum SelectionTypeEnum
        {
            File,
            Folder
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value);}
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(FilePicker), new PropertyMetadata(null));

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(FilePicker), new PropertyMetadata(null));

        public SelectionTypeEnum SelectionType
        {
            get { return (SelectionTypeEnum)GetValue(SelectionTypeProperty); }
            set { SetValue(SelectionTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectionTypeProperty =
            DependencyProperty.Register("SelectionType", typeof(SelectionTypeEnum), typeof(FilePicker), new PropertyMetadata(defaultValue: SelectionTypeEnum.File));

        public bool ShouldBeInProject
        {
            get { return (bool)GetValue(ShouldBeInProjectProperty); }
            set { SetValue(ShouldBeInProjectProperty, value); }
        }

        public static readonly DependencyProperty ShouldBeInProjectProperty =
            DependencyProperty.Register("ShouldBeInProject", typeof(bool), typeof(FilePicker), new PropertyMetadata(defaultValue: false));

        public event EventHandler<string> FileSelected;

        public FilePicker()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            switch (SelectionType)
            {
                case SelectionTypeEnum.File:
                    OpenFileDialog ofp = new OpenFileDialog();
                    ofp.InitialDirectory = Settings.Instance.ProjectLocation;
                    ofp.DefaultExt = "shader";
                    ofp.Filter = "Voxel.Net Shader file (*.shader)|*.shader";
                    ofp.Multiselect = false;
                    if (ofp.ShowDialog().GetValueOrDefault(false))
                    {
                        if (ShouldBeInProject)
                        {
                            if (ofp.FileName.StartsWith(Settings.Instance.ProjectLocation))
                            {
                                FileName = ofp.FileName;

                                FileSelected?.Invoke(this, ofp.FileName);
                            }
                            else
                            {
                                MessageBox.Show(
                                    "You must select a file that is in the Project Directory (see Project Preferences)",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            FileName = ofp.FileName;

                            FileSelected?.Invoke(this, ofp.FileName);
                        }
                    }
                    break;
                case SelectionTypeEnum.Folder:
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.SelectedPath = FileName;
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        if (ShouldBeInProject)
                        {
                            if (fbd.SelectedPath.StartsWith(Settings.Instance.ProjectLocation))
                            {
                                FileName = fbd.SelectedPath;

                                FileSelected?.Invoke(this, fbd.SelectedPath);
                            }
                            else
                            {
                                MessageBox.Show(
                                    "You must select a folder that is in the Project Directory (see Project Preferences)",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            FileName = fbd.SelectedPath;

                            FileSelected?.Invoke(this, fbd.SelectedPath);
                        }
                    }
                    break;
            }
        }
    }
}
