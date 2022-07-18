using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace PaparazziGroundControlStation.Controls
{
    class LedNode : TreeNode
    {
        LedList list;
        public LedNode(string text,LedList ledList)
            : base(text)
        {
            list = ledList;
            ImageIndex = 1;
            SelectedImageIndex = 1;
        }
        public void BlinkLed(){
            ImageIndex = 1;
            SelectedImageIndex = 1;
            System.Timers.Timer timer = new System.Timers.Timer(20);
            timer.Elapsed += (sender, e) => TurnOffLed(sender, e, this);
            timer.Start();
        }

        public static void TurnOffLed(object sender, ElapsedEventArgs e, LedNode node){
            try
            {
                node.list.BeginInvoke((ThreadStart)delegate()
                {
                    node.ImageIndex = 0;
                    node.SelectedImageIndex = 0;
                });
                ((System.Timers.Timer)(sender)).Stop();
                ((System.Timers.Timer)(sender)).Dispose();
            }
            catch (Exception exc)
            {

            }
        }
    }
}
