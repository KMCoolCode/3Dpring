using ArtCamSdk;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using sun;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Windows.Interop;
using static AngelDLP.ViewModels.MainWindowViewModel;
using System.Text;
using System.Drawing.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Color = System.Drawing.Color;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using AngleDLP.Models;
using System.Threading.Tasks;
using AngelDLP.PPC;
using System.Windows.Threading;
using AngleDLP.PPC;
using OpenCvSharp;
using System.Runtime.InteropServices;
using Point = OpenCvSharp.Point;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using MathNet.Numerics.LinearAlgebra.Double;
using g3;
using DenseMatrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using HkCameraDisplay;

namespace AngelDLP.ViewModels
{
    public class ProjCalibratorViewModel : BindableBase
    {
        public IEventAggregator _ea;


        private PrinterProjectionControl _ppc;

        public PrinterProjectionControl PPC
        {
            set
            {
                SetProperty(ref _ppc, value);
            }
            get { return _ppc; }
        }


        private string subViewName = "ProjCalibrator";

        private bool isUVCameraLiveModeEnableed = false;
        public bool IsUVCameraLiveModeEnableed
        {
            set
            {
                SetProperty(ref isUVCameraLiveModeEnableed, value);
            }
            get { return isUVCameraLiveModeEnableed; }
        }

        private int liveStackNum = 3;
        public int LiveStackNum
        {
            set
            {
                SetProperty(ref liveStackNum, value);
            }
            get { return liveStackNum; }
        }

        private int LiveStackIndex = -1;
        private List<Mat> LiveStacks = new List<Mat>();
        ModbusTCP Modbus_TCP = new ModbusTCP();
        private bool IsClosed { get; set; }

        Boolean Isconnectd = false;
        DispatcherTimer axStsMonitor_timer;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string para)
        {
            if (para == "TBCItem")
            {

                return;
            }
            if (para == "ExposeTimeSet")
            {

                ExposeTimeMS = 0.005 + (1074 - ExposeTimeSet) * 0.075;
                return;
            }
        }

        private byte[] m_pCapture;
        private Bitmap m_Bitmap = null;
        private int m_PreviewMode = -1;
        private CArtCam m_CArtCam = new CArtCam();
        private int m_DllType = -1;

        private int m_DllSata = -1;
        private int m_SataType = -1;

        private IntPtr hwnd;

        public Visibility ProjCalibratorVisibility { get; set; } = Visibility.Visible;
        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand SaveParametersCommand { get; private set; }
        public DelegateCommand CaptureCommand { get; private set; }
        public DelegateCommand<string> CameraControlComand { get; private set; }
        public DelegateCommand<string> ProjectorControlCommand { get; private set; }
        public DelegateCommand<string> ProjectorCalibCommand { get; private set; }

        private BitmapSource uvImage;
        public BitmapSource UVImage
        {
            set
            {
                SetProperty(ref uvImage, value);
            }
            get { return uvImage; }
        }
        private BitmapFrame uvImageF;
        public BitmapFrame UVImageF
        {
            set
            {
                SetProperty(ref uvImageF, value);
            }
            get { return uvImageF; }
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

        private List<Point> twoPoints = new List<Point> { new Point(100, 100), new Point(1400, 2600) };

        public List<Point2d> ProjOAtCsCalib = new List<Point2d> { new Point2d(0, 0), new Point2d(216, 0) };
        public List<double> ProjScales = new List<double> { 0.1414, 0.1414 };


        [Serializable]
        public class CalibItem
        {
            public Point calibPix { get; set; }
            public Point2d calibCalib { get; set; }
            public Point2d calibCam { get; set; }
            public Point2d calibRes { get; set; }
            [JsonIgnore]
            public Bitmap calibImage { get; set; }
        }
        List<CalibItem> calibItems;
        [Serializable]
        public class FocusItem
        {
            public int fStep { get; set; }
            public int rStep { get; set; }
            public double score { get; set; }

            public Point2d camCenter { get; set; }
            [JsonIgnore]
            public Bitmap calibImage { get; set; }
        }


        private int fMotorStep = 780;
        public int FMotorStep
        {
            set
            {
                SetProperty(ref fMotorStep, value);
            }
            get { return fMotorStep; }
        }
        private int focusStart = 60;
        public int FocusStart
        {
            set
            {
                SetProperty(ref focusStart, value);
            }
            get { return focusStart; }
        }

        private int focusEnd = 80;
        public int FocusEnd
        {
            set
            {
                SetProperty(ref focusEnd, value);
            }
            get { return focusEnd; }
        }

        public List<Point2d> nFocusPoints = new List<Point2d> { new Point2d(164,156)    ,   new Point2d(1364,156),
                                                                    new Point2d(464,756), new Point2d(1064,756),
                                                                            new Point2d(764,1356),
                                                                    new Point2d(464,1956),new Point2d(1064,1956),
                                                                new Point2d(164,2556)   ,   new Point2d(1364,2556)
        };

        private ObservableCollection<ObservableCollection<double>> twoPointCalib = new ObservableCollection<ObservableCollection<double>>();
        public ObservableCollection<ObservableCollection<double>> TwoPointCalib
        {
            set
            {
                SetProperty(ref twoPointCalib, value);
            }
            get { return twoPointCalib; }
        }

        private Visibility twoPointCalibVisibility = Visibility.Collapsed;
        public Visibility TwoPointCalibVisibility
        {
            set
            {
                SetProperty(ref twoPointCalibVisibility, value);
            }
            get { return twoPointCalibVisibility; }
        }

        private string twoPointCalibResultString;
        public string TwoPointCalibResultString
        {
            set
            {
                SetProperty(ref twoPointCalibResultString, value);
            }
            get { return twoPointCalibResultString; }
        }
        private ObservableCollection<BitmapSource> pImage = new ObservableCollection<BitmapSource>();
        public ObservableCollection<BitmapSource> PImage
        {
            set
            {
                SetProperty(ref pImage, value);
            }
            get { return pImage; }
        }


        private int exposeTimeSet;
        public int ExposeTimeSet
        {
            set
            {
                SetProperty(ref exposeTimeSet, value);
                OnPropertyChanged("ExposeTimeSet");
            }
            get { return exposeTimeSet; }
        }

        private double exposeTimeMS;
        public double ExposeTimeMS
        {
            set
            {
                SetProperty(ref exposeTimeMS, value);
            }
            get { return exposeTimeMS; }
        }

        private int threshSet = 40;
        public int ThreshSet
        {
            set
            {
                SetProperty(ref threshSet, value);
            }
            get { return threshSet; }
        }

        private int threshElementSet = 8;
        public int ThreshElementSet
        {
            set
            {
                SetProperty(ref threshElementSet, value);
            }
            get { return threshElementSet; }
        }
        private double pointSetX;
        public double PointSetX
        {
            set
            {
                SetProperty(ref pointSetX, value);
                OnPropertyChanged("PointSetX");
            }
            get { return pointSetX; }
        }
        private double pointSetY;
        public double PointSetY
        {
            set
            {
                SetProperty(ref pointSetY, value);
                OnPropertyChanged("PointSetY");
            }
            get { return pointSetY; }
        }
        private Visibility pointSetVisibility = Visibility.Collapsed;
        public Visibility PointSetVisibility
        {
            set
            {
                SetProperty(ref pointSetVisibility, value);
            }
            get { return pointSetVisibility; }
        }


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


        private int projXSet = 1000;
        public int ProjXSet
        {
            set
            {
                SetProperty(ref projXSet, value);
            }
            get { return projXSet; }
        }

        private int projYSet = 1000;

        public int ProjYSet
        {
            set
            {
                SetProperty(ref projYSet, value);
            }
            get { return projYSet; }
        }

        private int projRadiusSet = 3;
        public int ProjRadiusSet
        {
            set
            {
                SetProperty(ref projRadiusSet, value);
            }
            get { return projRadiusSet; }
        }
        public ObservableCollection<TBClass> tbcs = new ObservableCollection<TBClass>();

        public ObservableCollection<TBClass> TBCS
        {
            set
            {

                SetProperty(ref tbcs, value);
                OnPropertyChanged("TBCS");
                //tbcs = value;
            }
            get { return tbcs; }

        }

        public class TBClass : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string para)
            {
                if (para == "valueX")
                {
                    PropertyChangedEventHandler handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs(para));
                    }
                    return;
                }


            }
            public string Tittle { get; set; }
            private int _value { get; set; }
            public int Value
            {
                get => _value;
                set
                {
                    _value = value;
                    OnPropertyChanged("valueX");
                }
            }
        }
        private double xPosValue = 0;
        public double XPosValue
        {
            set
            {
                SetProperty(ref xPosValue, value);
                OnPropertyChanged("XPosValue");
            }
            get { return xPosValue; }
        }
        private double xPosValueP = 0;
        public double XPosValueP
        {
            set
            {
                SetProperty(ref xPosValueP, value);
                OnPropertyChanged("XPosValueP");
            }
            get { return xPosValueP; }
        }
        private double yPosValue = 0;
        public double YPosValue
        {
            set
            {
                SetProperty(ref yPosValue, value);
                OnPropertyChanged("YPosValue");
            }
            get { return yPosValue; }
        }
        private double yPosValueP = 0;
        public double YPosValueP
        {
            set
            {
                SetProperty(ref yPosValueP, value);
                OnPropertyChanged("YPosValueP");
            }
            get { return yPosValueP; }
        }


        private Thickness uvCameraMargin = new Thickness(220, 0, 0, 100);
        public Thickness UVCameraMargin
        {
            set
            {
                SetProperty(ref uvCameraMargin, value);
            }
            get { return uvCameraMargin; }
        }

        private Thickness uvCameraMargin2 = new Thickness(220, 0, 0, 100);
        public Thickness UVCameraMargin2
        {
            set
            {
                SetProperty(ref uvCameraMargin2, value);
            }
            get { return uvCameraMargin2; }
        }
        //public SolidColorBrush xStatusColor { get; set; } = new SolidColorBrush(Colors.Red);

        //public SolidColorBrush yStatusColor { get; set; } = new SolidColorBrush(Colors.Red);


        private SolidColorBrush xStatusColor = new SolidColorBrush(Colors.Red);
        public SolidColorBrush XStatusColor
        {
            set
            {
                SetProperty(ref xStatusColor, value);
            }
            get { return xStatusColor; }
        }
        private SolidColorBrush xErrorStatusColor = new SolidColorBrush(Colors.Gray);
        public SolidColorBrush XErrorStatusColor
        {
            set
            {
                SetProperty(ref xErrorStatusColor, value);
            }
            get { return xErrorStatusColor; }
        }
        private SolidColorBrush yStatusColor = new SolidColorBrush(Colors.Gray);
        public SolidColorBrush YStatusColor
        {
            set
            {
                SetProperty(ref yStatusColor, value);
            }
            get { return yStatusColor; }
        }
        private SolidColorBrush yErrorStatusColor = new SolidColorBrush(Colors.Gray);
        public SolidColorBrush YErrorStatusColor
        {
            set
            {
                SetProperty(ref yErrorStatusColor, value);
            }
            get { return yErrorStatusColor; }
        }
        public class ShowProjCalibratorView_Event : PubSubEvent
        {

        }
        
        public class ModbusBtnUp_Even : PubSubEvent<string> { }
        public class ModbusBtnDown_Even : PubSubEvent<string> { }

        public class OnCalibViewInited_Even : PubSubEvent { }
        public List<string> Devices { get; set; }
        public static HkCamera camera { get; set; }

        
        public ProjCalibratorViewModel(IEventAggregator ea, PrinterProjectionControl printerProjectionControl)
        {
            //Devices = new List<string>();
            //var cameraList = new List<string>();
            //camera = new HkCamera();
            //camera.GetCameraList();
            //TBCS.CollectionChanged += bretList_CollectionChanged;
            hwnd = new WindowInteropHelper(App.Current.MainWindow).Handle;
            _ea = ea;
            PPC = printerProjectionControl;
            CloseCommand = new DelegateCommand(Close);
            SaveParametersCommand = new DelegateCommand(Savevalue);
            CaptureCommand = new DelegateCommand(Capture);
            CameraControlComand = new DelegateCommand<string>(CameraControl);//开关定时
            ProjectorControlCommand = new DelegateCommand<string>(ProjectorControl);
            ProjectorCalibCommand = new DelegateCommand<string>(ProjectorCalib);


            _ea.GetEvent<ShowProjCalibratorView_Event>().Subscribe(() =>
            {
                ProjCalibratorVisibility = Visibility.Visible;
            }
            );
            _ea.GetEvent<ModbusBtnUp_Even>().Subscribe(msg =>
            {
                //停止移动
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (msg == "49")
                    {
                        PLC_Output.b[4 - 1] = 0;
                        PLC_Output.b[9 - 1] = 0;
                    }
                    else
                    {
                        PLC_Output.b[int.Parse(msg) - 1] = 0;
                    }


                    message($"ModbusBtnUp_{msg}");
                }));

            }
            );

            _ea.GetEvent<ModbusBtnDown_Even>().Subscribe(msg =>
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (msg == "49")
                    {
                        PLC_Output.b[4 - 1] = 1;
                        PLC_Output.b[9 - 1] = 1;
                        message($"ModbusBtnDown_{msg}");
                    }
                    else
                    {
                        PLC_Output.b[int.Parse(msg) - 1] = 1;
                        message($"ModbusBtnDown_{msg}");
                    }

                }));
                //开始移动


            }
           );
            _ea.GetEvent<OnCalibViewInited_Even>().Subscribe(() =>
            {
                if (ClassValue.EnableProjCalibratorMode)
                {
                    ProjCalibratorVisibility = Visibility.Visible;
                    message("当前设置为校准架模式");
                    ExposeTimeSet = 1062;
                    TwoPointCalib.Add(new ObservableCollection<double> { 0, 0 });
                    TwoPointCalib.Add(new ObservableCollection<double> { 0, 0 });
                    Task.Run(() =>
                    {
                        if (PPC.ReadCalibConfig(".\\ProjCalibRes\\ProjectorCalibConfig.cfg"))
                        {

                            ProjScales[0] = PPC._config.projectorConfigs[0].projectorPixScaleX;
                            ProjScales[1] = PPC._config.projectorConfigs[1].projectorPixScaleX;
                            ProjOAtCsCalib[0] = new Point2d(PPC._config.projectorConfigs[0].projectorOffsetX, PPC._config.projectorConfigs[0].projectorOffsetY);
                            ProjOAtCsCalib[1] = new Point2d(PPC._config.projectorConfigs[1].projectorOffsetX, PPC._config.projectorConfigs[1].projectorOffsetY);


                            PPC.InitScreens();
                            foreach (var p in PPC.P_List)
                            {
                                p.PropertyChanged += ProjectorOnPropertyChanged;
                            }

                            PPC.InitProjector();
                            ProjectorFlipSet = new ObservableCollection<ObservableCollection<bool>>();
                            foreach (var p in PPC.P_List)
                            {
                                int flip = p.GetProjectorFlip();
                                if (flip == 0)
                                {
                                    ProjectorFlipSet.Add(new ObservableCollection<bool> { true, false, false, false });
                                }
                                else if (flip == 1)
                                {
                                    ProjectorFlipSet.Add(new ObservableCollection<bool> { false, false, true, false });
                                }
                                else if (flip == 2)
                                {
                                    ProjectorFlipSet.Add(new ObservableCollection<bool> { false, true, false, false });
                                }
                                else if (flip == 3)
                                {
                                    ProjectorFlipSet.Add(new ObservableCollection<bool> { false, false, false, true });
                                }

                            }




                        }
                        else
                        {
                            PPC.P_List.Add(new Projector(_ea));
                            PPC.P_List.Add(new Projector(_ea));
                        }

                    });
                    StartModbusTCP();
                    axStsMonitor_timer = new DispatcherTimer();
                    axStsMonitor_timer.Tick += new EventHandler(axStsMonitor_timer_Tick);
                    axStsMonitor_timer.Interval = new TimeSpan(0, 0, 0, 0, 25);//设置时间间隔


                    PImage.Add(SpotImageXY(100, 100, 1, _ppc._config.projectorConfigs[0]));
                    PImage.Add(SpotImageXY(100, 100, 1, _ppc._config.projectorConfigs[1]));
                }
                else
                {
                    ProjCalibratorVisibility = Visibility.Collapsed;
                }
            });


        }

        public ProjCalibratorViewModel()
        {

        }

        private void axStsMonitor_timer_Tick(object sender, EventArgs e)//同步运动试图
        {
            if (!isUVCameraLiveModeEnableed) { return; }

            LivePhoto();

        }
        private BitmapImage SpotImageXY(int x, int y, int radius, ProjectorConfig config)
        {
            if (radius < 1)
            {
                if (x < 0 || x > config.printerPixWidth || y < 0 || y > config.printerPixHeight)
                {
                    return ImageProcessTool.MatToBitmap(new Mat(config.printerPixHeight, config.printerPixWidth, MatType.CV_8UC3));
                }
                Mat sliceProj = new Mat(config.printerPixHeight, config.printerPixWidth, MatType.CV_8UC3, Scalar.Black);
                //sliceProj.Rectangle(new OpenCvSharp.Point(0, 0), new OpenCvSharp.Point(config.printerPixHeight, config.printerPixWidth), Scalar.Black, -1);
                //sliceProj.Set<byte>(x, y, (byte)(255));
                sliceProj.Set<Vec3b>(x, y, new Vec3b(255, 255, 255));
                return ImageProcessTool.MatToBitmap(sliceProj);
            }
            else
            {
                int i0Start = Math.Max(config.printerPixHeight - y - radius, 0);
                int i0Stop = Math.Min(config.printerPixHeight, config.printerPixHeight - y + radius + 1);
                int i1Start = Math.Max(x - radius, 0);
                int i1Stop = Math.Min(config.printerPixWidth, x + radius + 1);
                Mat sliceProj = new Mat(config.printerPixHeight, config.printerPixWidth, MatType.CV_8UC3, Scalar.Black);
                //sliceProj.Rectangle(new OpenCvSharp.Point(0,0),new OpenCvSharp.Point( config.printerPixHeight, config.printerPixWidth ), Scalar.Yellow, -1);
                for (int i = i0Start; i < i0Stop; i++)
                {
                    for (int j = i1Start; j < i1Stop; j++)
                    {
                        //sliceProj.Set<byte>(i, j, (byte)(255));
                        sliceProj.Set<Vec3b>(i, j, new Vec3b(255, 255, 255));
                    }
                }
                //Cv2.ImWrite("D:\\proj.bmp", sliceProj);
                return ImageProcessTool.MatToBitmap(sliceProj);
            }


        }
        private BitmapImage SampleImage( ProjectorConfig config)
        {
           
            Mat sliceProj = new Mat(config.printerPixHeight, config.printerPixWidth, MatType.CV_8UC1, Scalar.Black);
            //sliceProj.Rectangle(new OpenCvSharp.Point(0,0),new OpenCvSharp.Point( config.printerPixHeight, config.printerPixWidth ), Scalar.Yellow, -1);
            Mat sample = Cv2.ImRead(".\\ProjCalibRes\\40pixels_Sample.png");
            foreach (var p in nFocusPoints)
            {
                OpenCvSharp.Rect rect = new OpenCvSharp.Rect((int)p.X - 20, (int)p.Y - 20, 40, 40);
                AddImgPart_OpenCV(ref sliceProj, sample, new g3.Vector2d(-20, -20), new g3.Vector2d(20, 20), new g3.Vector2d((int)p.X, (int)p.Y));
            }
           
            //Cv2.ImWrite("D:\\proj.bmp", sliceProj);
            return ImageProcessTool.MatToBitmap(sliceProj);

        }
        public static void AddImgPart_OpenCV(ref Mat mat, Mat mat_part, Vector2d start, Vector2d end, Vector2d center)
        {
            int startx = Math.Max((int)Math.Floor(start[0] + center[0]) , 0);
            int endx = Math.Min((int)Math.Ceiling(end[0] + center[0]) , 1525);
            int starty = Math.Max((int)Math.Floor(start[1] + center[1]) , 0);
            int endy = Math.Min((int)Math.Ceiling(end[1] + center[1]) , 2712);
            int centerX = (int)Math.Round(center[0], 0);
            int centerY = (int)Math.Round(center[1], 0);
            for (int i = startx; i < endx; i++)
            {
                for (int j = starty; j < endy; j++)
                {
                    byte color = mat_part.At<Vec3b>(j - centerY + 20, i - centerX + 20)[0];
                    mat.At<byte>(j, i) = color;
                }
            }
        }

        private BitmapImage PixelCheckBoard(ProjectorConfig config)
        {
            Mat sliceProj = new Mat(config.printerPixHeight, config.printerPixWidth, MatType.CV_8UC3, Scalar.Black);
            for (int i = 0; i < config.printerPixHeight; i++)
            {
                for (int j = 0; j < config.printerPixWidth; j++)
                {
                    if (i % 2 == j % 2)
                    {
                        sliceProj.Set<Vec3b>(i, j, new Vec3b(255, 255, 255));
                    }
                }
            }
            //Cv2.ImWrite("D:\\proj.bmp", sliceProj);
            return ImageProcessTool.MatToBitmap(sliceProj);
        }




        private Tuple<OpenCvSharp.Point, Mat> PHOTO()
        {
            if (!m_CArtCam.IsInit())
            {
                MessageBox.Show("Select available device");
                return new Tuple<OpenCvSharp.Point,  Mat>(new Point(),new  Mat());
            }
            PointSetVisibility = Visibility.Visible;
            List<Bitmap> bmss = new List<Bitmap>();
            for (int i = 0; i < LiveStackNum; i++)
            {
                // Release device
                m_CArtCam.Close();
                m_CArtCam.SetExposureTime(ExposeTimeSet);
                m_CArtCam.SnapShot(m_pCapture, getSize(), 1);
                m_CArtCam.GetImage(m_pCapture, getSize(), 1);
                // Create bit-map
                var res = CreateBitmapLocal();
                var bitmapX = res.Item1;
                var bitmapByte = res.Item2;
                //获取图像的BitmapData对像 
                BitmapData data = bitmapX.LockBits(new Rectangle(0, 0, bitmapX.Width, bitmapX.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                unsafe
                {
                    m_CArtCam.GetImage(data.Scan0, getSize(), 1);
                }


                bitmapX.UnlockBits(data);

                var intPtr = bitmapX.GetHbitmap();



                //BitmapSource bms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtr, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(intPtr);

                bmss.Add(bitmapX);
            }

            var average = Average(bmss);
            var res3 = MyFindContoursEllipse(average);

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                PointSetX = 0.00465 * (average.Width - res3.Item1.X);
                PointSetY = 0.00465 * (average.Height - res3.Item1.Y);
                Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(res3.Item3);
                UVImage = ToBitmapSource(BitmapToBitmapImage(bitmap), -1, 1);
                UVImageF = BitmapFrame.Create(UVImage);
            }));

            Point resPoint = new Point(0.00465 * (average.Width - res3.Item1.X), 0.00465 * (average.Height - res3.Item1.Y));

            return  new Tuple<OpenCvSharp.Point, Mat>(resPoint, res3.Item3);


        }
        private Mat Average(List<Bitmap> bmss)
        {
            Mat average = new Mat(bmss[0].Height, bmss[0].Width, MatType.CV_8UC3);
            int i = 0;
            foreach (var bm in bmss)
            {
                Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bm);
                //Cv2.Accumulate(mat, average);
                //Cv2.ImWrite($"image{i++}.bmp", mat);
                average += (mat / bmss.Count);
            }


            return average;
        }

        private Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat> MyFindContoursEllipse(Mat srcImage)
        {
            //转化为灰度图
            Mat src_gray = new Mat();
            Cv2.CvtColor(srcImage, src_gray, ColorConversionCodes.RGB2GRAY);
            Mat thImage = new Mat(srcImage.Height, srcImage.Width, new MatType(MatType.CV_8UC3));
            Cv2.Threshold(src_gray, thImage, ThreshSet, 255, ThresholdTypes.Binary);
            //thImage.SaveImage("D:\\test.bmp");
            Mat open = new Mat(); Mat close = new Mat();
            Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(ThreshElementSet, ThreshElementSet));
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
                //Mat dst_Image = Mat.Zeros(src_gray.Size(), srcImage.Type());
                Mat dst_Image = srcImage;
                Random rnd = new Random();
                for (int i = 0; i < contours.Length; i++)
                {
                    Scalar color = new Scalar(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
                    Cv2.DrawContours(dst_Image, contours, i, color, 2, LineTypes.Link8, hierarchly);
                }
                OpenCvSharp.Point center = new OpenCvSharp.Point(rot.Center.X, rot.Center.Y);
                OpenCvSharp.Point vecX = new OpenCvSharp.Point(rot.Size.Width / 2, 0);
                OpenCvSharp.Point vecY = new OpenCvSharp.Point(0, rot.Size.Height / 2);
                var l1p1 = center - vecX;
                var l1p2 = center + vecX;
                var l2p1 = center - vecY;
                var l2p2 = center + vecY;
                //OpenCvSharp.Point size = new OpenCvSharp.Point(rot.Size.Width, rot.Size.Height);
                OpenCvSharp.Size size = new OpenCvSharp.Size((int)rot.Size.Width / 2, (int)rot.Size.Height / 2);
                //Cv2.Ellipse(dst_Image, center, size, rot.Angle, 0, 360, Scalar.AliceBlue);
                Cv2.Line(dst_Image, l1p1.X, l1p1.Y, l1p2.X, l1p2.Y, Scalar.Red);
                Cv2.Line(dst_Image, l2p1.X, l2p1.Y, l2p2.X, l2p2.Y, Scalar.Red);
                return new Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat>(center, size, dst_Image);
            }
            else { return new Tuple<OpenCvSharp.Point, OpenCvSharp.Size, Mat>(new OpenCvSharp.Point(0, 0), new OpenCvSharp.Size(0, 0), srcImage); }
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

        private void Capture()
        {
            //move and photo


            double spotAtProj1X =   ProjXSet * ProjScales[SelectedProjectorIndex];
            double spotAtProj1Y =   ProjYSet * ProjScales[SelectedProjectorIndex];
            double x = ProjOAtCsCalib[SelectedProjectorIndex].X + spotAtProj1X - 6.33 / 2;
            double y = ProjOAtCsCalib[SelectedProjectorIndex].Y + spotAtProj1Y - 4.76 / 2;
            double cx = 65 - y;
            double cy = 350 - x;
            //XPosValueP = 350 - YPosValue;
            //YPosValueP = 65 - XPosValue;
            TBCS[1].Value = (int)Math.Round(cx);
            TBCS[4].Value = (int)Math.Round(cy);

            //设置
            Task t1 = new Task(() =>
            {
                IsUVCameraLiveModeEnableed = false;
                CameraControl("LiveSwitch");
                Thread.Sleep(400);
                _ea.GetEvent<ModbusBtnDown_Even>().Publish("49");

                while (Math.Abs(TBCS[1].Value - XPosValue) > 0.01 || Math.Abs(TBCS[4].Value - YPosValue) > 0.01)
                {
                    Thread.Sleep(20);
                }
                Thread.Sleep(100);
                message("arrive target pos");
                Thread.Sleep(100);
                _ea.GetEvent<ModbusBtnUp_Even>().Publish("49");


                PHOTO();

            });
            t1.Start();
        }

        private void CameraControl(string para)
        {
            if (para == "LiveSwitch")
            {
                if (IsUVCameraLiveModeEnableed)
                {
                    axStsMonitor_timer.Start();
                }
                else
                {
                    axStsMonitor_timer.Stop();
                }
                return;
            }

            if (para == "Photo")
            {
                PHOTO();
            }

        }
        private void LivePhoto()
        {
            if (!m_CArtCam.IsInit())
            {
                MessageBox.Show("Select available device");
                return;
            }
            PointSetVisibility = Visibility.Collapsed;
            if (m_Bitmap != null)
            {
                m_Bitmap.Dispose();
                m_Bitmap = null;
            }

            m_CArtCam.Close();
            int A = getWidth();
            int B = getHeight();
            int expoTime = 0;
            m_CArtCam.SetExposureTime(ExposeTimeSet);
            int colormode = m_CArtCam.GetColorMode();
            expoTime = m_CArtCam.GetExposureTime();
            m_CArtCam.SnapShot(m_pCapture, getSize(), 1);
            m_CArtCam.GetImage(m_pCapture, getSize(), 1);
            CreateBitmap();
            //获取图像的BitmapData对像 
            BitmapData data = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                m_CArtCam.GetImage(data.Scan0, getSize(), 1);
            }
            string path = AppDomain.CurrentDomain.BaseDirectory;
            var intPtr = m_Bitmap.GetHbitmap();
            BitmapSource bms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtr, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            UVImage = ToBitmapSource(bms, -1, 1);
            UVImageF = BitmapFrame.Create(UVImage);
            DeleteObject(intPtr);


            return;

        }
        private int getSize()
        {
            return ((getWidth() * (getColorMode() / 8) + 3) & ~3) * getHeight();
        }

        private int getWidth()
        {
            int[] Size = { 1, 2, 4, 8 };
            return m_CArtCam.Width() / Size[(int)(getSubSample())];
        }

        private int getHeight()
        {
            int[] Size = { 1, 2, 4, 8 };
            return m_CArtCam.Height() / Size[(int)getSubSample()];
        }

        private int getColorMode()
        {
            return ((m_CArtCam.GetColorMode() + 7) & ~7);
        }

        private int getSubSample()
        {
            return ((int)m_CArtCam.GetSubSample() & 0x03);
        }

        private void CreateBitmap()
        {
            // In case bitmap is already created, release.
            if (null != m_Bitmap)
            {
                m_Bitmap.Dispose();
            }

            switch (getColorMode())
            {
                case 8:
                case 16:
                    m_Bitmap = new Bitmap(getWidth(), getHeight(), PixelFormat.Format8bppIndexed);

                    // Pallet modification
                    ColorPalette pal = m_Bitmap.Palette;
                    Color[] cpe = m_Bitmap.Palette.Entries;

                    for (int i = 0; i < 256; i++)
                    {
                        cpe.SetValue(System.Drawing.Color.FromArgb(i, i, i), i);
                        pal.Entries[i] = cpe[i];
                    }
                    m_Bitmap.Palette = pal;
                    break;

                case 24: m_Bitmap = new Bitmap(getWidth(), getHeight(), PixelFormat.Format24bppRgb); break;
                case 32: m_Bitmap = new Bitmap(getWidth(), getHeight(), PixelFormat.Format24bppRgb); break;
                case 48: m_Bitmap = new Bitmap(getWidth(), getHeight(), PixelFormat.Format24bppRgb); break;
                case 64: m_Bitmap = new Bitmap(getWidth(), getHeight(), PixelFormat.Format24bppRgb); break;
            }

            // Arrangement for capture
            m_pCapture = new Byte[getSize()];
        }

        private Tuple<Bitmap, byte[]> CreateBitmapLocal()
        {
            // In case bitmap is already created, release.

            Bitmap res = null;
            byte[] resByte;
            switch (getColorMode())
            {
                case 8:
                case 16:
                    res = new Bitmap(getWidth(), getHeight(), PixelFormat.Format8bppIndexed);

                    // Pallet modification
                    ColorPalette pal = res.Palette;
                    Color[] cpe = res.Palette.Entries;

                    for (int i = 0; i < 256; i++)
                    {
                        cpe.SetValue(System.Drawing.Color.FromArgb(i, i, i), i);
                        pal.Entries[i] = cpe[i];
                    }
                    res.Palette = pal;
                    break;

                case 24: res = new Bitmap(getWidth(), getHeight(), PixelFormat.Format24bppRgb); break;
                case 32: res = new Bitmap(getWidth(), getHeight(), PixelFormat.Format24bppRgb); break;
                case 48: res = new Bitmap(getWidth(), getHeight(), PixelFormat.Format24bppRgb); break;
                case 64: res = new Bitmap(getWidth(), getHeight(), PixelFormat.Format24bppRgb); break;
            }

            // Arrangement for capture
            resByte = new Byte[getSize()];

            return new Tuple<Bitmap, byte[]>(res, resByte);
        }

        /// <summary>
        /// sunkai开发按键对照表 
        /// 1 xJog+ 2 xJog- 3 xHome 4 xAbsolutMove 5 xRelativeMove
        /// 6 yJog+ 7 yJog- 8 yHome 9 yAbsolutMove 10 yRelativeMove
        /// 11 xyMoveStart 12 xyMoveStop 
        /// 13 ErrorReset
        /// </summary>
        /// <param name="str"></param>

        private void StartModbusTCP()
        {
            Readvalue(".\\ProjCalibRes\\servo.xml");
            var test = PLC_Output.F1;
            var thread = new Thread(tcpmodbus1) { IsBackground = true };
            thread.Start();


            OnDllChange((object)0, System.EventArgs.Empty, 0, -1);

        }

        private void Release()
        {
            m_CArtCam.Release();
            if (m_Bitmap != null)
            {
                m_Bitmap.Dispose();
                m_Bitmap = null;
            }

            m_PreviewMode = -1;
            //timer1.Enabled = false;
        }
        private int OnDllChange(object sender, System.EventArgs e, int DllType, int SataType)
        {
            Release();
            m_CArtCam.FreeLibrary();
            int deviceIndex = 0;
            try
            {
                bool res = m_CArtCam.LoadLibrary(".\\ProjCalibRes\\ArtCamSdk_407UV_WOM.dll");
                if (!res)
                {
                    MessageBox.Show("DLL is not found.\nIt may have been relocated after executing.");
                    return -1;
                }

                // Initialize is to be called first
                // By setting Window Handle here, WMLERROR can be obtained
                if (!m_CArtCam.Initialize(hwnd))
                {
                    MessageBox.Show("Failed to initialize SDK");
                    return -1;
                }
                for (int i = 0; i < 8; i++)
                {
                    StringBuilder Temp = new StringBuilder(256);
                    if (0 != m_CArtCam.GetDeviceName(i, Temp, 256))
                    {
                        deviceIndex = i;
                    }
                    else
                    {

                    }
                }
                m_DllType = DllType;
                m_SataType = SataType;
                m_CArtCam.SetDeviceNumber(deviceIndex);
                if (-1 != SataType && ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_SATA == m_CArtCam.GetDllType())
                {
                    m_CArtCam.SetCameraType(SataType);
                    
                }
                
                

                DeviceChange(sender, e, deviceIndex);
                return deviceIndex;
            }
            catch (Exception ex)
            {

            }


            return -1;




            //else
            //{
            //    menuDLL.MenuItems[(int)DllType + 1].Checked = true;
            //}




        }


        private void DeviceChange(object sender, System.EventArgs e, int Number)
        {
            if (m_CArtCam.IsInit())
            {
                m_CArtCam.Close();
            }


            // To confirm whether the device is connected, use "GetDeviceName"
            // It can be found out easily with "GetDeviceName".
            // When area for obtain name is not secured, it results in error. Prepare alignment length of at least 32.
            StringBuilder Temp = new StringBuilder(256);
            if (0 == m_CArtCam.GetDeviceName(Number, Temp, 256))
            {
                m_PreviewMode = -1;
                return;
            }


            // A device will be changed, if a camera is displayed after changing the number of a device now
            // Notes: A device is not changed in this function simple substance
            //   After calling this function, a device is changed by initializing a device
            m_CArtCam.SetDeviceNumber(Number);


            //for (int i = 0; i < 8; i++)
            //{
            //    menuDevice.MenuItems[i].Checked = false;
            //}
            //menuDevice.MenuItems[Number].Checked = true;
        }
        private void SectionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var tb = sender as TBClass;
                message($"{tb.Tittle}参数被修改为{tb.Value}");
                string Str = tb.Tittle;
                if (int.Parse(Str.Substring(2, Str.Length - 2)) < 7)
                { PLC_Output.F[int.Parse(Str.Substring(2, Str.Length - 2)) + 11] = (new sun.Data_Handling()).highTolow_F1((float)tb.Value); }
                else
                { PLC_Output.F[int.Parse(Str.Substring(2, Str.Length - 2)) + 23 - 6] = (new sun.Data_Handling()).highTolow_F1((float)tb.Value); }
            }));


        }

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
                PPC.P_List[projIndex].ProjectorOn();
                return;
            }
            if (cmdStr == "Off")
            {
                PPC.P_List[projIndex].ProjectorOff();
                return;
            }
            if (cmdStr == "LedOn")
            {
                PPC.P_List[projIndex].LedOnOff(true);
                return;
            }
            if (cmdStr == "LedOff")
            {
                PPC.P_List[projIndex].LedOnOff(false);
                return;
            }
            if (cmdStr == "LedGet")
            {
                //获取光源信息，电流
                DAC_ValueSet[projIndex] = PPC.P_List[projIndex].GetLEDDAC();
                LightSensor_ValueSet[projIndex] = PPC.P_List[projIndex].GetLightSensor();
                return;
            }
            if (cmdStr == "LedSet")
            {
                //设置电流
                PPC.P_List[projIndex].SetLEDDAC(DAC_ValueSet[projIndex]);
                LightSensor_ValueSet[projIndex] = PPC.P_List[projIndex].GetLightSensor();
                return;
            }
            if (cmdStr == "Source")
            {
                //设置图像
                PPC.P_List[projIndex].PatternOrSource(int.Parse(cmdPara));
                return;
            }
            if (cmdStr == "Resolution")
            {
                //设置图像源
                PPC.P_List[projIndex].SetProjectorMode(int.Parse(cmdPara));
                return;
            }
            if (cmdStr == "Flip")
            {
                //设置图像翻转
                PPC.P_List[projIndex].ProjectorFlip(int.Parse(cmdPara));
                return;
            }
            if (cmdStr == "InfoGet")
            {
                //设置图像
                List<int> funSpeeds = PPC.P_List[projIndex].GetFanSpeed();
                float dmdTemp = PPC.P_List[projIndex].GetTemperature();
                float ledTemp = PPC.P_List[projIndex].GetLEDTemperature();
                float boardTemp = PPC.P_List[projIndex].GetLedBoardTemperature();
                FunSpeedStrSet[projIndex] = $"fun:{funSpeeds[0]},{funSpeeds[1]},{funSpeeds[2]}";
                TempStrSet[projIndex] = $"temp:{dmdTemp.ToString("0.0")},{ledTemp.ToString("0.0")},{boardTemp.ToString("0.0")}";
                return;
            }

            if (cmdStr == "FMotorHome")
            {
                //前群相机复位
                string status = string.Empty;
                PPC.P_List[projIndex].MtrHome(1, ref status);
                FMotorStatusSet[projIndex] = status;
                return;
            }
            if (cmdStr == "RMotorHome")
            {
                //后群相机复位
                string status = string.Empty;
                PPC.P_List[projIndex].MtrHome(2, ref status);
                RMotorStatusSet[projIndex] = status;
                return;
            }
            int freq1 = 50;
            int freq2 = 100;
            if (cmdStr == "FMotorStep")
            {
                //前群相机步进
                PPC.P_List[projIndex].MotorStep(FMotorDirSet[projIndex], FMotorStepSet[projIndex], 1, freq1);
                return;
            }
            if (cmdStr == "RMotorStep")
            {
                //后群相机复位
                PPC.P_List[projIndex].MotorStep(RMotorDirSet[projIndex], RMotorStepSet[projIndex], 2, freq2);
                return;
            }


            if (cmdStr == "TestImage")
            {

                BitmapImage bm = SpotImageXY(ProjXSet, ProjYSet, ProjRadiusSet, PPC._config.projectorConfigs[SelectedProjectorIndex]);
                BitmapSource bms = _ppc.P_List[SelectedProjectorIndex].TransProjectImage(bm);
                int test = bm.DecodePixelWidth;
                PImage[SelectedProjectorIndex] = ToBitmapSource(bm);
                //SaveBitmapImageIntoFile(bm, "D:\\bm.bmp");
                //SaveImageToFile(bms, "D:\\bms.bmp");
                _ppc.P_List[SelectedProjectorIndex].ProjectImageLED(bms, 70000, 50);

            }
            if (cmdStr == "TestImage2")
            {

                BitmapImage bm = SampleImage( PPC._config.projectorConfigs[SelectedProjectorIndex]);
                BitmapSource bms = _ppc.P_List[SelectedProjectorIndex].TransProjectImage(bm);
                int test = bm.DecodePixelWidth;
                PImage[SelectedProjectorIndex] = ToBitmapSource(bm);
                SaveBitmapImageIntoFile(bm, "D:\\bm.bmp");
                //SaveImageToFile(bms, "D:\\bms.bmp");
                _ppc.P_List[SelectedProjectorIndex].ProjectImageLED(bms, 70000, 50);

                //camera.OpenDevice(0);
                //Thread.Sleep(50);
                //camera.SetExposure(0, 600);

                //Thread.Sleep(50);
                //Mat mat1 = camera.Grab(0);
                //mat1.SaveImage("D:\\grab2.bmp");
                return;
            }
            if (cmdStr == "TestOrientImage")
            {


                string imgPath = $".\\ProjCalibRes\\Screen_Proj_OrientationCheck-{(SelectedProjectorIndex + 1).ToString("00")}.bmp";
                System.IO.FileStream fs = new System.IO.FileStream(imgPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                byte[] buffer = new byte[fs.Length]; fs.Read(buffer, 0, buffer.Length);
                fs.Close(); fs.Dispose();
                System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer);
                BitmapImage bm = new BitmapImage();

                bm.BeginInit();
                bm.StreamSource = ms;
                bm.CacheOption = BitmapCacheOption.OnLoad;
                bm.EndInit();

                ms.Dispose();


                BitmapSource bms = _ppc.P_List[SelectedProjectorIndex].TransProjectImage(bm);
                int test = bm.DecodePixelWidth;
                PImage[SelectedProjectorIndex] = ToBitmapSource(bm);
                //SaveBitmapImageIntoFile(bm, "D:\\bm.bmp");
                //SaveImageToFile(bms, "D:\\bms.bmp");
                _ppc.P_List[SelectedProjectorIndex].ProjectImageLED(bms, 70000, 50);


            }
        }


        private void ProjectorCalib(string msg)
        {
            string[] msgList = msg.Split('-');
            int projIndex = SelectedProjectorIndex;
            string cmdStr = msgList[0];
            string cmdPara = string.Empty;
            if (msgList.Count() > 1)
            {
                cmdPara = msgList[1];
            }

            if (cmdStr == "Proj")
            {
                //投影两点画面
                int intPara = int.Parse(cmdPara);
                BitmapImage bm = SpotImageXY(twoPoints[intPara].X, twoPoints[intPara].Y, 10, _ppc._config.projectorConfigs[projIndex]);
                BitmapSource bms = _ppc.P_List[projIndex].TransProjectImage(bm);
                PImage[projIndex] = ToBitmapSource(bm);
                _ppc.P_List[projIndex].ProjectImageLED(bms, 70000, 50);
                IsUVCameraLiveModeEnableed = true;
                CameraControl("LiveSwitch");

                return;
            }

            if (cmdStr == "Cap")
            {


                Task t1 = new Task(() =>
                {
                    //投影两点画面

                    int intPara = int.Parse(cmdPara);



                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        BitmapImage bm = SpotImageXY(twoPoints[intPara].X, twoPoints[intPara].Y, 3, _ppc._config.projectorConfigs[projIndex]);
                        BitmapSource bms = _ppc.P_List[projIndex].TransProjectImage(bm);
                        int test = bm.DecodePixelWidth;
                        IsUVCameraLiveModeEnableed = false;
                        CameraControl("LiveSwitch");
                        PImage[projIndex] = ToBitmapSource(bm);
                        _ppc.P_List[projIndex].ProjectImageLED(bms, 70000, 50);
                    }));

                    Thread.Sleep(500);
                    PHOTO();
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {

                        TwoPointCalibVisibility = Visibility.Visible;
                        TwoPointCalib[intPara][0] = XPosValueP + PointSetX;
                        TwoPointCalib[intPara][1] = YPosValueP + PointSetY;
                    }));



                });
                t1.Start();
            }

            if (cmdStr == "Caculate")
            {
                //计算scale
                double len = Math.Sqrt(Math.Pow(TwoPointCalib[0][0] - TwoPointCalib[1][0], 2) + Math.Pow(TwoPointCalib[0][1] - TwoPointCalib[1][1], 2));
                double lenPx = Math.Sqrt(Math.Pow(twoPoints[0].X - twoPoints[1].X, 2) + Math.Pow(twoPoints[0].Y - twoPoints[1].Y, 2));
                ProjScales[SelectedProjectorIndex] =   lenPx/ len;
                //用p1 计算坐标系偏差
                ProjOAtCsCalib[SelectedProjectorIndex] = new Point2d(TwoPointCalib[0][0] -   twoPoints[0].X/ ProjScales[SelectedProjectorIndex], TwoPointCalib[0][1] -   twoPoints[0].Y/ ProjScales[SelectedProjectorIndex]);
                TwoPointCalibResultString = $"proj{SelectedProjectorIndex} at CS calib is :{ProjOAtCsCalib[SelectedProjectorIndex].X.ToString("0.00")},{ProjOAtCsCalib[SelectedProjectorIndex].Y.ToString("0.00")}";
            }

            if (cmdStr == "SaveCfg")
            {
                PPC._config.projectorConfigs[0].projectorOffsetX = ProjOAtCsCalib[0].X;
                PPC._config.projectorConfigs[0].projectorOffsetY = ProjOAtCsCalib[0].Y;
                PPC._config.projectorConfigs[1].projectorOffsetX = ProjOAtCsCalib[1].X;
                PPC._config.projectorConfigs[1].projectorOffsetY = ProjOAtCsCalib[1].Y;
                PPC._config.projectorConfigs[0].projectorPixScaleX = ProjScales[0];
                PPC._config.projectorConfigs[0].projectorPixScaleY = ProjScales[0];
                PPC._config.projectorConfigs[1].projectorPixScaleX = ProjScales[1];
                PPC._config.projectorConfigs[1].projectorPixScaleY = ProjScales[1];
                PPC.WriteCalibConfig(".\\ProjCalibRes\\ProjectorCalibConfig.cfg");

                return;
            }
            if (cmdStr == "StartCalib")
            {
                Task t1 = new Task(() =>
                {
                    _ppc.P_List[SelectedProjectorIndex].LedOnOff(true);
                    Thread.Sleep(150);
                    _ppc.P_List[SelectedProjectorIndex].SetLEDDAC(50);
                    Thread.Sleep(150);
                    calibItems = new List<CalibItem>();

                    //投影两点画面
                    int row = 0;
                    int colsCount = 0;
                    for (int i = 114; i < 1428; i += 100)
                    {
                        colsCount = 0;
                        for (int j = 156; j < 2562; j += 160)
                        {
                            colsCount++;
                            CalibItem calibItem = new CalibItem();
                            int j2 = j;
                            if (row % 2 != 0)
                                j2 = 2712 - j;
                            double spotAtProj1X =   i/ ProjScales[SelectedProjectorIndex];
                            double spotAtProj1Y =   j2/ ProjScales[SelectedProjectorIndex];
                            double x = ProjOAtCsCalib[SelectedProjectorIndex].X + spotAtProj1X - 6.33 / 2;
                            double y = ProjOAtCsCalib[SelectedProjectorIndex].Y + spotAtProj1Y - 4.76 / 2;
                            double cx = 65 - y;
                            double cy = 350 - x;
                            //XPosValueP = 350 - YPosValue;
                            //YPosValueP = 65 - XPosValue;
                            
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                BitmapImage bm = SpotImageXY(i, j2, 3, _ppc._config.projectorConfigs[projIndex]);
                                calibItem.calibPix = new Point(i, j2);
                                BitmapSource bms = _ppc.P_List[projIndex].TransProjectImage(bm);
                                PImage[projIndex] = ToBitmapSource(bm);
                                _ppc.P_List[projIndex].ProjectImageLED(bms, 70000, 50);
                                
                                
                            }));
                            int icx = (int)Math.Round(cx);
                            int icy = (int)Math.Round(cy);

                            TBCS[1].Value = icx;
                            TBCS[4].Value = icy;


                            Thread.Sleep(100);
                            _ea.GetEvent<ModbusBtnDown_Even>().Publish("49");
                            Thread.Sleep(200);

                            while (Math.Abs(TBCS[1].Value - XPosValue) > 0.01 || Math.Abs(TBCS[4].Value - YPosValue) > 0.01)
                            {
                                Thread.Sleep(20);
                            }
                            Thread.Sleep(100);
                            message($"arrive target pos{XPosValue},{YPosValue}");
                            Thread.Sleep(300);
                            _ea.GetEvent<ModbusBtnUp_Even>().Publish("49");
                            Thread.Sleep(200);
                            if (!m_CArtCam.IsInit())
                            {
                                MessageBox.Show("Select available device");
                                return ;
                            }
                            if (m_Bitmap != null)
                            {
                                m_Bitmap.Dispose();
                                m_Bitmap = null;
                            }
                            // Release device
                            m_CArtCam.Close();
                            m_CArtCam.SetExposureTime(ExposeTimeSet);
                            m_CArtCam.SnapShot(m_pCapture, getSize(), 1);
                            m_CArtCam.GetImage(m_pCapture, getSize(), 1);
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                
                                // Create bit-map
                                CreateBitmap();
                                BitmapData data = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                                unsafe
                                {
                                    m_CArtCam.GetImage(data.Scan0, getSize(), 1);
                                }
                                m_Bitmap.UnlockBits(data);
                                var intPtr = m_Bitmap.GetHbitmap();
                                BitmapSource bms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtr, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                                Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(m_Bitmap);
                                var res3 = MyFindContoursEllipse(mat);


                                PointSetX = 0.00465 * (mat.Width - res3.Item1.X);
                                PointSetY = 0.00465 * (mat.Height - res3.Item1.Y);
                                Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(res3.Item3);
                                UVImage = ToBitmapSource(BitmapToBitmapImage(bitmap), -1, 1);
                                UVImageF = BitmapFrame.Create(UVImage);
                                DeleteObject(intPtr);
                            }));
                            //获取图像的BitmapData对像 
                            Thread.Sleep(400);



                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                double xp = XPosValueP + PointSetX;
                                double yp = YPosValueP + PointSetY;
                                calibItem.calibCalib = new Point2d(XPosValueP, YPosValueP);
                                calibItem.calibCam = new Point2d(PointSetX, PointSetY);
                                calibItem.calibRes = new Point2d(xp, yp);
                                calibItem.calibImage = m_Bitmap.Clone(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), PixelFormat.Format24bppRgb);
                                message($"cam:{calibItem.calibCam}");
                                message($"calib:{calibItem.calibCalib}");
                            }));
                            calibItems.Add(calibItem);
                            Thread.Sleep(200);

                        }
                        row++;
                    }
                    DateTime dt = DateTime.Now;
                    string dtstr = dt.ToString("yyyy-MM-dd-HH-mm-ss");
                    string calibfoldstr = $".\\ProjCalibRes\\{dtstr}-Proj{SelectedProjectorIndex}-Calib";
                    Directory.CreateDirectory(calibfoldstr);
                    using (FileStream fs = new FileStream($"{calibfoldstr}\\proj{SelectedProjectorIndex}-Res.json", FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(calibItems));

                        }
                    }
                    foreach (var item in calibItems)
                    {

                        item.calibImage.Save($"{calibfoldstr}\\proj{SelectedProjectorIndex}-{item.calibPix.X}-{item.calibPix.Y}.bmp");
                    }
                    //计算外参
                    //List<List<Point3f>> objectPoints =new  List<List<Point3f>>(); // input to CalibrateCamera
                    //List<List<Point2f>> imagePoints = new List<List<Point2f>>();// input to CalibrateCamera
                    ////Mat cameraMatrix = new Mat(); // output from CalibrateCamera
                    ////Mat distCoeffs = new Mat(); // output from CalibrateCamera
                    ////Mat[] rvecs ; // output from CalibrateCamera
                    ////Mat[] tvecs ; // output from CalibrateCamera
                    //double[, ] cameraMatrix = new double[4,4];
                    //double[] distCoeffs = new double[5];
                    //Vec3d[] rvecs = new Vec3d[1];
                    //Vec3d[] tvecs = new Vec3d[1];
                    //CalibrationFlags flags = CalibrationFlags.FixAspectRatio;
                    List<Point2f> ps1 = new List<Point2f>();
                    List<Point2f> ps2 = new List<Point2f>();
                    List<Point2d> ps1d = new List<Point2d>();
                    List<Point2d> ps2d = new List<Point2d>();
                    int indx= 0;
                    foreach (var item in calibItems)
                    {
                        if (indx == 0 || indx == (colsCount - 1) || indx == (calibItems.Count - colsCount) || indx == (calibItems.Count -1))
                        {
                            ps1.Add(new Point2f((float)(item.calibRes.X), (float)(item.calibRes.Y)));
                            ps2.Add(new Point2f((float)(item.calibPix.X), (float)(item.calibPix.Y)));
                        }
                        indx++;
                        ps1d.Add(new Point2d(item.calibRes.X, item.calibRes.Y));
                        ps2d.Add(new Point2d(item.calibPix.X, item.calibPix.Y));
                    }
                    //objectPoints.Add(ps1);
                    //imagePoints.Add(ps2);
                    //var res = myGetAffineTransform(ps2, ps1, calibItems.Count);
                    //var res = myGetPerspectiveTransform(ps2, ps1, calibItems.Count);
                    var res = Cv2.GetPerspectiveTransform(ps2, ps1);
                    var resd = Cv2.FindHomography(ps2d, ps1d);
                    
                    message(res.ToString());
                    using (FileStream fs = new FileStream($"{calibfoldstr}\\proj{SelectedProjectorIndex}-Res.txt", FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine($"Corner:");
                            for (int i = 0; i < ps1.Count; i++)
                            {
                                sw.WriteLine($"{ps2[i].X},{ps2[i].Y}->{ps1[i].X.ToString("0.00")},{ps1[i].Y.ToString("0.00")}");
                            }
                            sw.WriteLine($"PerspectiveTransform:");
                            for (int i = 0; i < res.Rows; i++)
                            {
                                for (int j = 0; j < res.Cols; j++)
                                {
                                    double resxx = res.At<double>(i, j);
                                    sw.Write($"{resxx},");
                                }
                                sw.Write("\r\n");
                            }
                            sw.WriteLine($"FindHomography:");
                            for (int i = 0; i < resd.Rows; i++)
                            {
                                for (int j = 0; j < resd.Cols; j++)
                                {
                                    double resxx = resd.At<double>(i, j);
                                    sw.Write($"{resxx},");
                                }
                                sw.Write("\r\n");
                            }

                        }
                    }
                    _ppc.P_List[SelectedProjectorIndex].LedOnOff(false);
                    Thread.Sleep(150);

                });
                t1.Start();
                return;
            }
            
            if (cmdStr == "SpFocus")
            {
                BitmapImage bm = PixelCheckBoard(PPC._config.projectorConfigs[SelectedProjectorIndex]);
                BitmapSource bms = _ppc.P_List[SelectedProjectorIndex].TransProjectImage(bm);
                int test = bm.DecodePixelWidth;
                PImage[SelectedProjectorIndex] = ToBitmapSource(bm);
                //SaveBitmapImageIntoFile(bm, "D:\\bm.bmp");
                //SaveImageToFile(bms, "D:\\bms.bmp");

                Task t1 = new Task(() =>
                {
                    _ppc.P_List[SelectedProjectorIndex].LedOnOff(true);
                    Thread.Sleep(150);
                    _ppc.P_List[SelectedProjectorIndex].SetLEDDAC(50);
                    Thread.Sleep(150);
                    _ppc.P_List[SelectedProjectorIndex].ProjectImageLED(bms, 70000, 50);
                    Thread.Sleep(2000);
                    int stepValue = 0;
                    int ind = 0;
                    List<FocusItem> res = SpFocus(true);
                    foreach (var item in res)
                    {
                        stepValue += item.rStep;
                        ind += 1;
                    }

                    double stepValuedouble = stepValue / ind;
                    stepValue = (int)Math.Round(stepValuedouble);

                    var maxvalue = res.Max(p => p.score);
                    FocusItem maxFocus = res.FirstOrDefault(p => p.score == maxvalue);
                    message($"Proj{SelectedProjectorIndex} best focus:rStep {maxFocus.rStep},{maxFocus.score.ToString("0")}");
                    DateTime dt = DateTime.Now;
                    string dtstr = dt.ToString("yyyy-MM-dd-HH-mm-ss");
                    string calibfoldstr = $".\\ProjCalibRes\\{dtstr}-Proj{SelectedProjectorIndex}-SinglePointFocus-{XPosValueP.ToString("0.0")}-{YPosValueP.ToString("0.0")}-";
                    Directory.CreateDirectory(calibfoldstr);
                    using (FileStream fs = new FileStream($"{calibfoldstr}\\proj{SelectedProjectorIndex}-SinglePointFocus-{maxFocus.camCenter.X.ToString("0.0")}-{maxFocus.camCenter.Y.ToString("0.0")}-Res.json", FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(res));

                        }
                    }
                    foreach (var item in res)
                    {

                        item.calibImage.Save($"{calibfoldstr}\\proj{SelectedProjectorIndex}-{item.rStep}-{item.score.ToString("0")}.bmp");
                    }
                    _ppc.P_List[SelectedProjectorIndex].LedOnOff(false);
                    Thread.Sleep(150);

                    string status = string.Empty;
                    //后群电机运动到起点
                    PPC.P_List[SelectedProjectorIndex].MtrHome(2, ref status);
                    RMotorStatusSet[SelectedProjectorIndex] = status;
                    message("等待后群电机复位完成");
                    Thread.Sleep(1000);
                    //后群电机运动到起点
                    PPC.P_List[SelectedProjectorIndex].MotorStep(true, stepValue, 2, 100);
                    message("等待后群电机运动到校准位置");
                    Thread.Sleep(4000);

                    //将电机设置到最优位置
                });
                t1.Start();
                //按步骤拍照并保存

                return;
            }
            if (cmdStr == "MpFocus")
            {
                BitmapImage bm = PixelCheckBoard(PPC._config.projectorConfigs[SelectedProjectorIndex]);
                BitmapSource bms = _ppc.P_List[SelectedProjectorIndex].TransProjectImage(bm);
                int test = bm.DecodePixelWidth;
                PImage[SelectedProjectorIndex] = ToBitmapSource(bm);
                //SaveBitmapImageIntoFile(bm, "D:\\bm.bmp");
                //SaveImageToFile(bms, "D:\\bms.bmp");

                Task t1 = new Task(() =>
                {

                    _ppc.P_List[SelectedProjectorIndex].LedOnOff(true);
                    Thread.Sleep(150);
                    _ppc.P_List[SelectedProjectorIndex].SetLEDDAC(50);
                    Thread.Sleep(150);
                    _ppc.P_List[SelectedProjectorIndex].ProjectImageLED(bms, 70000, 50);
                    Thread.Sleep(2000);
                    List<List<FocusItem>> focusItemsList = new List<List<FocusItem>>();
                    int index = 0;
                    foreach (var point in nFocusPoints)
                    {
                        double spotAtProj1X =   point.X/ ProjScales[SelectedProjectorIndex];
                        double spotAtProj1Y =   point.Y/ ProjScales[SelectedProjectorIndex];
                        double x = ProjOAtCsCalib[SelectedProjectorIndex].X + spotAtProj1X - 6.33 / 2;
                        double y = ProjOAtCsCalib[SelectedProjectorIndex].Y + spotAtProj1Y - 4.76 / 2;
                        double cx = 65 - y;
                        double cy = 350 - x;
                        int icx = (int)Math.Round(cx);
                        int icy = (int)Math.Round(cy);

                        TBCS[1].Value = icx;
                        TBCS[4].Value = icy;


                        Thread.Sleep(100);
                        _ea.GetEvent<ModbusBtnDown_Even>().Publish("49");
                        Thread.Sleep(300);
                        while (Math.Abs(TBCS[1].Value - XPosValue) > 0.01 || Math.Abs(TBCS[4].Value - YPosValue) > 0.01)
                        {
                            Thread.Sleep(20);
                        }
                        Thread.Sleep(100);
                        message($"arrive target pos{XPosValue},{YPosValue}");
                        Thread.Sleep(300);
                        _ea.GetEvent<ModbusBtnUp_Even>().Publish("49");
                        Thread.Sleep(200);
                        bool resetFM = false;
                        if (index++ == 0)
                        {
                            resetFM = true;
                        }
                        List<FocusItem> res = SpFocus(resetFM);
                        var maxvalue = res.Max(p => p.score);
                        FocusItem maxFocus = res.FirstOrDefault(p => p.score == maxvalue);
                        message($"Proj{SelectedProjectorIndex} best focus:rStep {maxFocus.rStep},{maxFocus.score.ToString("0")}");
                        focusItemsList.Add(res);
                    }

                    DateTime dt = DateTime.Now;
                    string dtstr = dt.ToString("yyyy-MM-dd-HH-mm-ss");
                    string calibfoldstr = $".\\ProjCalibRes\\{dtstr}-Proj{SelectedProjectorIndex}-9-PointFocus-Calib";
                    Directory.CreateDirectory(calibfoldstr);
                    using (FileStream fs = new FileStream($"{calibfoldstr}\\proj{SelectedProjectorIndex}-9-PointFocus-Res.json", FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(focusItemsList));

                        }
                    }
                    int stepValue = 0;
                    int ind = 0;
                    using (FileStream fs = new FileStream($"{calibfoldstr}\\proj{SelectedProjectorIndex}-9-PointFocus-Res.txt", FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {

                            foreach (var li in focusItemsList)
                            {
                                var maxvalue = li.Max(p => p.score);
                                FocusItem maxFocus = li.FirstOrDefault(p => p.score == maxvalue);
                                sw.WriteLine($"{++ind}:rStep {maxFocus.rStep},{maxFocus.score.ToString("0")}");
                                stepValue += maxFocus.rStep;
                            }

                        }
                    }
                    double stepValuedouble = stepValue / index;
                    stepValue = (int)Math.Round(stepValuedouble);
                    foreach (var li in focusItemsList)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            GenerateFocusReport($"{calibfoldstr}\\proj{SelectedProjectorIndex}-pos{li[0].camCenter.X.ToString("0")}-{li[0].camCenter.Y.ToString("0")}-step-score-plot.png", li);
                        }));
                        foreach (var item in li)
                        {

                            item.calibImage.Save($"{calibfoldstr}\\proj{SelectedProjectorIndex}-pos{item.camCenter.X.ToString("0")}-{item.camCenter.Y.ToString("0")}-step{item.rStep}-{item.score.ToString("0")}.bmp");
                        }
                    }

                    _ppc.P_List[SelectedProjectorIndex].LedOnOff(false);
                    Thread.Sleep(150);
                    string status = string.Empty;
                    //后群电机运动到起点
                    PPC.P_List[SelectedProjectorIndex].MtrHome(2, ref status);
                    RMotorStatusSet[SelectedProjectorIndex] = status;
                    message("等待后群电机复位完成");
                    Thread.Sleep(1000);
                    //后群电机运动到起点
                    PPC.P_List[SelectedProjectorIndex].MotorStep(true, stepValue, 2, 100);
                    message("等待后群电机运动到校准位置");
                    Thread.Sleep(4000);

                    //将电机设置到最优位置

                });
                t1.Start();
                //按步骤拍照并保存

                return;
            }
        }

        //        // 仿射变换模型，线性回归，正规⽅程求解参数矩阵的⽅法
        //        cv::Mat myGetAffineTransform(const cv::Point2f src[], const cv::Point2f dst[], int m)
        //{
        //    cv::Mat_<float> X = cv::Mat(m, 3, CV_32FC1, cv::Scalar(0));
        //        cv::Mat_<float> Y = cv::Mat(m, 2, CV_32FC1, cv::Scalar(0));
        //for (int i = 0; i<m; i++)
        //    {
        //float x0 = src[i].x, x1 = src[i].y;
        //        float y0 = dst[i].x, y1 = dst[i].y;
        //        X(i, 0) = x0;
        //        X(i, 1) = x1;
        //        X(i, 2) = 1;
        //        Y(i, 0) = y0;
        //        Y(i, 1) = y1;
        //    }
        //    cv::Mat_<float> F = (X.t() * X).inv() * (X.t() * Y);
        //// cout << F << endl;
        //return F.t();
        //}
        MathNet.Numerics.LinearAlgebra.Matrix<double> myGetAffineTransform(List<Point2f> src, List<Point2f> dst, int m)
        {
            
            DenseMatrix X = new DenseMatrix(m, 3);
            DenseMatrix Y = new DenseMatrix(m, 2);
            for (int i = 0; i < m; i++)
            {
                X[i, 0] = src[i].X;
                X[i, 1] = src[i].Y;
                X[i, 2] = 1;
                Y[i, 0] = dst[i].X;
                Y[i, 1] = dst[i].Y;
            }

            MathNet.Numerics.LinearAlgebra.Matrix<double> F = (X.Transpose() * X).Inverse() * (X.Transpose() * Y);
            Console.WriteLine(F);
            return F.Transpose();
        }


        /// <summary>
        /// 对焦报告内容：
        /// 1：9点对焦最佳step - 输出 txt 
        /// 2：每个点对焦详情，评价分数曲线，最佳周围5张照片
        /// 
        /// </summary>
        private void GenerateFocusReport(string path, List<FocusItem> focusItems)
        {
            int max = (int)focusItems.Max(p => p.score);
            int min = (int)focusItems.Min(p => p.score);
            var model = new PlotModel() { };

            // 添加图例说明
            model.Legends.Add(new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0,
                LegendTextColor = OxyColors.LightGray
            });

            // 定义第一个Y轴y1，
            var ay1 = new LinearAxis()
            {
                Key = "Brenner",
                Position = AxisPosition.Left,
                Minimum = min -10000,
                MajorStep = 1,
                Title = "Step",
                Maximum = max + 10000,
                LabelFormatter = v => $"{v:0}"
            };
            var ax = new LinearAxis()
            {
                Minimum = FocusStart - 1,
                Maximum = FocusEnd + 1,
                Position = AxisPosition.Bottom,
                IsZoomEnabled = true
            };

            // 定义折线图序列，指定数据轴为Y1轴
            LineSeries scoreSeries = new LineSeries();
            scoreSeries.Title = "Brenner";
            scoreSeries.YAxisKey = "Brenner";
            // 点击时弹出的label内容
            scoreSeries.TrackerFormatString = "{0}\r\brenner：{4:0}";
            // 设置颜色阈值范围
            //levelSeries.LimitHi = 0.15;
            //levelSeries.LimitLo = -0.15;
            // 设置数据绑定源和字段
            scoreSeries.ItemsSource = focusItems;
            scoreSeries.DataFieldX = "rStep";
            scoreSeries.DataFieldY = "score";
            scoreSeries.Color = OxyColors.YellowGreen;
            model.Series.Add(scoreSeries);
            
            model.Axes.Add(ay1);
            model.Axes.Add(ax);
            // 设置图形边框
            model.PlotAreaBorderThickness = new OxyThickness(1, 0, 1, 1);
            var pngExporter = new PngExporter { Width = 1440, Height = 800, };

            pngExporter.ExportToFile(model, path);
        }
        ///<summary>
        ///校准结果报告内容
        ///输出txt即可。
        ///</summary>
        private List<FocusItem> SpFocus(bool resetFM)
        {
            List<FocusItem> focusItems =  new List<FocusItem>();
            int freq1 = 50;
            int freq2 = 100;
            //前群电机复位
            string status = string.Empty;
            if (resetFM)
            {
                PPC.P_List[SelectedProjectorIndex].MtrHome(1, ref status);
                FMotorStatusSet[SelectedProjectorIndex] = status;
                message("等待前群电机复位完成");
                Thread.Sleep(1000);
            }
           
            //后群电机运动到起点
            PPC.P_List[SelectedProjectorIndex].MtrHome(2,ref status);
            RMotorStatusSet[SelectedProjectorIndex] = status;
            message("等待后群电机复位完成");
            Thread.Sleep(2000);
            //前群电机运动到780
            if (resetFM)
            {
                PPC.P_List[SelectedProjectorIndex].MotorStep(true, FMotorStep, 1, freq1);
                message("等待前群电机运动到位");
                Thread.Sleep(20000);
            }
           
            //后群电机运动到起点
            PPC.P_List[SelectedProjectorIndex].MotorStep(true, FocusStart, 2, freq2);
            message("等待后群电机运动到位");
            Thread.Sleep(6000);
            for (int i = FocusStart; i < FocusEnd + 1; i++)
            {
                
                
                //后群电机运动步距
                PPC.P_List[SelectedProjectorIndex].MotorStep(true, 1, 2, freq2);
                Thread.Sleep(1500);
                if (!m_CArtCam.IsInit())
                {
                    MessageBox.Show("Select available device");
                    return new List<FocusItem>();
                }
                if (m_Bitmap != null)
                {
                    m_Bitmap.Dispose();
                    m_Bitmap = null;
                }
                // Release device
                m_CArtCam.Close();
                m_CArtCam.SetExposureTime(ExposeTimeSet);
                m_CArtCam.SnapShot(m_pCapture, getSize(), 1);
                m_CArtCam.GetImage(m_pCapture, getSize(), 1);
                // Create bit-map
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    CreateBitmap();
                    BitmapData data = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    unsafe
                    {
                        m_CArtCam.GetImage(data.Scan0, getSize(), 1);
                    }
                    m_Bitmap.UnlockBits(data);
                    var intPtr = m_Bitmap.GetHbitmap();
                    BitmapSource bms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtr, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    UVImage = ToBitmapSource(bms, -1, 1);
                    UVImageF = BitmapFrame.Create(UVImage);
                    DeleteObject(intPtr);
                }));
                //获取图像的BitmapData对像 
                
                while (m_Bitmap == null)
                { Thread.Sleep(20); }
                Thread.Sleep(200);
                //bitmapX.Save($".\\focus-sp-{i}.bmp");
                FocusItem focusItem = new FocusItem();
                focusItem.fStep = fMotorStep;
                focusItem.rStep = i;
                focusItem.calibImage = m_Bitmap.Clone(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), PixelFormat.Format24bppRgb);
                focusItem.score = Brenner(focusItem.calibImage);
                focusItem.camCenter = new Point2d(ProjOAtCsCalib[selectedProjectorIndex].X + XPosValueP + 0.00465 * 1360 / 2, ProjOAtCsCalib[selectedProjectorIndex].Y + YPosValueP + 0.00465 * 1024 / 2);
                focusItems.Add(focusItem);
                
                
                message($"Focus {i}, {focusItem.score.ToString("0")}");
                
                //App.Current.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //}));
                
                Thread.Sleep(100);
                }
            
            //message($"best Focus is {FocusStart + ind }, {max.ToString("0")}");
            return focusItems;
        }

        private double Brenner(Bitmap bitmap)
        {
            //Brenner 梯度函数
            //:param img:narray 二维灰度图像
            //:return: float 图像约清晰越大
            //获取图像的BitmapData对像 
            Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
            double  res = 0;
            for (int i = 0; i < mat.Cols - 2 ; i++)
            {
                for (int j = 0; j < mat.Rows -2 ; j++)
                {
                    int color1 = (int)mat.At<Vec3b>(j, i )[0];
                    int color2 = (int)mat.At<Vec3b>(j, i +2)[0];
                    int color3 = (int)mat.At<Vec3b>(j +2, i)[0];
                    res += Math.Pow(color1 - color2, 2);
                    res += Math.Pow(color1 - color3, 2);
                }
            }
            return res;
        }

 


        /// <summary>
        /// 把内存里的BitmapImage数据保存到硬盘中
        /// </summary>
        /// <param name="bitmapImage">BitmapImage数据</param>
        /// <param name="filePath">输出的文件路径</param>
        public static void SaveBitmapImageIntoFile(BitmapImage bitmapImage, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        /// <summary>
        /// 保存图片到文件
        /// </summary>
        /// <param name="image">图片数据</param>
        /// <param name="filePath">保存路径</param>
        private void SaveImageToFile(BitmapSource image, string filePath)
        {
            BitmapEncoder encoder = GetBitmapEncoder(filePath);
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        /// <summary>
        /// 根据文件扩展名获取图片编码器
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>图片编码器</returns>
        private BitmapEncoder GetBitmapEncoder(string filePath)
        {
            var extName = Path.GetExtension(filePath).ToLower();
            if (extName.Equals(".png"))
            {
                return new PngBitmapEncoder();
            }
            else
            {
                return new JpegBitmapEncoder();
            }
        }
        public TransformedBitmap ToBitmapSource(BitmapImage bitmapSource)
        {
            var tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = bitmapSource;
            var transform = new ScaleTransform(1, 1, 0, 0); ;
            tb.Transform = transform;
            tb.EndInit();
            return tb;
        }
        public TransformedBitmap ToBitmapSource(BitmapSource bitmapSource ,int x, int y)
        {
            var tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = bitmapSource;
            var transform = new ScaleTransform(x, y, 0, 0); ;
            tb.Transform = transform;
            tb.EndInit();
            return tb;
        }
        public void Readvalue(string Name)
        {
            try
            {

                string logPath1 = Environment.CurrentDirectory;
                DirectoryInfo folder = new DirectoryInfo(logPath1);
                if (folder.GetFiles(Name).Count() > 0)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(Name);
                    XmlNode xn = doc.SelectSingleNode("values");
                    XmlNodeList xnl = xn.ChildNodes;
                    List<string> intvalue = new List<string>();
                    List<string> intkey = new List<string>();
                    TBCS = new ObservableCollection<TBClass>();
                    for (int i = 0; i < xnl.Count; i++)
                    {
                        TBCS.Add(new TBClass());
                    }

                    for (int i = 0; i < xnl.Count; i++)
                    {
                        XmlElement xe = (XmlElement)xnl[i];
                        int ind = int.Parse(xe.Name.Substring(2)) - 1;
                        //TBS[ind] = int.Parse(xe.GetAttribute("intvalue"));
                        TBCS[ind] = new TBClass()
                        {

                            Tittle = $"TB{ind + 1 }",
                            Value = int.Parse(xe.GetAttribute("intvalue"))
                        };
                        TBCS[ind].PropertyChanged += SectionOnPropertyChanged;
                    }
                    foreach (var tb in TBCS)
                    {
                        string Str = tb.Tittle;
                        if (int.Parse(Str.Substring(2, Str.Length - 2)) < 7)
                        { PLC_Output.F[int.Parse(Str.Substring(2, Str.Length - 2)) + 11] = (new sun.Data_Handling()).highTolow_F1((float)tb.Value); }
                        else
                        { PLC_Output.F[int.Parse(Str.Substring(2, Str.Length - 2)) + 23 - 6] = (new sun.Data_Handling()).highTolow_F1((float)tb.Value); }

                    }

                    //foreach (Control Item in this.Controls)
                    //{

                    //    if (Item is Sunny.UI.UITextBox && Item.Name.Length < 5)
                    //    {
                    //        var con = Item as Sunny.UI.UITextBox;
                    //        con.DoubleValue = double.Parse(intvalue[a]);
                    //        a = a + 1;
                    //    }
                    //}
                }

            }
            catch (Exception e)
            {
                message(e.ToString());
            }
        }

        public void Savevalue()
        {
            string Name = ".\\ProjCalibRes\\servo.xml";
            XmlDocument doc = new XmlDocument();
            //2、创建第一行描述信息
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement values = doc.CreateElement("values");
            doc.AppendChild(values);
            int index = 1;
            //foreach (var intvalue in TBS)
            //{
            //    XmlElement value = doc.CreateElement($"TB{index++}");
            //    values.AppendChild(value);
            //    value.SetAttribute("intvalue", intvalue.ToString());
            //}
            foreach (var tbc in TBCS)
            {
                XmlElement value = doc.CreateElement($"TB{index++}");
                values.AppendChild(value);
                value.SetAttribute("intvalue", tbc.Value.ToString());
            }
            doc.Save(Name);
        }
        private void message(string msg)
        {
            _ea.GetEvent<MessageEvent>().Publish(msg);
        }

        private void tcpmodbus1()
        {
            List<string> listIp = getIP();
            message($"Connect ModbusTCP[{listIp[0]}@{listIp[1]}");
            while (!IsClosed)
            {
                tcpmodbus(listIp);
            }
        }//与PLC modbus通讯
        private void tcpmodbus(List<string> listIp)
        {
            Socket modbusSocket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Modbus_TCP.modbusSocket = modbusSocket1;

            try
            {
                //int count = 0;
                Isconnectd = Modbus_TCP.Mod_Tcp_connect(listIp[0], int.Parse(listIp[1]));
                if (Isconnectd)
                {
                    message("PLC-ModbusTcp已连接");
                }
                else
                {
                    return;
                }
                while (!IsClosed)
                {

                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        form_change();
                    }));



                    //message(DateTime.Now.ToString()+"："+DateTime.Now.Millisecond.ToString());
                    string str = DateTime.Now.Millisecond.ToString();
                    if (Modbus_TCP.modbusSocket.Poll(1000, SelectMode.SelectRead) && (Modbus_TCP.modbusSocket.Available <= 0))
                    {

                        Isconnectd = false;
                        Modbus_TCP.modbusSocket.Close();
                        message("PLC-ModbusTcp连接断开");
                        return;
                    }
                    Boolean Is_C = false;
                    byte[] In_plc_1 = new byte[250];
                    byte[] In_plc_2 = new byte[250];
                    Is_C = Modbus_TCP.Mod_Tcp_func3(0x05, 0xDC, out In_plc_1);//
                    if (!Is_C)
                    {
                        Isconnectd = false;
                        Modbus_TCP.modbusSocket.Close();
                        message("PLC-ModbusTcp连接断开");
                        return;
                    }

                    Is_C = Modbus_TCP.Mod_Tcp_func3(0x06, 0x59, out In_plc_2);//
                    if (!Is_C)
                    {
                        Isconnectd = false;
                        Modbus_TCP.modbusSocket.Close();
                        message("PLC-ModbusTcp连接断开");
                        return;
                    }

                    Modbus_TCP.In_PLC_Rcv(In_plc_1, In_plc_2);

                    byte[] Out_plc_1 = new byte[246];
                    byte[] Out_plc_2 = new byte[246];
                    byte[] Out_plc_3 = new byte[246];
                    Modbus_TCP.Out_PLC_Send(out Out_plc_1, out Out_plc_2);
                    Modbus_TCP.Out_PLC_Send_1(out Out_plc_3);
                    Is_C = Modbus_TCP.Mod_Tcp_func16(0X03, 0XE8, Out_plc_1);//
                    if (!Is_C)
                    {
                        Isconnectd = false;
                        Modbus_TCP.modbusSocket.Close();
                        message("PLC-ModbusTcp连接断开");

                        return;
                    }

                    Is_C = Modbus_TCP.Mod_Tcp_func16(0x04, 0X63, Out_plc_2);//
                    if (!Is_C)
                    {
                        Isconnectd = false;
                        Modbus_TCP.modbusSocket.Close();
                        message("PLC-ModbusTcp连接断开");
                        return;
                    }
                    Is_C = Modbus_TCP.Mod_Tcp_func16(0x04, 0XE2, Out_plc_3);//
                    if (!Is_C)
                    {
                        Isconnectd = false;
                        Modbus_TCP.modbusSocket.Close();
                        message("PLC-ModbusTcp连接断开");
                        return;
                    }

                    Thread.Sleep(20);
                    string str1 = DateTime.Now.Millisecond.ToString();
                    //message((int.Parse(str1)-int.Parse(str)).ToString());
                }
            }
            catch (SocketException ex)
            {
                Isconnectd = false;
                message(ex.Message);
                Modbus_TCP.modbusSocket.Close();
                message("PLC-ModbusTcp连接断开");
                Thread.Sleep(1000);
            }
        }

        private void form_change()
        {
            
            XPosValue = (new sun.Data_Handling()).highTolow_F1(PLC_Input.F[12]);
            YPosValue = (new sun.Data_Handling()).highTolow_F1(PLC_Input.F[13]);
            XPosValueP = 350 - YPosValue;
            YPosValueP = 65 - XPosValue;
            UVCameraMargin = new Thickness(220 + XPosValueP , 0, 0, 100 + YPosValueP );
            if (xPosValueP > 216)
            {
                UVCameraMargin2 = new Thickness(130 + XPosValueP, 0, 0, 130 + YPosValueP);
            }
            else
            {
                UVCameraMargin2 = new Thickness(250 + XPosValueP, 0, 0, 130 + YPosValueP);
            }
            
            if (PLC_Input.b[288] == 1)
            {

                XStatusColor = new SolidColorBrush(Colors.Green);
            }
            else
            {
                XStatusColor = new SolidColorBrush(Colors.Gray);
            }

            if (PLC_Input.b[289] == 1)
            {
                YStatusColor = new SolidColorBrush(Colors.Green);
            }
            else
            {
                YStatusColor = new SolidColorBrush(Colors.Gray);
            }
            if (PLC_Input.b[290] == 1)
            {

                XErrorStatusColor = new SolidColorBrush(Colors.Red);
            }
            else
            {
                XErrorStatusColor = new SolidColorBrush(Colors.Green);
            }

            if (PLC_Input.b[291] == 1)
            {
                YErrorStatusColor = new SolidColorBrush(Colors.Red);
            }
            else
            {
                YErrorStatusColor = new SolidColorBrush(Colors.Green);
            }

            //try
            //{
            //    App.Current.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        message("form_change");
            //        xPosValue = (new sun.Data_Handling()).highTolow_F1(PLC_Input.F[12]).ToString();
            //        yPosValue = (new sun.Data_Handling()).highTolow_F1(PLC_Input.F[13]).ToString();
            //        if (PLC_Input.b[288] == 1)
            //        {

            //            xStatusColor = new SolidColorBrush(Colors.Green);
            //        }
            //        else
            //        {
            //            xStatusColor = new SolidColorBrush(Colors.Red);
            //        }

            //        if (PLC_Input.b[289] == 1)
            //        {
            //            yStatusColor = new SolidColorBrush(Colors.Green);
            //        }
            //        else
            //        {
            //            yStatusColor = new SolidColorBrush(Colors.Red);
            //        }

            //        //if (PLC_Input.b[290] == 1)
            //        //{
            //        //    uiLight3.State = Sunny.UI.UILightState.Off;
            //        //}
            //        //else
            //        //{
            //        //    uiLight3.State = Sunny.UI.UILightState.On;
            //        //}

            //        //if (PLC_Input.b[291] == 1)
            //        //{
            //        //    uiLight4.State = Sunny.UI.UILightState.Off;
            //        //}
            //        //else
            //        //{
            //        //    uiLight4.State = Sunny.UI.UILightState.On;
            //        //}


            //        //if (PLC_Input.b[292] == 0)
            //        //{
            //        //    uiLight5.State = Sunny.UI.UILightState.Off;
            //        //    //Strrr = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString()
            //        //    //+ DateTime.Now.Minute.ToString();

            //        //    Strrr = DateTime.Now.ToString("yyyyMMddHHmm");

            //        //}
            //        //else
            //        //{
            //        //    uiLight5.State = Sunny.UI.UILightState.On;

            //        //}

            //        //if (PLC_Input.b[293] == 1)
            //        //{
            //        //    if (!P)
            //        //    {
            //        //        PHOTO();
            //        //        P = true;
            //        //    }
            //        //}

            //        //if (PLC_Input.b[293] == 0)
            //        //{
            //        //    P = false;
            //        //}
            //    }));
            //}

            //catch
            //{
            //}

        }
        private List<string> getIP()
        {
            string logPath1 = Environment.CurrentDirectory;
            string logPath = logPath1 + @".\ProjCalibRes\ip.txt";
            DirectoryInfo folder = new DirectoryInfo(logPath1);
            if (folder.GetFiles(@".\ProjCalibRes\ip.txt").Count() == 0)
            {
                FileStream fs1 = new FileStream(logPath, FileMode.Create);
                using (StreamWriter sw = new StreamWriter(fs1))
                {
                    sw.Write("192.168.1.88" + "\r\n" + "502");
                }

                fs1.Close();
            }
            List<string> list_ipe = new List<string>();
            list_ipe = TxtControl.ReadLineTxt(@".\ProjCalibRes\ip.txt");
            return list_ipe;
        }

        private void Close()
        {
            _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
        }

        [DllImport("gdi32.dll",SetLastError =true)]
        internal static extern bool DeleteObject(IntPtr ptr);
    }
    

}