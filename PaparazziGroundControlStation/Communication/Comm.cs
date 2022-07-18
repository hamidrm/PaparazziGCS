using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PaparazziGroundControlStation.Communication
{
    
    class Comm
    {
        public Semaphore sem;
        public List<String> PortsName;
        public delegate void DataReceived(byte data);
        private DataReceived DataReceivedCB ;
        private SerialPort CurrentPort;
        private Thread ReceiveThread;
        public Comm(DataReceived CB)
        {
            PortsName = SerialPort.GetPortNames().ToList<string>();
            CurrentPort = new SerialPort();
            sem = new Semaphore(1, 1);

            DataReceivedCB = new DataReceived(CB);
        }
        public bool IsOpen()
        {
            return CurrentPort.IsOpen;
        }
        public void SendByte(byte [] buffer)
        {
            if (CurrentPort.IsOpen)
                CurrentPort.Write(buffer, 0, buffer.Length);
        }
        public static void ReceivingThread(Object p)
        {
            bool isLive=false;
            int recvByte=0;
            while (((Comm)(p)) != null && ((Comm)(p)).CurrentPort != null && ((Comm)(p)).CurrentPort.IsOpen)
            {
                    
                ((Comm)(p)).sem.WaitOne();
                if (((Comm)(p)) != null && ((Comm)(p)).CurrentPort != null && ((Comm)(p)).CurrentPort.IsOpen){
                    isLive = true;
                    recvByte = ((Comm)(p)).CurrentPort.ReadByte();
                }
                ((Comm)(p)).sem.Release();
                if (isLive)
                    ((Comm)(p)).DataReceivedCB((byte)(recvByte));
            }
        }
        public bool Connect(String PortName,int Baudrate)
        {
            CurrentPort.BaudRate = Baudrate;
            CurrentPort.PortName = PortName;
            if (CurrentPort.IsOpen)
                CurrentPort.Close();
            try
            {
                CurrentPort.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to open device...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                return false;
            }
            if (ReceiveThread == null || !ReceiveThread.IsAlive)
            {
                ReceiveThread = new Thread(new ParameterizedThreadStart(ReceivingThread));
                ReceiveThread.Start(this);
            }
            return true;
        }

        public void DisposeRecvThread()
        {
            sem.WaitOne();
            if (ReceiveThread != null && ReceiveThread.IsAlive)
                ReceiveThread.Abort();
            sem.Release();
        }
        public void Disconnect()
        {
            sem.WaitOne();
            if (CurrentPort.IsOpen)
                CurrentPort.Close();
            sem.Release();
        }
    }
}
