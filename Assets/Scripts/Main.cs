using MixedRealityToolkit.InputModule.EventData;
using MixedRealityToolkit.InputModule.InputHandlers;
using MixedRealityToolkit.UX.Progress;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;

namespace HoloSports
{
    public class Main : MonoBehaviour
    {
        // Anchor Data
        [SerializeField] private float maxDist = 10.0f;
        [SerializeField] private GameObject m_anchor = null;
        private WorldAnchorStore m_store = null;
        private WorldAnchor m_worldAnchor = null;
        private Renderer m_render = null;

        // References
        [SerializeField] private ProgressIndicator m_indicator = null;
        [SerializeField] private Calibration.CalibrationManager m_calibration = null;
        [SerializeField] private Widget.WidgetManager m_widgets = null;

        private const string m_path = "anchor";

        public WorldAnchor GetAnchor()
        {
            return m_worldAnchor;
        }

        // Use this for initialization
        void Start()
        {
            m_render = m_anchor.GetComponent<Renderer>();
            m_render.enabled = false;

            m_indicator.RunIndicator();
            StartCoroutine(Initiate());
        }

        private IEnumerator Initiate()
        {
            ProgressIndicator.Instance.Open("Load World Anchor");

            WorldAnchorStore.GetAsync(LoadAnchorStore);
#if !UNITY_EDITOR
            yield return new WaitUntil(() => m_store != null);
#else
            yield return new WaitForSeconds(2);
#endif

            ProgressIndicator.Instance.Close();
            
            if (LoadAnchor())
            {
                m_anchor.GetComponent<Renderer>().enabled = true;

                float dist = Vector3.Distance(Camera.main.transform.position, m_anchor.transform.position);

                Debug.LogFormat("Dist: {0}", dist);
                if (dist > maxDist)
                {
                    Debug.Log("Run Calibration");
                    m_anchor.GetComponent<Renderer>().enabled = false;
                    Destroy(m_anchor.GetComponent<WorldAnchor>());
                    m_calibration.StartCalibration();
                }
            }
            else
            {
                //First Calibration
                Debug.Log("Run Calibration");
                m_anchor.GetComponent<Renderer>().enabled = false;
                Destroy(m_anchor.GetComponent<WorldAnchor>());
                m_calibration.StartCalibration();
            }
        }

#region World Anchor
        private void LoadAnchorStore(WorldAnchorStore a_store)
        {
            Debug.LogFormat("World Anchor Store load successful");
            m_store = a_store;
        }

        private bool LoadAnchor()
        {
            Debug.Log("File Exists");

            string json = PlayerPrefs.GetString(m_path);
            AnchorData anchorData = JsonUtility.FromJson<AnchorData>(json);

            Debug.LogFormat("Loaded: {0}", json);

            if (anchorData == null)
            {
                Debug.LogWarning("No anchor has been loaded.");
                return false;
            }

            m_anchor.transform.position = anchorData.m_position;
            m_anchor.transform.rotation = anchorData.m_rotation;
            m_anchor.transform.localScale = anchorData.m_scale;

            if (m_store == null) return false;
            m_worldAnchor = m_store.Load(m_path, m_anchor);
            if (m_worldAnchor == null)
            {
                Debug.LogWarning("No anchor has been loaded.");
                return false;
            }
            Debug.Log("Anchor has been loaded.");
            return true;
        }

        public void SaveAnchor(GameObject a_anchor)
        {
            if (a_anchor == null) return;
            m_worldAnchor = a_anchor.GetComponent<WorldAnchor>();
            if (m_worldAnchor == null) return;

            //Save Anchor Transform
            AnchorData anchorData = new AnchorData(a_anchor.transform);
            string json = JsonUtility.ToJson(anchorData);
            PlayerPrefs.SetString(m_path, json);

            Debug.LogFormat("Saved: {0}", json);

            if (m_store == null) return;
            //Save WorldAnchor
            m_store.Clear();
            m_store.Delete("anchor");
            if (!m_store.Save("anchor", m_worldAnchor))
            {
                Debug.LogError("No anchor has been saved.");
            }
        }

        public void RemoveAnchor()
        {
            Debug.Log("Remove Anchor");
            PlayerPrefs.DeleteKey(m_path);
            StartCalibration();
        }

        public void InitiateAnchor(Transform a_transform)
        {
            if (m_anchor == null)
            {
                Debug.Log("m_anchor null error");
                return;
            }

            if (a_transform == null)
            {
                Debug.Log("a_transform null error");
                return;
            }

            m_render.enabled = true;

            m_anchor.transform.position = a_transform.position;
            m_anchor.transform.rotation = a_transform.rotation;
            m_anchor.transform.localScale = a_transform.localScale;
        }
#endregion

#region Button Events
        public void StartCalibration()
        {
            m_render.enabled = false;
            m_calibration.StartCalibration();
        }

        public void StartWidgets()
        {
            m_worldAnchor = m_anchor.AddComponent<WorldAnchor>();
            SaveAnchor(m_anchor);
            m_render.enabled = false;

            m_widgets.transform.position = m_anchor.transform.position;
            m_widgets.transform.rotation = m_anchor.transform.rotation;
            m_widgets.StartWidgets();
        }
        #endregion
    }

    [System.Serializable]
    public class AnchorData
    {
        public AnchorData(Transform a_transform)
        {
            m_position = a_transform.position;
            m_rotation = a_transform.rotation;
            m_scale = a_transform.localScale;
        }

        public Vector3 m_position;
        public Vector3 m_scale;
        public Quaternion m_rotation;
    }
}