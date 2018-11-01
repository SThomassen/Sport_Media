using UnityEngine;
using System.Linq;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR.WSA.WebCam;
using System;
using System.IO;
using MixedRealityToolkit.UX.Progress;

namespace HoloSports.Calibration
{
    public class ImageCapture : MonoBehaviour
    {
        private int m_captureCount = 0;
        private PhotoCapture m_photoCapture = null;
        private GestureRecognizer m_gesture;

        internal bool m_captureIsActive;
        internal string m_path = "";

        [SerializeField] private CalibrationManager m_calibration = null;
        [SerializeField] private CalibrationService m_service = null;

        // Use this for initialization
        void Awake()
        {
            // Clean up the LocalState folder of this application from all photos stored
            DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
            var fileInfo = info.GetFiles();
            foreach (var file in fileInfo)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception)
                {
                    Debug.LogFormat("Cannot delete file: ", file.Name);
                }
            }

            m_gesture = new GestureRecognizer();
        }

        public void RegisterInputHandlers()
        {
            // Subscribing to the Microsoft HoloLens API gesture recognizer to track user gestures
            m_gesture.SetRecognizableGestures(GestureSettings.Tap);
            m_gesture.Tapped += TapHandler;
            m_gesture.StartCapturingGestures();
        }

        public void RemoveInputHandlers()
        {
            m_gesture.Tapped -= TapHandler;
            m_gesture.StopCapturingGestures();
        }

        // <summary>
        /// Respond to Tap Input.
        /// </summary>
        private void TapHandler(TappedEventArgs obj)
        {
            Debug.Log("Tap Image Capture");
            if (!m_captureIsActive && gameObject.activeSelf)
            {
                m_captureIsActive = true;
                RemoveInputHandlers();

                ProgressIndicator.Instance.Close();

                // Begin the capture loop
                ExecuteImageCaptureAndAnalysis();
            }
        }

        /// <summary>
        /// Begin process of image capturing and send to Azure Custom Vision Service.
        /// </summary>
        private void ExecuteImageCaptureAndAnalysis()
        {
            Debug.Log("Exucute Image Capture & Analysis");
            ProgressIndicator.Instance.Open("Analyzing...");
            
            // Create a label in world space using the ResultsLabel class 
            // Invisible at this point but correctly positioned where the image was taken
            m_calibration.InitiatePrediction();

            // Set the camera resolution to be the highest possible
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending
                ((res) => res.width * res.height).First();
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

            // Begin capture process, set the image format
            PhotoCapture.CreateAsync(true, delegate (PhotoCapture captureObject)
            {
                m_photoCapture = captureObject;

                CameraParameters camParameters = new CameraParameters
                {
                    hologramOpacity = 0.0f,
                    cameraResolutionWidth = targetTexture.width,
                    cameraResolutionHeight = targetTexture.height,
                    pixelFormat = CapturePixelFormat.BGRA32
                };

                // Capture the image from the camera and save it in the App internal folder
                captureObject.StartPhotoModeAsync(camParameters, delegate (PhotoCapture.PhotoCaptureResult result)
                {
                    string filename = string.Format(@"CapturedImage{0}.jpg", m_captureCount);
                    m_path = Path.Combine(Application.persistentDataPath, filename);
                    m_captureCount++;
                    m_photoCapture.TakePhotoAsync(m_path, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
                });
            });
        }

        /// <summary>
        /// Register the full execution of the Photo Capture. 
        /// </summary>
        private void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult a_result)
        {
            try
            {
                // Call StopPhotoMode once the image has successfully captured
                m_photoCapture.StopPhotoModeAsync(OnStoppedPhotoMode);
            }
            catch (Exception e)
            {
                Debug.LogFormat("Exception capturing photo to disk: {0}", e.Message);
            }
        }

        /// <summary>
        /// The camera photo mode has stopped after the capture.
        /// Begin the image analysis process.
        /// </summary>
        private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult a_result)
        {
            Debug.LogFormat("Stopped Photo Mode");

            // Dispose from the object in memory and request the image analysis 
            m_photoCapture.Dispose();
            m_photoCapture = null;

            // Call the image analysis
            m_service.StartService(m_path);
        }

        /// <summary>
        /// Stops all capture pending actions
        /// </summary>
        internal void ResetImageCapture()
        {
            m_captureIsActive = false;

            // Set the cursor color to green
            ProgressIndicator.Instance.Close();

            // Stop the capture loop if active
            CancelInvoke();
        }
    }
}