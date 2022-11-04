using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.MES
{
    public class Rt_PrintSchedulesDto
    {
        /// <summary>
        /// 打印机Id
        /// </summary>
        public string printerId { get; set; }
        /// <summary>
        /// 打印机配置信息
        /// </summary>
        public List<DescriptionItem> desc { get; set; }
    }
   
}
