using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using static AngelDLP.ViewModels.MainWindowViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AngelDLP.PMC;
using AngleDLP.Models;
using AngelDLP.PPC;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using OpenCvSharp;
using AngleDLP.PPC;
using OpenCvSharp.Extensions;
using System.Drawing;
using OxyPlot.Wpf;
using static AngleDLP.Models.PlatformCalib;

namespace AngelDLP.ViewModels
{
    public class PrintControlViewModel : BindableBase
    {
        private static OxyPlot.OxyColor[] colors = { OxyPlot.OxyColors.Red, OxyPlot.OxyColors.Green, OxyPlot.OxyColors.Blue };
        PrinterProjectionControl _ppc;
        private PrinterMotionControl _pmc;
        DispatcherTimer axStsMonitor_timer;

        public PrintTask printTask;

        private int imgUpdateCount = 0;
        public ObservableCollection<string> CameraList { get; set; }
        private int tempRefleshCount = 0;

        public class WindowControlEvent : PubSubEvent<string>
        {

        }

        

        #region PMC_UI_Binding
        public class AddProjectorPropertyChangedEven : PubSubEvent { }

        private void ProjectorOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _ = App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var p = sender as Projector;

                if (p != null)
                {
                    if (e.PropertyName == "ONOFF")
                    {
                        if (p.IsON)
                        {
                            ProjectorOnOffStr[p.projectorIndex] = "Projector On";
                            ProjectorOnOffIconColor[p.projectorIndex] = new SolidColorBrush(Colors.Green);
                        }
                        else
                        {
                            ProjectorOnOffStr[p.projectorIndex] = "Projector Off";
                            ProjectorOnOffIconColor[p.projectorIndex] = new SolidColorBrush(Colors.Red);
                        }
                    }
                    if (e.PropertyName == "LEDONOFF")
                    {
                        if (p.IsLEDON)
                        {
                            ProjectorLEDOnOffStr[p.projectorIndex] = "LED On2";
                            ProjectorLEDOnOffIconColor[p.projectorIndex] = new SolidColorBrush(Colors.Green);
                        }
                        else
                        {
                            ProjectorLEDOnOffStr[p.projectorIndex] = "LED Off2";
                            ProjectorLEDOnOffIconColor[p.projectorIndex] = new SolidColorBrush(Colors.Red);
                        }
                    }
                }
            }));


        }

        public DelegateCommand<string> ProjectorControlCommand { get; private set; }

        private ObservableCollection<string> projectorOnOffStr = new ObservableCollection<string> { "Projector Off", "Projector Off" };
        public ObservableCollection<string> ProjectorOnOffStr
        {
            set
            {
                SetProperty(ref projectorOnOffStr, value);
            }
            get { return projectorOnOffStr; }
        }

        private ObservableCollection<SolidColorBrush> projectorOnOffIconColor = new ObservableCollection<SolidColorBrush> { new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Red) };
        public ObservableCollection<SolidColorBrush> ProjectorOnOffIconColor
        {
            set
            {
                SetProperty(ref projectorOnOffIconColor, value);
            }
            get { return projectorOnOffIconColor; }
        }

        private ObservableCollection<string> projectorLEDOnOffStr = new ObservableCollection<string> { "LED Off", "LED Off" };
        public ObservableCollection<string> ProjectorLEDOnOffStr
        {
            set
            {
                SetProperty(ref projectorLEDOnOffStr, value);
            }
            get { return projectorLEDOnOffStr; }
        }

        private ObservableCollection<SolidColorBrush> projectorLEDOnOffIconColor = new ObservableCollection<SolidColorBrush> { new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Red) };
        public ObservableCollection<SolidColorBrush> ProjectorLEDOnOffIconColor
        {
            set
            {
                SetProperty(ref projectorLEDOnOffIconColor, value);
            }
            get { return projectorLEDOnOffIconColor; }
        }

        private int selectedProjectorIndex = 0;
        public int SelectedProjectorIndex
        {
            set
            {
                SetProperty(ref selectedProjectorIndex, value);
            }
            get { return selectedProjectorIndex; }
        }
        private ObservableCollection<int> dac_ValueSet = new ObservableCollection<int> { 50, 50 };
        public ObservableCollection<int> DAC_ValueSet
        {
            set
            {
                SetProperty(ref dac_ValueSet, value);
            }
            get { return dac_ValueSet; }
        }

        private ObservableCollection<int> lightSensor_ValueSet = new ObservableCollection<int> { 50, 50 };
        public ObservableCollection<int> LightSensor_ValueSet
        {
            set
            {
                SetProperty(ref lightSensor_ValueSet, value);
            }
            get { return lightSensor_ValueSet; }
        }
        private ObservableCollection<string> funSpeedStrSet = new ObservableCollection<string> { "fun speed: ", "fun speed: " };
        public ObservableCollection<string> FunSpeedStrSet
        {
            set
            {
                SetProperty(ref funSpeedStrSet, value);
            }
            get { return funSpeedStrSet; }
        }
        private ObservableCollection<string> tempStrSet = new ObservableCollection<string> { "temp: ", "temp: " };
        public ObservableCollection<string> TempStrSet
        {
            set
            {
                SetProperty(ref tempStrSet, value);
            }
            get { return tempStrSet; }
        }
        private ObservableCollection<bool> rMotorDirSet = new ObservableCollection<bool> { true, true };
        public ObservableCollection<bool> RMotorDirSet
        {
            set
            {
                SetProperty(ref rMotorDirSet, value);
            }
            get { return rMotorDirSet; }
        }


        private ObservableCollection<bool> fMotorDirSet = new ObservableCollection<bool> { true, true };
        public ObservableCollection<bool> FMotorDirSet
        {
            set
            {
                SetProperty(ref fMotorDirSet, value);
            }
            get { return fMotorDirSet; }
        }

        private bool GetNextValue = false;

        private double grayItemValue = 0;
        public double GrayItemValue
        {
            set
            {
                SetProperty(ref grayItemValue, value);
            }
            get { return grayItemValue; }
        }
        private Visibility controlViewVisibility;
        public Visibility ControlViewVisibility
        {
            set
            {
                SetProperty(ref controlViewVisibility, value);
            }
            get { return controlViewVisibility; }
        }
        private double holeCalibX = 0;
        public double HoleCalibX
        {
            set
            {
                SetProperty(ref holeCalibX, value);
            }
            get { return holeCalibX; }
        }
        private double holeCalibY = 0;
        public double HoleCalibY
        {
            set
            {
                SetProperty(ref holeCalibY, value);
            }
            get { return holeCalibY; }
        }
        private double holeCalibRz = 0;
        public double HoleCalibRz
        {
            set
            {
                SetProperty(ref holeCalibRz, value);
            }
            get { return holeCalibRz; }
        }
        private double holeCalibScale= 0;
        public double HoleCalibScale
        {
            set
            {
                SetProperty(ref holeCalibScale, value);
            }
            get { return holeCalibScale; }
        }
        private BitmapSource resImage;
        public BitmapSource ResImage
        {
            set
            {
                SetProperty(ref resImage, value);
            }
            get { return resImage; }
        }
        private ObservableCollection<string> rMotorStatusSet = new ObservableCollection<string> { "", "" };
        public ObservableCollection<string> RMotorStatusSet
        {
            set
            {
                SetProperty(ref rMotorStatusSet, value);
            }
            get { return rMotorStatusSet; }
        }

        private ObservableCollection<string> fMotorStatusSet = new ObservableCollection<string> { "", "" };
        public ObservableCollection<string> FMotorStatusSet
        {
            set
            {
                SetProperty(ref fMotorStatusSet, value);
            }
            get { return fMotorStatusSet; }
        }

        private ObservableCollection<int> rMotorStepSet = new ObservableCollection<int> { 5, 5 };
        public ObservableCollection<int> RMotorStepSet
        {
            set
            {
                SetProperty(ref rMotorStepSet, value);
            }
            get { return rMotorStepSet; }
        }

        private ObservableCollection<int> fMotorStepSet = new ObservableCollection<int> { 5, 5 };
        public ObservableCollection<int> FMotorStepSet
        {
            set
            {
                SetProperty(ref fMotorStepSet, value);
            }
            get { return fMotorStepSet; }
        }

        private ObservableCollection<ObservableCollection<bool>> projectorFlipSet;
        public ObservableCollection<ObservableCollection<bool>> ProjectorFlipSet
        {
            set
            {
                SetProperty(ref projectorFlipSet, value);
            }
            get { return projectorFlipSet; }
        }

        public DelegateCommand ButtonInitMCB_ClickCommand { get; private set; }
        public DelegateCommand ButtonCloseMCB_ClikCommand { get; private set; }
        public DelegateCommand<string> ButtonEnable_ClickCommand { get; private set; }
        public DelegateCommand<string> ButtonDisable_ClickCommand { get; private set; }
        public DelegateCommand<string> ButtonBackOri_ClickCommand { get; private set; }
        public DelegateCommand<string> ButtonPTP_ClickCommand { get; private set; }
        public DelegateCommand<string> ButtonScrapter_ClickCommand { get; private set; }
        public DelegateCommand ButtonLevelRecord_ClickCommand { get; private set; }
        public DelegateCommand ButtonAddResind_ClickCommand { get; private set; }
        public DelegateCommand ButtonAddResindValue_ClickCommand { get; private set; }
        public DelegateCommand<string> ExposeCommand { get; private set; }
        public DelegateCommand<string> WindowControlCommand { get; private set; }

        public SolidColorBrush Icon_MCB_StatusFill { get; set; } = new SolidColorBrush(Colors.DarkGray);
        public SolidColorBrush Icon_WindowLock_StatusFill { get; set; } = new SolidColorBrush(Colors.DarkGray);
        public SolidColorBrush Icon_ScraperOut_StatusFill { get; set; } = new SolidColorBrush(Colors.DarkGray);
        public SolidColorBrush Icon_UpperDoorLock_StatusFill { get; set; } = new SolidColorBrush(Colors.DarkGray);
        public SolidColorBrush Icon_LowerDoorLock_StatusFill { get; set; } = new SolidColorBrush(Colors.DarkGray);
        public SolidColorBrush Icon_ResinAddBoxGood_StatusFill { get; set; } = new SolidColorBrush(Colors.DarkGray);
        public SolidColorBrush Icon_FootSwitchPressed_StatusFill { get; set; } = new SolidColorBrush(Colors.DarkGray);
        public string TextBox_ScraperPos { get; set; } = "0";
        public string SopText { get; set; } = "";
        public string TextBox_PlatformPos { get; set; } = "0";
        public string TextBox_WindowPos { get; set; } = "0";

        public string TextBox_ScraperSpeed { get; set; } = "10";
        public string TextBox_PlatformSpeed { get; set; } = "10";

        public string TextBox_WindowSpeed { get; set; } = "10";

        public string Label_ScraperPos { get; set; } = "0";

        public string Label_ScraperZleftPos { get; set; } = "0";

        public string Label_ScraperZRightPos { get; set; } = "0";

        public string Label_PlatformPos { get; set; } = "0";
        public string Label_WindowPos { get; set; } = "0";

        public string Label_ResinTemp { get; set; } = "0";
        public string Label_TankTemp { get; set; } = "0";

        public string Label_ControlTempGet { get; set; } = "32";

        public string Label_ControlTempSet { get; set; } = "32";

        public string Label_ResinAddBoxLevel { get; set; } = "32";
        public string ResinAddValueStatus { get; set; } = "x";


        public int Label_ResinAddBoxPercentage { get; set; } = 0;
        public string Label_ScrapeVacuum { get; set; } = "0";
        public string Label_EnvTemp { get; set; } = "0";
        public string Label_EnvHum { get; set; } = "0";

        public string TextBox_LevelboxPos { get; set; } = "0";

        public string Label_Level { get; set; } = "0";

        public string Label_LevelboxPos { get; set; } = "0";




        public bool IsChecked_ProjOnLeft { get; set; } = false;

        public bool IsChecked_ProjOnRight { get; set; } = false;

        public StyleValue Slider_PlatformDataContext { get; set; } = new StyleValue() { value = 4 };

        public StyleValue Slider_ScraperDataContext { get; set; } = new StyleValue() { value = 4 };

        public StyleValue Slider_LevelboxDataContext { get; set; } = new StyleValue() { value = 4 };

        public class StyleValue : BindableBase
        {
            public double value { get; set; }
        }
        #endregion

        public int SelectedTabIndex { get; set; } = 0;
        public IEventAggregator _ea;
        private string subViewName = "PrintControlView";

        public string DacValue { get; set; }
        public string ExposeTimeValue { get; set; }

        public DelegateCommand CloseCommand { get; private set; }

        public PrintControlViewModel(IEventAggregator ea, PrintTask printtask, PrinterMotionControl printerMotionControl, PrinterProjectionControl printerProjectionControl)
        {
            _ea = ea;
            _pmc = printerMotionControl;//用于板卡控制
            _ppc = printerProjectionControl;
            printTask = printtask;//目前仅用于获取液位设置数值
            CloseCommand = new DelegateCommand(Close);
            ButtonInitMCB_ClickCommand = new DelegateCommand(ButtonInitMCB_Click);
            ButtonCloseMCB_ClikCommand = new DelegateCommand(ButtonCloseMCB_Clik);
            ButtonEnable_ClickCommand = new DelegateCommand<string>(ButtonEnable_Click);
            ButtonDisable_ClickCommand = new DelegateCommand<string>(ButtonDisable_Click);
            ButtonBackOri_ClickCommand = new DelegateCommand<string>(ButtonBackOri_Click);
            ButtonPTP_ClickCommand = new DelegateCommand<string>(ButtonPTP_Click);
            ButtonScrapter_ClickCommand=new DelegateCommand<string> (ButtonScrapter_Click);
            ButtonLevelRecord_ClickCommand = new DelegateCommand(LevelRecord);
            ButtonAddResind_ClickCommand = new DelegateCommand(ResinAdd);
            ButtonAddResindValue_ClickCommand = new DelegateCommand(ResinAddValue);
            WindowControlCommand = new DelegateCommand<string>(WindowControl);
            ExposeCommand = new DelegateCommand<string>(ExposeCommandFunction);
            ProjectorControlCommand = new DelegateCommand<string>(ProjectorControl);
            _ea.GetEvent<WindowControlEvent>().Subscribe(message =>
            {
                if (message == "WindowUp")
                {
                    _pmc.StartJog(AxesEnum.Window, -1 * double.Parse(TextBox_WindowSpeed));
                    return;
                }
                if (message == "WindowDown")
                {
                    _pmc.StartJog(AxesEnum.Window, double.Parse(TextBox_WindowSpeed));
                    return;
                }
                if (message == "WindowStop")
                {
                    _pmc.StopMove(AxesEnum.Window);
                    return;
                }
            });
            _ea.GetEvent<AddProjectorPropertyChangedEven>().Subscribe(() =>
            {
                foreach (var p in _ppc.P_List)
                {
                    p.PropertyChanged += ProjectorOnPropertyChanged;
                }

            });
           
            axStsMonitor_timer = new DispatcherTimer();
            axStsMonitor_timer.Tick += new EventHandler(axStsMonitor_timer_Tick);
            axStsMonitor_timer.Interval = new TimeSpan(0, 0, 0, 0, 200);//设置时间间隔
            axStsMonitor_timer.Start();

        }

        private void ProjectorControl(string msg)
        {
            string[] msgList = msg.Split('-');
            int projIndex = int.Parse(msgList[0]) - 1;
            string cmdStr = msgList[1];
            string cmdPara = string.Empty;
            if (msgList.Count() > 2)
            {
                cmdPara = msgList[2];
            }
            if (cmdStr == "On")
            {
                _ppc.P_List[projIndex].ProjectorOn();
                return;
            }
            if (cmdStr == "Off")
            {
                _ppc.P_List[projIndex].ProjectorOff();
                return;
            }
            if (cmdStr == "LedOn")
            {
                _ppc.P_List[projIndex].LedOnOff(true);
                return;
            }
            if (cmdStr == "LedOff")
            {
                _ppc.P_List[projIndex].LedOnOff(false);
                return;
            }
            if (cmdStr == "LedGet")
            {
                //获取光源信息，电流
                DAC_ValueSet[projIndex] = _ppc.P_List[projIndex].GetLEDDAC();
                LightSensor_ValueSet[projIndex] = _ppc.P_List[projIndex].GetLightSensor();
                return;
            }
            if (cmdStr == "LedSet")
            {
                //设置电流
                _ppc.P_List[projIndex].SetLEDDAC(DAC_ValueSet[projIndex]);
                LightSensor_ValueSet[projIndex] = _ppc.P_List[projIndex].GetLightSensor();
                return;
            }
            if (cmdStr == "Source")
            {
                //设置图像
                _ppc.P_List[projIndex].PatternOrSource(int.Parse(cmdPara));
                return;
            }
            if (cmdStr == "Resolution")
            {
                //设置图像源
                _ppc.P_List[projIndex].SetProjectorMode(int.Parse(cmdPara));
                return;
            }
            if (cmdStr == "Flip")
            {
                //设置图像翻转
                _ppc.P_List[projIndex].ProjectorFlip(int.Parse(cmdPara));
                return;
            }
            if (cmdStr == "InfoGet")
            {
                //设置图像
                List<int> funSpeeds = _ppc.P_List[projIndex].GetFanSpeed();
                float dmdTemp = _ppc.P_List[projIndex].GetTemperature();
                float ledTemp = _ppc.P_List[projIndex].GetLEDTemperature();
                float boardTemp = _ppc.P_List[projIndex].GetLedBoardTemperature();
                FunSpeedStrSet[projIndex] = $"fun:{funSpeeds[0]},{funSpeeds[1]},{funSpeeds[2]}";
                TempStrSet[projIndex] = $"temp:{dmdTemp.ToString("0.0")},{ledTemp.ToString("0.0")},{boardTemp.ToString("0.0")}";
                return;
            }

            if (cmdStr == "FMotorHome")
            {
                //前群相机复位
                string status = string.Empty;
                _ppc.P_List[projIndex].MtrHome(1, ref status);
                FMotorStatusSet[projIndex] = status;
                return;
            }
            if (cmdStr == "RMotorHome")
            {
                //后群相机复位
                string status = string.Empty;
                _ppc.P_List[projIndex].MtrHome(2, ref status);
                RMotorStatusSet[projIndex] = status;
                return;
            }
            int freq1 = 50;
            int freq2 = 100;
            if (cmdStr == "FMotorStep")
            {
                //前群相机步进
                _ppc.P_List[projIndex].MotorStep(FMotorDirSet[projIndex], FMotorStepSet[projIndex], 1, freq1);
                return;
            }
            if (cmdStr == "RMotorStep")
            {
                //后群相机复位
                _ppc.P_List[projIndex].MotorStep(RMotorDirSet[projIndex], RMotorStepSet[projIndex], 2, freq2);
                return;
            }



        }

        //public void ProjGrayCalib()
        //{
        //    List<GrayCalib> clibItems = new List<GrayCalib>();
        //    for (int i = 180; i < 2160; i += 450)//1528 - 328 =1200
        //    {
        //        for (int j = 120; j < 3840; j += 450)//2712 - 312 =2400 ////0,8,36,44
        //        {
        //            new Thread(() =>
        //            {
        //                App.Current.Dispatcher.BeginInvoke(new Action(() =>
        //                {
        //                    _ppc.P_List[1].ProjectImageLED(GetProjImage(i, j, 120), 100000000, 1000);
        //                }));
        //            }).Start();

        //            //记录数据
        //            //通过一个状态量来记录Gray

        //            clibItems.Add(new GrayCalib()
        //            {
        //                X = i,
        //                Y = j,
        //                i = calibGray
        //            });

        //        }
        //    }
        //    using (FileStream fs = new FileStream($"D:\\grayCalib.json", FileMode.Create))
        //    {
        //        using (StreamWriter sw = new StreamWriter(fs))
        //        {
        //            sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(clibItems));
        //        }
        //    }



        //}
        public BitmapImage GetProjCubeImage(int x, int y, int r)//产生以投影仪左下角为原点，打印机XY坐标系方向的图像
        {
            int cvX = (int)_ppc._config.projectorConfigs[selectedProjectorIndex].printerPixHeight - y - 1;
            int cvY = x;
            Mat img = Mat.Zeros(new OpenCvSharp.Size((int)(int)_ppc._config.projectorConfigs[selectedProjectorIndex].printerPixWidth, (int)(int)_ppc._config.projectorConfigs[selectedProjectorIndex].printerPixHeight), new MatType(MatType.CV_8UC3));
            //img.Set<Vec3b>(cvX, cvY, new Vec3b(255, 255, 255));

            for (int mi = cvX - r; mi < cvX + r + 1; mi++)
            {
                for (int mj = cvY - r; mj < cvY + r + 1; mj++)
                {
                    img.Set<Vec3b>(mi, mj, new Vec3b(255, 255, 255));
                }
            }
            //img.ToBitmap().Save("D:\\x.png");
            return BitmapToBitmapImage(img.ToBitmap());
        }

        public BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bit3 = new BitmapImage();
            bit3.BeginInit();
            bit3.StreamSource = ms;
            bit3.EndInit();
            return bit3;
        }
        private void Close()
        {
            SelectedTabIndex = 0;
            _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
        }



        private void Window_Loaded(object sender, EventArgs e)
        {

        }


        private void ButtonInitMCB_Click()
        {
            if (_pmc.Init())
            {
            }
            else
            {
                _pmc.Close();
                _pmc.Init();
            }
        }

        private void ResinAddValue()
        {
            if (_pmc.GetIO(DIEnum.IsResinBlocked) == 0)
            {
                _pmc.AddResinValueSwitch(false);
            }
            else
            {
                _pmc.AddResinValueSwitch(true) ;
            }
        }
        
        private  void ResinAdd()
        {
            if (ClassValue.CanAddResin)
            {
                _ea.GetEvent<AddResinEvent>().Publish();
            }
            else
            {
                _ea.GetEvent<MessageEvent>().Publish("正在补液中");
            }


        }
        private void LevelRecord()
        {
            //记录液位并保存
            //每0.1s记录一个数据，共记录10000

            new Task(() =>
            {

                while (true)
                {
                    List<DateTime> times = new List<DateTime>();
                    List<double> levels = new List<double>();
                    List<double> temp1s = new List<double>();
                    List<double> temp2s = new List<double>();
                    List<double> temp3s = new List<double>();
                    List<double> temp4s = new List<double>();
                    List<double> scs = new List<double>();
                    int i = 0;
                    while (i++ < 36000)
                    {
                        if (i % 100 == 0)
                        {
                            _ea.GetEvent<MessageEvent>().Publish($"采集第{i}/{36000}个数据");
                        }
                        times.Add(DateTime.Now);
                        levels.Add(_pmc.GetAD(ADEnum.Level));
                        temp1s.Add(_pmc.GetAD(ADEnum.ResinTemp));
                        //temp2s.Add(_pmc.GetAD(ADEnum.ResinTemp2));
                        //temp3s.Add(_pmc.GetAD(ADEnum.ResinTemp3));
                        //temp4s.Add(_pmc.GetAD(ADEnum.ResinTemp4));
                        scs.Add(_pmc.GetAD(ADEnum.ScraperVacuum));
                        Thread.Sleep(100);
                    }
                    using (FileStream fs = new FileStream($"{times[0].ToString("dd-HH-mm")}__{times[times.Count - 1].ToString("dd-HH-mm")}__levels.txt", FileMode.OpenOrCreate))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            int j = 0;
                            foreach (var l in levels)
                            {
                                sw.WriteLine($"{times[j].ToString("dd-HH-mm-ss-ff")},{l.ToString("0.000")},{temp1s[j].ToString("0.000")},{temp2s[j].ToString("0.000")},{temp3s[j].ToString("0.000")},{temp4s[j].ToString("0.000")},{scs[j].ToString("0.0")}");
                                j++;
                            }
                        }
                    }
                }

            }
            ).Start();

        }
        private void updateHoleImage()
        {

            new Thread(() =>
            {

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        var sliceParas = new SliceParas()
                        {
                            projectorPixScaleX = holeCalibScale,
                            projectorPixScaleY = holeCalibScale,
                            calibScaleFix = _ppc._config.projectorConfigs[selectedProjectorIndex].calibScaleFix,
                            holeRz = _ppc._config.projectorConfigs[selectedProjectorIndex].holeRz,
                            holeOffsetX = HoleCalibX,
                            holeOffsetY = holeCalibY,
                            calibMartix = _ppc._config.projectorConfigs[selectedProjectorIndex].calibMartix,
                            holeComps = new List<double>() { 2.8 },
                            width = _ppc._config.projectorConfigs[selectedProjectorIndex].printerPixWidth,
                            height = _ppc._config.projectorConfigs[selectedProjectorIndex].printerPixHeight
                        };
                        _ea.GetEvent<MessageEvent>().Publish(_ppc._config.projectorConfigs[selectedProjectorIndex].calibMartix.ToString());
                        Mat mat = new Mat(_ppc._config.projectorConfigs[selectedProjectorIndex].printerPixHeight, _ppc._config.projectorConfigs[selectedProjectorIndex].printerPixWidth, MatType.CV_8U);
                        Cv2.Rectangle(mat, new OpenCvSharp.Rect(0, 0, _ppc._config.projectorConfigs[selectedProjectorIndex].printerPixWidth, _ppc._config.projectorConfigs[selectedProjectorIndex].printerPixHeight), Scalar.White, -1);
                        AngelSlicer.DrawPlatformHole_OpenCV(ref mat, sliceParas,0, new g3.Vector2d(0, 0), true);
                        var bitmapimage = BitmapToBitmapImage(mat.ToBitmap());
                        _ppc.P_List[selectedProjectorIndex].ProjectImageLED(_ppc.P_List[selectedProjectorIndex].TransProjectImage(bitmapimage), 100000000, 150);
                    }
                    catch (Exception ex)
                    {
                        _ea.GetEvent<MessageEvent>().Publish(ex.ToString());
                    }

                }));
            }).Start();

        }

        private void WindowControl(string para)
        {
            if (para == "Home")
            {
                _pmc.BackOri(AxesEnum.Window);
                return;
            }
            if (para == "Init")
            {
                var checktask = new Task(() =>
                {
                    var res = DialogHelper.ShowYONDialog("警告", $"是否初始化升降窗？\n 请确认上下门已关好，升降窗位置处于锁定状态。", DialogHelper.DialogType.YesNo);
                    if (res)
                    {
                        if (_pmc.GetIO(DIEnum.WindowUnlock) == 0 && _pmc.GetIO(DIEnum.DoorLocked) == 1)
                        {
                            //确认
                            //升降窗锁定状态
                            //上下门锁定状态
                            //初始化逻辑
                            //获取当前窗位置
                            //向上运动5mm
                            //解锁
                            //升降窗回原点
                            //关门
                            _pmc.Enable(AxesEnum.Window);
                            Thread.Sleep(300);
                            double currPos = _pmc.GetAxisPos_MM(AxesEnum.Window);
                            _pmc.StartJog(AxesEnum.Window, -4);
                            int waitCountMove = 0;
                            while (currPos - _pmc.GetAxisPos_MM(AxesEnum.Window) < 5)
                            {

                                if (++waitCountMove > 300)
                                {
                                    _ea.GetEvent<MessageEvent>().Publish("等待移动超时");
                                    return;
                                }
                                Thread.Sleep(100);
                            }
                            _pmc.StopMove(AxesEnum.Window);

                            if (_pmc.WindowUnlock())
                            {
                                _ea.GetEvent<MessageEvent>().Publish("升降门已解锁");
                            }
                            else
                            {
                                DialogHelper.ShowMessageDialog("错误", " 解锁超时!");
                                return;
                            }
                            
                            
                            _pmc.BackOri(AxesEnum.Window);
                            while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Window)) > 0.2)
                            {
                                Thread.Sleep(100);
                            }
                            Thread.Sleep(1000);
                            
                        }
                        else
                        {
                            DialogHelper.ShowMessageDialog("错误", " 请确认上下门已关好，升降窗位置处于锁定状态!");

                        }
                        
                    }
                });
                checktask.Start();
                return;
            }
            if (para == "Open")
            {
                if (_pmc.isWindowClose())
                {
                    Task t1 = new Task(() =>
                    {
                        _pmc.OpenWindow();
                    });
                    t1.Start();

                }

                return;
            }
            if (para == "Close")
            {
                if (_pmc.isWindowOpen())
                {
                    Task t1 = new Task(() =>
                    {
                        _pmc.CloseWindow();
                    });
                    t1.Start();

                }

                return;
            }
            if (para == "Lock")
            {
                Task t1 = new Task(() =>
                {

                    if (_pmc.WindowLock())
                    {
                        _ea.GetEvent<MessageEvent>().Publish("升降门已锁定");
                    }
                    else
                    {
                        DialogHelper.ShowMessageDialog("错误", " 锁定超时!");
                    }

                });
                t1.Start();
                return;
            }
            if (para == "Unlock")
            {
                Task t1 = new Task(() =>
                {

                    if (_pmc.WindowUnlock())
                    {
                        _ea.GetEvent<MessageEvent>().Publish("升降门已解锁");
                    }
                    else
                    {
                        DialogHelper.ShowMessageDialog("错误", " 解锁超时!");
                    }

                });
                t1.Start();
                return;
            }
        }
        private void ExposeCommandFunction(string para)
        {
            if (para == "GrayCalibSop")
            {
                //List<PrintTask.LayoutItem> layoutitems = new List<PrintTask.LayoutItem>();
                //int col = 1;
                //for (int i = 100; i < 2160; i += 490)//5 点
                //{
                //    int row = 1;
                //    for (int j = 100; j < 3840; j += 455)//2712 - 312 =2400 ////0,8,36,44
                //    {
                //        layoutitems.Add(new PrintTask.LayoutItem
                //        {
                //            Model = $"Detail_Test.stl",
                //            TX = i / 10,
                //            TY = j / 10,
                //            Matrix3D = new System.Windows.Media.Media3D.Matrix3D() { OffsetX = i / 10, OffsetY = j / 10, OffsetZ = 0 }
                //        });
                //        layoutitems.Add(new PrintTask.LayoutItem
                //        {
                //            Model = $"Detail_Test.stl",
                //            TX = 216 + i / 10,
                //            TY = j / 10,
                //            Matrix3D = new System.Windows.Media.Media3D.Matrix3D() { OffsetX = 216 + i / 10, OffsetY = j / 10, OffsetZ = 0 }
                //        });
                //    }
                //    col++;
                //}
                //using (FileStream fs = new FileStream($"D:\\detail-layout.json", FileMode.Create))
                //{
                //    using (StreamWriter sw = new StreamWriter(fs))
                //    {
                //        sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(layoutitems));
                //    }
                //}
                SopText = "光强校准操作流程:\n 1-  选择要校准的光机；\n 2- 若槽内有树脂需放出至低于刮刀30mm； \n 3- 更换干净网板，网板上铺A4纸； \n 4- 点击开始 \n 5- 光机投射光斑； \n 6- 将照度计探头放置于网板表面，并对准光斑； \n 7- 读取照度计数值，填入输入框； \n 8- 点击下一点，重复步骤5-8直至完成。";
                return;
            }
            if (para == "StartGrayCalib")
            {
                Task t1 = new Task(() =>
                {
                    _pmc.Enable(AxesEnum.Scraper);
                    _pmc.Enable(AxesEnum.Plaform);
                    _pmc.MoveScraper2Side(false);
                    Thread.Sleep(1000);

                    
                    _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, 14, 5);
                    while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) - 14) > 0.1)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    }
                    //开始投影第一点，等待输入和next
                    List<GrayCalibItem> clibItems = new List<GrayCalibItem>();


                    for (int i = 100; i < 2160; i += 490)//5 点
                    {
                        for (int j = 100; j < 3840; j += 455)//2712 - 312 =2400 ////0,8,36,44
                        {
                            new Thread(() =>
                            {
                                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _ppc.P_List[selectedProjectorIndex].ProjectImageLED(_ppc.P_List[selectedProjectorIndex].TransProjectImage(GetProjCubeImage(i, j, 95)), 100000000, 1000);
                                }));
                            }).Start();

                            //记录数据
                            //通过一个状态量来记录Gray
                            while (!GetNextValue)
                            {
                                Thread.Sleep(100);
                            }

                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                clibItems.Add(new GrayCalibItem()
                                {
                                    X = i,
                                    Y = j,
                                    i = GrayItemValue
                                });
                                GetNextValue = false;
                                SopText = $"当前点：\n    坐标：{i},{j}\n     光强：{GrayItemValue}";
                            }));
                            Thread.Sleep(100);

                        }
                    }
                    using (FileStream fs = new FileStream($"D:\\grayCalib-{selectedProjectorIndex}.json", FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(clibItems));
                        }
                    }

                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ResImage = LoadGrayCalibrationTestData(Newtonsoft.Json.JsonConvert.SerializeObject(clibItems), $".\\configs\\P{selectedProjectorIndex + 1}LightCalib.cfg");
                    }));
                    _ppc.P_List[selectedProjectorIndex].LedOnOff(false);
                });
                t1.Start();
                return;
            }
            if (para == "GrayCalibNext")
            {
                GetNextValue = true;
                return;
            }
            if (para == "GrayCalibViewer")
            {

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    using (FileStream fs = new FileStream(_ppc._config.projectorConfigs[selectedProjectorIndex].grayCalibParas.configInfo.path, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {

                            ResImage = LoadGrayJson(sr.ReadToEnd());

                        }
                    }
                }));


                return;
            }


            if (para == "HoleCalibSop")
            {
                SopText = "光强校准操作流程:\n 1-  确认槽内没有树脂，网板干净；\n 2- 选择要校准的光机； \n 3- 点击开始校准（Z轴回位，投影网孔图）； \n 4- 点击X-Y加减号，调整网孔图与网板孔重合 \n 5- 点击完成校准； ";
                return;
            }
            if (para == "HoleCalibStart")
            {
                //刮刀和平台使能
                _pmc.Enable(AxesEnum.Scraper);
                _pmc.Enable(AxesEnum.Plaform);
                //刮刀运动到外侧
                _pmc.MoveScraper2Side(false);
                Thread.Sleep(1000);

               
                //平台运动到0位`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
                _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, 0, 5);
                while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) - 0) > 0.1)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
                //按config初始化 xy补偿值
                HoleCalibX = _ppc._config.projectorConfigs[selectedProjectorIndex].holeOffsetX;
                HoleCalibY = _ppc._config.projectorConfigs[selectedProjectorIndex].holeOffsetY;
                HoleCalibRz = _ppc._config.projectorConfigs[selectedProjectorIndex].holeRz;
                HoleCalibScale = _ppc._config.projectorConfigs[selectedProjectorIndex].projectorPixScaleX;
                //更新投影图
                updateHoleImage();
                return;
            }
            if (para == "HoleCalibX-")
            {
                HoleCalibX -= 0.05;
            }
            if (para == "HoleCalibX+")
            {
                HoleCalibX += 0.05;
            }
            if (para == "HoleCalibY-")
            {
                HoleCalibY -= 0.05;
            }
            if (para == "HoleCalibY+")
            {
                HoleCalibY += 0.05;
            }
            if (para == "HoleCalibRz-")
            {
                HoleCalibRz += 0.00025;
            }
            if (para == "HoleCalibRz+")
            {
                HoleCalibRz -= 0.00025;
            }
            if (para == "HoleCalibScale-")
            {
                HoleCalibScale -= 0.0001;
            }
            if (para == "HoleCalibScale+")
            {
                HoleCalibScale += 0.0001;
            }
            if (para == "HoleCalibX-" || para == "HoleCalibX+" || para == "HoleCalibY-" || para == "HoleCalibY+" || para == "HoleCalibRz-" || para == "HoleCalibRz+" || para == "HoleCalibScale-" || para == "HoleCalibScale+")
            {
                //更新投影图片

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _ppc._config.projectorConfigs[selectedProjectorIndex].holeOffsetX = HoleCalibX;
                    _ppc._config.projectorConfigs[selectedProjectorIndex].holeOffsetY = HoleCalibY;
                    _ppc._config.projectorConfigs[selectedProjectorIndex].holeRz = HoleCalibRz;
                    _ppc._config.projectorConfigs[selectedProjectorIndex].projectorPixScaleX = HoleCalibScale;
                    _ppc._config.projectorConfigs[selectedProjectorIndex].projectorPixScaleY = HoleCalibScale;
                }));
                updateHoleImage();
                return;
            }
            if (para == "HoleCalibEnd")
            {
                //_ppc._config.projectorConfigs[selectedProjectorIndex].calibMartix[0, 2] = Math.Round(holeCalibX, 3);
                //_ppc._config.projectorConfigs[selectedProjectorIndex].calibMartix[1, 2] = Math.Round(holeCalibY, 3);


                _ppc.WriteConfig();
                return;
            }

            if (para == "LevelCalibSop")
            {

                SopText = "液位自动校准方法: \n 平台零位时，网板与刮刀间隙在0.3-0.7mm； \n 平台零位时，液位调整至与平台基本齐平 \n 点击开始自动校准即可；\n 设备将进行如下动作： 1-  ；\n 1- 将刮刀移动到外侧； \n 2- 平台移动到-2mm位置； \n 3- 平台按0.1mm向上步进运动 \n 4- 每个步进等待5s采集液位； \n 5- 直至网板运动到+2mm结束数据采集； \n 6- 根据采集到的液位曲线自动计算推荐液位位置";
                return;
            }
            if (para == "LevelCalib")
            {
                Task t1 = new Task(() =>
                {
                    printTask._config = new PrintTaskConfig();
                    printTask._config.baseCalibParas.zInitComp = 0.22;
                    printTask._config.motionConfig.motionSpeedConfig.platformInitSpeed = 20;
                    printTask._config.motionConfig.motionSpeedConfig.scraperInitSpeed = 100;
                    printTask._config.baseCalibParas.plateHeightest = -0.15;
                    printTask._config.baseCalibParas.plateLowest = -0.55;
                    
                    var resInit = printTask.PrintTaskInitForLevelCalib();
                    
                    resInit.Wait();
                    if (resInit.Result.isSuccess)
                    {
                       
                        List<List<double>> levels = new List<List<double>>();
                        _pmc.Enable(AxesEnum.Scraper);
                        _pmc.Enable(AxesEnum.Plaform);
                        _pmc.MoveScraper2Side(false);
                        Thread.Sleep(1000);

                        

                        for (int l = 0; l < 3; l++)
                        {
                            _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, -2, 5);
                            while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) + 2) > 0.1)
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                            }
                            //网板降低
                            //获取初步估计液位
                            //从-2mm开始步进
                            List<double> levelsDown = new List<double>();
                            for (int i = 0; i < 41; i++)
                            {
                                Thread.Sleep(6000);
                                levelsDown.Add(_pmc.GetAD(ADEnum.Level));
                                _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, -2 + 0.1 * i, 5);
                                while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) - (-2 + 0.1 * i)) > 0.02)
                                {
                                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                                }
                            }
                            levels.Add(levelsDown);
                        }
                        using (FileStream fs = new FileStream(".\\configs\\LevelInit.json", FileMode.OpenOrCreate))
                        {
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                //List<List<double>> levels = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<double>>>(sr.ReadToEnd()); ;
                                //LevelInitVisualize(levels);
                                sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(levels));

                            }
                        }
                        LevelInitVisualize(levels);


                    }
                    else
                    {

                    }
                    

                });
                t1.Start();
                return;
            }
            if (para == "LevelCalibViewer")
            {

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    using (FileStream fs = new FileStream(".\\configs\\LevelInit.json", FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            List<List<double>> levels = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<double>>>(sr.ReadToEnd()); ;
                            LevelInitVisualize(levels);

                        }
                    }
                }));

                return;
            }

            if (para == "BaseCalibInput")
            {
                Task t1 = new Task(() =>
                {
                    ////选择文件夹
                    //Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog()
                    //{
                    //    Multiselect = true,
                    //    Filter = "Excel Files (*.json)|*.json"
                    //};
                    //bool? result = openFileDialog.ShowDialog();
                    //if (result == true)
                    //{
                    //    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                    //    {
                    //        using (StreamReader  sr = new StreamReader(fs))
                    //        {
                    //            try {
                    //                var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GrayCalibItem>>(sr.ReadToEnd());
                    //                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    //                {
                    //                    ResImage = LoadBaseCalibrationTestData(Newtonsoft.Json.JsonConvert.SerializeObject(items), $".\\configs\\BaseCalib.cfg"); 
                    //                }));
                    //            }
                    //            catch(Exception e)
                    //            {
                    //                return;
                    //            }

                    //        }
                    //    }



                    //}
                    //else
                    //{
                    //    return ;
                    //}

                    // check if the 
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        using (FileStream fs = new FileStream(".\\configs\\platformtest.json", FileMode.Open))
                        {
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                PlatformTestResult platformtest = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatformTestResult>(sr.ReadToEnd()); ;
                                PlateFormInitVisualize(platformtest);

                            }
                        }
                    }));
                });
                t1.Start();
                return;
            }
            if (para == "LevelPrep")
            {

                LevelPrep();
                return;
            }
            if (para == "FloatExpose")
            {
                _ = App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    FloatExpose();
                }));
                
                return;
            }

        }
        public void LevelInitVisualize(List<List<double>> levels)
        {
            var model = new OxyPlot.PlotModel
            {
                Title = $"Level init ×{levels.Count}",
                Background = OxyPlot.OxyColors.White
            };
            int count = 0;
            using (FileStream fs = new FileStream(".\\configs\\LevelInit.txt", FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    //在零位时，平台在刮刀下多少
                    sw.WriteLine($"底面校准情况:");
                    double zcount = 0;
                    for (int y = 20; y < 400; y += 110)
                    {
                        double left = printTask.platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(y / 4))];
                        double right = printTask.platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(y / 4))];
                        sw.WriteLine($"  {left.ToString("0.00mm")}                              {right.ToString("0.00mm")}");
                        zcount += left;
                        zcount += right;
                        
                    }
                    double pUnders = zcount / 8;
                    sw.WriteLine($"  {printTask.platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(250 / 4))].ToString("0.00mm")}                              {printTask.platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(250 / 4))].ToString("0.00mm")}");
                    sw.WriteLine($"  {printTask.platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(140 / 4))].ToString("0.00mm")}                              {printTask.platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(140 / 4))].ToString("0.00mm")}");
                    sw.WriteLine($"  {printTask.platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(20 / 4))].ToString("0.00mm")}                              { printTask.platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(20 / 4))].ToString("0.00mm")}");
                    //sw.WriteLine($"打印初始平台位置:{_printTask.targetPlatePos.ToString("0.00mm")}");
                    //sw.WriteLine($"初始液位补偿")
                    SopText = "";
                    foreach (var li in levels)
                    {
                        var line = new OxyPlot.Series.LineSeries()
                        {
                            Title = $"Series {count + 1}",
                            Color = colors[count],
                            StrokeThickness = 1,
                            MarkerSize = 2,
                            MarkerType = OxyPlot.MarkerType.Circle
                        };
                        for (int i = 0; i < li.Count; i++)
                        {
                            line.Points.Add(new OxyPlot.DataPoint(-2 + i * 0.1, li[i]));
                        }
                        int splitInd = 0;
                        for (int i = 0; i < li.Count - 2; i++)
                        {
                            if (Math.Abs(li[i] - li[i + 2]) < 0.025)
                            {
                                splitInd = i;
                                break;
                            }
                        }
                        double SplitPos = -2 + splitInd * 0.1;
                        double SplitLevel = li[splitInd];
                        double levelAtP0 = Math.Round(li[splitInd] - SplitPos, 3);

                        var line2 = new OxyPlot.Series.LineSeries()
                        {
                            Title = $"b {count + 1}",
                            Color = colors[count],
                            StrokeThickness = 1,
                            MarkerSize = 1,
                            MarkerType = OxyPlot.MarkerType.Star
                        };
                        line2.Points.Add(new OxyPlot.DataPoint(SplitPos, li[splitInd]));
                        line2.Points.Add(new OxyPlot.DataPoint(0, levelAtP0));
                        var line3 = new OxyPlot.Series.LineSeries()
                        {
                            Title = $"c {count + 1}",
                            Color = colors[count],
                            StrokeThickness = 1,
                            MarkerSize = 1,
                            MarkerType = OxyPlot.MarkerType.None
                        };
                        line3.Points.Add(new OxyPlot.DataPoint(-2, levelAtP0));
                        line3.Points.Add(new OxyPlot.DataPoint(2, levelAtP0));
                        model.Series.Add(line);
                        model.Series.Add(line2);
                        model.Series.Add(line3);
                        count++;
                        //在平台零位时，液位与平台上表面平行的位置
                        //可以计算在此时，液位与刮刀平行的位置，这样以后液位都是相对值。
                        
                        double levelAtS0 = levelAtP0 + pUnders;
                        string currres = $"{count}: levelAtP0: {levelAtP0}   levelAtS0:{levelAtS0}\n";//print(f'suggest level for print: {round(ydown[splitInd] - (-2+splitInd*0.1),3) + 0.4}')
                        sw.WriteLine(currres);
                        SopText += currres;

                    }
                }
            }
            _ = App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var pngExporter = new PngExporter { Width = 800, Height = 600, };

                pngExporter.ExportToFile(model, ".\\configs\\LevelInit.png");
                var bitmapImage = new BitmapImage();
                using (FileStream fs = new FileStream(".\\configs\\LevelInit.png", FileMode.Open))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = fs;
                    bitmapImage.EndInit();
                }
                bitmapImage.Freeze();
                ResImage = ToBitmapSource(bitmapImage);
            }));


           
        }

        public void PlateFormInitVisualize(PlatformTestResult platformTestResult)
        {
            
            _ = App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var res = PlatformCalib.GetPlatformCalibResult(platformTestResult,_pmc._config);
                //var bitmapImage = LoadBaseCalibrationTestData(points, ".\\configs\\plate.png"); ;
                //using (FileStream fs = new FileStream(".\\configs\\plate.png", FileMode.Open))
                //{
                //    bitmapImage.BeginInit();
                //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                //    bitmapImage.StreamSource = fs;
                //    bitmapImage.EndInit();
                //}
                //bitmapImage.Freeze();
                
                ResImage = res.bitmapSource;
            }));
            

        }
        class GrayCalibItem
        {
            public int X { get; set; }
            public int Y { get; set; }
            public double i { get; set; }

        }


        public BitmapSource LoadGrayCalibrationTestData(string jsonstr,string filename)
        {

            List<GrayCalib> calibGrayPoint = new List<GrayCalib>();
            int grayDX = 40;
            DEMData calData;
            List<AltitudePoint> pointList = new List<AltitudePoint>();

            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GrayCalibItem>>(jsonstr);
            foreach (var it in items)
            {

                AltitudePoint altitudePoint = new AltitudePoint(it.X / grayDX, it.Y / grayDX, it.i);
                pointList.Add(altitudePoint);
            }

            InterpolationData interpolation = new InterpolationData(pointList, 2160 / grayDX, 3840 / grayDX);
            bool testbool = interpolation.IsRandomPointsOK();
            try
            {
                calData = new DEMData(interpolation.GetInterpolationData(1));
                interpolation.GetDrawingInfo();

                Bitmap bbm = calData.DEMBitmap();
                bbm.RotateFlip(RotateFlipType.Rotate90FlipXY);
                bbm.Save(filename.Replace(".cfg", ".bmp"));
                Console.WriteLine("最高值：" + calData.max.ToString("0.##") + "\n最低值：" + calData.min.ToString("0.##"));
                BitmapSource res = ToBitmapSource(bitmapToBitmapImage(bbm));
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        string jsonStrout = Newtonsoft.Json.JsonConvert.SerializeObject(calData);
                        sw.Write(jsonStrout);
                    }
                }
                return res;
            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
                return ResImage;
            }
        }
  

        
        public BitmapSource LoadGrayJson(string jsonstr)
        {
            try
            {
                DEMData calData = Newtonsoft.Json.JsonConvert.DeserializeObject<DEMData>(jsonstr); ;


                Bitmap bbm = calData.DEMBitmap();
                bbm.RotateFlip(RotateFlipType.Rotate90FlipXY);
                bbm.Save($"D:\\test-{selectedProjectorIndex}.bmp");
                //Console.WriteLine("最高值：" + calData.max.ToString("0.##") + "\n最低值：" + calData.min.ToString("0.##"));
                BitmapSource res = ToBitmapSource(bitmapToBitmapImage(bbm));

                return res;
            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
                return ResImage;
            }
        }

        private void LevelPrep()
        {
            //初始化动作
            //先将刮刀移动到外侧
            //int result = _pmc.GetIO(IOEnum.ResinBoxFloat);
            //_pmc.SetIO(IOEnum.ResinAdd, 1);
            //_pmc.SetIO(IOEnum.ResinAdd, 0);
            //_pmc.SetIO(IOEnum.Vacuum, 1);
            //_pmc.SetIO(IOEnum.Vacuum, 0);
            _pmc.Enable(AxesEnum.Scraper);
            _pmc.Enable(AxesEnum.Levelbox);
            _pmc.Enable(AxesEnum.Plaform);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            _pmc.MoveScraper2Side(false);

            while (_pmc.GetAxisPos_MM(AxesEnum.Scraper) < -1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }

            _pmc.PTP_MM(AxesEnum.Plaform, 2, 60000, 1000000, 1000000);
            _pmc.PTP_MM(AxesEnum.Levelbox, 0, 60000, 1000000, 1000000);
            while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) - 2) > 0.1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Thread.Sleep(TimeSpan.FromSeconds(3));
            //_pmc.PTP_MM(AxesEnum.Levelbox, 0, 60000, 1000000, 1000000);

            while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Levelbox)) > 0.1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Thread.Sleep(TimeSpan.FromSeconds(1));
            //获取当前液位
            //如果液位大于设定值，则提示排液
            //如果液位低于可调整值（1.08mm），则提示补液；
            //负压对于液位影响约1kPa -> 0.1mm
            //浮筒对于液位影响约1mm  -> 0.036mm
            //浮筒需留30mm调整余量，浮筒总行程60mm
            //负压有没有吸附上可以差0.5mm
            //负压破坏临界值-2mm


            //如果液位低于2mm,先补液至0.5mm，再进行自动调整
            if (_pmc.GetAD(ADEnum.Level) > 2)
            {
                //自动补液的动作
            }
            if (_pmc.GetAD(ADEnum.Level) > 1)
            {
                //提示补液
            }
            //第一次调整液位

            double gap = (0 - 0.2 - _pmc.GetAD(ADEnum.Level));
            double levelOffset1 = -gap / 0.036;
            _pmc.PTP_MM(AxesEnum.Levelbox, levelOffset1, 100000, 1000000, 1000000);
            while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Levelbox) - levelOffset1) > 0.1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            //刮刀动作
            _pmc.MoveScraper2Side(true, 700000);
            Thread.Sleep(TimeSpan.FromSeconds(4));
            _pmc.MoveScraper2Side(false, 700000);
            Thread.Sleep(TimeSpan.FromSeconds(4));
            Thread.Sleep(TimeSpan.FromSeconds(1));

            //第二次调整液位
            double gap2 = (0 - _pmc.GetAD(ADEnum.Level));
            double levelOffset2 = -gap2 / 0.036;
            _pmc.PTP_MM(AxesEnum.Levelbox, _pmc.GetAxisPos_MM(AxesEnum.Levelbox) + levelOffset2, 60000, 1000000, 1000000);
            //确认液位
            Thread.Sleep(TimeSpan.FromSeconds(5));
            double gap3 = (0 - _pmc.GetAD(ADEnum.Level));
            double levelOffset3 = -gap3 / 0.036;
            //第三次调整液位
            _pmc.PTP_MM(AxesEnum.Levelbox, _pmc.GetAxisPos_MM(AxesEnum.Levelbox) + levelOffset3, 60000, 1000000, 1000000);
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }

        private void FloatExpose()
        {
            ExposeTest.ProjRect(_ppc.P_List[selectedProjectorIndex], int.Parse(DacValue), double.Parse(ExposeTimeValue));
        }
        
        private double PosTrans10(double value, double zero, double lower, double upper)
        {
            double res = 10 * (10000 * value + zero - lower) / (upper - lower);
            return res;
        }



        private void axStsMonitor_timer_Tick(object sender, EventArgs e)//同步运动试图
        {
            if (ControlViewVisibility != Visibility.Visible)
            {
                return;
            }

            if (!_pmc._isReady)
            {
                Icon_MCB_StatusFill = new SolidColorBrush(Colors.Red);
                return;
            }
            else
            {
                Icon_MCB_StatusFill = new SolidColorBrush(Colors.Green);
            }

            try
            {
                //0-板卡
                //1-io
                if (SelectedTabIndex == 1) 
                {
                    //获取IO
                    
                    if (_pmc.GetIO(DIEnum.ScraperOut) == 1)
                    {
                        Icon_ScraperOut_StatusFill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        Icon_ScraperOut_StatusFill = new SolidColorBrush(Colors.Red);
                    }
                    
                    if (_pmc.GetIO(DIEnum.ResinAddBoxGood) == 1)
                    {
                        Icon_ResinAddBoxGood_StatusFill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        Icon_ResinAddBoxGood_StatusFill = new SolidColorBrush(Colors.Red);
                    }
                    if (_pmc.GetIO(DIEnum.FootSwitchRelease) == 1)
                    {
                        Icon_FootSwitchPressed_StatusFill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        Icon_FootSwitchPressed_StatusFill = new SolidColorBrush(Colors.Red);
                    }
                }
                //2-刮刀平台
                if (SelectedTabIndex == 2)
                {
                    double px = _pmc.GetAxisPos_MM(AxesEnum.Plaform);
                    double sx = _pmc.GetAxisPos_MM(AxesEnum.Scraper);
                    Label_PlatformPos = px.ToString("0.000") + "mm";
                    Label_ScraperPos = sx.ToString("0.0") + "mm";
                    Slider_PlatformDataContext.value = PosTrans10(px, _pmc._config.platformConfig.zeroPos, _pmc._config.platformConfig.startPos, _pmc._config.platformConfig.endPos);
                    Slider_ScraperDataContext.value = PosTrans10(sx, _pmc._config.scraperConfig.zeroPos, _pmc._config.scraperConfig.startPos, _pmc._config.scraperConfig.endPos);
                    double eta = _pmc.GetAD(ADEnum.EnvTemp);
                    double eha = _pmc.GetAD(ADEnum.EnvHum);

                    Label_EnvTemp = $"{eta.ToString("0.0")}℃";
                    Label_EnvHum = $"{eha.ToString("0.0")}%";
                    //位移传感器
                    Label_ScraperZleftPos = "";
                    foreach (var zs in _pmc._config.scraperConfig.zSensers)
                    {
                        Label_ScraperZleftPos += $"{_pmc.getScraperZ(zs.port).ToString("0.00mm")},";
                    }

                    double sa = _pmc.GetAD(ADEnum.ScraperVacuum);
                    Label_ScrapeVacuum = $"{sa.ToString("0")}Pa";
                }
                //3-液位浮筒
                if (SelectedTabIndex == 3)
                {
                    double lx = _pmc.GetAxisPos_MM(AxesEnum.Levelbox);
                    Label_LevelboxPos = lx.ToString("0.00") + "mm";
                    Slider_LevelboxDataContext.value = PosTrans10(lx, _pmc._config.levelBoxConfig.zeroPos, _pmc._config.levelBoxConfig.startPos, _pmc._config.levelBoxConfig.endPos);
                    double la = _pmc.GetAD(ADEnum.Level);
                    Label_Level = $"{la.ToString("0.000")}mm";

                    

                    double rt1a = _pmc.GetAD(ADEnum.ResinTemp);
                    Label_ResinTemp = $"{rt1a.ToString("0.0")}℃";

                    Label_TankTemp = "";

                    if (_pmc.GetIO(DIEnum.IsResinBlocked) == 1)
                    {
                        ResinAddValueStatus = "x";
                    }
                    else
                    {
                        ResinAddValueStatus = "<";
                    }
                    foreach (var cfg in _pmc._config.ADconfigs)
                    {
                        if (cfg.name == ADEnum.TankTemp.ToString())
                        {
                            Label_TankTemp += _pmc.GetAD(ADEnum.TankTemp, false, cfg.sn).ToString("0.0℃  ");
                        }
                    }

                    Label_ControlTempGet = _pmc.GetModBusAD(ModBusReadEnum.TempControlPV).ToString("0.0℃");
                    Label_ControlTempSet = _pmc.GetModBusAD(ModBusReadEnum.TempControlSV).ToString("0.0℃");
                    double ral = _pmc.GetModBusAD(ModBusReadEnum.ResinAddBoxLevel);
                    Label_ResinAddBoxLevel = ral.ToString("0.0");
                    Label_ResinAddBoxPercentage = (int)Math.Round(ral / 1.45, 0);
                }
                //6-升降门
                if (SelectedTabIndex == 6)
                {
                    if (_pmc.GetIO(DIEnum.WindowUnlock) == 1)
                    {
                        Icon_WindowLock_StatusFill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        Icon_WindowLock_StatusFill = new SolidColorBrush(Colors.Red);
                    }
                    if (_pmc.GetIO(DIEnum.DoorLocked) == 1)
                    {
                        Icon_UpperDoorLock_StatusFill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        Icon_UpperDoorLock_StatusFill = new SolidColorBrush(Colors.Red);
                    }
                    if (_pmc.GetIO(DIEnum.DoorLocked) == 1)
                    {
                        Icon_LowerDoorLock_StatusFill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        Icon_LowerDoorLock_StatusFill = new SolidColorBrush(Colors.Red);
                    }
                    double wx = _pmc.GetAxisPos_MM(AxesEnum.Window);
                    Label_WindowPos = wx.ToString("0.0") + "mm";
                }
            }
            catch (Exception)
            {
                Icon_MCB_StatusFill = new SolidColorBrush(Colors.Red);
                axStsMonitor_timer.Stop();
            }

        }

        private void ButtonCloseMCB_Clik()
        {
            _pmc.Close();
        }

        private void ButtonEnable_Click(string str)
        {
            _pmc.Enable((AxesEnum)int.Parse(str));
        }

        private void ButtonDisable_Click(string str)
        {
            _pmc.Disable((AxesEnum)int.Parse(str));
        }
        private void ButtonBackOri_Click(string str)
        {
            _pmc.BackOri((AxesEnum)int.Parse(str));
        }
        private void ButtonPTP_Click(string str)
        {
            double posValue = 0;
            double speedValue = 6;
            if (str == "0")//平台
            {
                //若平台目标小于0，则需确认刮刀在外侧位置
                if (Convert.ToDouble(TextBox_PlatformPos) < 0)
                {
                    if (_pmc.GetAxisPos_MM(AxesEnum.Scraper) < 0)
                    {
                        MessageBox.Show("请先将刮刀移到外侧");
                        return;
                    }
                }
                posValue = double.Parse(TextBox_PlatformPos);
                speedValue = double.Parse(TextBox_PlatformSpeed);
            }
            else if (str == "1")//浮筒
            {
                posValue = double.Parse(TextBox_LevelboxPos);
            }
            else if (str == "2")// 刮刀
            {
                //确认平台在正位置
                if (_pmc.GetAxisPos_MM(AxesEnum.Plaform) < 0)
                {
                    MessageBox.Show("平台位置过高，不可内移刮刀");
                    return;
                }

                posValue = double.Parse(TextBox_ScraperPos);
                if (posValue < _pmc.GetAxisPos_MM(AxesEnum.Scraper))
                {
                    MessageBox.Show("平台位置过高，不可内移刮刀");
                    return;
                }
                speedValue = double.Parse(TextBox_PlatformSpeed);
            }
            else if (str == "3")//浮筒
            {
                posValue = double.Parse(TextBox_WindowPos);
            }
            _pmc.PTP_MM((AxesEnum)int.Parse(str), posValue, speedValue * 10000, 1000000, 1000000);
        }
        private void ButtonScrapter_Click(string str)
        {
            //确认平台在正位置
            if (_pmc.GetAxisPos_MM(AxesEnum.Plaform) < 0)
            {
                MessageBox.Show("平台位置过高，不可内移刮刀");
                return;
            }
            
            if (str == "in")// 
            {
                Task.Run(() =>
                { _pmc.MoveScraper2Side(true); });
                
            }

            if (str == "out")// 
            {

                Task.Run(() =>
                { _pmc.MoveScraper2Side(false); });
            }
        }

        private void Button_PrintInit_Click(object sender, RoutedEventArgs e)
        {
            //this.Label_Info.Content = $"使能运动轴";
            _pmc.Enable(AxesEnum.Scraper);
            _pmc.Enable(AxesEnum.Levelbox);
            _pmc.Enable(AxesEnum.Plaform);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            _pmc.PTP_MM(AxesEnum.Levelbox, 0, 50000, 1000000, 1000000);//浮筒复位
            if (_pmc.GetAxisPos_MM(AxesEnum.Scraper) < 0)
            {
                _pmc.MoveScraper2Side(false);


            }
            Thread.Sleep(TimeSpan.FromSeconds(1));
            //上下搅动5次
            for (int i = 0; i < 5; i++)
            {
                _pmc.PTP_MM(AxesEnum.Plaform, 37, 400000, 1000000, 1000000);//平台下沉
                Thread.Sleep(TimeSpan.FromSeconds(2.5));
                _pmc.PTP_MM(AxesEnum.Plaform, -5, 400000, 1000000, 1000000);//平台下沉
                Thread.Sleep(TimeSpan.FromSeconds(2.5));
            }
            _pmc.PTP_MM(AxesEnum.Plaform, 5, 50000, 1000000, 1000000);//平台下沉
            Thread.Sleep(TimeSpan.FromSeconds(3));
            _pmc.MoveScraper2Side(true);

            Thread.Sleep(TimeSpan.FromSeconds(3));
            _pmc.MoveScraper2Side(false);
            Thread.Sleep(TimeSpan.FromSeconds(3));
            _pmc.PTP_MM(AxesEnum.Plaform, 0, 50000, 1000000, 1000000);//平台下沉
            Thread.Sleep(TimeSpan.FromSeconds(1));
            //if (!CheckComm())
            //{ _ppc.P_List[0].comm.Open(); }
            //CheckComm();
            //string str = projectorCurrentInfos[3].value;
            //byte[] dateArray = HexStringToByteArray(str);
            //_ppc.P_List[0].comm.WritePort(dateArray, 0, dateArray.Length);
            //_ppc.P_List[0].comm.Close();
        }


    }
}