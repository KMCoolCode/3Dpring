using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.MES
{
    public class Re_UserDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> functions { get; set; }
        /// <summary>
        /// 业务上的组
        /// </summary>
        public List<string> groups { get; set; }

        /// <summary>
        /// 权限
        /// </summary>
        public List<string> roles { get; set; }

        /// <summary>
        /// 权限code
        /// </summary>
        public List<string> roleCodes { get; set; }
    }
}
