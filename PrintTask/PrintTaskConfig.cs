using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace AngelDLP
{
    [Serializable]
    public class PrintTaskConfig : BindableBase
    {
        public AccuracyParas accuracyParas { get; set; } = new AccuracyParas();
        public LevelControlParas levelControlParas { get; set; } = new LevelControlParas();
        public PrintConfig printConfig { get; set; } = new PrintConfig();

        private BaseCalibParas _baseCalibParas= new BaseCalibParas();

        public BaseCalibParas baseCalibParas
        {
            get
            {
                return _baseCalibParas;
            }
            set
            {
                SetProperty(ref _baseCalibParas, value);
            }
        }

        public MotionConfig motionConfig { get; set; } = new MotionConfig();

        public static PrintTaskConfig DeserializeObject(string jsonStr)
        {
            PrintTaskConfig res = new PrintTaskConfig();
            res = JsonConvert.DeserializeObject<PrintTaskConfig>(jsonStr);
            return res;
        }
    }
    [Serializable]
    public class AccuracyParas : INotifyPropertyChanged
    {

        public double scaleCompX { get; set; }
        public double scaleCompY { get; set; }
        public double contourCompX { get; set; }
        public double contourCompY { get; set; }
        public double contourComp { get; set; }
        public double archCompPara { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [Serializable]
    public class LevelControlParas : INotifyPropertyChanged
    {
        public string materialType { get; set; }
        public double level { get; set; }

        public int startFFLC  { get; set; }
        public double materialShrinkRate { get; set; }
        public double levelCompRate { get; set; }

        public double levelWaitTime { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    [Serializable]
    public class BaseCalibParas : BindableBase
    {
        public bool enableBaseCalib { get; set; } = true;
        public double holeCompNum { get; set; }
        public int baseCompNum { get; set; } = 0;
        public double zInitComp { get; set; }

        public double plateLowest { get; set; }

        public double plateHeightest { get; set; }
        public double exposeBaseEnhanceRate { get; set; }
        public double lightBaseEnhanceRate { get; set; }
        public int exposeBaseEnhanceLayer { get; set; }
        private ObservableCollection<ElephantFeetItem> _elephantFeetComps = new ObservableCollection<ElephantFeetItem>();
        public ObservableCollection<ElephantFeetItem> elephantFeetComps
        {
            get { return _elephantFeetComps; }
            set
            {
                SetProperty(ref _elephantFeetComps, value);

            }
        }
        private ObservableCollection<HoleCompItem> _holeComps = new ObservableCollection<HoleCompItem>();
        public ObservableCollection<HoleCompItem> holeComps
        {
            get { return _holeComps; }
            set
            {
                SetProperty(ref _holeComps, value);

            }
        }

    }
    public class ElephantFeetItem : BindableBase
    {
        private double _offset = 0;
        public double offset
        {
            get { return _offset; }
            set {
                SetProperty(ref _offset, value);
            }
        }
    }

    public class HoleCompItem : BindableBase
    {
        private double _radius = 0;
        public double radius
        {
            get { return _radius; }
            set
            {
                SetProperty(ref _radius, value);
            }
        }
    }

    public class PrintConfig
    {
        public double layerThickness { get; set; }
        public double exposeLight { get; set; }
        public double exposeTime { get; set; }
        public bool enableScraper { get; set; }
        public bool isAlignerMold { get; set; } = true;
        public double dimmingRate { get; set; } = 1;
        public int dimmingWall { get; set; } = 6;
    }
    public class MotionConfig
    {
        public ScrapterLessConfig scrapterLessConfig { get; set; } = new ScrapterLessConfig();

        public MotionSpeedConfig motionSpeedConfig { get; set; } = new MotionSpeedConfig();

    }
    public class ScrapterLessConfig
    {
        public int scraperLessStartLayer { get; set; }

        public double platformDownSpeedNoUseScraper { get; set; }

        public double platformUpSpeedNoUseScraper { get; set; }
        public double platformOverDownNoUseScraper { get; set; }

    }

    public class MotionSpeedConfig
    {
        public double levelBoxSpeed { get; set; }

        public double scraperSpeed { get; set; }

        public double platformSpeed { get; set; }

        public double levelBoxInitSpeed { get; set; }


        public double scraperInitSpeed { get; set; }


        public double platformInitSpeed { get; set; }

    }
    public class MotionTimeConfig
    {
         
    }
    
}
