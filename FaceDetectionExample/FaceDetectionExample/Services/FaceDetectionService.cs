using Emgu.CV;
using Emgu.CV.GPU;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceDetectionExample.Services
{
    public class FaceDetectionService : WebCamService
    {
        private readonly string _faceFileName = "haarcascade_frontalface_default.xml";
        private readonly string _eyeFileName = "haarcascade_eye.xml";

        private List<Rectangle> _faces = new List<Rectangle>();
        private List<Rectangle> _eyes = new List<Rectangle>();

        public event ImageWithDetectionChangedEventHandler ImageWithDetectionChanged;
        public delegate void ImageWithDetectionChangedEventHandler(object sender, Image<Bgr, Byte> image);

        public FaceDetectionService()
        {
            InitializeServices();
        }

        private void InitializeServices()
        {
            base.ImageChanged += _webCamService_ImageChanged;
        }

        private void RaiseImageWithDetectionChangedEvent(Image<Bgr, Byte> image)
        {
            if (ImageWithDetectionChanged != null)
            {
                ImageWithDetectionChanged(this, image);
            }
        }

        private bool isDetecting = false;
        private async void _webCamService_ImageChanged(object sender, Image<Bgr, byte> image)
        {
            bool isDelayed = false;

            if (!isDetecting)
            {
                isDetecting = true;

                var result = await DetectFacesAsync(image);

                isDelayed = true;
                _faces = result;

                isDetecting = false;
            }

            if (!isDelayed)// to prevent displaing delayed image
            {
                DrawRectangles(image);
                RaiseImageWithDetectionChangedEvent(image);
            }
        }

        private void DrawRectangles(Image<Bgr, byte> image)
        {
            foreach (Rectangle face in _faces)
                image.Draw(face, new Bgr(Color.Red), 2);
            foreach (Rectangle eye in _eyes)
                image.Draw(eye, new Bgr(Color.Blue), 1);
        }

        private Task<Tuple<List<Rectangle>, List<Rectangle>>> DetectFacesAndEyesAsync(Image<Bgr, byte> image)
        {
            return Task.Run(() =>
            {
                List<Rectangle> faces = new List<Rectangle>();
                List<Rectangle> eyes = new List<Rectangle>();

                DetectFaceAndEyes(image, faces, eyes);

                return new Tuple<List<Rectangle>, List<Rectangle>>(faces, eyes);
            });
        }

        private Task<List<Rectangle>> DetectFacesAsync(Image<Bgr, byte> image)
        {
            return Task.Run(() =>
            {
                List<Rectangle> faces = new List<Rectangle>();

                DetectFace(image, faces);

                return faces;
            });
        }

        private void DetectFace(Image<Bgr, Byte> image, List<Rectangle> faces)
        {
#if !IOS
            if (GpuInvoke.HasCuda)
            {
                using (GpuCascadeClassifier face = new GpuCascadeClassifier(_faceFileName))
                {
                    using (GpuImage<Bgr, Byte> gpuImage = new GpuImage<Bgr, byte>(image))
                    using (GpuImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
                    {
                        Rectangle[] faceRegion = face.DetectMultiScale(gpuGray, 1.1, 10, Size.Empty);
                        faces.AddRange(faceRegion);
                    }
                }
            }
            else
#endif
            {
                //Read the HaarCascade objects
                using (CascadeClassifier face = new CascadeClassifier(_faceFileName))
                {
                    using (Image<Gray, Byte> gray = image.Convert<Gray, Byte>()) //Convert it to Grayscale
                    {
                        //normalizes brightness and increases contrast of the image
                        gray._EqualizeHist();

                        //Detect the faces  from the gray scale image and store the locations as rectangle
                        //The first dimensional is the channel
                        //The second dimension is the index of the rectangle in the specific channel
                        Rectangle[] facesDetected = face.DetectMultiScale(
                           gray,
                           1.1,
                           10,
                           new Size(20, 20),
                           Size.Empty);
                        faces.AddRange(facesDetected);
                    }
                }
            }
        }

        private void DetectFaceAndEyes(Image<Bgr, Byte> image, List<Rectangle> faces, List<Rectangle> eyes)
        {
#if !IOS
            if (GpuInvoke.HasCuda)
            {
                using (GpuCascadeClassifier face = new GpuCascadeClassifier(_faceFileName))
                using (GpuCascadeClassifier eye = new GpuCascadeClassifier(_eyeFileName))
                {
                    using (GpuImage<Bgr, Byte> gpuImage = new GpuImage<Bgr, byte>(image))
                    using (GpuImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
                    {
                        Rectangle[] faceRegion = face.DetectMultiScale(gpuGray, 1.1, 10, Size.Empty);
                        faces.AddRange(faceRegion);
                        foreach (Rectangle f in faceRegion)
                        {
                            using (GpuImage<Gray, Byte> faceImg = gpuGray.GetSubRect(f))
                            {
                                //For some reason a clone is required.
                                //Might be a bug of GpuCascadeClassifier in opencv
                                using (GpuImage<Gray, Byte> clone = faceImg.Clone())
                                {
                                    Rectangle[] eyeRegion = eye.DetectMultiScale(clone, 1.1, 10, Size.Empty);

                                    foreach (Rectangle e in eyeRegion)
                                    {
                                        Rectangle eyeRect = e;
                                        eyeRect.Offset(f.X, f.Y);
                                        eyes.Add(eyeRect);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
#endif
            {
                //Read the HaarCascade objects
                using (CascadeClassifier face = new CascadeClassifier(_faceFileName))
                using (CascadeClassifier eye = new CascadeClassifier(_eyeFileName))
                {
                    using (Image<Gray, Byte> gray = image.Convert<Gray, Byte>()) //Convert it to Grayscale
                    {
                        //normalizes brightness and increases contrast of the image
                        gray._EqualizeHist();

                        //Detect the faces  from the gray scale image and store the locations as rectangle
                        //The first dimensional is the channel
                        //The second dimension is the index of the rectangle in the specific channel
                        Rectangle[] facesDetected = face.DetectMultiScale(
                           gray,
                           1.1,
                           10,
                           new Size(20, 20),
                           Size.Empty);
                        faces.AddRange(facesDetected);

                        foreach (Rectangle f in facesDetected)
                        {
                            //Set the region of interest on the faces
                            gray.ROI = f;
                            Rectangle[] eyesDetected = eye.DetectMultiScale(
                               gray,
                               1.1,
                               10,
                               new Size(20, 20),
                               Size.Empty);
                            gray.ROI = Rectangle.Empty;

                            foreach (Rectangle e in eyesDetected)
                            {
                                Rectangle eyeRect = e;
                                eyeRect.Offset(f.X, f.Y);
                                eyes.Add(eyeRect);
                            }
                        }
                    }
                }
            }
        }
    }
}
