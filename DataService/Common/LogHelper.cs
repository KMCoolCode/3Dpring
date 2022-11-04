using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Common
{
    public class LogHelper
    {
        private static log4net.ILog mesLogger = log4net.LogManager.GetLogger("logMES");//获取一个日志记录器


        public static void Write_Info(string str1)
        {
            try
            {
                mesLogger.Info(str1 + "　时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " \r\n");//将传入的字符串加上时间写入文本文件一行  
            }
            catch { }
        }

        public static void Write_Err(string str1)
        {
            try
            {
                mesLogger.Error(str1 + "　时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " \r\n");//将传入的字符串加上时间写入文本文件一行  
            }
            catch { }
        }
        //public static void Write_Info(string str1)
        //{
        //    try
        //    {
        //        string pathfile = Directory.GetCurrentDirectory() + "/ApiLogs/";
        //        string strYMD = DateTime.Now.ToString("yyyyMMdd");
        //        string FILE_NAME = "info" + strYMD + ".txt";//每天按照日期建立一个不同的文件名  
        //        StreamWriter sr;

        //        if (Directory.Exists(pathfile) == false)//如果不存在就创建file文件夹{
        //            Directory.CreateDirectory(pathfile);


        //        if (System.IO.File.Exists(pathfile + FILE_NAME)) //如果文件存在,则创建File.AppendText对象
        //        {
        //            sr = System.IO.File.AppendText(pathfile + FILE_NAME);

        //        }


        //        else  //如果文件不存在,则创建File.CreateText对象 
        //        {

        //            sr = System.IO.File.CreateText(pathfile + FILE_NAME);

        //        }

        //        sr.WriteLine(str1 + "　时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " \r\n");//将传入的字符串加上时间写入文本文件一行  


        //        sr.Close();
        //    }
        //    catch { }


        //}

        //public static void Write_Err(string str1)
        //{
        //    try
        //    {
        //        string pathfile = Directory.GetCurrentDirectory() + "/ApiLogs/";
        //        string strYMD = DateTime.Now.ToString("yyyyMMdd");
        //        string FILE_NAME = "err" + strYMD + ".txt";//每天按照日期建立一个不同的文件名  
        //        StreamWriter sr;

        //        if (Directory.Exists(pathfile) == false)//如果不存在就创建file文件夹{
        //            Directory.CreateDirectory(pathfile);


        //        if (System.IO.File.Exists(pathfile + FILE_NAME)) //如果文件存在,则创建File.AppendText对象
        //        {
        //            sr = System.IO.File.AppendText(pathfile + FILE_NAME);

        //        }


        //        else  //如果文件不存在,则创建File.CreateText对象 
        //        {

        //            sr = System.IO.File.CreateText(pathfile + FILE_NAME);

        //        }

        //        sr.WriteLine(str1 + "　时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " \r\n");//将传入的字符串加上时间写入文本文件一行  


        //        sr.Close();
        //    }
        //    catch { }


        //}

    }
}
