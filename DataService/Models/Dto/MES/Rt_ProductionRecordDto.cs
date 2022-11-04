using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.MES
{
    public class Rt_ProductionRecordDto
    {
        /// <summary>
        /// 操作员
        /// </summary>
        public string userId { get; set; }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string deviceId { get; set; }
        /// <summary>
        /// 处理类型（光固化：UVcuring 光固化后处理：UVcuringNextTreat）
        /// </summary>
        public string productionType { get; set; }
        /// <summary>
        /// 生产时间。时间格式：1990-12-31T15:59:59.000+08:00
        /// </summary>
        public string creationTime { get; set; }
        /// <summary>
        /// 生产过程数据
        /// </summary>
        public List<DescriptionItem> description { get; set; }
    }

    public class DescriptionItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string value { get; set; }
    }
}
