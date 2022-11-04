using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Inovance.InoMotionCotrollerShop.InoServiceContract.EtherCATConfigApi;
using Prism.Events;
using MathNet.Numerics.LinearAlgebra.Double;
using HkCameraDisplay;
using OpenCvSharp;
using static AngelDLP.ViewModels.MainWindowViewModel;
using System.IO;
using System.Threading;
using AngleDLP.PMC;
using static AngleDLP.Models.LogHelper;
using AngleDLP.Models;
using System.IO.Ports;
using sun;
using AngleDLP.PPC;
using System.Linq;
using System.Threading.Tasks;
using static AngleDLP.Models.PlatformCalib;
using Prism.Mvvm;
using ReadIDCard;

namespace AngelDLP.PMC
{
    
    public static class CommonDefinition
    {
        public static string[] AxesEnumTrans = new string[4] { "平台轴", "浮筒轴", "刮刀轴" ,"升降门"};
        
        //public static string[] ADEnumTrans = new string[4] { "液位", "刮刀负压", "","校准位移" };
        public static string[] IOEnumTrans = new string[3] { "真空", "补液阀", "浮球" };
    }
    public enum AxesEnum
    {
        Plaform,
        Levelbox,
        Scraper,
        Window
    };
    public enum ADEnum//用这个枚举
    {
        Level,
        ScraperVacuum,
        EnvTemp,
        EnvHum,
        ResinTemp,
        TankTemp
    };
    public enum DOEnum
    {
        ResinAdd,//补液阀方案
        ResinAddMotorForward,//凸轮夹管机构方案
        ResinAddMotorBackward,//凸轮夹管机构方案
        WindowLockMotorRotate
    };
    public enum DIEnum
    {
        WindowUnlock,
        ScraperOut,
        DoorLocked,
        IsResinBlocked,//凸轮夹管机构方案
        ResinAddBoxGood,
        FootSwitchRelease,
        StartStopPrint,
        OpenCloseWindow,
        AddResin
    };
    public enum RFIDEnum
    {
        PlateRFID,
        CarRFID
    };
    public enum ModBusWriteEnum
    {
        TempControlSV
    };

    public enum ModBusReadEnum
    {
        TempControlSV,
        TempControlPV,
        ResinAddBoxLevel
    };

    public class PrinterMotionControl : BindableBase
    {
        public static int progressValue = 10;
        private readonly IEventAggregator _ea;
        public log4net.ILog mcLog;

        private object windowControlLock = new object();
        public PrinterMotionConfig _config { get; set; } = new PrinterMotionConfig();
        public bool _isReady = false;
        public bool _isAvaliable = true;
        public bool isAddResinByValue = true;
        //public MotionControlWindow motionControlWindow;
        //List<Projector> P_List;
        UInt32 ret = 0;
        Int32 nCardNo = 0;          // 默认要打开的卡号
        UInt64 nCardHandle = 0;     // 卡句柄
        Int16 nCrdNo = 0;           //默认插补坐标系
        Int16[] pMaskAxNo = { 0, 1, 2 };            //默认0轴绑定X，1轴绑定Y，2绑定Z
        List<ModbusTCP> rfidTCPs = new List<ModbusTCP>();
        ModbusRTU modbusRTU = new ModbusRTU();
        ReadIDCardHelper readIDCardHelper = new ReadIDCardHelper();
        SerialPort serialPortTemp = new SerialPort();
        string resintemp = string.Empty;
        string idCardNum = null;
        public PrinterMotionControl()
        {

        }
        //public class WindowEvent : PubSubEvent<bool>
        //{

        //}
        public PrinterMotionControl(IEventAggregator ea)
        {
            //P_List = projectFactory.GetProjectors();
            //motionControlWindow = new MotionControlWindow(projectFactory, this, eventAggregator);

            this._ea = ea;
            //ReadConfig();
            mcLog = log4net.LogManager.GetLogger("logMotion");
            _ea.GetEvent<PrintProgressViewEvent>().Subscribe(ppvm =>
            {

                progressValue = (int)Math.Round(19 * (double)(ppvm.progress / 100));
            });
            //_ea.GetEvent<WindowEvent>().Subscribe(isClose =>
            //{

            //    if (isClose)
            //    {
            //        if (isWindowOpen())
            //        {
            //            CloseWindow();
            //        }
            //    }
            //    else
            //    {
            //        if (isWindowClose())
            //        {
            //            OpenWindow();
            //        }
            //    }
            //});
        }

        public string GetIdCardNum()
        {
            if (idCardNum == null)
            {
                return null;
            }
            else
            {
                string temp = idCardNum;
                idCardNum = null;
                return temp;
            }
        }
        public void ResetIdCardNum()
        {
            idCardNum = null;
        }
        public bool IDC_ReaderInit()
        {
            if (readIDCardHelper.Open(_config.cardReaderConfig.com,_config.cardReaderConfig.bandRate))
            {
                readIDCardHelper.EventCardMes += new ReadIDCardHelper.SendMessage(ReadIdData);
                MessageInfoUpdate_Thread($"开始监控CardReader{_config.cardReaderConfig.com}-{_config.cardReaderConfig.bandRate}");
                return true;
            }
            else
            {
                return false;
            }
            
        }
        public void ReadIdData(string data)
        {
            idCardNum = data;
            Console.WriteLine("ID数据为：" + data);
        }
        public bool RfidInit()
        {
            foreach (object rfid in Enum.GetValues(typeof(RFIDEnum)))
            {

                ModbusTCP modbusTCP = new ModbusTCP();
                var tcpConfig = _config.RFIDConfigs.FirstOrDefault(p => p.name == rfid.ToString());
                bool Isconnectd = false;
                try
                {
                    Isconnectd = modbusTCP.Mod_Tcp_connect(tcpConfig.ip, tcpConfig.port);
                }
                catch
                {
                    return false;
                }
                

                if (Isconnectd)
                {
                    MessageInfoUpdate_Thread($"{tcpConfig.nickName}ModbusTcp已连接");

                }
                else
                {
                    MessageInfoUpdate_Thread($"{tcpConfig.nickName}ModbusTcp连接失败", LogLevel.Error);
                }
                rfidTCPs.Add(modbusTCP);
            }
            return true;
        }

        public void AddResinValueSwitch(bool open)
        {
            if (isAddResinByValue)
            {
                if (open)
                {
                    SetIO(DOEnum.ResinAdd, 1);
                }
                else
                {
                    SetIO(DOEnum.ResinAdd, 0);
                }
            }
            else
            {
                if (open)
                {
                    SetIO(DOEnum.ResinAddMotorForward, 0);
                    SetIO(DOEnum.ResinAddMotorBackward, 1);
                    while (GetIO(DIEnum.IsResinBlocked) == 1)
                    {
                        Thread.Sleep(50);
                    }
                    Thread.Sleep(1000);
                    SetIO(DOEnum.ResinAddMotorBackward, 0);
                }
                else
                {
                    SetIO(DOEnum.ResinAddMotorBackward, 0);
                    SetIO(DOEnum.ResinAddMotorForward, 1);
                    while (GetIO(DIEnum.IsResinBlocked) == 0)
                    {
                        Thread.Sleep(50);
                    }
                    
                    SetIO(DOEnum.ResinAddMotorForward, 0);
                }
            }
        }
        private void StartLightControl()
        {
            bool isOn = true;
            while (true)
            {

                int changeProgress = 50;
                int OnTime = 900;
                int OffTime = 400;


                LightControl(0, 100, 100, 0, (byte)progressValue, false);


                if (isOn)
                {
                    for (int i = 0; i < changeProgress; i++)
                    {
                        int light = (int)Math.Round((double)(i * 100 / changeProgress), 0);

                        LightControl(0, (byte)light, (byte)light, (byte)progressValue, 1, false);



                        Thread.Sleep(OnTime / changeProgress);
                    }

                    isOn = false;
                }
                else
                {
                    for (int i = 0; i < changeProgress; i++)
                    {
                        int light = (int)Math.Round((double)(100 - i * 100 / changeProgress), 0);

                        LightControl(0, (byte)light, (byte)light, (byte)progressValue, 1, false);


                        Thread.Sleep(OffTime / changeProgress);
                    }
                    isOn = true;
                }

            }
        }

        public bool WindowLock()
        {
            SetIO(DOEnum.WindowLockMotorRotate, 1);
            int waitTime = 50000;
            int count = 0;
            while (count++ * 100 < waitTime)
            {
                if (GetIO(DIEnum.WindowUnlock) == 0) {
                    SetIO(DOEnum.WindowLockMotorRotate, 0);
                    return true; }
                Thread.Sleep(100);
            }
            SetIO(DOEnum.WindowLockMotorRotate, 0);
            return false;
        }
        public bool isWindowOpen()
        {
            if (Math.Abs(GetAxisPos_Pulse(AxesEnum.Window) - _config.windowConfig.DownPos) < 30000 && GetIO(DIEnum.WindowUnlock) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool isWindowClose()
        {
            if (Math.Abs(GetAxisPos_Pulse(AxesEnum.Window) - _config.windowConfig.UpPos) < 30000 && GetIO(DIEnum.WindowUnlock) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool WindowUnlock()
        {
            SetIO(DOEnum.WindowLockMotorRotate, 1);
            int waitTime = 50000;
            int count = 0;
            while (count++ * 100 < waitTime)
            {
                if (GetIO(DIEnum.WindowUnlock) == 1) {
                    SetIO(DOEnum.WindowLockMotorRotate, 0);
                    return true; }
                Thread.Sleep(100);
            }
            SetIO(DOEnum.WindowLockMotorRotate, 0);
            return false;
        }
        public bool OpenWindow()
        {
            lock (windowControlLock)
            {
                if (GetIO(DIEnum.WindowUnlock) == 1)
                {
                    MessageInfoUpdate_Thread("升降门未锁定，请确认");
                    DialogHelper.ShowMessageDialog("错误", " 升降门未锁定，请确认!");
                    return false;
                }
                if (Math.Abs(GetAxisPos_Pulse(AxesEnum.Window) - _config.windowConfig.UpPos)> 3000)
                {
                    MessageInfoUpdate_Thread("升降门未处于关闭状态，无法打开");
                    return true;
                }
                if (!WindowUnlock())
                {
                    MessageInfoUpdate_Thread("解锁超时");
                    DialogHelper.ShowMessageDialog("错误", " 解锁超时!");
                    return false;
                }

                Thread.Sleep(300);
                Enable(AxesEnum.Window);
                Thread.Sleep(300);
                int speedInt = 800000;
                this.PTP_Pulse(AxesEnum.Window, _config.windowConfig.DownPos, speedInt, 500000, 500000);

                while (Math.Abs(GetAxisPos_Pulse(AxesEnum.Window) - _config.windowConfig.DownPos) > 3000)
                {
                    Thread.Sleep(300);
                }
                if (WindowLock())
                {
                    MessageInfoUpdate_Thread("升降门已锁定"); 
                    return true;
                }
                else
                {
                    MessageInfoUpdate_Thread("锁定超时");
                    DialogHelper.ShowMessageDialog("错误", " 锁定超时!");
                    return false;
                }
            }
            
        }

        public bool CloseWindow()
        {
            lock (windowControlLock)
            {
                if (GetIO(DIEnum.WindowUnlock) == 1)
                {
                    MessageInfoUpdate_Thread("升降门未锁定，请确认");
                    DialogHelper.ShowMessageDialog("错误", " 升降门未锁定，请确认!");
                    return false;
                }
                if (Math.Abs(GetAxisPos_Pulse(AxesEnum.Window) - _config.windowConfig.DownPos) > 3000)
                {
                    MessageInfoUpdate_Thread("升降门未处于打开状态，无法关闭");
                    return true;
                }
                if (!WindowUnlock())
                {
                    MessageInfoUpdate_Thread("解锁超时");
                    DialogHelper.ShowMessageDialog("错误", " 解锁超时!");
                    return false;
                }
            }
            Thread.Sleep(300);
            Enable(AxesEnum.Window);
            Thread.Sleep(300);
            int speedInt = 800000;
            this.PTP_Pulse(AxesEnum.Window, _config.windowConfig.UpPos, speedInt, 500000, 500000);

            while (Math.Abs(GetAxisPos_Pulse(AxesEnum.Window) - _config.windowConfig.UpPos) > 3000)
            {
                Thread.Sleep(300);
            }
            if (WindowLock())
            {
                MessageInfoUpdate_Thread("升降门已锁定");
                return true;
            }
            else
            {
                MessageInfoUpdate_Thread("锁定超时");
                DialogHelper.ShowMessageDialog("错误", " 锁定超时!");
                return false;
            }


        }
        public void StartJog(AxesEnum axes, double Vel)
        {


            ret = ImcApi.IMC_StartJogMove(nCardHandle, (short)axes, Vel * 10000.0);
            if (ret != 0)
            {
                MessageInfoUpdate_Thread(axes.ToString() + "启动JOG失败,错误代码为0x" + ret.ToString("x8"));
                return;
            }
            else
            {
                MessageInfoUpdate_Thread(axes.ToString() + "启动JOG成功");
            }
        }

        public void StopMove(AxesEnum axes)
        {


            ret = ImcApi.IMC_AxMoveStop(nCardHandle, (short)axes, 1); //StopType=1,立即停止
            if (ret != 0)
            {
                MessageInfoUpdate_Thread(axes.ToString() + "停止失败,错误代码为0x" + ret.ToString("x8"));
                return;
            }
            else
            {
                MessageInfoUpdate_Thread(axes.ToString() + "停止成功");
            }
        }
        public void ErrorClear()
        {
            UInt32 m_retRtn;
            try
            {
                m_retRtn = ImcApi.IMC_ClrAxSts(nCardHandle, 0, 1);
                m_retRtn = ImcApi.IMC_ClrAxSts(nCardHandle, 1, 1);
                m_retRtn = ImcApi.IMC_ClrAxSts(nCardHandle, 2, 1);
                m_retRtn = ImcApi.IMC_ClrAxSts(nCardHandle, 3, 1);

                if (m_retRtn != ImcApi.EXE_SUCCESS)
                {
                    MessageInfoUpdate_Thread("IMC_ClrAxSts failed 故障代码为：0x" + m_retRtn.ToString("x8"));
                }
                else
                {
                    MessageInfoUpdate_Thread("清除轴故障成功！");
                }

            }
            catch (Exception ex)
            {
                string exceptionMsg = ex.Message;
                MessageInfoUpdate_Thread(exceptionMsg);
            }
        }
        public Tuple<bool, string> RfidRead(RFIDEnum rfid)
        {
            bool IS_C = false;
            try
            {
                var rfidConfig = _config.RFIDConfigs.FirstOrDefault(p => p.name == rfid.ToString());
                string res = string.Empty;
                Tuple<bool, byte[]> value = rfidTCPs[(int)rfid].ModReadRegister(0x0a, rfidConfig.length, out IS_C);

                if (!value.Item1) { return new Tuple<bool, string>(false, string.Empty); }
                if (!IS_C)
                {
                    MessageInfoUpdate_Thread($"{rfidConfig.nickName}Socket已断开");
                    return new Tuple<bool, string>(false, string.Empty);
                }
                else
                {
                    return new Tuple<bool, string>(true, Encoding.UTF8.GetString(value.Item2, 0, value.Item2.Length));
                }
            }
            catch (Exception e)
            {
                MessageInfoUpdate_Thread(e.ToString());
                return new Tuple<bool, string>(false, string.Empty);
            }
        }
        public bool RfidWrite(RFIDEnum rfid, string content)
        {
            bool IS_C = false;
            try
            {
                var rfidConfig = _config.RFIDConfigs.FirstOrDefault(p => p.name == rfid.ToString());
                //if (content.Length != 2 * rfidConfig.length )
                //{
                //    return false;
                //}
                byte[] sendByte = new byte[rfidConfig.length * 2];
                if (rfidConfig.length * 2 <= Encoding.UTF8.GetBytes(content).Length)
                { Array.Copy(Encoding.UTF8.GetBytes(content), sendByte, rfidConfig.length * 2); }
                else
                { Encoding.UTF8.GetBytes(content).CopyTo(sendByte, 0); }
                // Encoding.UTF8.GetBytes(content).CopyTo(sendByte, 0);
                IS_C = rfidTCPs[(int)rfid].ModWriteRegister(0X0A, sendByte);


                if (!IS_C)
                {
                    MessageInfoUpdate_Thread($"{rfidConfig.nickName}Socket已断开");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageInfoUpdate_Thread(e.ToString());
                return false;
            }
        }
        public void LightControl(byte R, byte G, byte B, byte index, byte num, bool all)
        {
            try
            {
                byte[] RGBBytes = new byte[60];
                for (int i = 0; i < (int)num; i++)
                {
                    RGBBytes[(index + i) * 3] = R;
                    RGBBytes[(index + i) * 3 + 1] = G;
                    RGBBytes[(index + i) * 3 + 2] = B;
                }

                if (all)
                {
                    for (int i = 0; i < 20; i++)
                    {

                        RGBBytes[3 * i] = R;
                        RGBBytes[3 * i + 1] = G;
                        RGBBytes[3 * i + 2] = B;
                    }
                }

                byte[] SendBytes1 = { 0XDD, 0X55, 0XEE, 0X00, 0X00, 0X00, 0X01, 0X00, 0X99, 0X02, 0X00, 0X00, 0X00, 0X3C, 0X00, 0X01 };
                byte[] SendBytes = new byte[SendBytes1.Length + 62];
                SendBytes1.CopyTo(SendBytes, 0);
                RGBBytes.CopyTo(SendBytes, 16);
                SendBytes[76] = 0XAA;
                SendBytes[77] = 0XBB;
                modbusRTU.LightControl(SendBytes);
                
            }
            catch (Exception E)
            {
                //message(E.ToString());
            }

        }

        public bool ReadConfig()
        {
            //如何判断配置文件无问题？？
            //1、正常读入
            //2、MD5校验
            //3、否则状态变红
            try
            {
                if (!File.Exists(ClassValue.MotionControlConfigFile))//if (!ClassValue.CheckMD5(ClassValue.MotionControlConfigFile, ClassValue.MotionControlConfigMD5))
                {
                    _isAvaliable = false;
                    return false;
                }
                using (FileStream fs = new FileStream(ClassValue.MotionControlConfigFile, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string jsonstr = sr.ReadToEnd();
                        _config = Newtonsoft.Json.JsonConvert.DeserializeObject<PrinterMotionConfig>(jsonstr);
                    }
                }
                if (false)//if (!ClassValue.CheckMD5(_config.baseCalibParas.configInfo.path, _config.baseCalibParas.configInfo.MD5))
                {
                    _isAvaliable = false;
                    return false;
                }
                //using (FileStream fs = new FileStream(_config.baseCalibParas.configInfo.path, FileMode.Open))
                //{
                //    using (StreamReader sr = new StreamReader(fs))
                //    {
                //        string jsonstr = sr.ReadToEnd();
                //        _config.baseCalibParas.baseCalib = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseCalib>(jsonstr);
                //    }
                //}
               
                //确认所有的AD都有配置
                //config里有所有的ADconfigs，需要通过循环，确认所有AD是否都被配置到了，并记录indexs
                List<ADConfig> tempConfigs = new List<ADConfig>();
                foreach (object o in Enum.GetValues(typeof(ADEnum)))
                {
                    string test = o.ToString();
                    var cfgs = _config.ADconfigs.FindAll(p => p.name == o.ToString()).ToList();
                    if (cfgs == null||cfgs.Count ==0) {
                        _isAvaliable = false;
                        return false;
                    }
                    foreach (var cfg in cfgs)
                    {
                        tempConfigs.Add((ADConfig)cfg);
                    }
                }
                foreach (object o in Enum.GetValues(typeof(ModBusWriteEnum)))
                {
                    string test = o.ToString();
                    var cfgs = _config.modBusConfig.modBusWriteConfigs.FindAll(p => p.name == o.ToString()).ToList();
                    if (cfgs == null || cfgs.Count == 0)
                    {
                        _isAvaliable = false;
                        return false;
                    }
                }
                foreach (object o in Enum.GetValues(typeof(ModBusReadEnum)))
                {
                    string test = o.ToString();
                    var cfgs = _config.modBusConfig.modBusReadConfigs.FindAll(p => p.name == o.ToString()).ToList();
                    if (cfgs == null || cfgs.Count == 0)
                    {
                        _isAvaliable = false;
                        return false;
                    }
                }
                //确认DI DO
                if (_config.DOconfigs.FirstOrDefault(p => p.name == DOEnum.ResinAdd.ToString()) != null)
                {
                    //补液阀
                    isAddResinByValue = true;
                    
                }
                else
                {
                    //补液闸
                  
                    if (_config.DOconfigs.FirstOrDefault(p => p.name == DOEnum.ResinAddMotorForward.ToString()) != null)
                    {
                        isAddResinByValue = false;
                        //确认换向和闭合检测配置是否存在
                        if (_config.DOconfigs.FirstOrDefault(p => p.name == DOEnum.ResinAddMotorBackward.ToString()) == null|| _config.DIconfigs.FirstOrDefault(p => p.name == DIEnum.IsResinBlocked.ToString()) == null)
                        {
                            _isAvaliable = false;
                            return false;
                        }
                    }
                    else
                    {
                        _isAvaliable = false;
                        return false;
                    }
                }
                foreach (object o in Enum.GetValues(typeof(DIEnum)))
                {
                    if (o.ToString() == DIEnum.IsResinBlocked.ToString()) { continue; }
                        
                    var cfgs = _config.DIconfigs.FindAll(p => p.name == o.ToString()).ToList();
                    
                    if (cfgs == null || cfgs.Count == 0)
                    {
                        _isAvaliable = false;
                        return false;
                    }
                }
                _config.ADconfigs = tempConfigs;
            }
            catch (Exception ex)
            {
                _ea.GetEvent<MessageEvent>().Publish(ex.ToString());
                _isAvaliable = false;
                MessageInfoUpdate_Thread(ex.ToString());
                return false;
            }
            return true;
        }
        public bool Init()
        {


            try
            {
                
                // 用于接收API返回值
                _isReady = false;

                //【1】获取卡
                Int32 nCardNum = 0;
                ret = ImcApi.IMC_GetCardsNum(ref nCardNum);
                
                if (ret != 0)
                {
                    MessageInfoUpdate_Thread("获取轴卡失败,错误代码为0x" + ret.ToString("x8"), LogLevel.Warn);
                    return false;
                }
                else
                {
                    if (nCardNum <= 0)
                    {
                        MessageInfoUpdate_Thread("没有找到卡", LogLevel.Warn);
                        return false;
                    }
                    else
                    {
                        MessageInfoUpdate_Thread("获取轴卡成功,将默认打开第0号卡");
                    }
                }

                //【2】打开卡句柄
                ret = ImcApi.IMC_OpenCardHandle(nCardNo, ref nCardHandle);
                if (ret != 0)
                {
                    MessageInfoUpdate_Thread("获取卡句柄失败,错误代码为0x" + ret.ToString("x8"), LogLevel.Warn);
                    return false;
                }
                else
                {
                    MessageInfoUpdate_Thread("获取轴卡句柄成功");
                }

                //【3】下载设备参数
                ret = ImcApi.IMC_DownLoadDeviceConfig(nCardHandle, _config.deviceConfig.path);
                if (ret != 0)
                {
                    MessageInfoUpdate_Thread("下载设备参数失败,错误代码为0x" + ret.ToString("x8") + ",请检查是否有" + _config.deviceConfig.path + "文件", LogLevel.Warn);
                    return false;
                }
                else
                {
                    MessageInfoUpdate_Thread("下载设备参数成功");
                }

                //【4】启动主站
                MessageInfoUpdate_Thread("正在启动EtherCAT...请稍候...");

                uint masterStatus = 0;
                ret = ImcApi.IMC_GetECATMasterSts(nCardHandle, ref masterStatus);
                if (ret != 0)
                {
                    MessageInfoUpdate_Thread("获取主站状态失败,错误代码为0x" + ret.ToString("x8"), LogLevel.Warn);
                    return false;
                }
                else
                {
                    if (masterStatus != ImcApi.EC_MASTER_OP)
                    {
                        ret = ImcApi.IMC_ScanCardECAT(nCardHandle, 1);      //默认阻塞式启动EtherCAT
                        if (ret != 0)
                        {
                            MessageInfoUpdate_Thread("启动EtherCAT失败,错误代码为0x" + ret.ToString("x8"), LogLevel.Warn);
                            return false;
                        }
                    }
                }
                MessageInfoUpdate_Thread("启动EtherCAT成功");

                //【5】下载系统参数
                ret = ImcApi.IMC_DownLoadSystemConfig(nCardHandle, _config.systemConfig.path);
                if (ret != 0)
                {
                    MessageInfoUpdate_Thread("下载系统参数失败,错误代码为0x" + ret.ToString("x8") + ",请检查是否有" + _config.systemConfig.path + "文件", LogLevel.Warn);
                    return false;
                }
                else
                {
                    MessageInfoUpdate_Thread("下载系统参数成功");
                }

                //Task.Run(() =>
                //{
                //    StartLightControl();
                //});
                if (!RfidInit()) {
                    MessageInfoUpdate_Thread("RFID连接失败");
                    return false; }
                if (!IDC_ReaderInit()) {
                    MessageInfoUpdate_Thread("读卡器连接失败,仅可使用HID设备"); }
                if (!modbusRTU.OpenCom(_config.modBusConfig.com, _config.modBusConfig.baudRate, _config.modBusConfig.dataBits, (StopBits)_config.modBusConfig.stopBits, (Parity)_config.modBusConfig.parity))
                {
                    MessageInfoUpdate_Thread("ModbusRTU连接失败");
                    return false; }
                _isReady = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageInfoUpdate_Thread(ex.ToString());
                return false;
            }



        }

        public double GetModBusAD(ModBusReadEnum modBusReadEnum,bool isSimulate =  false)
        {
            if (!isSimulate)
            {
                var config = _config.modBusConfig.modBusReadConfigs.FirstOrDefault(p => p.name == modBusReadEnum.ToString());
                if (config == null) { return -1; }
                if (!modbusRTU.serialPort1.IsOpen)
                {
                    if (modbusRTU.serialPort1.PortName != null)
                    {
                        modbusRTU.CloseCom();
                        modbusRTU.OpenCom(_config.modBusConfig.com, _config.modBusConfig.baudRate, _config.modBusConfig.dataBits, (StopBits)_config.modBusConfig.stopBits, (Parity)_config.modBusConfig.parity);
                    }
                }

                
                if (modbusRTU.serialPort1.IsOpen)
                {
                    
                    short[] short1 = modbusRTU.ReaderInputRegister((byte)config.station, (short)config.address, (short)config.length, (byte)config.funcCode);
                    double temp1 = double.Parse(short1[0].ToString());
                    return AD_Trans(temp1, config);
                }
                else
                {
                    return -1;
                }
            }
            else { return AD_Simulate(modBusReadEnum); }
        }
        public bool SetModBus(ModBusWriteEnum modBusWriteEnum,short value, bool isSimulate = false)
        {
            if (!isSimulate)
            {
                var config = _config.modBusConfig.modBusWriteConfigs.FirstOrDefault(p => p.name == modBusWriteEnum.ToString());
                if (config == null) { return false; }
                if (modbusRTU.serialPort1.IsOpen)
                {
                    modbusRTU.WriteOneHoldingRegister((byte)config.station, (short)config.address, value, (byte)config.funcCode);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else { return true; }
        }
       
        //public void OpenCom(SerialPort sp, string STR)
        //{
        //    try
        //    {
        //        sp.PortName = STR;
        //        sp.BaudRate = Convert.ToInt32("9600", 10);
        //        sp.DataBits = Convert.ToInt32("8", 10);
        //        sp.Open();
        //        //listshow("COM14已打开");//打开串口
        //        MessageInfoUpdate_Thread(STR + "已打开");
        //        sp.DataReceived += new SerialDataReceivedEventHandler(post_DataReceived);
        //    }
        //    catch (Exception E)
        //    {
        //        MessageInfoUpdate_Thread(E.ToString());
        //    }
        //}
        //public void CloseCom(SerialPort sp)
        //{
        //    try
        //    {
        //        sp.Close();
        //    }
        //    catch (Exception E)
        //    {
        //        MessageInfoUpdate_Thread(E.ToString());
        //    }
        //}
        //public void SendCom()
        //{
        //    try
        //    {
        //        byte[] Sendbyte = { 0X01, 0X03, 0X10, 0X01, 0X00, 0X01, 0XD1, 0X0A };
        //        serialPortTemp.Write(Sendbyte, 0, Sendbyte.Length);
        //        //listshow(Sendbyte[0].ToString());
        //        //MessageInfoUpdate_Thread(Sendbyte.ToString());
        //    }
        //    catch { }
        //}



        private void post_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //string str1=serialPort1.ReadExisting();

            Thread.Sleep(60);
            byte[] Rcvbyte = new byte[serialPortTemp.BytesToRead];
            serialPortTemp.Read(Rcvbyte, 0, Rcvbyte.Length);
            string str = Encoding.UTF8.GetString(Rcvbyte);
            if (Rcvbyte.Length < 5)
            { resintemp = "0"; }
            resintemp = ((short)Rcvbyte[3] * 255 + (short)Rcvbyte[4]).ToString();
        }
        public bool Close()
        {

            _isReady = false;
            if (nCardHandle != 0)
            {
                ret = ImcApi.IMC_CloseCard(nCardHandle);
                if (ret != 0)
                {
                    MessageInfoUpdate_Thread("关卡失败,错误代码为0x" + ret.ToString("x8"), LogLevel.Warn);
                    return false;
                }
                else
                {
                    MessageInfoUpdate_Thread("关卡成功");
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public double getScraperZ(int nX)
        {
            UInt32 m_retRtn;
            byte[] target = new byte[4];
            uint result_size = 0;
            uint abot_code = 0;
            ushort n_X = (ushort)nX;
            //ushort n_X = 0x8011;
            //if (!LeftOrNot)
            //{
            //    n_X = 0x8001;
            //}
            m_retRtn = ImcApi.IMC_GetEcatSdo(nCardHandle, 5, n_X, 6, target, 4, ref result_size, ref abot_code);
            Int32 combined = (((Int32)(target[3]) << 24) | (((Int32)target[2]) << 16) | (((Int32)target[1]) << 8) | ((Int32)target[0]));
            return combined / 1000.0;
        }
        public void MessageInfoUpdate(string info, bool infoOrError = true)
        {
            _ea.GetEvent<MessageEvent>().Publish(info);

        }
        private void MessageInfoUpdate_Thread(string info, LogLevel logLevel = LogLevel.Info)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    mcLog.Info(info);
                    break;
                case LogLevel.Debug:
                    mcLog.Debug(info);
                    break;
                case LogLevel.Warn:
                    mcLog.Warn(info);
                    break;
                case LogLevel.Error:
                    mcLog.Error(info);
                    break;
                case LogLevel.Fatal:
                    mcLog.Fatal(info);
                    break;
            }
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageInfoUpdate(info);
            }));
        }

        public bool BackOri(AxesEnum axesEnum)
        {


            //【2】写入回原参数
            ImcApi.THomingPara tHomingPara = new ImcApi.THomingPara();
            tHomingPara.homeMethod = 27;
            tHomingPara.offset = 0;
            tHomingPara.highVel = 100000;
            tHomingPara.lowVel = 2000;
            tHomingPara.acc = 1000000;
            tHomingPara.overtime = 120000;
            tHomingPara.posSrc = 0;

            //【3】开始回原
            ret = ImcApi.IMC_StartHoming(nCardHandle, Convert.ToInt16(axesEnum), ref tHomingPara);
            if (ret != 0)
            {
                MessageInfoUpdate_Thread($"{CommonDefinition.AxesEnumTrans[(int)axesEnum]}开始回原失败,错误代码为0x{ret.ToString("x8")}", LogLevel.Warn);

                return false;
            }
            else
            {
                MessageInfoUpdate_Thread("伺服回原中！");
                return true;
            }
        }
        public bool Enable(AxesEnum axesEnum, bool isSimulate = false)
        {
            if (!isSimulate)
            {
                ret = ImcApi.IMC_AxServoOn(nCardHandle, Convert.ToInt16(axesEnum), 1);
                if (ret != 0)
                {
                    MessageInfoUpdate_Thread($"{CommonDefinition.AxesEnumTrans[(int)axesEnum]}上使能失败,错误代码为0x{ret.ToString("x8")}", LogLevel.Warn);
                    _isReady = false;
                    return false;
                }
                else
                {
                    MessageInfoUpdate_Thread($"{CommonDefinition.AxesEnumTrans[(int)axesEnum]}上使能成功");
                    _isReady = true;
                    return true;
                }
            }
            return true;

        }
        public bool Disable(AxesEnum axesEnum)
        {

            ret = ImcApi.IMC_AxServoOff(nCardHandle, Convert.ToInt16(axesEnum), 1);
            if (ret != 0)
            {
                MessageInfoUpdate_Thread($"{CommonDefinition.AxesEnumTrans[(int)axesEnum]}下使能失败,错误代码为0x{ret.ToString("x8")}", LogLevel.Warn);
                return false;
            }
            else
            {
                MessageInfoUpdate_Thread($"{CommonDefinition.AxesEnumTrans[(int)axesEnum]}下使能成功");
                return true;
            }

        }
        public double GetAxisPos_Pulse(AxesEnum axesEnum)
        {
            double[] nTimerAxEncPos = { 0 };
            ret = ImcApi.IMC_GetAxEncPos(nCardHandle, Convert.ToInt16(axesEnum), nTimerAxEncPos, 1);
            if (ret != 0)
            {

                MessageInfoUpdate_Thread("轴状态监控，查询轴反馈位置失败,错误代码为0x" + ret.ToString("x8"), LogLevel.Warn);
                return -999;
            }
            return nTimerAxEncPos[0];
        }
        double GET_Zero(AxesEnum axesEnum)
        {
            switch (axesEnum)
            {
                case AxesEnum.Levelbox:
                    return _config.levelBoxConfig.zeroPos;
                case AxesEnum.Scraper:
                    return _config.scraperConfig.zeroPos;
                case AxesEnum.Plaform:
                    return _config.platformConfig.zeroPos;
                case AxesEnum.Window:
                    return _config.windowConfig.zeroPos;
            }
            return -1;
        }
        double GET_Lower(AxesEnum axesEnum)
        {
            switch (axesEnum)
            {
                case AxesEnum.Levelbox:
                    return _config.levelBoxConfig.startPos;
                case AxesEnum.Scraper:
                    return _config.scraperConfig.startPos;
                case AxesEnum.Plaform:
                    return _config.platformConfig.startPos;
                case AxesEnum.Window:
                    return _config.windowConfig.startPos;
            }
            return -1;
        }
        double GET_Upper(AxesEnum axesEnum)
        {
            switch (axesEnum)
            {
                case AxesEnum.Levelbox:
                    return _config.levelBoxConfig.endPos;
                case AxesEnum.Scraper:
                    return _config.scraperConfig.endPos;
                case AxesEnum.Plaform:
                    return _config.platformConfig.endPos;
                case AxesEnum.Window:
                    return _config.windowConfig.endPos;
            }
            return -1;
        }
        public double GetAxisPos_MM(AxesEnum axesEnum)
        {
            double tempPos = GetAxisPos_Pulse(axesEnum);
            return (tempPos - GET_Zero(axesEnum)) / 10000;
        }
        public void PTP_Pulse(AxesEnum axesEnum, double ptpPos, double ptpVel, double ptpAcc, double ptpDec)
        {
            if (ptpPos <= GET_Lower(axesEnum) || ptpPos >= GET_Upper(axesEnum))
            {
                MessageInfoUpdate_Thread($"{CommonDefinition.AxesEnumTrans[(int)axesEnum]}设置PTP运行参数失败,行程超限", LogLevel.Warn);
                return;
            }
            double[] nTimerAxEncPos = { 0 };
            ret = ImcApi.IMC_SetSingleAxMvPara(nCardHandle, Convert.ToInt16(axesEnum), ptpVel, ptpAcc, ptpDec);
            if (ret != 0)
            {

                MessageInfoUpdate_Thread($"{CommonDefinition.AxesEnumTrans[(int)axesEnum]}设置PTP运行参数失败,错误代码为0x{ret.ToString("x8")}", LogLevel.Warn);
                return;
            }
            else
            {
                //MessageInfoUpdate_Thread($"{ CommonDefinition.AxesEnumTrans[(int)axesEnum]}设置PTP运行参数成功");
            }

            //【3】启动PTP
            ret = ImcApi.IMC_StartPtpMove(nCardHandle, Convert.ToInt16(axesEnum), ptpPos, 0);
            if (ret != 0)
            {
                MessageInfoUpdate_Thread("启动PTP失败,错误代码为0x" + ret.ToString("x8"), LogLevel.Warn);
                return;
            }
            else
            {
                //MessageInfoUpdate_Thread($"{ CommonDefinition.AxesEnumTrans[(int)axesEnum]}启动PTP成功");
            }
        }
        public void PTP_MM(AxesEnum axesEnum, double ptpPos, double ptpVel, double ptpAcc, double ptpDec, bool isSimulate = false)
        {
            if (!isSimulate)
            {
                double tempPulse = (10000 * ptpPos + GET_Zero(axesEnum));
                PTP_Pulse(axesEnum, tempPulse, ptpVel, ptpAcc, ptpDec);
            }


        }
        public void PTP_Pos_Speed_mm(AxesEnum axesEnum, double ptpPos, double ptpVel, bool isSimulate = false)
        {
            if (!isSimulate)
            {
                //if(axesEnum == AxesEnum.Scraper)
                double tempPulse = (10000 * ptpPos + GET_Zero(axesEnum));

                double ptpAcc = 1000000;
                double ptpDec = 1000000;
                PTP_Pulse(axesEnum, tempPulse, 10000 * ptpVel, ptpAcc, ptpDec);
            }


        }
        public bool MoveScraper2Side(bool inORout, double speed = 130, bool isSimulate = false)
        {
            int speedInt = (int)Math.Round(10000 * speed);
            double estimate = 500 / speed;
            if (!isSimulate)
            {
                if (inORout)
                {
                    this.PTP_Pulse(AxesEnum.Scraper, _config.scraperConfig.inPos, speedInt, 5000000, 5000000);
                }
                else
                {
                    this.PTP_Pulse(AxesEnum.Scraper, _config.scraperConfig.outPos, speedInt, 5000000, 5000000);
                }
                int count = 0;
                double timeInterval = 0.2;
                while (!IsScraper2Side(inORout))
                {
                    if ((count++) * timeInterval > (estimate + 3))
                    {
                        return false;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(timeInterval));
                }
                return true;
            }
            else
            {
                return true;
            }
        }
        private bool isScrpterOut()
        {
            if (Math.Abs(GetAxisPos_Pulse(AxesEnum.Scraper) - _config.scraperConfig.outPos) < 2000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool isScrpterIn()
        {
            if (Math.Abs(GetAxisPos_Pulse(AxesEnum.Scraper) - _config.scraperConfig.inPos) < 10000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public PlatformTestResult MoveScraperInAndTest(double platformPos,double speed = 130,bool isSimulate = false)
        {

            if (!isScrpterOut()) { return new PlatformTestResult() { canBeTested = false }; };
            if (Math.Abs(GetAxisPos_MM(AxesEnum.Plaform) - platformPos) >0.1)
            {
                return new PlatformTestResult() { canBeTested = false };
            }
            PlatformTestResult result = new PlatformTestResult();
            result.canBeTested = true;
            result.zInitComp = platformPos;
            foreach (var zs in _config.scraperConfig.zSensers)
            {
                result.platformTestItems.Add(new List<PlatformTestItem>());
                if (!isSimulate&&zs.isMotive)
                {
                    //使能电磁铁
                    SetIO(zs.moveIOport, 1);
                }
            }
            Thread.Sleep(200);
            
            int speedInt = (int) Math.Round( 10000 * speed);
            if (!isSimulate)
            {
                this.PTP_Pulse(AxesEnum.Scraper, _config.scraperConfig.inPos, speedInt, 5000000, 5000000);
                int waitCount = 0;
                while (!isScrpterIn())
                {
                    if (waitCount++ > 100) { return new PlatformTestResult() { canBeTested = false }; }
                    double y = (_config.scraperConfig.testPos - GetAxisPos_Pulse(AxesEnum.Scraper)) / 10000;
                    int list = 0;
                    foreach (var zs in _config.scraperConfig.zSensers)
                    {
                        result.platformTestItems[list++].Add(new PlatformTestItem()
                        {
                            plateformY = y,
                            zValue = getScraperZ(zs.port)
                        });
                    }

                    Thread.Sleep(100);
                }

                foreach (var zs in _config.scraperConfig.zSensers)
                {
                    if (zs.isMotive)
                    {
                        //使能电磁铁
                        SetIO(zs.moveIOport, 0);
                    }
                }
            }
            return result;
            
        }

        

        public bool IsScraper2Side(bool inORout)
        {
            double  currPos = GetAxisPos_Pulse(AxesEnum.Scraper);
            if (currPos == -999)
                { return false; }
            int targetPos = _config.scraperConfig.outPos;
            if (inORout)
            {
                targetPos = _config.scraperConfig.inPos;
            }
            if (Math.Abs(currPos - targetPos) < 1000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private double AD_Trans(double value, ADConfig ADc)
        {
            return ADc.k1* value + ADc.k0;
        }

        private double AD_Trans(double value, ModBusReadConfig ADc)
        {
            return ADc.k1 * value + ADc.k0;
        }
        private double AD_Simulate(ADEnum aDEnum)
        {
            Random rd = new Random();
            switch (aDEnum)
            {
                case ADEnum.Level:
                    return 0.7;
                case ADEnum.ScraperVacuum:
                    return 260 ;
                case ADEnum.ResinTemp:
                    return 24;
                case ADEnum.TankTemp:
                    return 24.4;
            }
            return -1;
        }
        private double AD_Simulate(ModBusReadEnum aDEnum)
        {
            Random rd = new Random();
            switch (aDEnum)
            {
                case ModBusReadEnum.TempControlPV:
                    return 25.7;
                case ModBusReadEnum.TempControlSV:
                    return 24;
                case ModBusReadEnum.ResinAddBoxLevel:
                    return 24;
            }
            return -1;
        }
        private static readonly object _locked = new object();
        public double GetAD(ADEnum aDEnum, bool isSimulate = false, int sn = 0)
        {
            if (!isSimulate)
            {
                var config = _config.ADconfigs.FirstOrDefault(p => p.name == aDEnum.ToString() && p.sn == sn);
                if (config == null) { return -1; }
                //单通道 
                if (config.isSingle)
                {
                    Int16 t_AdValue = 0;

                    ret = ImcApi.IMC_GetEcatAdVal(nCardHandle, Convert.ToInt16(config.port), ref t_AdValue);          //t_AdValue为数字量，需要根据输入类型以及相应对应关系转换为模拟量数据

                    return AD_Trans(t_AdValue, config);
                }
                else
                {
                    Int16 t_AdValue = 0;
                    Int16 t_AdValue1 = 0;
                    ret = ImcApi.IMC_GetEcatAdVal(nCardHandle, Convert.ToInt16(config.port), ref t_AdValue);
                    ret = ImcApi.IMC_GetEcatAdVal(nCardHandle, Convert.ToInt16(config.port + 1 ), ref t_AdValue1);
                    short[] SHORT1 = new short[2];
                    SHORT1[0] = t_AdValue;
                    SHORT1[1] = t_AdValue1;
                    byte[] byte1 = Data_Handling.shortTObyte(SHORT1, 2);
                    float[] float1 = Data_Handling.byteTOfloat(byte1, 1);
                    float float2 = Data_Handling.highTolow_F(float1[0]);
                    return AD_Trans(float2, config);
                }
            }
            return AD_Simulate(aDEnum);
        }

        public void SetIO(DOEnum dOEnum, int bit)
        {
            
            var io = _config.DOconfigs.FirstOrDefault(p => p.name == dOEnum.ToString());

            Int16 nEcatIoBitValue = (short)bit;

            ret = ImcApi.IMC_SetEcatDoBit(nCardHandle, (short)io.port, nEcatIoBitValue);
            if (ret != 0)
            {
                MessageInfoUpdate_Thread("按点设置ECAT DO失败,错误代码为0x" + ret.ToString("x8"),LogLevel.Warn);
                return;
            }
            else
            {
                //MessageInfoUpdate("按点设置ECAT DO成功");
            }
        }

        public void SetIO(int port, int bit)
        {

            

            Int16 nEcatIoBitValue = (short)bit;

            ret = ImcApi.IMC_SetEcatDoBit(nCardHandle, (short)port, nEcatIoBitValue);
            if (ret != 0)
            {
                MessageInfoUpdate_Thread("按点设置ECAT DO失败,错误代码为0x" + ret.ToString("x8"), LogLevel.Warn);
                return;
            }
            else
            {
                //MessageInfoUpdate("按点设置ECAT DO成功");
            }
        }

        public int GetIO(DIEnum diEnum)
        {
            var io = _config.DIconfigs.FirstOrDefault(p => p.name == diEnum.ToString());

            Int16 nEcatIoBitValue = 0;

            ret = ImcApi.IMC_GetEcatDiBit(nCardHandle, (short)io.port, ref nEcatIoBitValue);
            return nEcatIoBitValue;
        }
    }
    

    public class CalibCamGet
    {

        public List<string> Devices { get; set; }
        public HkCamera camera;
        public bool isleft;
        public CalibCamGet(bool isLeft)
        {
            Devices = new List<string>();
            var cameraList = new List<string>();
            camera = new HkCamera();
            camera.GetCameraList();
            //左右只对投影仪与校准架的初始相对位置有影响，各设备之间的方位关系都是一样的
            isleft = isLeft;
            //初始化
        }
        ~CalibCamGet()
        {
            camera.Dispose();
            // 这里是清理非托管资源的用户代码段
        }
    }

    public class Calib
    {
        public List<string> Devices { get; set; }
        public HkCamera camera;
        public bool isleft;
        //List<CAMInfo> CAMList;

        List<CalibItem> calibItems = new List<CalibItem>();

        //DenseMatrix Cam2AtCam1 = new DenseMatrix(1, 4, new double[4] { 0, 0, 0,1 });

        MathNet.Numerics.LinearAlgebra.Double.Matrix ProjAtCam1_Proj = new DenseMatrix(4, 1, new double[4] { 0, 0, 0, 1 });

        DenseMatrix ProjAtCam1_Cam1 = new DenseMatrix(4, 1, new double[4] { 0, 0, 0, 1 });
        DenseMatrix ProjOriAtCalib = new DenseMatrix(4, 1, new double[4] { 0, 0, 0, 1 });
        double gussProjectScale = 0.098477778;
        DenseMatrix ProjAtCaliPos_Cali = new DenseMatrix(4, 1, new double[4] { 0, 0, 0, 1 });
        DenseMatrix CamCenterOffset = new DenseMatrix(4, 1, new double[4] { 3.7, 0.8, 0, 1 });
        DenseMatrix ProjAtCaliPos_CaliProj = new DenseMatrix(4, 1, new double[4] { 0, 0, 0, 1 });
        public MathNet.Numerics.LinearAlgebra.Double.Matrix Cam1ToCali = new DenseMatrix(4, 4, new double[16] {
            Math.Cos(Math.PI/2), Math.Sin(Math.PI/2), 0, 0,
            -Math.Sin(Math.PI/2), Math.Cos(Math.PI/2), 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1 });

        public DenseMatrix Cam2ToCali = new DenseMatrix(4, 4, new double[16] {
            Math.Cos(Math.PI/2), Math.Sin(Math.PI/2), 0, 0,
            -Math.Sin(Math.PI/2), Math.Cos(Math.PI/2), 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1 });


        DenseMatrix ProjToCali = new DenseMatrix(4, 4, new double[16] {
            Math.Cos(Math.PI), 0 , Math.Sin(Math.PI),0,
            0,1,0,0,
            -Math.Sin(Math.PI), 0, Math.Cos(Math.PI), 0,
            0,0,0, 1 });

        
        public class CalibItem
        {
            public bool useCam1;
            public int projX;
            public int projY;
            public double calibX;
            public double calibY;
            public double spotResultX;
            public double spotResultY;
            
        }
        public class CAMInfo
        {
            public string camName { get; set; }
            public int index { get; set; }
        }
        public Calib(bool isLeft)
        {
            Devices = new List<string>();
            var cameraList = new List<string>();
            camera = new HkCamera();
            camera.GetCameraList();
            //左右只对投影仪与校准架的初始相对位置有影响，各设备之间的方位关系都是一样的
            isleft = isLeft;
            //初始化
            var info = ConfigurationManager.AppSettings["Cam2AtCam1"];
            string[] infoSplited = info.Split(',');
            Cam2ToCali[0, 3] = Convert.ToDouble(infoSplited[0]);
            Cam2ToCali[1, 3] = Convert.ToDouble(infoSplited[1]);
            Console.WriteLine(Cam2ToCali.ToString());
            if (isleft)
            {
                info = ConfigurationManager.AppSettings["LeftProjPointAtCam1"];
                infoSplited = info.Split(',');
            }
            else
            {
                info = ConfigurationManager.AppSettings["RightProjCenterInCam1"];
                infoSplited = info.Split(',');
            }
            ProjAtCam1_Proj[0, 0] = Convert.ToDouble(infoSplited[0]);
            ProjAtCam1_Proj[1, 0] = Convert.ToDouble(infoSplited[1]);
            ProjAtCam1_Cam1[0, 0] = Convert.ToDouble(infoSplited[2]);
            ProjAtCam1_Cam1[1, 0] = Convert.ToDouble(infoSplited[3]);
            ProjAtCaliPos_Cali = GetSpotAtCalib(true, new DenseMatrix(4, 1, new double[4] { 85, -25, 0, 1 }), ProjAtCam1_Cam1);
            //ProjAtCaliPos_CaliProj = (DenseMatrix)ProjAtCam1_Proj * gussProjectScale;
            //Console.WriteLine(GetSpotAtCalib(false, new DenseMatrix(4, 1, new double[4] { 0, 170, 0, 1 }), new DenseMatrix(4, 1, new double[4] { ProjAtCam1_Cam1[0, 0] * 0.00345, ProjAtCam1_Cam1[0, 1] * 0.00345, 0, 1 })).ToString());
            DenseMatrix temp = ProjectorPixRotateToCalib((DenseMatrix)ProjAtCam1_Proj);
            ProjOriAtCalib = ProjAtCaliPos_Cali - temp;

            //GenerateCalibData();
        }
        ~Calib()
        {
            camera.Dispose();
            // 这里是清理非托管资源的用户代码段
        }
        public void GenerateCalibData()
        {

        }
        private static Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat> MyFindContoursEllipse(Mat srcImage)
        {
            //转化为灰度图
            Mat src_gray = new Mat();
            Cv2.CvtColor(srcImage, src_gray, ColorConversionCodes.RGB2GRAY);
            Mat thImage = new Mat(srcImage.Height, srcImage.Width, new MatType(MatType.CV_8UC3));
            Cv2.Threshold(src_gray, thImage, 230, 255, ThresholdTypes.Binary);
            //thImage.SaveImage("D:\\test.bmp");
            Mat open = new Mat(); Mat close = new Mat();
            Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            Cv2.MorphologyEx(thImage, open, MorphTypes.Open, element);
            Cv2.MorphologyEx(open, close, MorphTypes.Close, element);

            //获得轮廓
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchly;
            Cv2.FindContours(close, out contours, out hierarchly, RetrievalModes.External, ContourApproximationModes.ApproxNone, new OpenCvSharp.Point(0, 0));
            if (contours.Length > 0)
            {
                RotatedRect rot = Cv2.FitEllipse(contours[contours.Length - 1]);
                ////将结果画出并返回结果
                Mat dst_Image = Mat.Zeros(src_gray.Size(), srcImage.Type());
                Random rnd = new Random();
                for (int i = 0; i < contours.Length; i++)
                {
                    Scalar color = new Scalar(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
                    Cv2.DrawContours(dst_Image, contours, i, color, 2, LineTypes.Link8, hierarchly);
                }
                OpenCvSharp.Point center = new OpenCvSharp.Point(rot.Center.X, rot.Center.Y);
                OpenCvSharp.Size size = new OpenCvSharp.Size((int)rot.Size.Width / 2, (int)rot.Size.Height / 2);
                Cv2.Ellipse(dst_Image, center, size, rot.Angle, 0, 360, Scalar.AliceBlue);
                return new Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat>(center, size, dst_Image);
            }
            else { return new Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat>(new OpenCvSharp.Point(0, 0), new OpenCvSharp.Size(0, 0), srcImage); }


        }
        public DenseMatrix ProjectorPixRotateToCalib(DenseMatrix ProjectPos)
        {

            DenseMatrix res = new DenseMatrix(4, 1, new double[4] { 0, 0, 0, 1 });
            res[0, 0] = (2160 - ProjectPos[0, 0]) * gussProjectScale;
            res[1, 0] = ProjectPos[1, 0] * gussProjectScale;
            return res;
        }
        public DenseMatrix GetSpotAtCalib(bool isCam1, DenseMatrix CalibPos, DenseMatrix CamPix)
        {

            if (isCam1)
            {
                DenseMatrix spotAtCam1 = (DenseMatrix)(Cam1ToCali * (CamPix * 0.00345));
                return CalibPos + spotAtCam1;
            }
            else
            {
                DenseMatrix spotAtCam2 = (DenseMatrix)(CamPix);
                DenseMatrix spotAtCam1 = (DenseMatrix)(Cam2ToCali * spotAtCam2);
                return CalibPos + spotAtCam1;
            }
        }
        public DenseMatrix GetCalibXY_calib(bool isCam1, DenseMatrix projectorPos)
        {

            if (isCam1)
            {
                DenseMatrix temp = ProjectorPixRotateToCalib(projectorPos);
                DenseMatrix res = ProjOriAtCalib + temp + CamCenterOffset;
                return res;
            }
            else
            {
                DenseMatrix res = ProjOriAtCalib + ProjectorPixRotateToCalib(projectorPos) + CamCenterOffset - new DenseMatrix(4, 1, new double[] { Cam2ToCali[0, 3], Cam2ToCali[1, 3], 0, 1 });
                return res;
            }
        }
        
        public void Doit()
        {

        }
    }

    //在所有坐标系方向一致的情况下做计算
    //需要   //1-像素点（以左下角为原点）XY ->投影图像
    //2-拍照图片(以图片中心为原点)->XY
    //3-定义校准架坐标系，下方相机中心为原点
    //4-获取同一个投影点成像时拍照xy1，xy2获取CAM2@CAM1

    ////轮子
    //public class ProjCalib : BindableBase

    //{
    //    //校准前调试准备：①将校准架与刮刀上表面以及直线导轨进行调平②将2个相机平面调平
    //    //功能规划1：可以初始化并得双相机以及坐标系校准结果
    //    //①CAM2@CAM1,xy->XY 方向
    //    //②ORI calix，caliy，xy->XY方向//校准架运动原点以及方向
    //    //③PROJ@CAM1，xy->XY
    //    //功能规划2：可以进行自动化校准程序
    //    public List<string> Devices { get; set; }
    //    public HkCamera camera;
    //    private bool isleft;
    //    private PrinterMotionControl PMC;
    //    List<Projector> P_List;

    //    private double camScale = 0.00345;//  mm/pixel
    //    private DenseVector camRes = new DenseVector(new double[2] { 2448, 2048 });//按相机原坐标系定义
    //    private DenseVector projRes = new DenseVector(new double[2] { 2160, 3840 }); //{ 1528, 2712 });//按打印机坐标系定义
    //    private double projScale = 0.1414;//  mm/pixel

    //    DenseVector proj11AtCam1 = new DenseVector(new double[2] { 0, 0 });
    //    DenseVector calibOri = new DenseVector(new double[2] { 0, 0 });
    //    private DenseVector Cam2AtCam1 = new DenseVector(new double[2] { 0, 200 });//上相机相对下相机的位置
    //    private bool isCam1Cam2_01 = true;


    //    public ImageSource CAM1_Image { get; set; }

    //    public ImageSource CAM2_Image { get; set; }

    //    public DelegateCommand Button_Point11Command { get; set; }
    //    public DelegateCommand Button_DoCalibCommand { get; set; }
    //    public DelegateCommand Button_GrayCalibCommand { get; set; }
    //    public DelegateCommand Button_GetCalibGrayCommand { get; set; }
    //    public DelegateCommand Button_GoCommand { get; set; }

    //    public DelegateCommand Button_CalibOriCommand { get; set; }

    //    public double calibX { get; set; } = 0;

    //    public double calibY { get; set; } = 0;
    //    public double calibGray { get; set; } = 0;

    //    public bool canGetCalib = false;

    //    private int CamLimitN = 0;

    //    //List<CAMInfo> CAMList;
    //    List<Calib.CalibItem> calibItems = new List<Calib.CalibItem>();

    //    private class CalibItem
    //    {
    //        public bool useCam1;
    //        public int projX;//pixel
    //        public int projY;//pixel
    //        public double calibX;//mm
    //        public double calibY;//mm
    //        public double spotResultX;//pixel
    //        public double soptResultY;//pixel
    //    }
    //    private class CAMInfo
    //    {
    //        public string camName { get; set; }
    //        public int index { get; set; }
    //    }
    //    public ProjCalib(IEventAggregator ea, IProjectFactory projectFactory, PrinterMotionControl printerMotionControl, bool isLeft)
    //    {
    //        Button_CalibOriCommand = new DelegateCommand(GetProjAtCalib);
    //        //try
    //        //{
    //        //    Devices = new List<string>();
    //        //    var cameraList = new List<string>();
    //        //    camera = new HkCamera();
    //        //    Devices = camera.GetCameraList();
    //        //}
    //        //catch (Exception ex) { }

    //        //if (Devices.Count < 2) { PMC.MessageInfoUpdate("相机初始化异常"); }
    //        //else { PMC.MessageInfoUpdate("相机初始化成功"); }
    //        isleft = isLeft;
    //        PMC = printerMotionControl;
    //        P_List = projectFactory.GetProjectors();
    //    }
    //    ~ProjCalib()
    //    {
    //        if (Devices.Count > 0)
    //        { camera.Dispose(); }

    //    }

    //    private DenseVector GetSpotXY(Mat srcImage)//获取以相机中心为坐标系原点，打印机XY坐标系方向的坐标值mm
    //    {
    //        Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat> res1 = MyFindContoursEllipse(srcImage);
    //        if (isleft)
    //        {
    //            return new DenseVector(new double[2] { -(res1.Item1.Y - camRes[1] / 2) * camScale, (res1.Item1.X - camRes[0] / 2) * camScale });
    //        }
    //        else
    //        {
    //            return new DenseVector(new double[2] { (res1.Item1.Y - camRes[1] / 2) * camScale, -(res1.Item1.X - camRes[0] / 2) * camScale });
    //        }
    //    }
    //    private Tuple<DenseVector, Mat, bool> GetSpotXYimg(Mat srcImage)//获取以相机中心为坐标系原点，打印机XY坐标系方向的坐标值mm
    //    {
    //        Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat> res1 = MyFindContoursEllipse(srcImage);
    //        if (res1.Item2.Width > 150) { return new Tuple<DenseVector, Mat, bool>(new DenseVector(new double[2] { 0, 0 }), new Mat(), false); }
    //        if (isleft)
    //        {
    //            return new Tuple<DenseVector, Mat, bool>(new DenseVector(new double[2] { -(res1.Item1.Y - camRes[1] / 2) * camScale, (res1.Item1.X - camRes[0] / 2) * camScale }), res1.Item3, true);
    //        }
    //        else
    //        {
    //            return new Tuple<DenseVector, Mat, bool>(new DenseVector(new double[2] { (res1.Item1.Y - camRes[1] / 2) * camScale, -(res1.Item1.X - camRes[0] / 2) * camScale }), res1.Item3, true);
    //        }
    //    }
    //    public void GoInCalibSys(double x, double y)
    //    {
    //        if (isleft)
    //        {
    //            PMC.PTP_MM(AxesEnum.CalibX, calibOri[0] - x, 60000, 1000000, 1000000);
    //            PMC.PTP_MM(AxesEnum.CalibY, calibOri[1] - y, 60000, 1000000, 1000000);
    //        }
    //        else
    //        {
    //            PMC.PTP_MM(AxesEnum.CalibX, calibOri[0] + x, 60000, 1000000, 1000000);
    //            PMC.PTP_MM(AxesEnum.CalibY, calibOri[1] + y, 60000, 1000000, 1000000);
    //        }
    //    }
    //    public bool CheckInCalibSys(double x, double y)
    //    {
    //        double Cx = calibOri[0] + x;
    //        double Cy = calibOri[1] + y;
    //        if (isleft)
    //        {
    //            Cx = calibOri[0] - x;
    //            Cy = calibOri[1] - y;
    //        }

    //        double Cdx = Math.Abs(PMC.GetAxisPos_MM(AxesEnum.CalibX) - Cx);
    //        double Cdy = Math.Abs(PMC.GetAxisPos_MM(AxesEnum.CalibY) - Cy);
    //        if (Cdx < 0.01 && Cdy < 0.01)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //    public void ProjInCalibSys(double x, double y)
    //    {
    //        double projx = x - proj11AtCam1[0];
    //        double projy = y - proj11AtCam1[1];
    //        if (isleft)
    //        {
    //            P_List[0].ProjectImageLED(GetProjImage((int)Math.Round(projx / projScale), (int)Math.Round(projy / projScale),1), 200000, 1000);
    //        }
    //        else
    //        {
    //            P_List[1].ProjectImageLED(GetProjImage((int)Math.Round(projx / projScale), (int)Math.Round(projy / projScale),1), 200000, 1000);
    //        }
    //    }
    //    public BitmapImage GetProjImage(int x, int y,int r)//产生以投影仪左下角为原点，打印机XY坐标系方向的图像
    //    {
    //        int cvX = (int)projRes[1] - y - 1;
    //        int cvY = x;
    //        Mat img = Mat.Zeros(new OpenCvSharp.Size((int)projRes[0], (int)projRes[1]), new MatType(MatType.CV_8UC3));
    //        //img.Set<Vec3b>(cvX, cvY, new Vec3b(255, 255, 255));

    //        for (int mi = cvX - r; mi < cvX + r + 1; mi++)
    //        {
    //            for (int mj = cvY - r; mj < cvY + r + 1; mj++)
    //            {
    //                img.Set<Vec3b>(mi, mj, new Vec3b(255, 255, 255));
    //            }
    //        }
    //        //img.ToBitmap().Save("D:\\x.png");
    //        return BitmapToBitmapImage(img.ToBitmap());
    //    }
    //    public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
    //    {

    //        using (MemoryStream stream = new MemoryStream())
    //        {
    //            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp); // 坑点：格式选Bmp时，不带透明度
    //            stream.Position = 0;
    //            BitmapImage result = new BitmapImage();
    //            result.BeginInit();
    //            result.CacheOption = BitmapCacheOption.OnLoad;
    //            result.StreamSource = stream;
    //            result.EndInit();
    //            result.Freeze();
    //            return result;
    //        }
    //    }

    //    public BitmapImage ToBitmapSource(Bitmap bitmap)
    //    {
    //        MemoryStream ms = new MemoryStream();
    //        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
    //        ms.Position = 0;
    //        BitmapImage bi = new BitmapImage();
    //        bi.BeginInit();
    //        bi.StreamSource = ms;
    //        bi.EndInit();
    //        return bi;
    //    }
    //    //流程：
    //    //投影1，1像素 // GetProjImage(1,1)
    //    //移动XY到CAM1图像中心 //输入XY坐标，并根据Cam1 图像监视按确定键
    //    //读取XY
    //    //读取SpotXY
    //    //oriXY == XY
    //    //proj@CaliXY  = SoptXY - projScale/2,projScale/2
    //    public void SetCamPic(bool isCam1, BitmapImage bitmapImage)
    //    {
    //        double IMGScaleX = 0.1; double IMGScaleY = -0.1;
    //        int IMGRotate = -90;
    //        if (!isleft)
    //        {
    //            IMGRotate = 90;
    //        }
    //        var tb = new TransformedBitmap();
    //        tb.BeginInit();
    //        tb.Source = bitmapImage;
    //        var transform = new ScaleTransform(IMGScaleX, IMGScaleY, 0, 0); ;
    //        tb.Transform = transform;
    //        tb.EndInit();
    //        var tbb = new TransformedBitmap();
    //        tbb.BeginInit();
    //        tbb.Source = tb;
    //        var transformb = new RotateTransform(IMGRotate);
    //        tbb.Transform = transformb;
    //        tbb.EndInit();
    //        //FLIP V ,ROTATE -90
    //        //FLIP V ,ROTATE 90
    //        if (isCam1)
    //        {
    //            CAM1_Image = tbb;
    //        }
    //        else
    //        {
    //            CAM2_Image = tbb;
    //        }
    //    }
    //    private void GetProjAtCalib() //获取ori xy ,获取估算的proj@ caliXY
    //    {
    //        //原点确认为当前calibX calibY
    //        //获取当前CAM1 光点中心
    //        calibOri[0] = PMC.GetAxisPos_MM(AxesEnum.CalibX);
    //        calibOri[1] = PMC.GetAxisPos_MM(AxesEnum.CalibY);

    //        Mat mat1 = GetCamPicMat(0, 600);
    //        //mat1.SaveImage("D:\\grap.bmp");
    //        //mat1.SaveImage($"D:\\calib_Cam0.png");
    //        SetCamPic(true, BitmapToBitmapImage(mat1.ToBitmap()));
    //        var res = GetSpotXYimg(mat1);
    //        proj11AtCam1 = res.Item1 - new DenseVector(new double[2] { 100 * projScale, 100 * projScale });
    //        res.Item2.SaveImage($"D:\\calib_Cam0.png");
    //        //计算CAM1对于投影仪的覆盖值
    //        if (isleft)
    //        {
    //            CamLimitN = (int)Math.Round((calibOri[1] - PMC.CalibYLowerLimit / 10000 - proj11AtCam1[1]) / projScale);
    //        }
    //        else
    //        {
    //            CamLimitN = (int)Math.Round((PMC.CalibYUpperLimit / 10000 - calibOri[1] - proj11AtCam1[1]) / projScale);
    //        }


    //    }
    //    public void CAM2CAM1Calibrate()//获取预设的CAM2CAM1距离
    //    {
    //        new Thread(() => {
    //            double projXinCalib = 30;
    //            double projYinCalib = -((PMC.CalibYUpperLimit / 10000 - 200) - calibOri[1] - (PMC.CalibYUpperLimit / 10000 - PMC.CalibYLowerLimit / 10000 - 200) / 2);
    //            if (!isleft)
    //            {
    //                projYinCalib = PMC.CalibYLowerLimit / 10000 + 200 - calibOri[1] + (PMC.CalibYUpperLimit / 10000 - PMC.CalibYLowerLimit / 10000 - 200) / 2;
    //            }
    //            App.Current.Dispatcher.BeginInvoke(new Action(() =>
    //            {

    //                ProjInCalibSys(projXinCalib, projYinCalib);

    //            }));
    //            //采集CAM1
    //            GoInCalibSys(projXinCalib, projYinCalib);
    //            //wait untill 
    //            while (!CheckInCalibSys(projXinCalib, projYinCalib))
    //            {
    //                Thread.Sleep(100);
    //            }
    //            Thread.Sleep(400);
    //            Mat mat1 = GetCamPicMat(0, 350);

    //            //GetCamPicMat(0).SaveImage("D:\\test1.png");
    //            var res = GetSpotXYimg(mat1);
    //            DenseVector CAM1Pos = res.Item1;
    //            res.Item2.SaveImage($"D:\\{projXinCalib}-{projYinCalib}-{CAM1Pos[0]}-{CAM1Pos[1]}_Cam0.png");
    //            //采集CAM2
    //            GoInCalibSys(projXinCalib, projYinCalib - 200);
    //            //wait untill 
    //            while (!CheckInCalibSys(projXinCalib, projYinCalib - 200))
    //            {
    //                Thread.Sleep(100);
    //            }
    //            Thread.Sleep(400);
    //            //GetCamPicMat(1).SaveImage("D:\\test2.png");
    //            Mat mat2 = GetCamPicMat(1, 350);
    //            var res2 = GetSpotXYimg(mat2);
    //            DenseVector CAM2Pos = res2.Item1;
    //            res2.Item2.SaveImage($"D:\\{projXinCalib}-{projYinCalib - 200}-{CAM2Pos[0]}-{CAM2Pos[1]}_Cam1.png");
    //            Cam2AtCam1 = CAM1Pos - CAM2Pos;
    //            Cam2AtCam1[1] += 200;
    //        }).Start();
    //    }
    //    public static List<string> getDirectory(string path)
    //    {
    //        List<String> list = new List<string>();
    //        DirectoryInfo root = new DirectoryInfo(path);
    //        DirectoryInfo[] di = root.GetDirectories();
    //        for (int i = 0; i < di.Length; i++)
    //        {
    //            //   Console.WriteLine(di[i].FullName);
    //            list.Add(di[i].FullName);
    //        }

    //        return list;
    //    }
    //    public static List<string> getFiles(string path)
    //    {
    //        List<String> list = new List<string>();
    //        DirectoryInfo root = new DirectoryInfo(path);
    //        FileInfo[] di = root.GetFiles();
    //        for (int i = 0; i < di.Length; i++)
    //        {
    //            //   Console.WriteLine(di[i].FullName);
    //            list.Add(di[i].FullName);
    //        }

    //        return list;
    //    }


    //    public void DoProjCalib()
    //    {

    //        new Thread(() =>
    //        {
    //            ProjPixelCalib();
    //        }).Start();

    //        //初步提取四条边的距离结果

    //    }
    //    public void Button_GrayCalib()
    //    {
    //        LoadCalibrationTestData();
    //        new Thread(() =>
    //        {
    //            ProjGrayCalib();
    //        }).Start();

    //    }
    //    public void Button_GetCalibGray()
    //    {
    //        canGetCalib = true;
    //    }

    //    BitmapImage bitmapToBitmapImage(Bitmap bitmap)
    //    {
    //        using (var stream = new MemoryStream())
    //        {
    //            bitmap.Save(stream, ImageFormat.Png);
    //            stream.Position = 0;
    //            BitmapImage reslut = new BitmapImage();
    //            reslut.BeginInit();
    //            reslut.CacheOption = BitmapCacheOption.OnLoad;
    //            reslut.StreamSource = stream;
    //            reslut.EndInit();
    //            reslut.Freeze();
    //            return reslut;
    //        }
    //    }
    //    public bool WriteTransformedBitmapToFile<T>(BitmapSource bitmapSource, string fileName) where T : BitmapEncoder, new()
    //    {
    //        if (string.IsNullOrEmpty(fileName) || bitmapSource == null)
    //            return false;

    //        //creating frame and putting it to Frames collection of selected encoder
    //        var frame = BitmapFrame.Create(bitmapSource);
    //        var encoder = new T();
    //        encoder.Frames.Add(frame);
    //        try
    //        {
    //            using (var fs = new FileStream(fileName, FileMode.Create))
    //            {
    //                encoder.Save(fs);
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            return false;
    //        }
    //        return true;
    //    }

    //    private BitmapImage GetBitmapImage<T>(BitmapSource bitmapSource) where T : BitmapEncoder, new()
    //    {
    //        var frame = BitmapFrame.Create(bitmapSource);
    //        var encoder = new T();
    //        encoder.Frames.Add(frame);
    //        var bitmapImage = new BitmapImage();
    //        bool isCreated;
    //        try
    //        {
    //            using (var ms = new MemoryStream())
    //            {
    //                encoder.Save(ms);
    //                ms.Position = 0;

    //                bitmapImage.BeginInit();
    //                bitmapImage.StreamSource = ms;
    //                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
    //                bitmapImage.EndInit();
    //                isCreated = true;
    //            }
    //        }
    //        catch
    //        {
    //            isCreated = false;
    //        }
    //        return isCreated ? bitmapImage : null;
    //    }

    //    public void ProjPixelCalib() {
    //        List<Calib.CalibItem> clibItems = new List<Calib.CalibItem>();
    //        for (int i = 164; i < 1400; i += 300)//1528 - 328 =1200
    //        {
    //            for (int j = 156; j < 2600; j += 300)//2712 - 312 =2400 ////0,8,36,44
    //            {
    //                DenseVector projSpotAtCalib = proj11AtCam1 + new DenseVector(new double[2] { i * projScale, j * projScale });
    //                bool useCam1 = true;//能用CAM1就用cam1，计算cam1极限值
    //                                    //CAM1/CAM2判断
    //                if (j > CamLimitN) { useCam1 = false; }
    //                App.Current.Dispatcher.BeginInvoke(new Action(() =>
    //                {
    //                    ProjInCalibSys(projSpotAtCalib[0], projSpotAtCalib[1]);
    //                }));
    //                //采集CAM1
    //                DenseVector calibXY = projSpotAtCalib;
    //                if (!useCam1)
    //                {
    //                    calibXY -= Cam2AtCam1;
    //                }
    //                GoInCalibSys(calibXY[0], calibXY[1]);
    //                //wait untill 
    //                while (!CheckInCalibSys(calibXY[0], calibXY[1]))
    //                {
    //                    Thread.Sleep(100);
    //                }
    //                Thread.Sleep(400);
    //                DenseVector v1; DenseVector v2;
    //                if (useCam1)
    //                {
    //                    int expo = 450;

    //                    if (j < 300) { expo = 600; }
    //                    Mat mat1 = GetCamPicMat(0, expo);
    //                    var res = GetSpotXYimg(mat1);
    //                    while (!res.Item3)
    //                    {
    //                        mat1 = GetCamPicMat(0, expo);
    //                        res = GetSpotXYimg(mat1);
    //                    }
    //                    res.Item2.SaveImage($"D:\\{i}-{j}-{calibXY[0]}-{calibXY[1]}_Cam0.png");
    //                    v1 = res.Item1;
    //                    v2 = v1 + calibXY;
    //                }
    //                else
    //                {
    //                    int expo = 350;

    //                    if (j > 2500) { expo = 550; }
    //                    Mat mat1 = GetCamPicMat(1, expo);
    //                    var res = GetSpotXYimg(mat1);
    //                    while (!res.Item3)
    //                    {
    //                        mat1 = GetCamPicMat(1, expo);
    //                        res = GetSpotXYimg(mat1);
    //                    }
    //                    res.Item2.SaveImage($"D:\\{i}-{j}-{calibXY[0]}-{calibXY[1]}_Cam1.png");
    //                    v1 = res.Item1;
    //                    v2 = v1 + calibXY + Cam2AtCam1;
    //                }
    //                //计算校准架移动坐标
    //                clibItems.Add(new Calib.CalibItem()
    //                {
    //                    useCam1 = useCam1,
    //                    projX = i,
    //                    projY = j,
    //                    calibX = v2[0],
    //                    calibY = v2[1],
    //                    spotResultX = v1[0],
    //                    spotResultY = v1[1]
    //                });
    //            }
    //        }
    //        using (FileStream fs = new FileStream($"D:\\calibResult.json", FileMode.Create))
    //        {
    //            using (StreamWriter sw = new StreamWriter(fs))
    //            {
    //                foreach (var item in clibItems)
    //                {
    //                    sw.WriteLine($"proj,{item.projX},{item.projY},cam1,{item.useCam1},calib,{item.calibX},{item.calibY},spot,{item.spotResultX},{item.spotResultY}");
    //                }
    //            }
    //        }
    //        using (FileStream fs = new FileStream($"D:\\calibitems.json", FileMode.Create))
    //        {
    //            using (StreamWriter sw = new StreamWriter(fs))
    //            {
    //                sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(clibItems));
    //            }
    //        }

    //        //using (FileStream fs = new FileStream($"D:\\calibEdgeResult.txt", FileMode.Create))
    //        //{
    //        //    using (StreamWriter sw = new StreamWriter(fs))
    //        //    {
    //        //        sw.WriteLine("upper side:");
    //        //        double length0 = Dist(clibItems[8], clibItems[44]);
    //        //        sw.WriteLine($"length->{length0};Err->{Math.Abs(length0 - projScale * projRes[0])}");
    //        //        sw.WriteLine("lower side:");
    //        //        double length1 = Dist(clibItems[0], clibItems[36]);
    //        //        sw.WriteLine($"length->{length1};Err->{Math.Abs(length1 - projScale * projRes[0])}");
    //        //        sw.WriteLine("lower side:");
    //        //        double length2 = Dist(clibItems[0], clibItems[8]);
    //        //        sw.WriteLine($"length->{length2};Err->{Math.Abs(length2 - projScale * projRes[1])}");
    //        //        sw.WriteLine("lower side:");
    //        //        double length3 = Dist(clibItems[44], clibItems[36]);
    //        //        sw.WriteLine($"length->{length3};Err->{Math.Abs(length3 - projScale * projRes[1])}");
    //        //    }
    //        //}

    //    }
    //    private void CheckCalibResult(string path)
    //    {
    //        using (FileStream fs = new FileStream(path, FileMode.Open))
    //        {
    //            using (StreamReader sr = new StreamReader(fs))
    //            {
    //                string fileString = sr.ReadToEnd();
    //                var items =  Newtonsoft.Json.JsonConvert.DeserializeObject<List<CalibItem>>(fileString);
    //                var objlist = new List<Point2f>();
    //                foreach (var item in items)
    //                {
    //                    int i = (item.projX - 164) / 300;
    //                    int j = (item.projY - 156) / 300;
    //                    objlist.Add(new Point2f() { X = (float)item.calibX, Y = (float)item.calibY });
    //                }
    //                InputArray objectPoints = InputArray.Create<Point2f>(objlist);
    //                var imglist = new List<Point2f>();
    //                for (int i = 164; i < 1400; i += 300)//1528 - 328 =1200
    //                {
    //                    for (int j = 156; j < 2600; j += 300)//2712 - 312 =2400 ////0,8,36,44
    //                    {
    //                        DenseVector projSpotAtCalib = proj11AtCam1 + new DenseVector(new double[2] { i * projScale, j * projScale });
    //                        imglist.Add( new Point2f()
    //                        {
    //                            X = i * (float)projScale,
    //                            Y = j * (float)projScale
    //                        });
    //                    }
    //                }
    //                InputArray imagePoints = InputArray.Create<Point2f>(imglist);

    //                var res = Cv2.EstimateAffinePartial2D(objectPoints, imagePoints);
    //                var matTrans = Cv2.EstimateAffine2D(objectPoints,imagePoints);
    //                var wPoints = new List<Point2f>();
    //                //Cv2.WarpAffine(objectPoints, wPoints, res);
    //                for (int i = 0; i < matTrans.Rows; i++)
    //                {
    //                    for (int j = 0; j < matTrans.Cols; j++)
    //                    {
    //                        Console.WriteLine(matTrans.At<double>(i,j));
    //                    }
    //                }
    //                for (int i = 0; i < res.Rows; i++)
    //                {
    //                    for (int j = 0; j < res.Cols; j++)
    //                    {
    //                        Console.WriteLine(res.At<double>(i, j));
    //                    }
    //                }
    //                DenseMatrix transmatrix = new DenseMatrix(matTrans.Rows, matTrans.Cols);

    //                for (int i = 0; i < matTrans.Rows; i++)
    //                {
    //                    for (int j = 0; j < matTrans.Cols; j++)
    //                    {
    //                        transmatrix[i,j] = matTrans.At<double>(i,j);
    //                        Console.WriteLine(matTrans.At<double>(i, j));
    //                    }
    //                }
    //                Console.WriteLine(transmatrix);
    //                //Mat objectPoints = new Mat<Point2f>(5,9);
    //                double error = 0;
    //                foreach(var item in items)
    //                {

    //                    DenseMatrix objpoint = new DenseMatrix(3,1);
    //                    //objpoint.in

    //                    objpoint[0, 0] = item.calibX;
    //                    objpoint[1, 0] = item.calibY;
    //                    objpoint[2, 0] = 1;

    //                    //Console.WriteLine($"OBJ:{objpoint[0,0]},{objpoint[1, 0]}");
    //                    DenseMatrix transedObjPoint = transmatrix * objpoint;
    //                    Console.WriteLine($"transedOBJ:{transedObjPoint[0,0]},{transedObjPoint[1, 0]}");
    //                    Console.WriteLine($"proj:{item.projX*projScale},{item.projY * projScale}");
    //                    double err = Math.Sqrt(Math.Pow(transedObjPoint[0, 0] - item.projX * projScale, 2) + Math.Pow(transedObjPoint[1, 0] - item.projY * projScale, 2));
    //                    Console.WriteLine($"err:{err}mm");
    //                    error += err;
    //                }
    //                Console.WriteLine($"average err:{error/45}mm");
    //                // Mat imagePoints = new Mat<Point2f>(5, 9);
    //                //尝试仿射变换结果确认
    //                //



    //            }
    //        }
    //    }
    //    public static double Dist(Calib.CalibItem i1, Calib.CalibItem i2)
    //    {
    //        double x1 = i1.calibX + i1.spotResultX;
    //        double y1 = i1.calibY + i1.spotResultY;

    //        double x2 = i2.calibX + i2.spotResultX;
    //        double y2 = i2.calibY + i2.spotResultY;

    //        double dist = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

    //        return dist;
    //    }
    //    public BitmapSource GetCamPicBmS(int cam)
    //    {

    //        var image1 = GetCamPicMat(cam,300);
    //        return image1.ToBitmapSource();
    //    }

    //    public Mat GetCamPicMat(int cam,int expo)
    //    {
    //        if (!isCam1Cam2_01 && cam == 1)
    //        {
    //            cam = 0;
    //        }
    //        else if (!isCam1Cam2_01 && cam == 0)
    //        {
    //            cam = 1;
    //        }


    //        camera.SetExposure(cam, expo);
    //        Thread.Sleep(50);
    //        camera.OpenDevice(cam);
    //        Thread.Sleep(50);
    //        Mat mat1 = camera.Grab(cam);
    //        return mat1;

    //    }




    //    private BitmapEncoder GetBitmapEncoder(string filePath)
    //    {
    //        var extName = System.IO.Path.GetExtension(filePath).ToLower();
    //        if (extName.Equals(".png"))
    //        {
    //            return new PngBitmapEncoder();
    //        }
    //        else
    //        {
    //            return new JpegBitmapEncoder();
    //        }
    //    }
    //    private static Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat> MyFindContoursEllipse(Mat srcImage)
    //    {
    //        //转化为灰度图
    //        //Mat src_gray = new Mat();
    //        //Cv2.CvtColor(srcImage, src_gray, ColorConversionCodes.RGB2GRAY);
    //        //Mat thImage = new Mat(srcImage.Height, srcImage.Width, new MatType(MatType.CV_8UC3));
    //        //Cv2.Threshold(src_gray, thImage, 50, 255, ThresholdTypes.Binary);
    //        //thImage.SaveImage("D:\\test.bmp");


    //        Mat src_gray = new Mat();
    //        //Cv2.CvtColor(img, src_gray, ColorConversionCodes.RGB2GRAY);
    //        //InputArray kernel = InputArray.Create<int>(new int[ 2 ] { 1,1 });
    //        Cv2.ExtractChannel(srcImage, src_gray, 1);

    //        //Cv2.MorphologyEx(img, morph, MorphTypes.Gradient, kernel);
    //        Mat thImage = new Mat();
    //        Cv2.Threshold(src_gray, thImage, 220, 255, ThresholdTypes.Otsu);

    //        Mat open = new Mat(); Mat close = new Mat();
    //        Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
    //        Cv2.MorphologyEx(thImage, open, MorphTypes.Open, element);
    //        Cv2.MorphologyEx(open, close, MorphTypes.Close, element);
    //        //获得轮廓
    //        OpenCvSharp.Point[][] contours;
    //        HierarchyIndex[] hierarchly;
    //        Cv2.FindContours(close, out contours, out hierarchly, RetrievalModes.External, ContourApproximationModes.ApproxNone, new OpenCvSharp.Point(0, 0));
    //        Mat dst_Image = srcImage;
    //        if (contours.Length > 0)
    //        {
    //            RotatedRect rot = Cv2.FitEllipse(contours[contours.Length - 1]);
    //            ////将结果画出并返回结果

    //            Random rnd = new Random();
    //            for (int i = 0; i < contours.Length; i++)
    //            {
    //                Scalar color = new Scalar(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
    //                Cv2.DrawContours(dst_Image, contours, i, color, 2, LineTypes.Link8, hierarchly);
    //            }
    //            OpenCvSharp.Point center = new OpenCvSharp.Point(rot.Center.X, rot.Center.Y);
    //            OpenCvSharp.Size size = new OpenCvSharp.Size((int)rot.Size.Width / 2, (int)rot.Size.Height / 2);
    //            Cv2.Ellipse(dst_Image, center, size, rot.Angle, 0, 360, Scalar.AliceBlue);
    //            return new Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat>(center, size, dst_Image);
    //        }
    //        else { return new Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat>(new OpenCvSharp.Point(0, 0), new OpenCvSharp.Size(0, 0), dst_Image); }
    //    }  
    //}
    
}
