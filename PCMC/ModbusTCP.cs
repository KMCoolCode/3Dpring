using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace sun
{

    class ModbusTCP
    {

        Data_Handling Data_handling = new Data_Handling();
        private static object obj = new object();
        public Socket modbusSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public Boolean Mod_Tcp_connect(string Ip, int port)
        {
            Boolean Isconnectd = false;
            IPAddress ip = IPAddress.Parse(Ip);
            IPEndPoint ipe = new IPEndPoint(ip, port);
            modbusSocket.Connect(ipe);
            Thread.Sleep(100);
            Isconnectd = true;
            return Isconnectd;
        }

        public Boolean Mod_Tcp_func3(byte start_adrW1, byte start_adrW2, out byte[] in_PLC_rcv)
        {
            byte[] sendByte = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x03, start_adrW1, start_adrW2, 0x00, 0x7D };//功能码03读取多个字
                                                                                                                       // byte[] sendByte = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x03, start_adrW1, start_adrW2, 0x00, 0x7B };//功能码03读取
            modbusSocket.Send(sendByte, sendByte.Length, 0);

            string str = DateTime.Now.ToString() + "：" + DateTime.Now.Millisecond.ToString();
            int a1 = 0;
            while (modbusSocket.Available <= 0)
            {
                if (modbusSocket.Poll(1000, SelectMode.SelectRead) && (modbusSocket.Available <= 0))
                {
                    a1 = 1;
                    break;
                }

            }
            string recStr = "";
            Boolean Is_C = false;
            byte[] PLC_rcv = new byte[250];
            if (a1 != 1)
            {
                byte[] recByte = new byte[modbusSocket.Available];
                int bytes = modbusSocket.Receive(recByte, recByte.Length, 0);
                string str1 = DateTime.Now.ToString() + "：" + DateTime.Now.Millisecond.ToString();
                Array.Copy(recByte, 9, PLC_rcv, 0, 250);
                recStr += Encoding.UTF8.GetString(recByte, 0, 10);
                Is_C = true;
            }
            in_PLC_rcv = PLC_rcv;
            return Is_C;

        }

        public Boolean Mod_Tcp_func16(byte start_adrW1, byte start_adrW2, byte[] out_PLC_send)
        {
            byte[] sendByte1 = { 00, 00, 00, 00, 0x00, 0xFD, 00, 0x10, start_adrW1, start_adrW2, 0x00, 0x7B, 0xF6 };//功能码16设置字
            //byte[] sendByte1 = { 00, 00, 00, 00, 0x00, 0xF9, 00, 0x10, start_adrW1, start_adrW2, 0x00, 0x79, 0xF2 };//功能码16设置字
            //byte[] TEST = new byte[242];
            //TEST[0] = 4;
            byte[] sendByte = new byte[sendByte1.Length + out_PLC_send.Length];
            sendByte1.CopyTo(sendByte, 0);
            out_PLC_send.CopyTo(sendByte, sendByte1.Length);
            //TEST.CopyTo(sendByte, sendByte1.Length);
            modbusSocket.Send(sendByte, sendByte.Length, 0);
            int a1 = 0;
            while (modbusSocket.Available <= 0)
            {
                if (modbusSocket.Poll(1000, SelectMode.SelectRead) && (modbusSocket.Available <= 0))
                {
                    a1 = 1;
                    break;
                }
            }
            string recStr = "";
            Boolean Is_C = false;
            if (a1 != 1)
            {
                byte[] recByte = new byte[modbusSocket.Available];
                int bytes = modbusSocket.Receive(recByte, recByte.Length, 0);
                recStr += Encoding.UTF8.GetString(recByte, 0, 10);
                Is_C = true;
            }
            return Is_C;
        }

        public Boolean Mod_Tcp_func3_1(byte start_adrW1, byte start_adrW2, out byte[] in_PLC_rcv)
        {
            //byte[] sendByte = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x03, start_adrW1, start_adrW2, 0x00, 0x7D };//功能码03读取多个字
            byte[] sendByte = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x03, start_adrW1, start_adrW2, 0x00, 0x7B };//功能码03读取
            modbusSocket.Send(sendByte, sendByte.Length, 0);

            string str = DateTime.Now.ToString() + "：" + DateTime.Now.Millisecond.ToString();
            int a1 = 0;
            while (modbusSocket.Available <= 0)
            {
                if (modbusSocket.Poll(1000, SelectMode.SelectRead) && (modbusSocket.Available <= 0))
                {
                    a1 = 1;
                    break;
                }

            }
            string recStr = "";
            Boolean Is_C = false;
            byte[] PLC_rcv = new byte[250];
            if (a1 != 1)
            {
                byte[] recByte = new byte[modbusSocket.Available];
                int bytes = modbusSocket.Receive(recByte, recByte.Length, 0);
                string str1 = DateTime.Now.ToString() + "：" + DateTime.Now.Millisecond.ToString();
                Array.Copy(recByte, 9, PLC_rcv, 0, 246);
                recStr += Encoding.UTF8.GetString(recByte, 0, 10);
                Is_C = true;
            }
            in_PLC_rcv = PLC_rcv;
            return Is_C;

        }

        public Boolean Mod_Tcp_func16_1(byte start_adrW1, byte start_adrW2, byte[] out_PLC_send)
        {
            //byte[] sendByte1 = { 00, 00, 00, 00, 0x00, 0xFD, 00, 0x10, start_adrW1, start_adrW2, 0x00, 0x7B, 0xF6 };//功能码16设置字
            byte[] sendByte1 = { 00, 00, 00, 00, 0x00, 0xF9, 00, 0x10, start_adrW1, start_adrW2, 0x00, 0x79, 0xF2 };//功能码16设置字
            //byte[] TEST = new byte[242];
            //TEST[0] = 4;
            byte[] sendByte = new byte[sendByte1.Length + out_PLC_send.Length];
            sendByte1.CopyTo(sendByte, 0);
            out_PLC_send.CopyTo(sendByte, sendByte1.Length);
            //TEST.CopyTo(sendByte, sendByte1.Length);
            modbusSocket.Send(sendByte, sendByte.Length, 0);
            int a1 = 0;
            while (modbusSocket.Available <= 0)
            {
                if (modbusSocket.Poll(1000, SelectMode.SelectRead) && (modbusSocket.Available <= 0))
                {
                    a1 = 1;
                    break;
                }
            }
            string recStr = "";
            Boolean Is_C = false;
            if (a1 != 1)
            {
                byte[] recByte = new byte[modbusSocket.Available];
                int bytes = modbusSocket.Receive(recByte, recByte.Length, 0);
                recStr += Encoding.UTF8.GetString(recByte, 0, 10);
                Is_C = true;
            }
            return Is_C;
        }

        public void Out_PLC_Send(out byte[] out_PLC_1, out byte[] out_PLC_2)
        {
            byte[] out_PLC_send = new byte[492];
            byte[] out_PLC_send1 = new byte[246];
            byte[] out_PLC_send2 = new byte[246];
            Data_handling.bitTObyte(PLC_Output.b, 40).CopyTo(out_PLC_send, 0);
            Data_handling.highTolow(out_PLC_send, 40);
            PLC_Output.B.CopyTo(out_PLC_send, 40);
            Data_handling.highTolow(out_PLC_send, 80);
            Data_handling.shortTObyte(PLC_Output.W, 40).CopyTo(out_PLC_send, 80);
            Data_handling.intTObyte(PLC_Output.D, 40).CopyTo(out_PLC_send, 160);
            Data_handling.floatTObyte(PLC_Output.F, 40).CopyTo(out_PLC_send, 320);
            Array.Copy(out_PLC_send, 0, out_PLC_send1, 0, 246);
            Array.Copy(out_PLC_send, 246, out_PLC_send2, 0, 246);
            out_PLC_1 = out_PLC_send1;
            out_PLC_2 = out_PLC_send2;
        }
        public void Out_PLC_Send_1(out byte[] out_PLC)
        {
            byte[] out_PLC_send = new byte[246];
            Data_handling.floatTObyte(PLC_Output.F1, 40).CopyTo(out_PLC_send, 0);
            out_PLC = out_PLC_send;
        }

        public void In_PLC_Rcv(byte[] in_plc_1, byte[] in_plc_2)
        {
            byte[] in_PLC_rcv = new byte[in_plc_1.Length + in_plc_2.Length];
            in_plc_1.CopyTo(in_PLC_rcv, 0);
            in_plc_2.CopyTo(in_PLC_rcv, 250);
            byte[] byte_middle = new byte[40];
            Array.Copy(in_PLC_rcv, 0, byte_middle, 0, 40);
            Data_handling.highTolow(byte_middle, 40);
            PLC_Input.b = Data_handling.byteTObit(byte_middle, 40);
            Array.Copy(in_PLC_rcv, 40, byte_middle, 0, 40);
            PLC_Input.B = byte_middle;
            byte_middle = new byte[80];
            Array.Copy(in_PLC_rcv, 80, byte_middle, 0, 80);
            PLC_Input.W = Data_handling.byteTOshort(byte_middle, 40);
            byte_middle = new byte[160];
            Array.Copy(in_PLC_rcv, 160, byte_middle, 0, 160);
            PLC_Input.D = Data_handling.byteTOint(byte_middle, 40);
            Array.Copy(in_PLC_rcv, 320, byte_middle, 0, 160);
            PLC_Input.F = Data_handling.byteTOfloat(byte_middle, 40);
        }

        public byte[] ModReadInput(int start_adr, int count, out bool Is_C)//0x02：读离散量输入
        {
            byte start_adrW1 = (byte)(start_adr >> 8);
            byte start_adrW2 = (byte)(start_adr & 255);
            byte countW1 = (byte)(count >> 8);
            byte countW2 = (byte)(count & 255);
            byte[] sendByte = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x02, start_adrW1, start_adrW2, countW1, countW2 };//0x02：读离散量输入
            byte[] Rcvbytes = ModbusSend_Rcv(sendByte, out Is_C);
            if (Is_C)
            {
                if (count % 8 == 0)
                {
                    byte[] RcvByte = new byte[count / 8];
                    Array.Copy(Rcvbytes, 9, RcvByte, 0, RcvByte.Length);
                    return RcvByte;
                }
                else
                {
                    byte[] RcvByte = new byte[1 + count / 8];
                    Array.Copy(Rcvbytes, 9, RcvByte, 0, RcvByte.Length);
                    return RcvByte;
                }

            }
            else
            {
                return new byte[1];
            }
        }

        public byte[] ModReadOutput(int start_adr, int count, out bool Is_C)//0x01:读线圈状态
        {
            byte start_adrW1 = (byte)(start_adr >> 8);
            byte start_adrW2 = (byte)(start_adr & 255);
            byte countW1 = (byte)(count >> 8);
            byte countW2 = (byte)(count & 255);
            byte[] sendByte = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x01, start_adrW1, start_adrW2, countW1, countW2 };//0x01:读线圈状态
            byte[] Rcvbytes = ModbusSend_Rcv(sendByte, out Is_C);
            if (Is_C)
            {
                if (count % 8 == 0 & count > 8)
                {
                    byte[] RcvByte = new byte[count / 8];
                    Array.Copy(Rcvbytes, 9, RcvByte, 0, RcvByte.Length);
                    return RcvByte;
                }
                else
                {
                    byte[] RcvByte = new byte[1 + count / 8];
                    Array.Copy(Rcvbytes, 9, RcvByte, 0, RcvByte.Length);
                    return RcvByte;
                }
            }
            else
            {
                return new byte[1];
            }
        }

        public Tuple<bool,byte[]> ModReadRegister(int start_adr, int count, out bool Is_C)//0x03:读保持寄存器
        {
            byte start_adrW1 = (byte)(start_adr >> 8);
            byte start_adrW2 = (byte)(start_adr & 255);
            byte countW1 = (byte)(count >> 8);
            byte countW2 = (byte)(count & 255);
            byte[] sendByte = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0xFF, 0x03, start_adrW1, start_adrW2, countW1, countW2 };//0x01:读保持寄存器
            byte[] Rcvbytes = ModbusSend_Rcv(sendByte, out Is_C);
            if (Is_C)
            {
                byte[] RcvByte = new byte[count * 2];
                if (8 + count * 2 > Rcvbytes.Length)
                {
                    return new Tuple<bool, byte[]>(false, new byte[1]);
                }
                Array.Copy(Rcvbytes, 9, RcvByte, 0, count * 2);
                return new Tuple<bool, byte[]>(true, RcvByte);
            }
            else
            {
                return new Tuple<bool, byte[]>(false, new byte[1]);
            }
        }

        public byte[] ModReadINRegister(int start_adr, int count, out bool Is_C)//0x04:读输入寄存器
        {
            byte start_adrW1 = (byte)(start_adr >> 8);
            byte start_adrW2 = (byte)(start_adr & 255);
            byte countW1 = (byte)(count >> 8);
            byte countW2 = (byte)(count & 255);
            byte[] sendByte = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x04, start_adrW1, start_adrW2, countW1, countW2 };//0x04:读输入寄存器
            byte[] Rcvbytes = ModbusSend_Rcv(sendByte, out Is_C);
            if (Is_C)
            {
                byte[] RcvByte = new byte[count * 2];
                Array.Copy(Rcvbytes, 9, RcvByte, 0, count * 2);
                return RcvByte;
            }
            else
            {
                return new byte[1];
            }
        }

        public bool ModWriteSigleOutput(int start_adr, short num)//0x05:写单个线圈
        {
            byte start_adrW1 = (byte)(start_adr >> 8);
            byte start_adrW2 = (byte)(start_adr & 255);
            byte countW1 = 0;
            byte countW2 = 0;
            if (num == 0)
            {
                countW1 = 0;
                countW2 = 0;
            }
            else if (num == 1)
            {
                countW1 = 0xff;
                countW2 = 0;
            }
            bool Is_C = false;
            byte[] sendByte = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x05, start_adrW1, start_adrW2, countW1, countW2 };//0x05:写单个线圈
            byte[] Rcvbytes = ModbusSend_Rcv(sendByte, out Is_C);
            return Is_C;
        }

        public bool ModWriteOutput(int start_adr, byte[] WriteByte)//0x0F:写多线圈
        {
            byte start_adrW1 = (byte)(start_adr >> 8);
            byte start_adrW2 = (byte)(start_adr & 255);
            int count = WriteByte.Length * 8;
            byte countW1 = (byte)(count >> 8);
            byte countW2 = (byte)(count & 255);
            byte[] sendByte1 = { 0x00, 0x00, 0x00, 0x00, 0x00, (byte)(WriteByte.Length + 7), 0x00, 0x0F, start_adrW1, start_adrW2, countW1, countW2, (byte)WriteByte.Length };//0x05:写多个线圈
            bool Is_C = false;
            byte[] sendByte = new byte[sendByte1.Length + WriteByte.Length];
            sendByte1.CopyTo(sendByte, 0);
            WriteByte.CopyTo(sendByte, sendByte1.Length - 1);
            byte[] Rcvbytes = ModbusSend_Rcv(sendByte, out Is_C);
            return Is_C;
        }

        public bool ModWriteSigleRegister(int start_adr, short num)//0x06:写单个寄存器
        {
            byte start_adrW1 = (byte)(start_adr >> 8);
            byte start_adrW2 = (byte)(start_adr & 255);
            byte countW1 = (byte)(num >> 8);
            byte countW2 = (byte)(num & 255);

            bool Is_C = false;
            byte[] sendByte = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x06, start_adrW1, start_adrW2, countW1, countW2 };//0x05:写单个寄存器
            byte[] Rcvbytes = ModbusSend_Rcv(sendByte, out Is_C);
            return Is_C;
        }

        public bool ModWriteRegister(int start_adr, byte[] WriteByte)//0x10:写多寄存器
        {
            byte start_adrW1 = (byte)(start_adr >> 8);
            byte start_adrW2 = (byte)(start_adr & 255);
            int count = WriteByte.Length / 2;
            byte countW1 = (byte)(count >> 8);
            byte countW2 = (byte)(count & 255);
            byte[] sendByte1 = { 0x00, 0x00, 0x00, 0x00, 0x00, (byte)(WriteByte.Length + 7), 0xFF, 0x10, start_adrW1, start_adrW2, countW1, countW2, (byte)WriteByte.Length };//0x10:写多寄存器
            bool Is_C = false;
            byte[] sendByte = new byte[sendByte1.Length + WriteByte.Length];
            sendByte1.CopyTo(sendByte, 0);
            WriteByte.CopyTo(sendByte, sendByte1.Length);
            byte[] Rcvbytes = ModbusSend_Rcv(sendByte, out Is_C);
            return Is_C;
        }
        public byte[] ModbusSend_Rcv(byte[] sendByte, out bool success)
        {
            lock (obj)
            {
                List<byte> recByte1 = new List<byte>();
                try
                {
                    modbusSocket.Send(sendByte, sendByte.Length, 0);
                    int a1 = 0;
                    while (modbusSocket.Available <= 0)
                    {
                        if (modbusSocket.Poll(1000, SelectMode.SelectRead) && (modbusSocket.Available <= 0))
                        {
                            a1 = 1;
                            break;
                        }

                    }

                    if (a1 != 1)
                    {
                        byte[] recByte = new byte[modbusSocket.Available];
                        int bytes = modbusSocket.Receive(recByte, recByte.Length, 0);
                        success = true;
                        return recByte;
                    }
                    else
                    {
                        success = false;
                        return new byte[1];
                    }

                }
                catch
                {
                    success = false;
                    return new byte[1];
                }

            }
        }


    }


}
