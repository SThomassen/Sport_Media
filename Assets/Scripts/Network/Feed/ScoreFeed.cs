using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloSports.Network
{
    public class ScoreFeed : RssReader
    {
        [Header("Score Feed")]
        [SerializeField] private Animator m_anim = null;
        [SerializeField] protected int m_delay = 5;

        private DateTime delayDate;
        private DateTime pubDate;

        private int m_previousEvent = -1;
        private int m_index = 1;

        public override void ConsoleRssFeed(List<Item> a_items)
        {
            foreach (Item item in a_items)
                Debug.LogFormat("Score: {0}", item.m_title);
        }

        public override void UpdateRssFeed(List<Item> a_items)
        {
            //base.ParseRssFeed(a_items);

            for (int i = 0; i < a_items.Count; i++)
            {
                // ScoreData score = JsonUtility.FromJson<ScoreData>(a_items[i].m_description);
                
                pubDate = DateTime.Parse(a_items[i].m_pubDate);
                delayDate = pubDate.AddSeconds(m_delay);
                if (m_dateTime >= pubDate)
                    break;

                m_index = i;
            }

            // Debug.LogFormat("item {0}: date: {1} / pub: {2}", m_index, m_dateTime, DateTime.Parse(a_items[m_index].m_pubDate));

            if (m_previousEvent != m_index && (m_dateTime >= pubDate && m_dateTime <= delayDate))
            {
                m_anim.SetTrigger("fade");
                m_previousEvent = m_index;
            }
        }
    }

    [System.Serializable]
    public class ScoreData
    {
        public string team;
        public string player;
        public int score;
    }
}