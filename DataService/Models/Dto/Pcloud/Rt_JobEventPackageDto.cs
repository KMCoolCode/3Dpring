using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.Pcloud
{
    public class Rt_JobEventPackageDto
    {
        /// <summary>
        /// 打印机ID

        /// </summary>
        public int printerId { get; set; }
        /// <summary>
        /// 打印机Token
        /// </summary>
        public string printerToken { get; set; }
        /// <summary>
        /// 任务创建列表
        /// </summary>
        public List<JobEventItem> events { get; set; }

         
    }

    public class JobEvent
    {
        /// <summary>
        /// 事件类型
        /// </summary>
       [JsonConverter(typeof(StringEnumConverter))]
        public JobEventType type { get; set; }
        /// <summary>
        /// 事件发生时间
        /// </summary>
        public string time { get; set; }
        /// <summary>
        /// 作业进度
        /// </summary>
        public JobProgress progress { get; set; }
        /// <summary>
        /// 完成的模型实例数量
        /// </summary>
        public int completedInstanceCount { get; set; }
        /// <summary>
        /// 作业场景中隐藏不打印的模型
        /// </summary>
        public List<string> hiddenInstances { get; set; }
        /// <summary>
        /// 
        /// </summary
        

        public Result result { get; set; }
    }

    public enum JobEventType
    {
        start,
        resume,
        end
    }

    public class JobEventItem
    {
        /// <summary>
        /// 来源任务ID
        /// </summary>
        public int jobId { get; set; }
        /// <summary>
        /// 事件数据

        /// </summary>
        public JobEvent @event { get; set; }
        

    }

    public class Result
    { 
        [JsonConverter(typeof(StringEnumConverter))]
        public JobResultType type { get; set; }
    }

    public enum JobResultType
    {
        successful,
        failed,
        canceled,
        aborted
    }
   
}
