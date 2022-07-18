using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PaparazziGroundControlStation
{
    public partial class InputBox : Form
    {
        public static String InputText = "";
        private InputBox(string title, string promptText, string defaultVal)
        {
            InitializeComponent();
            info.Text = promptText;
            textBox_value.Text = defaultVal;
            Text = title;
        }
        public static DialogResult Show(string title, string promptText, string defaultVal, ref string value)
        {
            DialogResult answer = DialogResult.Cancel;
            InputBox IB = new InputBox(title, promptText, defaultVal);
            IB.ShowDialog();
            value = InputText;
            answer = IB.DialogResult;
            return answer;
        }

        private void textBox_value_TextChanged(object sender, EventArgs e)
        {
            InputText = textBox_value.Text;
        }

        private void button_ok_Click(object sender, EventArgs e)
        {

        }
    }
}
