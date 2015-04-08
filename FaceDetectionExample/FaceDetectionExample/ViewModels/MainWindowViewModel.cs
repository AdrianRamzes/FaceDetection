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
        private WebCamService _webCamService;

        private List<Rectangle> _faces = new List<Rectangle>();
        private List<Rectangle> _eyes = new List<Rectangle>();

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
            _webCamService = new WebCamService();
            _webCamService.ImageChanged += _webCamService_ImageChanged;
            _faceDetectionService = new FaceDetectionService();
        }

        private bool isRunning = false;
        private async void _webCamService_ImageChanged(object sender, Image<Bgr, byte> image)
        {
            bool isDetecting = false;

            if (!isRunning)
            {
                isRunning = true;
                isDetecting = true;
                var result = await DetectAsync(image);
                //var result = await DetectFacesAsync(image);
                //_faces = result;

                _faces = result.Item1;
                _eyes = result.Item2;

                isRunning = false;
            }

            if (!isDetecting)
            {
                foreach (Rectangle face in _faces)
                    image.Draw(face, new Bgr(Color.Red), 2);
                foreach (Rectangle eye in _eyes)
                    image.Draw(eye, new Bgr(Color.Blue), 2);

                this.Frame = image.Bitmap;
            }
        }

        private Task<Tuple<List<Rectangle>, List<Rectangle>>> DetectAsync(Image<Bgr, byte> image)
        {
            return Task.Run(() =>
            {
                List<Rectangle> faces = new List<Rectangle>();
                List<Rectangle> eyes = new List<Rectangle>();

                _faceDetectionService.Detect(image, faces, eyes);

                return new Tuple<List<Rectangle>, List<Rectangle>>(faces, eyes);
            });
        }

        private Task<List<Rectangle>> DetectFacesAsync(Image<Bgr, byte> image)
        {
            return Task.Run(() =>
            {
                List<Rectangle> faces = new List<Rectangle>();

                _faceDetectionService.DetectFace(image, faces);

                return faces;
            });
        }

        private void InitializeCommands()
        {
            _toggleWebServiceCommand = new DelegateCommand(ToggleWebServiceExecute);
        }

        private void ToggleWebServiceExecute()
        {
            if (!_webCamService.IsRunning)
            {
                _webCamService.RunServiceAsync();
            }
            else
            {
                _webCamService.CancelServiceAsync();
            }
        }
    }
}
