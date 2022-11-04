using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace AngleDLP.PMC
{
    [Serializable]
    public class PrinterMotionConfig : BindableBase
    {
        public string printerModel { get; set; }
        public string printerSN { get; set; }
        public ScraperConfig scraperConfig { get; set; } = new ScraperConfig();
        public AxisConfig platformConfig { get; set; } 
        public AxisConfig levelBoxConfig { get; set; }
        public WindowConfig windowConfig { get; set; }
        public LevelControlConfig levelControlConfig { get; set; }
        public ConfigInfo deviceConfig { get; set; }
        public ConfigInfo systemConfig { get; set; }
        //public BaseCalibParas baseCalibParas { get; set; }
        public List<ADConfig> ADconfigs { get; set; }
        public List<DOConfig> DOconfigs { get; set; }
        public List<DIConfig> DIconfigs { get; set; }

        public List<RFIDConfig> RFIDConfigs { get; set; }

        public ModBusConfig modBusConfig { get; set; }
        public CardReaderConfig cardReaderConfig { get; set; }
    }

    [Serializable]
    public class BaseCalibParas
    {
        public ConfigInfo configInfo { get; set; }
        [JsonIgnore]
        public BaseCalib baseCalib { get; set; }
    }
    public class ADConfig
    {
        public string name { get; set; }

        public int sn { get; set; } = 0;

        public bool isSingle { get; set; }

        public int port { get; set; }

        public string nickName { get; set; }

        public double k1 { get; set; }

        public double k0 { get; set; }
    }
    public class DOConfig
    {
        public string name { get; set; }

        public int port { get; set; }

        public string nickName { get; set; }

    }

    public class ModBusConfig
    {
        public string com { get; set; }

        public int baudRate { get; set; }

        public int dataBits { get; set; }

        public int stopBits { get; set; }

        public int parity { get; set; }

        public List<ModBusRWBaseConfig> modBusWriteConfigs { get; set; }

        public List<ModBusReadConfig> modBusReadConfigs { get; set; }

    }
    public class ModBusReadConfig: ModBusRWBaseConfig
    {

        public int length { get; set; }

        public double k1 { get; set; }

        public double k0 { get; set; }

    }

    public class ModBusRWBaseConfig
    {
        public string name { get; set; }

        public string nickName { get; set; }

        public int station { get; set; }

        public int address { get; set; }

        public int funcCode { get; set; }

    }
    public class DIConfig
    {
        public string name { get; set; }

        public int port { get; set; }

        public string nickName { get; set; }

    }

    public class RFIDConfig
    {
        public string ip { get; set; }
        public int port { get; set; }
        public string name { get; set; }
        public string nickName { get; set; }
        public int length { get; set; }
    }

    public class CardReaderConfig 
    {
        public string com { get; set; }
        public int bandRate { get; set; }
    }
    [Serializable]
    public class ScraperConfig : AxisConfig
    {
        public int outPos { get; set; }
        public int inPos { get; set; }
        public int testPos { get; set; }

        public List<ZSenser> zSensers { get; set; }

    }
    public class ZSenser
    {
        public double zGap { get; set; }

        public double zValue { get; set; }

        public int port { get; set; }

        public double x { get; set; }

        public bool isMotive { get; set; } = false;

        public int moveIOport { get; set; }
    }
    [Serializable]
    public class WindowConfig : AxisConfig
    {
        public int LockPos { get; set; }
        public int UpPos { get; set; }
        public int DownPos { get; set; }

        public double initBackDist { get; set; }
    }


    [Serializable]
    public class AxisConfig : BindableBase
    {
        public int startPos { get; set; }

        public int zeroPos { get; set; }
        public int endPos { get; set; }
    }
    public class LevelControlConfig
    {
        public int zCrossArea { get; set; }

        public int floatCrossArea { get; set; }
        public int tankCrossArea { get; set; }

        public int resinAddCrossArea { get; set; }
    }
    public class ConfigInfo
    {
        public string path { get; set; }
        public string MD5 { get; set; }
        public string Version { get; set; }
        public string description { get; set; }
    }

    [Serializable]
    public class BaseCalib
    {

        public double max { get; set; }
        public double min { get; set; }
        public List<List<double>> values { get; set; } = new List<List<double>>();
    }
}
