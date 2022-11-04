using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.MES
{
    public class Rt_PrintTaskDto
    {
        /// <summary>
        /// 打印机ID
        /// </summary>
        public string printerId { get; set; }
        /// <summary>
        /// scheduleId
        /// </summary>
        public string scheduleId { get; set; }
        /// <summary>
        /// 正常牙模排版
        /// </summary>
        public List<ModelsItem> models { get; set; }
        /// <summary>
        /// 异常牙模排版
        /// </summary>
        public List<BadModelsItem> badModels { get; set; }
    }
    public class ModelsItem
    {
        /// <summary>
        /// 牙模ID
        /// </summary>
        public string modelId { get; set; }
        /// <summary>
        /// 牙模排版时摆放的位置,编码成字符串的变换矩阵。
        /// </summary>
        public string position { get; set; }
        
    }

    public class BadModelsItem
    {
        /// <summary>
        /// 牙模ID
        /// </summary>
        public string modelId { get; set; }
        /// <summary>
        /// 牙模排版时摆放的位置,编码成字符串的变换矩阵。
        /// </summary>
        public string position { get; set; }
        /// <summary>
        /// 排版失败原因
        /// </summary>
        public string reason { get; set; }
    }

}
