using AngelDLP;
using AngelDLP.PMC;
using AngleDLP.Models;
using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AngleDLP.LocalDataServer;
using DataService.DataServer;
using DataService.Models.Dto.MES;
using DataService.Models.Dto;
using System.Net;
using System.Threading;
using static AngelDLP.ViewModels.MainWindowViewModel;
using AngleDLP.PPC;
using static AngelDLP.PrintTask;
using DataService.Models.Dto.Pcloud;
using static AngleDLP.Models.LogHelper;
using DataService.Models;
using System.ComponentModel;
using Prism.Mvvm;
using static AngleDLP.Models.DialogHelper;

namespace AngleDLP
{
    //连续打印
    //包含本地数据和MES连续打印
    public enum PrintMode { Continual, Single, MES, Calib };
    public class PrintTaskHolder : BindableBase
    {
        public bool _isMES;

        IEventAggregator _ea;
        
        public PrintTask _printTask;

        private PrinterProjectionControl _ppc;

        private PrinterMotionControl _pmc;

        public log4net.ILog ptLog;
        public LocalDataServer localDataServer;

        public MESTaskServer mesTaskServer  = new MESTaskServer();
        public MESTaskServer MESTaskServerx
        {
            get { return mesTaskServer; }
            set { SetProperty(ref mesTaskServer, value); }

        }

        private void SetProperty(ref MESTaskServer mesTaskServer, MESTaskServer value)
        {
            throw new NotImplementedException();
        }

        private List<LocalDataItem> currTaskItems = new List<LocalDataItem>();

        public class CreatJobEvent : PubSubEvent
        {

        }

        public class UpdatePrintProgressEvent : PubSubEvent<JobProgress>
        {

        }
        public class UpdatePrintJobEvent : PubSubEvent<JobEventItem>
        {

        }
        public PrintTaskHolder()
        {

        }
        public PrintTaskHolder(IEventAggregator eventAggregator, PrintTask printTask, PrinterMotionControl printerMotionControl, PrinterProjectionControl printerProjectionControl)
        {
            _ea = eventAggregator;
            _printTask = printTask;
            _ppc = printerProjectionControl;
            _pmc = printerMotionControl;
            ptLog = log4net.LogManager.GetLogger($"logPrintTask");//获取一个日志记录器
            //注册打印机任务状态事件
            _ea.GetEvent<UpdatePrintProgressEvent>().Subscribe(jobProgress =>
            {
                mesTaskServer.UpdatePrintProgress(jobProgress);
            });
            _ea.GetEvent<UpdatePrintJobEvent>().Subscribe(item =>
            {
                mesTaskServer.UpdatePrintJob(item);
            });
                
        }
        

        public bool InitCheck()
        {
            //确认是否满足条件
            //printtask config读入默认配方
            if (LiteDBHelper.PrintTaskConfigDB.GetConfig(
                ClassValue.DefalutPrintTaskConfigName, ClassValue.DefalutPrintTaskConfigVersion,
                ref _printTask._config))
            {
                _printTask.configName = ClassValue.DefalutPrintTaskConfigName;
                _printTask.configVersion = ClassValue.DefalutPrintTaskConfigVersion;
                MessageInfoLogUpdate_Thread($"参数{_printTask.configName}[v{_printTask.configVersion}]获取成功");
            }
            else
            {
                new Task(() => DialogHelper.ShowMessageDialog("提示", $"未找到默认参数！")).Start();
                //退出连续打印
                return false;
            }
            //尝试初始化
            Task<InitResult> printerInitwTask = new Task<InitResult>(() => _printTask.PrintTaskInit());
            printerInitwTask.Start();
            MessageInfoLogUpdate_Thread($"开始尝试初始化...");
            //提示树脂补液料位
            //MES确认：
            //请求测试
            //人员信息请求测试
            //人员登录时间校验
            //登录存入数据库

            if (!_isMES)
            {
                localDataServer = new LocalDataServer();
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    localDataServer.InitLocalPrint();
                }));
                
                //待开发：
                //提示本地打印初始化情况
                //询问继续打印或全部重新打印
            }
            else
            {
                mesTaskServer.Init();
                if (!GetIDByInputAndCard())
                {
                    return false ;
                }

                if (ClassValue.userinfo.userRole != UserRole.mesUser) { 
                    new Task(() => DialogHelper.ShowMessageDialog("提示", $"非MES登录，无法启动MES打印！")).Start();
                    return false; }
               
                var sublotRes = mesTaskServer.GetSublots(ClassValue.MESConfig.SublotId);
                if (!sublotRes.Item1) { 
                    new Task(() => DialogHelper.ShowMessageDialog("提示", $"物料编号校验失败:{sublotRes.Item2}！")).Start();
                    return false; }          
            }
            printerInitwTask.Wait();
            initResult = printerInitwTask.Result;
            if (!initResult.isSuccess)
            {
                new Task(() => DialogHelper.ShowMessageDialog("错误", $"打印机初始化失败！打印退出!")).Start();
                return false;
            }
            Thread.Sleep(4000);
            return true;
        }
       
        private bool GetIDByInputAndCard()
        {
            Task taskCard = Task.Factory.StartNew(CheckIdCard);
            Task < Tuple<bool,string>> taskInput = Task<Tuple<bool, string>>.Factory.StartNew(() => DialogHelper.ShowInputDialog("请扫员工条形码或刷卡"));

            while (!taskCard.IsCompleted && !taskInput.IsCompleted)
            {
                Thread.Sleep(200);
            }
            if (taskInput.IsCompleted)
            {
                getByInput = true;
                taskCard.Wait();
                if (taskInput.Result == null) {
                    MessageInfoLogUpdate_Thread($"取消输入");
                    return false;
                }
                if (!mesTaskServer.CheckCardId(taskInput.Result.Item2))
                {
                    MessageInfoLogUpdate_Thread($"人员条形码{taskInput.Result}信息错误");
                    return false;
                }
                mesTaskServer.userId = taskInput.Result.Item2;
            }
            else
            {
                DialogHelper.AskCloseInput = true;
            }
            MessageInfoLogUpdate_Thread($"人员信息{mesTaskServer.userId }校验成功");
            return true;
        }
        private void MessageInfoLogUpdate_Thread(string info, LogLevel logLevel = LogLevel.Info)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    ptLog.Info(info);
                    break;
                case LogLevel.Debug:
                    ptLog.Debug(info);
                    break;
                case LogLevel.Warn:
                    ptLog.Warn(info);
                    break;
                case LogLevel.Error:
                    ptLog.Error(info);
                    break;
                case LogLevel.Fatal:
                    ptLog.Fatal(info);
                    break;
            }
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _ea.GetEvent<MessageEvent>().Publish(info);
            }));
        }
        private bool _useWindow = true;
        private InitResult initResult = null;
        [Obsolete]
        public void StartPrinting(bool ismes)
        {
            _isMES = ismes;
            Task t1 = new Task(() =>
            {
                if (InitCheck())
                {
                    //本地，打完即终止
                    //本地，MES如何手动结束？
                    //接受消息，如果未在打印，则立即退出打印模式，若正在打印，则
                    while (true)
                    {
                        Task<bool> closeWindowTask = new Task<bool>(() => _pmc.CloseWindow());
                        if (_useWindow) { closeWindowTask.Start(); }
                        Task<InitResult> printerInitwTask = new Task<InitResult>(() => _printTask.PrintTaskInit());

                        _printTask.Init();
                        if (_isMES) { _printTask.printMode = PrintMode.MES; }
                        else
                        {
                            _printTask.printMode = PrintMode.Continual;
                        }
                        MessageInfoLogUpdate_Thread($"打印任务初始化...");
                        printerInitwTask.Start();

                        LiteDBHelper.PrintTaskConfigDB.GetConfig(
                        ClassValue.DefalutPrintTaskConfigName, ClassValue.DefalutPrintTaskConfigVersion,
                        ref _printTask._config);
                        MessageInfoLogUpdate_Thread($"参数{_printTask.configName}[v{_printTask.configVersion}]获取成功");
                        if (!GetTask(_isMES))
                        {
                            MessageInfoLogUpdate_Thread($"MES打印任务获取失败");
                            break;
                        }
                        if (_isMES)
                        {
                            MessageInfoLogUpdate_Thread($"MES打印任务{mesTaskServer.schedulesData.scheduleId}获取成功");
                        }
                        

                        MessageInfoLogUpdate_Thread($"上传模型...");
                        var uploadRes = _printTask.UploadFiles(false);
                        if (!uploadRes.Item1)
                        {
                            if (_isMES) { mesTaskServer.LayoutError($"{uploadRes.Item2}"); }
                            new Task(() => DialogHelper.ShowMessageDialog("错误", $"模型上传失败{uploadRes.Item2}!")).Start();
                            break;
                        }
                        if (_printTask.AskedStop)
                        {
                            if (_isMES)
                            {
                                mesTaskServer.LayoutError("手工退出");
                            }
                            new Task(() => DialogHelper.ShowMessageDialog("错误", $"手工退出!")).Start();
                            break;
                        }
                        Task sliceTask = new Task(() =>
                        {
                            Thread.Sleep(2000);
                            _printTask.StartSlice();
                        });
                        MessageInfoLogUpdate_Thread($"开始模型切层...");
                        sliceTask.Start();
                        MessageInfoLogUpdate_Thread($"开始自动排版...");
                        var unprints = _printTask.AutoLayout_LXH_New();
                        printerInitwTask.Wait();
                        MessageInfoLogUpdate_Thread($"打印任务初始化完成!");
                        initResult = printerInitwTask.Result;
                        sliceTask.Wait();
                        if (!initResult.isSuccess)
                        {
                            if (_isMES)
                            {
                                mesTaskServer.LayoutError($"{uploadRes.Item2}");

                            }
                            new Task(() => DialogHelper.ShowMessageDialog("错误", $"打印机初始化失败！打印退出!")).Start();
                            break;
                        }
                        if (_printTask.AskedStop)
                        {
                            if (_isMES)
                            {
                                mesTaskServer.LayoutError("手工退出");
                            }
                            new Task(() => DialogHelper.ShowMessageDialog("错误", $"手工退出!")).Start();
                            break;
                        }

                        if (!LockLayout(_isMES)) { break; }
                        //打印
                        closeWindowTask.Wait();
                        if (!closeWindowTask.Result) { _useWindow = false; }

                        //网板信息校-更新任务信息
                        if (_isMES)
                        {
                            //网板信息校-更新任务信息
                            if (mesTaskServer.AskContainer(_printTask.plateNum))
                            {
                                //网板信息校验成功
                                MessageInfoLogUpdate_Thread($"网板{_printTask.plateNum}信息校验成功");
                            }
                            else
                            {
                                //网板信息校验失败
                                //提示更换新网版
                                MessageInfoLogUpdate_Thread($"网板{_printTask.plateNum}信息校验失败");
                                mesTaskServer.LayoutError($"网板{_printTask.plateNum}信息校验失败");
                                new Task(() => DialogHelper.ShowMessageDialog("提示", $"网板信息校验失败！MES打印退出!")).Start();
                                break;
                            }

                            if (!mesTaskServer.CreatJobMessage(_printTask.partModels.ToList()))
                            {
                                MessageInfoLogUpdate_Thread($"post job {mesTaskServer.jobId} 失败");
                                new Task(() => DialogHelper.ShowMessageDialog("错误", $"post job{mesTaskServer.jobId} 失败")).Start();
                                mesTaskServer.LayoutError("post job 失败");
                                break;
                            }

                        }
                        if (_printTask.AskedStop)
                        {
                            if (_isMES)
                            {
                                mesTaskServer.LayoutError("手工退出");
                            }
                            new Task(() => DialogHelper.ShowMessageDialog("错误", $"手工退出!")).Start();
                            break;
                        }
                        try
                        {
                            _printTask.Print();
                        }
                        catch (Exception e)
                        {
                            _ea.GetEvent<MessageEvent>().Publish(e.ToString());
                            if (_isMES)
                            {
                                mesTaskServer.LayoutError($"打印任务异常退出{e.ToString()}");
                                mesTaskServer.PrintError($"打印任务异常退出{e.ToString()}");
                            }
                            new Task(() => DialogHelper.ShowMessageDialog("提示", $"打印任务异常退出{e.ToString()}！MES打印退出!")).Start();
                            break;
                        }

                        if (_printTask.AskedStop)
                        {
                            if (_isMES)
                            {
                                mesTaskServer.LayoutError("人工取消");
                                mesTaskServer.PrintError("人工取消");
                            }
                            new Task(() => DialogHelper.ShowMessageDialog("错误", $"人工取消！MES打印退出!")).Start();
                            break;
                        }
                        //开门
                        if (_useWindow)
                        {
                            if (!_pmc.OpenWindow())
                            {
                                //停止打印门的使用
                                _useWindow = false;
                            }
                        }

                        if (!FinishPrint(_isMES))
                        {
                            break;
                        }
                        if (!ClassValue.EnableSimulate)
                        {
                            var checktask = new Task<bool>(() => DialogHelper.ShowYONDialog("提示", $"脚踏开关开始打印或按按否取消继续打印？", DialogHelper.DialogType.No));
                            checktask.Start();
                            //等待收料完成指令
                            while (_pmc.GetIO(DIEnum.FootSwitchRelease) == 1 && !checktask.IsCompleted)
                            {
                                Thread.Sleep(50);
                            }
                            if (checktask.IsCompleted)
                            {
                                MessageInfoLogUpdate_Thread("踩下脚踏，继续打印。");
                            }
                            else
                            {
                                MessageInfoLogUpdate_Thread("连续打印被人工终止！");
                                DialogHelper.AskCheckInput = true;
                                checktask.Wait();
                                break;
                            }
                        }
                        
                    }

                    _printTask.Init();
                }
            });
            t1.Start();
            
        }
        public bool GetTask(bool isMes)
        {
            if (isMes)
            {
                mesTaskServer.Init();
                return GetMesTask();
            }
            else
            {
                return GetLocalTask();
            }

        }
        public bool GetMesTask() 
        {
            var getNewTaskRes = mesTaskServer.GetNewTask(this._printTask._config);
            MessageInfoLogUpdate_Thread(getNewTaskRes.Item2);
            if (getNewTaskRes.Item1)
            {
                //输出获取新任务
            }
            else
            {
                //获取任务失败
                new Task(() => DialogHelper.ShowMessageDialog("警告", getNewTaskRes.Item2)).Start();
                return false;
            }

            var sm = mesTaskServer.schedulesData.desc.FirstOrDefault(p => p.isStandard = true);

            foreach (ScheduleDescItem mesLayoutItem in mesTaskServer.schedulesData.desc)
            {
                if (mesLayoutItem.isStandard) { continue; }
                _printTask.files_Selected.Add(new PrintTask.FileSelected()
                {
                    modelId = mesLayoutItem.modelId,
                    stl = mesLayoutItem.stl,
                    isStandard = mesLayoutItem.isStandard,
                    standardPosition = mesLayoutItem.standardPosition,
                    pretreatmentVersion = mesLayoutItem.pretreatmentVersion
                });
            }
            int randMax = Math.Min(40, _printTask.files_Selected.Count);
            Random rd = new Random();
            int insertInd = rd.Next(0, randMax);
            //if (sm != null)
            //{
            //    _printTask.files_Selected.Insert(insertInd, new PrintTask.FileSelected()
            //    {
            //        modelId = sm.modelId,
            //        stl = sm.stl,
            //        isStandard = sm.isStandard,
            //        standardPosition = sm.standardPosition,
            //        pretreatmentVersion = sm.pretreatmentVersion
            //    });
            //    _printTask.hasStandmodel = true;
            //}
             
            return true;
        }

        public bool GetLocalTask()
        {
            //尝试获取本地打印数据
            if (!localDataServer.GetNewTask(ref currTaskItems))
            {
                //弹窗打印完成
                new Task(() => DialogHelper.ShowMessageDialog("提示", $"本地连续打印完成！")).Start();
                //退出连续打印
                return false;
            }
            foreach (var item in currTaskItems)
            {
                _printTask.files_Selected.Add(new PrintTask.FileSelected() { stl = item.path });
            }

            return true;
        }

        public bool LockLayout(bool isMes)
        {
            if (isMes)
            {
                return LockMesLayout();
            }
            else
            {
                return LockLocalLayout();
            }

        }
        public bool LockMesLayout()
        {
            var lockLayoutRes = mesTaskServer.LockLayout(_printTask.partModels.ToList(), new List<PartModel>());
            MessageInfoLogUpdate_Thread(lockLayoutRes.Item2);
            if (lockLayoutRes.Item1)
            {
                //锁定排版成功
                _printTask.isMES = true;
                _printTask.mesTaskId = mesTaskServer.printData.taskId;
            }
            else
            {
                //锁定排版失败
                //尝试取消排版
                mesTaskServer.LayoutError("锁定排版失败");
                new Task(() => DialogHelper.ShowMessageDialog("警告", lockLayoutRes.Item2)).Start();
                return false;
            }
            return true;
        }

        public bool LockLocalLayout()
        {
            foreach (var item in currTaskItems)
            {
                item.itemstatus = ItemStatus.printing;
            }
            foreach (var part in _printTask.partModels)
            {
                if (part.IsToPrint)
                {
                    currTaskItems[part.index].itemstatus = ItemStatus.inited;
                }
            }
            localDataServer.Update(currTaskItems);

            return true;
        }

        public bool FinishPrint(bool isMes)
        {
            //等待新网板装入
            if (isMes)
            {
                while (!GetIDByInputAndCard())
                {
                    MessageInfoLogUpdate_Thread("人员校验失败，请重试");
                    Thread.Sleep(300);
                }
                MessageInfoLogUpdate_Thread($"{mesTaskServer.jobId}上传生产记录...");
                mesTaskServer.PostPrintLog(_printTask, false);
                MessageInfoLogUpdate_Thread($"{_printTask.plateNum}绑定容器...");
                mesTaskServer.PutContainer(_printTask.partModels.ToList());
                MessageInfoLogUpdate_Thread($"Pcloud提示收料...");
                _ea.GetEvent<UpDatePrinterAlertEvent>().Publish(new Tuple<PrinterAlerts, bool>(PrinterAlerts.replenishment, true));
            }
            else
            {
            }
            
            if (!ClassValue.EnableSimulate) 
            {
                var checktask = new Task<bool>(() => DialogHelper.ShowYONDialog("请更换网板", $"请更换网板？", DialogHelper.DialogType.None));
                checktask.Start();
                //等待换板完成检测完成指令
                CheckNewPlate();
                DialogHelper.AskCheckInput = true;
                checktask.Wait();
            }
            
            

            if (!isMes)
            {
                foreach (var item in currTaskItems)
                {
                    if (item.itemstatus == ItemStatus.printing)
                    {
                        item.itemstatus = ItemStatus.printed;
                    }
                }
                localDataServer.Update(currTaskItems);
                currTaskItems = new List<LocalDataItem>();
            }

            return true;

        }
        static bool getByInput = false;
        private bool CheckIdCard()
        {
            getByInput = false;
            _pmc.ResetIdCardNum();
            string idCardNum = null;
            int  tryx = 0;
            while (idCardNum == null&& !getByInput)
            {
                idCardNum = _pmc.GetIdCardNum();
                if (idCardNum != null)
                {
                    if (mesTaskServer.CheckCardId(idCardNum))
                    {
                        break;
                    }
                    else
                    {
                        idCardNum = null;
                    }
                }
            }
            //确认idcard是否有效
            return true;
        }
        private bool CheckNewPlate()
        {      
            while (true)
            {
                var rRes = _pmc.RfidRead(RFIDEnum.PlateRFID);
                if (rRes.Item1)
                {
                    string plateId = rRes.Item2.Split(',')[0];
                    if (plateId.Length == 8)
                    {
                        if (plateId.Substring(0, 5) == "3D-P-")
                        {
                            if (plateId != _printTask.plateNum)
                            {
                                if (_isMES)
                                {
                                    if (mesTaskServer.AskContainer(plateId))
                                    {
                                        _ea.GetEvent<ProgressDialogEvent>().Publish(new Tuple<int, string, string>(90, $"新网版{plateId}，踩下脚踏开始打印", "人员刷卡√ ->记录上传√ -> 网板更换√  ->脚踏开始    。"));
                                        Thread.Sleep(2000);
                                        break;
                                    }
                                    else
                                    {
                                        _ea.GetEvent<ProgressDialogEvent>().Publish(new Tuple<int, string, string>(50, $"新网版{plateId}校验失败", "人员刷卡√ ->记录上传√ -> 网板更换×  ->脚踏开始    。"));
                                        Thread.Sleep(2000);
                                        MessageInfoLogUpdate_Thread($"网板编号{plateId}校验失败！");
                                    }
                                }
                                else
                                {
                                    _ea.GetEvent<ProgressDialogEvent>().Publish(new Tuple<int, string, string>(90, $"新网版{plateId}，踩下脚踏开始打印", "网板更换√  ->脚踏开始    。"));
                                    break;
                                }
                                
                            }
                        }
                    }
                }
                Thread.Sleep(1000);
            }
            return true;
        }

    }

    public class LocalDataServer
    {
        public enum ItemStatus { inited, locked, printing, printed };

        public class LocalDataItem
        {
            public string name { get; set; }
            public string path { get; set; }

            public ItemStatus itemstatus { get; set; } = ItemStatus.inited;

            public string printJob { get; set; }
            public LocalDataItem()
            {

            }
            public LocalDataItem(string filepath)
            {
                name = filepath.Substring(filepath.LastIndexOf("\\") + 1, filepath.Length - filepath.LastIndexOf("\\") - 1);
                path = filepath;
            }
        }
        public List<LocalDataItem> localDataItems { get; set; } = new List<LocalDataItem>();

        public bool GetNewTask(ref List<LocalDataItem> newTaskItems)
        {
            var list = localDataItems.Where(p => p.itemstatus == ItemStatus.inited).ToList();
            foreach (var item in list)
            {
                item.itemstatus = ItemStatus.locked;
                newTaskItems.Add(item);
                if (newTaskItems.Count > 58)
                {
                    break;
                }
            }
            return true;
        }
        public string folderPath { get; set; }
        public string jsonPath { get; set; }

        public string jsonName { get; set; } = "LocalPrintStatus.json";
        public bool InitLocalPrint()
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return false;
            }

            DirectoryInfo directory = new DirectoryInfo(m_Dialog.SelectedPath);
            folderPath = m_Dialog.SelectedPath;
            jsonPath = $"{folderPath}\\{jsonName}";
            if (File.Exists(jsonPath))
            {
                using (FileStream fs = new FileStream(jsonPath, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string jsonstr = sr.ReadToEnd();
                        localDataItems = JsonConvert.DeserializeObject<List<LocalDataItem>>(jsonstr);
                    }
                }
                return true;
            }
            else
            {
                List<FileInfo> files = directory.GetFiles().ToList().Where(p => p.Name.Contains(".stl") || p.Name.Contains(".amf") || p.Name.Contains(".obj")).ToList();
                foreach (var fi in files)
                {
                    localDataItems.Add(new LocalDataItem(fi.FullName));
                }
                if (localDataItems.Count > 0)
                {
                    UpdateLocalData();
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public void ResetTask()
        {
            foreach (var item in localDataItems)
            {
                item.itemstatus = (int)ItemStatus.inited;
            }
            UpdateLocalData();
        }
        public void UpdateLocalData()
        {
            using (FileStream fs = new FileStream(jsonPath, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(JsonConvert.SerializeObject(localDataItems));

                }
            }

        }
        public void Update(List<LocalDataItem> newTaskItems)
        {
            foreach (var item in newTaskItems)
            {
                var itemUpdate = localDataItems.FirstOrDefault(p => p.name == item.name);
                itemUpdate.itemstatus = item.itemstatus;
            }
            UpdateLocalData();
        }
    }

    public class MESTaskServer : INotifyPropertyChanged
    {
        IServer mesServer = new MesServer();
        public Re_PrintSchedulesDto schedulesData;
        public Re_AddGroupDto groupData;
        public Re_PrintTaskDto printData;
        public int jobId;
        public string releaseBatchId  ;
        public string ReleaseBatchId
        {
            get { return releaseBatchId; }
            set {
                releaseBatchId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReleaseBatchId"));
            }

        }
        public string userId;

        public event PropertyChangedEventHandler PropertyChanged;

        public MESTaskServer()
        {
            
        }

        public void Init()
        {
            schedulesData = null;
            groupData = null;
            printData = null;
            jobId = 0;
            ReleaseBatchId = null; 
        }

        public Tuple<bool, string> GetSublots(string sublotId)
        {
            #region 树脂信息获取
            //树脂信息获取
            var sublotsData = mesServer.GetSublots(DataService.Models.ConfigModel.DeviceId, sublotId);
            #region 返回示例
            //{"category":null,"createtime":null,"lotId":"KD220522001","createBy":null,"quantity":500,"sublotId":"JDSCLL00013777|WL-B-52|2315478","expireDate":"2023-04-21","storageLocation":null,"remainingQuantity":500,"description":[{"value":"2","key":"resinCraft"},{"value":"R2","key":"resinType"}]}
            #endregion
            #endregion
            if (sublotsData.Success)
            {
                //ConfigModel.SublotId = sublotsData.Data.sublotId;
                return new Tuple<bool, string>(true, MessageAssemb($"Get Sublots 【{sublotId}】 Success。"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"Get Sublots 【{sublotId}】 Fail:{sublotsData.RawText}。"));
            }
        }


        public Tuple<bool, string> GetNewTask(PrintTaskConfig config)
        {
            //请求打印数据
            //{"printerId":"EA-P-0251","desc":[{"key":"tags","value":"第一产线,正常,MCP0.76"},{"key":"maxLimit","value":"56"},{"key":"isManual","value":"false"},{"key":"layerHeight","value":"0.12"},{"key":"shrinkCompX","value":"0.99800"},{"key":"shrinkCompY","value":"1.00700"},{"key":"inflate","value":"0.0"},{"key":"inflateX","value":"0.0"},{"key":"inflateY","value":"1.0"}]}
            Rt_PrintSchedulesDto rt_PrintSchedules = new Rt_PrintSchedulesDto()
            {
                printerId = DataService.Models.ConfigModel.DeviceId,
                desc = new List<DescriptionItem>() { 
                                    //打印机配置信息    
                                    //标签-tags，value的取值：正常,返工,加急,手工线,正常,异常处理。以英文逗号分隔表示。比如：返工,加急。 矫治器的膜片厚度-thickness，value的取值：0.76,0.625,0.4。以英文逗号分隔表示。只有手工压膜手工切割时，无需传厚度；其余情况都需要传厚度。
                                    new DescriptionItem(){ key="tags",value= ClassValue.MESConfig.tags},//需要更新tag
                                    //最大的排版数
                                    new DescriptionItem(){ key="maxLimit",value= ClassValue.MESConfig.maxLimit},//需要更新最大排版数量
                                    //是否手动
                                    new DescriptionItem(){ key="isManual",value= ClassValue.isManual},// 需要更新是否手工模型
                                    //层厚
                                    new DescriptionItem(){ key="layerHeight",value=config.printConfig.layerThickness.ToString("0.00")},
                                    //X补偿系数
                                    new DescriptionItem(){ key="shrinkCompX",value=config.accuracyParas.scaleCompX.ToString("0.000")},
                                    //Y补偿系数
                                    new DescriptionItem(){ key="shrinkCompY",value=config.accuracyParas.scaleCompY.ToString("0.000")},
                                    //像素额外补偿
                                    new DescriptionItem(){ key="inflate",value=config.accuracyParas.contourComp.ToString("0.00")},
                                    //像素X补偿
                                    new DescriptionItem(){ key="inflateX",value=config.accuracyParas.contourCompX.ToString("0.00")},
                                    //像素Y补偿
                                    new DescriptionItem(){ key="inflateY",value=config.accuracyParas.contourCompY.ToString("0.00")},
                                    }
            };
            var printData = mesServer.PostPrintSchedules(rt_PrintSchedules);
            // {"printerId":"EA-P-0251","desc":[{"key":"tags","value":"第一产线,正常,MCP0.76"},
            // {"key":"maxLimit","value":"56"},{"key":"isManual","value":"false"},{"key":"layerHeight","value":"0.12"},
            // {"key":"shrinkCompX","value":"0.99800"},{"key":"shrinkCompY","value":"1.00700"},{"key":"inflate","value":"0.0"},
            // {"key":"inflateX","value":"0.0"},{"key":"inflateY","value":"1.0"}]}
            // { "scheduleStrategy":"auto","scheduleId":634012,"releaseBatchId":"HS-XQ-1-20220530-01",
            // "desc":[{ "standardPosition":"5","pretreatmentVersion":2,"modelId":"PC081898",
            // "stl":"\\\\172.22.0.148\\stl\\标准件\\c\\标准件_PC081898PA.stl","isStandard":true},
            // { "standardPosition":null,"pretreatmentVersion":2,"modelId":"6XHJ647X",
            // "stl":"\\\\172.22.0.148\\stl\\模型处理v2\\设计\\C01006174659\\中期阶段1\\3D设计\\DC01006174659Z101\\U_16_6XHJ647X1P.stl",
            // "isStandard":false}]}
            //using (FileStream fs = new FileStream("printData.json", FileMode.OpenOrCreate))
            //{
            //    using (StreamWriter sw = new StreamWriter(fs))
            //    {
            //        sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(printData.Data));
            //    }
            //} 
            if (printData.Success)
            {
                schedulesData = printData.Data;
                ReleaseBatchId = schedulesData.releaseBatchId;
                return new Tuple<bool, string>(true, MessageAssemb($"Get new task {schedulesData.scheduleId} success!"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"Get new task fail:{printData.RawText}"));
            }
        }

        public Tuple<bool, string> LockLayout(List<PartModel> partmodels, List<PartModel> badPartmodels)
        {
            var pModels = new List<ModelsItem>();
            var bModels = new List<BadModelsItem>();
            foreach (var model in partmodels)
            {
                if (model.IsToPrint)
                {
                    pModels.Add(new ModelsItem()
                    {
                        modelId = model.modelId,
                        position = $"1 0 0 0 0 1 0 0 0 0 1 0 {model.TX} {model.TY} {model.TZ} 1"
                    });
                }
            }

            //锁定排版
            #region 请求数据示例
            //{"printerId":"EA-P-0251","scheduleId":"630827","models":[{"modelId":"6XFKK6FX","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -112.9346 -45.07595 0 1"},{"modelId":"6XFKK74P","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -44.40191 115.95 0 1"},{"modelId":"6XFKK680","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -112.9454 -88.11535 0 1"},{"modelId":"6XFKK70Y","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -44.40191 36.55115 0 1"},{"modelId":"6XFKK6PK","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -44.40192 -2.44025 0 1"},{"modelId":"6XFKJ91A","position":"1 0 0 0 0 -1 0 0 0 0 1 0 232.0581 -135.2252 0 1"},{"modelId":"6XFKJ98P","position":"1 0 0 0 0 -1 0 0 0 0 1 0 162.3681 -94.9752 0 1"},{"modelId":"6XFKJ94Y","position":"1 0 0 0 0 -1 0 0 0 0 1 0 91.75809 102.3648 0 1"},{"modelId":"6XFKJ9E5","position":"1 0 0 0 0 -1 0 0 0 0 1 0 162.1381 -57.4852 0 1"},{"modelId":"6XFKJ901","position":"1 0 0 0 0 -1 0 0 0 0 1 0 231.8281 -94.96 0 1"},{"modelId":"6XFKJ977","position":"1 0 0 0 0 -1 0 0 0 0 1 0 161.9081 -19.9952 0 1"},{"modelId":"6XFKJ939","position":"1 0 0 0 0 -1 0 0 0 0 1 0 162.5981 -135.2252 0 1"},{"modelId":"6XFKJ9AK","position":"1 0 0 0 0 -1 0 0 0 0 1 0 161.6781 17.4948 0 1"},{"modelId":"6XFKJ8YP","position":"1 0 0 0 0 -1 0 0 0 0 1 0 231.3681 -14.46 0 1"},{"modelId":"6XFKJ96X","position":"1 0 0 0 0 -1 0 0 0 0 1 0 92.21809 22.0948 0 1"},{"modelId":"6XFKJ920","position":"1 0 0 0 0 -1 0 0 0 0 1 0 161.2181 92.47481 0 1"},{"modelId":"6XFKJ996","position":"1 0 0 0 0 -1 0 0 0 0 1 0 161.4481 54.9848 0 1"},{"modelId":"6XFKJ8X7","position":"1 0 0 0 0 -1 0 0 0 0 1 0 231.5981 -54.70655 0 1"},{"modelId":"6XFKJ958","position":"1 0 0 0 0 -1 0 0 0 0 1 0 91.98808 62.11481 0 1"},{"modelId":"6XFKYJ61","position":"1 0 0 0 0 -1 0 0 0 0 1 0 22.98809 -18.37 0 1"},{"modelId":"6XFKYH7K","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -252.9021 -135.21 0 1"},{"modelId":"6XFKYHJ2","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -252.8831 3.25 0 1"},{"modelId":"6XFKYJ23","position":"1 0 0 0 0 -1 0 0 0 0 1 0 23.44809 -97.0452 0 1"},{"modelId":"6XFKYH3X","position":"1 0 0 0 0 -1 0 0 0 0 1 0 22.75809 22.11 0 1"},{"modelId":"6XFKYHA4","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -183.2059 -89.2252 0 1"},{"modelId":"6XFKYHXA","position":"1 0 0 0 0 -1 0 0 0 0 1 0 92.90808 -94.73 0 1"},{"modelId":"6XFKYJ5E","position":"1 0 0 0 0 -1 0 0 0 0 1 0 93.13808 -135.21 0 1"},{"modelId":"6XFKYH66","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -183.2152 49.02 0 1"},{"modelId":"6XFKYHHF","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -183.2028 -135.2252 0 1"},{"modelId":"6XFKYJ1H","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -44.40191 77.77 0 1"},{"modelId":"6XFKYH28","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -252.882 -89.21 0 1"},{"modelId":"6XFKYH9J","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -183.207 -43.2252 0 1"},{"modelId":"6XFKYHP1","position":"1 0 0 0 0 -1 0 0 0 0 1 0 92.44808 -16.07 0 1"},{"modelId":"6XFKYJ42","position":"1 0 0 0 0 -1 0 0 0 0 1 0 22.29809 103.07 0 1"},{"modelId":"6XFKYH5P","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -183.2122 95.02576 0 1"},{"modelId":"6XFKYHF3","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -252.894 49.2348 0 1"},{"modelId":"6XFKYJ04","position":"1 0 0 0 0 -1 0 0 0 0 1 0 23.67809 -135.21 0 1"},{"modelId":"6XFKYJ7A","position":"1 0 0 0 0 -1 0 0 0 0 1 0 22.52809 62.59 0 1"},{"modelId":"6XFKYH85","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -183.2169 2.7748 0 1"},{"modelId":"6XFKYHKE","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -252.882 -42.98 0 1"},{"modelId":"6XFKYJ3F","position":"1 0 0 0 0 -1 0 0 0 0 1 0 92.67809 -56.55 0 1"},{"modelId":"6XFKYH47","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -113.7601 -134.9217 0 1"},{"modelId":"6XFKYHEH","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -252.8951 95.46481 0 1"},{"modelId":"6XFKYHY0","position":"1 0 0 0 0 -1 0 0 0 0 1 0 23.21808 -56.55 0 1"},{"modelId":"6XFP0683","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -44.59256 -133.8484 0 1"},{"modelId":"6XFP06PY","position":"1 0 0 0 0 -1 0 0 0 0 1 0 231.1381 25.6187 0 1"},{"modelId":"6XFP06EE","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -113.1378 44.98925 0 1"},{"modelId":"6XFP0702","position":"1 0 0 0 0 -1 0 0 0 0 1 0 230.9081 65.6387 0 1"},{"modelId":"6XFP067H","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -44.59014 -89.89931 0 1"},{"modelId":"6XFP06J0","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -113.1461 -2.2627 0 1"},{"modelId":"6XFP06A2","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -113.1363 88.9565 0 1"},{"modelId":"6XFP06YX","position":"1 0 0 0 0 -1 0 0 0 0 1 0 230.6781 105.6587 0 1"},{"modelId":"6XFP0664","position":"1 0 0 0 0 -1 0 0 0 0 1 0 -44.58951 -45.7393 0 1"}],"badModels":[]}
            Rt_PrintTaskDto rt_PrintTask = new Rt_PrintTaskDto()
            {
                scheduleId = schedulesData.scheduleId,
                printerId = DataService.Models.ConfigModel.DeviceId,
                models = pModels,
                //异常牙模，日志都为空
                badModels = bModels
            };
            #endregion
            var taskData = mesServer.PostPrintTask(rt_PrintTask);

            if (taskData.Success)
            {
                printData = taskData.Data;
                return new Tuple<bool, string>(true, MessageAssemb($"{schedulesData.scheduleId} Lock Layout success!"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"{schedulesData.scheduleId} Lock Layout fail:{taskData.RawText}"));
            }
        }

        public bool PostPrintLog(PrintTask printTask,bool isRetry)
        {
        #region 上传记录
        //上传记录
        #region 请求数据示例
        //{"userId":"yinhaijun","deviceId":"EA-P-0251","productionType":"UVcuring","creationTime":"2022-05-28T05:23:17.323+08:00","description":[{"key":"printTaskId","value":"621866"},{"key":"beginTime","value":"2022-05-28T04:23:36+08:00"},{"key":"endTime","value":"2022-05-28T05:23:17+08:00"},{"key":"duration","value":"-25257"},{"key":"sublotId","value":"KDWL-B-522205221160"},{"key":"modelQuantity","value":"53"},{"key":"modelVolume","value":"363016.2"},{"key":"temperature","value":"-46.9"},{"key":"humidity","value":"-6.0"},{"key":"resinTemperature","value":"30.4"},{"key":"laserPower","value":"0.000"},{"key":"shrinkCompX","value":"0.99800"},{"key":"shrinkCompY","value":"1.00700"},{"key":"inflate","value":"0.0"},{"key":"inflateX","value":"0.0"},{"key":"inflateY","value":"1.0"},{"key":"retry","value":"FALSE"}]}
        Rt_ProductionRecordDto rt_ProductionRecord = new Rt_ProductionRecordDto()
            {
                userId = userId,
                creationTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                deviceId = DataService.Models.ConfigModel.DeviceId,
                productionType = "UVcuring",
                description = new List<DescriptionItem>()
                {
                    //taskId
                    new DescriptionItem(){ key="printTaskId",value= this.printData.taskId},
                    //打印开始时间
                    new DescriptionItem(){ key="beginTime",value=printTask.printStartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")},
                    //endTime
                    new DescriptionItem(){ key="endTime",value=printTask.printEndTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")},
                    //持续时间
                    new DescriptionItem(){ key="duration",value=printTask.printDuration.TotalMinutes.ToString("0.0")},
                    //树脂批次号
                    new DescriptionItem(){ key="sublotId",value=ClassValue.MESConfig.SublotId},
                    //牙模个数
                    new DescriptionItem(){ key="modelQuantity",value= printTask.layoutItems.Count.ToString()},
                    //牙模体积
                    new DescriptionItem(){ key="modelVolume",value=printTask.volumeCount.ToString("0.0")},
                    //打印机温度
                    new DescriptionItem(){ key="temperature",value=printTask.printLog[0].printerTemp.ToString("0.0")},
                    //湿度
                    new DescriptionItem(){ key="humidity",value=printTask.printLog[0].printerHumidity.ToString("0.0")},
                    //树脂温度
                    new DescriptionItem(){ key="resinTemperature",value=printTask.printLog[0].resinTemp.ToString("0.0")},
                    //光强
                    new DescriptionItem(){ key="laserPower",value="5000,4000" },
                    //X补偿系数
                    new DescriptionItem(){ key="shrinkCompX",value=printTask._config.accuracyParas.scaleCompX.ToString("0.0000")},
                    //Y补偿系数
                    new DescriptionItem(){ key="shrinkCompY",value=printTask._config.accuracyParas.scaleCompY.ToString("0.0000")},
                    //像素额外补偿
                    new DescriptionItem(){ key="inflate",value=printTask._config.accuracyParas.contourComp.ToString("0.0000")},
                    //像素X补偿
                    new DescriptionItem(){ key="inflateX",value=printTask._config.accuracyParas.contourCompX.ToString("0.0000")},
                    //像素Y补偿
                    new DescriptionItem(){ key="inflateY",value=printTask._config.accuracyParas.contourCompY.ToString("0.0000")},
                    //是否重发（false-上传多次，多条记录，true-上传多次，更新为最后一条记录）
                    new DescriptionItem(){ key="retry", value=isRetry.ToString().ToLower()}
                }
            };
            #endregion
            var recordData = mesServer.PostRecord(rt_ProductionRecord);
            #endregion
            if (recordData.Success)
            {
                return true;
            }
            else 
            { return false; }
        }

        public bool AskContainer(string cId)
        {
            #region 请求料框
            //请求料框
            #region 请求数据示例
            //{"groupCategory":"UVcuringGroup","itemType":"model","containerId":"WX1RPMC-07","deviceId":"EA-P-0251"}
            Rt_AddGroupDto re_AddGroup = new Rt_AddGroupDto()
            {
                //RFID号
                containerId = cId,
                deviceId = DataService.Models.ConfigModel.DeviceId,
                groupCategory = GroupCategory.UVcuringGroup,
                itemType = GroupItemType.model
            };
            #endregion
            var groupData = mesServer.PostGroup(re_AddGroup); 
            #endregion
            if (groupData.Success)
            {
                this.groupData = groupData.Data;
                return true;
            }
            else
            { return false; }
        }

        public bool CheckCardId(string userID)
        {
           
            var userData = mesServer.GetUserInfo(userID);
            if (userData.Success)
            {
                //调用成功
                //使用数据Data
                var userinfo = userData.Data;

                if (userinfo.groups.Contains("光固化"))
                {
                    userId = userinfo.userId;
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                //失败流程
                return false;
            }
        }

        public bool PutContainer(List<PartModel> models)
        {
            var putModels = new List<UpdateGroupDesc>();
            foreach (var model in models)
            {
                if (!model.IsToPrint) { continue; }
                putModels.Add(new UpdateGroupDesc()
                {
                    itemId = model.modelId
                });
            }
            #region 绑定料框
            //绑定料框
            #region 请求数据示例
            // [{"itemId":"6XHA469P"},{"itemId":"6XHA462A"},{"itemId":"6XH8PF58"},{"itemId":"6XH8PE63"},{"itemId":"6XH8PEH9"},{"itemId":"6XH8PF1A"},{"itemId":"6XH8PE9E"},{"itemId":"6XH8PEPX"},{"itemId":"6XH8PF4Y"},{"itemId":"6XH8PE5H"},{"itemId":"6XH8PEF0"},{"itemId":"6XH8PF01"},{"itemId":"6XH8PE82"},{"itemId":"6XH8PEK8"},{"itemId":"6XH8PF39"},{"itemId":"6XH8PEEA"},{"itemId":"6XH8PEYP"},{"itemId":"6XH8PF6X"},{"itemId":"6XH8PE7F"},{"itemId":"6XH8PEJY"},{"itemId":"6XH8PF20"},{"itemId":"6XH8PEA1"},{"itemId":"6XH8PEX7"},{"itemId":"6XH8JY2H"},{"itemId":"6XH8JY90"},{"itemId":"6XH8JXX1"},{"itemId":"6XH8JY52"},{"itemId":"6XH8JYF8"},{"itemId":"6XH8JXH3"},{"itemId":"6XH8JY14"},{"itemId":"6XH8JY8A"},{"itemId":"6XH8JXPE"},{"itemId":"6XH8JY4F"},{"itemId":"6XH8JYEY"},{"itemId":"6XH8JY0J"},{"itemId":"6XH8JY71"},{"itemId":"6XH8JYJ7"},{"itemId":"6XH8JXK2"},{"itemId":"6XH8JY33"},{"itemId":"6XH8JYA9"},{"itemId":"6XH8JXYA"},{"itemId":"6XH8JY6E"},{"itemId":"6XH8JYHX"},{"itemId":"6XH8JXJF"},{"itemId":"6XH8J826"},{"itemId":"6XH8J89F"},{"itemId":"6XH8J8PY"},{"itemId":"6XH8J7XH"},{"itemId":"6XH8J85J"},{"itemId":"6XH8J8F1"},{"itemId":"6XH8J81P"},{"itemId":"6XH8J883"},{"itemId":"6XH8J8K9"},{"itemId":"6XH8J7P4"},{"itemId":"6XH8J845"}]
            Rt_UpdateGroupDto rt_UpdateGroup = new Rt_UpdateGroupDto()
            {
                deviceCode = DataService.Models.ConfigModel.DeviceId,
                groupId = groupData.groupId,
                updateGroupDesc = putModels
            };
            #endregion
            var putGroup = mesServer.PutGroup(rt_UpdateGroup);
            #endregion
            if (putGroup.Success)
            {
                return true;
            }
            else
            { return false; }
        }

        public Tuple<bool, string> LayoutError(string reason)
        {
            #region 排版失败调用
            //排版失败调用
            #region 请求数据示例
            //http://mes2-web.sw.eainc.com:8080/mes2/rest/printSchedules/629560?deviceCode=EA-P-0251&toManual=False&reason=用户取消。
            #endregion
            var deletePrint = mesServer.DeletePrintSchedules(schedulesData.scheduleId, DataService.Models.ConfigModel.DeviceId, false, reason);
            #endregion
            if (deletePrint.Success)
            {
                return new Tuple<bool, string>(true, MessageAssemb($"LayoutError:{reason},取消打印计划成功"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"LayoutError:{reason},取消打印计划失败:{deletePrint.RawText}"));
            }
        }
        private string MessageAssemb(string msg)
        {
            return $"MES->{ClassValue.MESConfig.DeviceId} {msg}";
        }
        public Tuple<bool,string> PrintError(string reason)
        {
            #region 取消打印（排版完成，锁定后使用）
            //取消打印（排版完成，锁定后使用）
            #region 请求数据示例
            //http://mes2-web.sw.eainc.com:8080/mes2/rest/printTasks/640670?deviceCode=EA-P-0251&toManual=False&reason=Start printing failed. 打印准备失败。%0D%0A复位平台失败！%0D%0A无法连接设备！请检查设备是否开机并正确连接，如果设备已死机请重启设备电源。
            #endregion
            var deleteTask = mesServer.DeletePrintTask(printData.taskId, DataService.Models.ConfigModel.DeviceId, false, reason);
            #endregion
            if (deleteTask.Success)
            {
                return new Tuple<bool, string>(true, MessageAssemb($"PrintError:{reason},取消打印任务成功"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"PrintError:{reason},取消打印任务失败:{deleteTask.RawText}")); 
            }
        }
        public bool CreatJobMessage( List<PartModel> models)
        {
            var jModels = new List<JobModels>();
            foreach (var model in models)
            {
                if (model.IsToPrint)
                {
                    jModels.Add(new JobModels()
                    {
                        name = model.fileName,
                        id = model.modelId,
                        filePath = model.filePath,
                        dimension = new Dimension() { x = Math.Round(model.DX,3), y = Math.Round(model.DY,3), z = Math.Round(model.DZ,3) },
                        instances = new List<InstancesItem>() { new InstancesItem() { matrix = $"1 0 0 0 0 1 0 0 0 0 1 0 {model.TX.ToString("0.00")} {model.TY.ToString("0.00")} {model.TZ.ToString("0.00")} 1" } }
                    }
                    );
                }
            }
            #region 创建打印作业(一个或多个)
            //创建打印作业(可含多个job，现场都为单job使用)
            #region 请求数据示例
            //{"printerId":105,"printerToken":"613fb118-cf54-4816-bbed-b3888e49cbb7","jobs":[{"taskId":"691192","startTime":"2022-07-01T10:11:21.3045837+08:00","scene":{"dimension":{"x":584.089539,"y":328.103851,"z":85.0},"models":[{"name":"标准件_PC091148PA.stl","id":"PC091148","filePath":"\\\\172.22.0.148\\stl\\标准件\\c\\标准件_PC091148PA.stl","dimension":{"x":67.8462,"y":51.4797,"z":14.0},"instances":[{"matrix":"1 0 0 0 0 -1 0 0 0 0 1 0 -252.31 -133.7272 0 1"}]},{"name":"U_1_701K1YK71P.stl","id":"701K1YK7","filePath":"\\\\172.22.0.148\\stl\\模型处理v2\\设计\\C01001852073\\中期阶段5\\3D设计\\DC01001852073Z501\\U_1_701K1YK71P.stl","dimension":{"x":65.9696045,"y":50.9696,"z":14.5159},"instances":[{"matrix":"1 0 0 0 0 -1 0 0 0 0 1 0 -115.9352 -2.7452 0 1"}]},{"name":"U_5_701Y3EFE1P.stl","id":"701Y3EFE","filePath":"\\\\172.22.0.148\\stl\\模型处理v2\\设计\\C01002429007\\中期阶段4\\3D设计\\DC01002429007Z403\\U_5_701Y3EFE1P.stl","dimension":{"x":65.9696045,"y":50.9392,"z":14.845},"instances":[{"matrix":"1 0 0 0 0 -1 0 0 0 0 1 0 18.6148 51.78 0 1"}]}]}}]}
            Rt_JobCreationPackageDto rt_JobCreationPackage = new Rt_JobCreationPackageDto()
            {
                printerId = DataService.Models.ConfigModel.P_PrintId,
                printerToken = DataService.Models.ConfigModel.P_PrintToken,
                jobs = new List<JobsItem>()
                {
                new JobsItem(){
                   taskId= printData.taskId,
                   startTime=DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                   scene=new Scene(){
                   dimension=new Dimension(){ x=432,y=484,z=100},//如何定义scene
                   models=jModels
                   }
                 }
                }
            };
            #endregion
            var jobData = mesServer.PostJob(rt_JobCreationPackage);
            #region 返回示例
            //[683924] JobId
            #endregion
            #endregion
            if (jobData.Success)
            {
                try
                {
                    List<int> jobsids = JsonConvert.DeserializeObject<List<int>>(jobData.Data.ToString());
                    jobId = jobsids[0];
                    return true;
                }
                catch
                {
                    return false;
                }
                
            }
            else
            { return false; }
        }

        public bool UpdatePrintProgress(JobProgress jobProgress)
        {
            #region 更新打印作业
            //更新打印作业(一层一次)
            #region 请求数据示例
            //{"printerId":105,"printerToken":"613fb118-cf54-4816-bbed-b3888e49cbb7","progress":{"totalLayers":129,"startLayers":0,"printedLayers":1,"totalHeight":15.3187494,"printedHeight":0.11875,"printedVolume":1031.073,"isPaused":false}}
            Rt_JobProgressPackageDto rt_JobProgressPackage = new Rt_JobProgressPackageDto()
            {
                printerId = DataService.Models.ConfigModel.P_PrintId,
                printerToken = DataService.Models.ConfigModel.P_PrintToken,
                progress = jobProgress
            };
            #endregion
            var updateJob = mesServer.PutJob(jobId, rt_JobProgressPackage);
            #endregion
            if (updateJob.Success)
            {
                return true;
            }
            else
            { return false; }
        }

        public bool UpdatePrintJob(JobEventItem item)
        {
            #region 提交打印作业事件(开始，暂停，结束调用)
            //提交打印作业事件
            //必须提交开始、结束事件完成job周期
            #region 请求数据示例
            item.jobId = jobId;
            // {"printerId":105,"printerToken":"613fb118-cf54-4816-bbed-b3888e49cbb7","events":[{"jobId":683924,"event":{"type":"start","time":"2022-07-01T10:11:21.9589489+08:00","progress":{"totalLayers":0,"startLayers":0,"printedLayers":0,"totalHeight":0.0,"printedHeight":0.0,"printedVolume":0.0,"isPaused":false},"completedInstanceCount":0,"hiddenInstances":[]}}]}
            Rt_JobEventPackageDto rt_JobEventPackage = new Rt_JobEventPackageDto()
            {
                printerId = DataService.Models.ConfigModel.P_PrintId,
                printerToken = DataService.Models.ConfigModel.P_PrintToken,
                events = new List<JobEventItem>() {
                    item
                }
            };
            #endregion
            var jobEvent = mesServer.PostJobEvent(rt_JobEventPackage);
            #endregion
            if (jobEvent.Success) { return true; }
            else
            { return false; }
        }

    }

}