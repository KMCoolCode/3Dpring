using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.Pcloud
{
    public class Rt_JobProgressPackageDto
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
        /// 打印作业进度
        /// </summary>
        public JobProgress progress { get; set; }
    }
    public class JobProgress
    {
        /// <summary>
        /// 总层数
        /// </summary>
        public int totalLayers { get; set; }
        /// <summary>
        /// 开始层数
        /// </summary>
        public int startLayers { get; set; }
        /// <summary>
        /// 已打印层数
        /// </summary>
        public int printedLayers { get; set; }
        /// <summary>
        /// 总打印高度
        /// </summary>
        public double totalHeight { get; set; }
        /// <summary>
        /// 已打印高度(mm)
        /// </summary>
        public double printedHeight { get; set; }
        /// <summary>
        /// 已打印体积(mm3)
        /// </summary>
        public double printedVolume { get; set; }
        /// <summary>
        /// 是否暂停，包括用户暂停和故障暂停
        /// </summary>
        public bool isPaused { get; set; }
    }

   
}
