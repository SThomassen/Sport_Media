using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MixedRealityToolkit.InputModule.Cursor;
using MixedRealityToolkit.UX.Progress;

namespace HoloSports.Calibration
{
    public class CalibrationManager : MonoBehaviour
    {
        [SerializeField] private Main m_main = null;
        [SerializeField] private ImageCapture m_imageCapture = null;
        [SerializeField] private float m_probability = 0.9f;

        private Renderer m_render;
        private GameObject m_quad;
        private Transform m_camera;

        public void StartCalibration()
        {
            ProgressIndicator.Instance.Open(IndicatorStyleEnum.None, 
                                            ProgressStyleEnum.None, 
                                            MessageStyleEnum.Visible,
                                            "Tap to start Calibration");
            m_imageCapture.RegisterInputHandlers();
        }

        public void SetQuadTexture(Texture2D a_texture)
        {
            if (m_render == null || a_texture == null) return;
            m_render.material.SetTexture("_MainTex", a_texture);
        }

        /// <summary>
        /// Instantiate a GameObject in the appropriate location relative to the Main Camera.
        /// </summary>
        public void InitiatePrediction()
        {
            m_camera = Camera.main.transform;

            if (m_quad != null)
            {
                Destroy(m_quad);
            }

            // Create a GameObject to which the texture can be applied
            m_quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_render = m_quad.GetComponent<Renderer>() as Renderer;
            Material m = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
            m_render.material = m;

            // Here you can set the transparency of the quad. Useful for debugging. 0 = hidden, 1 = visible
            m_render.material.color = new Color(1, 1, 1, 0);

            // Set the position and scale of the quad depending on user position
            m_quad.transform.parent = transform;
            m_quad.transform.rotation = Quaternion.Euler(0, m_camera.rotation.eulerAngles.y,0);

            // The quad is positioned slightly forward in font of the user
            m_quad.transform.localPosition = m_camera.forward * 3.0f;// new Vector3(0.0f, 0.0f, 3.0f);
            // The quad scale as been set with the following value following experimentation,  
            // to allow the image on the quad to be as precisely imposed to the real world as possible
            m_quad.transform.localScale = new Vector3(3f, 1.65f, 1f);
            m_quad.transform.parent = null;
        }

        public void CancelPrediction()
        {
            Debug.Log("No Internet Connection Available");
            GameObject target = new GameObject();
            target.transform.position = m_quad.transform.position;
            target.transform.rotation = m_quad.transform.rotation;
            target.transform.localScale = new Vector3(0.64f, 0.4f, 0.001f);
            m_main.InitiateAnchor(target.transform);

            ProgressIndicator.Instance.Close();

            //Stop the analysis process
            m_imageCapture.ResetImageCapture();
        }

        // <summary>
        /// Initialize World Anchor based on best prediction
        /// </summary>
        public void FinalisePrediction(AnalysisRootObject a_analysisObject)
        {
            if (a_analysisObject != null && a_analysisObject.predictions != null)
            {
                //Sort the predictions to locate the highest one
                List<Prediction> sortedPredictions = new List<Prediction>();
                sortedPredictions = a_analysisObject.predictions.OrderBy(p => p.probability).ToList();
                Prediction bestPrediction = new Prediction();
                bestPrediction = sortedPredictions[sortedPredictions.Count - 1];

                if (bestPrediction.probability > m_probability)
                {
                    Bounds quadBounds = m_render.bounds;
                    //Get position as close as possible to the Bounding Box of the prediction 
                    Transform bound = CalculateBoundingBoxPosition(quadBounds, bestPrediction.boundingBox);

                    // Check if prediction tag is equal to television
                    Debug.LogFormat("tag Name: {0}", bestPrediction.tagName);
                    if (bestPrediction.tagName == "television")
                    {
                        m_main.InitiateAnchor(bound);
                    }
                }
                else
                {
                    Debug.LogWarning("Prediction too small");
                    StartCalibration();
                }
            }
            else
            {
                Debug.LogWarning("Analyse no success");
                StartCalibration();
            }
            ProgressIndicator.Instance.Close();

            //Stop the analysis process
            m_imageCapture.ResetImageCapture();
        }
           
        /// <summary>
        /// This method hosts a series of calculations to determine the position 
        /// of the Bounding Box on the quad created in the real world
        /// by using the Bounding Box received back alongside the Best Prediction
        /// </summary>
        private Transform CalculateBoundingBoxPosition(Bounds a_bounds, BoundingBox a_boundingBox)
        {
            Debug.LogFormat("BB: left {0}, top {1}, width {2}, height {3}", a_boundingBox.left, a_boundingBox.top, a_boundingBox.width, a_boundingBox.height);

            float centerX =  (float)(a_boundingBox.left + (a_boundingBox.width * 0.5f));
            float centerY = (float)(a_boundingBox.top + (a_boundingBox.height * 0.5f));
            Debug.LogFormat("BB CenterFromLeft {0}, CenterFromTop {1}", centerX, centerY);

            float quadWidth = a_bounds.size.normalized.x;
            float quadHeight = a_bounds.size.normalized.y;
            Debug.LogFormat("Quad Width {0}, Quad Height {1}", a_bounds.size.normalized.x, a_bounds.size.normalized.y);

            float predictionCenter_X = (float)((quadWidth * centerX) - (quadWidth * 0.5f));
            float predictionCenter_Y = (float)((quadHeight * centerY) - (quadHeight * 0.5f));
            Vector3 predictionCenter = new Vector3( m_quad.transform.position.x + predictionCenter_X, 
                                                    m_quad.transform.position.y + predictionCenter_Y, 
                                                    m_quad.transform.position.z);

            // Use raycast to get the depth
            RaycastHit rayHit;
            GameObject target = new GameObject();
            target.transform.rotation = m_quad.transform.rotation;
            target.transform.position = predictionCenter;
            target.transform.localScale = new Vector3((float)quadWidth, (float)quadHeight, 0.001f);
            if (Physics.Raycast(m_camera.position, predictionCenter, out rayHit, 30.0f))
            {
                target.transform.position = rayHit.point;
            }

            return target.transform;
        }
    }
}