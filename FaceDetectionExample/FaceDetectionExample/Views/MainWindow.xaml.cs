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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaceDetectionExample.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool _isRunning = false;

        private void v_Button_ToggleWebCam_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = !_isRunning;

            v_Button_ToggleWebCam.Content = _isRunning ? "Stop" : "Start";
        }
    }
}
