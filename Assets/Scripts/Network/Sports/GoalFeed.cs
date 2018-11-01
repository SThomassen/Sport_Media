using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloSports.Network
{
    public class GoalFeed : RssReader
    {
        public override void ConsoleRssFeed(List<Item> a_items)
        {
            foreach (Item item in a_items)
                Debug.LogFormat("Goal: {0}", item.m_title);
        }

        public override void UpdateRssFeed(List<Item> a_items)
        {
            //base.ParseRssFeed(a_items);
            for (int i = 0; i < a_items.Count; i++)
            {
                DateTime pubDate = DateTime.Parse(a_items[i].m_pubDate);
                if (m_date >= pubDate)
                    break;

                m_index = i;
            }
            m_output.text = a_items[m_index].m_description;
            Debug.LogFormat("index: {0} / date: {1} / pub: {2}", m_index, m_date, DateTime.Parse(a_items[m_index].m_pubDate));
        }
    }
}