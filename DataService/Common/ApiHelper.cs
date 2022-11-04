using DataService.Models.Dto;
using EasyHttp.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Common
{
    public class ApiHelper : HttpClient
    {
        public ApiHelper()
        {
            this.Request.Accept = HttpContentTypes.ApplicationJson;
            this.Request.ContentType = "application/json";
        }
        public XnRestfulResult<T> ApiGet<T>(string uri, object query = null)
        {
            try
            {
                var response = ConvertResult<T>(Get(uri, query));
                //日志记录
                System.Diagnostics.Debug.WriteLine("ReqMethod:Get,URL:" + uri + ",\r\n Request:" + response.RawText);
                LogHelper.Write_Info("ReqMethod:Get,URL:" + uri + ",\r\n Request:" + response.RawText);
                return response;
            }
            catch (Exception ex)
            {
                LogHelper.Write_Err($"GET异常，原因{ex.Message}");
                return new Models.Dto.XnRestfulResult<T>()
                {
                    Data = default(T),
                    RawText = null,
                    Success = false,
                    Message = $"GET异常，原因{ex.Message}"
                };
            }
            
        }

        public XnRestfulResult<T> ApiDelete<T>(string uri, object query = null)
        {
            try
            {
                var response = ConvertResult<T>(Delete(uri, query));
                //日志记录
                System.Diagnostics.Debug.WriteLine("ReqMethod:Delete,URL:" + uri + ",\r\n Response:" + response.RawText);
                LogHelper.Write_Info("ReqMethod:Delete,URL:" + uri + ",\r\n Response:" + response.RawText);
                return response;
            }
            catch (Exception ex)
            {
                LogHelper.Write_Err($"Delete异常，原因{ex.Message}");
                return new Models.Dto.XnRestfulResult<T>()
                {
                    Data = default(T),
                    RawText = null,
                    Success = false,
                    Message = $"Delete异常，原因{ex.Message}"
                };
            }

        }

        public XnRestfulResult<T> ApiPost<T>(string uri, object data, string contentType, object query = null)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data);
                var response = ConvertResult<T>(Post(uri, json, contentType));
                //日志记录
                System.Diagnostics.Debug.WriteLine("ReqMethod: Post, URL: " + uri + ", Request: " + JsonConvert.SerializeObject(data)+ "\r\n Response:" + response.RawText);
                LogHelper.Write_Info("ReqMethod: Post, URL: " + uri + ", Request: " + JsonConvert.SerializeObject(data) + "\r\n Response:" + response.RawText);
                return response;
            }
            catch (Exception ex)
            {
                LogHelper.Write_Err($"Post异常，原因{ex.Message}");
                return new Models.Dto.XnRestfulResult<T>()
                {
                    Data = default(T),
                    RawText = null,
                    Success = false,
                    Message = $"Post异常，原因{ex.Message}"
                };
            }
           
        }

        public XnRestfulResult<T> ApiPut<T>(string uri, object data, string contentType, object query = null, log4net.ILog loger = null)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data);
                var response = ConvertResult<T>(Put(uri, json, contentType));
                //日志记录
                Console.WriteLine("ReqMethod: Put, URL: " + uri + ", Request: " + JsonConvert.SerializeObject(data) + "\r\n Response:" + response.RawText);
                LogHelper.Write_Info("ReqMethod: Put, URL: " + uri + ", Request: " + JsonConvert.SerializeObject(data) + "\r\n Response:" + response.RawText);
                return response;
            }
            catch (Exception ex)
            {
                LogHelper.Write_Err($"PUT异常，原因{ex.Message}");
                return new Models.Dto.XnRestfulResult<T>()
                {
                    Data = default(T),
                    RawText = null,
                    Success = false,
                    Message = $"PUT异常，原因{ex.Message}"
                };
            }
            
        }

        private XnRestfulResult<T> ConvertResult<T>(HttpResponse httpResponse)
        {
            try
            {
                XnRestfulResult<T> xnRestfulResult = new Models.Dto.XnRestfulResult<T>()
                {
                    Code = httpResponse.StatusCode,
                    RawText = httpResponse.RawText,
                    Success = false,
                    Timestamp = httpResponse.Date.Millisecond
                };
                //httpcoded
                //2开头 （请求成功）表示成功处理了请求的状态代码。
                //3开头 （请求被重定向）表示要完成请求，需要进一步操作。 通常，这些状态代码用来重定向。
                //4开头 （请求错误）这些状态代码表示请求可能出错，妨碍了服务器的处理。
                //5开头（服务器错误）这些状态代码表示服务器在尝试处理请求时发生内部错误。 这些错误可能是服务器本身的错误，而不是请求出错。
                if ((int)httpResponse.StatusCode < 300)
                {
                    xnRestfulResult.Success = true;
                    if (!string.IsNullOrEmpty(httpResponse.RawText))
                    {
                        //xnRestfulResult.Message = JsonConvert.DeserializeObject<XnRestfulResult<object>>(httpResponse.RawText).Message;
                        xnRestfulResult.Data = JsonConvert.DeserializeObject<T>(httpResponse.RawText);

                    }

                }

                return xnRestfulResult;
            }
            catch (Exception ex)
            {
                return  new Models.Dto.XnRestfulResult<T>()
                {
                    Code = httpResponse.StatusCode,
                    Data = default(T),
                    RawText = null,
                    Success = false,
                    Message=$"api调用异常，原因{ex.Message}"
                };
            }
        }
    }
}
