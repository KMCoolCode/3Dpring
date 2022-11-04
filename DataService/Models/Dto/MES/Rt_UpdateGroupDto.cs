using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.MES
{
    public class Rt_UpdateGroupDto
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string deviceCode { get; set; }
        public string groupId { get; set; }
        public List<UpdateGroupDesc> updateGroupDesc { get; set; }
    }

    public class UpdateGroupDesc
    {
        /// <summary>
        /// 光固化ID
        /// </summary>
        public string itemId { get; set; }
        /// <summary>
        /// 操作动作
        /// </summary>
        //[JsonConverter(typeof(StringEnumConverter))]
        //public GroupAction action { get; set; }
    }

    public enum GroupAction
    {
        add,
        delete
    }
}
