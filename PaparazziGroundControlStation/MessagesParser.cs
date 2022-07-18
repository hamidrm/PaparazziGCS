using PaparazziGroundControlStation.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using ZedGraph;

namespace PaparazziGroundControlStation
{
    public class MessagesParser
    {
        public static List<TreeNode> ledOnList;
        public Thread tuningGrapThread;
        int offset;
        bool tuningLoop;
        static LedList targetObject;
        public CheckBox tuningCheckBox;
        public ZedGraphControl tuningGraph;
        public TreeView tuningTree;
        static Semaphore ledListSem;
        XmlDocument xmlDoc = new XmlDocument();
        public Dictionary<byte, string> msgClassNames;
        public Dictionary<byte, string> msgClassFieldsName;
        public Dictionary<byte, string> msgClassFieldsType;
        Dictionary<byte, string> msgClassFieldsUnit;
        Dictionary<byte, string> msgClassFieldsValue;
        Semaphore tuningGraphSem;
        Hashtable tuningData;
        UInt32 totalTuningTime = 150;


        public MessagesParser(string messageXml, LedList target, CheckBox tuningEnableCheckBox, ZedGraphControl tuningZedGraph,TreeView tuningTreeView)
        {
            ledOnList = new List<TreeNode>();
            ledListSem = new Semaphore(1, 100);
            offset = 0;
            targetObject = target;
            tuningCheckBox = tuningEnableCheckBox;
            tuningGraph = tuningZedGraph;
            tuningTree = tuningTreeView;
            msgClassNames = new Dictionary<byte, string>();
            msgClassFieldsName = new Dictionary<byte, string>();
            msgClassFieldsType = new Dictionary<byte, string>();
            msgClassFieldsUnit = new Dictionary<byte, string>();
            msgClassFieldsValue = new Dictionary<byte, string>();
            tuningData = new Hashtable();
            tuningLoop = true;
            tuningGraphSem = new Semaphore(0, 1);
            tuningGrapThread = new Thread(_ => TaskTuning(this));
            tuningGrapThread.Start();
            /* Set TuningGraph style */
            GraphPane myPane = tuningGraph.GraphPane;
            myPane.CurveList.Clear();
            myPane.Title.Text = "Tuning";
            myPane.XAxis.Title.Text = "Samples";
            myPane.YAxis.Title.Text = "";
            myPane.YAxis.Scale.MinAuto = true;
            myPane.YAxis.Scale.MaxAuto = true;
            myPane.YAxis.Scale.FontSpec.FontColor = Color.White;
            myPane.YAxis.Title.FontSpec.FontColor = Color.White;
            myPane.XAxis.Scale.FontSpec.FontColor = Color.White;
            myPane.XAxis.Title.FontSpec.FontColor = Color.White;
            myPane.Title.FontSpec.FontColor = Color.White;
            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = totalTuningTime;
            myPane.Chart.Fill.Brush = Brushes.Black;
            myPane.Chart.Border.Color = Color.White;
            myPane.Fill.Color = Color.Black;
            myPane.Legend.IsVisible = false;

            if (!File.Exists(messageXml))
            {
                MessageBox.Show("Messages.XML not found!");
                return;
            }
            xmlDoc.XmlResolver = null;
            xmlDoc.Load(messageXml);

            XmlNode msgClassElem = xmlDoc.SelectSingleNode("protocol");

            foreach (XmlNode msgClasses in msgClassElem.ChildNodes)
            {
                if (msgClasses.NodeType != XmlNodeType.Comment && msgClasses.Attributes["name"] != null)
                    if (msgClasses.Attributes["name"].Value == "telemetry")
                    {
                        // Telemetry messages found
                        foreach (XmlNode msgClass in msgClasses.ChildNodes)
                        {
                            if (msgClass.Name == "message")
                            {
                                byte id = Convert.ToByte(msgClass.Attributes["id"].Value);
                                msgClassNames.Add(id, msgClass.Attributes["name"].Value);
                                string fieldsName = "";
                                string fieldsType = "";
                                string fieldsUnit = "";
                                foreach (XmlNode classFields in msgClass.ChildNodes)
                                {
                                    if (classFields.Name == "field")
                                    {
                                        if (classFields.Attributes["name"] != null)
                                            fieldsName += classFields.Attributes["name"].Value;
                                        fieldsName += "|";

                                        if (classFields.Attributes["type"] != null)
                                            fieldsType += classFields.Attributes["type"].Value;
                                        fieldsType += "|";

                                        if (classFields.Attributes["unit"] != null)
                                            fieldsUnit += classFields.Attributes["unit"].Value;
                                        fieldsUnit += "|";
                                    }
                                }
                                msgClassFieldsName.Add(id, fieldsName);
                                msgClassFieldsType.Add(id, fieldsType);
                                msgClassFieldsUnit.Add(id, fieldsUnit);
                                msgClassFieldsValue.Add(id, "");
                            }
                        }
                        break;
                    }
            }
        }

        public void killTuningThread()
        {
            tuningLoop = false;
            try
            {
                tuningGraphSem.Release();
            }
            catch(SemaphoreFullException e){

            }
            while (tuningGrapThread.IsAlive) ;
        }
        private static void TaskTuning(MessagesParser msgParser)
        {
            while (msgParser.tuningLoop)
            {
                msgParser.tuningGraphSem.WaitOne();
                msgParser.tuningGraph.BeginInvoke((Action)(() =>
                {
                    msgParser.UpdateTuningGraph();
                }));
                Thread.Sleep(10);
            }
        }

        private float GetFloat(byte[] rv)
        {
            float r = System.BitConverter.ToSingle(rv, offset);
            offset += 4;
            return r;
        }
        private double GetDouble(byte[] rv)
        {
            double r = System.BitConverter.ToDouble(rv, offset);
            offset += 8;
            return r;
        }
        private int GetInt32(byte[] rv)
        {
            int r = System.BitConverter.ToInt32(rv, offset);
            offset += 4;
            return r;
        }
        private short GetInt16(byte[] rv)
        {
            short r = System.BitConverter.ToInt16(rv, offset);
            offset += 2;
            return r;
        }
        private uint GetUInt32(byte[] rv)
        {
            uint r = System.BitConverter.ToUInt32(rv, offset);
            offset += 4;
            return r;
        }
        private ushort GetUInt16(byte[] rv)
        {
            ushort r = System.BitConverter.ToUInt16(rv, offset);
            offset += 2;
            return r;
        }
        public void MessageArrived(byte cmd, List<byte> data)
        {
            
            string[] Names = msgClassFieldsName[cmd].Split('|');
            string[] Types = msgClassFieldsType[cmd].Split('|');
            string[] Unites = msgClassFieldsUnit[cmd].Split('|');
            Random rand = new Random(DateTime.Now.Millisecond);
            offset = 0;
            msgClassFieldsValue[cmd] = "";
            Names = Names.Take(Names.Length - 1).ToArray();
            Types = Types.Take(Types.Length - 1).ToArray();
            Unites = Unites.Take(Unites.Length - 1).ToArray();
            LedNode node = new LedNode(msgClassNames[cmd], targetObject);
            node.Name = msgClassNames[cmd];
            if (!tuningTree.Nodes.ContainsKey(msgClassNames[cmd]))
            {
                TreeNode tn = tuningTree.Nodes.Add(msgClassNames[cmd]);
                tn.Name = msgClassNames[cmd];
                foreach(string field in Names){
                    TreeNode childTn;
                    
                    childTn = tn.Nodes.Add(field);
                    childTn.BackColor = Color.FromArgb(rand.Next(60, 255), rand.Next(60, 255), rand.Next(60, 255));
                }
            }
            if (!targetObject.Nodes.ContainsKey(msgClassNames[cmd]))
            {
                targetObject.Nodes.Add(node);
            }
            else
            {
                node = (LedNode)targetObject.Nodes.Find(msgClassNames[cmd], false)[0];
            }
            //Turn on LED
            node.BlinkLed();
            node.Nodes.Clear();
            for (int i = 0; i < Types.Length; i++)
            {
                TreeNode nodeChild = new TreeNode();
                nodeChild.ImageIndex = 2;
                nodeChild.SelectedImageIndex = 2;
                string value = "";
                switch (Types[i])
                {
                    case "int8":
                        value = ((SByte)data[offset]).ToString();
                        offset++;
                        nodeChild.Text = Names[i] + " = " + value + " " + Unites[i];
                        msgClassFieldsValue[cmd] += value + "|";
                        break;
                    case "uint8":
                        value = ((Byte)data[offset]).ToString();
                        offset++;
                        nodeChild.Text = Names[i] + " = " + value + " " + Unites[i];
                        msgClassFieldsValue[cmd] += value + "|";
                        break;
                    case "int16":
                        value = GetInt16(data.ToArray()).ToString();
                        nodeChild.Text = Names[i] + " = " + value + " " + Unites[i];
                        msgClassFieldsValue[cmd] += value + "|";
                        break;
                    case "uint16":
                        value = GetUInt16(data.ToArray()).ToString();
                        nodeChild.Text = Names[i] + " = " + value + " " + Unites[i];
                        msgClassFieldsValue[cmd] += value + "|";
                        break;
                    case "int32":
                        value = GetInt32(data.ToArray()).ToString();
                        nodeChild.Text = Names[i] + " = " + value + " " + Unites[i];
                        msgClassFieldsValue[cmd] += value + "|";
                        break;
                    case "uint32":
                        value = GetUInt32(data.ToArray()).ToString();
                        nodeChild.Text = Names[i] + " = " + value + " " + Unites[i];
                        msgClassFieldsValue[cmd] += value + "|";
                        break;
                    case "float":
                        value = GetFloat(data.ToArray()).ToString();
                        nodeChild.Text = Names[i] + " = " + value + " " + Unites[i];
                        msgClassFieldsValue[cmd] += value + "|";
                        break;
                    case "double":
                        value = GetDouble(data.ToArray()).ToString();
                        nodeChild.Text = Names[i] + " = " + value + " " + Unites[i];
                        msgClassFieldsValue[cmd] += value + "|";
                        break;
                    case "uint8[]":
                        {
                            byte len = data[offset++];
                            string arrayText = "[ ";
                            for (int j = 0; j < len; j++)
                                arrayText += data[j + offset] + (j == len - 1 ? "" : " , ");
                            arrayText += " ]";
                            msgClassFieldsValue[cmd] += arrayText + "|";
                            nodeChild.Text = Names[i] + " = " + arrayText + " " + Unites[i];
                            offset += len * 2;
                        }
                        break;
                    case "int8[]":
                        {
                            byte len = data[offset++];
                            string arrayText = "[ ";
                            for (int j = 0; j < len; j++)
                                arrayText += (SByte)data[j + offset] + (j == len - 1 ? "" : " , ");
                            arrayText += " ]";
                            msgClassFieldsValue[cmd] += arrayText + "|";
                            nodeChild.Text = Names[i] + " = " + arrayText + " " + Unites[i];
                            offset += len * 2;
                        }
                        break;
                    case "uint16[]":
                        {
                            byte len = data[offset++];
                            string arrayText = "[ ";
                            for (int j = 0; j < len; j++)
                                arrayText += (UInt16)((data[j * 2 + offset + 1] * 0x100) + (data[j * 2 + offset])) + (j == len - 1 ? "" : " , ");
                            arrayText += " ]";
                            msgClassFieldsValue[cmd] += arrayText + "|";
                            nodeChild.Text = Names[i] + " = " + arrayText + " " + Unites[i];
                            offset += len * 2;
                        }
                        break;
                    case "int16[]":
                        {
                            byte len = data[offset++];
                            string arrayText = "[ ";
                            for (int j = 0; j < len; j++)
                                arrayText += (Int16)((data[j * 2 + offset + 1] * 0x100) + (data[j * 2 + offset])) + (j == len - 1 ? "" : " , ");
                            arrayText += " ]";
                            msgClassFieldsValue[cmd] += arrayText + "|";
                            nodeChild.Text = Names[i] + " = " + arrayText + " " + Unites[i];
                            offset += len * 2;
                        }
                        break;
                    case "char[]":
                        nodeChild.Text = Names[i] + " = " + Encoding.UTF8.GetString(data.Skip(offset + 1).Take(data[offset]).ToArray()) + " " + Unites[i];
                        msgClassFieldsValue[cmd] += Encoding.UTF8.GetString(data.Skip(offset + 1).Take(data[offset]).ToArray()) + "|";
                        offset += data[offset];
                        break;
                }

                node.Nodes.Add(nodeChild);
            }

            

            if (tuningCheckBox.Checked)
            {
                string[] array = new string[tuningData.Count];
                string[] values = msgClassFieldsValue[cmd].Split('|');
                tuningData.Keys.CopyTo(array, 0);
                values = values.Take(values.Length - 1).ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    bool isCurrentPacket = false;
                    List<double> fieldList = (List<double>)tuningData[array[i]];
                    for (int j = 0; j < Types.Length; j++)
                    {
                        if (msgClassNames[cmd] + "_" + Names[j] == array[i])
                        {
                            isCurrentPacket = true;
                            break;
                        }
                    }
                    if (!isCurrentPacket)
                    {
                        fieldList.Add(fieldList[fieldList.Count - 1]);
                    }

                    if (fieldList.Count > totalTuningTime)
                        fieldList.RemoveAt(0);
                }
                for (int i = 0; i < Types.Length; i++)
                {
                    if (Types[i] == "char[]" || Types[i] == "int16[]" || Types[i] == "uint16[]" || Types[i] == "int8[]" || Types[i] == "uint8[]")
                        continue;

                    if (tuningData.ContainsKey(msgClassNames[cmd] + "_" + Names[i]))
                    {
                        List<double> fieldList = (List<double>)tuningData[msgClassNames[cmd] + "_" + Names[i]];
                        fieldList.Add(Convert.ToDouble(values[i]));
                    }
                    else
                    {
                        tuningData[msgClassNames[cmd] + "_" + Names[i]] = new List<double>();
                        List<double> fieldList = (List<double>)tuningData[msgClassNames[cmd] + "_" + Names[i]];
                        fieldList.Add(Convert.ToDouble(values[i]));
                    }
                }
                try
                {
                    tuningGraphSem.Release();
                }
                catch (SemaphoreFullException e)
                {

                }
                
            }

        }

        public void UpdateTuningGraph()
        {
            GraphPane myPane = tuningGraph.GraphPane;
            myPane.CurveList.Clear();

            double x, y;

            foreach (TreeNode tn in tuningTree.Nodes)
            {
                foreach (TreeNode tnChild in tn.Nodes)
                {
                    if (tnChild.Checked) {
                        string fieldName = tn.Text + "_" + tnChild.Text;
                        PointPairList list = new PointPairList();
                        List<double> fieldList = (List<double>)tuningData[fieldName];
                        if (fieldList == null)
                            continue;
                        for (int i = 0; i < fieldList.Count; i++)
                        {
                            x = i;
                            y = fieldList[i];
                            list.Add(x, y);
                        }

                        LineItem myCurve = myPane.AddCurve(tnChild.Text,
                              list, tnChild.BackColor, SymbolType.None);
                    }
                }
            }

            myPane.XAxis.Type = AxisType.Linear;

            tuningGraph.AxisChange();

            tuningGraph.Refresh();
            tuningGraph.Validate();

        }

        public void MessageArrivedLog(byte cmd, List<byte> data, ref string FiledsValue)
        {
            offset = 0;
            string[] Names = msgClassFieldsName[cmd].Split('|');
            string[] Types = msgClassFieldsType[cmd].Split('|');
            msgClassFieldsValue[cmd] = "";
            Names = Names.Take(Names.Length - 1).ToArray();
            Types = Types.Take(Types.Length - 1).ToArray();

            for (int i = 0; i < Types.Length; i++)
            {

                string value = "";
                switch (Types[i])
                {
                    case "int8":
                        value = ((SByte)data[offset]).ToString();
                        offset++;
                        FiledsValue += value + "|";
                        break;
                    case "uint8":
                        value = ((Byte)data[offset]).ToString();
                        offset++;
                        FiledsValue += value + "|";
                        break;
                    case "int16":
                        value = GetInt16(data.ToArray()).ToString();
                        FiledsValue += value + "|";
                        break;
                    case "uint16":
                        value = GetUInt16(data.ToArray()).ToString();
                        FiledsValue += value + "|";
                        break;
                    case "int32":
                        value = GetInt32(data.ToArray()).ToString();
                        FiledsValue += value + "|";
                        break;
                    case "uint32":
                        value = GetUInt32(data.ToArray()).ToString();
                        FiledsValue += value + "|";
                        break;
                    case "float":
                        value = GetFloat(data.ToArray()).ToString();
                        FiledsValue += value + "|";
                        break;
                    case "double":
                        value = GetDouble(data.ToArray()).ToString();
                        FiledsValue += value + "|";
                        break;
                    case "uint8[]":
                        {
                            byte len = data[offset++];
                            string arrayText = "[ ";
                            for (int j = 0; j < len; j++)
                                arrayText += data[j + offset] + (j == len - 1 ? "" : " , ");
                            arrayText += " ]";
                            FiledsValue += arrayText + "|";
                            offset += len * 2;
                        }
                        break;
                    case "int8[]":
                        {
                            byte len = data[offset++];
                            string arrayText = "[ ";
                            for (int j = 0; j < len; j++)
                                arrayText += (SByte)data[j + offset] + (j == len - 1 ? "" : " , ");
                            arrayText += " ]";
                            FiledsValue += arrayText + "|";
                            offset += len * 2;
                        }
                        break;
                    case "uint16[]":
                        {
                            byte len = data[offset++];
                            string arrayText = "[ ";
                            for (int j = 0; j < len; j++)
                                arrayText += (UInt16)((data[j * 2 + offset + 1] * 0x100) + (data[j * 2 + offset])) + (j == len - 1 ? "" : " , ");
                            arrayText += " ]";
                            FiledsValue += arrayText + "|";
                            offset += len * 2;
                        }
                        break;
                    case "int16[]":
                        {
                            byte len = data[offset++];
                            string arrayText = "[ ";
                            for (int j = 0; j < len; j++)
                                arrayText += (Int16)((data[j * 2 + offset + 1] * 0x100) + (data[j * 2 + offset])) + (j == len - 1 ? "" : " , ");
                            arrayText += " ]";
                            FiledsValue += arrayText + "|";
                            offset += len * 2;
                        }
                        break;
                    case "char[]":
                        FiledsValue += Encoding.UTF8.GetString(data.Skip(offset + 1).Take(data[offset]).ToArray()) + "|";
                        offset += data[offset];
                        break;
                }
            }


        }
    }
}
