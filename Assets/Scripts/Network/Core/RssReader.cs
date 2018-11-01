using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloSports.Network
{
    [System.Serializable]
    public struct Time
    {
        public int m_year;
        public int m_month;
        public int m_day;
        public int m_hour;
        public int m_minute;
    }

    public class RssReader : MonoBehaviour
    {
        [SerializeField] private string m_baseURL = null;
        [SerializeField] private string m_file = null;

        [Header("Debug Time")]
        [Tooltip("request rss feed interval. 0 = recursive")]
        [SerializeField] private int m_interval = 1;

        [Tooltip("Set time to imitate request. Only works when interval is larget than 0")]
        [SerializeField] protected Time m_time;
        protected DateTime m_dateTime;

        private string m_url;
        private Rss m_rss;

        private void Start()
        {
            m_dateTime = new DateTime(  m_time.m_year, 
                                        m_time.m_month, 
                                        m_time.m_day, 
                                        m_time.m_hour,
                                        m_time.m_minute,0);

            if (m_interval == 0)
            {
                m_dateTime = DateTime.Now;
            }

            Debug.LogFormat("time: {0}", m_dateTime);

            StartCoroutine(CheckConnection());
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            if (m_interval != 0)
            {
                m_dateTime = new DateTime(m_time.m_year,
                                        m_time.m_month,
                                        m_time.m_day,
                                        m_time.m_hour,
                                        m_time.m_minute, 0);
            }
        }
#endif

        private void IncrementTimer()
        {
            if (m_interval == 0)
            {
                m_dateTime = DateTime.Now;
            }
            else
            {
                m_dateTime = m_dateTime.AddMinutes(m_interval);
                m_time.m_minute = m_dateTime.Minute;
                m_time.m_hour = m_dateTime.Hour;
                m_time.m_day = m_dateTime.Day;
                m_time.m_month = m_dateTime.Month;
                m_time.m_year = m_dateTime.Year;
            }
        }

        private IEnumerator CheckConnection()
        {
            string url = string.Format("{0}{1}.xml", m_baseURL,m_file);
            WWW check = new WWW(url);
            yield return check;

            if (check.error != null)
            {
                string local = string.Format("file:///{0}/RSS/{1}.xml", Application.dataPath, m_file);
                check = new WWW(local);
                yield return check;

                if (check.error != null)
                {
                    Debug.LogErrorFormat("URL not Found: {0}", check.error);
                }
                else
                {
                    m_url = local;
                    if (m_interval == 0)
                        StartCoroutine(RequestRSSFeed(m_url));
                    else
                        InvokeRepeating("IntervalUpdate", 0, m_interval);
                }
            }
            else
            {
                m_url = url;
                if (m_interval == 0)
                    StartCoroutine(RequestRSSFeed(m_url));
                else
                    InvokeRepeating("IntervalUpdate", 0, m_interval);
            }
        }

        private void IntervalUpdate()
        {
            StartCoroutine(RequestRSSFeed(m_url));
        }

        private IEnumerator RequestRSSFeed(string a_url)
        {
            using (WWW request = new WWW(a_url))
            {
                yield return request;
                DeserializeXml(request.text);
            }
        }

        private void DeserializeXml(string a_rss)
        {
            if (string.IsNullOrEmpty(a_rss))
            {
                Debug.LogWarning("No RSS Feed has been found.");
                StartCoroutine(RequestRSSFeed(m_url));
                return;
            }

            using (XmlTextReader reader = new XmlTextReader(new StringReader(a_rss)))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Rss));
                m_rss = (Rss)xmlSerializer.Deserialize(reader);

                List<Item> items = m_rss.m_channel.m_items;

                if (m_interval == 0)
                {
                    StartCoroutine(RequestRSSFeed(m_url));
                }
                UpdateRssFeed(items);
                IncrementTimer();
            }
        }

        public virtual void ConsoleRssFeed(List<Item> a_items)
        {
            foreach (Item item in a_items)
            {
                Debug.Log(item.m_title);
            }
        }

        public virtual void UpdateRssFeed(List<Item> a_items)
        {
            
        }
    }

    // RSS xml format
    [Serializable, XmlRoot("rss")]
    public class Rss
    {
        [XmlElement("channel")]
        public Channel m_channel;
    }

    public class Channel
    {
        [XmlElement("title")]
        public string m_title;
        [XmlElement("link")]
        public string m_link;
        [XmlElement("description")]
        public string m_description;
        [XmlElement("item")]
        public List<Item> m_items;
        [XmlElement("pubDate")]
        public string m_pubData;
    }

    public class Item
    {
        [XmlElement("title")]
        public string m_title;
        [XmlElement("category")]
        public string m_category;
        [XmlElement("description")]
        public string m_description;
        [XmlElement("link")]
        public string m_link;
        [XmlElement("pubDate")]
        public string m_pubDate;
    }
}