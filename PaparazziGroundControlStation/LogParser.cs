using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;

namespace PaparazziGroundControlStation
{
    public partial class LogParser : Form
    {
        public UAVLogAnalyse uavLog = null;
        public Timer loadingTimer;
        public bool treeViewLoaded;
        public string current_node = "";
        public Random randomColor = new Random();
        public Hashtable curvesList;
        public void SetUavLog(UAVLogAnalyse log)
        {
            uavLog = log;
        }

        public void LoadData()
        {
            treeViewLoaded = false;
            foreach (uint i in uavLog.cmdList.Keys)
            {
                string[] fields = uavLog.fieldsList[i].Split('|');
                TreeNode tn = new TreeNode(uavLog.cmdList[i]);
                for (int j = 0; j < fields.Length - 1; j++)
                {
                    TreeNode childTn = new TreeNode(fields[j]);
                    tn.Nodes.Add(childTn);
                    childTn.BackColor = Color.FromArgb(255, randomColor.Next(50, 0x100), randomColor.Next(50, 0x100), randomColor.Next(50, 0x100));
                }

                MessagesList.Nodes.Add(tn);
            }
            treeViewLoaded = true;
        }
        private LineItem AddGraph(string name, Color color, List<double> values, List<uint> times)
        {
            if (times == null || values == null)
                return null;
            // get a reference to the GraphPane
            GraphPane myPane = zedGraphControl.GraphPane;
            // Set the Titles
            myPane.Title.Text = name;
            myPane.YAxis.Title.Text = name;

            // Make up some data arrays based on the Sine function
            double x, y;
            PointPairList list = new PointPairList();
            for (int i = 0; i < times.Count; i++)
            {
                x = times[i];
                y = values[i];
                list.Add(x, y);
            }


            LineItem myCurve = myPane.AddCurve(name,
                  list, color, SymbolType.None);
            
            myPane.XAxis.Type = AxisType.Linear;

            zedGraphControl.AxisChange();

            zedGraphControl.Refresh();
            zedGraphControl.Validate();
            return myCurve;
        }

        private void removeGraph(LineItem curve)
        {
            if (curve == null)
                return;
            GraphPane myPane = zedGraphControl.GraphPane;
            
            myPane.CurveList.Remove(curve);

            zedGraphControl.AxisChange();

            zedGraphControl.Refresh();
            zedGraphControl.Validate();
            return;
        }

        public LogParser()
        {
            InitializeComponent();
            curvesList = new Hashtable();
            // get a reference to the GraphPane
            GraphPane myPane = zedGraphControl.GraphPane;
            zedGraphControl.GraphPane.CurveList.Clear();
            zedGraphControl.GraphPane.GraphObjList.Clear();
            myPane.Title.Text = "UAV Log";
            myPane.XAxis.Title.Text = "Time(ms) ";
            myPane.YAxis.Scale.MinAuto = true;
            myPane.YAxis.Scale.MaxAuto = true;
            myPane.XAxis.Scale.MinAuto = true;
            myPane.XAxis.Scale.MaxAuto = true;
            myPane.YAxis.Title.Text = "";
            myPane.YAxis.Scale.MinAuto = true;
            myPane.YAxis.Scale.MaxAuto = true;
            
            myPane.YAxis.Scale.FontSpec.FontColor = Color.White;
            myPane.YAxis.Title.FontSpec.FontColor = Color.White;
            myPane.XAxis.Scale.FontSpec.FontColor = Color.White;
            myPane.XAxis.Title.FontSpec.FontColor = Color.White;
            myPane.Title.FontSpec.FontColor = Color.White;
            myPane.XAxis.Scale.MinAuto = true;
            myPane.XAxis.Scale.MaxAuto = true;
            myPane.Chart.Fill.Brush = Brushes.Black;
            myPane.Chart.Border.Color = Color.White;
            myPane.Fill.Color = Color.Black;
            zedGraphControl.Validate();
            loadingTimer = new Timer();
            loadingTimer.Tick += loadingTimer_Tick;
            loadingTimer.Interval = 100;
            loadingTimer.Start();
            
        }

        void loadingTimer_Tick(object sender, EventArgs e)
        {
            if (!uavLog.getStatus())
            {
                loadingTimer.Stop();
                LoadData();
                statusStrip.Visible = false;
            }
            else
            {
                toolStripProgressBar.Value = (int)(uavLog.getProgressAdvanced() * 100.0f);
                toolStripStatusLabel.Text = String.Format("{0:0.00} %", uavLog.getProgressAdvanced() * 100.0f);
            }
        }

        private void MessagesList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
        }

        private void LogParser_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = statusStrip.Visible;
        }

        private void MessagesList_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                bool allNodesChecked = true;
                foreach (TreeNode tn in e.Node.Parent.Nodes)
                {
                    if (!tn.Checked)
                    {
                        allNodesChecked = false;
                        break;
                    }

                }
                e.Node.Parent.Checked = allNodesChecked;

                
                List<double> values = ((List<double>)(uavLog.fieldsValues[e.Node.Parent.Text + "_" + e.Node.Text]));
                List<uint> times = ((List<uint>)(uavLog.fieldsTimeValues[e.Node.Parent.Text + "_" + e.Node.Text]));

                if (e.Node.Checked)
                {
                    LineItem curve = AddGraph(e.Node.Text, e.Node.BackColor, values, times);
                    if (curve != null)
                    {
                        if (curvesList.ContainsKey(e.Node.Parent.Text + "_" + e.Node.Text))
                            removeGraph((LineItem)curvesList[e.Node.Parent.Text + "_" + e.Node.Text]);
                        else
                            curvesList.Add(e.Node.Parent.Text + "_" + e.Node.Text, curve);
                    }
                }
                else
                {
                    if (curvesList.ContainsKey(e.Node.Parent.Text + "_" + e.Node.Text))
                    {
                        removeGraph((LineItem)curvesList[e.Node.Parent.Text + "_" + e.Node.Text]);
                        curvesList.Remove(e.Node.Parent.Text + "_" + e.Node.Text);
                    }
                }
            }
        }
    }
}
