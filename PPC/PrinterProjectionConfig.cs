using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Prism.Mvvm;

namespace AngleDLP.PPC
{
    [Serializable]
    public class PrinterProjectionConfig : BindableBase
    {
        public int projectorNum { get; set; }
        public List<ProjectorConfig> projectorConfigs { get; set; } 
    }
    [Serializable]
    public class ProjectorConfig : BindableBase
    {
        public int displayPort { get; set; }
        public int controlPort { get; set; }
        public int fMotorStep { get; set; }
        public int rMotorStep { get; set; }
        public int printerPixWidth { get; set; }//2160
        public int printerPixHeight { get; set; }//3840
        public int projectorPixWidth { get; set; }//3840
        public int projectorPixHeight { get; set; }//2160
        public double projectorOffsetX { get; set; }
        public double projectorOffsetY { get; set; }

        public List<double> layoutPixPadding { get; set; }
        public double imageTransScaleX { get; set; }
        public double imageTransScaleY { get; set; }
        public double imageTransRotate { get; set; }

        public double projectorPixScaleX { get; set; }//光机校准结果
        public double projectorPixScaleY { get; set; }//光机校准结果
        public double projectorPrintScale { get; set; }//标准件校准结果

        public double calibScaleFix { get; set; }

        public double holeOffsetX { get; set; }
        public double holeOffsetY { get; set; }

        public double holeRz { get; set; } = 0;
        public DenseMatrix calibMartix { get; set; } = new DenseMatrix(3, 3);
        public int exposeDAC { get; set; }
        public int exposeLight { get; set; }

        public double holeRadius { get; set; }

        public GrayCalibParas grayCalibParas { get; set; }
        public ProjectorConfig()
        {
        }

    }

    [Serializable]
    public class GrayCalibParas
    {
        public ConfigInfo configInfo { get; set; }
        [JsonIgnore]
        public GrayCalib grayCalib { get; set; }
    }

    [Serializable]
    public class GrayCalib
    {
        public double max { get; set; }
        public double min { get; set; }
        public List<List<double>> values { get; set; } = new List<List<double>>();
    }
    public class ConfigInfo
    {
        public string path { get; set; }
        public string MD5 { get; set; }
        public string Version { get; set; }
        public string description { get; set; }
    }
}
