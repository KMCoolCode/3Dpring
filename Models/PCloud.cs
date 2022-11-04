using AngleDLP.PMC;
using DataService.DataServer;
using DataService.Models;
using DataService.Models.Dto.Pcloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleDLP.Models
{
    public class PCloud
    {
        IServer mesServer = new MesServer();
        
        public PCloud()
        { 
        }

        public Tuple<bool,string> StartUpCheck()
        {
            //打印机上线同步信息（软件启动获取）
            var printConfig = mesServer.GetPrintConfig(DataService.Models.ConfigModel.P_PrintId);
            #region 返回示例
            //{"printerName":"RP41","operatorAuth":{"serviceUrl":"http://mes2-web.sw.eainc.com:8080/mes2/rest/users","expirationMinutes":1440},"cooldownMinutes":1,
            //"taskTags":["tags=第一产线,正常,MCP0.76","maxLimit=56"],"time":"2022-07-01T01:32:20.968Z","fullName":"EA-P-0251",
            //"task":{"serviceUrl":"http://mes2-web.sw.eainc.com:8080/mes2/rest/",
            //"modelServer":"\\\\172.22.0.148\\stl","modelUser":"app_stl","modelPassword":"5MfcTUYXUsmnJ30p"}}
            #endregion
            if (printConfig.Success)
            {
                //update the printer infomation to 
                if (printConfig.Data.fullName != ClassValue.MESConfig.DeviceId)
                {
                    return new Tuple<bool, string>(false, MessageAssemb($"MES信息校验失败,MES编号{printConfig.Data.fullName} vs 本地编号{ClassValue.MESConfig.DeviceId}。"));
                }
                if (printConfig.Data.task.serviceUrl != ClassValue.MESConfig.MesHead)
                {
                    return new Tuple<bool, string>(false, MessageAssemb($"MES信息校验失败,MES头{printConfig.Data.task.serviceUrl} vs 本地头{ClassValue.MESConfig.MesHead}"));
                }
                string connectCode = NetworkShareConnect.connectToShare(printConfig.Data.task.modelServer, printConfig.Data.task.modelUser, printConfig.Data.task.modelPassword);
                if (connectCode!=null)
                {
                    return new Tuple<bool, string>(false, MessageAssemb($"MES文件服务器访问失败,路径{printConfig.Data.task.modelServer} ，错误类型:{connectCode}"));
                }
                ClassValue.printerConfig = printConfig.Data;
                ClassValue.MESConfig.tags = printConfig.Data.taskTags.FirstOrDefault(p => p.Contains("tags")).Split('=')[1];
                ClassValue.MESConfig.maxLimit = printConfig.Data.taskTags.FirstOrDefault(p => p.Contains("maxLimit")).Split('=')[1];
                return new Tuple<bool,string>( true, MessageAssemb("MES信息校验通过"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"MES信息校验失败:{printConfig.RawText}"));
            }
        }
        private string MessageAssemb(string msg)
        {
            return $"PCloud->{ClassValue.MESConfig.DeviceId} {msg}";
        }
        public bool UpdateStatus()
        {
            #region 更新打印机状态
            //更新打印机状态
            //打印机状态更新分为固件信息，生产状态，打印机状态监控
            #region 请求数据示例
            #region 硬件、软件信息（启动时更新）
            //{"token":"613fb118-cf54-4816-bbed-b3888e49cbb7","firmwareProperties":{"prtVersion":"2.5.17.0","expVersion":"2.5.9.0","hasCollector":true,"hasReplenisher":true},"hardwareConnectionStatus":{"connected":true,"updatedTime":"2022-07-01T01:32:43.9632054Z"}}
            #endregion

            #region 生产状态（是否在生产，打印时on，打印完成off）
            // {"token":"613fb118-cf54-4816-bbed-b3888e49cbb7","produceStatus":{"mode":"off","updatedTime":"2022-07-01T01:32:41.5221903Z"}}
            #endregion

            #region 状态监控（6秒更新一次）
            //{"token":"613fb118-cf54-4816-bbed-b3888e49cbb7","hardwareStatus":{"resinTemp":[30.15625,29.53125,33.88125,-10.0],"airTemp":-10.0,"ledTemp":23.76875,"platformTemp":-46.85,"platformHum":-6.0,"resinLevel":99.99901},"alerts":{"replenishment":{"active":true,"updatedTime":"2022-07-01T01:32:44.1121345Z"}}}
            #endregion
            Rt_PrinterInfoPackageDto rt_PrinterInfoPackage = new Rt_PrinterInfoPackageDto()
            {
                token = DataService.Models.ConfigModel.P_PrintToken,
                firmwareProperties = new FirmwareProperties()
                {
                    //固件版本，可根据自定为AM硬件版本
                    prtVersion = "2.5.17.0",
                    expVersion = "2.5.9.0",
                    hasCollector = true,
                    hasReplenisher = true
                },
                hardwareConnectionStatus = new HardwareConnectionStatus()
                {
                    connected = true,
                    updatedTime = DateTime.Now
                }
            };

            #endregion
            var updatePrint = mesServer.PutPrintInfo(DataService.Models.ConfigModel.P_PrintId, rt_PrinterInfoPackage);
            #endregion
            if (updatePrint != null)
            {
                //update the printer infomation to 


                return true;
            }
            else
            {
                return false;
            }

        }
        public Tuple<bool, string> UpdatePrinterInfo(PrinterMotionConfig printMotionConfig)
        {
            #region 更新打印机状态
            //更新打印机状态
            //打印机状态更新分为固件信息，生产状态，打印机状态监控
            #region 请求数据示例
            #region 硬件、软件信息（启动时更新）
            //{"token":"613fb118-cf54-4816-bbed-b3888e49cbb7","firmwareProperties":{"prtVersion":"2.5.17.0","expVersion":"2.5.9.0","hasCollector":true,"hasReplenisher":true},"hardwareConnectionStatus":{"connected":true,"updatedTime":"2022-07-01T01:32:43.9632054Z"}}
            #endregion
            Rt_PrinterInfoPackageDto rt_PrinterInfoPackage = new Rt_PrinterInfoPackageDto()
            {
                token = DataService.Models.ConfigModel.P_PrintToken
                
                
            };

            #endregion
            var updatePrint = mesServer.PutPrintInfo(DataService.Models.ConfigModel.P_PrintId, rt_PrinterInfoPackage);
            #endregion

            if (updatePrint.Success)
            {
                return new Tuple<bool, string>(true, MessageAssemb($"打印机信息更新成功。"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"打印机信息更新失败,{updatePrint.RawText}。"));
            }
            
        }
        public Tuple<bool, string> UpdatePrinterSoftwareRunningtatus(bool running)
        {
            string action = "软件关闭事件"; 
            if (running) {action = "软件启动事件"; }
            Rt_PrinterInfoPackageDto rt_PrinterInfoPackage = new Rt_PrinterInfoPackageDto()
            {
                token = DataService.Models.ConfigModel.P_PrintToken,
                softwareRunningStatus = new SoftwareRunningStatus()
                {
                    //固件版本，可根据自定为AM硬件版本
                    running = running.ToString().ToLower(),
                    updatedTime = TimeHelper.GetNewTimeString()
                },
                softwareProperties = new SoftwareProperties()
                {
                    version = ClassValue.AppVersion,
                    model = "AM400",
                    printDimension = new PrintDimension()
                    {
                        x = 432,
                        y = 284,
                        z = 100
                    }
                }
            };

            var updatePrint = mesServer.PutPrintInfo(DataService.Models.ConfigModel.P_PrintId, rt_PrinterInfoPackage);
           
            if (updatePrint.Success)
            {
                return new Tuple<bool, string>(true, MessageAssemb($"{action}更新成功。"));
            }
            else
            {
                return new Tuple<bool ,string>(false, MessageAssemb($"{action}更新失败,{updatePrint.RawText}。"));
            }

        }

        public Tuple<bool, string> UpdatePrinterHardwareRunningtatus(bool running,PrinterMotionConfig printMotionConfig)
        {
            string action = "硬件断开事件";
            if (running) { action = "硬件连接事件"; }
            Rt_PrinterInfoPackageDto rt_PrinterInfoPackage = new Rt_PrinterInfoPackageDto()
            {
                token = DataService.Models.ConfigModel.P_PrintToken,
                firmwareProperties = new FirmwareProperties()
                {
                    //固件版本，可根据自定为AM硬件版本
                    prtVersion = printMotionConfig.deviceConfig.Version,
                    expVersion = printMotionConfig.systemConfig.Version,
                    hasCollector = false,
                    hasReplenisher = true
                },
                hardwareConnectionStatus = new HardwareConnectionStatus()
                {
                    connected = running,
                    updatedTime = DateTime.Now
                }
            };

            var updatePrint = mesServer.PutPrintInfo(DataService.Models.ConfigModel.P_PrintId, rt_PrinterInfoPackage);

            if (updatePrint.Success)
            {
                return new Tuple<bool, string>(true, MessageAssemb($"{action}更新成功。"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"{action}更新失败,{updatePrint.RawText}。"));
            }

        }

        public Tuple<bool, string> UpdatePrinterHardwareStatus(HardwareStatus hardwareStatus)
        {
            #region 更新打印机状态
            //更新打印机状态
            //打印机状态更新分为固件信息，生产状态，打印机状态监控
            #region 请求数据示例
            #region 硬件、软件信息（启动时更新）
            //{"token":"613fb118-cf54-4816-bbed-b3888e49cbb7","firmwareProperties":{"prtVersion":"2.5.17.0","expVersion":"2.5.9.0","hasCollector":true,"hasReplenisher":true},"hardwareConnectionStatus":{"connected":true,"updatedTime":"2022-07-01T01:32:43.9632054Z"}}
            #endregion
            Rt_PrinterInfoPackageDto rt_PrinterInfoPackage = new Rt_PrinterInfoPackageDto()
            {
                token = DataService.Models.ConfigModel.P_PrintToken,
                hardwareStatus = hardwareStatus
            };

            #endregion
            var updatePrint = mesServer.PutPrintInfo(DataService.Models.ConfigModel.P_PrintId, rt_PrinterInfoPackage);
            #endregion

            if (updatePrint.Success)
            {
                return new Tuple<bool, string>(true, MessageAssemb($"Update hardwareStatus Success。"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"Update hardwareStatus Fail:{updatePrint.RawText}"));
            }

        }
        public Tuple<bool, string> UpdatePrinterProduceMode(PrinterProduceMode produceMode)
        {
            #region 更新打印机状态
            //更新打印机状态
            //打印机状态更新分为固件信息，生产状态，打印机状态监控
            #region 请求数据示例
            #region 硬件、软件信息（启动时更新）
            //{"token":"613fb118-cf54-4816-bbed-b3888e49cbb7","firmwareProperties":{"prtVersion":"2.5.17.0","expVersion":"2.5.9.0","hasCollector":true,"hasReplenisher":true},"hardwareConnectionStatus":{"connected":true,"updatedTime":"2022-07-01T01:32:43.9632054Z"}}
            #endregion
            Rt_PrinterInfoPackageDto rt_PrinterInfoPackage = new Rt_PrinterInfoPackageDto()
            {
                token = DataService.Models.ConfigModel.P_PrintToken,
                produceStatus = new ProduceStatus() {
                    mode = produceMode.ToString(),
                    updatedTime = TimeHelper.GetNewTimeString()
                }

            };

            #endregion
            var updatePrint = mesServer.PutPrintInfo(DataService.Models.ConfigModel.P_PrintId, rt_PrinterInfoPackage);
            #endregion

            if (updatePrint.Success)
            {
                return new Tuple<bool, string>(true, MessageAssemb($"Set {produceMode.ToString()} Success。"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"Set {produceMode.ToString()} Fail:{updatePrint.RawText}"));
            }

        }
        public Tuple<bool, string> UpdatePrinterAlert(PrinterAlerts alertType, bool active)
        {
            #region 更新打印机状态
            //更新打印机状态
            //打印机状态更新分为固件信息，生产状态，打印机状态监控
            #region 请求数据示例
            #region 硬件、软件信息（启动时更新）
            //{"token":"613fb118-cf54-4816-bbed-b3888e49cbb7","firmwareProperties":{"prtVersion":"2.5.17.0","expVersion":"2.5.9.0","hasCollector":true,"hasReplenisher":true},"hardwareConnectionStatus":{"connected":true,"updatedTime":"2022-07-01T01:32:43.9632054Z"}}
            #endregion
            string avtiveStr = active.ToString().ToLower();
            Alerts alerts = new Alerts();
            switch (alertType)
            {
                case PrinterAlerts.replenishing:
                    alerts.replenishing = new Replenishing()
                    {
                        active = avtiveStr,
                        updatedTime = TimeHelper.GetNewTimeString()
                    };
                    break;
                case PrinterAlerts.replenishment:
                    alerts.replenishment = new Replenishment()
                    {
                        active = avtiveStr,
                        updatedTime = TimeHelper.GetNewTimeString()
                    };
                    break;
                case PrinterAlerts.collection:
                    alerts.collection = new Collection()
                    {
                        active = avtiveStr,
                        updatedTime = TimeHelper.GetNewTimeString()
                    };
                    break;
                case PrinterAlerts.reaping:
                    alerts.reaping = new Reaping()
                    {
                        active = avtiveStr,
                        updatedTime = TimeHelper.GetNewTimeString()
                    };
                    break;
            }
            Rt_PrinterInfoPackageDto rt_PrinterInfoPackage = new Rt_PrinterInfoPackageDto()
            {
                token = DataService.Models.ConfigModel.P_PrintToken,
                alerts = alerts
            };

            #endregion
            var updatePrint = mesServer.PutPrintInfo(DataService.Models.ConfigModel.P_PrintId, rt_PrinterInfoPackage);
            #endregion
            

            if (updatePrint.Success)
            {
                return new Tuple<bool, string>(true, MessageAssemb($"Alert {alertType.ToString()} {active.ToString()} Success。"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"Alert {alertType.ToString()}  {active.ToString()} Fail:{updatePrint.RawText}。"));
            }
        }

        public Tuple<bool, string> PushMessage(string mes, PrinterEventLevel lev)
        {
            #region 提交打印机事件（信息，故障，警告）
            #region 请求数据示例
            //{"token":"613fb118-cf54-4816-bbed-b3888e49cbb7","events":[{"time":"2022-07-01T01:32:40.2027548Z","level":"info","message":"软件启动。"}]}
            Rt_PrinterEventPackageDto rt_PrinterEventPackage = new Rt_PrinterEventPackageDto()
            {
                token = DataService.Models.ConfigModel.P_PrintToken,
                events = new List<EventsItem>()
                {
                new EventsItem(){ time= TimeHelper.GetZ0NewTimeString(),level=lev,message=mes}
                }
            };
            #endregion
            //提交打印机事件
            var postPrintEvent = mesServer.PostPrintEvent(DataService.Models.ConfigModel.P_PrintId, rt_PrinterEventPackage);
            #endregion
            if (postPrintEvent.Success)
            {
                return new Tuple<bool, string>(true, MessageAssemb($"Push Message 【{lev}】 '{ mes }' Success。"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"Push Message 【{lev}】  '{ mes }' Fail:{postPrintEvent.RawText}。"));
            }
        }

        public Tuple<bool, string> GetSublots(string sublotId)
        {
            #region 树脂信息获取
            //树脂信息获取
            var sublotsData = mesServer.GetSublots(DataService.Models.ConfigModel.DeviceId, sublotId);
            #region 返回示例
            //{"category":null,"createtime":null,"lotId":"KD220522001","createBy":null,"quantity":500,"sublotId":"JDSCLL00013777|WL-B-52|2315478","expireDate":"2023-04-21","storageLocation":null,"remainingQuantity":500,"description":[{"value":"2","key":"resinCraft"},{"value":"R2","key":"resinType"}]}
            #endregion
            #endregion
            if (sublotsData.Success)
            {
                //ConfigModel.SublotId = sublotsData.Data.sublotId;
                return new Tuple<bool, string>(true, MessageAssemb($"Get Sublots 【{sublotId}】 Success。"));
            }
            else
            {
                return new Tuple<bool, string>(false, MessageAssemb($"Get Sublots 【{sublotId}】 Fail:{sublotsData.RawText}。"));
            }
        }



    }
}
