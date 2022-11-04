using DataService.Models.Dto;
using DataService.Models.Dto.MES;
using DataService.Models.Dto.Pcloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DataServer
{
    public interface IServer
    {
        #region 生产相关
        /// <summary>
        /// 请求打印数据
        /// </summary>
        /// <param name="rt_PrintSchedules"></param>
        /// <returns></returns>
        XnRestfulResult<Re_PrintSchedulesDto> PostPrintSchedules(Rt_PrintSchedulesDto rt_PrintSchedules);
        /// <summary>
        /// 锁定排版
        /// </summary>
        /// <param name="rt_PrintTask"></param>
        /// <returns></returns>
        XnRestfulResult<Re_PrintTaskDto> PostPrintTask(Rt_PrintTaskDto rt_PrintTask);
        /// <summary>
        /// 上传记录
        /// </summary>
        /// <param name="rt_ProductionRecord"></param>
        /// <returns></returns>
        XnRestfulResult<object> PostRecord(Rt_ProductionRecordDto rt_ProductionRecord);
        /// <summary>
        /// 获取树脂详细信息
        /// </summary>
        /// <param name="deviceCode">设备编号</param>
        /// <param name="sublots">树脂批次号</param>
        /// <returns></returns>
        XnRestfulResult<Re_SubLotInfoDto> GetSublots(string deviceCode,string sublots);
        /// <summary>
        /// 根据cardid获取用户信息
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        XnRestfulResult<Re_UserDto> GetUserInfo(string cardId);
        /// <summary>
        /// 绑定料框（新增）
        /// </summary>
        /// <param name="rt_AddGroup"></param>
        /// <returns></returns>
        XnRestfulResult<Re_AddGroupDto> PostGroup(Rt_AddGroupDto rt_AddGroup);
        /// <summary>
        /// 更新料框信息
        /// </summary>
        /// <param name="rt_UpdateGroup"></param>
        /// <returns></returns>
        XnRestfulResult<object> PutGroup(Rt_UpdateGroupDto rt_UpdateGroup);
        /// <summary>
        /// 取消排版锁定
        /// </summary>
        /// <param name="printScheduleId"></param>
        /// <param name="deviceId"></param>
        /// <param name="toManual"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        XnRestfulResult<object> DeletePrintSchedules(string printScheduleId, string deviceId, bool toManual, string reason);
        /// <summary>
        /// 取消打印任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="deviceId"></param>
        /// <param name="toManual"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        XnRestfulResult<object> DeletePrintTask(string taskId, string deviceId,bool toManual,string reason);

        #endregion

        #region Pcloud
        /// <summary>
        /// 注册打印机
        /// </summary>
        /// <param name="rt_PrinterRegistration"></param>
        /// <returns></returns>
        XnRestfulResult<Re_PrinterTokenDto> PostPrint(Rt_PrinterRegistrationDto rt_PrinterRegistration);
        /// <summary>
        /// 打印机上线同步信息
        /// </summary>
        /// <param name="printId"></param>
        /// <returns></returns>
        XnRestfulResult<Re_PrinterBonjourDto> GetPrintConfig(int printId);
        /// <summary>
        /// 更新打印机状态
        /// </summary>
        /// <param name="PrintId"></param>
        /// <param name="rt_PrinterInfo"></param>
        /// <returns></returns>
        XnRestfulResult<object> PutPrintInfo(int PrintId,Rt_PrinterInfoPackageDto rt_PrinterInfo);
        /// <summary>
        /// 提交打印机事件
        /// </summary>
        /// <param name="printId"></param>
        /// <param name="rt_PrinterEventPackage"></param>
        /// <returns></returns>
        XnRestfulResult<object> PostPrintEvent(int printId,Rt_PrinterEventPackageDto rt_PrinterEventPackage);
        /// <summary>
        /// 创建一个或多个打印作业
        /// </summary>
        /// <param name="rt_JobCreationPackage"></param>
        /// <returns></returns>
        XnRestfulResult<object> PostJob(Rt_JobCreationPackageDto rt_JobCreationPackage);

        /// <summary>
        /// 提交打印作业事件
        /// </summary>
        /// <param name="rt_JobEventPackage"></param>
        /// <returns></returns>
        XnRestfulResult<object> PostJobEvent(Rt_JobEventPackageDto rt_JobEventPackage);

        /// <summary>
        /// 更新打印作业进度
        /// </summary>
        /// <param name="PrintId"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        XnRestfulResult<object> PutJob(int jobId, Rt_JobProgressPackageDto rt_JobProgressPackage);

        #endregion
    }
}
