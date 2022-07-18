using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;

namespace PaparazziGroundControlStation
{
    public class UAVLogAnalyse
    {
        public Dictionary<uint, string> cmdList = new Dictionary<uint,string>();
        public Dictionary<uint, string> fieldsList = new Dictionary<uint, string>();
        public Hashtable fieldsValues = new Hashtable();
        public Hashtable fieldsTimeValues = new Hashtable();
        MessagesParser messages;
        float progressValue;
        bool isLoading;
        public UAVLogAnalyse(MessagesParser msg)
        {
            messages = msg;
            isLoading = false;
            progressValue = 0;
        }
        public void LoadFile(string fileName)
        {
            isLoading = true;
            Thread logLoader = new Thread(() => ThreadLoadFile(fileName, this));
            logLoader.Start();
        }
        public bool getStatus(){
            return isLoading;
        }
        public float getProgressAdvanced()
        {
            return progressValue;
        }
        public static void ThreadLoadFile(string fileName,UAVLogAnalyse log)
        {
            uint currentByte=0;
            uint messageFsm=0;
            uint len=0;
            uint timeOffset=0;
            uint src=0;
            uint senderId=0;
            uint msgId=0;
            uint currentPayloadPos=0;
          
            List<byte> payload=new List<byte>();
            uint checkSum=0;
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            /* According to https://wiki.paparazziuav.org/wiki/Messages_Format ( Telemetry storage format for data logger ) */
            while (file.Position < file.Length)
            {
                log.progressValue = (float)file.Position / (float)file.Length;
                currentByte = (uint)file.ReadByte();
                if (currentByte == 0x99 && messageFsm == 0)
                {
                    messageFsm = 1;
                }
                else switch (messageFsm)
                {
                    case 1:
                        /* LENGTH  */
                        len = currentByte;
                        messageFsm = 2;
                        break;
                    case 2:
                        /* SOURCE */
                        src = currentByte;
                        messageFsm = 3;
                        break;
                    case 3:
                        /* TIMESTAMP_LSB */
                        timeOffset = currentByte;
                        messageFsm = 4;
                        break;
                    case 4:
                        /* TIMESTAMP */
                        timeOffset += currentByte << 8;
                        messageFsm = 5;
                        break;
                    case 5:
                        /* TIMESTAMP */
                        timeOffset += currentByte << 16;
                        messageFsm = 6;
                        break;
                    case 6:
                        /* TIMESTAMP_MSB */
                        timeOffset += currentByte << 24;
                        messageFsm = 7;
                        break;
                    case 7:
                        /* SENDER_ID */
                        senderId = currentByte;
                        messageFsm = 8;
                        break;
                    case 8:
                        /* MSG_ID */
                        msgId = currentByte;
                        
                        messageFsm = 9;
                        break;
                    case 9:
                        /* MSG_PAYLOAD */
                        currentPayloadPos++;
                        payload.Add((byte)currentByte);
                        if (currentPayloadPos == len - 2)
                        {
                            messageFsm = 10;
                            currentPayloadPos = 0;
                        }
                        break;
                    case 10:
                        /* CHECKSUM */
                        uint packetCheckSum = (uint)(len + src + (timeOffset & 0xFF) + (timeOffset >> 8 & 0xFF) + (timeOffset >> 16 & 0xFF) + (timeOffset >> 24 & 0xFF) + senderId + msgId + payload.Sum(item => item));
                        packetCheckSum %= 0x100;
                        checkSum = currentByte;
                        if (checkSum == packetCheckSum)
                        {
                            string[] fieldsStr = log.messages.msgClassFieldsName[(byte)msgId].Split('|');
                            string[] types = log.messages.msgClassFieldsType[(byte)msgId].Split('|');
                            string values = "";
                            log.messages.MessageArrivedLog((byte)msgId, payload, ref values);
                            string[] valuesStr = values.Split('|');
                            string drawableParams = "";
                            for (int i=0;i<fieldsStr.Length-1;i++)
                            {
                                if (types[i] == "uint8[]" || types[i] == "int8[]" || types[i] == "int16[]" || types[i] == "uint16[]" || types[i] == "char[]")
                                    continue;
                                if (!log.fieldsValues.ContainsKey(log.messages.msgClassNames[(byte)msgId] + "_" + fieldsStr[i]))
                                {
                                    log.fieldsValues.Add(log.messages.msgClassNames[(byte)msgId] + "_" + fieldsStr[i], new List<double>());
                                    log.fieldsTimeValues.Add(log.messages.msgClassNames[(byte)msgId] + "_" + fieldsStr[i], new List<uint>());
                                }
                                ((List<double>)log.fieldsValues[log.messages.msgClassNames[(byte)msgId] + "_" + fieldsStr[i]]).Add(Convert.ToDouble(valuesStr[i]));
                                ((List<uint>)log.fieldsTimeValues[log.messages.msgClassNames[(byte)msgId] + "_" + fieldsStr[i]]).Add(timeOffset/10);
                                drawableParams += fieldsStr[i] + "|";
                            }
                            if (!log.cmdList.ContainsKey(msgId) && drawableParams != "")
                                log.cmdList.Add(msgId, log.messages.msgClassNames[(byte)msgId]);
                            if (!log.fieldsList.ContainsKey((byte)msgId) && drawableParams != "")
                                log.fieldsList.Add((byte)msgId, drawableParams);
                        }
                        payload.Clear();
                        messageFsm = 0;
                        break;
                }
            }

            log.isLoading = false;
        }
    }
}
