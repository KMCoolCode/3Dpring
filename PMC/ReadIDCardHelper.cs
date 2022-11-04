using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadIDCard
{
    public class ReadIDCardHelper
    {
       private SerialPort serialPort;
        public delegate void SendMessage(string msg);
        public event SendMessage EventCardMes;
        public bool Open(string name,int bandRate)
        {
            try
            {
                serialPort = new System.IO.Ports.SerialPort();
                serialPort.PortName = name;
                serialPort.BaudRate = bandRate;
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.Parity = System.IO.Ports.Parity.None;
                serialPort.StopBits = System.IO.Ports.StopBits.One;
                serialPort.Encoding = Encoding.Unicode;
                serialPort.DataBits = 8;
                if (!serialPort.IsOpen)
                    serialPort.Open();
                
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool Close()
        {
            try
            {
                if (serialPort != null) {
                    if (serialPort.IsOpen)
                        serialPort.Close();
                }
                

            }
            catch
            {
                return false;
            }
            return true;
        }
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            var buff = new byte[100];
            var length = serialPort.Read(buff, 0, buff.Length);
            var msg = Encoding.ASCII.GetString(buff, 0, length);
            string[] msgarray = msg.Split('\r');
            msg = msgarray[0].Replace("", "");
            EventCardMes?.Invoke(msg);
        }
    }
}
