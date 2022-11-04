using AngelDLP.PMC;
using AngleDLP;
using AngleDLP.Models;
using AngleDLP.PPC;
using DataService.Models.Dto.Pcloud;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using static AngelDLP.ViewModels.MainWindowViewModel;

namespace AngelDLP.ViewModels
{
    public class SettingViewModel : BindableBase
    {
        IEventAggregator _ea;
        private string subViewName = "Setting";


        

        public string isManual
        {
            get
            {
                return ClassValue.isManual;
            }
            protected set => ClassValue.isManual = value;
        }

        

        public bool ReplaceLabel
        {
            get
            {
                return ClassValue.ReplaceLabel;
            }
            protected set => ClassValue.ReplaceLabel = value;
        }
        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand UpdatePrinterProduceModeCommand { get; private set; }
        public DelegateCommand UpdateAlertCommand { get; private set; }
        public DelegateCommand UpdateMessageCommand { get; private set; }
        public DelegateCommand UpdateSublotCommand { get; private set; }
        public DelegateCommand<string> UserDBCommand { get; private set; }

        private PrinterProjectionControl _ppc;
        public PrinterProjectionControl PPC
        {
            get { return _ppc; }
            set { SetProperty(ref _ppc, value); }

        }


        private PrinterMotionControl _pmc;
        public PrinterMotionControl PMC
        {
            get { return _pmc; }
            set { SetProperty(ref _pmc, value); }

        }

        private int _SelectedPrintTaskStatus;
        public int SelectedPrintTaskStatus
        {
            get { return _SelectedPrintTaskStatus; }
            set { SetProperty(ref _SelectedPrintTaskStatus, value); }

        }

        private int _SelectedPrintAlert;
        public int SelectedPrintAlert
        {
            get { return _SelectedPrintAlert; }
            set { SetProperty(ref _SelectedPrintAlert, value); }

        }
        private bool _AlertChecked;
        public bool AlertChecked
        {
            get { return _AlertChecked; }
            set { SetProperty(ref _AlertChecked, value); }

        }
        private string _MessageToUpdate;
        public string MessageToUpdate
        {
            get { return _MessageToUpdate; }
            set { SetProperty(ref _MessageToUpdate, value); }

        }
        private int _SelectedLev;
        public int SelectedLev
        {
            get { return _SelectedLev; }
            set { SetProperty(ref _SelectedLev, value); }

        }

        private string _SublotToUpdate;
        public string SublotToUpdate
        {
            get { return _SublotToUpdate; }
            set { SetProperty(ref _SublotToUpdate, value); }

        }
        public SettingViewModel(IEventAggregator ea, PrinterMotionControl printerMotionControl, PrinterProjectionControl printerProjectionControl)
        {

            _pmc = printerMotionControl;
            _ppc = printerProjectionControl;
            _ea = ea;
            CloseCommand = new DelegateCommand(Close);
            UpdatePrinterProduceModeCommand = new DelegateCommand(new Action(() =>
            {
                _ea.GetEvent<UpDatePrintTaskStatusEvent>().Publish((PrinterProduceMode)SelectedPrintTaskStatus);
            }));
            UpdateAlertCommand = new DelegateCommand(new Action(() =>
            {
                _ea.GetEvent<UpDatePrinterAlertEvent>().Publish(new Tuple<PrinterAlerts, bool>((PrinterAlerts)SelectedPrintAlert,AlertChecked));
            }));
            UpdateMessageCommand = new DelegateCommand(new Action(() =>
            {
                _ea.GetEvent<PushMessageEvent>().Publish(new Tuple<string, PrinterEventLevel>(MessageToUpdate,(PrinterEventLevel)SelectedLev));
            }));
            UpdateSublotCommand = new DelegateCommand(new Action(() =>
            {
                _ea.GetEvent<GetSublotEvent>().Publish(SublotToUpdate);
            }));
            UserDBCommand = new DelegateCommand<string>(UserDBFunction);
        }
        private void UserDBFunction(string para)
        {
            if (para == "ImportUserInfo")
            {
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
                                List<UserInfo> users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UserInfo>>(jsonstr);
                                foreach (var u in users)
                                {
                                    LiteDBHelper.UserInfoDB.AddUser(u);
                                }
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
        }
        private void Close()
        {
            _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
        }
    }
}