using AngelDLP.PMC;
using AngleDLP;
using AngleDLP.Models;
using AngleDLP.PPC;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using static AngelDLP.PrintTask;
using static AngelDLP.ViewModels.MainWindowViewModel;
using static AngleDLP.LiteDBHelper;

namespace AngelDLP.ViewModels
{
    public class PrintTaskSetupViewModel : BindableBase
    {
        IEventAggregator _ea;
        private string subViewName = "PrintTaskSetup";
        
        private PrintTaskHolder _printTaskHolder;
        private PrinterVarifycation printerVarifycation;
        
        

        

        private PrintTaskConfig selectedPrintTaskConfig = new PrintTaskConfig();
        public PrintTaskConfig SelectedPrintTaskConfig
        {
            get { return selectedPrintTaskConfig; }
            set { SetProperty(ref selectedPrintTaskConfig, value); }

        }

        private PrintTaskConfigDB selectedPrintTaskConfigDB = new PrintTaskConfigDB();
        public PrintTaskConfigDB SelectedPrintTaskConfigDB
        {
            get { return selectedPrintTaskConfigDB; }
            set { SetProperty(ref selectedPrintTaskConfigDB, value); }

        }

        private List<PrintTaskConfigDB> selectedPrintTaskConfigs = new List<PrintTaskConfigDB>();
        public List<PrintTaskConfigDB> SelectedPrintTaskConfigs
        {
            get { return selectedPrintTaskConfigs; }
            set { SetProperty(ref selectedPrintTaskConfigs, value); }

        }

        private IEnumerable<IGrouping<string, PrintTaskConfigDB>> groupedPrintTaskConfigDBs;
        public IEnumerable<IGrouping<string, PrintTaskConfigDB>> GroupedPrintTaskConfigDBs
        {
            get { return groupedPrintTaskConfigDBs; }
            set { SetProperty(ref groupedPrintTaskConfigDBs, value); }

        }

        private PrinterProjectionControl _ppc;
        private PrinterMotionControl _pmc;

        private int _SelectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set { SetProperty(ref _SelectedTabIndex, value); }
        }
        private int _SelectedConfigIndex;
        public int SelectedConfigIndex
        {
            get
            {
                return _SelectedConfigIndex;
            }
            set
            {
                _SelectedConfigIndex = value;
                //OnPropertyChanged(new PropertyChangedEventArgs("SelectedConfigIndex"));
            }
        }
        private int _SelectedConfigGroupIndex;
        public int SelectedConfigGroupIndex
        {
            get
            {
                return _SelectedConfigGroupIndex;
            }
            set
            {
                _SelectedConfigGroupIndex = value;
                //OnPropertyChanged(new PropertyChangedEventArgs("SelectedConfigGroupIndex"));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
           
            //if (e.PropertyName == "SelectedTabIndex" && SelectedTabIndex == 1)
            //{
            //    List<PrintTaskConfigDB> allconfigs = AngleDLP.LiteDBHelper.PrintTaskConfigDB.GetConfigs(ClassValue.PrinterDBFile);//应按name进行group
            //    GroupedPrintTaskConfigDBs = allconfigs.GroupBy(s => s.name).ToList();
            //    //找到预先配置的版本
            //    var groupRes = GroupedPrintTaskConfigDBs.Select((s, i) => {
            //        return new
            //        {
            //            Value = s,
            //            Index = i
            //        };
            //    }).FirstOrDefault(x => x.Value.Key == ClassValue.DefalutPrintTaskConfigName);
            //    SelectedPrintTaskConfigs = groupRes.Value.ToList();
            //    SelectedConfigGroupIndex = groupRes.Index;
            //    var configRes = SelectedPrintTaskConfigs.Select((s, i) => {
            //        return new
            //        {
            //            Value = s,
            //            Index = i
            //        };
            //    }).FirstOrDefault(x => x.Value.version == ClassValue.DefalutPrintTaskConfigVersion);
            //    selectedPrintTaskConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<PrintTaskConfig>(configRes.Value.printTaskConfigStr);
            //    SelectedConfigIndex = configRes.Index; 
            //    //SelectedPrintTaskConfigs = .ToList();
            //    //SelectedPrintTaskConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<PrintTaskConfig>(SelectedPrintTaskConfigs.FirstOrDefault(s => s.version == ClassValue.DefalutPrintTaskConfigVersion).printTaskConfigStr);
            //}
            
            if (e.PropertyName == "SelectedConfigGroupIndex")
            {
                if (SelectedConfigGroupIndex != -1)
                {
                    SelectedPrintTaskConfigs = GroupedPrintTaskConfigDBs.ElementAt(SelectedConfigGroupIndex).OrderBy(s => s.version).ToList();
                }
                else
                {
                    
                }
            }

            if (e.PropertyName == "SelectedConfigIndex" )
            {
                if (SelectedConfigIndex != -1)
                {
                    SelectedPrintTaskConfigDB = SelectedPrintTaskConfigs[SelectedConfigIndex];
                    SelectedPrintTaskConfig = PrintTaskConfig.DeserializeObject(SelectedPrintTaskConfigDB.printTaskConfigStr);
                }
                else
                {
                    
                }
            }
        }

        private bool printtaskSetupEnable = true;
        public bool PrinttaskSetupEnable
        {
            get { return printtaskSetupEnable; }
            set
            {
                if (_printTaskHolder._printTask.printTaskStatus == 0)
                {
                    printtaskSetupEnable = true;

                }
                else
                {
                    printtaskSetupEnable = false;
                }
                SetProperty(ref printtaskSetupEnable, value);
            }
        }

        private string _fileinfo = "请选择导入文件夹或文件";

        public string Fileinfo
        {
            get { return _fileinfo; }
            set { SetProperty(ref _fileinfo, value); }

        }
        private bool _uploadButtonEnabled = false;

        public bool UploadButtonEnabled
        {
            get { return _uploadButtonEnabled; }
            set { SetProperty(ref _uploadButtonEnabled, value); }

        }
        #region select file tab item
        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand <string> FileSelectCommand { get; private set; }
        public class PrintTack_SelectInfoEvent : PubSubEvent<object[]>
        {

        }
        #endregion
        public DelegateCommand ResetTaskCommand { get; private set; }
        public DelegateCommand<string> SwitchTabCommand { get; private set; }
        public DelegateCommand<string> PrintTaskDBCommand { get; private set; }
        public DelegateCommand AutoLayoutCommand { get; private set; }
        public DelegateCommand <string> SelectPrintModeCommand { get; private set; }
        //public DelegateCommand StartSliceCommand { get; private set; }
        //public DelegateCommand ConfirmPrintTaskCommand { get; private set; }
        public DelegateCommand<string> ConfirmPrintCommand { get; private set; }

        private string indText = "0";

        public string IndText
        {
            get { return indText; }
            set { SetProperty(ref indText, value); }

        }
        private ModelsTipViewModel modelsTipControl = new ModelsTipViewModel();
        public ModelsTipViewModel ModelsTipControl
        {
            get { return modelsTipControl; }
            set { SetProperty(ref modelsTipControl, value); }

        }

        public PrintTaskSetupViewModel(IEventAggregator ea, PrinterMotionControl printerMotionControl, PrinterProjectionControl printerProjectionControl, PrintTaskHolder printTaskHolder)
        {
            _ea = ea;
            _pmc = printerMotionControl;
            _ppc = printerProjectionControl;
            _printTaskHolder = printTaskHolder;
            CloseCommand = new DelegateCommand(Close);
            FileSelectCommand = new DelegateCommand<string>(FileSelect);
            SelectPrintModeCommand = new DelegateCommand<string>(SelectPrintMode);
            _ea.GetEvent<PrintTack_SelectInfoEvent>().Subscribe(message =>
            {
                if (message != null)
                {
                    this.Fileinfo = message[0].ToString();
                    this.UploadButtonEnabled = (bool)message[1];
                }
                else
                {
                    this.UploadButtonEnabled = false;
                    this.Fileinfo = "打印中无法载入";
                }
            });
            ResetTaskCommand = new DelegateCommand(ResetTask);
            AutoLayoutCommand = new DelegateCommand(AutoLayout);
            //ConfirmPrintTaskCommand = new DelegateCommand(ConfirmPrintTask);
            //StartSliceCommand = new DelegateCommand(StartSlice);
            //LevelInitCommand = new DelegateCommand(LevelInit);
            ConfirmPrintCommand = new DelegateCommand<string>(ConfirmPrint);
            SwitchTabCommand =  new DelegateCommand<string>(SwitchTab);
            PrintTaskDBCommand =  new DelegateCommand<string>(PrintTaskDB);
            _ea.GetEvent<ModelsProgressViewEvent>().Subscribe(mpvm =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    ModelsTipControl = mpvm;
                }));
            });
        }
        private void UpdateConfigSource(string configName, int configVersion)
        {
            List<PrintTaskConfigDB> allconfigs = AngleDLP.LiteDBHelper.PrintTaskConfigDB.GetConfigs();//应按name进行group
            GroupedPrintTaskConfigDBs = allconfigs.GroupBy(s => s.name).ToList().OrderBy(s => s.Key);
            if (GroupedPrintTaskConfigDBs == null|| GroupedPrintTaskConfigDBs.Count() ==0)
            {
                new Task(() => DialogHelper.ShowMessageDialog("提示", $"数据库中无配方数据")).Start();
                selectedPrintTaskConfig = null;
                return;
            }
            //找到预先配置的版本
            var groupRes = GroupedPrintTaskConfigDBs.Select((s, i) => {
                return new
                {
                    Value = s,
                    Index = i
                };
            }).FirstOrDefault(x => x.Value.Key == configName);
            if(groupRes == null )
            {
                new Task(() => DialogHelper.ShowMessageDialog("提示", $"数据库中无名为{ ClassValue.DefalutPrintTaskConfigName}的配方数据")).Start();
                //选择最新一个配方
                groupRes = GroupedPrintTaskConfigDBs.Select((s, i) => {
                    return new
                    {
                        Value = s,
                        Index = i
                    };  
                }).First();
            }
            SelectedPrintTaskConfigs = groupRes.Value.ToList();
            SelectedConfigGroupIndex = groupRes.Index;
            SelectedPrintTaskConfigs = SelectedPrintTaskConfigs.OrderBy(s => s.version).ToList();

            var configRes = SelectedPrintTaskConfigs.Select((s, i) => {
                return new
                {
                    Value = s,
                    Index = i
                };
            }).FirstOrDefault(x => x.Value.version == configVersion);
            if (configRes == null)
            {
                new Task(() => DialogHelper.ShowMessageDialog("提示", $"数据中无名称为{ClassValue.DefalutPrintTaskConfigName},版本号{ ClassValue.DefalutPrintTaskConfigVersion}的配方数据")).Start();
                configRes = SelectedPrintTaskConfigs.Select((s, i) => {
                    return new
                    {
                        Value = s,
                        Index = i
                    };
                }).First();
            }
            SelectedPrintTaskConfigDB = configRes.Value;
            SelectedPrintTaskConfig = PrintTaskConfig.DeserializeObject(configRes.Value.printTaskConfigStr);
            SelectedConfigIndex = configRes.Index;
        }
        /// <summary>
        /// 连接远程共享文件夹
        /// </summary>
        /// <param name="path">远程共享文件夹的路径</param>
        /// <param name="userName">用户名</param>
        /// <param name="passWord">密码</param>
        /// <returns></returns>
        public static bool connectState(string path, string userName, string passWord)
        {
            bool Flag = false;
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string dosLine = "net use " + path + " " + passWord + " /user:" + userName;
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                string errormsg = proc.StandardError.ReadToEnd();
                proc.StandardError.Close();
                if (string.IsNullOrEmpty(errormsg))
                {
                    Flag = true;
                }
                else
                {
                    throw new Exception(errormsg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }

        public void SwitchTab(string ind)
        {
            int tab = int.Parse(ind);
            if (tab == 1)
            {
                UpdateConfigSource(ClassValue.DefalutPrintTaskConfigName, ClassValue.DefalutPrintTaskConfigVersion);
            }
            if (tab == 2)
            {
                if(_printTaskHolder._printTask.printMode == PrintMode.Single)
                {
                    _printTaskHolder._printTask._config = SelectedPrintTaskConfig;
                    _printTaskHolder._printTask.configName = selectedPrintTaskConfigDB.name;
                    _printTaskHolder._printTask.configVersion = selectedPrintTaskConfigDB.version;
                }
                if (_printTaskHolder._printTask.printMode == PrintMode.Calib)
                {
                    bool connect120 = connectState("\\\\172.16.0.120\\stl\\开发\\PrinterVarifacation", "stluser", "stluser");
                    //string connectCode = NetworkShareConnect.connectToShare();
                    if (!connect120)
                    {
                        _ea.GetEvent<MessageEvent>().Publish($"MES文件服务器访问失败,路径\\\\172.16.0.120\\stl\\开发\\PrinterVarifacation ");
                        
                        return;
                    }
                    new Task(() => {
                        SelectedPrintTaskConfig.accuracyParas.scaleCompX = printerVarifycation.varifycationInfo.ShinkX;
                        SelectedPrintTaskConfig.accuracyParas.scaleCompY = printerVarifycation.varifycationInfo.ShinkY;
                        SelectedPrintTaskConfig.accuracyParas.contourComp = printerVarifycation.varifycationInfo.inflate;
                        SelectedPrintTaskConfig.accuracyParas.contourCompX = printerVarifycation.varifycationInfo.inflateX;
                        SelectedPrintTaskConfig.accuracyParas.contourCompY = printerVarifycation.varifycationInfo.inflateY;
                        SelectedPrintTaskConfig.accuracyParas.archCompPara = printerVarifycation.varifycationInfo.archCompPara;
                        string newDBName = printerVarifycation.varifycationInfo.VarifycationId.ToString() + "_" + printerVarifycation.varifycationInfo.PQnum;
                        LiteDBHelper.PrintTaskConfigDB.RemoveConfig(newDBName);
                        LiteDBHelper.PrintTaskConfigDB.SaveAsNewConfig(SelectedPrintTaskConfigDB, SelectedPrintTaskConfig, newDBName);
                        _printTaskHolder._printTask._config = SelectedPrintTaskConfig;
                        _printTaskHolder._printTask.configName = selectedPrintTaskConfigDB.name;
                        _printTaskHolder._printTask.configVersion = selectedPrintTaskConfigDB.version;

                        _printTaskHolder._printTask.layoutItems = new List<LayoutItem>();
                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 0; j < 5; j++)
                            {
                                int indx = i * 5 + j;
                                double x = 108 * i + 54;
                                double y = 77 * j + 38;
                                _printTaskHolder._printTask.files_Selected.Add(new PrintTask.FileSelected()
                                {
                                    stl = printerVarifycation.varifycationInfo.stls[indx].stl,
                                    modelId = printerVarifycation.varifycationInfo.stls[indx].name,
                                    isStandard = false,
                                    pretreatmentVersion = 2
                                });
                                Matrix3D matrix = new Matrix3D(1, 0, 0, 0,
                                    0, 1, 0, 0,
                                    0, 0, 1, 0,
                                    x, y, 0, 1);
                                _printTaskHolder._printTask.layoutItems.Add(new LayoutItem()
                                {
                                    Model = printerVarifycation.varifycationInfo.stls[indx].stl,
                                    TX = Math.Round(x, 3),
                                    TY = Math.Round(y, 3),
                                    RZ = 0,
                                    Matrix3D = matrix
                                });
                            }
                        }
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _ea.GetEvent<ViewSwitchEvent>().Publish(this.subViewName);
                        }));

                        _printTaskHolder._printTask.UploadFiles(true);
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            SwitchTab("4");
                            _ea.GetEvent<ViewSwitchEvent>().Publish(this.subViewName);
                        }));
                    }).Start();
                    return;
                }
                //确定是否可以导入文件
            }
            if (tab == 3)
            {
                if (_printTaskHolder._printTask.printMode == PrintMode.Continual)
                {
                    //弹出选择文件夹
                    SelectedTabIndex = tab +1;
                }
                if (_printTaskHolder._printTask.printMode == PrintMode.Single)
                {
                    SelectedTabIndex = tab;
                }
                return;
            }
            if (tab == 4)
            {
                if (_printTaskHolder._printTask.isMSS_Computed == 0)
                {
                    _printTaskHolder._printTask.isMSS_Computed = 1;
                    _ea.GetEvent<MessageEvent>().Publish("开始计算切层");
                    ConfirmPrintTask();
                }
                else
                {
                    _ea.GetEvent<MessageEvent>().Publish("已计算过切层");
                }
            }
            SelectedTabIndex = tab;
        }

        [Obsolete]
        public void SelectPrintMode(string mode)
        {
            PrintMode pMode = (PrintMode)int.Parse(mode);
            if (pMode == PrintMode.Single)//Continual Single MES Calib
            {
                _printTaskHolder._printTask.printMode = pMode;
                SwitchTab("1");
                return;
            }

            if (pMode == PrintMode.MES|| pMode == PrintMode.Continual )//Continual Single MES Calib
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
                }));
                bool ismes = true;
                if (pMode == PrintMode.Continual) { ismes = false; }
                _printTaskHolder.StartPrinting(ismes);
                return;
            }
            if (pMode == PrintMode.Calib)//Continual Single MES Calib
            {
                _printTaskHolder._printTask.printMode = pMode;
                new Task(() => {
                    printerVarifycation = new PrinterVarifycation(ClassValue.MESConfig.DeviceId);
                    if (printerVarifycation.CheckNew())
                    {
                        //新建参数=
                        new Task(() => DialogHelper.ShowMessageDialog("提示", $"请选择一个参数模板，除缩放、轮廓补偿、ACP外都将用于本次校准打印！本次验证参数{printerVarifycation.varifycationInfo.ShinkX}×{printerVarifycation.varifycationInfo.ShinkY}[{printerVarifycation.varifycationInfo.inflateX}×{printerVarifycation.varifycationInfo.inflateY}]ACP{printerVarifycation.varifycationInfo.archCompPara}")).Start();

                        SwitchTab("1");
                        //去选参数
                    }
                    else
                    {
                        new Task(() => DialogHelper.ShowMessageDialog("提示", $"未有关联的新验证信息")).Start();
                    }
                }).Start();
                
                return;
            }
        }
        public async void PrintTaskDB(string cmdPara)
        {
            string currConfigName = string.Empty;
            int currConfigVersion = 0;
            PrintTaskConfig currPrintTaskConfig = null;
            //增加 和另存为 ，都是保存当前视图的编辑结果
            //需要接受当前视图数据的，做一个校验方法
            //新增和保存，会被选择为当前显示
            //包括 Add，SaveAsNewVersion,


            if (cmdPara != "ImportFromFile" && selectedPrintTaskConfig == null )//除了导入文件外，其余都需要当前选中了一个版本详情
            {
                new Task(() => DialogHelper.ShowMessageDialog("提示", $"当前未选中配方")).Start();
                return;
            }
            if (cmdPara == "addHole")
            {

                SelectedPrintTaskConfig.baseCalibParas.holeComps.Add(new HoleCompItem() { radius = 3 }); ;

                return;
            }
            if (cmdPara == "minusHole")
            {
                if (selectedPrintTaskConfig.baseCalibParas.holeComps.Count > 1)
                {
                    SelectedPrintTaskConfig.baseCalibParas.elephantFeetComps.RemoveAt(SelectedPrintTaskConfig.baseCalibParas.elephantFeetComps.Count - 1);
                }
                return;
            }
            if (cmdPara == "addElephant")
            {

                SelectedPrintTaskConfig.baseCalibParas.elephantFeetComps.Add(new ElephantFeetItem() { offset = 0 }) ; ;

                return;
            }
            if (cmdPara == "minusElephant")
            {
                if (selectedPrintTaskConfig.baseCalibParas.elephantFeetComps.Count > 1)
                {
                    SelectedPrintTaskConfig.baseCalibParas.elephantFeetComps.RemoveAt(SelectedPrintTaskConfig.baseCalibParas.elephantFeetComps.Count -1);
                }
                return;
            }
            if (cmdPara == "SaveAsFile")
            {
                Stream myStream;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "cfg files   (*.cfg)|*.cfg";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        using (StreamWriter sw = new StreamWriter(myStream))
                        {
                            sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(SelectedPrintTaskConfig,Formatting.Indented));
                        }

                        myStream.Close();
                        new Task(() => DialogHelper.ShowMessageDialog("提示", $"保存成功")).Start();
                    }
                }
                return;
            }
            if (cmdPara == "Add"|| cmdPara == "ImportFromFile")//此两项都需要获取新配方名称，若名称无效，则失败
            {
                if (cmdPara == "Add")
                {
                    //参数校验待开发
                    //参考：https://www.dandelioncloud.cn/article/details/1509138129637421058
                    //对于前台数据验证也需要考虑
                    //if (!CheckConfig())
                    //{
                    //    new Task(() => DialogHelper.ShowMessageDialog("提示", $"参数输入校验失败")).Start();
                    //    return;
                    //}
                }
                else
                {
                    //check the imported value
                    //尝试反序列化
                    try
                    {
                        //文件选择
                        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog()
                        {
                            Multiselect = false,
                            Filter = "Excel Files (*.cfg)|*.cfg"
                        };
                        bool? result = openFileDialog.ShowDialog();
                        if ((bool)result)
                        {
                            using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    //lxh todo
                                    //导入、导出json未完成
                                    //保存、刷新机制未完成
                                    //
                                    string jsonstr = sr.ReadToEnd();
                                    currPrintTaskConfig = PrintTaskConfig.DeserializeObject(jsonstr);
                                    SelectedPrintTaskConfig = currPrintTaskConfig;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                //弹出新命名名称
                //异步等待窗口完成
                var task = new Task<Tuple<bool, string>>(() => DialogHelper.ShowInputDialog("请输入配方名称"));
                task.Start();
                await task;
                if (!task.Result.Item1) { var task2 = new Task(() => DialogHelper.ShowMessageDialog("提示", $"无法输入")); return; }
                var newDBName = task.Result.Item2;
                if (newDBName != null)
                {
                    if (LiteDBHelper.PrintTaskConfigDB.SaveAsNewConfig(SelectedPrintTaskConfigDB, SelectedPrintTaskConfig, newDBName))
                    {
                        //添加成功
                        var task2 = new Task(() => DialogHelper.ShowMessageDialog("提示", $"新配方 {newDBName}添加成功"));
                        task2.Start();
                    }
                    else
                    {
                        //名称重复请重试
                        var task2 = new Task(() => DialogHelper.ShowMessageDialog("提示", $"新配方 {newDBName}名称已存在，请重试！"));
                        task2.Start();
                        return;
                    }

                    currConfigName = newDBName;
                    currConfigVersion = 0;
                }

                
                
            }
            
            
            if (cmdPara == "SaveAsNewVersion")
            {

                //参数校验待开发
                //参考：https://www.dandelioncloud.cn/article/details/1509138129637421058
                //对于前台数据验证也需要考虑
                //if (!CheckConfig())
                //{
                //    new Task(() => DialogHelper.ShowMessageDialog("提示", $"参数输入校验失败")).Start();
                //    return;
                //}
                LiteDBHelper.PrintTaskConfigDB.SaveAsNewVersion(SelectedPrintTaskConfigDB, SelectedPrintTaskConfig);
                currConfigName = SelectedPrintTaskConfigDB.name;
                currConfigVersion = SelectedPrintTaskConfigDB.version + 1;
                new Task(() => DialogHelper.ShowMessageDialog("提示", $"配方 {currConfigName}的新版本Ver{currConfigVersion}添加成功")).Start();
            }

            if (cmdPara == "CancelEdit")
            {
                SelectedPrintTaskConfig = PrintTaskConfig.DeserializeObject(SelectedPrintTaskConfigDB.printTaskConfigStr);
                return;
            }
            if (cmdPara == "SetAsDefault")
            {
                //如何判断是否被更改过

                //修改配置文件中键为keyName的项的值
                var checktask = new Task<bool>(() => DialogHelper.ShowYONDialog("提示", $"原默认参数设置为{ClassValue.DefalutPrintTaskConfigName},版本号{ ClassValue.DefalutPrintTaskConfigVersion}，即将变更为{selectedPrintTaskConfigDB.name},版本号{selectedPrintTaskConfigDB.version}？",DialogHelper.DialogType.YesNo));
                checktask.Start();
                await checktask;
                if (checktask.Result)
                {
                    SelectedPrintTaskConfig = PrintTaskConfig.DeserializeObject(SelectedPrintTaskConfigDB.printTaskConfigStr);
                    IniConfigHelper.WriteSetting(ClassValue.IniFile, "PrintTask", "DefalutPrintTaskConfigName", selectedPrintTaskConfigDB.name);
                    IniConfigHelper.WriteSetting(ClassValue.IniFile, "PrintTask", "DefalutPrintTaskConfigVersion", selectedPrintTaskConfigDB.version.ToString());
                    ClassValue.DefalutPrintTaskConfigName = selectedPrintTaskConfigDB.name;
                    ClassValue.DefalutPrintTaskConfigVersion = selectedPrintTaskConfigDB.version;
                    currConfigName = ClassValue.DefalutPrintTaskConfigName;
                    currConfigVersion = ClassValue.DefalutPrintTaskConfigVersion;
                    new Task(() => DialogHelper.ShowMessageDialog("提示", $"默认参数设置为{ClassValue.DefalutPrintTaskConfigName},版本号{ ClassValue.DefalutPrintTaskConfigVersion}")).Start();
                    return;
                }
                else
                {
                    return;
                }
                
            }
            if (cmdPara == "DeleteConfig")
            {
                if (SelectedConfigGroupIndex == -1)
                {
                    new Task(() => DialogHelper.ShowMessageDialog("提示", $"未选择版本")).Start();
                    return;
                }
                //修改配置文件中键为keyName的项的值
                LiteDBHelper.PrintTaskConfigDB.RemoveConfig(selectedPrintTaskConfigs[0].name);
                //
            }
            if (cmdPara == "DeleteVersion")
            {
                if (SelectedConfigIndex == -1)
                {
                    new Task(() => DialogHelper.ShowMessageDialog("提示", $"未选择版本")).Start();
                    return;
                }
                //修改配置文件中键为keyName的项的值
                LiteDBHelper.PrintTaskConfigDB.RemoveConfigVersion( selectedPrintTaskConfigs[SelectedConfigIndex].name, selectedPrintTaskConfigs[SelectedConfigIndex].version);
            }

            //如果修改过数据库，则需要更新数据源
            UpdateConfigSource(currConfigName,currConfigVersion);
        }

        private bool CheckNum(bool isIntOrDouble, double lowLimit, double hightLimit)
        {
            // check number type
            if (isIntOrDouble)
            {

            }
            else
            {
                
            }

            return false;
        }
        public bool CheckConfig()
        {
            //Check double 

            //Check int
            
            
            return false ;
        }
        public void ResetTask()
        {
            _ea.GetEvent<PrintTack_SelectInfoEvent>().Publish(new object[] { "未选择文件", false });
            _printTaskHolder._printTask.Init();
            SelectedTabIndex = 0;
            
        }
        
        
        public void StartSlice()//开始切层
        {
            Thread thread = new Thread(() => _printTaskHolder._printTask.StartSlice());
            thread.Start();
        }
        public void AutoLayout()//自动排版
        {
            _ea.GetEvent<ViewSwitchEvent>().Publish(string.Empty);
            Thread thread = new Thread(() => {
                _printTaskHolder._printTask.AutoLayout_LXH_New();
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
                    SwitchTab("4");
                }));
            });
            thread.Start();
            
        }
        //public void LevelInit()//液位初始化
        //{
        //    Thread thread = new Thread(() => _printtask.PrintTaskInit());
        //    thread.Start();
        //}
        public void ConfirmPrintTask()
        {
            if (_printTaskHolder._printTask.printTaskStatus != 0)
            {
                _ea.GetEvent<MessageEvent>().Publish("已开始打印");
                return;
            }
            //SaveImageToFile(_printtask.layoutBitmap, "test.bmp");
            Thread thread = new Thread(() => _printTaskHolder._printTask.StartSlice());
            thread.Start();
        }


        public async void ConfirmPrint(string para)
        { 
            if(para == "PrintCurrScene")
            {
                Task t1 = new Task(() =>
                {
                    if (_printTaskHolder._printTask.printTaskStatus != 0)
                    {
                        _ea.GetEvent<MessageEvent>().Publish("已开始打印");
                        return;
                    }

                    var resInit = _printTaskHolder._printTask.PrintTaskInit();

                    
                    if (resInit.isSuccess)
                    {
                        //_printtask.initResult = resInit.Result;
                        //等待切层完成
                        if (_printTaskHolder._printTask.isMSS_Computed != 2)
                        {
                            Thread.Sleep(100);
                            
                        }
                     
                     
                        Thread thread = new Thread(() => _printTaskHolder._printTask.Print());
                        thread.Start();
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
                        }));
                    }
                    else
                    {
                        new Task(() => DialogHelper.ShowMessageDialog("提示", $"打印任务初始化失败，返回信息：{resInit.message}.打印任务终止")).Start();
                    }
                });
                t1.Start();
                
                return;
            }

            if (para == "LevelInit")
            {
                Task t1 = new Task(() =>
                {
                    _= _printTaskHolder._printTask.PrintTaskInit();
                });
                t1.Start();
                return;
            }


        }

        public void FileSelect(string para)//导入打印文件
        {
           
            if (para == "Folder" || para == "Files")
            {
                
                if (para == "Folder")
                {
                    var message= _printTaskHolder._printTask.SelectFolder();
                    if (message != null)
                    {
                        this.Fileinfo = message[0].ToString();
                        this.UploadButtonEnabled = (bool)message[1];
                    }
                    else
                    {
                        this.UploadButtonEnabled = false;
                        this.Fileinfo = "打印中无法载入";
                    }
                }
                if (para == "Files")
                {
                    var message= _printTaskHolder._printTask.SelectFiles();
                    if (message != null)
                    {
                        this.Fileinfo = message[0].ToString();
                        this.UploadButtonEnabled = (bool)message[1];
                    }
                    else
                    {
                        this.UploadButtonEnabled = false;
                        this.Fileinfo = "打印中无法载入";
                    }
                }
                return;
            }
            if (para == "Upload")
            {
                new Task(() => {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
                        this.UploadButtonEnabled = false;
                    }));
                    
                    //_ea.GetEvent<ViewSwitchEvent>().Publish("PrintTaskSetup");
                    //this.Fileinfo = string.Empty;
                    //判断是否是排版文件
                    bool isLayoutImport = false;
                    if (_printTaskHolder._printTask.files_Selected.Count == 1 && _printTaskHolder._printTask.files_Selected[0].stl.Contains(".json"))
                    {
                        isLayoutImport = true;
                    }


                    if (isLayoutImport)
                    {
                        string jsonpath = _printTaskHolder._printTask.files_Selected[0].stl;

                        string folder = jsonpath.Substring(0, jsonpath.LastIndexOf("\\") + 1);
                        _printTaskHolder._printTask.files_Selected.Clear();
                        foreach (var li in _printTaskHolder._printTask.layoutItems)
                        {
                            _printTaskHolder._printTask.files_Selected.Add(new PrintTask.FileSelected() { stl = folder + li.Model });
                        }
                    }

                    if (_printTaskHolder._printTask.files_Selected.Count == 0)
                    {
                        return;
                    }
                    else
                    {
                        string folder = _printTaskHolder._printTask.files_Selected[0].stl.Substring(0, _printTaskHolder._printTask.files_Selected[0].stl.LastIndexOf("\\"));
                        _printTaskHolder._printTask.currFileFolder = folder;
                    }
                    _printTaskHolder._printTask.UploadFiles(isLayoutImport);

                    //打开print task setup window
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
                        SwitchTab("3");
                    }));
                }).Start();
                //_ea.GetEvent<PrintTack_UploadFilesEvent>().Publish();
                
                //Thread thread = new Thread(() => PrintTask.StartSlice());
                //thread.Start();
                return;
            }
            
        }


        private void Close()
        {
            _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
        }

        
    }
}