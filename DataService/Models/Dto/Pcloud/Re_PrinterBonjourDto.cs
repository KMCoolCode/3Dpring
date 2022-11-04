using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.Pcloud
{
    public class Re_PrinterBonjourDto
    {
        /// <summary>
        /// 打印机名称
        /// </summary>
        public string printerName { get; set; }
        /// <summary>
        /// 操作员身份认证设置
        /// </summary>
        public OperatorAuth operatorAuth { get; set; }
        /// <summary>
        /// 打印任务冷却时间
        /// </summary>
        public int cooldownMinutes { get; set; }
        /// <summary>
        /// 打印任务标签
        /// </summary>
        public List<string> taskTags { get; set; }
        /// <summary>
        /// 服务器时间
        /// </summary>
        public DateTime time { get; set; }
        /// <summary>
        /// 打印机全名/编号
        /// </summary>
        public string fullName { get; set; }
        /// <summary>
        /// 打印任务设置
        /// </summary>
        public TaskConfig task { get; set; }
    }

    public class OperatorAuth
    {
        /// <summary>
        /// 过期分钟数
        /// </summary>
        public int expirationMinutes { get; set; }
        /// <summary>
        /// 人员验证uri
        /// </summary>
        public string serviceUrl { get; set; }
    }

    public class TaskConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public string modelUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string modelPassword { get; set; }
        /// <summary>
        /// 模型路径
        /// </summary>
        public string modelServer { get; set; }
        /// <summary>
        /// mes地址
        /// </summary>
        public string serviceUrl { get; set; }
    }
}
