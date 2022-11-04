using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.Pcloud
{
    public class Rt_PrinterEventPackageDto
    {

        /// <summary>
        /// 打印机Token
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 打印机事件列表
        /// </summary>
        public List<EventsItem> events { get; set; }
    }

    public class EventsItem
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string time { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PrinterEventLevel level { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string message { get; set; }
    }

    public enum PrinterEventLevel
    {
        error,
        warning,
        info
    }
}
