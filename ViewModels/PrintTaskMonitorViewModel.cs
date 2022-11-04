using AngleDLP;
using AngleDLP.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static AngelDLP.PrintTask;
using static AngelDLP.ViewModels.MainWindowViewModel;
namespace AngelDLP.ViewModels
{
    public class PrintTaskMonitorViewModel : BindableBase
    {
        public IEventAggregator _ea;
        public PrintTaskHolder _printTaskHolder { set; get; }
        private string subViewName = "PrintTaskMonitor";
        private PlotModel _ChartModel;
        
        public PrintProgressViewModel PrintProgress { get; set; } 
        /// <summary>
        /// 图表Model的mvvm属性，可通知UI更新
        /// </summary>
        public PlotModel ChartModel
        {
            get { return _ChartModel; }
            set { SetProperty(ref _ChartModel, value); }
        }

        private string printInfoText;
        public string PrintInfoText
        {
            get { return printInfoText; }
            set { SetProperty(ref printInfoText, value); }

        }

        private bool tempOrVaccum = true;
        public bool TempOrVaccum
        {
            get { return tempOrVaccum; }
            set
            {
                if (_printTaskHolder._printTask.printLog != null && _printTaskHolder._printTask._config != null)
                {
                    ChartModel = OxyPlotHelper.CreateChartModel2(_printTaskHolder._printTask._config, _printTaskHolder._printTask.printLog, value);//初始化列表，设定Level范围
                }
                SetProperty(ref tempOrVaccum, value);
            }
        }
        public class PrintTaskChartUpdate : PubSubEvent<int>
        {

        }
        public class PrintMonitorTabEvent : PubSubEvent<int> { }
        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand<string> PrintTaskEventCommand { get; private set; }
        public PrintTaskMonitorViewModel(IEventAggregator ea, PrintTaskHolder printTaskHolder)
        {
            _ea = ea;
            _printTaskHolder = printTaskHolder;//用于打印数据展示
            CloseCommand = new DelegateCommand(Close);
            PrintTaskEventCommand = new DelegateCommand<string>(PrintTaskAction);
            _ea.GetEvent<PrintTaskChartUpdate>().Subscribe(ind =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    if (ind == 0 || ChartModel == null)
                    {
                        ChartModel = OxyPlotHelper.CreateChartModel2(_printTaskHolder._printTask._config, _printTaskHolder._printTask.printLog);//初始化列表，设定Level范围
                    }
                    else
                    {
                        ChartModel.InvalidatePlot(true);
                    }
                }));
            });
            _ea.GetEvent<PrintProgressViewEvent>().Subscribe(ppvm =>
            {
                PrintProgress = ppvm;

            });
            _ = _ea.GetEvent<PrintMonitorTabEvent>().Subscribe(ind =>
            {
                if (ind == -1)
                {
                    return;
                }
                if (ind == 1)
                {
                    try
                    {
                        string logFolder = $"{ClassValue.appDir}\\logs\\{_printTaskHolder._printTask.printStartTime.ToString("yyyyMM")}\\{_printTaskHolder._printTask.printStartTime.ToString("yyyyMMdd")}\\{_printTaskHolder._printTask.printTaskName}";
                        string imagePath = $"{logFolder}\\layout.bmp";
                        string textPath = $"{logFolder}\\printtaskinfo.txt";
                        using (FileStream fs = new FileStream(textPath, FileMode.Open))
                        {
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                PrintInfoText = sr.ReadToEnd();
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    return;
                }
               
                


            });
        }

        private void PrintTaskAction(string msg)
        {
            Task.Run(() => {
                var res = DialogHelper.ShowYONDialog("警告", $"是否停止打印?", DialogHelper.DialogType.YesNo);
                if (res)
                {
                    _ea.GetEvent<PrintTaskEvent>().Publish(msg);
                }
            });
            
        }
        //private PlotModel CreateChartModel()
        //{
        //    //y1-液位
        //    //y2-温度
        //    //y3-负压
        //    var model = new PlotModel() { };

        //    // 添加图例说明
        //    model.Legends.Add(new Legend
        //    {
        //        LegendPlacement = LegendPlacement.Outside,
        //        LegendPosition = LegendPosition.BottomCenter,
        //        LegendOrientation = LegendOrientation.Horizontal,
        //        LegendBorderThickness = 0,
        //        LegendTextColor = OxyColors.LightGray
        //    });

        //    // 定义第一个Y轴y1，显示液位
        //    var ay1 = new LinearAxis()
        //    {
        //        Key = "y1",
        //        Position = AxisPosition.Left,
        //        Minimum = Math.Min(-3,_printTask._config.levelControlParas.level -0.15),
        //        MajorStep = 0.5,
        //        MinorStep = 0.1,
        //        Title = "Level(mm)/Vaccum(bar)",
        //        Maximum = Math.Max(1, _printTask._config.levelControlParas.level + 0.15),
        //        LabelFormatter = v => $"{v:0.00}"
        //    };
        //    // 在第1Y轴坐标50%和80%处显示网格线
        //    ay1.ExtraGridlines = new double[2] { _printTask._config.levelControlParas.level - 0.1, _printTask._config.levelControlParas.level + 0.1 };
        //    ay1.ExtraGridlineStyle = LineStyle.DashDashDot; // 网格线样式

        //    // 定义第二个Y轴y2，显示温度
        //    var ay2 = new LinearAxis()
        //    {
        //        Key = "y2",
        //        Position = AxisPosition.Right,
        //        Minimum = 24,
        //        MajorStep = 5,
        //        MinorStep = 1,
        //        Maximum = 36,
        //        Title="Temp(℃)",
        //        LabelFormatter = v => $"{v:00}"

        //    };




        //    var ax = new LinearAxis()
        //    {
        //        Minimum = Math.Max(_printTask.printLogBe.Count - 18, 0),
        //        Maximum = Math.Max(_printTask.printLogBe.Count, 18),
        //        Position = AxisPosition.Bottom,
        //        IsZoomEnabled = true
        //    };

        //    // 定义折线图序列，指定数据轴为Y1轴
        //    LineSeries levelBASeries = new LineSeries();
        //    levelBASeries.Title = "曝光前液位";
        //    levelBASeries.YAxisKey = "y1";
        //    // 点击时弹出的label内容
        //    levelBASeries.TrackerFormatString = "{0}\r\n时间：{LayerStartTime:HH:mm:ss}\r\n{2:0}层: {4:0.00}mm";
        //    // 设置颜色阈值范围
        //    //levelSeries.LimitHi = 0.15;
        //    //levelSeries.LimitLo = -0.15;
        //    // 设置数据绑定源和字段
        //    levelBASeries.ItemsSource = _printTask.printLogBe;
        //    levelBASeries.DataFieldX = "layer";
        //    levelBASeries.DataFieldY = "level";
        //    levelBASeries.Color = OxyColors.YellowGreen;

        //    // 定义折线图序列，指定数据轴为Y1轴
        //    var levelAASeries = new LineSeries();
        //    levelAASeries.Title = "曝光后液位";
        //    levelAASeries.YAxisKey = "y1";
        //    // 点击时弹出的label内容
        //    levelAASeries.TrackerFormatString = "{0}\r\n时间：{LayerStartTime:HH:mm:ss}\r\n{2:0}层: {4:0.00}mm";

        //    // 设置颜色阈值范围
        //    //levelSeries.LimitHi = 0.15;
        //    //levelSeries.LimitLo = -0.15;
        //    // 设置数据绑定源和字段
        //    levelAASeries.ItemsSource = _printTask.printLogAe;
        //    levelAASeries.DataFieldX = "layer";
        //    levelAASeries.DataFieldY = "level";
        //    levelAASeries.Color = OxyColors.Green;


        //    // 定义折线图序列，指定数据轴为Y2轴
        //    var tempSeries = new LineSeries();
        //    tempSeries.Title = "温度";
        //    tempSeries.YAxisKey = "y2";
        //    tempSeries.LineStyle = LineStyle.DashDashDot;
        //    // 点击时弹出的label内容
        //    tempSeries.TrackerFormatString = "{0}\r\n时间：{LayerStartTime:HH:mm:ss}\r\n{2:0}层: {4:0.0}℃";
        //    // 设置颜色
        //    tempSeries.Color = OxyColors.Blue;
        //    // 设置数据绑定源和字段
        //    tempSeries.ItemsSource = _printTask.printLogBe;
        //    tempSeries.DataFieldX = "layer";
        //    tempSeries.DataFieldY = "resinTemp";

        //    // 定义折线图序列，指定数据轴为Y2轴
        //    var vacummSeries = new LineSeries();
        //    vacummSeries.Title = "真空负压";
        //    vacummSeries.YAxisKey = "y1";
        //    // 点击时弹出的label内容
        //    vacummSeries.TrackerFormatString = "{0}\r\n时间：{LayerStartTime:HH:mm:ss}\r\n{2:0}层: {4:0.00}bar";
        //    // 设置颜色
        //    vacummSeries.Color = OxyColors.Yellow;
        //    // 设置数据绑定源和字段
        //    vacummSeries.ItemsSource = _printTask.printLogBe;
        //    vacummSeries.DataFieldX = "layer";
        //    vacummSeries.DataFieldY = "scraperVaCuum";

        //    // 下面为手动添加数据方式
        //    //passedRateSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now.Date.AddDays(-15)), .750));
        //    // 添加图标资源
        //    model.Series.Add(levelBASeries);
        //    model.Series.Add(levelAASeries);
        //    model.Series.Add(tempSeries);
        //    model.Series.Add(vacummSeries);
        //    model.Axes.Add(ay1);
        //    model.Axes.Add(ay2);
        //    model.Axes.Add(ax);
        //    // 设置图形边框
        //    model.PlotAreaBorderThickness = new OxyThickness(1, 0, 1, 1);
        //    return model;
        //}
        private void Close()
        {
            _ea.GetEvent<ViewSwitchEvent>().Publish (subViewName);
        }
    }
}