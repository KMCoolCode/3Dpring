using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace DataService.Models.Dto.MES
{
    public class Re_PrintSchedulesDto
    {
        /// <summary>
        /// 放行批次
        /// </summary>
        public string releaseBatchId { get; set; }
        /// <summary>
        /// 排版策略（auto:允许当排不下的时候，剔除的是最后几个，但是如果最后几个中发现有个小的刚好能塞进去，就会加进去。 fixed：严格按照mes返回的牙模ID顺序进行排版，不允许发现小的塞进去。）
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ScheduleStrategy scheduleStrategy { get; set; }
        /// <summary>
        /// 排版id
        /// </summary>
        public string scheduleId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ScheduleDescItem> desc { get; set; }
    }

    public enum ScheduleStrategy
    {
        auto,
        @fixed
    }


    public class ScheduleDescItem
    {
        /// <summary>
        /// 是否标准件（true:是标准件，false:非标准件）
        /// </summary>
        public bool isStandard { get; set; }
        /// <summary>
        /// 标准件位置: 标准件按照给定的九宫格位置打印，供打印人员确认打印机是否正常
        /// </summary>
        public string standardPosition { get; set; }
        /// <summary>
        /// 预处理版本（1：旧预处理，2：新预处理）
        /// </summary>
        public int pretreatmentVersion { get; set; }
        /// <summary>
        /// 光固化ID
        /// </summary>
        public string modelId { get; set; }
        /// <summary>
        /// stl路径
        /// </summary>
        public string stl { get; set; }
    }

}
