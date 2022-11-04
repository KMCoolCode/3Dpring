using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using Prism.Events;
using AngelDLP.ViewModels;
using static AngelDLP.ViewModels.MainWindowViewModel;
using Prism.Mvvm;
using AngleDLP.PPC;
using static AngleDLP.Models.LogHelper;
using AngleDLP.Models;
using static Ddp442xLibrary.Ddp442x;

namespace AngelDLP.PPC
{
    public class Projector : BindableBase
    {
        #region Init
        Ddp442xWithId mDdp442x;
        CypressI2cWithId mCpressI2c;
        bool isI2CBusy;
        bool LedSwitch;
        #endregion
        public ProjectorWindow window;

        //private object lockProj = new object();

        private ProjectorViewModel projector_ViewModel;
        public ProjectorViewModel Projector_ViewModel
        {
            get { return projector_ViewModel; }
            set { SetProperty(ref projector_ViewModel, value); }

        }

        public bool windowsReady = false;
        
        public bool projectorReady = false;

        public int projectorIndex { get; set; }


        private  bool isON = false;

        public bool IsON
        {
            set
            {
                OnPropertyChanged("ONOFF");
                SetProperty(ref isON, value);
            }
            get { return isON; }
        }

        private bool isLEDON = false;

        public bool IsLEDON
        {
            set
            {
                SetProperty(ref isLEDON, value);
                OnPropertyChanged("LEDONOFF");
            }
            get { return isLEDON; }
        }

        public string projectorName = string.Empty;
        
        private log4net.ILog pLog;
        private readonly IEventAggregator _ea;

        public ProjectorConfig projectorConfig { get; set; }
        public Projector(IEventAggregator eventAggregator)
        {
            this._ea = eventAggregator;
        }

     
        private void OnPropertyChanged(string para)
        {

            if (para == "ONOFF" || para == "LEDONOFF")
            {
                RaisePropertyChanged(para);
            }
        }

        public void SetPara(int index, ProjectorConfig config)
        {
            projectorConfig = config;
            projectorIndex = index;
        }
        public bool InitProjector(int displayPort, int controlPort)
        {
          
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (window != null) return;
                mDdp442x = new Ddp442xWithId((byte)controlPort);
                mCpressI2c = new CypressI2cWithId((byte)controlPort);
                projectorName = $"Projector{projectorIndex + 1}";
                pLog = log4net.LogManager.GetLogger("logProjection");//获取一个日志记录器
                                                                     //MessageInfoUpdate($"Start to init the {projectorName}");
                window = new ProjectorWindow(_ea);
                Projector_ViewModel = new ProjectorViewModel(_ea,  projectorConfig);
                window.DataContext = this.Projector_ViewModel;
                var res = window.InitProjectorWindow(projectorIndex, projectorConfig.projectorPixWidth, projectorConfig.projectorPixHeight, displayPort);
                if (!res.Item1)
                {
                    MessageInfoUpdate_Thread($"屏幕{projectorIndex+1}初始化失败, {res.Item2}!", LogLevel.Warn);
                    new Task(() => DialogHelper.ShowMessageDialog("提示", $"屏幕{projectorIndex + 1}初始化失败, {res.Item2}!")).Start();
                    return;
                }
                MessageInfoUpdate_Thread($"初始化为 {projectorConfig.projectorPixWidth}×{projectorConfig.projectorPixHeight}", LogLevel.Info);
                window.Show();
                windowsReady = true;
            })).Wait();
            if (windowsReady)
            { 
                TurnonProjector(); 
                LedOnOff(false); 
            }
            return true;
        }
        public bool TurnonProjector()
        {
            bool res =  ProjectorOn();
            if (res)
            { projectorReady = true; }
            else
            { projectorReady = false; }
            return res;
        }
        private void MessageInfoUpdate(string info)
        {
            _ea.GetEvent<MessageEvent>().Publish(info);
        }

        private void MessageInfoUpdate_Thread(string info, LogLevel logLevel = LogLevel.Info)
        {
            info = $"Projector{projectorIndex}-->{info}";
            switch (logLevel)
            {
                case LogLevel.Info:
                    pLog.Info(info);
                    break;
                case LogLevel.Debug:
                    pLog.Debug(info);
                    break;
                case LogLevel.Warn:
                    pLog.Warn(info);
                    break;
                case LogLevel.Error:
                    pLog.Error(info);
                    break;
                case LogLevel.Fatal:
                    pLog.Fatal(info);
                    break;
            }
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageInfoUpdate(info);
            }));
        }
        public void ProjectImage(BitmapSource image, double time = 0)
        {
            new Thread(() => {
                //MessageInfoUpdate_Thread($"start to show {projectorName}'s image->");
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Projector_ViewModel.P1_Image = image;
                }));
                
                if (time !=0)
                {
                    while (time > 0)
                    {
                        time -= 100;
                        Thread.Sleep(100);
                    }
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Projector_ViewModel.P1_Image = null;
                    }));
                }
                
            }).Start();
        }

        public TransformedBitmap TransProjectImage(BitmapSource bitmapSource)
        {
            var tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = bitmapSource;
            var transform = new ScaleTransform(projectorConfig.imageTransScaleX, projectorConfig.imageTransScaleY, 0, 0); ;
            tb.Transform = transform;
            tb.EndInit();
            var tbb = new TransformedBitmap();
            tbb.BeginInit();
            tbb.Source = tb;
            var transformb = new RotateTransform(projectorConfig.imageTransRotate);
            tbb.Transform = transformb;
            tbb.EndInit();
            return tbb;
        }

        public void PrepareToProj(int lightDAC = 0, bool isSimulate = false)
        {
            if (!SetDmdPark(true))
            {
                //return new Tuple<bool, TimeSpan,string>(false, DateTime.Now - start,$"Projector {this.projectorIndex}, SetDmdPark error !");
            }
            Thread.Sleep(30);
            if (!LedOnOff(true, isSimulate))
            {
                //return new Tuple<bool, TimeSpan, string>(false, DateTime.Now - start, $"Projector {this.projectorIndex}, LedOn error !");
            }
            Thread.Sleep(30);
            if (!SetLEDDAC(lightDAC, isSimulate))
            {
                //return new Tuple<bool, TimeSpan, string>(false, DateTime.Now - start, $"Projector {this.projectorIndex}, SetLEDDAC error !");
            }
        }

        public void ProjSleep(bool isSimulate = false)
        {
            if (!LedOnOff(false, isSimulate))
            {
                //return new Tuple<bool, TimeSpan, string>(false, DateTime.Now - start, $"Projector {this.projectorIndex}, LedOff error !");
            }
            Thread.Sleep(30);
            if (!SetDmdPark(false))
            {
                //return new Tuple<bool, TimeSpan, string>(false, DateTime.Now - start, $"Projector {this.projectorIndex}, SetDmdPark error !");
            }
        }
        public Tuple<bool, TimeSpan, string> ProjectImageOnly(BitmapSource bitmapSource, double time = 0)
        {
            //FLIP V ,ROTATE -90
            //FLIP V ,ROTATE 90
            DateTime start; DateTime end = DateTime.Now;
            start = DateTime.Now;
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Projector_ViewModel.P1_Image = bitmapSource;
            }));
            if (time < 60000)
            {
                if (time != 0)
                {
                    while (time > 0)
                    {
                        time -= 100;
                        Thread.Sleep(100);
                    }
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Projector_ViewModel.P1_Image = null;
                        bitmapSource.Freeze();
                    }));
                    Thread.Sleep(40);
                    end = DateTime.Now;
                    pLog.Info($"{projectorName}->【{end.ToString("HH：mm：ss.ff")}】结束曝光，曝光时长{(end - start).TotalMilliseconds.ToString("0")};");
                }
            }
            return new Tuple<bool, TimeSpan, string>(true, (end - start), $"Projector {this.projectorIndex}, expose success !");

        }

        public Tuple<bool , TimeSpan, string >  ProjectImageLED(BitmapSource bitmapSource,double time = 0,int lightDAC = 0,bool isSimulate = false)//已经解决了方向问题
        {
            //FLIP V ,ROTATE -90
            //FLIP V ,ROTATE 90
            DateTime start; DateTime end = DateTime.Now;
            start = DateTime.Now;
            //App.Current.Dispatcher.BeginInvoke(new Action(() =>
            //{

            //}));
            if (isSimulate)
            {
                //return new Tuple<bool, TimeSpan, string>(true, DateTime.Now - start, $"Projector {this.projectorIndex}, simulate expose !");
            }
            if (!SetDmdPark(true))
            {
                //return new Tuple<bool, TimeSpan,string>(false, DateTime.Now - start,$"Projector {this.projectorIndex}, SetDmdPark error !");
            }
            Thread.Sleep(30);
            if (!LedOnOff(true, isSimulate))
            {
                //return new Tuple<bool, TimeSpan, string>(false, DateTime.Now - start, $"Projector {this.projectorIndex}, LedOn error !");
            }
            Thread.Sleep(30);
            if (!SetLEDDAC(lightDAC, isSimulate))
            {
                //return new Tuple<bool, TimeSpan, string>(false, DateTime.Now - start, $"Projector {this.projectorIndex}, SetLEDDAC error !");
            }
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Projector_ViewModel.P1_Image = bitmapSource;
            }));
            if (time < 60000)
            {
                if (time != 0)
                {
                    while (time > 0)
                    {
                        time -= 100;
                        Thread.Sleep(100);
                    }
                    if (!LedOnOff(false, isSimulate))
                    {
                        //return new Tuple<bool, TimeSpan, string>(false, DateTime.Now - start, $"Projector {this.projectorIndex}, LedOff error !");
                    }
                    Thread.Sleep(30);
                    if (!SetDmdPark(false))
                    {
                        //return new Tuple<bool, TimeSpan, string>(false, DateTime.Now - start, $"Projector {this.projectorIndex}, SetDmdPark error !");
                    }
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Projector_ViewModel.P1_Image = null;
                        bitmapSource.Freeze();
                    }));
                    Thread.Sleep(40);
                    end = DateTime.Now;
                    pLog.Info($"{projectorName}->【{end.ToString("HH：mm：ss.ff")}】结束曝光，曝光时长{(end - start).TotalMilliseconds.ToString("0")};");
                }
            }
            return new Tuple<bool, TimeSpan, string>(true, (end - start), $"Projector {this.projectorIndex}, expose success !");

        }
        #region Check state

        public void checkI2CBusy()
        {
            int ret;
            mDdp442x.ClearLog();

            ret = mDdp442x.OpenGpio();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("GPIO CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }

            ret = mDdp442x.GetI2CBusy(ref isI2CBusy);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }

            ret = mDdp442x.CloseGpio();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("GPIO CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }
            if (isI2CBusy)
                MessageInfoUpdate_Thread("I2C is busy now!!!");
        }

        public void checkLedOnOff()
        {
            int ret;
            mDdp442x.ClearLog();

            ret = mDdp442x.OpenGpio();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("GPIO Communication FAIL!!!", LogLevel.Warn);
                return;
            }

            ret = mDdp442x.GetLedOnOff(ref LedSwitch);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }

            ret = mDdp442x.CloseGpio();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("GPIO Communication FAIL!!!", LogLevel.Warn);
                return;
            }
        }

        #endregion Check state
        #region Projector ON/OFF

        public bool ProjectorOn()
        {
            bool ProjectorEnable = true;
            bool AsicReady = false;
            int ret;
            int loading_count = 0;
            int Vx1StableCount = 0;
            int Vx1TimeOut = 0;
            mDdp442x.ClearLog();
            ret = mDdp442x.OpenGpio();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("GPIO Communication FAIL!!!", LogLevel.Error);
                return false;
            }

            ret = mDdp442x.SetProjectorOnOff(ProjectorEnable);
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("Communication FAIL!!!", LogLevel.Error);
                return false;
            }
            while (!AsicReady && loading_count < 30)
            {
                ret = mDdp442x.GetAsicReady(ref AsicReady);
                if (ret < 0)
                {
                    MessageInfoUpdate_Thread("Communication FAIL!!!", LogLevel.Error);
                    return false;
                }
                Thread.Sleep(1000);
                loading_count++;
            }

            ret = mDdp442x.CloseGpio();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("GPIO Communication FAIL!!!", LogLevel.Error);
                return false;
            }

            while (Vx1StableCount < 2 && loading_count < 30 && Vx1TimeOut < 10)
            {
                int Vx1Status = 0;
                GetVx1Sync(out Vx1Status);
                if (Vx1Status == 255)
                    Vx1StableCount++;
                else
                    Vx1StableCount = 0;
                Thread.Sleep(800);
                Vx1TimeOut++;
            }

            if (loading_count == 30)
            {
                isON = false;
                ProjectorOff();
                MessageInfoUpdate_Thread("ProjectorOn Fail", LogLevel.Error);
                return false;
            }
            else if (Vx1TimeOut == 10)
            {
                MessageInfoUpdate_Thread("Vx1 Locked Failed. External source is abnormal.", LogLevel.Error);
            }


            MessageInfoUpdate_Thread("ASIC Is Ready!");

            //----------------Initialize---------------------
            isI2CBusy = true;
            IsON = true;
            return true;
        }
        private bool GetVx1Sync(out int status)
        {
            checkI2CBusy();
            status = 0;
            if (isI2CBusy)
                return false;
            int ret;

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!",LogLevel.Warn);
                return false;
            }

            ret = mDdp442x.GetVx1Lock(out status);
            if (ret < 0)
            {
                MessageInfoUpdate_Thread(mDdp442x.GetLog(), LogLevel.Warn);
                return false;
            }

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!");
                return false;
            }
            return true;
        }
        public bool ProjectorOff()
        {
            LedOnOff(false);
            bool ProjectorEnable = false;
            int ret;
            bool fail = false;
            if (mDdp442x == null)
            {
                return false;
            }
            mDdp442x.ClearLog();
            /***********************Remove the radiobutton checked when projector off.*********/
            //RadioMtrDirTemp1.IsChecked = true;
            //RadioMtrDirTemp2.IsChecked = true;
            //RadioPatternTemp.IsChecked = true;
            /***********************Turn off the fan when projector off.************************/
            ret = mDdp442x.OpenI2C();
            if (ret < 0 && !fail)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!(Fan)", LogLevel.Warn);
                fail = true;
                return false;
            }
            ret = mDdp442x.SetFanDuty("DMDFan", 0);
            if (ret < 0 && !fail)
            {
                pLog.Info(mDdp442x.GetLog());
                fail = true;
                return false;
            }

            ret = mDdp442x.SetFanDuty("LEDFan1", 0);
            if (ret < 0 && !fail)
            {
                pLog.Info(mDdp442x.GetLog());
                fail = true;
                return false;
            }

            ret = mDdp442x.SetFanDuty("LEDFan2", 0);
            if (ret < 0 && !fail)
            {
                pLog.Info(mDdp442x.GetLog());
                fail = true;
                return false;
            }
            ret = mDdp442x.CloseI2C();
            if (ret < 0 && !fail)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!(Fan)", LogLevel.Warn);
                fail = true;
                return false;
            }

            /********************************Turn off LED****************************/
            //RadioLedOff.IsChecked = true;
            LedOnOff(false);
            /************************************************************************/
            ret = mDdp442x.OpenGpio();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("GPIO Communication FAIL!!!", LogLevel.Warn);
                return false;
            }

            ret = mDdp442x.SetProjectorOnOff(ProjectorEnable);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return false;
            }

            ret = mDdp442x.CloseGpio();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("GPIO Communication FAIL!!!", LogLevel.Warn);
                return false;
            }
            IsON = false;
            return true;
        }

        #endregion Projector ON/OFF

        #region LED ON/OFF

        public bool LedOnOff(bool wantOn,bool isSimulate = false)//背光模组开关
        {
            if (isSimulate) { return true; }
            bool LedEnable = true;
            if (!wantOn)
                LedEnable = false;

            int ret;
            mDdp442x.ClearLog();

            ret = mDdp442x.OpenGpio();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("GPIO Communication FAIL!!!", LogLevel.Warn);
                return false;
            }

            ret = mDdp442x.SetLedOnOff(LedEnable);
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("Communication FAIL!!!", LogLevel.Warn);
                return false;
            }

            ret = mDdp442x.CloseGpio();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("GPIO Communication FAIL!!!", LogLevel.Warn);
                return false;
            }
            if (IsLEDON != LedEnable)
            {
                IsLEDON = !IsLEDON;
            }

            return  true;
        }

        public bool SetLEDDAC(int level, bool isSimulate = false)//设置LED电流
        {
            if (isSimulate) { return true; }
            checkI2CBusy();
            if (isI2CBusy)
                return false;

            int ret;

            try
            {
                if ((level > 1000 || level < 50) && level != 0)
                    throw new Exception("Value is not in the range(0,50~1000)");
            }
            catch (Exception exception)
            {
                MessageInfoUpdate_Thread(exception.Message);
                return false;
            }

            Console.WriteLine("level = {0}", level);

            mDdp442x.ClearLog();
            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
                return false;
            }

            ret = mDdp442x.SetLEDDAC(level);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return false;
            }

            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
                return false;
            }


            return true;
        }

        public int GetLEDDAC()//获取LED电流
        {
            checkI2CBusy();
            if (isI2CBusy)
                return -1;
            int Duty;
            int wavelength;
            int ret;

            if (mDdp442x == null)
                return -1;

            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
                return -1;
            }

            ret = mDdp442x.GetLEDDAC(out Duty);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return -1;
            }

            
            //ShowLog(mDdp442x.GetLog());

            ret = mDdp442x.GetLEDType(out wavelength);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return -1;
            }


            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
                return -1;
            }
            return Duty;
        }

        #endregion LED ON/OFF

        #region Temperature

        public int GetTemperature()
        {
            checkI2CBusy();
            if (isI2CBusy)
                return -1;
            int Temperature;
            int ret;
            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
            }

            ret = mDdp442x.GetTemperature(out Temperature);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return -1;
            }
            pLog.Info(mDdp442x.GetLog());



            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
                return -1;
            }
            return Temperature;

        }

        public float GetLEDTemperature()
        {
            float Temperature = -1;
            checkI2CBusy();
            if (isI2CBusy)
                return -1;

            int ret;
            mDdp442x.ClearLog();
            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
            }

            ret = mDdp442x.GetLEDTemperature(out Temperature);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return -1;
            }
            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
                return -1;
            }


            return (float)Math.Round(Temperature, 2);
        }

        public float GetLedBoardTemperature()
        {
            checkI2CBusy();
            if (isI2CBusy)
                return -1;
            float Temperature;
            int ret;
            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
            }

            ret = mDdp442x.GetLedDriverBoardTemperature(out Temperature);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return -1;
            }
            pLog.Info(mDdp442x.GetLog());

            //TextBoxLedBoardTemperature.Text = Temperature.ToString();

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
                return -1;
            }
            return (float)Math.Round(Temperature, 2);

        }

        #endregion Temperature

        #region Source and Pattern

        public void PatternOrSource(int sourcetype)//0-HDMI 1-Ramp 2-Checker 3-SolidField
        {
            checkI2CBusy();
            if (isI2CBusy)
                return;
            byte Select = 0x00;
            byte TestPattern = 0x00;

            int ret;
            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
            }

            if (sourcetype ==0)
                Select = 0x00;
            else if (sourcetype==1)
            {
                Select = 0x01;
                TestPattern = 0x01;
            }
            else if (sourcetype ==2)
            {
                Select = 0x01;
                TestPattern = 0x07;
            }
            else if (sourcetype==3)
            {
                Select = 0x02;

                ret = mDdp442x.SetSolidFieldGenerator(1023, 1023, 1023);        //Change Solid Filed Color To [White]. (Must change before chosing source.)

                if (ret < 0)
                {
                    pLog.Info(mDdp442x.GetLog());
                    return;
                }
            }

            ret = mDdp442x.SetInputSource(Select);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }

            Thread.Sleep(500);      //must sleep a little bit time.

            if (sourcetype !=0 && sourcetype!=3)
            {
                ret = mDdp442x.SetTestPatterns(TestPattern);
                if (ret < 0)
                {
                    MessageInfoUpdate_Thread("Communication FAIL!!!");
                    pLog.Info(mDdp442x.GetLog());
                    return;
                }
            }
            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!", LogLevel.Warn);
                return;
            }
        }

        public void SetProjectorMode(int resMode) //resMode 0-native 1-4K
        {
            int ret;
            byte mode = 0x00;
            checkI2CBusy();
            if (isI2CBusy)
                return;
            // the handler will be called before mDdp442x is initialized
            if (mDdp442x == null)
                return;

            mDdp442x.ClearLog();

            if (resMode ==1)
                mode = 0x01;
            else if (resMode ==0)
                mode = 0x02;


            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C Communication FAIL!!!");
                return;
            }

            ret = mDdp442x.SetResolutionMode(mode);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }
            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }
        }

        #endregion Source and Pattern

        #region Lightsensor

        public int GetLightSensor()
        {
            checkI2CBusy();
            if (isI2CBusy)
                return -1;
            int Light;
            int ret;

            // the handler will be called before mDdp442x is initialized
            if (mDdp442x == null)
                return -1;

            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return -1;
            }

            ret = mDdp442x.GetLightSensor(out Light);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return -1;
            }

            //TextBoxLightSensor.Text = Light.ToString();
            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return -1;
            }
            return Light;
        }

        #endregion Lightsensor

        #region MOTOR

        public void SetMtrDirFwd_FG()
        {
            int ret;
            checkI2CBusy();
            if (isI2CBusy)
                return;
            // the handler will be called before mDdp442x is initialized
            if (mDdp442x == null)
                return;

            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }

            ret = mDdp442x.SetMotor1Direction(0); // 0:Forward, 1:Backward
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }
            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }
        }

        public void SetMtrDirBwd_FG()
        {
            int ret;
            checkI2CBusy();
            if (isI2CBusy)
                return;
            // the handler will be called before mDdp442x is initialized
            if (mDdp442x == null)
                return;

            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }

            ret = mDdp442x.SetMotor1Direction(1); // 0:Forward, 1:Backward
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }
            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }
        }

        public void SetMtrDirFwd_RG()
        {
            int ret;
            checkI2CBusy();
            if (isI2CBusy)
                return;
            // the handler will be called before mDdp442x is initialized
            if (mDdp442x == null)
                return;

            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }

            ret = mDdp442x.SetMotor2Direction(0); // 0:Forward, 1:Backward
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }
            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }
        }

        public void SetMtrDirBwd_RG()
        {
            int ret;
            checkI2CBusy();
            if (isI2CBusy)
                return;
            // the handler will be called before mDdp442x is initialized
            if (mDdp442x == null)
                return;

            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }

            ret = mDdp442x.SetMotor2Direction(1); // 0:Forward, 1:Backward
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }
            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }
        }

        public void MotorStep(bool isFwd ,int stepCount, int motorNum, int freq)//motorNum1 = FG, motorNum2 = RG //FG-FREQ 50 RG-FREQ 100
        {
            int ret;
            checkI2CBusy();
            if (isI2CBusy)
                return;
            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }

            if (motorNum == 1)
            {
                if (!isFwd)
                {
                    ret = mDdp442x.SetMotor1Direction(1);
                    if (ret < 0)
                    {
                        pLog.Info(mDdp442x.GetLog());
                        return;
                    }
                }
                else
                {
                    ret = mDdp442x.SetMotor1Direction(0);
                    if (ret < 0)
                    {
                        pLog.Info(mDdp442x.GetLog());
                        return;
                    }
                }

                ret = mDdp442x.SetMotor1Step(stepCount, freq);
                if (ret < 0)
                {
                    pLog.Info(mDdp442x.GetLog());
                    return;
                }
                pLog.Info(mDdp442x.GetLog());
            }
            else if (motorNum == 2)
            {
                if (!isFwd)
                {
                    ret = mDdp442x.SetMotor2Direction(1);
                    if (ret < 0)
                    {
                        pLog.Info(mDdp442x.GetLog());
                        return;
                    }
                }
                else
                {
                    ret = mDdp442x.SetMotor2Direction(0);
                    if (ret < 0)
                    {
                        pLog.Info(mDdp442x.GetLog());
                        return;
                    }
                }

                ret = mDdp442x.SetMotor2Step(stepCount, freq);
                if (ret < 0)
                {
                    pLog.Info(mDdp442x.GetLog());
                    return;
                }
                pLog.Info(mDdp442x.GetLog());
            }

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }
            Console.Write("stepCount={0}, motorNum={1}, Freq={2}", stepCount, motorNum, freq);
        }

        public void MtrHome(int motorNum ,ref string statusStr)//FG- 1- FREQ 50 RG-2 -FREQ 100
        {
            checkI2CBusy();
            if (isI2CBusy)
                return;

            int ret;
            int freq1 = 50;
            int freq2 = 100;
            byte status;

            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C() ;
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }
            if ( motorNum ==1)
            {
                ret = mDdp442x.SetMotorHoming(0, freq1);
                if (ret < 0)
                {
                    pLog.Info(mDdp442x.GetLog());
                    return;
                }
                ret = mDdp442x.CloseI2C();
                if (ret < 0)
                {
                   MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                    return;
                }

                pLog.Info(mDdp442x.GetLog());


                while (true)
                {
                    checkI2CBusy();
                    if (!isI2CBusy)
                        break;
                    Thread.Sleep(100);
                }

                ret = mDdp442x.OpenI2C();
                if (ret < 0)
                {
                    MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                    return;
                }
                ret = mDdp442x.GetHomingInformation(0, out status);
                if (ret < 0)
                {
                    pLog.Info(mDdp442x.GetLog());
                    return;
                }
                pLog.Info(mDdp442x.GetLog());

                statusStr = string.Format("0x{0:X2}", status);
            }
            else if( motorNum ==2)
            {
                ret = mDdp442x.SetMotorHoming(1, freq2);
                if (ret < 0)
                {
                    pLog.Info(mDdp442x.GetLog());
                    return;
                }

                ret = mDdp442x.OpenI2C();
                if (ret < 0)
                {
                    MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                    return;
                }

                pLog.Info(mDdp442x.GetLog());


                while (true)
                {
                    checkI2CBusy();
                    if (!isI2CBusy)
                        break;
                    Thread.Sleep(100);
                }

                ret = mDdp442x.OpenI2C();
                if (ret < 0)
                {
                    MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                    return;
                }
                ret = mDdp442x.GetHomingInformation(1, out status);
                if (ret < 0)
                {
                    pLog.Info(mDdp442x.GetLog());
                    return;
                }
                pLog.Info(mDdp442x.GetLog());

                statusStr = string.Format("0x{0:X2}", status);

            }
            
            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }

        }


        #endregion //MOTOR 

        #region Flip

        public void ProjectorFlip(int flipMode) //0-No 1-H  2-V 3-HV
        {
            checkI2CBusy();
            if (isI2CBusy)
                return;
            byte ProjectorFlipV = 0x00;     //sender.Equals(RadioProjectorFlipNo)
            byte ProjectorFlipH = 0x00;
            if (flipMode ==2)
            {
                ProjectorFlipV = 0x01;
                ProjectorFlipH = 0x00;
            }
            else if (flipMode ==1)
            {
                ProjectorFlipV = 0x00;
                ProjectorFlipH = 0x01;
            }
            else if (flipMode ==3)
            {
                ProjectorFlipV = 0x01;
                ProjectorFlipH = 0x01;
            }

            int ret;
            mDdp442x.ClearLog();

            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
            }

            ret = mDdp442x.SetProjectorFlipV(ProjectorFlipV);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }
            ret = mDdp442x.SetProjectorFlipH(ProjectorFlipH);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }

            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }
        }

        public int GetProjectorFlip() //0-No 1-H  2-V 3-HV
        {
            checkI2CBusy();
            if (isI2CBusy)
                return -1 ;
            int ret;
            if (mDdp442x == null)
                return -1;

            byte ProjectorFlipV = 0x00;
            byte ProjectorFlipH = 0x00;

            mDdp442x.ClearLog();
            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return -1;
            }

            mDdp442x.GetProjectorFlip(out ProjectorFlipH, out ProjectorFlipV);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return -1;
            }

            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return -1;
            }

            if (ProjectorFlipH == 0 && ProjectorFlipV == 0)
                return 0;
            else if (ProjectorFlipH == 0 && ProjectorFlipV == 1)
                return 2;
            else if (ProjectorFlipH == 1 && ProjectorFlipV == 0)
                return 1;
            else if (ProjectorFlipH == 1 && ProjectorFlipV == 1)
                return 3;

            return -1;
        }

        #endregion Flip

        #region Fan

        public List<int> GetFanSpeed()
        {
            checkI2CBusy();
            if (isI2CBusy)
                return new List<int>();
            int ret;
            if (mDdp442x == null)
                return new List<int>();

            mDdp442x.ClearLog();
            ret = mDdp442x.OpenI2C() ;
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return new List<int>();
            }
            //Read Fan Speed
            int DMDDuty, LED1Duty, LED2Duty;
            ret = mDdp442x.GetFanDuty(out DMDDuty, out LED1Duty, out LED2Duty);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return new List<int>();
            }
            List<int> dutys = new List<int>();
            dutys.Add(DMDDuty);
            dutys.Add(LED1Duty);
            dutys.Add(LED2Duty);
            
            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return new List<int>();
            }
            return dutys;
        }

        private void ButtonSetFan_Click(int fanIndex , int duty)// fanindex 0-DMD 1-LED1 2-LED2
        {
            checkI2CBusy();
            if (isI2CBusy)
                return;
            int dutyCycle = 0x00;
            int ret;
            String FanName = "";
            if (fanIndex ==0)
            {
                FanName = "DMDFan";
                try
                {
                    dutyCycle = duty;
                    if (dutyCycle > 100 || dutyCycle < 0)
                        throw new Exception("Value is not in the range(0~100)");
                }
                catch (Exception exception)
                {
                    MessageInfoUpdate_Thread(exception.Message);
                    return;
                }
            }
            else if (fanIndex == 1)
            {
                FanName = "LEDFan1";
                try
                {
                    dutyCycle = duty;
                    if (dutyCycle > 100 || dutyCycle < 0)
                        throw new Exception("Value is not in the range(0~100)");
                }
                catch (Exception exception)
                {
                    MessageInfoUpdate_Thread(exception.Message);
                    return;
                }
            }
            else if (fanIndex ==2)
            {
                FanName = "LEDFan2";
                try
                {
                    dutyCycle = duty;
                    if (dutyCycle > 100 || dutyCycle < 0)
                        throw new Exception("Value is not in the range(0~100)");
                }
                catch (Exception exception)
                {
                    MessageInfoUpdate_Thread(exception.Message);
                    return;
                }
            }

            Console.WriteLine("dutyCycle = {0}", dutyCycle);

            mDdp442x.ClearLog();
            ret = mDdp442x.OpenI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
            }

            ret = mDdp442x.SetFanDuty(FanName, dutyCycle);
            if (ret < 0)
            {
                pLog.Info(mDdp442x.GetLog());
                return;
            }

            pLog.Info(mDdp442x.GetLog());

            ret = mDdp442x.CloseI2C();
            if (ret < 0)
            {
                MessageInfoUpdate_Thread("I2C CommunicationFAIL!!!", LogLevel.Warn);
                return;
            }
        }

        #endregion Fan

        #region DMD Park
        public bool SetDmdPark(bool enable)
        {
            const int cWriteSize = 2;
            byte[] sendBuf = new byte[cWriteSize];
            int ret;


            mDdp442x.InitLog(CommandType.WRITE, CommandId.DMD_PARK, cSlaveAddress7bit);

            sendBuf[0] = (byte)CommandId.DMD_PARK;
            sendBuf[1] = (byte)(enable ? 0x01 : 0x00);

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);
            if (ret < 0)
            {
                mDdp442x.mLogString += " (Fail)";
                return false;
            }

            mDdp442x.AppendLogSend(sendBuf);

            return true;
        }
        public bool GetDmdPark()
        {
            const int cWriteSize = 2;
            const int cReadSize = 1;
            byte[] sendBuf = new byte[cWriteSize];
            byte[] recvBuf = new byte[cReadSize];
            int ret;

            bool isPark = false;

            mDdp442x.InitLog(CommandType.READ, CommandId.DMD_PARK, cSlaveAddress7bit);
            sendBuf[0] = (byte)CommandId.I2C_READ;           //0x15
            sendBuf[1] = (byte)CommandId.DMD_PARK;           //0x87

            ret = mCpressI2c.Write(cSlaveAddress7bit, sendBuf, cWriteSize);//測試移除stopbit = false
            if (ret < 0)
            {
                mDdp442x.mLogString += " (Fail)";
                return isPark;
            }
            Thread.Sleep(100);
            ret = mCpressI2c.Read(cSlaveAddress7bit, ref recvBuf, cReadSize);
            if (ret < 0)
            {
                mDdp442x.mLogString += " (Fail)";
                return isPark;
            }

            isPark = recvBuf[0] == 1 ? true : false;

            mDdp442x.AppendLogRecv(recvBuf);

            return isPark;
        }

        #endregion
    }

}
