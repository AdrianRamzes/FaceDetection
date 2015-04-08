using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Drawing;

namespace FaceDetectionExample.Services
{
    public class WebCamService
    {
        private Capture _capture;
        private BackgroundWorker _webCamWorker;

        public event ImageChangedEventHndler ImageChanged;
        public delegate void ImageChangedEventHndler(object sender, Image<Bgr, Byte> image);

        public bool IsRunning
        {
            get
            {
                return (_webCamWorker != null) ? _webCamWorker.IsBusy : false;
            }
        }

        public void RunServiceAsync()
        {
            _webCamWorker.RunWorkerAsync();
        }

        public void CancelServiceAsync()
        {
            if (_webCamWorker != null)
            {
                _webCamWorker.CancelAsync();
            }
        }

        private void RaiseImageChangedEvent(Image<Bgr, Byte> image)
        {
            if (ImageChanged != null)
            {
                ImageChanged(this, image);
            }
        }

        public WebCamService()
        {
            _capture = new Emgu.CV.Capture();
            InitializeWorkers();
        }

        private void InitializeWorkers()
        {
            _webCamWorker = new BackgroundWorker();
            _webCamWorker.WorkerSupportsCancellation = true;
            _webCamWorker.DoWork += _webCamWorker_DoWork;
            _webCamWorker.RunWorkerCompleted += _webCamWorker_RunWorkerCompleted;
        }
        private void _webCamWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_webCamWorker.CancellationPending)
            {
                RaiseImageChangedEvent(_capture.QueryFrame().Copy());
            }
        }
        private void _webCamWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
    }
}
