using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MaterialEditor.Controls
{
    /// <summary>
    /// Interaction logic for ProjectLocationWindow.xaml
    /// </summary>
    public partial class ProjectLocationWindow : Window
    {
        public ProjectLocationWindow()
        {
            InitializeComponent();

            ProjLocPicker.FileName = Settings.Instance.ProjectLocation;
        }

        private void FilePicker_OnFileSelected(object sender, string e)
        {
            Settings.Instance.ProjectLocation = e;
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Save();
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
