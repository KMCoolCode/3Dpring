using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.MES
{
    public class Rt_AddGroupDto
    {
        /// <summary>
        /// 容器策略
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public GroupCategory groupCategory { get; set; }
        /// <summary>
        /// item种类（model, aligner）
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public GroupItemType itemType { get; set; }
        /// <summary>
        /// 容器标识
        /// </summary>
        public string containerId { get; set; }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string deviceId { get; set; }
    }

    public enum GroupCategory
    {
        UVcuringGroup, 
        thermoformmingGroup, 
        trimmingGroup, 
        releasedGroup, 
        polishingGroup, 
        manualPolishingGroup, 
        manualBurnishingGroup
    }

    public enum GroupItemType
    {
        model, aligner
    }
}
