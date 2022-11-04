using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto
{
    /// <summary>
    /// RESTful风格返回格式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class XnRestfulResult<T>
    {
        /// <summary>
        /// 执行成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public HttpStatusCode Code { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// 原始数据
        /// </summary>
        public string RawText { get; set; }
    }
}
