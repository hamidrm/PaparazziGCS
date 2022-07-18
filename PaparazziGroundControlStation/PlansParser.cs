using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace PaparazziGroundControlStation
{
    class PlansParser
    {
        XmlDocument xmlDoc;
        Dictionary<int, string> Waypoints;
        Dictionary<int, string> Blocks;
        public PlansParser(string xmlFile)
        {
            xmlDoc = new XmlDocument();
            Waypoints = new Dictionary<int, string>();
            Blocks = new Dictionary<int, string>();

            if (!File.Exists(xmlFile))
            {
                MessageBox.Show("Settings.XML not found!");
                return;
            }
            xmlDoc.XmlResolver = null;
            xmlDoc.Load(xmlFile);
            
            XmlNode waypointsElem = xmlDoc.SelectSingleNode("flight_plan/waypoints");
            int i = 0;
            Waypoints.Add(i++, "Dummy");
            foreach (XmlNode waypoint in waypointsElem.ChildNodes)
            {
                Waypoints.Add(i++, waypoint.Attributes["name"].Value);
            }
            XmlNode blocksElem = xmlDoc.SelectSingleNode("flight_plan/blocks");
            i = 0;
            foreach (XmlNode block in blocksElem.ChildNodes)
            {
                Blocks.Add(i++, block.Attributes["name"].Value);
            }
        }
        public string GetBlock(int index)
        {
            return Blocks[index];
        }
        public int GetBlocksCount()
        {
            return Blocks.Count;
        }
        public string GetWaypoint(int index)
        {
            return Waypoints[index];
        }
        public int GetWaypointsCount()
        {
            return Waypoints.Count;
        }
    }
}
