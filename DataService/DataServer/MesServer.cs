using DataService.Common;
using DataService.Models;
using DataService.Models.Dto;
using DataService.Models.Dto.MES;
using DataService.Models.Dto.Pcloud;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DataServer
{
    public class MesServer : IServer
    {
       

        #region 生产相关
        public XnRestfulResult<object> DeletePrintSchedules(string printScheduleId, string deviceId, bool toManual, string reason)
        {
            var http = new ApiHelper();
            var response = http.ApiDelete<object>(ConfigModel.MesHead + $"printSchedules/{printScheduleId}?deviceCode={deviceId}&toManual={toManual}&reason={reason}");
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<object> DeletePrintTask(string taskId, string deviceId, bool toManual, string reason)
        {
            var http = new ApiHelper();
            var response = http.ApiDelete<object>(ConfigModel.MesHead + $"printTasks/{taskId}?deviceCode={deviceId}&toManual={toManual}&reason={reason}");
            string responseValue = response.RawText;
            return response;
        }



        public XnRestfulResult<Re_SubLotInfoDto> GetSublots(string deviceCode, string sublots)
        {
            var http = new ApiHelper();
            var response = http.ApiGet<Re_SubLotInfoDto>(ConfigModel.MesHead + $"sublots/{sublots}?deviceCode={deviceCode}");
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<Re_UserDto> GetUserInfo(string cardId)
        {
            var http = new ApiHelper();
            var response = http.ApiGet<Re_UserDto>(ConfigModel.MesHead + $"users?cardId={cardId}");
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<Re_AddGroupDto> PostGroup(Rt_AddGroupDto rt_AddGroup)
        {
            var http = new ApiHelper();
            var response = http.ApiPost<Re_AddGroupDto>(ConfigModel.MesHead + "groups", rt_AddGroup, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }


        public XnRestfulResult<Re_PrintSchedulesDto> PostPrintSchedules(Rt_PrintSchedulesDto rt_PrintSchedules)
        {
            var http = new ApiHelper();
            var response = http.ApiPost<Re_PrintSchedulesDto>(ConfigModel.MesHead + "printSchedules", rt_PrintSchedules, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            //string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<Re_PrintTaskDto> PostPrintTask(Rt_PrintTaskDto rt_PrintTask)
        {
            var http = new ApiHelper();
            var response = http.ApiPost<Re_PrintTaskDto>(ConfigModel.MesHead + "printTasks", rt_PrintTask, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<object> PostRecord(Rt_ProductionRecordDto rt_ProductionRecord)
        {
            var http = new ApiHelper();
            var response = http.ApiPost<object>(ConfigModel.MesHead + "productionRecords", rt_ProductionRecord, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<object> PutGroup(Rt_UpdateGroupDto rt_UpdateGroup)
        {
            var http = new ApiHelper();
            var response = http.ApiPut<object>(ConfigModel.MesHead + $"groups/{rt_UpdateGroup.groupId}?deviceCode={rt_UpdateGroup.deviceCode}", rt_UpdateGroup.updateGroupDesc, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }
        #endregion

        #region Pcloud
        public XnRestfulResult<object> PutJob(int jobId, Rt_JobProgressPackageDto rt_JobProgressPackage)
        {
            var http = new ApiHelper();
            var response = http.ApiPut<object>(ConfigModel.MesHead + $"api/job/{jobId}", rt_JobProgressPackage, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<object> PutPrintInfo(int PrintId, Rt_PrinterInfoPackageDto rt_PrinterInfo)
        {
            var http = new ApiHelper();
            var response = http.ApiPut<object>(ConfigModel.MesHead + $"api/printer/{PrintId}", rt_PrinterInfo, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<object> PostJob(Rt_JobCreationPackageDto rt_JobCreationPackage)
        {
            var http = new ApiHelper();
            var response = http.ApiPost<object>(ConfigModel.MesHead + "api/job", rt_JobCreationPackage, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<object> PostJobEvent(Rt_JobEventPackageDto rt_JobEventPackage)
        {
            var http = new ApiHelper();
            var response = http.ApiPost<object>(ConfigModel.MesHead + "api/job/event", rt_JobEventPackage, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<Re_PrinterTokenDto> PostPrint(Rt_PrinterRegistrationDto rt_PrinterRegistration)
        {
            var http = new ApiHelper();
            var response = http.ApiPost<Re_PrinterTokenDto>(ConfigModel.MesHead + "api/printer", rt_PrinterRegistration, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<Re_PrinterTokenDto> PostPrint(Dictionary<string, object> rt_PrinterRegistration)
        {
            var http = new ApiHelper();
            var response = http.ApiPost<Re_PrinterTokenDto>(ConfigModel.MesHead + "api/printer", rt_PrinterRegistration, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }
        public XnRestfulResult<object> PostPrintEvent(int printId,Rt_PrinterEventPackageDto rt_PrinterEventPackage)
        {
            var http = new ApiHelper();
            var response = http.ApiPost<object>(ConfigModel.MesHead + $"api/printer/{printId}/event", rt_PrinterEventPackage, EasyHttp.Http.HttpContentTypes.ApplicationJson);
            string responseValue = response.RawText;
            return response;
        }

        public XnRestfulResult<Re_PrinterBonjourDto> GetPrintConfig(int printId)
        {
            var http = new ApiHelper();
            var response = http.ApiGet<Re_PrinterBonjourDto>(ConfigModel.MesHead + $"api/printer/{printId}/bonjour");
            string responseValue = response.RawText;
            return response;
        }

        #endregion

    }
}
