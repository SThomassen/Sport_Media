using Newtonsoft.Json;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace HoloSports.Calibration
{
    public class CalibrationService : MonoBehaviour
    {
        [SerializeField] private CalibrationManager m_calibration = null;

        private string m_preditionKey = "045a085b59714007a0cce054bc27fd5a";
        private string m_preditionEndpoint = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/dca1be4d-8849-4a0f-b2e8-33e1376158d4/image";

        private string m_path;
        private byte[] m_imageBytes;

        public void StartService(string a_imagePath)
        {
            m_path = a_imagePath;
            StartCoroutine(CheckConnection());
        }

        public IEnumerator AnalyzeImage(string a_imagePath)
        {
            Debug.Log("Analyzing..");

            WWWForm form = new WWWForm();

            using (UnityWebRequest unityRequest = UnityWebRequest.Post(m_preditionEndpoint, form))
            {
                m_imageBytes = GetImageAsByteArray(a_imagePath);

                unityRequest.SetRequestHeader("Content-Type", "application/octet-stream");
                unityRequest.SetRequestHeader("Prediction-Key", m_preditionKey);

                // The upload handler will help uploading the byte array with the request
                unityRequest.uploadHandler = new UploadHandlerRaw(m_imageBytes);
                unityRequest.uploadHandler.contentType = "application/octet-stream";

                // The download handler will help receiving the analysis from Azure
                unityRequest.downloadHandler = new DownloadHandlerBuffer();

                // Send the request
                yield return unityRequest.SendWebRequest();

                string jsonResponse = unityRequest.downloadHandler.text;

                Debug.Log("response: " + jsonResponse);
                if (jsonResponse.Equals(string.Empty))
                {
                    m_calibration.CancelPrediction();
                }

                // Create a texture. Texture size does not matter, since
                // LoadImage will replace with the incoming image size.
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(m_imageBytes);
                m_calibration.SetQuadTexture(tex);

                //The response will be in JSON format, therefore it needs to be deserialized
                AnalysisRootObject analysisRootObject = new AnalysisRootObject();
                analysisRootObject = JsonConvert.DeserializeObject<AnalysisRootObject>(jsonResponse);

                m_calibration.FinalisePrediction(analysisRootObject);
            }
        }

        static byte[] GetImageAsByteArray(string a_imagePath)
        {
            FileStream stream = new FileStream(a_imagePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            return reader.ReadBytes((int)stream.Length);
        }

        private IEnumerator CheckConnection()
        {
            bool connected = true;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                connected = false;
            }
            WWW www = new WWW("www.google.com");
            yield return www;

            if (www.error != null)
            {
                connected = false;
            }

            if (connected)
                StartCoroutine(AnalyzeImage(m_path));
            else
                m_calibration.CancelPrediction();
        }
    }
}