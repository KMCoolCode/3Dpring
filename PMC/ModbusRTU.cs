using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace sun
{
    class ModbusRTU
    {
        public SerialPort serialPort1 = new SerialPort();

        private object lockCom = new object();
       
        public bool OpenCom(string com, Int32 baudRate , Int32 dataBits, StopBits stopBits,Parity parity)
        {
            try
            {
                serialPort1.PortName = com;
                serialPort1.BaudRate = baudRate;
                serialPort1.DataBits = dataBits;
                serialPort1.StopBits = stopBits;
                serialPort1.Parity   =   parity;
                serialPort1.Open();     //打开串口
                //serialPort1.DataReceived += new SerialDataReceivedEventHandler(post_DataReceived);
                return true;
            }
            catch {
                return false;
            }
        }
        public void CloseCom()
        {
            serialPort1.Close();     //关闭串口
        }

        public void SendBytes(byte[] sendbytes)
        {
            serialPort1.Write(sendbytes, 0, sendbytes.Length);
        }

        public byte[] RcvBytes()
        {
                byte[] Rcvbytes = new byte[serialPort1.BytesToRead];
                serialPort1.Read(Rcvbytes, 0, Rcvbytes.Length);
                return Rcvbytes;
        }

        public short[] ReaderInputRegister(byte Station, short startAdr, short count, byte FuncCode)
        {
            short[] DATAs = new short[count];
            lock (lockCom)
            {
                byte[] senbytes = new byte[8];
                byte[] DATA = new byte[count * 2];
                senbytes[0] = Station;
                senbytes[1] = FuncCode;
                senbytes[2] = (byte)(startAdr >> 8);
                senbytes[3] = (byte)(startAdr & 0x00FF);
                senbytes[4] = (byte)(count >> 8);
                senbytes[5] = (byte)(count & 0x00FF);
                byte[] bytes = new byte[6];
                byte[] bytes1 = new byte[2];
                Array.Copy(senbytes, 0, bytes, 0, 6);
                GetCRC(senbytes, ref bytes1);
                senbytes[6] = bytes1[0];
                senbytes[7] = bytes1[1];
                SendBytes(senbytes);
                Thread.Sleep(100);
                if (serialPort1.BytesToRead > 0)
                {
                    byte[] RcvB = RcvBytes();
                    Array.Copy(RcvB, 3, DATA, 0, count * 2);
                    DATAs = (new Data_Handling()).byteTOshort(DATA, count);
                }
                else
                {
                    ///F_Main.Message("获取保持性寄存器失败");
                }
            }
            
            return DATAs;
        }
        public void WriteOneHoldingRegister(byte Station, short ADR,short DATA, byte FuncCode) 
        {
            lock (lockCom)
            {
                byte[] senbytes = new byte[8];
                senbytes[0] = Station;
                senbytes[1] = FuncCode;
                senbytes[2] = (byte)(ADR >> 8);
                senbytes[3] = (byte)(ADR & 0x00FF);
                senbytes[4] = (byte)(DATA >> 8);
                senbytes[5] = (byte)(DATA & 0x00FF);
                byte[] bytes = new byte[6];
                byte[] bytes1 = new byte[2];
                Array.Copy(senbytes, 0, bytes, 0, 6);
                GetCRC(senbytes, ref bytes1);
                senbytes[6] = bytes1[0];
                senbytes[7] = bytes1[1];
                SendBytes(senbytes);
                Thread.Sleep(100);
                if (serialPort1.BytesToRead > 0)
                {
                    byte[] RcvB = RcvBytes();
                    //F_Main.Message("写入保持性寄存器成功");
                }
                else
                {
                    //F_Main.Message("写入保持性寄存器失败");
                }
            }
            
        }

        public void LightControl(byte[] bytes)
        {
            if (serialPort1.IsOpen)
            {
                SendBytes(bytes);
                Thread.Sleep(100);
                if (serialPort1.BytesToRead > 0)
                {
                    byte[] RcvB = RcvBytes();
                }
            }
            else
            {
                //message("串口未打开");
            }
        }

        public void GetCRC(byte[] message, ref byte[] CRC)
        {

            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;

            for (int i = 0; i < (message.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
        }
    }
}
