using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using g3;
using gs;
using Newtonsoft.Json;
using Prism.Events;
using AngelDLP.PMC;
using static AngelDLP.ViewModels.PrintScene3DViewModel;
using static AngelDLP.ViewModels.MainWindowViewModel;
using System.Xml;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using OpenCvSharp;
using System.Windows.Media;
using System.Drawing.Imaging;
using static AngelDLP.Views.PrintScene3D;
using AngelDLP.ViewModels;
using AngleDLP.Models;
using static AngleDLP.Models.LogHelper;
using static AngelDLP.ViewModels.PrintTaskMonitorViewModel;
using AngleDLP.PPC;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using AngelModelPreprocess;
using Timer = System.Windows.Forms.Timer;
using static AngleDLP.Models.PlatformCalib;
using AngleDLP;
using ICSharpCode.SharpZipLib.Zip;
using static AngleDLP.PrintTaskHolder;
using DataService.Models.Dto.Pcloud;

namespace AngelDLP
{

    public class PrintTask
    {

        //打印前记录、保存
        public List<LayoutItem> layoutItems { set; get; } = new List<LayoutItem>();
        public BitmapSource layoutBitmap { set; get; }

        public PrintTaskConfig _config;

        public string configName { get; set; }
        public int configVersion { get; set; }
        public PrintMode printMode { get; set; }
        public bool _Avaliable { set; get; } = true;
        public string printTaskName { set; get; }

        public string plateNum { set; get; }

        //打印中记录、打印后保存，中断最好可以保存
        public List<PrintLog> printLog { set; get; } = new List<PrintLog>();


        //打印后保存
        public Guid printtask_GUID { set; get; }
        public DateTime printStartTime { set; get; }
        public List<FileSelected> files_Selected { set; get; } = new List<FileSelected>();

        public DateTime printEndTime { set; get; }

        public TimeSpan printDuration { set; get; }

        public ValueChanges valueChanges { set; get; } = new ValueChanges();
        public class FileSelected
        {
            public string stl { set; get; }
            public string modelId { set; get; }

            public string modelName { set; get; }
            public bool isStandard { set; get; }
            public string standardPosition { set; get; }
            public int pretreatmentVersion { set; get; } = 2;

            public bool isManual { get; set; } = false;
        }


        IEventAggregator _ea;
        public PrinterProjectionControl _ppc;
        PrinterMotionControl _pmc;
        public log4net.ILog ptLog;

        public bool isMES = false;

        public string mesTaskId = string.Empty;
        public List<SliceParas> _sliceParas { set; get; } = new List<SliceParas>();
        public int layoutCount;
        public int isMSS_Computed = 0;//计算切层数据
        public bool isSliceReady = false;//切层数据预处理
        public int printTaskStatus = 0;//0->初始化 // //1->打印中//2->打印完成// 准备中
        public bool isInited = false;//是否完成初始化





        public PlatformTestResult platformTestResult { get; set; } = new PlatformTestResult();
        public PlatformCalibResult platformCalibResult { get; set; } = new PlatformCalibResult();

        public double targetPlatePos { get; set; } = 0;
        public string currFileFolder;
        int maxcomp = 0;
        public double levelInitOffset = 0;
        public double resinTempInit = 0;
        public int maxslice = 0;
        public double volumeCount = 0;
        //PrinterMotionControl _pmc;
        //PrinterProjectionControl _ppc;
        public List<PrintTaskModels> taskModels = new List<PrintTaskModels>();

        public class PrintTaskEvent : PubSubEvent<string>
        {

        }

        public ObservableCollection<PartModel> partModels { set; get; } = new ObservableCollection<PartModel>();
        public PrintTask()
        { 
        }
        public bool AskedStop = false;
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
        public PrintTask(IEventAggregator ea, PrinterMotionControl printerMotionControl, PrinterProjectionControl printerProjectionControl, PartModelFactory partModelFactory)
        {
            _ea = ea;
            _pmc = printerMotionControl;
            _ppc = printerProjectionControl;
            partModels = partModelFactory.GetPartModels();
            ptLog = log4net.LogManager.GetLogger($"logPrintTask");//获取一个日志记录器
            _ea.GetEvent<PrintTaskEvent>().Subscribe(msg =>
            {
                //PrintTask 根据事件响应
                //msg: 
                //stop print 停止打印
                if(msg == "StopPrint")
                {
                    //打印完当前层后升起
                    AskedStop = true;
                    return;
                }

            });
            Init();
            
        }

        public void SavePrinterLogFile(string logFilePath)
        {
            List<Object[]> fls = new List<Object[]>();
            fls.Add(new Object[2] { "layout.bmp", StreamFromBitmapSource(layoutBitmap) });
            fls.Add(new Object[2] { "layout.json", new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(layoutItems, Newtonsoft.Json.Formatting.Indented))) });

            ByteToFile(CreateZipStream(fls), logFilePath);
        }
        public void StartSlice()//开始切层
        {
            //_ea.GetEvent<SaveViewPortEvent>().Publish(this);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            MessageInfoLogUpdate_Thread("开始切层", LogLevel.Info);
            //System.Threading.Tasks.Parallel.For(0, taskModels.Count, i =>
            //{//并行计算
            //    MeshPlanarSlicer mps = new MeshPlanarSlicer();
            //    mps.AddMesh(taskModels[i].mesh);
            //    //mps.DefaultOpenPathMode = PrintMeshOptions.OpenPathsModes.Clipped;
            //    mps.LayerHeightMM = this._config.layerThicness;
            //    //pbBar.Dispatcher.Invoke(new Action<System.Windows.DependencyProperty, object>(pbBar.SetValue), System.Windows.Threading.DispatcherPriority.Background, ProgressBar.ValueProperty, value);
            //    taskModels[i].pss = mps.Compute();
            //    taskModels[i].mps = mps;
            //});

            foreach (var item in taskModels)
            {
                MeshPlanarSlicer mps = new MeshPlanarSlicer();
                mps.AddMesh(item.mesh);
                mps.DefaultOpenPathMode = PrintMeshOptions.OpenPathsModes.Embedded; 
                //mps.DefaultOpenPathMode = PrintMeshOptions.OpenPathsModes.Clipped;
                //mps.OpenPathDefaultWidthMM = 10;
                mps.LayerHeightMM = this._config.printConfig.layerThickness;
                //pbBar.Dispatcher.Invoke(new Action<System.Windows.DependencyProperty, object>(pbBar.SetValue), System.Windows.Threading.DispatcherPriority.Background, ProgressBar.ValueProperty, value);
                item.pss = mps.Compute();
                item.mps = mps;
                //_ea.GetEvent<MessageEvent>().Publish($"第{item.index +1}个切层完成");
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    ModelsTipViewModel mpvm = new ModelsTipViewModel()
                    {
                        visibility = System.Windows.Visibility.Visible,
                        cColor = new SolidColorBrush(System.Windows.Media.Colors.Yellow),
                        fColor = new SolidColorBrush(System.Windows.Media.Colors.Yellow),
                        startAngle = 0,
                        currInd = item.index + 1,
                        endAngle = (int)(360 * (item.index + 1) / taskModels.Count),
                        arcThickness = 2
                    };
                    _ea.GetEvent<ModelsProgressViewEvent>().Publish(mpvm);
                }));

            };

            sw.Stop();
            isMSS_Computed = 2;
            MessageInfoLogUpdate_Thread($"切层完成，耗时{sw.Elapsed.TotalSeconds.ToString("0.0")}秒", LogLevel.Info);
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                ModelsTipViewModel mpvm = new ModelsTipViewModel()
                {
                    visibility = System.Windows.Visibility.Visible,
                    cColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                    fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                    startAngle = 0,
                    endAngle = 360,
                    currInd = taskModels.Count,
                    arcThickness = 2
                };
                _ea.GetEvent<ModelsProgressViewEvent>().Publish(mpvm);
            }));
        }
        public static string connectState(string path/*要访问的文件路径*/, string userName, string passWord)
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
                //登录验证
                string dosLine = @"net use " + path + " " + passWord + " /User:domain\\" + userName;
                proc.StandardInput.WriteLine("net use * /del /y");
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
                return ex.Message;
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag.ToString();
        }
        [Obsolete]
        public Tuple<bool, string> UploadFiles(bool isLayoutImport)
        {
            int ind = 0;
            int count = files_Selected.Count;
            double maxSliceHeight = 0;

            foreach (FileSelected fi in files_Selected)
            {
                //log.Info($"正在导入{ fi}");
                //支持格式 stl,amf(zip/amf)/obj
                //stl obj 直接传参支持
                //amf 转换成obj
                Stream s = new MemoryStream();
                Stream s1 = new MemoryStream();
                Stream s2 = new MemoryStream();
                Stream s3 = new MemoryStream();
                string formate = fi.stl.Substring(fi.stl.Length - 3, 3);
                bool isSTL = true;
                if (formate != "stl") { isSTL = false; }

                try
                {
                    using (FileStream fs = new FileStream(fi.stl, FileMode.Open))
                    {
                        byte[] bytes = new byte[fs.Length];
                        _ = fs.Read(bytes, 0, bytes.Length);
                        if (formate == "amf")
                        {
                            var zip = new ZipFile(new MemoryStream(bytes));
                            s = zip.GetInputStream(zip.GetEntry("data.amf"));
                        }
                        else
                        {
                            s = new MemoryStream(bytes);
                        }
                        ModelPreprocesser modelPreprocesser = new ModelPreprocesser();
                        if (_config.printConfig.isAlignerMold)
                        {
                            fi.isManual = modelPreprocesser.ModelProcess(s, fi.pretreatmentVersion, _config.accuracyParas.archCompPara, s2, formate);
                        }
                        else
                        {
                            s2 = new MemoryStream(bytes);
                        }
                        bytes = new byte[s2.Length];
                        s2.Position = 0;
                        s2.Read(bytes, 0, bytes.Length);
                        s1 = new MemoryStream(bytes);
                        s3 = new MemoryStream(bytes);
                    }
                    
                }
                catch (Exception e)
                {
                    return new Tuple<bool, string>(false, $"{fi.stl}导入失败");
                }
                fi.modelName = fi.stl.Substring(fi.stl.LastIndexOf("\\") + 1, fi.stl.Length - fi.stl.LastIndexOf("\\") - 1);
                if (fi.modelId == null)
                {
                    fi.modelId = fi.modelName;
                }
                taskModels.Add(AngelSlicer.NewPTM(ind, fi.stl, fi.modelName, s1, isSTL));
                PrintTaskModels tm = taskModels[taskModels.Count - 1];
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    //更新进度窗口
                    PartModel pm = new PartModel();
                    if (isLayoutImport)
                    {
                        pm = new PartModel(_ea, ind, fi, s3, layoutItems[ind].Matrix3D, fi.isManual, PhongMaterials.Bisque, isSTL);
                        pm.IsToPrint = true;
                    }
                    else
                    {
                        pm = new PartModel(_ea, ind, fi, s3, new System.Windows.Media.Media3D.Matrix3D(), fi.isManual, PhongMaterials.Bisque, isSTL);
                    }
                    partModels.Add(pm);
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {

                        _ea.GetEvent<PartSelectByListViewEvent>().Publish(ind);
                        _ea.GetEvent<UpdateTransform3DsEvent>().Publish(new UI2PartModel(ind, pm.ModelTransform));
                    }));
                    if (pm.MaxSliceHeight > maxSliceHeight)
                    {
                        maxSliceHeight = pm.MaxSliceHeight;
                    }
                    _ea.GetEvent<UploadProgressEvent>().Publish(new Tuple<int, int, double>(ind, count, maxSliceHeight));
                    ind++;
                }));
                s.Dispose();
                s2.Dispose();
                s3.Dispose();

            }
            _ea.GetEvent<UploadProgressEvent>().Publish(new Tuple<int, int, double>(-1, count, maxSliceHeight));
            return new Tuple<bool, string>(true, "模型导入成功");
            
        }
        public bool ApplyConfig(string jsonstr)
        {
            try
            {
                _config = PrintTaskConfig.DeserializeObject(jsonstr);
                return true;
            }
            catch (Exception e)
            {
                MessageInfoLogUpdate_Thread(e.ToString());
                return false;
            }

        }
        
        public bool Init()
        {
            if (isMSS_Computed == 1)
            {
                _ea.GetEvent<MessageEvent>().Publish("切层计算中，无法初始化");
                return false;
            }
            if (printTaskStatus == 1)
            {
                _ea.GetEvent<MessageEvent>().Publish("打印中，无法初始化");
                return false;
            }
            hasStandmodel = false;
            platformTestResult = null;
            platformCalibResult = null;
            targetPlatePos = 0.2;
            _config = null;
            printLog = new List<PrintLog>();
            _sliceParas = new List<SliceParas>();
            layoutCount = 0;
            isSliceReady = false;
            isMSS_Computed = 0;
            printTaskStatus = 0;
            isInited = false;
            currFileFolder = String.Empty;
            maxcomp = 0;
            levelInitOffset = 0;
            resinTempInit = 0;
            maxslice = 0;
            layoutItems.Clear();
            files_Selected = new List<FileSelected>();
            taskModels.Clear();
            plateNum = string.Empty;
            volumeCount = 0;
            configName = String.Empty;
            configVersion = -1;
            printtask_GUID = System.Guid.NewGuid();
            valueChanges = new ValueChanges();
            isMES = false;
            mesTaskId = string.Empty;
            AskedStop = false;
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                partModels.Clear();
                ModelsTipViewModel mpvm = new ModelsTipViewModel()
                {
                    visibility = System.Windows.Visibility.Visible,
                    cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                    fColor = new SolidColorBrush(System.Windows.Media.Colors.Gray),
                    startAngle = 0,
                    endAngle = 360,
                    currInd = 0,
                    arcThickness = 25
                };

                _ea.GetEvent<ModelsProgressViewEvent>().Publish(mpvm);
                _pmc.LightControl(0, 0, 0, 0, 0, true);
                _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
                {
                    printMode = PrintMode.Single,
                    progressBarVisibility = System.Windows.Visibility.Collapsed,
                    cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                    fColor = new SolidColorBrush(System.Windows.Media.Colors.Gray),
                    progress = 0,
                    layerTips = "未开始打印"
                });
                _ea.GetEvent<PartSelectByListViewEvent>().Publish(-1);
            }));
            return true;
        }
        public object[] SelectFolder()
        {
            files_Selected = new List<FileSelected>();

            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return new object[] { "未选择文件", false };
            }
            //判断是否包含layout 文件
            string folderPathSelected = m_Dialog.SelectedPath.Trim();
            string layoutPath = folderPathSelected + "\\layout.json";
            if (File.Exists(layoutPath))
            {
                List<LayoutItem> layoutItems_Temp;
                using (FileStream fs = new FileStream(layoutPath, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string jsonstr = sr.ReadToEnd();
                        layoutItems_Temp = JsonConvert.DeserializeObject<List<LayoutItem>>(jsonstr);
                    }
                }
                //确认是否所有文件都存在
                foreach (var li in layoutItems_Temp)
                {
                    if (!File.Exists($"{folderPathSelected}\\{li.Model}"))
                    {
                        return new object[] { $"排版文件不完全", false };
                    }
                }
                files_Selected.Add(new FileSelected() { stl = layoutPath });
                layoutItems = layoutItems_Temp;
                currFileFolder = folderPathSelected;
                return new object[] { $"已选择排版文件，包含{layoutItems.Count}个stl", true };
            }

            DirectoryInfo directory = new DirectoryInfo(folderPathSelected);
            List<FileInfo> files = directory.GetFiles().ToList().Where(p => p.Name.Contains(".stl")).ToList();
            foreach (var fi in files)
            {
                files_Selected.Add(new FileSelected() { stl = fi.FullName });
            }
            if (files_Selected.Count == 0)
            {
                return new object[] { $"未包含stl文件", false };
            }
            else
            {
                currFileFolder = folderPathSelected;
                return new object[] { $"已选择{files_Selected.Count()}个文件", true };
            }
        }
        public object[] SelectFiles()
        {
            files_Selected = new List<FileSelected>();
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Multiselect = true,
                Filter = "all 3d Files (*.stl,*.amf,*.obj)|*.stl;*.amf;*.obj|stereolithography (*.stl)|*.stl|obj文件 (*.obj)|*.obj"
            };
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                foreach (string s in openFileDialog.FileNames)
                {
                    files_Selected.Add(new FileSelected() { stl = s });
                }
            }
            else
            {
                return new object[] { "未选择文件", false };
            }
            currFileFolder = files_Selected[0].stl.Substring(0, files_Selected[0].stl.LastIndexOf("\\"));
            return new object[] { $"已选择{files_Selected.Count()}个文件", true };
        }
        BitmapImage bitmapToBitmapImage(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                BitmapImage reslut = new BitmapImage();
                reslut.BeginInit();
                reslut.CacheOption = BitmapCacheOption.OnLoad;
                reslut.StreamSource = stream;
                reslut.EndInit();
                reslut.Freeze();
                return reslut;
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
        public void GetPrintSliceAreas(int ind)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var res = GetSliceImage_OpenCV(ind);
            //ProjectionData.printImage0 = res.Item1[0];
            //ProjectionData.printImage0.Freeze();
            //ProjectionData.printImage1 = res.Item1[1];
            //ProjectionData.printImage1.Freeze();
            //ProjectionData.printPixCounts0 = res.Item2[0];
            //ProjectionData.printPixCounts1 = res.Item2[1];
            //ProjectionData.printPixCounts = new List<int>();
            ProjectionData.printImages.Clear();
            for (int i = 0; i < _ppc._config.projectorNum; i++)
            {
                ProjectionData.printImages.Add(res.Item1[i]);
                ProjectionData.printImages[i].Freeze();
                ProjectionData.printPixCounts.Add(res.Item2[i]);
            }
            ProjectionData.currPrintImagesIndexs = ind;
            sw.Stop();
            ptLog.Info($"切层耗时{sw.Elapsed.TotalSeconds}");
        }
        public void CheckLayoutProjectS()
        {
            if (_ppc._config.projectorNum < 2)
            {
                return;
            }
            foreach (var p in partModels)
            {
                if (p.TX > _ppc._config.projectorConfigs[1].projectorOffsetX && p.printInWhitchProjector != 1)
                {
                    p.printInWhitchProjector = 1;
                }
            }
        }
        public class LayoutItem
        {
            public string Model { get; set; }

            public double TX { get; set; } = 0;


            public double TY { get; set; } = 0;

            public double RZ { get; set; } = 0;

            public Matrix3D Matrix3D { get; set; }
        }

        public void PrintTask_PreProcess(DEMData base_values, bool isSimulate)
        {
            //对轨迹的
            if (_config.printConfig.isAlignerMold)
            {
                ConnectColumn();
            }
            
            volumeCount = 0;

            for (int i = 0; i < partModels.Count(); i++)
            {
                if (partModels[i].IsToPrint)
                {
                    volumeCount += taskModels[i].volume;
                }
            }
            //确认位置和光机分配
            CheckLayoutProjectS();
            //SaveLayout();
            bool isAddLayout = false;
            if (layoutItems.Count == 0)
            {
                isAddLayout = true;
            }
            foreach (var p in partModels)
            {
                if (p.IsToPrint)
                {
                    if (isAddLayout)
                    {
                        Matrix3D matrix = new Matrix3D(Math.Cos(Math.PI * p.RZ / 180), Math.Sin(Math.PI * p.RZ / 180), 0, 0,
                                -Math.Sin(Math.PI * p.RZ / 180), Math.Cos(Math.PI * p.RZ / 180), 0, 0,
                                0, 0, 1, 0,
                               p.TX, p.TY, 0, 1);

                        layoutItems.Add(new LayoutItem()
                        {
                            Model = p.fileName,
                            TX = Math.Round(p.TX, 3),
                            TY = Math.Round(p.TY, 3),
                            RZ = Math.Round(p.RZ, 3),
                            Matrix3D = matrix
                        });
                    }
                    //计算在当前投影仪下坐标
                    double x = p.TX;
                    double y = p.TY;
                    //获取当前坐标下的高度
                    int xi = (int)Math.Floor(x / (4));
                    int yi = (int)Math.Floor(y / (4));
                    double layercompd = 0;
                    if (!isSimulate)
                    {
                        layercompd = (base_values.values[xi, yi] - base_values.min) / _config.printConfig.layerThickness;
                    }

                    if (_config.baseCalibParas.enableBaseCalib)
                    {
                        p.printLayerOffset = (int)Math.Round(layercompd);
                    }
                    else
                    {
                        p.printLayerOffset = 0;//取消底部补偿
                    }


                    if (p.printLayerOffset > maxcomp) { maxcomp = p.printLayerOffset; }
                    int currLayerCount = (int)Math.Floor(p.DZ / _config.printConfig.layerThickness + 0.5);
                    if (currLayerCount + p.printLayerOffset + _config.baseCalibParas.baseCompNum > maxslice) { maxslice = currLayerCount + p.printLayerOffset + _config.baseCalibParas.baseCompNum; }
                }
            }


        }
        public class InitResult
        {
            public bool isSuccess { get; set; }
            public PlatformTestResult platformTestResult { get; set; }
            public PlatformCalibResult platformCalibResult { get; set; }
            public double targetPlatePos { get; set; }

            public string message { get; set; }
        }

        
        public  InitResult PrintTaskInit()
        {
            if (ClassValue.EnableSimulate)
            {
                MessageInfoLogUpdate_Thread("测试模式!");
                return new InitResult() { isSuccess = true, platformCalibResult = new PlatformCalibResult() };
            }
            if (!_pmc._isReady || !_ppc.P_List[0].projectorReady || !_ppc.P_List[1].projectorReady)
            {
                MessageInfoLogUpdate_Thread("硬件连接异常，初始化失败!");
                return new InitResult() { isSuccess = false };
            }
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                 _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
                            {
                                printMode = PrintMode.Single,
                                progressBarVisibility = System.Windows.Visibility.Visible,
                                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                                progress = 0,
                                layerTips = "开始初始化"
                            });
            }));
           
            if (Math.Abs(_config.baseCalibParas.zInitComp) > 0.36)
            {
                MessageInfoLogUpdate_Thread($"z轴初始位置补偿过多{_config.baseCalibParas.zInitComp.ToString("0.00")}mm，请确认是否错误设置");
            }
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
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                 _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
                            {
                                printMode = PrintMode.Single,
                                progressBarVisibility = System.Windows.Visibility.Visible,
                                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                                progress = 10,
                                layerTips = "使能运动轴"
                            });
            }));
           
            Thread.Sleep(TimeSpan.FromSeconds(1));
            _pmc.MoveScraper2Side(false);//刮刀快速回位
            double levelBoxPosSet = 15;
            //刮刀移动到-120，读取RFID
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
            {
                printMode = PrintMode.Single,
                progressBarVisibility = System.Windows.Visibility.Visible,
                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                progress = 15,
                layerTips = "Rfid读取"
            });
            }));
            
            _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, -120, _config.motionConfig.motionSpeedConfig.platformInitSpeed);
            while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) + 120) > 0.1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            var resPlateRFID = _pmc.RfidRead(RFIDEnum.PlateRFID);
            if (!resPlateRFID.Item1)
            {
                MessageInfoLogUpdate_Thread($"未读取到网板RFID，无法初始化。");
                return new InitResult() { isSuccess = false ,message = "未读取到网板RFID，无法初始化" };
            }
            var spliString = resPlateRFID.Item2.Split(',');
            if (spliString.Count() != 2)
            {
                MessageInfoLogUpdate_Thread($"网板编号异常，无法初始化。");
                return new InitResult() { isSuccess = false ,message = "网板编号异常，无法初始化。" };
            }
            plateNum = resPlateRFID.Item2.Substring(0, resPlateRFID.Item2.LastIndexOf(","));
            MessageInfoLogUpdate_Thread($"当前网板ID:'{plateNum}'");
            string iniPlateStr = plateNum + ",001";
            _pmc.RfidWrite(RFIDEnum.PlateRFID, iniPlateStr);
            Thread.Sleep(300);
            resPlateRFID = _pmc.RfidRead(RFIDEnum.PlateRFID);
            if (!resPlateRFID.Item1)
            {
                MessageInfoLogUpdate_Thread($"未读取到网板RFID，无法初始化。");
                return new InitResult() { isSuccess = false ,message = "未读取到网板RFID，无法初始化。" };
            }
            if (resPlateRFID.Item2 != iniPlateStr)
            {
                MessageInfoLogUpdate_Thread($"网板RFID擦除失败，无法初始化。");
                return new InitResult() { isSuccess = false ,message = "网板RFID擦除失败，无法初始化。" };
            }
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
 _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
            {
                printMode = PrintMode.Single,
                progressBarVisibility = System.Windows.Visibility.Visible,
                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                progress = 20,
                layerTips = "网板沉入"
            });
            }));
           
            _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, 5, _config.motionConfig.motionSpeedConfig.platformInitSpeed);
            _pmc.PTP_Pos_Speed_mm(AxesEnum.Levelbox, levelBoxPosSet, _config.motionConfig.motionSpeedConfig.levelBoxInitSpeed);
            while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) - 5) > 0.1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Thread.Sleep(TimeSpan.FromSeconds(3));
            //_pmc.PTP_MM(AxesEnum.Levelbox, 0, 60000, 1000000, 1000000);
            _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, _config.baseCalibParas.zInitComp, _config.motionConfig.motionSpeedConfig.platformInitSpeed);
            while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Levelbox) - levelBoxPosSet) > 0.1 || Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) - _config.baseCalibParas.zInitComp) > 0.1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Thread.Sleep(TimeSpan.FromSeconds(3));
            //获取当前液位
            //如果液位大于设定值，则提示排液
            //如果液位低于可调整值（1.08mm），则提示补液；
            //负压对于液位影响约1kPa -> 0.1mm
            //浮筒对于液位影响约1mm  -> 0.036mm
            //浮筒需留30mm调整余量，浮筒总行程60mm
            //负压有没有吸附上可以差0.5mm
            //负压破坏临界值-2mm
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
 _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
            {
                printMode = PrintMode.Single,
                progressBarVisibility = System.Windows.Visibility.Visible,
                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                progress = 30,
                layerTips = "补液"
            });
            }));
           
            if (_pmc.GetAD(ADEnum.Level) - _config.levelControlParas.level < -0.4)
            {
                string errorMessage = $"液位过高，{(_pmc.GetAD(ADEnum.Level) - _config.levelControlParas.level).ToString("0.0mm")}，无法初始化。";
                MessageInfoLogUpdate_Thread(errorMessage);
                return new InitResult() { isSuccess = false ,message = errorMessage };
            }

            //如果液位低于2mm,先补液至0.5mm，再进行自动调整
            Task<bool> t1 = new Task<bool>(() =>
                    AddResin()
                );
            t1.Start();
            //刮刀动作
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
  _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
            {
                printMode = PrintMode.Single,
                progressBarVisibility = System.Windows.Visibility.Visible,
                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                progress = 40,
                layerTips = "平台检测"
            });
            }));
          
            MessageInfoLogUpdate_Thread($"开始平台检测...");
            var res = _pmc.MoveScraperInAndTest(_config.baseCalibParas.zInitComp, 100, ClassValue.EnableSimulate);
            
            //_pmc.MoveScraper2Side(true, _config.motionConfig.motionSpeedConfig.scraperInitSpeed);
            Thread.Sleep(TimeSpan.FromSeconds(5));
            _pmc.MoveScraper2Side(false, _config.motionConfig.motionSpeedConfig.scraperInitSpeed);
            platformTestResult = res;
            platformCalibResult = PlatformCalib.GetPlatformCalibResult(res, _pmc._config);

            
            t1.Wait();
            if (!res.canBeTested)
            {
                MessageInfoLogUpdate_Thread($"平台测试失败");
                return new InitResult() { isSuccess = false , message= "平台测试失败" };
            }
            if (!t1.Result)
            {
                MessageInfoLogUpdate_Thread($"补液失败");
                return new InitResult() { isSuccess = false, message = "补液失败" };
            }
            MessageInfoLogUpdate_Thread($"平台测试结果：");
            MessageInfoLogUpdate_Thread($"{platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(360 / 4))].ToString("0.00mm")}                {platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(360 / 4))].ToString("0.00mm")}");
            MessageInfoLogUpdate_Thread($"{platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(250 / 4))].ToString("0.00mm")}                {platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(250 / 4))].ToString("0.00mm")}");
            MessageInfoLogUpdate_Thread($"{platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(140 / 4))].ToString("0.00mm")}                {platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(140 / 4))].ToString("0.00mm")}");
            MessageInfoLogUpdate_Thread($"{platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(20 / 4))].ToString("0.00mm")}                 {platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(20 / 4))].ToString("0.00mm")}");

            if (platformCalibResult.data.max - platformCalibResult.data.min > _config.baseCalibParas.plateHeightest - _config.baseCalibParas.plateLowest)
            {
                string errorMessage = $"平台平面度{(platformCalibResult.data.max - platformCalibResult.data.min).ToString("0.0mm")},超差无法初始化";
                MessageInfoLogUpdate_Thread(errorMessage);
                return new InitResult() { isSuccess = false ,message= errorMessage };
            }
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
_ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
            {
                printMode = PrintMode.Single,
                progressBarVisibility = System.Windows.Visibility.Visible,
                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                progress = 60,
                layerTips = "负压监测"
            });
            }));
            
            // 等待 负压 稳定
            MessageInfoLogUpdate_Thread($"等待负压稳定...");
            double gapS = 100;
            double stableV = 0;
            while (gapS > 2)
            {
                double max = -1000; double min = 1000;
                for (int i = 0; i < 30; i++)
                {

                    double currVaccum = _pmc.GetAD(ADEnum.ScraperVacuum);
                    if (currVaccum > max) { max = currVaccum; }
                    if (currVaccum < min) { min = currVaccum; }
                    Thread.Sleep(100);
                }
                gapS = max - min;
                stableV = (max + min) / 2;
            }
            MessageInfoLogUpdate_Thread($"负压稳定在:{stableV.ToString("0Pa")}");
            //开始开启光机背光预热
            foreach (var p in _ppc.P_List)
            {
                p.LedOnOff(true);
                p.SetLEDDAC(1000);
            }
            //移动平台到目标位置
            double targetPlatePos = platformCalibResult.data.min - _config.baseCalibParas.plateLowest;
            MessageInfoLogUpdate_Thread($"打印初始平台位置:{targetPlatePos.ToString("0.00mm")}");
            if (targetPlatePos < -0.35)
            {
                MessageInfoLogUpdate_Thread($"平台目标位置过高无法初始化");
                return new InitResult() { isSuccess = false,message = "平台目标位置过高无法初始化" };
            }
            _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, targetPlatePos, _config.motionConfig.motionSpeedConfig.platformInitSpeed);
            while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) - targetPlatePos) > 0.1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Thread.Sleep(TimeSpan.FromSeconds(3));
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
 _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
            {
                printMode = PrintMode.Single,
                progressBarVisibility = System.Windows.Visibility.Visible,
                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                progress = 70,
                layerTips = "液位第一次调整"
            });
            }));
           
            //第一次调整液位

            double gap = (this._config.levelControlParas.level - _pmc.GetAD(ADEnum.Level));
            double levelOffset1 = -gap / 0.036;
            _pmc.PTP_Pos_Speed_mm(AxesEnum.Levelbox, _pmc.GetAxisPos_MM(AxesEnum.Levelbox) + levelOffset1, _config.motionConfig.motionSpeedConfig.levelBoxInitSpeed);
            Thread.Sleep(TimeSpan.FromSeconds(3));
            Thread.Sleep(TimeSpan.FromSeconds(1));
            MessageInfoLogUpdate_Thread($"第一次液位调整:{gap.ToString("0.000mm")}");
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
_ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
            {
                printMode = PrintMode.Single,
                progressBarVisibility = System.Windows.Visibility.Visible,
                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                progress = 80,
                layerTips = "刮平"
            });
            }));
            
            //再次刮平
            _pmc.MoveScraper2Side(true, _config.motionConfig.motionSpeedConfig.scraperInitSpeed);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            _pmc.MoveScraper2Side(false, _config.motionConfig.motionSpeedConfig.scraperInitSpeed);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            //_pmc.PTP_MM(AxesEnum.Plaform, 0, 60000, 1000000, 1000000);
            //Thread.Sleep(TimeSpan.FromSeconds(1));
            //第二次调整液位
            double gap2 = (this._config.levelControlParas.level - _pmc.GetAD(ADEnum.Level));
            double levelOffset2 = -gap2 / 0.036;
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
 _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
            {
                printMode = PrintMode.Single,
                progressBarVisibility = System.Windows.Visibility.Visible,
                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                progress = 90,
                layerTips = "液位第二次调整"
            });
            }));
           
            MessageInfoLogUpdate_Thread($"第二次液位调整:{gap2.ToString("0.000mm")}");
            _pmc.PTP_Pos_Speed_mm(AxesEnum.Levelbox, _pmc.GetAxisPos_MM(AxesEnum.Levelbox) + levelOffset2, _config.motionConfig.motionSpeedConfig.levelBoxInitSpeed);
            //确认液位
            Thread.Sleep(TimeSpan.FromSeconds(5));
            double gap3 = (this._config.levelControlParas.level - _pmc.GetAD(ADEnum.Level));
            double levelOffset3 = -gap3 / 0.036;
            //第三次调整液位
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
_ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
            {
                printMode = PrintMode.Single,
                progressBarVisibility = System.Windows.Visibility.Visible,
                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                progress = 95,
                layerTips = "液位第三次调整"
            });
            }));
            
            MessageInfoLogUpdate_Thread($"第三次液位调整:{gap3.ToString("0.000mm")}");
            _pmc.PTP_Pos_Speed_mm(AxesEnum.Levelbox, _pmc.GetAxisPos_MM(AxesEnum.Levelbox) + levelOffset3, _config.motionConfig.motionSpeedConfig.levelBoxInitSpeed);
            Thread.Sleep(TimeSpan.FromSeconds(5));
            this.levelInitOffset = _pmc.GetAxisPos_MM(AxesEnum.Levelbox);
            this.resinTempInit = _pmc.GetAD(ADEnum.ResinTemp);
            this.targetPlatePos = targetPlatePos;
            double finalLevel = _pmc.GetAD(ADEnum.Level);
            if (Math.Abs(this._config.levelControlParas.level - finalLevel) > 0.05)
            {
                MessageInfoLogUpdate_Thread($"液位初始化失败,最终液位{finalLevel.ToString("0.00mm")}");
                return new InitResult() { isSuccess = false,message= $"液位初始化失败,最终液位{finalLevel.ToString("0.00mm")}" };
            }
            else
            {
                MessageInfoLogUpdate_Thread($"最终液位值:{finalLevel.ToString("0.00mm")}");
            }
            foreach (var p in _ppc.P_List)
            {
                p.LedOnOff(false);
            }
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
_ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
            {
                printMode = PrintMode.Single,
                progressBarVisibility = System.Windows.Visibility.Visible,
                cColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray),
                fColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                pColor = new SolidColorBrush(System.Windows.Media.Colors.Green),
                progress = 100,
                layerTips = "初始化完成"
            });
            }));
            
            MessageInfoLogUpdate_Thread("初始化完成");
            this.isInited = true;
            return new InitResult()
            {
                isSuccess = true,
                platformTestResult = res,
                platformCalibResult = this.platformCalibResult,
                targetPlatePos = targetPlatePos
            };
        }
        public bool AddResinForVolumn(double volumn)
        {
            double addResinBoxLevel = _pmc.GetModBusAD(ModBusReadEnum.ResinAddBoxLevel);
            double levelGapAddBox = volumn / _pmc._config.levelControlConfig.resinAddCrossArea;
            double v2 = addResinBoxLevel * _pmc._config.levelControlConfig.resinAddCrossArea ;
            if (volumn + 50000 > v2)
            {
                MessageInfoLogUpdate_Thread($"剩余树脂{(v2 / 1000).ToString("0.0ml")}不足，需求{volumn.ToString("0.0ml")}");
                if (isMES)
                {
                    _ea.GetEvent<UpDatePrinterAlertEvent>().Publish(new Tuple<PrinterAlerts, bool>(PrinterAlerts.replenishment, true));
                }
                return false;
            }

            if (levelGapAddBox > 0.5)
            {
                
                MessageInfoLogUpdate_Thread($"补液箱树脂高度{addResinBoxLevel.ToString("0.0mm")}");

                //打开阀门
                _pmc.AddResinValueSwitch(true);
                //监测液位，液位差0.2时停止

                int timescap = 150;
                int interval = 0;
                while (addResinBoxLevel - _pmc.GetModBusAD(ModBusReadEnum.ResinAddBoxLevel) < levelGapAddBox)
                {
                    if (interval++ * timescap > 240 * 1000)
                    {
                        _pmc.AddResinValueSwitch(false);
                        MessageInfoLogUpdate_Thread($"补液超时");
                        return false;
                    }
                    Thread.Sleep(timescap);
                }
                _pmc.AddResinValueSwitch(false);
                double afterLevel = _pmc.GetModBusAD(ModBusReadEnum.ResinAddBoxLevel);
                MessageInfoLogUpdate_Thread($"补液箱树脂高度{afterLevel.ToString("0.0mm")}");
                MessageInfoLogUpdate_Thread($"成功补液{((addResinBoxLevel - afterLevel) * _pmc._config.levelControlConfig.resinAddCrossArea).ToString("0.0mm^3")}");
            }
            else
            {
            }
            return true;
        }
        public  bool AddResin()
        {
            double levelGap = (_pmc.GetAD(ADEnum.Level) - _config.levelControlParas.level);
            if (levelGap > 0.3)
            {
                MessageInfoLogUpdate_Thread($"液位差距{levelGap.ToString("0.0mm")}");
                double addResinBoxLevel = _pmc.GetModBusAD(ModBusReadEnum.ResinAddBoxLevel);
                MessageInfoLogUpdate_Thread($"补液箱树脂高度{addResinBoxLevel.ToString("0.0mm")}");
                double v1 = levelGap * _pmc._config.levelControlConfig.tankCrossArea/1000;
                double v2 = addResinBoxLevel * _pmc._config.levelControlConfig.resinAddCrossArea/1000;
                if ( v1> v2)
                {
                    MessageInfoLogUpdate_Thread($"剩余树脂{v2.ToString("0.0ml")}不足，需求{v1.ToString("0.0ml")}");
                    if (isMES)
                    {
                        _ea.GetEvent<UpDatePrinterAlertEvent>().Publish(new Tuple<PrinterAlerts, bool>(PrinterAlerts.replenishment, true));
                    }
                    return false;
                }
                //打开阀门
                _pmc.AddResinValueSwitch(true);
                //监测液位，液位差0.2时停止
                double gap = 0;
                if (_pmc.isAddResinByValue)
                {
                    gap = 0.1;
                }
                int timescap = 150;
                int interval = 0;
                while (_pmc.GetAD(ADEnum.Level) - _config.levelControlParas.level > gap)
                {
                    if (interval++ * timescap > 240 * 1000) {
                        _pmc.AddResinValueSwitch(false);
                        MessageInfoLogUpdate_Thread($"补液超时"); 
                        return false; }
                    Thread.Sleep(timescap);
                }
                _pmc.AddResinValueSwitch(false);
            }
            return true;
        }
        public async Task<InitResult> PrintTaskInitForLevelCalib()
        {
            if (!_pmc._isReady)
            {
                return new InitResult() { isSuccess = true };
            }

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
            _pmc.MoveScraper2Side(false);//刮刀快速回位
            Thread.Sleep(TimeSpan.FromSeconds(3));
            //_pmc.PTP_MM(AxesEnum.Levelbox, 0, 60000, 1000000, 1000000);
            _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, _config.baseCalibParas.zInitComp, _config.motionConfig.motionSpeedConfig.platformInitSpeed);
            while (Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) - _config.baseCalibParas.zInitComp) > 0.1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Thread.Sleep(TimeSpan.FromSeconds(3));


            //刮刀动作
            var res = _pmc.MoveScraperInAndTest(_config.baseCalibParas.zInitComp, 100, ClassValue.EnableSimulate);
            if (!res.canBeTested)
            {
                MessageInfoLogUpdate_Thread($"平台测试失败");
                return new InitResult() { isSuccess = false };
            }
            //_pmc.MoveScraper2Side(true, _config.motionConfig.motionSpeedConfig.scraperInitSpeed);
            Thread.Sleep(TimeSpan.FromSeconds(5));
            _pmc.MoveScraper2Side(false, _config.motionConfig.motionSpeedConfig.scraperInitSpeed);
            platformTestResult = res;
            platformCalibResult = PlatformCalib.GetPlatformCalibResult(res, _pmc._config);



            if (platformCalibResult.data.max - platformCalibResult.data.min > _config.baseCalibParas.plateHeightest - _config.baseCalibParas.plateLowest)
            {
                MessageInfoLogUpdate_Thread($"平台平面度超差无法初始化");
                return new InitResult() { isSuccess = false };
            }
            //移动平台到目标位置
            double targetPlatePos = platformCalibResult.data.min - _config.baseCalibParas.plateLowest;
            if (targetPlatePos < -0.2)
            {
                MessageInfoLogUpdate_Thread($"平台目标位置过高无法初始化");
                return new InitResult() { isSuccess = false };
            }

            this.targetPlatePos = targetPlatePos;
            MessageInfoLogUpdate_Thread("初始化完成");
            this.isInited = true;
            return new InitResult()
            {
                isSuccess = true,
                platformTestResult = res,
                platformCalibResult = this.platformCalibResult,
                targetPlatePos = targetPlatePos
            };
        }
        public class ValueChanges
        {
            public double levelRange { get; set; }
            public double levelSlope { get; set; }

            public int levelAttIndex { get; set; }
            public double vaccumRange { get; set; }
            public double vaccumSlope { get; set; }

            public int vaccumAttIndex { get; set; }
            public double tempRange { get; set; }
            public double tempSlope { get; set; }
            public int tempAttIndex { get; set; }


        }

        public Tuple<double, int, double> GetChange(List<double> values)
        {
            double range = Math.Abs(values.Max() - values.Min());
            double max = 0;
            int index = 0;
            for (int i = 0; i < values.Count - 3; i++)
            {
                List<double> list3 = values.GetRange(i, 3).ToList();
                double slope3 = Math.Abs(list3.Max() - list3.Min());
                if (slope3 > max)
                {
                    max = slope3;
                    index = i;
                }
            }
            return new Tuple<double, int, double>(range, index, max);
        }
        private bool GetValueChange(int startInd = 9, int count = 10)
        {
            if (count < 2)
            {
                return false;
            }
            if (printLog.Count < startInd + count)
            {
                return false;
            }
            List<double> levels = printLog.Select(t => t.levelBe).ToList().GetRange(startInd,count); ;
            //levels = levels.GetRange(_config.levelControlParas.startFFLC, levels.Count - _config.levelControlParas.startFFLC - 1);
            var resLevel = GetChange(levels);
            valueChanges.levelRange = resLevel.Item1;
            valueChanges.levelAttIndex = resLevel.Item2 + _config.levelControlParas.startFFLC;
            valueChanges.levelSlope = resLevel.Item3;

            List<double> vaccums = printLog.Select(t => t.scraperVacuum).ToList().GetRange(startInd, count); ;
            //vaccums = vaccums.GetRange(_config.levelControlParas.startFFLC, vaccums.Count - _config.levelControlParas.startFFLC - 1);
            var resVaccum = GetChange(vaccums);
            valueChanges.vaccumRange = resVaccum.Item1;
            valueChanges.vaccumAttIndex = resVaccum.Item2 + _config.levelControlParas.startFFLC;
            valueChanges.vaccumSlope = resVaccum.Item3;

            List<double> temps = printLog.Select(t => t.resinTemp).ToList().GetRange(startInd, count); ;
            //temps = temps.GetRange(_config.levelControlParas.startFFLC, temps.Count - _config.levelControlParas.startFFLC - 1);
            var resTemp = GetChange(temps);
            valueChanges.tempRange = resTemp.Item1;
            valueChanges.tempAttIndex = resTemp.Item2 + _config.levelControlParas.startFFLC;
            valueChanges.tempSlope = resTemp.Item3;
            return true;
        }
        public class PrintResult
        {
            public bool isSuccess { get; set; }
            public List<PrintLog> printLogs { get; set; }
            

            public string message { get; set; }
        }
        public PrintResult Print()
        {
            if (isMSS_Computed != 2)
            {
                _ea.GetEvent<MessageEvent>().Publish("未完成切层计算");
                return new PrintResult() { 
                    isSuccess = false,
                    message = "未完成切层计算"
                };
            }

            _ea.GetEvent<SaveViewPortEvent>().Publish(this);
            printStartTime = DateTime.Now;
            if (ClassValue.EnableSimulate)
            {
                PrintTask_PreProcess(new DEMData(), ClassValue.EnableSimulate);
            }
            else
            {
                PrintTask_PreProcess(platformCalibResult.data, ClassValue.EnableSimulate);
            }
            
            //保存当前打印任务到数据库
            //新建打印任务文件夹
            printTaskName = AngleDLP.LiteDBHelper.PrintTaskDBInfo.AddPrintTaskInfo(new AngleDLP.LiteDBHelper.PrintTaskDBInfo(this));
            //保存当前layout.bmp
            string currtaskLogFolder = $"{ClassValue.appDir}\\logs\\{printStartTime.Date.ToString("yyyyMM")}\\{printStartTime.Date.ToString("yyyyMMdd")}\\{printTaskName}";
            Directory.CreateDirectory(currtaskLogFolder);
            
            if (!ClassValue.EnableSimulate)
            {
                using (FileStream fs = new FileStream(Path.Combine(Application.StartupPath, $"{currtaskLogFolder}\\platformtest.json"), FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(JsonConvert.SerializeObject(platformTestResult, Newtonsoft.Json.Formatting.Indented));
                    }
                }
                using (FileStream fs = new FileStream(Path.Combine(Application.StartupPath, $"{currtaskLogFolder}\\platformcalib.json"), FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(JsonConvert.SerializeObject(platformCalibResult.data, Newtonsoft.Json.Formatting.Indented));
                    }
                }
                SaveImageToFile(platformCalibResult.bitmapSource, $"{currtaskLogFolder}\\platformCalibResult.bmp");

            }
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SaveImageToFile(layoutBitmap, $"{currtaskLogFolder}\\layout.bmp");
            }));
            

            

            using (FileStream fs = new FileStream(Path.Combine(Application.StartupPath, $"{currtaskLogFolder}\\layout.json"), FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(layoutItems, Newtonsoft.Json.Formatting.Indented));
                }
            }


            using (FileStream fs = new FileStream(Path.Combine(Application.StartupPath, $"{currtaskLogFolder}\\taskconfig.cfg"), FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(_config, Newtonsoft.Json.Formatting.Indented));

                }
            }

            printTaskStatus = 1;

            //判断是否两个光机都启动
            List<bool> isProjectorOn = new List<bool>();
            isProjectorOn.Add(false);
            isProjectorOn.Add(false);
            foreach (var p in partModels)
            {
                if (p.IsToPrint && p.printInWhitchProjector == 0)
                {
                    isProjectorOn[0] = true;
                }
                if (p.IsToPrint && p.printInWhitchProjector == 1)
                {
                    isProjectorOn[1] = true;
                }
            }
            var processPathRes = AngelSlicer.ProcessPath(ref taskModels);
            MessageInfoLogUpdate_Thread($"{processPathRes.Item2}");
            if (!processPathRes.Item1)
            {
                return new PrintResult()
                {
                    isSuccess = false,
                    message = processPathRes.Item2
                };
            }
           
            _sliceParas = new List<SliceParas>();
            List<double> eles = new List<double>();
            foreach (var ele in _config.baseCalibParas.elephantFeetComps)
            {
                eles.Add(ele.offset);
            }
            foreach (var conf in _ppc._config.projectorConfigs)
            {
                List<double> holecomps = new List<double>();
                if (_config.baseCalibParas.holeComps.Count > 0)
                {
                    foreach(var h in _config.baseCalibParas.holeComps)
                    { holecomps.Add(h.radius); }
                }
                else
                {
                    for (int ih = 0; ih < _config.baseCalibParas.holeCompNum; ih++)
                    {
                        holecomps.Add(conf.holeRadius);
                    }
                }
                _sliceParas.Add(new SliceParas()
                {
                    layerThicness = _config.printConfig.layerThickness,
                    drawScaleX = _config.accuracyParas.scaleCompX / conf.projectorPixScaleX,
                    drawScaleY = _config.accuracyParas.scaleCompY / conf.projectorPixScaleY,
                    contourCompX = _config.accuracyParas.contourCompX,
                    contourCompY = _config.accuracyParas.contourCompY,
                    projectorPixScaleX = conf.projectorPixScaleX,
                    projectorPixScaleY = conf.projectorPixScaleY,
                    width = conf.printerPixWidth,
                    height = conf.printerPixHeight,
                    projectorOffsetX = conf.projectorOffsetX,
                    projectorOffsetY = conf.projectorOffsetY,
                    grayCalib = conf.grayCalibParas.grayCalib,
                    calibScaleFix = conf.calibScaleFix,
                    holeOffsetX = conf.holeOffsetX,
                    holeOffsetY = conf.holeOffsetY,
                    holeRz = conf.holeRz,
                    calibMartix = conf.calibMartix,
                    baseCompNum = _config.baseCalibParas.baseCompNum,
                    baseEnhanceRate = _config.baseCalibParas.exposeBaseEnhanceRate * _config.baseCalibParas.lightBaseEnhanceRate,
                    baseEnhanceLayer = _config.baseCalibParas.exposeBaseEnhanceLayer,
                    isAlignerMold = _config.printConfig.isAlignerMold,
                    dimmingRate = _config.printConfig.dimmingRate,
                    dimmingWall = _config.printConfig.dimmingWall,
                    elephantFeetComps = eles,
                    holeComps = holecomps
                });
            }
            
            Task task_GetSupport =new Task(() => { AngelSlicer.GetSupports_OpenCV(ref taskModels, _sliceParas, partModels); });
            if (_config.printConfig.isAlignerMold)
            {
                task_GetSupport.Start();
            }
            
            //_ea.GetEvent<GetProjectorImagesEvent>().Publish(0);
            Task task_Slice0 = Task.Factory.StartNew(() => { GetPrintSliceAreas(0); });


            //double levelboxInteval = -_config.levelboxIntevalvsLayer * _config.layerThicness;
            double levelboxPos = levelInitOffset;
            double platformPos = targetPlatePos;
            bool scraperForward = false;
            //使能
            _pmc.Enable(AxesEnum.Scraper, ClassValue.EnableSimulate);
            _pmc.Enable(AxesEnum.Levelbox, ClassValue.EnableSimulate);
            _pmc.Enable(AxesEnum.Plaform, ClassValue.EnableSimulate);
            //_pmc.MessageInfoUpdate($"使能运动轴");

            int i = 0;

            //开始打印
            //液位实测值，经过符号修正，变更为上升为正,符合直觉
            List<double> levelsMeasured = new List<double>();//每一层的实测液位
            List<double> volumnsCompInterval = new List<double>();//每一层需要步进的体积（使液位升高为正） = 平台下沉截面积*层厚 - 打印面积*层厚*树脂体收缩率。用于计算液位步进量

            List<double> levelsBeforeFixed = new List<double>();//液位实测值+液位修正量,用于评价步进值计算的合理性
            double levelFix = 0;
            ///液位相关参数解释：
            ///树脂槽截面积-343900 - 7900 - 3000 = 333000mm2
            ///浮筒截面积-7900mm2
            ///Z轴支架截面积 - 3000mm2
            ///levelboxIntevalvsLayer = 0.38  = 3000/79000
            ///levelboxIntevalvsLayerArea = 0.0000110 = 0.086/8000

            double layerTimeEstimate = _config.printConfig.exposeTime + 1.5 + 465 / _config.motionConfig.motionSpeedConfig.scraperSpeed;
            task_Slice0.Wait();
            using (FileStream fs = new FileStream(Path.Combine(Application.StartupPath, $"{currtaskLogFolder}\\printtaskinfo.txt"), FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine($"打印机:{_pmc._config.printerModel}[{_pmc._config.printerSN}]");
                    sw.WriteLine($"任务名称:{printTaskName}");
                    sw.WriteLine($"任务GUID:{this.printtask_GUID.ToString()}");
                    sw.WriteLine($"配方名称:{this.configName},ver[{this.configVersion}]");
                    sw.WriteLine($"网板ID:{this.plateNum}");
                    sw.WriteLine($"开始时间:{printStartTime}");
                    sw.WriteLine($"导入模型数量:{taskModels.Count}");
                    sw.WriteLine($"总层数:{maxslice}");
                    sw.WriteLine($"模型总体积:{volumeCount.ToString("0.0")}mm3");//
                    sw.WriteLine($"底面校准情况:");
                    if (!ClassValue.EnableSimulate)
                    {
                        sw.WriteLine($"{platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(360 / 4))].ToString("0.00mm")}                              {platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(360 / 4))].ToString("0.00mm")}");
                        sw.WriteLine($"{platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(250 / 4))].ToString("0.00mm")}                              {platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(250 / 4))].ToString("0.00mm")}");
                        sw.WriteLine($"{platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(140 / 4))].ToString("0.00mm")}                              {platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(140 / 4))].ToString("0.00mm")}");
                        sw.WriteLine($"{platformCalibResult.data.values[(int)Math.Round((double)(20 / 4)), (int)Math.Round((double)(20 / 4))].ToString("0.00mm")}                              {platformCalibResult.data.values[(int)Math.Round((double)(400 / 4)), (int)Math.Round((double)(20 / 4))].ToString("0.00mm")}");
                        sw.WriteLine($"打印初始平台位置:{targetPlatePos.ToString("0.00mm")}");
                    }

                    
                    sw.WriteLine($"---------------------------------------------------------------");//
                    sw.WriteLine($"排版模型列表:");//
                    int modelCount = 0;
                    foreach (var pm in partModels)
                    {
                        if (!pm.IsToPrint) { continue; }
                        sw.WriteLine($"  ※{files_Selected[pm.index].modelId}  :  {files_Selected[pm.index].stl}");//
                        modelCount++;
                    }
                    sw.WriteLine($"共排版{modelCount}个模型:");//
                    sw.WriteLine($" ");//
                    sw.WriteLine($"未排版模型列表:");//
                    foreach (var pm in partModels)
                    {
                        if (pm.IsToPrint) { continue; }
                        sw.WriteLine($"  ※{files_Selected[pm.index].modelId}  :  {files_Selected[pm.index].stl}");//
                        modelCount++;
                    }
                    sw.WriteLine($"---------------------------------------------------------------");//
                    sw.WriteLine($"{ processPathRes.Item2}");

                }
            }
            if (isMES) { _ea.GetEvent<UpdatePrintJobEvent>().Publish(
                new JobEventItem() { 
                    @event = new JobEvent() { 
                        time = TimeHelper.GetNewTimeString(),
                        progress = new JobProgress() {
                            printedLayers = 0,
                            totalLayers = maxslice,
                            startLayers = 1,
                            printedVolume = volumeCount,
                            totalHeight = Math.Round(maxslice * _config.printConfig.layerThickness, 2),
                            printedHeight = 0,
                            isPaused = false
                        },
                        type = JobEventType.start,
                        completedInstanceCount = 0
                    }

                }
                );; }
           
            double printedVolumnCount = 0;
            MessageInfoLogUpdate_Thread($"{printTaskName}开始打印");
            while (i < maxslice)
            {
                if (i>0 && i % 10 == 0)
                {
                    using (FileStream fs = new FileStream($"{currtaskLogFolder}\\printlog.json", FileMode.OpenOrCreate))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine(JsonConvert.SerializeObject(printLog, Newtonsoft.Json.Formatting.Indented));
                        }
                    }
                    if (i > 15)
                    {
                        GetValueChange(i  - 10);
                        MessageInfoLogUpdate_Thread($"{i - 10}-{i}温度波动{valueChanges.tempRange.ToString("0.0℃")}");
                        MessageInfoLogUpdate_Thread($"{i - 10}-{i}液位波动{valueChanges.levelRange.ToString("0.00mm")}");
                        MessageInfoLogUpdate_Thread($"{i - 10}-{i}负压波动{valueChanges.vaccumRange.ToString("0 Pa")}");
                    }
                    LiteDBHelper.PrintTaskDBInfo.UpDatePrintTaskProgress(this.printtask_GUID.ToString(), i + 1);
                }
                if(i == 20 && _config.printConfig.isAlignerMold){ task_GetSupport.Wait(); }
                //下一层绘图和当前层是否完成绘图校验
                
                //_pmc.MessageInfoUpdate($"开始曝光第{i + 1}层");
                // MessageInfoLogUpdate_Thread($"正在打印{i + 1}/{maxslice}层，预计剩余{Math.Ceiling(layerTimeEstimate * (maxslice - i - 1) / 60)}分钟", LogLevel.Info);
                double progress = 20 * i / maxslice;
                int progressInt = (int)Math.Ceiling(progress) + 1;
                _pmc.LightControl(0, 100, 100, 0, (byte)progressInt, false);
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    _ea.GetEvent<SliceAll3DEvent>().Publish((i + 0.5) * _config.printConfig.layerThickness);
                    _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
                    {
                        printMode = printMode,
                        printTaskDiscription = printMode.ToString() + this.printTaskName,
                        progressBarVisibility = System.Windows.Visibility.Visible,
                        cColor = new SolidColorBrush(System.Windows.Media.Colors.Gray),
                        fColor = new SolidColorBrush(System.Windows.Media.Colors.Gray),
                        progress = (int)Math.Ceiling((double)100 * (i + 1) / maxslice),
                        layerTips = $"正在打印{i + 1}/{maxslice}层，剩余{Math.Ceiling(layerTimeEstimate * (maxslice - i - 1) / 60)}分钟"
                    }); ;

                }));

                if (ProjectionData.currPrintImagesIndexs == i)
                {
                    //_pmc.MessageInfoUpdate($"{i}图片序列校验成功");
                }
                else
                {
                    MessageInfoLogUpdate_Thread($"{i}图片序列校验失败", LogLevel.Fatal);
                    return new PrintResult()
                    {
                        isSuccess = false,
                        message = $"{i}图片序列校验失败"
                    };
                }

                if (i < _config.levelControlParas.startFFLC && i > 0)
                {
                    Thread.Sleep(500 * (1 - i / _config.levelControlParas.startFFLC));
                    //补偿液位
                    double levelOffset = _pmc.GetAD(ADEnum.Level, ClassValue.EnableSimulate) - _config.levelControlParas.level;
                    double levelBoxOffset = levelOffset * _pmc._config.levelControlConfig.tankCrossArea / _pmc._config.levelControlConfig.floatCrossArea;
                    levelboxPos += levelBoxOffset;
                    _pmc.PTP_Pos_Speed_mm(AxesEnum.Levelbox, levelboxPos, _config.motionConfig.motionSpeedConfig.levelBoxSpeed, ClassValue.EnableSimulate);//浮筒下沉 
                    MessageInfoLogUpdate_Thread($"液位补偿{levelOffset.ToString("0.000mm")}");
                    Thread.Sleep(TimeSpan.FromSeconds(1.3));
                    Thread.Sleep(100 * (1 - i / _config.levelControlParas.startFFLC));
                }
                double currLevelMeasured_BeforeExpose = _pmc.GetAD(ADEnum.Level, ClassValue.EnableSimulate);
                //double currResinTempMeasured_BeforeExpose = _pmc.GetAD(ADEnum.ResinTemp1, isSimulate);
                
                
                PrintLog layerLog = new PrintLog()
                {
                    layer = i,
                    levelBe = currLevelMeasured_BeforeExpose,
                    resinTemp = _pmc.GetAD(ADEnum.ResinTemp, ClassValue.EnableSimulate),
                    levelBoxPos = levelboxPos,
                    printerTemp = Math.Round(_pmc.GetAD(ADEnum.EnvTemp), 1),
                    printerHumidity = Math.Round(_pmc.GetAD(ADEnum.EnvTemp), 1)
                };
                
                foreach (var cfg in _pmc._config.ADconfigs)
                {
                    if (cfg.name == ADEnum.TankTemp.ToString() && cfg.sn == 1)
                    {
                        layerLog.tankTemp1 = Math.Round(_pmc.GetAD(ADEnum.TankTemp, ClassValue.EnableSimulate, cfg.sn), 1);
                    }
                    if (cfg.name == ADEnum.TankTemp.ToString() && cfg.sn == 2)
                    {
                        layerLog.tankTemp2 = Math.Round(_pmc.GetAD(ADEnum.TankTemp, ClassValue.EnableSimulate, cfg.sn), 1);
                    }
                    if (cfg.name == ADEnum.TankTemp.ToString() && cfg.sn == 3)
                    {
                        layerLog.tankTemp3 = Math.Round(_pmc.GetAD(ADEnum.TankTemp, ClassValue.EnableSimulate, cfg.sn), 1);
                    }
                    if (cfg.name == ADEnum.TankTemp.ToString() && cfg.sn == 4)
                    {
                        layerLog.tankTemp4 = Math.Round(_pmc.GetAD(ADEnum.TankTemp, ClassValue.EnableSimulate, cfg.sn), 1);
                    }
                    if (cfg.name == ADEnum.TankTemp.ToString() && cfg.sn == 5)
                    {
                        layerLog.tankTemp5 = Math.Round(_pmc.GetAD(ADEnum.TankTemp, ClassValue.EnableSimulate, cfg.sn), 1);
                    }
                    if (cfg.name == ADEnum.TankTemp.ToString() && cfg.sn == 6)
                    {
                        layerLog.tankTemp6 = Math.Round(_pmc.GetAD(ADEnum.TankTemp, ClassValue.EnableSimulate, cfg.sn), 1);
                    }
                    if (cfg.name == ADEnum.TankTemp.ToString() && cfg.sn == 7)
                    {
                        layerLog.tankTemp7 = Math.Round(_pmc.GetAD(ADEnum.TankTemp, ClassValue.EnableSimulate, cfg.sn), 1);
                    }
                }

                //ptLog.Info(printLogBe[printLogBe.Count-1].ToLogger());
                DateTime layerTime = DateTime.Now;
                //投影图像
                double pTimeP = _config.printConfig.exposeTime * 1000;
                double lightEnhanse = 1;
                if (i < _config.baseCalibParas.exposeBaseEnhanceLayer)
                {
                    pTimeP = 1000 * _config.printConfig.exposeTime * _config.baseCalibParas.exposeBaseEnhanceRate;
                    lightEnhanse = _config.baseCalibParas.lightBaseEnhanceRate;
                }
                ptLog.Info($"【{DateTime.Now.ToString("HH：mm：ss.ff")}】开始曝光");
                BitmapSource image1 = _ppc.P_List[0].TransProjectImage(ProjectionData.printImages[0]);
                if (ClassValue.EnableSimulate)
                {
                    SaveImageToFile(image1, $"{currtaskLogFolder}\\{i}-left.bmp");
                }
                
                image1.Freeze();
                _ppc.P_List[0].PrepareToProj((int)Math.Round(lightEnhanse * _ppc._config.projectorConfigs[0].exposeDAC * (_config.printConfig.exposeLight / _ppc._config.projectorConfigs[0].exposeLight)), ClassValue.EnableSimulate);
                _ppc.P_List[1].PrepareToProj((int)Math.Round(lightEnhanse * _ppc._config.projectorConfigs[1].exposeDAC * (_config.printConfig.exposeLight / _ppc._config.projectorConfigs[1].exposeLight)), ClassValue.EnableSimulate);
                Task<Tuple<bool, TimeSpan, string>> t1 = new Task<Tuple<bool, TimeSpan, string>>(() =>
                    _ppc.P_List[0].ProjectImageOnly(image1, pTimeP));
                BitmapSource image2 = _ppc.P_List[1].TransProjectImage(ProjectionData.printImages[1]);
                if (ClassValue.EnableSimulate)
                {
                    SaveImageToFile(image2, $"{currtaskLogFolder}\\{i}-right.bmp");
                }
                
                image2.Freeze();
                Task<Tuple<bool, TimeSpan, string>> t2 = new Task<Tuple<bool, TimeSpan, string>>(() =>
                    _ppc.P_List[1].ProjectImageOnly(image2, pTimeP));

                t1.Start();
                t2.Start();


                double area = 0;
                if (isProjectorOn[0]) { area += Math.Pow(_ppc._config.projectorConfigs[0].projectorPixScaleX, 2) * ProjectionData.printPixCounts[0]; }//scale 仅能用于粗略计算
                if (isProjectorOn[1]) { area += Math.Pow(_ppc._config.projectorConfigs[1].projectorPixScaleX, 2) * ProjectionData.printPixCounts[1]; }

                //_ea.GetEvent<GetProjectorImagesEvent>().Publish(i + 1);

                Task taskSlicei = Task.Factory.StartNew(() => { GetPrintSliceAreas(i + 1); });
                layerLog.exposeArea = area;
                printedVolumnCount += area;


                platformPos += _config.printConfig.layerThickness;
                levelsMeasured.Add(currLevelMeasured_BeforeExpose);



                double currCompVolumn = _config.printConfig.layerThickness * _pmc._config.levelControlConfig.zCrossArea - area * _config.printConfig.layerThickness * _config.levelControlParas.materialShrinkRate;
                volumnsCompInterval.Add(currCompVolumn);
                //levelFix = 0.15*(currResinTempMeasured_BeforeExpose - this.resinTempInit); //温度补偿？？
                double offset = -currCompVolumn / _pmc._config.levelControlConfig.floatCrossArea;// - levelFix* _pmc._config.levelControlConfig.tankCrossArea / _pmc._config.levelControlConfig.floatCrossArea;
                levelboxPos += offset;
                Timer timer = new Timer();
                timer.Start();
                int logInterval = 500;
                int logCount = 0;
                int logTimming = (int)Math.Round(_config.printConfig.exposeTime * 500 / logInterval, 0);
                while (!t1.IsCompleted || !t2.IsCompleted)
                {
                    ////暂时取消光机温度采集
                    //if (++logCount == logTimming)
                    //{
                    //    if (isProjectorOn[0])
                    //    {
                    //        layerLog.projectorTemps.Add(_ppc.P_List[0].GetTemperature());
                    //        layerLog.projectorLEDTemps.Add(_ppc.P_List[0].GetLEDTemperature());
                    //        layerLog.projectorBoardTemps.Add(_ppc.P_List[0].GetLedBoardTemperature());
                    //        layerLog.projectorLights.Add(_ppc.P_List[0].GetLightSensor());
                    //    }
                    //    if (isProjectorOn[1])
                    //    {
                    //        layerLog.projectorTemps.Add(_ppc.P_List[1].GetTemperature());
                    //        layerLog.projectorLEDTemps.Add(_ppc.P_List[1].GetLEDTemperature());
                    //        layerLog.projectorBoardTemps.Add(_ppc.P_List[1].GetLedBoardTemperature());
                    //        layerLog.projectorLights.Add(_ppc.P_List[1].GetLightSensor());
                    //    }
                    //}

                    //循环记录曝光期间的负压情况
                    layerLog.scraperStayVaCuum.Add(Math.Round(_pmc.GetAD(ADEnum.ScraperVacuum), 0));
                    Thread.Sleep(logInterval);

                }
                t1.Wait();
                t2.Wait();
                _ppc.P_List[0].ProjSleep(ClassValue.EnableSimulate);
                _ppc.P_List[1].ProjSleep(ClassValue.EnableSimulate);
                if (!t1.Result.Item1) {
                    return new PrintResult()
                    {
                        isSuccess = false,
                        message = t1.Result.Item3
                    };
                }
                if (!t2.Result.Item1)
                {
                    return new PrintResult()
                    {
                        isSuccess = false,
                        message = t1.Result.Item3
                    };
                }
                layerLog.projectorTime.Add(t1.Result.Item2);
                layerLog.projectorTime.Add(t2.Result.Item2);
                layerLog.levelAe = _pmc.GetAD(ADEnum.Level, ClassValue.EnableSimulate);

                int logCount2 = 0;
                if (_config.printConfig.enableScraper)  //使用刮刀进行打印
                {

                    double sSpeed = _config.motionConfig.motionSpeedConfig.scraperSpeed;
                    if (i < 7)
                    {
                        sSpeed = _config.motionConfig.motionSpeedConfig.scraperSpeed * (0.7 + 0.3 * i / 7);
                    }
                    double scrapterWaitTime = 1000 * ((_pmc._config.scraperConfig.outPos - _pmc._config.scraperConfig.inPos) / (sSpeed * 10000)) + 700;

                    //_pmc.MessageInfoUpdate($"第{i + 1}层曝光完成，开始动作");
                    _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, platformPos, _config.motionConfig.motionSpeedConfig.platformSpeed, ClassValue.EnableSimulate);//平台下沉
                    _pmc.PTP_Pos_Speed_mm(AxesEnum.Levelbox, levelboxPos, _config.motionConfig.motionSpeedConfig.levelBoxSpeed, ClassValue.EnableSimulate);//浮筒下沉 
                    Thread.Sleep(TimeSpan.FromSeconds(0.3));
                    if (i > _config.levelControlParas.startFFLC) { Thread.Sleep((int)Math.Round(1000 * _config.levelControlParas.levelWaitTime)); }
                    Task<bool> t_ScapterMove = new Task<bool>(() =>_pmc.MoveScraper2Side(!scraperForward, sSpeed, ClassValue.EnableSimulate));
                    t_ScapterMove.Start();
                    while (logCount2++ < scrapterWaitTime / logInterval)
                    {
                        //循环记录曝光期间的负压情况
                        layerLog.scraperMoveVaCuum.Add(Math.Round(_pmc.GetAD(ADEnum.ScraperVacuum), 0));
                        Thread.Sleep(logInterval);
                    }
                    if (layerLog.scraperStayVaCuum.Count > 0)
                    { layerLog.scraperVacuum = layerLog.scraperStayVaCuum[0]; }
                    t_ScapterMove.Wait();
                    if (!t_ScapterMove.Result)
                    {
                        return new PrintResult()
                        {
                            isSuccess = false,
                            printLogs = this.printLog,
                            message = "打印失败，刮刀运动控制异常"
                        };
                    }
                    scraperForward = !scraperForward;
                    if (i > _config.levelControlParas.startFFLC) { Thread.Sleep((int)Math.Round(1000 * _config.levelControlParas.levelWaitTime)); }
                }
                else//免刮刀
                {
                    double scrapterWaitTime = 1000 * ((_pmc._config.scraperConfig.outPos - _pmc._config.scraperConfig.inPos) / (_config.motionConfig.motionSpeedConfig.scraperSpeed * 10000)) + 700;

                    if (i > _config.motionConfig.scrapterLessConfig.scraperLessStartLayer)
                    {
                        //_pmc.MessageInfoUpdate($"第{i + 1}层曝光完成，开始动作");
                        _pmc.PTP_Pos_Speed_mm(AxesEnum.Levelbox, levelboxPos, _config.motionConfig.motionSpeedConfig.levelBoxSpeed, ClassValue.EnableSimulate);//浮筒动作 
                        _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, platformPos + _config.motionConfig.scrapterLessConfig.platformOverDownNoUseScraper, _config.motionConfig.scrapterLessConfig.platformDownSpeedNoUseScraper, ClassValue.EnableSimulate);//平台下沉
                        double downTime = 0.4 + (_config.printConfig.layerThickness + _config.motionConfig.scrapterLessConfig.platformOverDownNoUseScraper)  / _config.motionConfig.scrapterLessConfig.platformDownSpeedNoUseScraper;
                        Thread.Sleep(TimeSpan.FromSeconds(downTime ));
                        double upTime = 1.3 + _config.motionConfig.scrapterLessConfig.platformOverDownNoUseScraper  / _config.motionConfig.scrapterLessConfig.platformUpSpeedNoUseScraper;
                        _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, platformPos, _config.motionConfig.scrapterLessConfig.platformUpSpeedNoUseScraper, ClassValue.EnableSimulate);//平台上浮
                        Thread.Sleep(TimeSpan.FromSeconds(upTime ));
                    }
                    else
                    {
                        //_pmc.MessageInfoUpdate($"第{i + 1}层曝光完成，开始动作");
                        _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, platformPos, _config.motionConfig.motionSpeedConfig.platformSpeed, ClassValue.EnableSimulate);//平台下沉
                        _pmc.PTP_Pos_Speed_mm(AxesEnum.Levelbox, levelboxPos, _config.motionConfig.motionSpeedConfig.levelBoxSpeed, ClassValue.EnableSimulate);//浮筒下沉 
                        Thread.Sleep(TimeSpan.FromSeconds(0.2));
                        //_pmc.MoveScraper2Side(!scraperForward, _config.motionConfig.motionSpeedConfig.scraperSpeed, isSimulate);//真空涂敷
                        Task<bool> t_ScapterMove = new Task<bool>(() =>
                    _pmc.MoveScraper2Side(!scraperForward, _config.motionConfig.motionSpeedConfig.scraperSpeed, ClassValue.EnableSimulate));
                        t_ScapterMove.Start();
                        while (logCount2++ < scrapterWaitTime / logInterval)
                        {
                            //循环记录曝光期间的负压情况
                            layerLog.scraperMoveVaCuum.Add(Math.Round(_pmc.GetAD(ADEnum.ScraperVacuum), 0));
                            Thread.Sleep(logInterval);
                        }
                        layerLog.scraperVacuum = layerLog.scraperMoveVaCuum[0];
                        scraperForward = !scraperForward;
                        t_ScapterMove.Wait();
                        if (!t_ScapterMove.Result)
                        {
                            return new PrintResult()
                            {
                                isSuccess = false,
                                printLogs = this.printLog,
                                message = "打印失败，刮刀运动控制异常"
                            };
                        } 
                    }
                }
                printLog.Add(layerLog);
                if (!ClassValue.EnableSimulate)
                { ptLog.Info(printLog[printLog.Count - 1].ToLogger()); }

                if (isMES) { _ea.GetEvent<UpdatePrintProgressEvent>().Publish(new JobProgress() { 
                    printedLayers = i+1,
                    totalLayers = maxslice,
                    startLayers = 1,
                    printedVolume = Math.Round(printedVolumnCount,0),
                    totalHeight = Math.Round(maxslice * _config.printConfig.layerThickness,2),
                    printedHeight = Math.Round((i+1) * _config.printConfig.layerThickness,2),
                    isPaused = false
                }); }

                _ea.GetEvent<PrintTaskChartUpdate>().Publish(i);
                i++;
                taskSlicei.Wait();
                if (AskedStop) {
                    using (FileStream fs = new FileStream($"{currtaskLogFolder}\\printlog.json", FileMode.OpenOrCreate))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine(JsonConvert.SerializeObject(printLog, Newtonsoft.Json.Formatting.Indented));
                        }
                    }
                    LiteDBHelper.PrintTaskDBInfo.UpDatePrintTaskProgress(this.printtask_GUID.ToString(), i + 1);
                    MessageInfoLogUpdate_Thread($"{printTaskName}打印任务被终止于第{i}层");
                    break; }
                

            }

            _pmc.MoveScraper2Side(false, 130, ClassValue.EnableSimulate);
            if (!ClassValue.EnableSimulate)
            {
                if (!_pmc.IsScraper2Side(false) )//若完成时刮刀不在外侧则移动到外侧
                {
                    _pmc.MoveScraper2Side(false, 130, ClassValue.EnableSimulate);
                }
            }
            if (!_pmc.IsScraper2Side(false))//若完成时刮刀不在外侧则移动到外侧
            {
                int ixx = 1;
            }
            _pmc.PTP_Pos_Speed_mm(AxesEnum.Plaform, -120, 100, ClassValue.EnableSimulate);//平台上升到高位
            while (!ClassValue.EnableSimulate && Math.Abs(_pmc.GetAxisPos_MM(AxesEnum.Plaform) + 120) > 0.1)
            {
                Thread.Sleep(100);
            }
            if (!ClassValue.EnableSimulate)
            {
                string rfidStr = $"{plateNum},{Math.Round(volumeCount * 0.0012, 0).ToString("000")}";
                _pmc.RfidWrite(RFIDEnum.PlateRFID, rfidStr);
                if (_pmc.RfidRead(RFIDEnum.PlateRFID).Item2 != rfidStr)
                {
                    _pmc.RfidWrite(RFIDEnum.PlateRFID, rfidStr);
                    MessageInfoLogUpdate_Thread($"网板RFID[{rfidStr}]");
                }
                if (_pmc.RfidRead(RFIDEnum.PlateRFID).Item2 != rfidStr)
                {
                    MessageInfoLogUpdate_Thread("网板RFID更新失败");
                }
            }
            
            printEndTime = DateTime.Now;
            printDuration = printEndTime - printStartTime;
            var valueRes = GetValueChange(_config.levelControlParas.startFFLC, printLog.Count - _config.levelControlParas.startFFLC - 1);
            
            using (FileStream fs = new FileStream(Path.Combine(Application.StartupPath, $"{currtaskLogFolder}\\printtaskinfo.txt"), FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine($"结束时间:{printEndTime}");
                    if (AskedStop)
                    {
                        sw.WriteLine($"是否打印完成：否，停止层数{i}");
                    }
                    else
                    {
                        sw.WriteLine($"是否打印完成：是");
                    }
                    sw.WriteLine($"打印耗时:{printDuration.ToString()}");
                    if (valueRes)
                    {
                        sw.WriteLine($"温度变化极差{valueChanges.tempRange.ToString("0.0℃")}");
                        sw.WriteLine($"温度最大变化{valueChanges.tempSlope.ToString("0.0℃")}/3layers At layer index {valueChanges.tempAttIndex}");
                        sw.WriteLine($"液位变化极差{valueChanges.levelRange.ToString("0.00mm")}");
                        sw.WriteLine($"液位最大变化{valueChanges.levelSlope.ToString("0.00mm")}/3layers At layer index {valueChanges.levelAttIndex}");
                        sw.WriteLine($"负压变化极差{valueChanges.vaccumRange.ToString("0 Pa")}");
                        sw.WriteLine($"负压最大变化{valueChanges.vaccumSlope.ToString("0 Pa")}/3layers At layer index {valueChanges.vaccumAttIndex}");
                    }
                }
            }
            using (FileStream fs = new FileStream($"{currtaskLogFolder}\\printlog.json", FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(printLog, Newtonsoft.Json.Formatting.Indented));
                }
            }

            using (FileStream fs = new FileStream($"{currtaskLogFolder}\\printtaskinfo.txt", FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string taskinfostring = sr.ReadToEnd();
                    try
                    {
                        if (!AskedStop)
                        {
                            if (!AngleDLP.LiteDBHelper.PrintTaskDBInfo.UpDatePrintTaskFinishTime(printtask_GUID.ToString(), printEndTime, taskinfostring))
                            {
                                MessageInfoLogUpdate_Thread("打印数据库更新失败。", LogLevel.Error);
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        MessageInfoLogUpdate_Thread(ex.ToString(), LogLevel.Error);
                    }

                }
            }
            if (isMES && !AskedStop)
            {
                if (isMES)
                {
                    _ea.GetEvent<UpdatePrintJobEvent>().Publish(new JobEventItem()
                    {
                        @event = new JobEvent()
                        {
                            time = TimeHelper.GetNewTimeString(),
                            progress = new JobProgress()
                            {
                                printedLayers = maxslice,
                                totalLayers = maxslice,
                                startLayers = 1,
                                printedVolume = volumeCount,
                                totalHeight = Math.Round(maxslice * _config.printConfig.layerThickness, 2),
                                printedHeight = Math.Round(maxslice * _config.printConfig.layerThickness, 2),
                                isPaused = false
                            },
                            type = JobEventType.end,
                            completedInstanceCount = layoutCount,
                            result = new Result()
                            {
                                type = JobResultType.successful
                            }
                        }
                        
                    } ); ;
                }
            }
            LiteDBHelper.PrintTaskDBInfo.UpDatePrintTaskProgress(this.printtask_GUID.ToString(), maxslice);
            _pmc.LightControl(0, 0, 0, 0, 0, true);
            
            if (AskedStop)
            {
                if (isMES)
                {
                    _ea.GetEvent<UpdatePrintJobEvent>().Publish(new JobEventItem()
                    {
                        @event = new JobEvent()
                        {
                            time = TimeHelper.GetNewTimeString(),
                            progress = new JobProgress()
                            {
                                printedLayers = i + 1,
                                totalLayers = maxslice,
                                startLayers = 1,
                                printedVolume = volumeCount,
                                totalHeight = Math.Round(maxslice * _config.printConfig.layerThickness, 2),
                                printedHeight = Math.Round((i + 1) * _config.printConfig.layerThickness, 2),
                                isPaused = true
                            },
                            type = JobEventType.end,
                            completedInstanceCount = 0,
                            result = new Result()
                            {
                                type = JobResultType.canceled
                            }
                        }

                    }); ;
                }
                
                new Task(() => DialogHelper.ShowMessageDialog("提示", $"{printTaskName} 打印任务已被终止！")).Start();
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    _ea.GetEvent<PrintProgressViewEvent>().Publish(new PrintProgressViewModel()
                    {
                        printMode = printMode,
                        progressBarVisibility = System.Windows.Visibility.Visible,
                        cColor = new SolidColorBrush(System.Windows.Media.Colors.Gray),
                        fColor = new SolidColorBrush(System.Windows.Media.Colors.Gray),
                        progress = 100,
                        layerTips = $"打印完成，耗时{printDuration.ToString()}"
                    });
                }));

                MessageInfoLogUpdate_Thread($"{printTaskName}打印完成，耗时{printDuration.ToString()}", LogLevel.Fatal);
            }

            printTaskStatus = 2;
            if (!AskedStop&&volumeCount > 100 * 1000) {
                Task.Run(() => {
                    AddResinForVolumn(volumeCount - 50 * 1000);
                });
            }
            return new PrintResult()
            {
                isSuccess = true,
                printLogs = this.printLog,
                message = "打印成功"
            };
        }
        /// <summary>
        /// 获取切层图像
        /// </summary>
        public Tuple<List<BitmapImage>, List<int>> GetSliceImage_OpenCV(int ind)
        {
            List<BitmapImage> bms = new List<BitmapImage>();
            List<int> counts = new List<int>();

            List<Mat> imgs = new List<Mat>();
            for (int i = 0; i < _ppc._config.projectorNum; i++)
            {
                imgs.Add(AngelSlicer.GetSliceImage_OpenCV(partModels, ref taskModels, ind, _sliceParas, i, connectPointPairs));
            }

            Mat th_img0 = new Mat();
            Cv2.Threshold(imgs[0], th_img0, 50, 255, ThresholdTypes.Binary);
            Mat th_img1 = new Mat();
            Cv2.Threshold(imgs[1], th_img1, 50, 255, ThresholdTypes.Binary);
            counts.Add(th_img0.CountNonZero());
            counts.Add(th_img1.CountNonZero());
            bms.Add(ImageProcessTool.MatToBitmap(imgs[0]));
            bms.Add(ImageProcessTool.MatToBitmap(imgs[1]));
            th_img0.Dispose();
            th_img1.Dispose();
            imgs[0].Dispose();
            imgs[1].Dispose();
            imgs.Clear();
            return Tuple.Create<List<BitmapImage>, List<int>>(bms, counts);
        }

        public void LoadCalibrationTestData()
        {
            int grayDX = 40;
            DEMData calData;
            List<AltitudePoint> pointList = new List<AltitudePoint>();
            XmlDocument doc = new XmlDocument();
            doc.Load("GrayTestResult.xml");
            XmlNode xn = doc.SelectSingleNode("root");
            // 得到根节点的所有子节点
            XmlNodeList xnl = xn.ChildNodes;

            foreach (XmlNode xn1 in xnl)
            {
                GrayTestItem grayTestItem = new GrayTestItem();
                // 将节点转换为元素，便于得到节点的属性值
                XmlElement xe = (XmlElement)xn1;
                // 得到Type和ISBN两个属性的属性值
                grayTestItem.X = Convert.ToDouble(xe.GetAttribute("X").ToString()) / grayDX;
                grayTestItem.Y = Convert.ToDouble(xe.GetAttribute("Y").ToString()) / grayDX;
                grayTestItem.I = Convert.ToDouble(xe.GetAttribute("i").ToString());
                AltitudePoint altitudePoint = new AltitudePoint(grayTestItem.X, grayTestItem.Y, grayTestItem.I);
                pointList.Add(altitudePoint);
            }

            InterpolationData interpolation = new InterpolationData(pointList, 2160 / grayDX, 3840 / grayDX);
            bool testbool = interpolation.IsRandomPointsOK();
            try
            {
                calData = new DEMData(interpolation.GetInterpolationData(1));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }
            interpolation.GetDrawingInfo();
            calData.DEMBitmap().Save("DEM_Bitimage.bmp");
            List<GrayTestItem> results = new List<GrayTestItem>();


            Console.WriteLine("最高值：" + calData.max.ToString("0.##") + "\n最低值：" + calData.min.ToString("0.##"));
        }
        public void BaseCalibrationTest()
        {
            int DX = 40;
            DEMData calData;
            List<AltitudePoint> pointList = new List<AltitudePoint>();
            //XmlDocument doc = new XmlDocument();
            //doc.Load("BaseTestResult-p1.xml");
            //XmlNode xn = doc.SelectSingleNode("root");
            //// 得到根节点的所有子节点
            //XmlNodeList xnl = xn.ChildNodes;
            List<GrayTestItem> items;
            using (FileStream fs = new FileStream(".\\LIGHT.csv", FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    //记录每次读取的一行记录
                    string strLine = "";
                    //记录每行记录中的各字段内容
                    string[] aryLine = null;
                    string[] tableHead = null;
                    //标示列数
                    int columnCount = 0;
                    //标示是否是读取的第一行
                    bool IsFirst = true;
                    //逐行读取CSV中的数据
                    while ((strLine = sr.ReadLine()) != null)
                    {

                        aryLine = strLine.Split(',');
                        AltitudePoint altitudePoint = new AltitudePoint(int.Parse(aryLine[0]) / DX, int.Parse(aryLine[1]) / DX, double.Parse(aryLine[2]));
                        pointList.Add(altitudePoint);

                    }
                    //string jsonstr = sr.ReadToEnd();

                }
            }

            //foreach (var it in items)
            //{
            //    //GrayTestItem grayTestItem = new GrayTestItem();
            //    // 将节点转换为元素，便于得到节点的属性值
            //    //XmlElement xe = (XmlElement)xn1;
            //    // 得到Type和ISBN两个属性的属性值
            //    AltitudePoint altitudePoint = new AltitudePoint(it.X/DX, it.Y/DX, it.I);
            //    pointList.Add(altitudePoint);
            //}

            InterpolationData interpolation = new InterpolationData(pointList, 2160 / DX, 3840 / DX);
            bool testbool = interpolation.IsRandomPointsOK();
            try
            {
                calData = new DEMData(interpolation.GetInterpolationData(1));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }
            interpolation.GetDrawingInfo();
            Bitmap bm = calData.DEMBitmap();
            bm.RotateFlip(RotateFlipType.Rotate270FlipNone);
            bm.Save("Light_DEM_Bitimage.bmp");
            //BaseCalib calibRes = new BaseCalib();
            //calibRes.max = Math.Round(calData.MaxValue, 3);
            //calibRes.min = Math.Round(calData.MinValue, 3);
            //for (int i = 0; i < 54; i++)
            //{
            //    List<double> vs = new List<double>();
            //    for (int j = 0; j < 96; j++)
            //    {
            //        vs.Add(Math.Round(calData.altitudeData[i, j], 3));
            //    }
            //    calibRes.values.Add(vs);
            //}
            //string resStr = Newtonsoft.Json.JsonConvert.SerializeObject(calibRes);
            //using (FileStream fs = new FileStream("LightCalib.json", FileMode.Create))
            //{
            //    using (StreamWriter sw = new StreamWriter(fs))
            //    {
            //        sw.WriteLine(resStr);
            //    }
            //}

            //Console.WriteLine("最高值：" + calData.MaxValue.ToString("0.##") + "\n最低值：" + calData.MinValue.ToString("0.##"));
        }
        public void SaveBaseData()
        {
            int startX = 180;
            int startY = 120;
            int diffX = 600;
            int diffY = 600;
            int rowNum = 7;
            int clumnNum = 4;
            int imgCount = 0;
            List<List<double>> zss = new List<List<double>>();
            zss.Add(new List<double>() { 2.26, 2.22, 2.16, 2.08, 2.04, 1.96, 1.92 });
            zss.Add(new List<double>() { 2.2, 2.18, 2.16, 2.10, 2.04, 1.94, 1.88 });
            zss.Add(new List<double>() { 2.3, 2.26, 2.18, 2.10, 2.06, 1.94, 1.86 });
            zss.Add(new List<double>() { 2.34, 2.28, 2.20, 2.16, 2.12, 1.92, 1.88 });
            List<GrayTestItem> items = new List<GrayTestItem>();
            for (int i = 0; i < clumnNum; i++)
            {
                for (int j = 0; j < rowNum; j++)
                {
                    items.Add(new GrayTestItem()
                    {
                        X = startX + i * diffX,
                        Y = startY + j * diffY,
                        I = zss[i][j]
                    });
                }
            }
            using (FileStream fs = new FileStream(".\\test.json", FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                    sw.WriteLine(jsonstr);
                }
            }
        }
        private class GrayTestItem
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double I { get; set; }

        }
        public bool hasStandmodel = false;
        public List<int> GetLayoutIndex()
        {
            //如果有标准件，应该在第一个
            int startInd = 0;
            if (hasStandmodel) { startInd = 1; }
            List<int> layoutIndex = new List<int>();
            //如果数量大于32
            if (partModels.Count < 42)
            {
                int ind = 0;
                foreach (var partModel in partModels)
                {
                    layoutIndex.Add(ind++);
                }
            }
            else
            {
                //前面32个按y宽度升序排列
                //后面按自然顺序排列
                //前32个冒泡排序
                //初始化列表
                int ind = 0;
                foreach (var partModel in partModels)
                {
                    layoutIndex.Add(ind++);
                }
                for (int i = 40; i > startInd; i--)
                {
                    for (int j = i; j > startInd - 1; j--)
                    {
                        if (partModels[layoutIndex[j]].DX > partModels[layoutIndex[j + 1]].DX)//j和j+1比较,
                        {
                            //互换j和j+1
                            int temp = layoutIndex[j];
                            layoutIndex[j] = layoutIndex[j + 1];
                            layoutIndex[j + 1] = temp;
                        }
                    }
                }
            }
            return layoutIndex;
        }

        public class ClumnLayout
        {
            public bool isR0 = true;
            public bool isEmpty = true;
            public int projectorIndex = 0;
            public int clumnIndex = 0;
            public double X_Offset = 0;
            public int startIndex = 0;
            public int endIndex = 0;
            public double maxLeft = 0;
            public double maxRight = 0;
            public List<double> Y_Offsets = new List<double>();
            public List<bool> rotDirs = new List<bool>();
        }
        public bool GetColumnR0(ref ClumnLayout clumnLayout, List<int> layoutIndex, int standPos = -1)
        {
            //标准件位置, -1 ,0,1,2 // 无，始，中，末
            //始，放在第一个，然后下一个再按layoutIndex 开始
            //中，如果当前剩余数量大于4个，放在第4个，
            //以上两种情况都直接排版
            //中，如果当前数量小于4个，
            //排版完所有的，再放在最后一个,
            //末，如果当前数量大于7个
            double tempScaleX = _config.accuracyParas.scaleCompX;
            double tempScaleY = _config.accuracyParas.scaleCompY;
            if (clumnLayout.startIndex > partModels.Count - 1)
            {
                clumnLayout.endIndex = clumnLayout.startIndex - 1;
                return false;
            }

            //task1 从上到下获取Y_Offsets
            //获取endIndex
            //获取maxLeft和maxRight

            //定位第一个
            int currentColumnInd = 0;
            clumnLayout.Y_Offsets.Add(_ppc._config.projectorConfigs[clumnLayout.projectorIndex].layoutPixPadding[3] - partModels[layoutIndex[clumnLayout.startIndex]].bondsMin[1] * tempScaleY);
            clumnLayout.maxLeft = -partModels[layoutIndex[clumnLayout.startIndex]].bondsMin[0] * tempScaleX;
            clumnLayout.maxRight = partModels[layoutIndex[clumnLayout.startIndex]].bondsMax[0] * tempScaleX;
            clumnLayout.endIndex = clumnLayout.startIndex;
            int prevIndex = layoutIndex[clumnLayout.startIndex];
            partModels[layoutIndex[clumnLayout.startIndex]].layoutRow = currentColumnInd++;
            for (int i = clumnLayout.startIndex + 1; i < partModels.Count; i++)
            {
                int currIndex = layoutIndex[i];
                //判断是否越界
                var res = GetModelYOffset2(partModels[prevIndex].layoutYRanges, partModels[currIndex].layoutYRanges);
                double yOffset = clumnLayout.Y_Offsets[clumnLayout.Y_Offsets.Count - 1] + res.Item1;
                yOffset *= tempScaleY;
                if (yOffset + partModels[currIndex].bondsMax[1] * tempScaleY + _ppc._config.projectorConfigs[clumnLayout.projectorIndex].layoutPixPadding[1] >
                    _ppc._config.projectorConfigs[clumnLayout.projectorIndex].printerPixHeight * (_ppc._config.projectorConfigs[clumnLayout.projectorIndex].projectorPixScaleY))
                {
                    break;
                }
                partModels[currIndex].layoutRow = currentColumnInd++;
                clumnLayout.Y_Offsets.Add(yOffset);
                partModels[prevIndex].connectPointBackward_1 = res.Item2;
                partModels[currIndex].connectPointForward_1 = res.Item3;
                partModels[prevIndex].connectPointBackward_2 = res.Item4;
                partModels[currIndex].connectPointForward_2 = res.Item5;
                if (partModels[currIndex].bondsMax[0] * tempScaleX > clumnLayout.maxRight) { clumnLayout.maxRight = partModels[currIndex].bondsMax[0] * tempScaleX; }
                if (-partModels[currIndex].bondsMin[0] * tempScaleX > clumnLayout.maxLeft) { clumnLayout.maxLeft = -partModels[currIndex].bondsMin[0] * tempScaleX; }
                prevIndex = currIndex;
                clumnLayout.endIndex = i;
            }

            clumnLayout.isEmpty = false;
            return true;
        }

        public Tuple<double, System.Drawing.PointF, System.Drawing.PointF, System.Drawing.PointF, System.Drawing.PointF> GetModelYOffset2(List<List<List<double>>> preYRange, List<List<List<double>>> curYRange)
        {
            double res = 0;
            double cDist_1 = 0;
            double cDist_2 = 0;
            int cL = 0;
            int cXI_1 = 0;
            int cXI_2 = 0;
            for (int l = 0; l < 6; l++)
            {
                for (int i = 0; i < 400; i++)
                {

                    double preMaxY = preYRange[l][i][1];//上一个的最大
                    double currMinY = curYRange[l][i][0];//下一个的最小
                    double currDist = preMaxY - currMinY;
                    if (res < currDist)
                    {
                        res = currDist;
                    }
                    if (l == cL)
                    {
                        if (cDist_1 < currDist && i >100 && i <150)
                        {
                            cXI_1 = i;
                            cDist_1 = currDist;
                        }
                        if (cDist_2 < currDist && i > 250 && i < 300)
                        {
                            cXI_2 = i;
                            cDist_2 = currDist;
                        }
                    }

                }
            }

            return new Tuple<double, System.Drawing.PointF, System.Drawing.PointF, System.Drawing.PointF, System.Drawing.PointF>(
                res + 0.5,
                new System.Drawing.PointF((float)(-40.2 + cXI_1 * 0.2), (float)preYRange[cL][cXI_1][1]),
                new System.Drawing.PointF((float)(-40.2 + cXI_1 * 0.2), (float)curYRange[cL][cXI_1][0]),
                new System.Drawing.PointF((float)(-40.2 + cXI_2 * 0.2), (float)preYRange[cL][cXI_2][1]),
                new System.Drawing.PointF((float)(-40.2 + cXI_2 * 0.2), (float)curYRange[cL][cXI_2][0]));
        }

        public Tuple<double, System.Drawing.PointF, System.Drawing.PointF, System.Drawing.PointF, System.Drawing.PointF> GetModelYOffsetRotate(int prevind, bool prevDir, int currind, bool currDir)//X考虑正转，反转
        {
            //先进行旋转，将值赋layoutYRanges，再调用GetModelYOffset2
            List<List<List<double>>> preYRange = new List<List<List<double>>>();
            List<List<List<double>>> currYRange = new List<List<List<double>>>();
            //for (int l = 0; l < 6; l++)
            //{
            //    List < List<double>> x2x = new List<List<double>>();
            //    for (int i = 0; i < 400; i++)
            //    {
            //        List<double> x1x = new List<double>();
            //        x1x.Add(0);
            //        x1x.Add(0);
            //        x2x.Add(x1x);
            //    }
            //    preYRange.Add(x2x);
            //    currYRange.Add(x2x);
            //}
            for (int l = 0; l < 6; l++)
            {
                List<List<double>> x2xp = new List<List<double>>();
                List<List<double>> x2xc = new List<List<double>>();
                for (int i = 0; i < 400; i++)
                {
                    //lxh mark 计算错误
                    if (prevDir)
                    {
                        //preYRange[l][i] = partModels[prevind].layoutXRanges[l][399 - i];
                        //preYRange[l][i][1] = partModels[prevind].layoutXRanges[l][399 - i][1];
                        //preYRange[l][i][0] = partModels[prevind].layoutXRanges[l][399 - i][0];
                        List<double> x1x = new List<double>();
                        x1x.Add((double)partModels[prevind].layoutXRanges[l][399 - i][0]);
                        x1x.Add((double)partModels[prevind].layoutXRanges[l][399 - i][1]);
                        x2xp.Add(x1x);
                    }
                    else
                    {
                        //if (partModels[prevind].layoutXRanges[l][i][0] > 40)
                        //{
                        //    preYRange[l][i][0] = 40.2;
                        //    preYRange[l][i][1] = -40.2;
                        //}
                        //else
                        //{
                        //    preYRange[l][i][1] = partModels[prevind].layoutXRanges[l][i][0];
                        //    preYRange[l][i][0] = partModels[prevind].layoutXRanges[l][i][1];
                        //}
                        //preYRange[l][i][1] = -partModels[prevind].layoutXRanges[l][i][0];
                        //preYRange[l][i][0] = -partModels[prevind].layoutXRanges[l][i][1];

                        List<double> x1x = new List<double>();
                        x1x.Add((double)-partModels[prevind].layoutXRanges[l][i][1]);
                        x1x.Add((double)-partModels[prevind].layoutXRanges[l][i][0]);
                        x2xp.Add(x1x);

                    }

                    if (currDir)
                    {
                        //currYRange[l][i][1] = partModels[currind].layoutXRanges[l][399 - i][1];
                        //currYRange[l][i][0] = partModels[currind].layoutXRanges[l][399 - i][0];
                        List<double> x1x = new List<double>();
                        x1x.Add((double)partModels[currind].layoutXRanges[l][399 - i][0]);
                        x1x.Add((double)partModels[currind].layoutXRanges[l][399 - i][1]);
                        x2xc.Add(x1x);
                    }
                    else
                    {
                        //if (partModels[currind].layoutXRanges[l][i][0] > 40)
                        //{
                        //    currYRange[l][i][0] = 40.2;
                        //    currYRange[l][i][1] = -40.2;
                        //}
                        //else
                        //{
                        //    currYRange[l][i][1] = partModels[currind].layoutXRanges[l][i][0];
                        //    currYRange[l][i][0] = partModels[currind].layoutXRanges[l][i][1];
                        //}
                        //currYRange[l][i][1] = -partModels[currind].layoutXRanges[l][i][0];
                        //currYRange[l][i][0] = -partModels[currind].layoutXRanges[l][i][1];
                        List<double> x1x = new List<double>();
                        x1x.Add((double)-partModels[currind].layoutXRanges[l][i][1]);
                        x1x.Add((double)-partModels[currind].layoutXRanges[l][i][0]);
                        x2xc.Add(x1x);
                    }

                }
                preYRange.Add(x2xp);
                currYRange.Add(x2xc);
            }

            return GetModelYOffset2(preYRange, currYRange);
        }
        public bool GetColumnR90(ref ClumnLayout clumnLayout, List<int> layoutIndex, int standPos = -1)
        {
            //标准件位置, -1 ,0,1,2 // 无，始，中，末
            double tempScaleX = _config.accuracyParas.scaleCompX;
            double tempScaleY = _config.accuracyParas.scaleCompY;

            if (clumnLayout.startIndex > partModels.Count - 1)
            {
                clumnLayout.endIndex = clumnLayout.startIndex - 1;
                return false;
            }

            //task1 从上到下获取Y_Offsets
            //获取endIndex
            //获取maxLeft和maxRight

            //定位第一个

            clumnLayout.Y_Offsets.Add(_ppc._config.projectorConfigs[clumnLayout.projectorIndex].layoutPixPadding[3] - partModels[layoutIndex[clumnLayout.startIndex]].bondsMin[0] * tempScaleX);
            clumnLayout.rotDirs.Add(true);
            clumnLayout.maxLeft = partModels[layoutIndex[clumnLayout.startIndex]].bondsMin[1] * tempScaleY;
            clumnLayout.maxRight = -partModels[layoutIndex[clumnLayout.startIndex]].bondsMax[1] * tempScaleY;
            clumnLayout.endIndex = clumnLayout.startIndex;
            int prevIndex = layoutIndex[clumnLayout.startIndex];
            bool currRotateDir = false; //-Z
            for (int i = clumnLayout.startIndex + 1; i < partModels.Count; i++)
            {

                int currIndex = layoutIndex[i];
                //判断是否越界
                var res = GetModelYOffsetRotate(prevIndex, !currRotateDir, currIndex, currRotateDir);
                double yOffset = clumnLayout.Y_Offsets[clumnLayout.Y_Offsets.Count - 1] + res.Item1;
                yOffset *= tempScaleX;
                double currUpper = partModels[currIndex].bondsMax[0] * tempScaleX;
                if (!currRotateDir)
                {
                    currUpper = -partModels[currIndex].bondsMin[0] * tempScaleX;
                }
                if (yOffset + currUpper + _ppc._config.projectorConfigs[clumnLayout.projectorIndex].layoutPixPadding[1] >
                    _ppc._config.projectorConfigs[clumnLayout.projectorIndex].printerPixHeight * (_ppc._config.projectorConfigs[clumnLayout.projectorIndex].projectorPixScaleY))
                {
                    break;
                }
                partModels[prevIndex].connectPointBackward_1 = res.Item2;
                partModels[currIndex].connectPointForward_1 = res.Item3; 
                partModels[prevIndex].connectPointBackward_2 = res.Item4;
                partModels[currIndex].connectPointForward_2 = res.Item5;
                clumnLayout.Y_Offsets.Add(yOffset);
                clumnLayout.rotDirs.Add(currRotateDir);
                double currLeft = partModels[currIndex].bondsMax[1] * tempScaleY;
                double currRight = -partModels[currIndex].bondsMin[1] * tempScaleY;
                if (!currRotateDir)
                {
                    currLeft = -partModels[currIndex].bondsMin[1] * tempScaleY;
                    currRight = partModels[currIndex].bondsMax[1] * tempScaleY;
                }
                if (currLeft > clumnLayout.maxRight) { clumnLayout.maxRight = currLeft; }
                if (currRight > clumnLayout.maxLeft) { clumnLayout.maxLeft = currRight; }
                prevIndex = currIndex;
                clumnLayout.endIndex = i;
                currRotateDir = !currRotateDir;
            }

            clumnLayout.isEmpty = false;
            return true;
        }
        public List<int> AutoLayout_LXH_New()
        {
            if (isMSS_Computed == 1)
            {
                MessageInfoLogUpdate_Thread($"切层计算中，无法排版", LogLevel.Warn);
                return new List<int>();
            }
            if (printTaskStatus != 0)
            {
                MessageInfoLogUpdate_Thread($"已开始打印，无法排版", LogLevel.Warn);
                return new List<int>();
            }
            MessageInfoLogUpdate_Thread($"开始排版", LogLevel.Info);
            double safeRangeOffset = 1.3;
            //对所有要打印的模型计算切层辅助文件
            int preTreatCount = 0;
            foreach (var pm in partModels)
            {
                pm.GetLayoutXYRange();

                ProgressViewModel pvm2 = new ProgressViewModel()
                {
                    topic = "预处理模型",
                    value = (int)Math.Round((double)100 * (preTreatCount + 1) / partModels.Count),
                    info = $"正在处理第{preTreatCount++ + 1}个模型",
                    visibility = System.Windows.Visibility.Visible
                };
                _ea.GetEvent<ProgressViewEvent>().Publish(pvm2);

            }
            _ea.GetEvent<ProgressViewEvent>().Publish(new ProgressViewModel());

            List<int> layoutIndex = GetLayoutIndex();//排除标准件，按顺序排版，排版后没有用了
            //按layoutIndex排版
            List<ClumnLayout> clumnLayouts = new List<ClumnLayout>();

            //前两行排版
            int pievEndIndex = -1;
            //if (hasStandmodel) { pievEndIndex = 0; }
            int clumnCount = 0;

            for (int cI = 0; cI < 2; cI++)
            {
                for (int pI = 0; pI < 2; pI++)
                {
                    ClumnLayout clumnLayout = new ClumnLayout()
                    {
                        isR0 = true,
                        projectorIndex = pI,
                        clumnIndex = cI,
                        startIndex = pievEndIndex + 1
                    };
                    _ = GetColumnR0(ref clumnLayout, layoutIndex);
                    pievEndIndex = clumnLayout.endIndex;
                    clumnLayouts.Add(clumnLayout);
                    clumnCount++;
                }
            }

            for (int pI = 0; pI < 2; pI++)
            {
                //尝试R0
                ClumnLayout clumnLayout = new ClumnLayout()
                {
                    isR0 = true,
                    projectorIndex = pI,
                    clumnIndex = 2,
                    startIndex = pievEndIndex + 1
                };
                _ = GetColumnR0(ref clumnLayout, layoutIndex);
                //检验宽度
                double first2Width = 0;
                foreach (ClumnLayout cl in clumnLayouts)
                {
                    if (cl.projectorIndex == pI)
                    {
                        first2Width += cl.maxLeft;
                        first2Width += cl.maxRight;
                    }
                }
                clumnLayouts.Add(clumnLayout);
                pievEndIndex = clumnLayout.endIndex;
                if (first2Width + 2 * safeRangeOffset + clumnLayout.maxLeft + clumnLayout.maxRight + _ppc._config.projectorConfigs[pI].layoutPixPadding[0] + _ppc._config.projectorConfigs[pI].layoutPixPadding[2]
                    > _ppc._config.projectorConfigs[pI].printerPixWidth * (_ppc._config.projectorConfigs[pI].projectorPixScaleX))
                {
                    clumnLayouts.RemoveAt(clumnLayouts.Count - 1);
                    pievEndIndex = clumnLayout.startIndex - 1;
                    //尝试横向排版
                    ClumnLayout clumnLayout90 = new ClumnLayout()
                    {
                        isR0 = false,
                        projectorIndex = pI,
                        clumnIndex = 2,
                        startIndex = pievEndIndex + 1
                    };
                    _ = GetColumnR90(ref clumnLayout90, layoutIndex);
                    //再次校验宽度是否合格
                    if (first2Width + clumnLayout90.maxLeft + clumnLayout90.maxRight + 2 * safeRangeOffset + _ppc._config.projectorConfigs[pI].layoutPixPadding[0] + _ppc._config.projectorConfigs[pI].layoutPixPadding[2]
                        < _ppc._config.projectorConfigs[pI].printerPixWidth * (_ppc._config.projectorConfigs[pI].projectorPixScaleX))
                    {
                        clumnLayouts.Add(clumnLayout90);
                        pievEndIndex = clumnLayout90.endIndex;
                    }
                    else
                    {
                        clumnLayout90.isEmpty = true;
                        clumnLayout90.Y_Offsets.Clear();
                        clumnLayouts.Add(clumnLayout90);
                        pievEndIndex = clumnLayout90.startIndex - 1;
                    }
                }
            }

            //计算X_Offset
            if (!clumnLayouts[0].isEmpty) { clumnLayouts[0].X_Offset = _ppc._config.projectorConfigs[0].printerPixWidth * _ppc._config.projectorConfigs[0].projectorPixScaleX - _ppc._config.projectorConfigs[0].layoutPixPadding[2] - clumnLayouts[0].maxRight; };
            if (!clumnLayouts[1].isEmpty) { clumnLayouts[1].X_Offset = _ppc._config.projectorConfigs[1].layoutPixPadding[0] + clumnLayouts[1].maxLeft; };
            if (!clumnLayouts[2].isEmpty) { clumnLayouts[2].X_Offset = clumnLayouts[0].X_Offset - clumnLayouts[0].maxLeft - safeRangeOffset - clumnLayouts[2].maxRight; };
            if (!clumnLayouts[3].isEmpty) { clumnLayouts[3].X_Offset = clumnLayouts[1].X_Offset + clumnLayouts[1].maxRight + safeRangeOffset + clumnLayouts[3].maxLeft; };
            if (!clumnLayouts[4].isEmpty) { clumnLayouts[4].X_Offset = clumnLayouts[2].X_Offset - clumnLayouts[2].maxLeft - safeRangeOffset - clumnLayouts[4].maxRight; };
            if (!clumnLayouts[5].isEmpty) { clumnLayouts[5].X_Offset = clumnLayouts[3].X_Offset + clumnLayouts[3].maxRight + safeRangeOffset + clumnLayouts[5].maxLeft; };

            //显示结果

            foreach (ClumnLayout cl in clumnLayouts)
            {
                if (!cl.isEmpty)
                {
                    for (int i = 0; i < cl.Y_Offsets.Count; i++)
                    {
                        int currModelIndex = layoutIndex[cl.startIndex + i];
                        partModels[currModelIndex].TX = cl.X_Offset + _ppc._config.projectorConfigs[cl.projectorIndex].projectorOffsetX;
                        partModels[currModelIndex].TY = cl.Y_Offsets[i];
                        partModels[currModelIndex].RZ = 0;

                        if (cl.rotDirs.Count > 0)
                        {
                            if (cl.clumnIndex == 2 && cl.rotDirs[i])
                            {
                                partModels[currModelIndex].RZ = 90;
                            }
                            if (cl.clumnIndex == 2 && !cl.rotDirs[i])
                            {
                                partModels[currModelIndex].RZ = -90;
                            }
                        }

                        Matrix3D matrix = new Matrix3D(Math.Cos(Math.PI * partModels[currModelIndex].RZ / 180), Math.Sin(Math.PI * partModels[currModelIndex].RZ / 180), 0, 0,
                                -Math.Sin(Math.PI * partModels[currModelIndex].RZ / 180), Math.Cos(Math.PI * partModels[currModelIndex].RZ / 180), 0, 0,
                                0, 0, 1, 0,
                                partModels[currModelIndex].TX, partModels[currModelIndex].TY, 0, 1);

                        layoutItems.Add(new LayoutItem()
                        {
                            Model = partModels[currModelIndex].fileName,
                            TX = Math.Round(partModels[currModelIndex].TX, 3),
                            TY = Math.Round(partModels[currModelIndex].TY, 3),
                            RZ = Math.Round(partModels[currModelIndex].RZ, 3),
                            Matrix3D = matrix
                        });

                        partModels[currModelIndex].ModelTransform = new MatrixTransform3D(matrix);
                        partModels[currModelIndex].printInWhitchProjector = cl.projectorIndex;
                        partModels[currModelIndex].layoutColumn = cl.clumnIndex;
                        //partModels[currModelIndex].layoutRow = currModelIndex;
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            //更新进度窗口
                            Transform3D temp = new MatrixTransform3D(matrix);
                            _ea.GetEvent<PartSelectByListViewEvent>().Publish(currModelIndex);
                            _ea.GetEvent<UpdateTransform3DsEvent>().Publish(new UI2PartModel(currModelIndex, temp));
                        }));

                        //System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        //{
                        //    //更新进度窗口
                        //    Transform3D temp = new MatrixTransform3D(matrix);
                        //    
                        //    
                        //}));
                        Thread.Sleep(10);
                    }
                }
            }
            //_ea.GetEvent<PartSelectByListViewEvent>().Publish(0);
            MessageInfoLogUpdate_Thread($"共排版{pievEndIndex + 1}个模型", LogLevel.Info);
            //未排版模型
            //istoprint = false
            //不显示
            List<int> unPrints = new List<int>();
            for (int i = pievEndIndex + 1; i < partModels.Count; i++)
            {
                unPrints.Add(i);
                partModels[i].isRendering = false;
                partModels[i].IsToPrint = false;
                MessageInfoLogUpdate_Thread($"{partModels[i].fileName}不打印", LogLevel.Warn);
            }

            return unPrints;
        }
        List<List<PointF>> connectPointPairs;
        public void ConnectColumn()
        {
            connectPointPairs = new List<List<PointF>>();
            var groupbyProj = partModels.Where(p => p.layoutRow > -1).GroupBy(p => p.printInWhitchProjector);
            foreach (var po in groupbyProj)
            {
                var groupbyColumn = po.GroupBy(p => p.layoutColumn);
                foreach (var co in groupbyColumn)
                {
                    var coo = co.OrderBy(s => s.layoutRow).ToList();

                    for (int i = 0; i < coo.Count - 1; i++)
                    {
                        //if (Math.Abs(partModels[i].connectPointBackward.X) < 0.1 && Math.Abs(partModels[i].connectPointBackward.Y) < 0.1)
                        //{
                        //    continue;
                        //}
                        List<PointF> pair_1 = new List<PointF>();
                        PointF temp1 = new PointF((float)partModels[coo[i].index].TX + partModels[coo[i].index].connectPointBackward_1.X,
                                                    (float)partModels[coo[i].index].TY + partModels[coo[i].index].connectPointBackward_1.Y);
                        pair_1.Add(temp1);
                        PointF temp2 = new PointF((float)partModels[coo[i + 1].index].TX + partModels[coo[i + 1].index].connectPointForward_1.X,
                                                    (float)partModels[coo[i + 1].index].TY + partModels[coo[i + 1].index].connectPointForward_1.Y);
                        pair_1.Add(temp2);
                        connectPointPairs.Add(pair_1);
                        List<PointF> pair_2 = new List<PointF>();
                        PointF temp1_2 = new PointF((float)partModels[coo[i].index].TX + partModels[coo[i].index].connectPointBackward_2.X,
                                                    (float)partModels[coo[i].index].TY + partModels[coo[i].index].connectPointBackward_2.Y);
                        pair_2.Add(temp1_2);
                        PointF temp2_2 = new PointF((float)partModels[coo[i + 1].index].TX + partModels[coo[i + 1].index].connectPointForward_2.X,
                                                    (float)partModels[coo[i + 1].index].TY + partModels[coo[i + 1].index].connectPointForward_2.Y);
                        pair_2.Add(temp2_2);
                        connectPointPairs.Add(pair_2);
                        //ConnectTheTwoModels(, coo[i + 1].index);
                    }
                }


            }

        }


        //List中存入的是Object类型的数组，数组中第一个元素存的是文件名（带文件后缀），数组中第二个元素存的是MemoryStream（流）
        private byte[] CreateZipStream(List<Object[]> lstFile)//Object[] ->item[0] filename item[1] file MemoryStream 
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (var zipArich = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (object[] item in lstFile)
                    {
                        var bytes = ((MemoryStream)item[1]).ToArray();
                        var entry = zipArich.CreateEntry((string)item[0]);
                        using (Stream stream = entry.Open())
                        {
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                //return ms.GetBuffer(); 
                return ms.ToArray();
            }
        }
        public static bool ByteToFile(byte[] byteArray, string fileName)
        {
            bool result = false;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    result = true;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }


        private MemoryStream StreamFromBitmapSource(BitmapSource writeBmp)
        {
            MemoryStream bmp;
            using (bmp = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(writeBmp));
                enc.Save(bmp);
            }

            return bmp;
        }

        //public void SaveAllSlices()
        //{
        //    if (isMSS_Computed!=2)
        //    {
        //        _ea.GetEvent<MessageEvent>().Publish($"模型切层未完成，无法保存图像");
        //        return;
        //    }
        //    ReadConfig();
        //    PrintTask_PreProcess();
        //    AngelSlicer.ProcessPath(ref taskModels);
        //    Stopwatch sw = Stopwatch.StartNew();
        //    sw.Start();
        //    AngelSlicer.GetSupports_OpenCV(ref taskModels, _config, partModels);
        //    sw.Stop();
        //    double time = sw.Elapsed.TotalSeconds;
        //    isSliceReady = true;
        //    //新建文件夹
        //    string slices_path = currFileFolder + "\\slices";
        //    if (Directory.Exists(slices_path))
        //    {
        //        DirectoryInfo di = new DirectoryInfo(slices_path);
        //        di.Delete(true);
        //    }
        //    Thread.Sleep(10);
        //    Directory.CreateDirectory(slices_path);
        //    for (int ind = 0; ind <= maxslice;ind++)
        //    {
        //        var bms = GetSliceImage_OpenCV(ind);
        //        _ea.GetEvent<MessageEvent>().Publish($"正在保存第{ind+1}层，共{maxslice}层");
        //        SaveBitmapImageIntoFile(bms.Item1[0], $"{slices_path}\\BM1-{ind.ToString("000")}.bmp");
        //        SaveBitmapImageIntoFile(bms.Item1[1], $"{slices_path}\\BM2-{ind.ToString("000")}.bmp");
        //    }
        //}
        public bool CheckThreeColumns(int proj)
        {
            return false;
        }
        //public double GetModelOffset(int prevind, int currind)
        //{
        //    double res = 0;
        //    for (int l = 0; l < 8; l++)
        //    {
        //        for (int i = 0; i < 400; i++)
        //        {
        //            double preMaxY = taskModels[prevind].layoutYRanges[l][i][1];//上一个的最大
        //            double currMinY = taskModels[currind].layoutYRanges[l][i][0];//下一个的最小
        //            double currDist = preMaxY - currMinY;
        //            if (res < currDist)
        //            {
        //                res = currDist;
        //            }
        //        }
        //    }
        //    return res;
        //}


    }
    public class SliceParas
    {
        public double layerThicness { get; set; }
        public double drawScaleX { get; set; }// mm -> pixels
        public double drawScaleY { get; set; }// mm -> pixels

        public double projectorPixScaleX { get; set; }

        public double projectorPixScaleY { get; set; }
        public double contourCompX { get; set; }
        public double contourCompY { get; set; }

        public int width { get; set; }
        public int height { get; set; }
        public double projectorOffsetX { get; set; }
        public double projectorOffsetY { get; set; }
        public double calibScaleFix { get; set; }

        public double holeOffsetX { get; set; }
        public double holeOffsetY { get; set; }
        public double holeRz { get; set; } = 0;
        public GrayCalib grayCalib { get; set; }
        public MathNet.Numerics.LinearAlgebra.Double.DenseMatrix calibMartix { get; set; } = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(3, 3);

        public List<double> holeComps { get; set; } = new List<double>();

        public double baseCompNum { get; set; }

        public double baseEnhanceRate { get; set; }
        
        public int baseEnhanceLayer { get; set; }
        public bool isAlignerMold { get; set; }

        public double dimmingRate { get; set; }
        public int dimmingWall { get; set; }
        public List<double> elephantFeetComps { get; set; }
    }
    public class PrintTaskModels
    {
        public int index;
        public string fileName;
        public string subfix = "stl";
        public string path = string.Empty;
        public MeshPlanarSlicer mps;
        public PlanarSliceStack pss;
        public DMesh3 mesh;
        public int vertexCount;
        public int edgeCount;
        public double triangleCount;
        public double volume;
        public double surfaceArea;
        public bool isSolidorPath = true;
        //public List<List<List<double>>> layoutYRanges;//层-X-minmax
        public List<List<PolyLine2d>> solidPathsList = new List<List<PolyLine2d>>();
        public List<List<PolyLine2d>> solidPathsInHoleList = new List<List<PolyLine2d>>();
        public List<List<PolyLine2d>> holePathsList = new List<List<PolyLine2d>>();
        public List<SupportModels> supportModelsList = new List<SupportModels>();
    }
    public class SupportModels
    {
        public int startLayer;//判断时写入，开始出现孤岛或大角度的层号
        public double centerX;//判断时写入，孤岛或大角度悬出的中心坐标
        public double CenterY;//判断时写入，孤岛或大角度悬出的中心坐标
        public bool isIsland;//判断时写入，是孤岛或大角度
        public bool lrORud;
        public List<SupportPixFill> supportPixFills = new List<SupportPixFill>();//第一层填充时初始化像素列表 ，之后轮询，如果已经涂白则下一层取消操作
    }
    public class SupportPixFill
    {
        public int x;
        public int y;
        public int fillingTopCount;

    }

    public static class ProjectionData
    {
        //public static BitmapImage printImage0 = new BitmapImage();
        //public static BitmapImage printImage1 = new BitmapImage();
        //public static int printPixCounts0;
        //public static int printPixCounts1;
        public static List<int> printPixCounts = new List<int>();
        public static List<BitmapImage> printImages = new List<BitmapImage>();
        public static int currPrintImagesIndexs;
    }

}
