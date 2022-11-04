using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.Pcloud
{
    public class Rt_PrinterInfoPackageDto
    {
        /// <summary>
        /// 打印机Token
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 软件属性
        /// </summary>
        public SoftwareProperties softwareProperties { get; set; }
        /// <summary>
        /// 固件属性
        /// </summary>
        public FirmwareProperties firmwareProperties { get; set; }
        /// <summary>
        /// 软件运行状态
        /// </summary>
        public SoftwareRunningStatus softwareRunningStatus { get; set; }
        /// <summary>
        /// 硬件连接状态
        /// </summary>
        public HardwareConnectionStatus hardwareConnectionStatus { get; set; }
        /// <summary>
        /// 硬件状态
        /// </summary>
        public HardwareStatus hardwareStatus { get; set; }
        /// <summary>
        /// 生产状态
        /// </summary>
        public ProduceStatus produceStatus { get; set; }
        /// <summary>
        /// 警报列表
        /// </summary>
        public Alerts alerts { get; set; }
    }
    public class PrintDimension
    {
        /// <summary>
        /// 
        /// </summary>
        public double x { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double y { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double z { get; set; }
    }

    public class SoftwareProperties
    {
        /// <summary>
        /// 
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string model { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PrintDimension printDimension { get; set; }
    }

    public class FirmwareProperties
    {
        /// <summary>
        /// PRT版本号
        /// </summary>
        public string prtVersion { get; set; }
        /// <summary>
        /// EXP版本号
        /// </summary>
        public string expVersion { get; set; }
        /// <summary>
        /// 是否带有自动收集装置
        /// </summary>
        public bool hasCollector { get; set; }
        /// <summary>
        /// 是否带有自动补料装置
        /// </summary>
        public bool hasReplenisher { get; set; }
    }

    public class SoftwareRunningStatus
    {
        /// <summary>
        /// 
        /// </summary>
        public string running { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string updatedTime { get; set; }
    }

    public class HardwareConnectionStatus
    {
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool connected { get; set; }
        /// <summary>
        /// 上次状态更改时间
        /// </summary>
        public DateTime updatedTime { get; set; }
    }

    public class HardwareStatus
    {
        /// <summary>
        /// 树脂温度
        /// </summary>
        public List<double> resinTemp { get; set; }
        /// <summary>
        /// 空气温度
        /// </summary>
        public double airTemp { get; set; }
        /// <summary>
        /// 液晶温度
        /// </summary>
        public double ledTemp { get; set; }
        /// <summary>
        /// 箱内实测温度
        /// </summary>
        public double platformTemp { get; set; }
        /// <summary>
        /// 箱内实测湿度
        /// </summary>
        public double platformHum { get; set; }
        /// <summary>
        /// 树脂液位
        /// </summary>
        public double resinLevel { get; set; }
    }

    public class ProduceStatus
    {
        /// <summary>
        /// 模式
        /// </summary>
        public string mode { get; set; }
        /// <summary>
        /// 状态更改时间
        /// </summary>
        public string updatedTime { get; set; }
    }


    public enum PrinterProduceMode
    {
        //维护模式
        off,
        //生产模式
        on,
        //待维护模式
        waitingOff


    }

    public enum PrinterAlerts
    {
        replenishing,
        replenishment,
        collection,
        reaping


    }
    public class Replenishing
    {
        /// <summary>
        /// 
        /// </summary>
        public string active { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string updatedTime { get; set; }
    }

    public class Replenishment
    {
        /// <summary>
        /// 
        /// </summary>
        public string active { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string updatedTime { get; set; }
    }

    public class Collection
    {
        /// <summary>
        /// 
        /// </summary>
        public string active { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string updatedTime { get; set; }
    }

    public class Reaping
    {
        /// <summary>
        /// 
        /// </summary>
        public string active { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string updatedTime { get; set; }
    }

    public class Alerts
    {
        /// <summary>
        /// 补液中
        /// </summary>
        public Replenishing replenishing { get; set; }
        /// <summary>
        /// 需要补液
        /// </summary>
        public Replenishment replenishment { get; set; }
        /// <summary>
        /// 需要收料
        /// </summary>
        public Collection collection { get; set; }
        /// <summary>
        /// 收料中
        /// </summary>
        public Reaping reaping { get; set; }
    }

    
}
