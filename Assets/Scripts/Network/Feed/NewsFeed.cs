using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloSports.Network
{

    public class NewsFeed : RssReader
    {
        [Header("News Feed")]
        [SerializeField] private TextMesh m_output = null;

        private DateTime pubDate;

        private int m_index = 1;

        public override void ConsoleRssFeed(List<Item> a_items)
        {  
            foreach (Item item in a_items)
            {
                Debug.LogFormat("Time; {0}", DateTime.Parse(item.m_pubDate));
            }
        }

        public override void UpdateRssFeed(List<Item> a_items)
        {
            //base.ParseRssFeed(a_items);
            for (int i = 0; i < a_items.Count; i++)
            {
                pubDate = DateTime.Parse(a_items[i].m_pubDate);
                if (m_dateTime >= pubDate)
                    break;

                m_index = i;
            }
            m_output.text = a_items[m_index].m_description;
            // Debug.LogFormat("item {0}: {1} / {2}", m_index, m_dateTime, DateTime.Parse(a_items[m_index].m_pubDate));
        }
    }
}