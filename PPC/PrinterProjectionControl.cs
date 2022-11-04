using AngelDLP;
using AngelDLP.PMC;
using AngelDLP.PPC;
using AngleDLP.Models;
using AngleDLP.PPC;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AngelDLP.ViewModels.MainWindowViewModel;
using static AngleDLP.Models.LogHelper;

namespace AngleDLP.PPC
{
    public class PrinterProjectionControl: BindableBase
    {
        private readonly IEventAggregator _ea;
        public log4net.ILog pcLog;
        public bool _isAvaliable = true;
        public bool _isReady = true;

        public PrinterProjectionConfig _config { get; set; } 
        public ObservableCollection<Projector> P_List { get; set; } =  new ObservableCollection<Projector>();

        public PrinterProjectionControl()
        {

        }

        public PrinterProjectionControl(IEventAggregator ea)
        {
            
            this._ea = ea;
            pcLog = log4net.LogManager.GetLogger("logProjection");
           
        }

        private void MessageInfoUpdate(string info)
        {

            _ea.GetEvent<MessageEvent>().Publish(info);

        }

        private void MessageInfoUpdate_Thread(string info, LogLevel logLevel = LogLevel.Info)
        {
            
            switch (logLevel)
            {
                case LogLevel.Info:
                    pcLog.Info(info);
                    break;
                case LogLevel.Debug:
                    pcLog.Debug(info);
                    break;
                case LogLevel.Warn:
                    pcLog.Warn(info);
                    break;
                case LogLevel.Error:
                    pcLog.Error(info);
                    break;
                case LogLevel.Fatal:
                    pcLog.Fatal(info);
                    break;
            }
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageInfoUpdate(info);
            }));
        }
        public bool ReadConfig()
        {
            //如何判断配置文件无问题？？
            //1、正常读入
            //2、MD5校验
            //3、否则状态变红

            try
            {
                if (false)//if (!ClassValue.CheckMD5(ClassValue.ProjectionControlConfigFile, ClassValue.ProjectionControlConfigMD5))
                {
                    _isAvaliable = false;
                    return false;
                }
                using (FileStream fs = new FileStream(ClassValue.ProjectionControlConfigFile, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string jsonstr = sr.ReadToEnd();
                        _config = Newtonsoft.Json.JsonConvert.DeserializeObject<PrinterProjectionConfig>(jsonstr);
                    }
                }
                foreach (var conf in _config.projectorConfigs)
                {
                    if (true)//if (!ClassValue.CheckMD5(conf.grayCalibParas.configInfo.path, conf.grayCalibParas.configInfo.MD5))
                    {
                        using (FileStream fs = new FileStream(conf.grayCalibParas.configInfo.path, FileMode.Open))
                        {
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                string jsonstr = sr.ReadToEnd();
                                conf.grayCalibParas.grayCalib = Newtonsoft.Json.JsonConvert.DeserializeObject<GrayCalib>(jsonstr);
                            }
                        }
                    }
                    else
                    {
                        _isAvaliable = false;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _ea.GetEvent<MessageEvent>().Publish(ex.ToString());
                _isAvaliable = false;
                _isReady = false;
                MessageInfoUpdate_Thread(ex.ToString());
                return false;
            }
            return true;
        }

        public bool WriteConfig()
        {


            try
            {
                using (FileStream fs = new FileStream(ClassValue.ProjectionControlConfigFile, FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        
                        string  jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(_config,Newtonsoft.Json.Formatting.Indented);
                        sw.Write(jsonString);
                    }
                }
                //更新MD5值
            }
            catch (Exception ex)
            {
                _ea.GetEvent<MessageEvent>().Publish(ex.ToString());

                MessageInfoUpdate_Thread(ex.ToString());
                return false;
            }
            return true;
        }

        public bool ReadCalibConfig(string path)
        {
            //如何判断配置文件无问题？？
            //1、正常读入
            //2、MD5校验
            //3、否则状态变红

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string jsonstr = sr.ReadToEnd();
                        _config = Newtonsoft.Json.JsonConvert.DeserializeObject<PrinterProjectionConfig>(jsonstr);
                    }
                }
            }
            catch (Exception ex)
            {
                _ea.GetEvent<MessageEvent>().Publish(ex.ToString());
                _isAvaliable = false;
                MessageInfoUpdate_Thread(ex.ToString());
                return false;
            }
            return true;
        }
        public bool WriteCalibConfig(string path)
        {
            //如何判断配置文件无问题？？
            //1、正常读入
            //2、MD5校验
            //3、否则状态变红

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        
                        sw.Write( Newtonsoft.Json.JsonConvert.SerializeObject(_config));
                    }
                }
            }
            catch (Exception ex)
            {
                _ea.GetEvent<MessageEvent>().Publish(ex.ToString());
                _isAvaliable = false;
                MessageInfoUpdate_Thread(ex.ToString());
                return false;
            }
            return true;
        }
        public void InitScreens()
        {
            Screen[] screenlist = Screen.AllScreens;
            int ind = 0;
            foreach (var s in screenlist)
            {
                _ea.GetEvent<MessageEvent>().Publish($"{ind++}->{s.WorkingArea.Width}x{s.WorkingArea.Height}");
            }
            for (int i = 0; i < _config.projectorNum; i++)
            {
                P_List.Add(new Projector(_ea));
            }
        }
        public void InitProjector()
        {
            
            for (int i = 0; i < _config.projectorNum; i++)
            {
                
                P_List[i].SetPara(i, _config.projectorConfigs[i]);
                if (P_List[i].InitProjector(_config.projectorConfigs[i].displayPort, _config.projectorConfigs[i].controlPort))
                {

                }
                else
                {
                    _isReady = false;
                }
            }
        }

        public void CloseProjector()
        {
            foreach (var p in P_List)
            {
               if (p.IsON) { p.ProjectorOff(); }
            }
        }
    }
}
