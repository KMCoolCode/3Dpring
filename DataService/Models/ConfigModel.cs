using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models
{
    public class ConfigModel
    {
        /// <summary>
        /// Head 
        /// </summary>
        public static string MesHead{get;set;}
        /// <summary>
        /// 设备编号
        /// </summary>
        public static string DeviceId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public static string UserId { get; set; }
        /// <summary>
        /// 树脂批次号
        /// </summary>
        public static string SublotId { get; set; }
        /// <summary>
        /// Pcloud中打印机token
        /// </summary>
        public static string P_PrintToken { get; set; }
        /// <summary>
        /// Pcloud中打印机Id
        /// </summary>
        public static int P_PrintId { get; set; }
    }
}
