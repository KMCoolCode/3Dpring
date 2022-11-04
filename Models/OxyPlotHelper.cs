using AngelDLP;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AngleDLP.Models.LogHelper;

namespace AngleDLP.Models
{
    internal class OxyPlotHelper
    {
        /// <summary>
        /// 过程记录图表数据
        /// </summary>
        /// <param name="printTaskConfig">打印参数</param>
        /// <param name="printLog">loglist</param>
        public static PlotModel CreateChartModel2(PrintTaskConfig printTaskConfig, List<PrintLog> printLog,
            bool tempOrVaccum = true, bool overView = false)
        {
            //y1-液位
            //y2-温度
            //y3-负压
            var model = new PlotModel() { };

            // 添加图例说明
            model.Legends.Add(new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0,
                LegendTextColor = OxyColors.LightGray
            });

            // 定义第一个Y轴y1，显示液位
            var ay1 = new LinearAxis()
            {
                Key = "y1",
                Position = AxisPosition.Left,
                Minimum = Math.Min(0, printTaskConfig.levelControlParas.level - 0.15),
                MajorStep = 0.1,
                MinorStep = 0.02,
                Title = "Level(mm)",
                Maximum = Math.Max(printTaskConfig.levelControlParas.level - 0.3, printTaskConfig.levelControlParas.level + 0.3),
                LabelFormatter = v => $"{v:0.00}"
            };
            // 在第1Y轴坐标50%和80%处显示网格线
            ay1.ExtraGridlines = new double[2] { printTaskConfig.levelControlParas.level - 0.1, printTaskConfig.levelControlParas.level + 0.1 };
            ay1.ExtraGridlineStyle = LineStyle.DashDashDot; // 网格线样式

            // 定义第二个Y轴y2，显示温度
            var ay2 = new LinearAxis()
            {
                Key = "y2",
                Position = AxisPosition.Right,
                Minimum = 22,
                MajorStep = 2,
                MinorStep = 1,
                Maximum = 33,
                Title = "Temp(℃)",
                LabelFormatter = v => $"{v:0.0}"

            };
            if (!tempOrVaccum)
            {
                ay2 = new LinearAxis()
                {
                    Key = "y2",
                    Position = AxisPosition.Right,
                    Minimum = -310,
                    MajorStep = 10,
                    MinorStep = 5,
                    Maximum = -210,
                    Title = "Vaccum(Pa)",
                    LabelFormatter = v => $"{v:00}"

                };
            }

            var ax = new LinearAxis()
            {
                Minimum = Math.Max(printLog.Count - 30, 0),
                Maximum = Math.Max(printLog.Count, 30),
                Position = AxisPosition.Bottom,
                IsZoomEnabled = true
            };
            if(overView)
            {
                ax = new LinearAxis()
                {
                    Minimum = 0,
                    Maximum = printLog.Count,
                    Position = AxisPosition.Bottom,
                    IsZoomEnabled = true
                };
                }
            // 定义折线图序列，指定数据轴为Y1轴
            LineSeries levelBASeries = new LineSeries();
            levelBASeries.Title = "曝光前液位";
            levelBASeries.YAxisKey = "y1";
            // 点击时弹出的label内容
            levelBASeries.TrackerFormatString = "{0}\r\n时间：{LayerStartTime:HH:mm:ss}\r\n{2:0}层: {4:0.00}mm";
            // 设置颜色阈值范围
            //levelSeries.LimitHi = 0.15;
            //levelSeries.LimitLo = -0.15;
            // 设置数据绑定源和字段
            levelBASeries.ItemsSource = printLog;
            levelBASeries.DataFieldX = "layer";
            levelBASeries.DataFieldY = "levelBe";
            levelBASeries.Color = OxyColors.YellowGreen;

            // 定义折线图序列，指定数据轴为Y1轴
            var levelAASeries = new LineSeries();
            levelAASeries.Title = "曝光后液位";
            levelAASeries.YAxisKey = "y1";
            // 点击时弹出的label内容
            levelAASeries.TrackerFormatString = "{0}\r\n时间：{LayerStartTime:HH:mm:ss}\r\n{2:0}层: {4:0.00}mm";

            // 设置颜色阈值范围
            //levelSeries.LimitHi = 0.15;
            //levelSeries.LimitLo = -0.15;
            // 设置数据绑定源和字段
            levelAASeries.ItemsSource = printLog;
            levelAASeries.DataFieldX = "layer";
            levelAASeries.DataFieldY = "levelAe";
            levelAASeries.Color = OxyColors.Green;

            if (tempOrVaccum)
            {
                // 定义折线图序列，指定数据轴为Y2轴
                var tempSeries = new LineSeries();
                tempSeries.Title = "温度";
                tempSeries.YAxisKey = "y2";
                tempSeries.LineStyle = LineStyle.DashDashDot;
                // 点击时弹出的label内容
                tempSeries.TrackerFormatString = "{0}\r\n时间：{LayerStartTime:HH:mm:ss}\r\n{2:0}层: {4:0.0}℃";
                // 设置颜色
                tempSeries.Color = OxyColors.Blue;
                // 设置数据绑定源和字段
                tempSeries.ItemsSource = printLog;
                tempSeries.DataFieldX = "layer";
                tempSeries.DataFieldY = "resinTemp";

                if (printLog.Count > 0)
                {
                    if (printLog[0].tankTemp1 != 0)
                    {
                        // 定义折线图序列，指定数据轴为Y2轴
                        var tempTankSeries = new LineSeries();
                        tempTankSeries.Title = "槽温度1";
                        tempTankSeries.YAxisKey = "y2";
                        tempTankSeries.LineStyle = LineStyle.DashDashDot;
                        // 点击时弹出的label内容
                        tempTankSeries.TrackerFormatString = "{0}\r\n槽温度1\r\n{2:0}层: {4:0.0}℃";
                        // 设置颜色
                        tempTankSeries.Color = OxyColors.LightBlue;
                        // 设置数据绑定源和字段
                        tempTankSeries.ItemsSource = printLog;
                        tempTankSeries.DataFieldX = "layer";
                        tempTankSeries.DataFieldY = "tankTemp1";
                        model.Series.Add(tempTankSeries);
                    }
                    if (printLog[0].tankTemp2 != 0)
                    {
                        // 定义折线图序列，指定数据轴为Y2轴
                        var tempTankSeries = new LineSeries();
                        tempTankSeries.Title = "槽温度2";
                        tempTankSeries.YAxisKey = "y2";
                        tempTankSeries.LineStyle = LineStyle.DashDashDot;
                        // 点击时弹出的label内容
                        tempTankSeries.TrackerFormatString = "{0}\r\n槽温度2\r\n{2:0}层: {4:0.0}℃";
                        // 设置颜色
                        tempTankSeries.Color = OxyColors.LightBlue;
                        // 设置数据绑定源和字段
                        tempTankSeries.ItemsSource = printLog;
                        tempTankSeries.DataFieldX = "layer";
                        tempTankSeries.DataFieldY = "tankTemp2";
                        model.Series.Add(tempTankSeries);
                    }
                    if (printLog[0].tankTemp3 != 0)
                    {
                        // 定义折线图序列，指定数据轴为Y2轴
                        var tempTankSeries = new LineSeries();
                        tempTankSeries.Title = "槽温度3";
                        tempTankSeries.YAxisKey = "y2";
                        tempTankSeries.LineStyle = LineStyle.DashDashDot;
                        // 点击时弹出的label内容
                        tempTankSeries.TrackerFormatString = "{0}\r\n槽温度3\r\n{2:0}层: {4:0.0}℃";
                        // 设置颜色
                        tempTankSeries.Color = OxyColors.LightBlue;
                        // 设置数据绑定源和字段
                        tempTankSeries.ItemsSource = printLog;
                        tempTankSeries.DataFieldX = "layer";
                        tempTankSeries.DataFieldY = "tankTemp3";
                        model.Series.Add(tempTankSeries);
                    }
                    if (printLog[0].tankTemp4 != 0)
                    {
                        // 定义折线图序列，指定数据轴为Y2轴
                        var tempTankSeries = new LineSeries();
                        tempTankSeries.Title = "槽温度4";
                        tempTankSeries.YAxisKey = "y2";
                        tempTankSeries.LineStyle = LineStyle.DashDashDot;
                        // 点击时弹出的label内容
                        tempTankSeries.TrackerFormatString = "{0}\r\n槽温度4\r\n{2:0}层: {4:0.0}℃";
                        // 设置颜色
                        tempTankSeries.Color = OxyColors.LightBlue;
                        // 设置数据绑定源和字段
                        tempTankSeries.ItemsSource = printLog;
                        tempTankSeries.DataFieldX = "layer";
                        tempTankSeries.DataFieldY = "tankTemp4";
                        model.Series.Add(tempTankSeries);
                    }
                    if (printLog[0].tankTemp5 != 0)
                    {
                        // 定义折线图序列，指定数据轴为Y2轴
                        var tempTankSeries = new LineSeries();
                        tempTankSeries.Title = "槽温度5";
                        tempTankSeries.YAxisKey = "y2";
                        tempTankSeries.LineStyle = LineStyle.DashDashDot;
                        // 点击时弹出的label内容
                        tempTankSeries.TrackerFormatString = "{0}\r\n槽温度5\r\n{2:0}层: {4:0.0}℃";
                        // 设置颜色
                        tempTankSeries.Color = OxyColors.LightBlue;
                        // 设置数据绑定源和字段
                        tempTankSeries.ItemsSource = printLog;
                        tempTankSeries.DataFieldX = "layer";
                        tempTankSeries.DataFieldY = "tankTemp5";
                        model.Series.Add(tempTankSeries);
                    }
                    if (printLog[0].tankTemp6 != 0)
                    {
                        // 定义折线图序列，指定数据轴为Y2轴
                        var tempTankSeries = new LineSeries();
                        tempTankSeries.Title = "槽温度6";
                        tempTankSeries.YAxisKey = "y2";
                        tempTankSeries.LineStyle = LineStyle.DashDashDot;
                        // 点击时弹出的label内容
                        tempTankSeries.TrackerFormatString = "{0}\r\n槽温度6\r\n{2:0}层: {4:0.0}℃";
                        // 设置颜色
                        tempTankSeries.Color = OxyColors.LightBlue;
                        // 设置数据绑定源和字段
                        tempTankSeries.ItemsSource = printLog;
                        tempTankSeries.DataFieldX = "layer";
                        tempTankSeries.DataFieldY = "tankTemp6";
                        model.Series.Add(tempTankSeries);
                    }
                    if (printLog[0].tankTemp7 != 0)
                    {
                        // 定义折线图序列，指定数据轴为Y2轴
                        var tempTankSeries = new LineSeries();
                        tempTankSeries.Title = "槽温度7";
                        tempTankSeries.YAxisKey = "y2";
                        tempTankSeries.LineStyle = LineStyle.DashDashDot;
                        // 点击时弹出的label内容
                        tempTankSeries.TrackerFormatString = "{0}\r\n槽温度7\r\n{2:0}层: {4:0.0}℃";
                        // 设置颜色
                        tempTankSeries.Color = OxyColors.LightBlue;
                        // 设置数据绑定源和字段
                        tempTankSeries.ItemsSource = printLog;
                        tempTankSeries.DataFieldX = "layer";
                        tempTankSeries.DataFieldY = "tankTemp7";
                        model.Series.Add(tempTankSeries);
                    }
                }

                model.Series.Add(tempSeries);
            }
            else
            {
                // 定义折线图序列，指定数据轴为Y2轴
                var vacummSeries = new LineSeries();
                vacummSeries.Title = "真空负压";
                vacummSeries.YAxisKey = "y2";
                // 点击时弹出的label内容
                vacummSeries.TrackerFormatString = "{0}\r\n时间：{LayerStartTime:HH:mm:ss}\r\n{2:0}层: {4:0.00}Pa";
                // 设置颜色
                vacummSeries.Color = OxyColors.Yellow;
                // 设置数据绑定源和字段
                vacummSeries.ItemsSource = printLog;
                vacummSeries.DataFieldX = "layer";
                vacummSeries.DataFieldY = "scraperVacuum";

                model.Series.Add(vacummSeries);
            }


            

            // 下面为手动添加数据方式
            //passedRateSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now.Date.AddDays(-15)), .750));
            // 添加图标资源
            model.Series.Add(levelBASeries);
            model.Series.Add(levelAASeries);
            
            
            model.Axes.Add(ay1);
            model.Axes.Add(ay2);
            model.Axes.Add(ax);
            // 设置图形边框
            model.PlotAreaBorderThickness = new OxyThickness(1, 0, 1, 1);
            return model;
        }
    }
}
