using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using CypressLibrary;

namespace Ddp442xLibrary
{
    public class Ddp442x
    {
        public enum CommandType { 
            WRITE,
            READ
        }

        public enum CommandId {
            // PC--> DDP4422
            LED_CURRENT_DAC             = 0xD1,         //Control the Current of LED
            TEMPERATURE_SENSOR          = 0x9C,         //Read DMD Temperature
            WRITE_TEST_PATTERNS         = 0x11,         //but not include the pattern type
            WRITE_INPUT_SOURCE          = 0x01,
            I2C_READ                    = 0x15,         //All Read cmd must use this first
            DMD_FAN_CYCLE               = 0xEB,
            LED_FAN1_CYCLE              = 0xEC,
            LED_FAN2_CYCLE              = 0xED,
            LED_TEMPERATURE             = 0x9F,         //Read LED Junction Temperature.
            LED_DRIVERBOARD_TEMPERATURE = 0x9E,         //Read LED Driverboard Temperature.
            READ_LIGHT                  = 0xF7,         //Read Light Sensor
            APPLICATION_VER             = 0x85,         //Read FW Version
            SEQ_VER                     = 0xBE,         //Read Sequence Revision
            MOTOR1_DIR                  = 0xB5,         //Motor Direction
            MOTOR1_STEP                 = 0xB6,         //Motor Step & Frequence
            MOTOR2_DIR                  = 0xBA,         //Motor Direction
            MOTOR2_STEP                 = 0xBB,         //Motor Step & Frequence
            SOLID_FIELD_GENERATOR       = 0x13,         //Use to set solid field color.
            LED_TYPE                    = 0xD2,         //To Read the LED Wavelength.
            PROJECTOR_FLIP_HORIZONTAL   = 0x1F,
            PROJECTOR_FLIP_VERTICAL     = 0x10,
            V_BY_ONE                    = 0xF3,         //Get Vx1 Status.
            EXTERNAL_SOURCE             = 0xF0,         //Use for changing projector resolution mode.
            MOTOR_HOMING = 0xFC,         // Set motor homing/Get motor homing information(status)
            PI_INDEX_READING = 0xFD,         // Set motor index for 0xfb & 0xfc to read.
            DMD_PARK = 0x87,
        }

        CypressI2c mCpressI2c;
        public const byte cSlaveAddress7bit = 0x1A;     //Equal 34h / 2
        const int I2C_DELAY_TIME = 200;
        public String mLogString;

        public Ddp442x(CypressI2c cypressI2c, CypressSpi cypressSpi, CypressGpio cypressGpio)
        {
            mCpressI2c = cypressI2c;
            mCpressGpio = cypressGpio;
        }

        //
        // Misc
        //
        public void InitLog(CommandType type, CommandId id, byte SlaveAddress)
        {
            switch (type) { 
                case CommandType.WRITE:
                    mLogString += "Write:" +
                                //" SlaveAddr: " + (SlaveAddress * 2).ToString("X2") +
                                " SlaveAddr: " + (SlaveAddress).ToString("X2") +
                                " SubAddr: " + ((byte)id).ToString("X2");
                    break;
                case CommandType.READ:
                default:
                    mLogString += "Read:" +
                                //" SlaveAddr: " + (SlaveAddress * 2 + 1).ToString("X2") +
                                " SlaveAddr: " + (SlaveAddress).ToString("X2") +
                                " SubAddr: " + ((byte)id).ToString("X2");
                    break;
            }
        }

        public void AppendLogSend(byte[] sendBuf)
        {
            mLogString += " Data: ";
            for (int i = 1; i < sendBuf.Length; i++)
            {
                mLogString += sendBuf[i].ToString("X2") + " ";
            }
            mLogString += "(OK)";
        }

        public void AppendLogRecv(byte[] recvBuf)
        {
            mLogString += " Data: ";
            for (int i = 0; i < recvBuf.Length; i++)
            {
                mLogString += recvBuf[i].ToString("X2") + " ";
            }
            mLogString += "(OK)";
        }

		
        //
        // Public
        //
        public int Ddp442x_Config(byte slaveAddress, ref byte[] buf, int len, int rnw)
        {

            int ret;

            if (rnw > 0) //Read
                ret = mCpressI2c.Read(slaveAddress, ref buf, len);
            else //Write
                ret = mCpressI2c.Write(slaveAddress, buf, len);

            if (ret < 0)
            {
                return ret;
            }

            return 0;
        }

        public String GetLog() {
            return mLogString + "\n";
        }

        public void ClearLog()
        {
            mLogString = "";
        }

        #region I2C

        public int SetSolidFieldGenerator(int red, int green, int blue) {
            int cWriteSize = 7;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;

            InitLog(CommandType.WRITE, CommandId.SOLID_FIELD_GENERATOR, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.SOLID_FIELD_GENERATOR;
            sendBuf[2] = (byte)((red >> 8) & 0xFF);
            sendBuf[1] = (byte)(red & 0xFF);
            sendBuf[4] = (byte)((green >> 8) & 0xFF);
            sendBuf[3] = (byte)(green & 0xFF);
            sendBuf[6] = (byte)((blue >> 8) & 0xFF); 
            sendBuf[5] = (byte)(blue & 0xFF);
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, 7);
            if (ret < 0) {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogSend(sendBuf);

            return 0;
        }

        public int SetLEDDAC(int level)           //for controlling 4422 LED current
        {
            const int cWriteSize = 3;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;
            InitLog(CommandType.WRITE, CommandId.LED_CURRENT_DAC, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.LED_CURRENT_DAC;
            sendBuf[1] = (byte)((level >> 8) & 0xFF);
            sendBuf[2] = (byte)(level & 0xFF);

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogSend(sendBuf);
            return 0;
        }

        public int GetLEDDAC(out int Dac)    //get LED Current DAC
        {
            const int cWriteSize = 2;
            const int cReadSize = 2;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            Dac = 0;

            InitLog(CommandType.READ, CommandId.LED_CURRENT_DAC, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.LED_CURRENT_DAC;

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize, false);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(I2C_DELAY_TIME);

            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            Dac = (recvBuf[0] << 8) | recvBuf[1];

            AppendLogRecv(recvBuf);

            return 0;
        }

        public int SetMotor1Direction(int index)
        {
            const int cWriteSize = 2; // (index = 0 or 1)
            byte[] sendBuf = new byte[cWriteSize];
            int ret;

            InitLog(CommandType.WRITE, CommandId.MOTOR1_DIR, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.MOTOR1_DIR;
            sendBuf[1] = (byte)(index & 0x01);

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogSend(sendBuf);
            return 0;
        }

        public int SetMotor2Direction(int index)
        {
            const int cWriteSize = 2; // (index = 0 or 1)
            byte[] sendBuf = new byte[cWriteSize];
            int ret;

            InitLog(CommandType.WRITE, CommandId.MOTOR2_DIR, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.MOTOR2_DIR;
            sendBuf[1] = (byte)(index & 0x01);

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogSend(sendBuf);
            return 0;
        }

        public int SetMotor1Step(int count,int freq) // modify in 442
        {
            const int cWriteSize = 5;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;

            InitLog(CommandType.WRITE, CommandId.MOTOR1_STEP, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.MOTOR1_STEP;
            sendBuf[1] = (byte)(count & 0xFF);
            sendBuf[2] = (byte)(count >> 8 & 0xFF);
            sendBuf[3] = (byte)(freq & 0xFF);
            sendBuf[4] = (byte)(freq >> 8);

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogSend(sendBuf);
            return 0;
        }

        public int SetMotor2Step(int count, int freq) // modify in 442
        {
            const int cWriteSize = 5;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;

            InitLog(CommandType.WRITE, CommandId.MOTOR2_STEP, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.MOTOR2_STEP;
            sendBuf[1] = (byte)(count & 0xFF);
            sendBuf[2] = (byte)(count >> 8 & 0xFF);
            sendBuf[3] = (byte)(freq & 0xFF);
            sendBuf[4] = (byte)(freq >> 8);

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            AppendLogSend(sendBuf);
            return 0;
        }

        public int SetMotorHoming(int motorNum, int freq)
        {
            const int cWriteSize = 4;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;

            InitLog(CommandType.WRITE, CommandId.MOTOR_HOMING, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.MOTOR_HOMING;
            sendBuf[1] = (byte)motorNum;
            sendBuf[2] = (byte)(freq & 0xFF);
            sendBuf[3] = (byte)(freq >> 8);

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            AppendLogSend(sendBuf);
            return 0;
        }

        public int GetApplicationVer(out string outApplicationVer)
        {
            const int cWriteSize = 2;
            const int cReadSize = 4;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;
            int patched = 0;
            InitLog(CommandType.READ, CommandId.APPLICATION_VER, cSlaveAddress7bit);

            outApplicationVer = "";
            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.APPLICATION_VER;
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogRecv(recvBuf);
            patched = recvBuf[1] << 8 | recvBuf[0];
            outApplicationVer = (recvBuf[3]).ToString() + "." + (recvBuf[2]).ToString() + "." + patched.ToString();

            return 0;
        }

        public int GetSeqVer(out string SeqVer)
        {
            const int cWriteSize = 2;
            const int cReadSize = 4;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;
            UInt32 ver;
            InitLog(CommandType.READ, CommandId.SEQ_VER, cSlaveAddress7bit);

            SeqVer = "";
            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.SEQ_VER;
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogRecv(recvBuf);
            
            ver = (UInt32)(recvBuf[3] << 24) + (UInt32)(recvBuf[2] << 16) + (UInt32)(recvBuf[1] << 8) + (UInt32) recvBuf[0];
            Console.Write(ver);
            SeqVer = ver.ToString();

            return 0;
        }

        public int GetTemperature(out int outTemperature)
        {
            const int cWriteSize = 2;
            const int cReadSize = 1;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            InitLog(CommandType.READ, CommandId.TEMPERATURE_SENSOR, cSlaveAddress7bit);

            outTemperature = 0;

            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.TEMPERATURE_SENSOR;
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            //Thread.Sleep(100);
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            outTemperature = recvBuf[0];

            AppendLogRecv(recvBuf);

            return 0;
        }

        public int GetLEDTemperature(out float outTemperature)
        {
            const int cWriteSize = 2;
            const int cReadSize = 4;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            InitLog(CommandType.READ, CommandId.LED_TEMPERATURE, cSlaveAddress7bit);

            outTemperature = 0;

            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.LED_TEMPERATURE;
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(100);
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            outTemperature = FixedToFloat(recvBuf);
            Console.Write("Temp = {0}\n",outTemperature);
            for (int i = 0; i < 4; i++)
            {
                Console.Write("recvBuf[{0}] = {1}\n",i,recvBuf[i]);
            }
                AppendLogRecv(recvBuf);

            return 0;
        }

        public float FixedToFloat(byte[] input)
        {
            float temperatureF;
            
            temperatureF = (input[3] << 8) + input[2] + (float)((input[1] << 8) + input[0]) / 65536;
            return temperatureF;
        }

        public int GetLedDriverBoardTemperature(out float outTemperature)
        {
            const int cWriteSize = 2;
            const int cReadSize = 2;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            InitLog(CommandType.READ, CommandId.LED_DRIVERBOARD_TEMPERATURE, cSlaveAddress7bit);

            outTemperature = 0;

            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.LED_DRIVERBOARD_TEMPERATURE;
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(I2C_DELAY_TIME);
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            outTemperature = tmp75Conversion((recvBuf[1] << 8) | recvBuf[0]);

            AppendLogRecv(recvBuf);

            return 0;
        }

        public float tmp75Conversion(int temperature)
        {
            if ((temperature & 0x0800) > 0) // negative
            {
                temperature = 0x1000 - temperature;
                return 0 - (((temperature & 0x0FF0) >> 4) + ((float)(temperature & 0x0F) / 16));
            }
            else
            {
                return ((temperature & 0x0FF0) >> 4) + ((float)(temperature & 0x0F) / 16);
            }
        }

        public int GetFanDuty(out int DMDDuty, out int LED1Duty, out int LED2Duty)
        {
            const int cWriteSize = 2;
            const int cReadSize = 1;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            InitLog(CommandType.READ, CommandId.DMD_FAN_CYCLE, cSlaveAddress7bit);

            DMDDuty = 0;
            LED1Duty = 0;
            LED2Duty = 0;

            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.DMD_FAN_CYCLE;
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(I2C_DELAY_TIME);
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            AppendLogRecv(recvBuf);

            DMDDuty = recvBuf[0];

            InitLog(CommandType.READ, CommandId.LED_FAN1_CYCLE, cSlaveAddress7bit);

            sendBuf[1] = (byte)CommandId.LED_FAN1_CYCLE;
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(I2C_DELAY_TIME);
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            AppendLogRecv(recvBuf);

            LED1Duty = recvBuf[0];

            InitLog(CommandType.READ, CommandId.LED_FAN2_CYCLE, cSlaveAddress7bit);

            sendBuf[1] = (byte)CommandId.LED_FAN2_CYCLE;
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(I2C_DELAY_TIME);
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            AppendLogRecv(recvBuf);

            LED2Duty = recvBuf[0];
            return 0;
        }

        public int GetProjectorFlip(out byte ProjectorFlipH, out byte ProjectorFlipV)
        {
            const int cWriteSize = 2;
            const int cReadSize = 1;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            InitLog(CommandType.READ, CommandId.PROJECTOR_FLIP_HORIZONTAL, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.PROJECTOR_FLIP_HORIZONTAL;
            ProjectorFlipH = 0;
            ProjectorFlipV = 0;
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            ProjectorFlipH = recvBuf[0];
            mLogString += "\n";
            InitLog(CommandType.READ, CommandId.PROJECTOR_FLIP_VERTICAL, cSlaveAddress7bit);
            sendBuf[1] = (byte)CommandId.PROJECTOR_FLIP_VERTICAL;
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            ProjectorFlipV = recvBuf[0];
            return 0;
        }

        public int SetProjectorFlipH(byte ProjectorFlipH)
        {
            const int cWriteSize = 2;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;

            InitLog(CommandType.WRITE, CommandId.PROJECTOR_FLIP_HORIZONTAL, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.PROJECTOR_FLIP_HORIZONTAL;
            sendBuf[1] = ProjectorFlipH;

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogSend(sendBuf);

            return 0;
        }

        public int SetProjectorFlipV(byte ProjectorFlipV)
        {
            const int cWriteSize = 2;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;

            InitLog(CommandType.WRITE, CommandId.PROJECTOR_FLIP_VERTICAL, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.PROJECTOR_FLIP_VERTICAL;
            sendBuf[1] = ProjectorFlipV;

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogSend(sendBuf);

            return 0;
        }

        public int SetFanDuty(String FanName, int dutyCycle)       //442
        {
            const int cWriteSize = 2;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;
            if (FanName.Equals("DMDFan"))
            {
                InitLog(CommandType.WRITE, CommandId.DMD_FAN_CYCLE, cSlaveAddress7bit);
                sendBuf[0] = (byte)CommandId.DMD_FAN_CYCLE;
            }
            else if(FanName.Equals("LEDFan1")){
                InitLog(CommandType.WRITE, CommandId.LED_FAN1_CYCLE, cSlaveAddress7bit);
                sendBuf[0] = (byte)CommandId.LED_FAN1_CYCLE;
            }
            else if (FanName.Equals("LEDFan2"))
            {
                InitLog(CommandType.WRITE, CommandId.LED_FAN2_CYCLE, cSlaveAddress7bit);
                sendBuf[0] = (byte)CommandId.LED_FAN2_CYCLE;
            }
            sendBuf[1] = (byte)(dutyCycle & 0xFF);
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogSend(sendBuf);
            return 0;
        }

        public int SetInputSource(byte Select)      //442
        {
            const int cWriteSize = 2;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;

            InitLog(CommandType.WRITE, CommandId.WRITE_INPUT_SOURCE, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.WRITE_INPUT_SOURCE;
            sendBuf[1] = Select;

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogSend(sendBuf);

            return 0;
        }

        public int SetTestPatterns(byte PatternSel)     //442
        {
            const int cWriteSize = 2;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;

            InitLog(CommandType.WRITE, CommandId.WRITE_TEST_PATTERNS, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.WRITE_TEST_PATTERNS;
            sendBuf[1] = PatternSel;
            
            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            AppendLogSend(sendBuf);

            return 0;
        }

        public int GetLightSensor(out int outLight)      //442
        {
            const int cWriteSize = 2;
            const int cReadSize = 2;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            outLight = 0;

            InitLog(CommandType.READ, CommandId.READ_LIGHT, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.READ_LIGHT;

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize,false);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(1000);     //need match with 4422 FW
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            outLight = (recvBuf[1] << 8) | recvBuf[0];

            AppendLogRecv(recvBuf);

            return 0;
        }

        public int GetLEDType(out int wavelength)
        {
            const int cWriteSize = 2;
            const int cReadSize = 1;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            byte ledtype;
            wavelength = 0;

            InitLog(CommandType.READ, CommandId.LED_TYPE, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.LED_TYPE; ;

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, 2, false);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            ledtype = recvBuf[0];

            if (ledtype == 0)
                wavelength = 405;
            else if (ledtype == 8)
                wavelength = 385;
            else if (ledtype == 6)
                wavelength = 365;
            AppendLogRecv(recvBuf);

            return 0;
        }
        public int GetVx1Lock(out int status)
        {
            const int cWriteSize = 2;
            const int cReadSize = 1;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            status = 0;

            InitLog(CommandType.WRITE, CommandId.V_BY_ONE, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.V_BY_ONE;          //0xF3
            sendBuf[1] = 0x29;

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize, false);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(100);

            InitLog(CommandType.READ, CommandId.EXTERNAL_SOURCE, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.I2C_READ;           //0x15
            sendBuf[1] = (byte)CommandId.EXTERNAL_SOURCE;       //0xF0

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize, false);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(100);
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            status = recvBuf[0];

            AppendLogRecv(recvBuf);

            return 0;
        }

        public int SetResolutionMode(byte mode)
        {
            const int cWriteSize = 3;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;
            InitLog(CommandType.WRITE, CommandId.EXTERNAL_SOURCE, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.EXTERNAL_SOURCE;
            sendBuf[1] = mode;
            sendBuf[2] = 0x00;

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            AppendLogSend(sendBuf);
            return 0;
        }

        public int GetHomingInformation(int index, out byte status)
        {
            const int cWriteSize = 2;
            const int cReadSize = 2;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            status = 0;

            InitLog(CommandType.WRITE, CommandId.PI_INDEX_READING, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.PI_INDEX_READING;
            sendBuf[1] = (byte)index;

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize, false);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(10);

            InitLog(CommandType.READ, CommandId.MOTOR_HOMING, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.I2C_READ;
            sendBuf[1] = (byte)CommandId.MOTOR_HOMING;

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize, false);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            Thread.Sleep(100);
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            status = recvBuf[0];


            AppendLogRecv(recvBuf);

            return 0;
        }
        #endregion I2C

        #region GPIO
        CypressGpio mCpressGpio;
        public int SetProjectorOnOff(bool ProjectorEnable)  //442
        {
            int ret;
            byte GpioNum = 0;

            byte Value;
            Value = (byte)(ProjectorEnable ? 1 : 0);

            ret = mCpressGpio.Write(GpioNum, Value);
            if (ret < 0)
            {
                mLogString += "Gpio Write (Fail)";
                return ret;
            }

            return 0;
        }

        public int SetLedOnOff(bool LedEnable)          //new in 442
        {
            int ret;
            byte GpioNum = 14;

            byte Value;
            Value = (byte)(LedEnable ? 1 : 0);

            ret = mCpressGpio.Write(GpioNum, Value);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }

            return 0;
        }

        public int GetLedOnOff(ref bool LedSwitch)
        {
            int ret;
            byte GpioNum = 14;
            byte Value = 0;
            ret = mCpressGpio.Read(GpioNum, ref Value);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            LedSwitch = (Value == 1) ? true : false;
            return 0;
        }

        public int GetI2CBusy(ref bool isI2CBusy)       //new in 442
        {
            int ret;
            byte GpioNum = 1;
            byte Value = 0;
            ret = mCpressGpio.Read(GpioNum,ref Value);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            isI2CBusy = (Value == 0) ? true : false;
            return 0;
        }

        public int GetAsicReady(ref bool isAsicReady)   //new in 442
        {
            int ret;
            byte GpioNum = 15;
            byte Value = 0;
            ret = mCpressGpio.Read(GpioNum, ref Value);
            if (ret < 0)
            {
                mLogString += " (Fail)";
                return ret;
            }
            isAsicReady = (Value == 1) ? true : false;
            return 0;
        }

        #endregion GPIO
       

    }//class Ddp442x
}//namespace DDP442xLEDConsole
