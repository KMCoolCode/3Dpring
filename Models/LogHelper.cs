using log4net.Core;
using log4net.Layout.Pattern;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleDLP.Models
{
    public class LogHelper
    {
        public enum LogLevel {Info,
            Debug,
            Warn,
            Error,
            Fatal
        }

        public class PrintLog
        {
            public int layer { get; set; }

            public double levelBe { get; set; }//曝光前，曝光后

            public double levelAe { get; set; }//曝光前，曝光后


            public double resinTemp { get; set; }//单次测量

            public double tankTemp1 { get; set; } = 0;//树脂槽温度
            public double tankTemp2 { get; set; } = 0;//树脂槽温度
            public double tankTemp3 { get; set; } = 0;//树脂槽温度
            public double tankTemp4 { get; set; } = 0;//树脂槽温度
            public double tankTemp5 { get; set; } = 0;//树脂槽温度
            public double tankTemp6 { get; set; } = 0;//树脂槽温度

            public double tankTemp7 { get; set; }//树脂槽温度

            public List<double> scraperMoveVaCuum { get; set; } = new List<double>();//连续监测-刮刀动作过程

            public double scraperVacuum { get; set; }
            public List<double> scraperStayVaCuum { get; set; } = new List<double>();//连续监测-曝光过程，曲线取曝光过程第一个

            public double printerTemp { get; set; }//单次测量

            public double printerHumidity { get; set; }//单次测量


            public double levelBoxPos { get; set; }

            public double exposeArea { get; set; }

            public List<int> projectorTemps { get; set; } = new List<int>();//左右光机
            public List<float> projectorLEDTemps { get; set; } = new List<float>();//左右光机
            public List<float> projectorBoardTemps { get; set; } = new List<float>();//左右光机

            public List<TimeSpan> projectorTime { get; set; } = new List<TimeSpan>();//左右光机，开始时间，结束时间，延续时常


            public List<int> projectorLights { get; set; } = new List<int>();//左右光机

            public string ToLogger()
            {
                return $"{layer}->level:{levelBe.ToString("0.00")}mm,resinTemp:{resinTemp.ToString("0.0")}℃,sV:{scraperStayVaCuum[0].ToString("0.0")}bar";
                //switch (logTiming)
                //{
                //    case 0:
                //        return $"before{commonLog},levelBoxPos:{levelBoxPos.ToString("0.00")}mm,printerTemp:{printerTemp}℃,printerHumi{printerHumidity}%,levelCompValue{levelCompValue}";
                //    case 1:
                //        string res = $"during{commonLog},exposeArea:{exposeArea.ToString("0.0")},";
                //        res += "projectorTemps:";
                //        foreach (var t in projectorTemps)
                //        { res += $"{t.ToString("0.0")}℃"; }
                //        res += "projectorLEDTemps:";
                //        foreach (var t in projectorLEDTemps)
                //        { res += $"{t.ToString("0.0")}℃"; }
                //        res += "projectorBoardTemps:";
                //        foreach (var t in projectorBoardTemps)
                //        { res += $"{t.ToString("0.0")}℃"; }
                //        res += "projectorLights:";
                //        foreach (var t in projectorLights)
                //        { res += $"{t.ToString("0.0")}"; }
                //        return res;
                //    case 2:
                //        return $"after_{commonLog},levelBoxPos:{  levelBoxPos.ToString("0.00")}mm";
                //}
                //return "";
            }

        }
        
    }
}
