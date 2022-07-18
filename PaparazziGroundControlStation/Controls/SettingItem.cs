using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PaparazziGroundControlStation.Communication;

namespace PaparazziGroundControlStation.Controls
{
    public partial class SettingItem : UserControl
    {
        TrackBar trackBar;
        ComboBox listBox;
        FlowLayoutPanel radiosGroup;
        RadioButton[] radios;
        string SettingTitle;
        SettingType currentSettingType = SettingType.SettingTrackBar;
        byte pprzSettingType;
        string RadioButton1Text;
        string RadioButton2Text;
        double TBMaxValue;
        double TBMinValue;

        double TBSteps;

        float RadioButton1Value;
        float RadioButton2Value;

        private List<String> listBoxCollection = new List<String>(); 
        public enum SettingType
        {
            SettingTrackBar,
            SettingListBox,
            SettingRadioButton,
        };

        public event Action<byte,float> saveClick;
        public event Action<byte> loadClick;
        [Description("Setting Type"), Category("Setting")]
        public SettingType settingType
        {
            get { return currentSettingType; }
            set { 
                  currentSettingType = value;
                  SettingTitle = Setting.GetSettingTitle((int)pprzSettingType) + " :";
                  UpdateUi();
            }
        }
        [Description("Setting Title"), Category("Setting")]
        public string settingTitle
        {
            get { return SettingTitle; }
            set
            {
                SettingTitle = value;
                UpdateUi();
            }
        }
        [Description("Setting Code"), Category("Setting")]
        public byte settingPprz
        {
            get { return pprzSettingType; }
            set
            {
                pprzSettingType = value;
                UpdateUi();
            }
        }
        [Description("Max Value"), Category("Setting->SettingTrackBar")]
        public double MaxValue
        {
            get { 
                return TBMaxValue; 
            }
            set
            {
                TBMaxValue = value;
                trackBar.Maximum = (int)(Math.Pow(10,(double)(GetDecimalsNumber(TBSteps))) * TBMaxValue);
            }
        }
        [Description("Min Value"), Category("Setting->SettingTrackBar")]
        public double MinValue
        {
            get
            {
                return TBMinValue;
            }
            set
            {
                TBMinValue = value;
                trackBar.Minimum = (int)(Math.Pow(10, (double)(GetDecimalsNumber(TBSteps))) * TBMinValue);
            }
        }
        [Description("Steps"), Category("Setting->SettingTrackBar")]
        public double Steps
        {
            get
            {
                return TBSteps;
            }
            set
            {
                TBSteps = value;
                trackBar.Minimum = (int)(Math.Pow(10, (double)(GetDecimalsNumber(TBSteps))) * TBMinValue);
                trackBar.Maximum = (int)(Math.Pow(10, (double)(GetDecimalsNumber(TBSteps))) * TBMaxValue);
            }
        }
        [Description("List Items"), Category("Setting->SettingListBox")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design","System.Drawing.Design.UITypeEditor, System.Drawing")] 
        public List<string> ListItems
        {
            get { return listBoxCollection; }
            set
            {
                listBoxCollection = value;
                UpdateUi();
            }
        }
        [Description("Radio Button 1 text"), Category("Setting->SettingRadioButton")]
        public string RadioButton1
        {
            get
            {
                return RadioButton1Text;
            }
            set
            {
                RadioButton1Text = value;
                UpdateUi();
            }
        }
        [Description("Radio Button 1 value"), Category("Setting->SettingRadioButton")]
        public float RadioButton1V
        {
            get
            {
                return RadioButton1Value;
            }
            set
            {
                RadioButton1Value = value;
                UpdateUi();
            }
        }
        [Description("Radio Button 2 text"), Category("Setting->SettingRadioButton")]
        public string RadioButton2
        {
            get
            {
                return RadioButton2Text;
            }
            set
            {
                RadioButton2Text = value;
                UpdateUi();
            }
        }
        [Description("Radio Button 2 value"), Category("Setting->SettingRadioButton")]
        public float RadioButton2V
        {
            get
            {
                return RadioButton2Value;
            }
            set
            {
                RadioButton2Value = value;
                UpdateUi();
            }
        }
        public SettingItem()
        {
            InitializeComponent();
            trackBar = new TrackBar();
            listBox = new ComboBox();
            radiosGroup = new FlowLayoutPanel();
            radios = new RadioButton[2];
            radios[0] = new RadioButton();
            radios[1] = new RadioButton();
            radios[0].Visible = true;
            radios[1].Visible = true;

            listBox.DropDownStyle = ComboBoxStyle.DropDownList;

            radios[0].Text = "True";
            radios[1].Text = "False";
            radios[0].Checked = true;

            trackBar.ValueChanged += trackBar_ValueChanged;
            trackBar.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            listBox.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            listBox.Height = 32;
            radiosGroup.Anchor = AnchorStyles.Right | AnchorStyles.Left;

            radiosGroup.Controls.AddRange(radios);

     
            listBox.Visible = false;
            radiosGroup.Visible = false;
            trackBar.Visible = false;

            tableLayoutPanel4.Controls.Add(listBox, 1, 0);
            tableLayoutPanel4.Controls.Add(radiosGroup, 1, 0);
            tableLayoutPanel4.Controls.Add(trackBar, 1, 0);
            RadioButton1Value = 0;
            RadioButton2Value = 1;
            UpdateUi();
        }

        void trackBar_ValueChanged(object sender, EventArgs e)
        {
            labelValue.Text = ((float)(trackBar.Value) / (float)Math.Pow(10, (double)(GetDecimalsNumber(TBSteps)))).ToString("0.000");
        }

        private void UpdateUi(){
            if (currentSettingType == SettingType.SettingListBox){
                listBox.Visible = true;
                radiosGroup.Visible = false;
                trackBar.Visible = false;
                labelValue.Visible = false;
            }else if (currentSettingType == SettingType.SettingRadioButton){
                tableLayoutPanel4.Controls.Add(radiosGroup, 1, 0);
                listBox.Visible = false;
                radiosGroup.Visible = true;
                trackBar.Visible = false;
                labelValue.Visible = false;
            }else {
                tableLayoutPanel4.Controls.Add(trackBar, 1, 0);
                listBox.Visible = false;
                radiosGroup.Visible = false;
                trackBar.Visible = true;
                labelValue.Visible = true;
                trackBar.Minimum = (int)(Math.Pow(10, (double)(GetDecimalsNumber(TBSteps))) * TBMinValue);
                trackBar.Maximum = (int)(Math.Pow(10, (double)(GetDecimalsNumber(TBSteps))) * TBMaxValue);
                trackBar.Value = trackBar.Minimum;
                labelValue.Text = ((float)(trackBar.Value) / (float)Math.Pow(10, (double)(GetDecimalsNumber(TBSteps)))).ToString("0.000");
            }

            label.Text = SettingTitle;

            if (listBoxCollection.Count > 0) {
                listBox.Items.Clear();
                foreach (string s in listBoxCollection)
                    listBox.Items.Add(s);

                listBox.SelectedIndex = 0;
            }

            radios[0].Text = RadioButton1Text;
            radios[1].Text = RadioButton2Text;

        }

        private int GetDecimalsNumber(double f)
        {
            int i=0;

            

            f = Convert.ToDouble(f.ToString("0.0000"));

            f = f - (UInt32)(f);


            while ((UInt32)(f) == 0)
            {
                if (f == 0)
                    return i;
                f *= 10;
                f = Math.Round(f,4);
                i++;

            }
            return i;
        }
        public void SetValue(byte index, float value)
        {
            if (index == pprzSettingType)
            {
                switch (currentSettingType)
                {
                    case SettingType.SettingListBox:
                        listBox.SelectedIndex = ((int)value) % listBox.Items.Count;
                        break;
                    case SettingType.SettingRadioButton:
                        radios[0].Checked = (value == RadioButton1Value) ? true : false;
                        radios[1].Checked = (value == RadioButton2Value) ? true : false;
                        break;
                    case SettingType.SettingTrackBar:
                        int trackBarValue = (int)(value * (float)Math.Pow(10, (double)(GetDecimalsNumber(TBSteps))));
                        if (trackBarValue <= trackBar.Maximum && trackBarValue >= trackBar.Minimum)
                            trackBar.Value = trackBarValue;
                        break;
                }
            }
        }
        private void buttonSave_Click(object sender, EventArgs e)
        {
            float value = 0;
            switch (currentSettingType)
            {
                case SettingType.SettingListBox:
                    value = listBox.SelectedIndex;
                    break;
                case SettingType.SettingRadioButton:
                    value = radios[0].Checked ? RadioButton1Value : RadioButton2Value;
                    break;
                case SettingType.SettingTrackBar:
                    value = (float)(trackBar.Value) / (float)Math.Pow(10, (double)(GetDecimalsNumber(TBSteps)));
                    break;
            }
            saveClick((byte)(pprzSettingType), value);
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            loadClick((byte)(pprzSettingType));
        }
    }
}
