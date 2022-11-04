using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AngleDLP
{
    [Serializable]
    internal class PrinterVarifycation
    {
        public string deviceId { get; set; }
        
        public VarifycationInfo varifycationInfo { get; set; }
        public class VarifycationInfo
        {
            public int VarifycationId { get; set; }

            public string PQnum { get; set; }

            public double ShinkX { get; set; } = 0;

            public double ShinkY { get; set; } = 0;

            public double inflate { get; set; } = 0;

            public double inflateX { get; set; } = 0;

            public double inflateY { get; set; } = 0;
            public double archCompPara { get; set; } = 0;
            public List<STLItem> stls { get; set; }
        }
        [Serializable]
        public class STLItem
        {
            public string stl {get;set;}
            public string name { get; set; }

        }
        public PrinterVarifycation(string deviceId)
        {
            this.deviceId = deviceId;
        }
        public bool CheckNew()
        {

            string poststrBack = Post("http://172.16.9.50:8080/STLmark.asmx/GetVerification", deviceId);

            if (poststrBack.Length > 500)
            {
                varifycationInfo = JsonConvert.DeserializeObject<VarifycationInfo>(poststrBack);
                return true;
            }

            if (poststrBack.Contains("NoNew"))
            {
                return false;
            }
            
            if (poststrBack.Contains("Incomplete"))
            {
                return false;
            }
            return false;

        }
        public static string Post(string url, string jsonParas)
        {
            HttpWebResponse res;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                if (request != null)
                {
                    string retval = null;
                    //  init_Request(ref request);
                    request.Method = "POST";
                    //request.Credentials = new NetworkCredential("xiaofeileung@outlook.com", "Lxh161146");
                    request.ServicePoint.Expect100Continue = false;
                    request.ContentType = "application/x-www-form-urlencoded";
                    //request.ContentType = "application/x-www-form-urlencoded";
                    string paraUrlCoded = jsonParas;//
                    string param = HttpUtility.UrlEncode("deviceId") + "=" + HttpUtility.UrlEncode(paraUrlCoded);
                    var bytes = Encoding.UTF8.GetBytes(param);
                    request.ContentLength = bytes.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    using (var Response = request.GetResponse() as HttpWebResponse)
                    {
                        var httpStatusCode = (int)Response.StatusCode;
                        using (var reader = new StreamReader(Response.GetResponseStream()))
                        {
                            retval = reader.ReadToEnd();
                        }
                    }
                    return retval;
                }
            }
            catch (WebException e)
            {
                res = (HttpWebResponse)e.Response;
                Console.WriteLine(e.Message);
                throw;
            }
            return null;
        }
    }
    public static class HttpRquest
    {
        /// <summary>
        /// get 基本方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string GetModelInfo(string url, ref int status)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                request.Timeout = 5000;
                System.Net.ServicePointManager.Expect100Continue = false;
                if (request != null)
                {
                    string retval = null;
                    request.Method = "GET";
                    request.KeepAlive = true;
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
                    using (var Response = request.GetResponse() as HttpWebResponse)
                    {
                        using (var reader = new StreamReader(Response.GetResponseStream(), Encoding.UTF8))
                        {
                            retval = reader.ReadToEnd();
                        }
                        status = (int)Response.StatusCode;
                    }
                    return retval;
                }
            }
            catch (WebException e)
            {

                var rsp = e.Response as HttpWebResponse;
                if (rsp == null)
                {
                    status = (int)e.Status;
                    Console.WriteLine(status);
                    status = 407;
                    return null;
                }

                var httpStatusCode = (int)rsp.StatusCode;
                status = httpStatusCode;

            }
            return null;
        }
    }
}
