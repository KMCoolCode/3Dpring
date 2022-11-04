using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Controls;
using Prism.Events;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using static AngelDLP.Views.PrintScene3D;
using AngelDLP.PMC;
using AngelDLP.PPC;
using System.Threading.Tasks;
using System.Diagnostics;
using AngleDLP.Models;
using static AngleDLP.Models.LogHelper;
using static AngelDLP.ViewModels.ProjCalibratorViewModel;
using static AngelDLP.ViewModels.PrintControlViewModel;
using AngleDLP;
using AngleDLP.PPC;
using DataService.Models.Dto.Pcloud;
using System.Collections.Generic;
using System.Threading;
using static AngelDLP.PrintTask;
using static AngelDLP.ViewModels.UserViewModel;

namespace AngelDLP.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        IEventAggregator _ea;
        private PrintTask printtask;
        public PrintTask _printtask
        {
            get { return printtask; }
            set { SetProperty(ref printtask, value); }

        }
        PartModelFactory _partmodels;
        
        DispatcherTimer pStsMonitor_timer;

        DispatcherTimer pCloudMonitor_timer;

        PrinterMotionControl _pmc { get; set; }

        private PrintTaskHolder _printTaskHolder;
        public PrintTaskHolder PrintTaskHolderx
        {
            get { return _printTaskHolder; }
            set { SetProperty(ref _printTaskHolder, value); }

        }

        private PCloud pCloud = new PCloud();

        private PrinterProjectionControl _ppc;
        public PrinterProjectionControl PPC
        {
            get { return _ppc; }
            set { SetProperty(ref _ppc, value); }

        }
        #region ViewModels
        private ProgressViewModel progressControl = new ProgressViewModel();
        public ProgressViewModel ProgressControl
        {
            get { return progressControl; }
            set { SetProperty(ref progressControl, value); }

        }
        private ModelsTipViewModel modelsTipControl = new ModelsTipViewModel();
        public ModelsTipViewModel ModelsTipControl
        {
            get { return modelsTipControl; }
            set { SetProperty(ref modelsTipControl, value); }

        }
        private PrintProgressViewModel printProgressControl = new PrintProgressViewModel();
        public PrintProgressViewModel PrintProgressControl
        {
            get { return printProgressControl; }
            set { SetProperty(ref printProgressControl, value); }

        }
        private PrinterStatusTipViewModel printerStatusTipControl = new PrinterStatusTipViewModel();
        public PrinterStatusTipViewModel PrinterStatusTipControl
        {
            get { return printerStatusTipControl; }
            set { SetProperty(ref printerStatusTipControl, value); }

        }
        private PrintControlViewModel printControl_ViewModel;
        public PrintControlViewModel PrintControl_ViewModel
        {
            get { return printControl_ViewModel; }
            set { SetProperty(ref printControl_ViewModel, value); }

        }
        private PrintTaskSetupViewModel printTaskSetup_ViewModel;
        public PrintTaskSetupViewModel PrintTaskSetup_ViewModel
        {
            get { return printTaskSetup_ViewModel; }
            set { SetProperty(ref printTaskSetup_ViewModel, value); }

        }
        private PrintScene3DViewModel printScene3D_ViewModel;
        public PrintScene3DViewModel PrintScene3D_ViewModel
        {
            get { return printScene3D_ViewModel; }
            set { SetProperty(ref printScene3D_ViewModel, value); }

        }
        //private HistoryViewModel history_ViewModel;
        //public HistoryViewModel History_ViewModel
        //{
        //    get { return history_ViewModel; }
        //    set { SetProperty(ref history_ViewModel, value); }

        //}
        private PrintTaskMonitorViewModel printTaskMonitor_ViewModel;
        public PrintTaskMonitorViewModel PrintTaskMonitor_ViewModel
        {
            get { return printTaskMonitor_ViewModel; }
            set { SetProperty(ref printTaskMonitor_ViewModel, value); }

        }

        private SettingViewModel setting_ViewModel;
        public SettingViewModel Setting_ViewModel
        {
            get { return setting_ViewModel; }
            set { SetProperty(ref setting_ViewModel, value); }

        } 
        private UserViewModel user_ViewModel;
        public UserViewModel User_ViewModel
        {
            get { return user_ViewModel; }
            set { SetProperty(ref user_ViewModel, value); }

        }
        //private ProjCalibratorViewModel projCalibrator_ViewModel;
        //public ProjCalibratorViewModel ProjCalibrator_ViewModel
        //{
        //    get { return projCalibrator_ViewModel; }
        //    set { SetProperty(ref projCalibrator_ViewModel, value); }

        //}
        #endregion

        #region UIBindings
        private double sliceAllIndex = 0;
        public double SliceAllIndex
        {
            get { return sliceAllIndex; }
            set { SetProperty(ref sliceAllIndex, value); }

        }
        private double sliceAllMaxHeight = 0;
        public double SliceAllMaxHeight
        {
            get { return sliceAllMaxHeight; }
            set { SetProperty(ref sliceAllMaxHeight, value); }

        }

        private ObservableCollection<MessageBoxItem> messageBoxItems = new ObservableCollection<MessageBoxItem>();
        public ObservableCollection<MessageBoxItem> MessageBoxItems
        {
            get { return messageBoxItems; }
            set { SetProperty(ref messageBoxItems, value); }
        }
        #endregion

        public Geometry fullscreenIcon { get; set; } = (Geometry)Application.Current.Resources["i_Maxmize"];
        //public ObservableCollection<PartModel> Items { set; get; }
        public static string[] subviews = new string[] { "PrintTaskSetup", "PrintTaskMonitor", "PrintControlView", "History", "Setting", "Info", "User" };

        public DelegateCommand<string> subViewSwitchCommand { get; private set; }
        //public DelegateCommand<SelectionChangedEventArgs> SwitchItemCommand { get; private set; }
        public DelegateCommand SliceAllLayerCommand { get; private set; }
        public DelegateCommand<string> PrinterControlCommand { get; private set; }

        #region PubSubEvents
        public class ViewSwitchEvent : PubSubEvent<string>
        {

        }
        public class ChangeFullscreanIconEvent : PubSubEvent<bool>
        {

        }
        public class UploadProgressEvent : PubSubEvent<Tuple<int, int, double>>
        {

        }

        public class ProgressDialogEvent : PubSubEvent<Tuple<int, string, string>>
        {

        }
        public class UpdateTransformUIEvent : PubSubEvent<UI2PartModel>
        {

        }
        public class UICloseEvent : PubSubEvent
        {

        }
        public class PartListViewSwitchEvent : PubSubEvent
        {

        }

        public class MessageEvent : PubSubEvent<string>
        {

        }
        public class AppLogEvent : PubSubEvent<string>
        {

        }
        public class ProgressViewEvent : PubSubEvent<ProgressViewModel>
        {

        }
        public class ModelsProgressViewEvent : PubSubEvent<ModelsTipViewModel>
        {

        }
        public class PrintProgressViewEvent : PubSubEvent<PrintProgressViewModel>
        {

        }
        public class StartAPPViewEvent : PubSubEvent
        {

        }
        public class UpDatePrintTaskStatusEvent : PubSubEvent<PrinterProduceMode>
        {

        }
        public class UpDatePrinterAlertEvent : PubSubEvent<Tuple<PrinterAlerts, bool>>
        {

        }
        public class PushMessageEvent : PubSubEvent<Tuple<string, PrinterEventLevel>>
        {

        }
        public class GetSublotEvent : PubSubEvent<string>
        {

        }

        public class AddResinEvent : PubSubEvent
        {

        }
        #endregion

        public partial class SubViewModel
        {
            public Button _button;
            public UserControl _userControl;
            public SubViewModel(Button button, UserControl userControl)
            {
                _button = button;
                _userControl = userControl;
            }
            public void SetDisaable()
            {
                _userControl.IsEnabled = false;
                _button.IsEnabled = false;
                _button.Opacity = 0.3;
                _button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
            }
            public void SetVisiable()
            {
                _userControl.IsEnabled = true;
                _userControl.Visibility = Visibility.Visible;
                _button.Opacity = 1;
                _button.Foreground = new SolidColorBrush(Colors.RoyalBlue);
            }
            public void SetHidden()
            {
                _userControl.IsEnabled = true;
                _userControl.Visibility = Visibility.Hidden;
                _button.Opacity = 1;
                _button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
            }
        }
        public class MessageBoxItem
        {
            public string info { get; set; }

        }
        int messageNumLmit = 100;
        
        private log4net.ILog log = log4net.LogManager.GetLogger("logApp");//获取一个日志记录器

        private void MessageInfoUpdate(string info, LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    log.Info(info);
                    break;
                case LogLevel.Debug:
                    log.Debug(info);
                    break;
                case LogLevel.Warn:
                    log.Warn(info);
                    break;
                case LogLevel.Error:
                    log.Error(info);
                    break;
                case LogLevel.Fatal:
                    log.Fatal(info);
                    break;
            }
            _ea.GetEvent<MessageEvent>().Publish(info);

        }
        private void MessageInfoUpdate_X(Tuple<bool,string> res)
        {
            if (res.Item1)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageInfoUpdate_Thread($"{res.Item2}", LogLevel.Info);
                }));
            }
            else
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageInfoUpdate_Thread($"{res.Item2}", LogLevel.Warn);
                }));
            }
        }
        private void MessageInfoUpdate_Thread(string info, LogLevel logLevel = LogLevel.Info)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageInfoUpdate(info, logLevel);
            }));
        }

        public void MessageInfoUpdate(string info)
        {
            while (messageBoxItems.Count > messageNumLmit)
            {
                messageBoxItems.RemoveAt(messageBoxItems.Count - 1);
            }

            MessageBoxItem messageBoxItem = new MessageBoxItem()
            {
                info = DateTime.Now.ToString("hh:mm->") + info,
            };
            messageBoxItems.Insert(0, messageBoxItem);

            log.Info(DateTime.Now.ToString("yyyy-MM-dds hh:mm:ss->") + info);
        }

        Task _indeputMonitor;
        public MainWindowViewModel(IEventAggregator ea, PrinterMotionControl printerMotionControl, PrinterProjectionControl printerProjectionControl, PartModelFactory partModelFactory)
        {
            string appName = Process.GetCurrentProcess().MainModule.ModuleName;
            string appFile = Process.GetCurrentProcess().MainModule.FileName;
            log4net.Config.XmlConfigurator.Configure();
            ClassValue.appDir = appFile.Substring(0, appFile.LastIndexOf("\\"));
            _ea = ea;
            _partmodels = new PartModelFactory(_ea);
            partModelFactory = _partmodels;
            PrintScene3D_ViewModel = new PrintScene3DViewModel(_ea, _partmodels);
            _ppc = new PrinterProjectionControl(_ea);
            _pmc = new PrinterMotionControl(_ea);
            _printtask = new PrintTask(_ea, _pmc, _ppc, _partmodels);
            _printtask.partModels = _partmodels.GetPartModels();
            _printTaskHolder = new PrintTaskHolder(_ea, _printtask, _pmc, _ppc);
            PrintControl_ViewModel = new PrintControlViewModel(_ea, _printtask, _pmc, _ppc);
            PrintTaskSetup_ViewModel = new PrintTaskSetupViewModel(_ea, _pmc, _ppc, _printTaskHolder);
            PrintTaskMonitor_ViewModel = new PrintTaskMonitorViewModel(_ea, _printTaskHolder);
            Setting_ViewModel = new SettingViewModel(_ea, _pmc, _ppc);
            User_ViewModel = new UserViewModel(_ea);
            subViewSwitchCommand = new DelegateCommand<string>(SubViewSwitch_Model);
            PrinterControlCommand = new DelegateCommand<string>(PrinterControlFunction);
            SliceAllLayerCommand = new DelegateCommand(SliceAllLayer);

            _indeputMonitor= new Task(() => {
                pInputMonitor_Thread();
            });
            _indeputMonitor.Start();
            _ea.GetEvent<UpDatePrintTaskStatusEvent>().Subscribe(produceMode =>
            {
                Task.Run(() =>
                { MessageInfoUpdate_X(pCloud.UpdatePrinterProduceMode(produceMode)); });
            });
            _ea.GetEvent<AddResinEvent>().Subscribe(() =>
            {
                MessageInfoUpdate_Thread("触发补液");
                ResinAdd();
            });
            _ea.GetEvent<UpDatePrinterAlertEvent>().Subscribe(alerts =>
            {
                Task.Run(() =>
                { MessageInfoUpdate_X(pCloud.UpdatePrinterAlert(alerts.Item1, alerts.Item2)); });
            });
            _ea.GetEvent<PushMessageEvent>().Subscribe(msg =>
            {
                Task.Run(() =>
                { MessageInfoUpdate_X(pCloud.PushMessage(msg.Item1,msg.Item2)); });
            });
            _ = _ea.GetEvent<GetSublotEvent>().Subscribe(msg =>
              {
                  _ = Task.Run(() =>
                    {
                        var getSublotRes = pCloud.GetSublots(msg);
                        MessageInfoUpdate_X(getSublotRes);
                        if (getSublotRes.Item1)
                        {
                            double startLevel = _pmc.GetModBusAD(ModBusReadEnum.ResinAddBoxLevel);
                            if (startLevel == -1) 
                            {
                                MessageInfoUpdate_Thread($"MODBUS连接断开");
                                return;
                            }
                            MessageInfoUpdate_Thread($"加树脂前液位{startLevel.ToString("0.0mm")}");
                            var res = DialogHelper.ShowYONDialog("提示", $"{msg}物料校验成功，加液完成后再点击确定", DialogHelper.DialogType.YesNo);
                            double endLevel = _pmc.GetModBusAD(ModBusReadEnum.ResinAddBoxLevel);
                            if (endLevel == -1)
                            {
                                MessageInfoUpdate_Thread($"MODBUS连接断开");
                                return;
                            }
                            MessageInfoUpdate_Thread($"加树脂后液位{endLevel.ToString("0.0mm")}");
                            if (res)
                            {
                                ClassValue.MESConfig.SublotId = msg;
                                double addVolumn = _pmc._config.levelControlConfig.resinAddCrossArea * (endLevel - startLevel) / 1000;
                                MessageInfoUpdate_Thread($"树脂添加量{addVolumn.ToString("0.0ml")}");
                                if (ClassValue.userinfo.userRole == UserRole.mesUser)
                                {
                                    _ea.GetEvent<UpDatePrinterAlertEvent>().Publish(new Tuple<PrinterAlerts, bool>(PrinterAlerts.replenishment, false));
                                }
                            }
                            else
                            {
                                MessageInfoUpdate_Thread($"点击取消添加树脂按钮");
                            }
                        }
                        else
                        {
                            DialogHelper.ShowMessageDialog("错误", getSublotRes.Item2, DialogHelper.DialogType.Yes, 10);
                        }


                    });
              });
            _ea.GetEvent<StartAPPViewEvent>().Subscribe(() =>
            {
                
                AngleDLP.LiteDBHelper.CheckDBfile();
                _ea.GetEvent<ProgressViewEvent>().Publish(new ProgressViewModel()
                {
                    topic = "设备初始化",
                    value = 0,
                    info = $"开始初始化",
                    visibility = System.Windows.Visibility.Visible
                });
                
                
                //根据角色控制打印机功能
                if (!ClassValue.EnableProjCalibratorMode)
                {
                    log.Info($"启动目录{ClassValue.appDir}");
                    string versionString = "版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n";
                    MessageInfoUpdate_Thread($"启动程序ver[{ClassValue.AppVersion}]", LogLevel.Info);

                    if (ClassValue.userinfo.userRole == UserRole.mesUser)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageInfoUpdate_Thread($"{ClassValue.userinfo.name}MES登录", LogLevel.Info);
                        }));
                        var pCheckRes = pCloud.StartUpCheck();
                        if (pCheckRes.Item1)
                        {
                            _ea.GetEvent<UpDatUserUIEvent>().Publish(ClassValue.userinfo);
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MessageInfoUpdate_Thread($"{pCheckRes.Item2}", LogLevel.Info);
                            }));
                            MessageInfoUpdate_X(pCloud.UpdatePrinterSoftwareRunningtatus(true));
                        }
                        else
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MessageInfoUpdate_Thread($"{pCheckRes.Item2}。 设备取消初始化", LogLevel.Error);
                            }));
                            new Task(() => DialogHelper.ShowMessageDialog("提示", $"{pCheckRes.Item2}。 设备取消初始化")).Start();
                            _ea.GetEvent<ProgressViewEvent>().Publish(new ProgressViewModel()
                            {
                                topic = "设备初始化",
                                value = 100,
                                info = $"初始化完成",
                                visibility = System.Windows.Visibility.Collapsed
                            });
                            return;
                        }
                    }
                    _ea.GetEvent<ProgressViewEvent>().Publish(new ProgressViewModel()
                    {
                        topic = "设备初始化",
                        value = 10,
                        info = $"确认登录信息",
                        visibility = System.Windows.Visibility.Visible
                    });
                    Task.Run(() =>
                    {
                        _ea.GetEvent<ProgressViewEvent>().Publish(new ProgressViewModel()
                        {
                            topic = "设备初始化",
                            value = 20,
                            info = $"控制卡初始化",
                            visibility = System.Windows.Visibility.Visible
                        });
                        if (_pmc.ReadConfig())
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MessageInfoUpdate_Thread("读入motion配置文件", LogLevel.Info);
                            }));

                            //_pmc.Init();
                            if (!_pmc.Init())
                            {
                                _pmc.Close();
                                _pmc.Init();
                            }

                        }
                        else
                        {
                            //弹窗pmc参数读入失败
                            //信息，文件，MD5是否正确
                            new Task(() => DialogHelper.ShowMessageDialog("提示", $"控制参数读入异常，请确认print motion config文件！")).Start();
                        }

                        _ea.GetEvent<ProgressViewEvent>().Publish(new ProgressViewModel()
                        {
                            topic = "设备初始化",
                            value = 40,
                            info = $"初始化屏幕",
                            visibility = System.Windows.Visibility.Visible
                        });
                        if (_ppc.ReadConfig())
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MessageInfoUpdate_Thread("读入projection配置文件", LogLevel.Info);
                            }));
                            _ppc.InitScreens();
                            _ea.GetEvent<ProgressViewEvent>().Publish(new ProgressViewModel()
                            {
                                topic = "设备初始化",
                                value = 70,
                                info = $"初始化光机",
                                visibility = System.Windows.Visibility.Visible
                            });
                            _ea.GetEvent<AddProjectorPropertyChangedEven>().Publish();
                            _ppc.InitProjector();
                           
                        }
                        else
                        {
                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MessageInfoUpdate_Thread($"光机参数读入异常，请确认print prjection config文件！", LogLevel.Info);
                            }));
                            new Task(() => DialogHelper.ShowMessageDialog("提示", $"光机参数读入异常，请确认print prjection config文件！")).Start();
                            _ppc.P_List.Add(new Projector(_ea));
                            _ppc.P_List.Add(new Projector(_ea));
                        }
                        _ea.GetEvent<ProgressViewEvent>().Publish(new ProgressViewModel()
                        {
                            topic = "设备初始化",
                            value = 100,
                            info = $"初始化完成",
                            visibility = System.Windows.Visibility.Visible
                        });
                        Thread.Sleep(3000);
                        _ea.GetEvent<ProgressViewEvent>().Publish(new ProgressViewModel()
                        {
                            topic = "设备初始化",
                            value = 100,
                            info = $"初始化完成",
                            visibility = System.Windows.Visibility.Collapsed
                        });
                        if (ClassValue.userinfo.userRole == UserRole.mesUser&& _ppc._isReady & _pmc._isReady)
                        {
                            MessageInfoUpdate_X(pCloud.UpdatePrinterHardwareRunningtatus(true, _pmc._config));
                            Task.Run(() =>
                            {
                                pCloudMonitor_timer_Tick();
                            });
                            
                            
                            
                        }
                    });
                    
                   
                    
                    
                    //开始对PMC PPC进行状态扫描
                    pStsMonitor_timer = new DispatcherTimer();
                    pStsMonitor_timer.Tick += new EventHandler(pStsMonitor_timer_Tick);
                    pStsMonitor_timer.Interval = new TimeSpan(0, 0, 0, 0, 500);//设置时间间隔
                    pStsMonitor_timer.Start();
                    
                }
                else
                {
                    _ea.GetEvent<ShowProjCalibratorView_Event>().Publish();
                }
               
            });

            _ea.GetEvent<MessageEvent>().Subscribe(message =>
            {
                if (ClassValue.userinfo.userRole == UserRole.mesUser)
                {
                    try
                    {
                        pCloud.PushMessage(message, PrinterEventLevel.info);
                    }
                    catch
                    { }
                }
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    while (messageBoxItems.Count > messageNumLmit)
                    {
                        MessageBoxItems.RemoveAt(messageBoxItems.Count - 1);
                    }
                    MessageBoxItem messageBoxItem = new MessageBoxItem()
                    {
                        info = DateTime.Now.ToString("hh:mm->") + message,
                    };
                    MessageBoxItems.Insert(0, messageBoxItem);
                }));
            });
            _ea.GetEvent<ProgressDialogEvent>().Subscribe(message =>
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (message.Item1 == -1)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            //隐藏进度窗口
                            ProgressControl = new ProgressViewModel();
                        }));
                    }
                    else
                    {
                        ProgressViewModel pvm = new ProgressViewModel()
                        {
                            topic = message.Item2,
                            value = message.Item1,
                            info = message.Item3,
                            visibility = System.Windows.Visibility.Visible
                        };
                        ProgressControl = pvm;
                    }
                }));
            });
            _ea.GetEvent<UploadProgressEvent>().Subscribe(message =>
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (message.Item1 == -1)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            //隐藏进度窗口
                            ProgressControl = new ProgressViewModel();
                        }));
                    }
                    else
                    {
                        ProgressViewModel pvm = new ProgressViewModel()
                        {
                            topic = "导入选定模型",
                            value = (int)Math.Round((double)100 * (message.Item1 + 1) / message.Item2),
                            info = $"正在导入第{message.Item1 + 1}个模型",
                            visibility = System.Windows.Visibility.Visible
                        };
                        ProgressControl = pvm;
                    }
                    SliceAllMaxHeight = message.Item3;
                }));
            });
            _ea.GetEvent<ChangeFullscreanIconEvent>().Subscribe(isfull =>
            {
                if (isfull)
                {
                    fullscreenIcon = (Geometry)Application.Current.Resources["i_Maxmize"];
                }
                else
                {
                    fullscreenIcon = (Geometry)Application.Current.Resources["i_Minimize"];
                }

            });

            _ea.GetEvent<AppLogEvent>().Subscribe(message =>
            {
                log.Info(message);
            });

            _ea.GetEvent<UpdateTransformUIEvent>().Subscribe(trans =>
            {
                _printtask.partModels[trans.index].TX = trans.trans.Value.OffsetX;
                _printtask.partModels[trans.index].TY = trans.trans.Value.OffsetY;
                _printtask.partModels[trans.index].TZ = trans.trans.Value.OffsetZ;
                _printtask.partModels[trans.index].ModelTransform = trans.trans;
            });

            _ea.GetEvent<UICloseEvent>().Subscribe(() =>
            {
                if (App.Current.MainWindow.IsActive)
                {
                    if (_printtask.printTaskStatus == 1)
                    {
                        //提示是否停止打印
                        Task.Run(() => {
                            var res = DialogHelper.ShowYONDialog("警告", $"是否先停止打印?", DialogHelper.DialogType.YesNo);
                            if (res)
                            {
                                _ea.GetEvent<PrintTaskEvent>().Publish("StopPrint");
                            }
                        });
                    }
                    else
                    {
                        var checktask = new Task(() =>
                        {
                            var res = DialogHelper.ShowYONDialog("警告", $"是否关闭程序？", DialogHelper.DialogType.YesNo);
                            if (res)
                            {
                                if (ClassValue.userinfo.userRole == UserRole.mesUser && _ppc._isReady & _pmc._isReady)
                                {
                                    MessageInfoUpdate_X(pCloud.UpdatePrinterHardwareRunningtatus(false, _pmc._config));
                                }
                                if (_pmc != null)
                                {
                                    _pmc.LightControl(0, 0, 0, 0, 0, true);
                                    if (_pmc._isReady)
                                    {
                                        if (_pmc.GetIO(DIEnum.IsResinBlocked) == 0)
                                        {
                                            _pmc.AddResinValueSwitch(false);
                                            while (_pmc.GetIO(DIEnum.IsResinBlocked) == 0)
                                            {
                                                Thread.Sleep(200);
                                            }
                                        }
                                        _pmc.Close();
                                        
                                    }
                                    
                                }
                                if (PPC != null)
                                {
                                    PPC.CloseProjector();
                                }
                                if (ClassValue.userinfo.userRole == UserRole.mesUser)
                                {
                                    MessageInfoUpdate_X(pCloud.UpdatePrinterSoftwareRunningtatus(false));
                                }
                                //Thread.Sleep(1000);
                                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    System.Windows.Application.Current.Shutdown();
                                    Process.GetCurrentProcess().Kill();
                                }));

                            }
                        });
                        checktask.Start();
                    }
                    
                }
                
                //ProjCalibratorViewModel.camera.Dispose();
            });

            _ea.GetEvent<ProgressViewEvent>().Subscribe(pvm =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    ProgressControl = pvm;
                }));
            });

            _ea.GetEvent<ModelsProgressViewEvent>().Subscribe(mpvm =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    ModelsTipControl = mpvm;
                }));
            });

            _ea.GetEvent<PrintProgressViewEvent>().Subscribe(ppvm =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    if (ppvm.printMode == PrintMode.Continual)
                    {
                        ppvm.printIcon = (Geometry)Application.Current.Resources["i_Continual"];
                        ppvm.printHolderDiscription = "本地连续打印";
                        ppvm.printHolderDiscription += $"已打印{_printTaskHolder.localDataServer.localDataItems.FindAll(p => p.itemstatus == LocalDataServer.ItemStatus.printed).Count}个，共{_printTaskHolder.localDataServer.localDataItems.Count}个。";
                    }
                    else if (ppvm.printMode == PrintMode.Single)
                    {
                        ppvm.printIcon = (Geometry)Application.Current.Resources["i_OneTime"];
                        ppvm.printHolderDiscription = "本地单次打印";
                    }
                    else if (ppvm.printMode == PrintMode.MES)
                    {
                        ppvm.printIcon = (Geometry)Application.Current.Resources["i_Cloud"];
                        ppvm.printHolderDiscription = "MES打印";
                    }
                    else if (ppvm.printMode == PrintMode.Calib)
                    {
                        ppvm.printIcon = (Geometry)Application.Current.Resources["i_Calib"];
                        ppvm.printHolderDiscription = "PQ验证打印:";
                        ppvm.printHolderDiscription += $"验证信息:{_printTaskHolder._printTask.configName}个。";
                    }
                    PrintProgressControl = ppvm;
                }));
            });

        }
        private async void ResinAdd()
        {
            ClassValue.CanAddResin = false;
            if (ClassValue.userinfo.userRole != UserRole.mesUser)
            {
                DialogHelper.ShowMessageDialog("错误", "未登录MES账户!");
                ClassValue.CanAddResin = true;
                return;
            }

            var taskSublot = new Task<Tuple<bool, string>>(() => DialogHelper.ShowInputDialog("请扫树脂桶条形码"));
            taskSublot.Start();
            await taskSublot;
            var sublot = taskSublot.Result.Item2;
            if (sublot != null)
            {
                _ea.GetEvent<GetSublotEvent>().Publish(sublot);

            }
            else
            {
                DialogHelper.ShowMessageDialog("提示", "取消补液!", DialogHelper.DialogType.Yes, 5);
                ClassValue.CanAddResin = true;
                return;
            }
            ClassValue.CanAddResin = true;


        }
        private void pStsMonitor_timer_Tick(object sender, EventArgs e)//同步打印机状态视图
        {
            int finishCount=0;
            bool isError = false;
            try
            {
                PrinterStatusTipViewModel pstvm = new PrinterStatusTipViewModel();
                if (PPC.P_List.Count > 0)
                {
                    if (PPC.P_List[0] != null && PPC._isAvaliable)
                    {
                        if (PPC.P_List[0].windowsReady)
                        {
                            pstvm.p1Color = new SolidColorBrush(System.Windows.Media.Colors.Yellow);
                            if (PPC.P_List[0].projectorReady)
                            {
                                pstvm.p1Color = new SolidColorBrush(System.Windows.Media.Colors.Green);
                                finishCount++;
                            }
                        }
                    }
                    else
                    {
                        pstvm.p1Color = new SolidColorBrush(System.Windows.Media.Colors.Red);
                        isError = true;
                    }


                    if (PPC.P_List[1] != null && PPC._isAvaliable)
                    {
                        if (PPC.P_List[1].windowsReady)
                        {
                            pstvm.p2Color = new SolidColorBrush(System.Windows.Media.Colors.Yellow);
                            if (PPC.P_List[1].projectorReady)
                            {
                                pstvm.p2Color = new SolidColorBrush(System.Windows.Media.Colors.Green);
                                finishCount++;
                            }
                        }
                    }
                    else
                    {
                        pstvm.p2Color = new SolidColorBrush(System.Windows.Media.Colors.Red);
                        isError = true;
                    }
                }
                else
                {
                    pstvm.p1Color = new SolidColorBrush(System.Windows.Media.Colors.Gray);
                    pstvm.p2Color = new SolidColorBrush(System.Windows.Media.Colors.Gray);
                }
                
                if (_pmc!= null&&_pmc._isAvaliable)
                {
                    if (_pmc._isReady)
                    {
                        pstvm.cColor = new SolidColorBrush(System.Windows.Media.Colors.Green);
                        finishCount++;
                    }
                    else
                    {
                        pstvm.cColor = new SolidColorBrush(System.Windows.Media.Colors.Gray);
                    }
                }
                else
                {
                    pstvm.cColor = new SolidColorBrush(System.Windows.Media.Colors.Red);
                    isError = true;
                }

                if (finishCount == 3)
                {
                    pstvm.tips = "Ready";
                    pstvm.pColor = new SolidColorBrush(System.Windows.Media.Colors.Green);

                }
                if (isError)
                {
                    pstvm.tips = "配置失败";
                    pstvm.pColor = new SolidColorBrush(System.Windows.Media.Colors.Red);
                }

                if (_printtask != null && _printtask._Avaliable)
                {
                    //如果计算过切层，则变为浅绿色
                    //如果完成初始化，则变为绿色
                    if( _printtask.isMSS_Computed == 2 )
                    {
                        pstvm.ptColor = new SolidColorBrush(System.Windows.Media.Colors.LightGreen);
                    }

                    if (_printtask.isInited)
                    {
                        pstvm.ptColor = new SolidColorBrush(System.Windows.Media.Colors.Green);
                    }
                }
                else
                {
                    pstvm.ptColor = new SolidColorBrush(System.Windows.Media.Colors.Red);
                }
                
                PrinterStatusTipControl = pstvm;

                

            }
            catch (Exception ex)
            {
                MessageInfoUpdate_Thread(ex.ToString(), LogLevel.Error);
            }

        }
        private void pInputMonitor_Thread()
        {
            while (true)
            {
                if (_pmc._isReady)
                {
                    int resStartStopPrint = _pmc.GetIO(DIEnum.StartStopPrint);
                    if (resStartStopPrint == 1)
                    {
                        //根据printTask状态
                    }
                    int resOpenCloseWindow = _pmc.GetIO(DIEnum.OpenCloseWindow);
                    if (resOpenCloseWindow == 1)
                    {
                        if (_pmc.isWindowClose())
                        {
                            new Task(() =>
                            {
                                _pmc.OpenWindow();
                            }).Start();

                        }
                        else if (_pmc.isWindowOpen())
                        {
                            new Task(() =>
                            {
                                _pmc.CloseWindow();
                            }).Start();
                        }
                        Thread.Sleep(1000);
                    }
                    int resAddResin = _pmc.GetIO(DIEnum.AddResin);
                    if (resAddResin == 1)
                    {
                        if (ClassValue.CanAddResin)
                        {
                            _ea.GetEvent<AddResinEvent>().Publish();
                        }
                        else
                        {
                            MessageInfoUpdate_Thread("正在补液中");
                        }
                    }


                   
                }
                Thread.Sleep(200);
            }
            
        }
        private void pCloudMonitor_timer_Tick()//
        {
            while (true)
            {
                try
                {
                    if (_pmc._isReady && _ppc._isReady)
                    {
                        //开始上传数据，否则
                        List<double> resinTemp = new List<double>();
                        resinTemp.Add(Math.Round(_pmc.GetAD(ADEnum.ResinTemp), 1));
                        double ledTemp = 24;
                        //if (leftProj)
                        //{
                        //    ledTemp = Math.Round(_ppc.P_List[0].GetLEDTemperature(),1);
                        //}
                        //else
                        //{
                        //    ledTemp = Math.Round(_ppc.P_List[1].GetLEDTemperature(),1);
                        //}
                        //leftProj = !leftProj;
                        HardwareStatus hardwareStatus = new HardwareStatus()
                        {
                            platformHum = Math.Round(_pmc.GetAD(ADEnum.EnvHum), 1),
                            platformTemp = Math.Round(_pmc.GetAD(ADEnum.EnvTemp), 1),
                            resinTemp = resinTemp,
                            airTemp = Math.Round(_pmc.GetAD(ADEnum.EnvTemp), 1),
                            resinLevel = Math.Round(_pmc.GetModBusAD(ModBusReadEnum.ResinAddBoxLevel), 1),
                            ledTemp = ledTemp
                        };
                        var res = pCloud.UpdatePrinterHardwareStatus(hardwareStatus);
                        log.Info($"Pcloud hardware monitor :{Newtonsoft.Json.JsonConvert.SerializeObject(hardwareStatus)}");
                        if (res.Item1)
                        {
                            log.Warn(res.Item2);
                        }
                        else
                        {
                            MessageInfoUpdate_X(res);
                        }
                    }
                    else
                    {
                        MessageInfoUpdate_Thread($"pCloud 硬件状态监控失败: 硬件未连接", LogLevel.Error);
                    }
                }
                catch(Exception e)
                {
                    
                }
                Thread.Sleep(6000);
            }
            


        }

        public void SliceAllLayer()
        {
            _ea.GetEvent<SliceAll3DEvent>().Publish(SliceAllIndex);
        }

        [Obsolete]
        private void PrinterControlFunction(string cmd)
        {
            if (cmd == "StartPrint")
            {
                if (ClassValue.userinfo.userRole == UserRole.mesUser)
                {
                    //启动MES打印
                    _printTaskHolder.StartPrinting(true); 
                    return;
                }
                else
                {
                    //打开setup界面
                    _ea.GetEvent<ViewSwitchEvent>().Publish("PrintTaskSetup");
                }
                return;
            }
            if (cmd == "StopPrint")
            {
                
                return;
            }
            if (cmd == "AddResin")
            {
                
                if (ClassValue.CanAddResin)
                {
                    _ea.GetEvent<AddResinEvent>().Publish();
                }
                else
                {
                    MessageInfoUpdate_Thread("正在补液中");
                }
            }
            if (cmd == "Window")
            {
                if (_pmc.isWindowClose())
                {
                    new Task(() =>
                    {
                        _pmc.OpenWindow();
                    }).Start();

                }
                else if (_pmc.isWindowOpen())
                {
                    new Task(() =>
                    {
                        _pmc.CloseWindow();
                    }).Start();
                }
            }
            if (cmd == "ShutDown")
            {
                _ea.GetEvent<UICloseEvent>().Publish();
            }

        }
        private void SubViewSwitch_Model(string viewInd)
        {
            _ea.GetEvent<ViewSwitchEvent>().Publish(viewInd);
        }
       
    }

    public class ProgressViewModel
    {
        public Visibility visibility { get; set; } = Visibility.Collapsed;
        public string topic { get; set; } = "initTopic";
        public int value { get; set; } = 0;
        public string info { get; set; } = "testContent";

    }
    public class ModelsTipViewModel
    {
        public Visibility visibility { get; set; } = Visibility.Collapsed;
        public SolidColorBrush fColor { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
        public SolidColorBrush cColor { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Gray);
        public int startAngle { get; set; } = 0;
        public int endAngle { get; set; } = 90;
        public int currInd { get; set; } = 0;

        public int arcThickness { get; set; } = 15;

    }
    public class PrinterStatusTipViewModel
    {
        
        public SolidColorBrush p1Color { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Gray);
        public SolidColorBrush p2Color { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Gray);
        public SolidColorBrush cColor { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Gray);

        public SolidColorBrush pColor { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Gray);
        public SolidColorBrush ptColor { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Gray);
        public string tips { get; set; } = "未连接";


    }
    public class PrintProgressViewModel
    {
        public PrintMode printMode { get; set; }

        public Geometry printIcon { get; set; } = null; 

        public string printHolderDiscription { get; set; } = null;

        public string printTaskDiscription { get; set; } = null;
        public Visibility progressBarVisibility { get; set; } = Visibility.Collapsed;
        public SolidColorBrush fColor { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
        public SolidColorBrush cColor { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Gray);
        public SolidColorBrush pColor { get; set; } = new SolidColorBrush(System.Windows.Media.Colors.Blue);
        public int progress { get; set; } = 0;
        public string layerTips { get; set; } = "";

    }
}