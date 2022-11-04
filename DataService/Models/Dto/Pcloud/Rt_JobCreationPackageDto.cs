using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Models.Dto.Pcloud
{
    public class Rt_JobCreationPackageDto
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
        /// 任务创建列表
        /// </summary>
        public List<JobsItem> jobs { get; set; }
    }

    public class Dimension
    {
        /// <summary>
        /// 
        /// </summary>
        public double x { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double y { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double z { get; set; }
    }

    public class InstancesItem
    {
        /// <summary>
        /// 模型实例变换矩阵
        /// </summary>
        public string matrix { get; set; }
    }

    public class JobModels
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 模型ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string filePath { get; set; }
        /// <summary>
        /// 尺寸
        /// </summary>
        public Dimension dimension { get; set; }
        /// <summary>
        /// 模型实例信息
        /// </summary>
        public List<InstancesItem> instances { get; set; }
    }

    public class Scene
    {
        /// <summary>
        /// 平台尺寸
        /// </summary>
        public Dimension dimension { get; set; }
        /// <summary>
        /// 模型信息
        /// </summary>
        public List<JobModels> models { get; set; }
    }

    public class JobsItem
    {
        /// <summary>
        /// taskid
        /// </summary>
        public string taskId { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string startTime { get; set; }
        /// <summary>
        /// 场景信息
        /// </summary>
        public Scene scene { get; set; }
    }

 
}
