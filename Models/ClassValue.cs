using DataService.Models;
using DataService.Models.Dto.Pcloud;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static AngleDLP.LiteDBHelper;

namespace AngleDLP.Models
{
    public static class ClassValue
    {
        public static string appDir { get; set; } = "";

        public static string DefalutPrintTaskConfigName { get; set; }
        public static bool EnableProjCalibratorMode { get; set; }
        public static int DefalutPrintTaskConfigVersion { get; set; }


        public static string IniFile { get; set; }
        public static string MotionControlConfigFile { get; set; }

        public static string MotionControlConfigMD5 { get; set; }

        public static string ProjectionControlConfigFile { get; set; }

        public static string ProjectionControlConfigMD5 { get; set; }
        public static string PrinterDBFile { get; set; }

        public static string isManual { get; set; }

        public static bool ReplaceLabel { get; set; }

        public static bool CanAddResin { get; set; } = true;
        public static bool EnableSimulate { get; set; } = false;

        public static string AppVersion { get; set; }


        private static MESConfig _mesConfig = new MESConfig();
        public static MESConfig MESConfig
        {
            get { return _mesConfig; }
            set
            {
                _mesConfig = value;
            }
        }
        private static UserInfo _userinfo;
        public static UserInfo userinfo
        {
            get { return _userinfo; }
            set
            {
                if (value.userId != null)
                {
                    ConfigModel.UserId = value.userId;
                }
                _userinfo = value;
            }
        }
        public static Re_PrinterBonjourDto printerConfig { get; set; }

        public static bool CheckMD5(string file, string filemd5)
        {
            try
            {
                using (FileStream fStream = new FileStream(file, System.IO.FileMode.Open))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(fStream);

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        builder.Append(retVal[i].ToString("X2"));
                    }
                    if (builder.ToString().ToLower() == filemd5)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashCodeFromFile() fail,error:" + ex.Message);
            }
        }

        public static bool AccessAppSettings()
        {
            try
            {
                //获取Configuration对象
                Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                IniFile = config.AppSettings.Settings["PrinterConfigIniFile"].Value;
                if (!File.Exists(IniFile)) //todo 
                {
                    //if ini file not exist use default param
                    IniConfigHelper.WriteSetting(IniFile, "app", "EnableProjCalibratorMode", "false");
                    return false;
                }
                //根据Key读取<add>元素的Value
                ClassValue.EnableProjCalibratorMode = bool.Parse(IniConfigHelper.ReadSetting(IniFile, "app", "EnableProjCalibratorMode", ""));
                ClassValue.DefalutPrintTaskConfigName = IniConfigHelper.ReadSetting(IniFile, "PrintTask", "DefalutPrintTaskConfigName", "");
                ClassValue.DefalutPrintTaskConfigVersion = int.Parse(IniConfigHelper.ReadSetting(IniFile, "PrintTask", "DefalutPrintTaskConfigVersion", ""));
                ClassValue.MotionControlConfigFile = IniConfigHelper.ReadSetting(IniFile, "PrinterConfigs", "MotionControlConfigFile", "");
                ClassValue.MotionControlConfigMD5 = IniConfigHelper.ReadSetting(IniFile, "PrinterConfigs", "MotionControlConfigMD5", "");
                ClassValue.ProjectionControlConfigFile = IniConfigHelper.ReadSetting(IniFile, "PrinterConfigs", "ProjectionControlConfigFile", "");
                ClassValue.ProjectionControlConfigMD5 = IniConfigHelper.ReadSetting(IniFile, "PrinterConfigs", "ProjectionControlConfigMD5", "");
                ClassValue.PrinterDBFile = IniConfigHelper.ReadSetting(IniFile, "app", "PrinterDBFile", "");
                ClassValue.MESConfig.MesHead = IniConfigHelper.ReadSetting(IniFile, "MESConfig", "MesHead", "");
                ClassValue.MESConfig.maxLimit = IniConfigHelper.ReadSetting(IniFile, "MESConfig", "maxLimit", "");
                ClassValue.isManual = IniConfigHelper.ReadSetting(IniFile, "MESConfig", "isManual", "");
                ClassValue.MESConfig.DeviceId = IniConfigHelper.ReadSetting(IniFile, "MESConfig", "DeviceId", "");
                ClassValue.MESConfig.P_PrintToken = IniConfigHelper.ReadSetting(IniFile, "MESConfig", "P_PrintToken", "");
                ClassValue.MESConfig.P_PrintId = int.Parse(IniConfigHelper.ReadSetting(IniFile, "MESConfig", "P_PrintId", ""));
                ClassValue.MESConfig.SublotId = IniConfigHelper.ReadSetting(IniFile, "MESConfig", "SublotId", "");
                ClassValue.MESConfig.PrinterProduceMode = (PrinterProduceMode)System.Enum.Parse(typeof(PrinterProduceMode), IniConfigHelper.ReadSetting(IniFile, "MESConfig", "PrinterProduceMode", ""));
                ClassValue.ReplaceLabel = bool.Parse(IniConfigHelper.ReadSetting(IniFile, "PrintTask", "ReplaceLabel", ""));
                return true;
            }
            catch
            {
                return false;
            }
            ////写入<add>元素的Value
            //config.AppSettings.Settings["name"].Value = "fx163";
            ////增加<add>元素
            //config.AppSettings.Settings.Add("url", "http://www.fx163.net");
            ////删除<add>元素
            //config.AppSettings.Settings.Remove("name");
            ////一定要记得保存，写不带参数的config.Save()也可以
            //config.Save(ConfigurationSaveMode.Modified);
            ////刷新，否则程序读取的还是之前的值（可能已装入内存）
            //System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        }


    }

    public class MESConfig : INotifyPropertyChanged
    {
        private PCloud pCloud = new PCloud();

        private string _MesHead;
        public string MesHead
        {
            get { return _MesHead; }
            set
            {
                _MesHead = value;
                ConfigModel.MesHead = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MesHead"));
            }
        }

        private string _tags;
        public string tags
        {
            get { return _tags; }
            set
            {
                _tags = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("tags"));
            }
        }
        private string _maxLimit;
        public string maxLimit
        {
            get { return _maxLimit; }
            set
            {
                _maxLimit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("maxLimit"));
            }
        }

        private string _DeviceId;
        public string DeviceId
        {
            get { return _DeviceId; }
            set
            {
                _DeviceId = value;
                ConfigModel.DeviceId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DeviceId"));
            }
        }

        //public static string P_PrintToken { get; set; }
        private string _P_PrintToken;
        public string P_PrintToken
        {
            get { return _P_PrintToken; }
            set
            {
                _P_PrintToken = value;
                ConfigModel.P_PrintToken = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("P_PrintToken"));
            }
        }
        //public static int P_PrintId { get; set; }
        private int _P_PrintId;
        public int P_PrintId
        {
            get { return _P_PrintId; }
            set
            {
                _P_PrintId = value;
                ConfigModel.P_PrintId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("P_PrintId"));
            }
        }

        private string _SublotId;
        public string SublotId
        {
            get { return _SublotId; }
            set
            {
                if (_SublotId != value)
                {
                    _SublotId = value;
                    try
                    {
                        //获取Configuration对象
                        IniConfigHelper.WriteSetting(ClassValue.IniFile, "MESConfig", "SublotId", value);
                    }
                    catch
                    {

                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SublotId"));
                }


                ConfigModel.SublotId = value;


            }
        }

        private PrinterProduceMode _PrinterProduceMode;
        public PrinterProduceMode PrinterProduceMode
        {
            get { return _PrinterProduceMode; }
            set
            {
                if (_PrinterProduceMode != value)
                {

                    try
                    {
                        if (pCloud.UpdatePrinterProduceMode(value).Item1)
                        {
                            _PrinterProduceMode = value;
                            //获取Configuration对象
                            IniConfigHelper.WriteSetting(ClassValue.IniFile, "MESConfig", "PrinterProduceMode", value.ToString());
                        }

                    }
                    catch
                    {

                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PrinterProduceMode"));
                }




            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
