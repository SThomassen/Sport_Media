using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloSports.Widget
{

    public class WidgetManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] m_widgets = null;
        [SerializeField] private int m_interval = 1;

        private const string m_path = "widgets";
        private WidgetSave m_save = new WidgetSave();

        public void StartWidgets()
        {
            foreach (GameObject widget in m_widgets)
            {
                widget.SetActive(true);
            }

            LoadWidgets();
            InvokeRepeating("SaveWidgets", 0, m_interval);
        }

        private void LoadWidgets()
        {
            string json = PlayerPrefs.GetString(m_path);
            m_save = JsonUtility.FromJson<WidgetSave>(json);

            if (m_save == null || m_save.m_widgets == null)
            {
                Debug.Log("No Widget save found");
                return;
            }

            foreach (WidgetData widget in m_save.m_widgets)
            {
                Debug.LogFormat("widget {0}: {1}", widget.m_index, widget.m_position);
                m_widgets[widget.m_index].transform.localPosition = widget.m_position;
                m_widgets[widget.m_index].transform.localRotation = widget.m_rotation;
                m_widgets[widget.m_index].transform.localScale = widget.m_scale;
            }
       }

        private void SaveWidgets()
        {
            if (m_save == null) m_save = new WidgetSave();

            m_save.m_widgets.Clear();
            for (int i = 0; i < m_widgets.Length; i++)
            {
                WidgetData widget = new WidgetData(m_widgets[i].transform, i);
                m_save.m_widgets.Add(widget);
            }
            string json = JsonUtility.ToJson(m_save);
            PlayerPrefs.SetString(m_path, json);

            Debug.Log(json);
        }
    }

    [System.Serializable]
    public class WidgetSave
    {
        public List<WidgetData> m_widgets = new List<WidgetData>();
    }

    [System.Serializable]
    public class WidgetData
    {
        public int m_index;

        public Vector3 m_position;
        public Vector3 m_scale;
        public Quaternion m_rotation;

        public WidgetData(Transform a_gameObject, int a_index)
        {
            m_index = a_index;

            m_position = a_gameObject.transform.localPosition;
            m_scale = a_gameObject.transform.localScale;
            m_rotation = a_gameObject.transform.localRotation;
        }
    }
}