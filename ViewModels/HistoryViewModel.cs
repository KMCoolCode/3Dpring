using AngleDLP.Models;
using Newtonsoft.Json;
using OxyPlot;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using static AngelDLP.ViewModels.MainWindowViewModel;
using static AngleDLP.LiteDBHelper;
using static AngleDLP.Models.LogHelper;

namespace AngelDLP.ViewModels
{
    public class HistoryViewModel : BindableBase
    {
        public IEventAggregator _ea;
        private string subViewName = "History";

        private List<PrintTaskDBInfo> printTaskDBInfos ;
        public List<PrintTaskDBInfo> PrintTaskDBInfos
        {
            get { return printTaskDBInfos; }
            set { SetProperty(ref printTaskDBInfos, value); }

        }
        private BitmapSource layoutImage;
        public BitmapSource LayoutImage
        {
            get { return layoutImage; }
            set { SetProperty(ref layoutImage, value); }

        }
        private string printInfoText;
        public string PrintInfoText
        {
            get { return printInfoText; }
            set { SetProperty(ref printInfoText, value); }

        }
        private PlotModel _ChartModel;
        public PlotModel ChartModel
        {
            get { return _ChartModel; }
            set { SetProperty(ref _ChartModel, value); }
        }
        
        private Visibility _IsChartVisiable;
        public Visibility IsChartVisiable
        {
            get { return _IsChartVisiable; }
            set { SetProperty(ref _IsChartVisiable, value); }
        }

        private bool tempOrVaccum = true;
        public bool TempOrVaccum
        {
            get { return tempOrVaccum; }
            set {
                if (printLog != null && printTaskConfig != null)
                {
                    if (printLog.Count > 0)
                    {
                        ChartModel = OxyPlotHelper.CreateChartModel2(printTaskConfig, printLog, value,true);//初始化列表，设定Level范围
                        IsChartVisiable = Visibility.Visible;
                    }
                    else
                    {
                        ChartModel = null;
                        IsChartVisiable = Visibility.Collapsed;
                    }
                }
                SetProperty(ref tempOrVaccum, value); }
        }


        List<PrintLog> printLog;
        PrintTaskConfig printTaskConfig;
        public DelegateCommand CloseCommand { get; private set; }
        public class HistoryDateViewEvent : PubSubEvent<DateTime> { }
        public class PrintLogViewEvent : PubSubEvent<int> { }
        public HistoryViewModel(IEventAggregator ea)
        {
            _ea = ea;
            _ = _ea.GetEvent<HistoryDateViewEvent>().Subscribe(date =>
            {
                PrintTaskDBInfos = AngleDLP.LiteDBHelper.PrintTaskDBInfo.GetPrintTaskDBInfos(date).OrderBy(p=>p.startTime).ToList();
                
                if (PrintTaskDBInfos.Count > 0)
                {
                    _ea.GetEvent<PrintLogViewEvent>().Publish(0);
                }
                else
                {
                    LayoutImage = null;
                    PrintInfoText = null;
                    IsChartVisiable = Visibility.Collapsed;
                }
            });
            _ = _ea.GetEvent<PrintLogViewEvent>().Subscribe(ind =>
            {
                if (ind == -1) {
                    return; }
                PrintTaskDBInfo printTaskinfo = PrintTaskDBInfos[ind];
                printTaskConfig = null; printLog = null;
                try
                {
                    string logFolder =  $"{ClassValue.appDir}\\logs\\{printTaskinfo.startTime.ToString("yyyyMM")}\\{printTaskinfo.startTime.ToString("yyyyMMdd")}\\{printTaskinfo.name}";
                    string imagePath = $"{logFolder}\\layout.bmp";
                    LayoutImage = new BitmapImage(new Uri(imagePath));
                    string textPath = $"{logFolder}\\printtaskinfo.txt";
                    printTaskConfig = PrintTaskConfig.DeserializeObject(printTaskinfo.printTaskConfigJsonStr);
                    using (FileStream fs = new FileStream(textPath,FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            PrintInfoText = sr.ReadToEnd();
                        }
                    }

                    if (File.Exists($"{logFolder}\\printlog.json"))
                    {
                        using (FileStream fs = new FileStream($"{logFolder}\\printlog.json", FileMode.Open))
                        {
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                printLog = JsonConvert.DeserializeObject<List<PrintLog>>(sr.ReadToEnd());
                            }
                        }
                        if(printLog !=null)
                        {
                            ChartModel = OxyPlotHelper.CreateChartModel2(printTaskConfig, printLog,TempOrVaccum,true);//初始化列表，设定Level范围
                            IsChartVisiable = Visibility.Visible;
                        }
                        else
                        {
                            ChartModel = null;
                            IsChartVisiable = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        ChartModel = null;
                        IsChartVisiable = Visibility.Collapsed;
                    }
                    
                }
                catch (Exception ex)
                {
                    
                }
                

            });
            CloseCommand = new DelegateCommand(Close);
        }
        private void Close()
        {
            _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
        }
    }
}