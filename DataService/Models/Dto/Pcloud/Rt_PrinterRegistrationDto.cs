using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.Pcloud
{
    public class Rt_PrinterRegistrationDto
    {
        /// <summary>
        /// 旧打印机ID，用于重新关联已注册的打印机
        /// </summary>
        public int existPrinterId { get; set; }
        /// <summary>
        /// 新注册打印机名称
        /// </summary>
        public string newPrinterName { get; set; }
        /// <summary>
        /// 管理员用户名
        /// </summary>
        public string adminUsername { get; set; }
        /// <summary>
        /// 管理员密码
        /// </summary>
        public string adminPassword { get; set; }
    }
}
