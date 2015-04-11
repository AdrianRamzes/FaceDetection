using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FaceDetectionExample.Helpers;
using FaceDetectionExample.Services;
using Emgu.CV;
using Emgu.CV.Structure;

namespace FaceDetectionExample.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private FaceDetectionService _faceDetectionService;

        private Bitmap _frame;
        public Bitmap Frame
        {
            get
            {
                return _frame;
            }

            set
            {
                if (_frame != value)
                {
                    _frame = value;
                    RaisePropertyChanged(() => Frame);
                }
            }
        }

        private ICommand _toggleWebServiceCommand;
        public ICommand ToggleWebServiceCommand
        {
            get
            {
                return _toggleWebServiceCommand;
            }

            private set { }
        }

        public MainWindowViewModel()
        {
            InitializeServices();
            InitializeCommands();
        }

        private void InitializeServices()
        {
            _faceDetectionService = new FaceDetectionService();
            _faceDetectionService.ImageWithDetectionChanged += _faceDetectionService_ImageChanged;
        }

        private void _faceDetectionService_ImageChanged(object sender, Image<Bgr, byte> image)
        {
            this.Frame = image.Bitmap;
        }

        private void InitializeCommands()
        {
            _toggleWebServiceCommand = new DelegateCommand(ToggleWebServiceExecute);
        }

        private void ToggleWebServiceExecute()
        {
            if (!_faceDetectionService.IsRunning)
            {
                _faceDetectionService.RunServiceAsync();
            }
            else
            {
                _faceDetectionService.CancelServiceAsync();
            }
        }
    }
}
