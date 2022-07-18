using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Diagnostics;

namespace PaparazziGroundControlStation
{
    class Recorder
    {
        public BinaryWriter StreamOut;
        public BinaryReader StreamIn;
        public System.Timers.Timer ReadTimeLineTimer;
        public Action<byte, byte[]> PlaybackMessageCB;
        public Action PlaybackFinish;
        public long StartTime;
        public Semaphore WriterSem;
        public Semaphore ReadSem;
        public Stopwatch CurrentPosTime;
        public int CurrentPosTimeRelativity;
        public ulong LogLengthMs;
        public Recorder()
        {
            ReadTimeLineTimer = new System.Timers.Timer();
            WriterSem = new Semaphore(1, 1);
            ReadSem = new Semaphore(1, 1);
            CurrentPosTime = new Stopwatch();
            ReadTimeLineTimer.Elapsed += (sender, e) => ReadTimeLine(sender, e, this);
        }
        public long GetCurrentPosTime()
        {
            return CurrentPosTime.ElapsedMilliseconds;
        }
        public void StartRecording(BinaryWriter StreamOut)
        {
            DateTime CurrentTime = DateTime.Now;
            StartTime = CurrentTime.Ticks / TimeSpan.TicksPerMillisecond;
            this.StreamOut = StreamOut;
            CurrentPosTimeRelativity = 0;
            CurrentPosTime.Reset();
            CurrentPosTime.Start();
        }
        public void StopRecording()
        {
            WriterSem.WaitOne();
            StreamOut.Close();
            WriterSem.Release();
            CurrentPosTime.Stop();
        }
        public void AddNewData(byte cmd, byte[] data)
        {
            WriterSem.WaitOne();
            long DiffTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - StartTime;
            byte[] buff = new byte[data.Length + 8 + 3];
            BitConverter.GetBytes(DiffTime).CopyTo(buff, 0);
            buff[8] = cmd;
            BitConverter.GetBytes((ushort)(data.Length)).CopyTo(buff, 9);
            data.CopyTo(buff, 11);
            StreamOut.BaseStream.Write(buff, 0, buff.Length);
            StartTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            WriterSem.Release();
        }
        public void StartPlayBack(BinaryReader StreamIn, Action<byte, byte[]> PlaybackMessageCallBack, Action PlayBackFinish)
        {
            PlaybackMessageCB = PlaybackMessageCallBack;
            LogLengthMs = GetLogLength(StreamIn);
            this.PlaybackFinish = PlayBackFinish;
            this.StreamIn = StreamIn;
            ReadTimeLine(null, null, this);
            ReadTimeLineTimer.Start();
            CurrentPosTimeRelativity = 0;
            CurrentPosTime.Reset();
            CurrentPosTime.Start();
        }

        public void ConvertToCSV(BinaryReader BR, StreamWriter BW, MessagesParser parser)
        {
            String[] LastValues;
            int CntFields = 0;
            List<byte> CommandsList = new List<byte>();
            while (true)
            {
                byte[] header = new byte[11];
                BR.BaseStream.Read(header, 0, 11);
                ushort len = BitConverter.ToUInt16(header, 9);

                if (len > 0 && !CommandsList.Exists(e => e==header[8]))
                    CommandsList.Add(header[8]);
                BR.BaseStream.Seek(BR.BaseStream.Position + len, SeekOrigin.Begin);
                if (BR.BaseStream.Position == BR.BaseStream.Length)
                    break;
            };

            LastValues = new String[CommandsList.Count];
            BR.BaseStream.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < CommandsList.Count; i++)
            {
                string[] fieldsNames = parser.msgClassFieldsName[CommandsList[i]].Split('|');
                for (int j = 0; j < fieldsNames.Length - 1; j++)
                {
                    CntFields++;
                    BW.Write("\"" + parser.msgClassNames[CommandsList[i]] + "." + fieldsNames[j] + "\",");
                }
            }
            LastValues = new String[CntFields];
            BW.Write("\r\n");
            while (true)
            {
                byte[] header = new byte[11];
                BR.BaseStream.Read(header, 0, 11);
                ushort len = BitConverter.ToUInt16(header, 9);
                byte c = header[8];
                byte[] buffer = new byte[len];
                BR.BaseStream.Read(buffer, 0, len);

                if (BR.BaseStream.Position == BR.BaseStream.Length)
                    break;

                string fieldsValue = "";

                parser.MessageArrivedLog(c, buffer.ToList(), ref fieldsValue);
                string[] Values = fieldsValue.Split('|');
                int fieldNamesCnt=0;
                CntFields = 0;
                for (int i = 0; i < CommandsList.Count; i++)
                {
                    fieldNamesCnt = parser.msgClassFieldsName[CommandsList[i]].Split('|').Length;
                    for (int j = 0; j < fieldNamesCnt - 1; j++)
                    {
                        if (CommandsList[i] == c)
                            LastValues[CntFields] = Values[j];
                        if (LastValues[CntFields] != null)
                            BW.Write("\"" + LastValues[CntFields] + "\",");
                        else
                            BW.Write("\"0\",");
                        CntFields++;
                    }
                }
                BW.Write("\r\n");

            };
            BR.BaseStream.Seek(0, SeekOrigin.Begin);
        }
        public void StopPlayBack()
        {
            ReadSem.WaitOne();
            ReadTimeLineTimer.Stop();
            ReadSem.Release();
            CurrentPosTime.Stop();
        }

        public ulong GetLogLength(BinaryReader stream)
        {
            ulong LogLenght = 0;
            while (true)
            {
                byte[] header = new byte[11];
                stream.BaseStream.Read(header, 0, 11);
                ushort len = BitConverter.ToUInt16(header, 9);
                stream.BaseStream.Seek(stream.BaseStream.Position + len, SeekOrigin.Begin);
                if (stream.BaseStream.Position == stream.BaseStream.Length)
                    break;
                stream.BaseStream.Read(header, 0, 8);
                stream.BaseStream.Seek(stream.BaseStream.Position - 8, SeekOrigin.Begin);
                LogLenght += BitConverter.ToUInt64(header, 0);
            };
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
            return LogLenght;
        }
        public void SetSeekBar(BinaryReader stream, ulong time)
        {
            ulong LogLength = 0;
            ReadSem.WaitOne();
            ReadTimeLineTimer.Stop();
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
            while (true)
            {
                byte[] header = new byte[11];
                if (stream.BaseStream.Position == stream.BaseStream.Length)
                {
                    ReadSem.Release();
                    return;
                }
                stream.BaseStream.Read(header, 0, 11);
                ushort len = BitConverter.ToUInt16(header, 9);
                stream.BaseStream.Seek(stream.BaseStream.Position + len, SeekOrigin.Begin);
                if (stream.BaseStream.Position == stream.BaseStream.Length)
                {
                    ReadSem.Release();
                    return;
                }
                stream.BaseStream.Read(header, 0, 8);
                stream.BaseStream.Seek(stream.BaseStream.Position - 8, SeekOrigin.Begin);

                LogLength += BitConverter.ToUInt64(header, 0);
                if (LogLength >= time)
                {
                    ReadSem.Release();
                    CurrentPosTimeRelativity = (int)LogLength;
                    ReadTimeLine(null, null, this);
                    return;
                }
            };
        }
        public void ReadTimeLine(object sender, ElapsedEventArgs e, Recorder RecorderObj)
        {
            RecorderObj.ReadTimeLineTimer.Stop();
            ReadSem.WaitOne();
            if (RecorderObj.StreamIn.BaseStream.Position == RecorderObj.StreamIn.BaseStream.Length)
            {
                RecorderObj.PlaybackFinish();
                ReadSem.Release();
                CurrentPosTime.Stop();
                return;
            }

            byte[] header = new byte[11];
            RecorderObj.StreamIn.BaseStream.Read(header, 0, 11);
            ushort len = BitConverter.ToUInt16(header, 9);
            byte[] buffer = new byte[len];
            RecorderObj.StreamIn.BaseStream.Read(buffer, 0, len);
            PlaybackMessageCB(header[8], buffer);
            if (RecorderObj.StreamIn.BaseStream.Position == RecorderObj.StreamIn.BaseStream.Length)
            {
                RecorderObj.PlaybackFinish();
                ReadSem.Release();
                CurrentPosTime.Stop();
                return;
            }
            RecorderObj.StreamIn.BaseStream.Read(header, 0, 8);
            RecorderObj.StreamIn.BaseStream.Seek(RecorderObj.StreamIn.BaseStream.Position - 8, SeekOrigin.Begin);
            long diffTime = BitConverter.ToInt64(header, 0);
            CurrentPosTimeRelativity += (int)diffTime;
            if (diffTime == 0)
            {
                ReadSem.Release();
                ReadTimeLine(null, null, RecorderObj);
            }
            else
            {
                ReadTimeLineTimer.Interval = diffTime;
                RecorderObj.ReadTimeLineTimer.Start();
                ReadSem.Release();
            }
        }
    }
}
