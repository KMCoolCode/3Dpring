using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.MES
{
    public class Re_SubLotInfoDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string category { get; set; }
        /// <summary>
        /// 物料子批次号
        /// </summary>
        public string lotId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string createBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int quantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sublotId { get; set; }
        /// <summary>
        /// 

        /// </summary>
        public string expireDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string storageLocation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int remainingQuantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string createtime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> description { get; set; }
    }
}
