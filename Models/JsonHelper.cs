using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace AngleDLP.Models
{
    public static class JsonHelper
    {
        static JsonHelper()
        {
            Newtonsoft.Json.JsonSerializerSettings setting = new Newtonsoft.Json.JsonSerializerSettings();
            JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
            {
                //日期类型默认格式化处理
                setting.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                setting.DateFormatString = "yyyy-MM-dd HH:mm:ss";


                //空值处理
                setting.NullValueHandling = NullValueHandling.Ignore;

                //高级用法九中的Bool类型转换 设置
                //setting.Converters.Add(new BoolConvert("是,否"));

                if (setting.Converters.FirstOrDefault(p => p.GetType() == typeof(JsonCustomDoubleConvert)) == null)
                {
                    setting.Converters.Add(new JsonCustomDoubleConvert(3));
                }

                return setting;
            });
        }

        public static String ToJsonStr<T>(this T obj) where T : class
        {
            if (obj == null)
                return string.Empty;
            return JsonConvert.SerializeObject(obj, Formatting.Indented);

        }
        public static T ToInstance<T>(this String jsonStr) where T : class
        {
            if (string.IsNullOrEmpty(jsonStr))
                return null;
            try
            {
                var instance = JsonConvert.DeserializeObject<T>(jsonStr);

                return instance;
            }
            catch
            {
                return null;
            }

        }
    }

    /// <summary>
    /// 自定义数值类型序列化转换器(默认保留3位)
    /// </summary>
    public class JsonCustomDoubleConvert : CustomCreationConverter<double>
    {
        /// <summary>
        /// 序列化后保留小数位数
        /// </summary>
        public virtual int Digits { get; private set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public JsonCustomDoubleConvert()
        {
            this.Digits = 3;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="digits">序列化后保留小数位数</param>
        public JsonCustomDoubleConvert(int digits)
        {
            this.Digits = digits;
        }

        /// <summary>
        /// 重载是否可写
        /// </summary>
        public override bool CanWrite { get { return true; } }

        /// <summary>
        /// 重载创建方法
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override double Create(Type objectType)
        {
            return 0.0;
        }

        /// <summary>
        /// 重载序列化方法
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var formatter = ((double)value).ToString("N" + Digits.ToString());
                writer.WriteValue(formatter);
            }

        }

        
    }

    public class JsonCustomMatrix3DConvert : CustomCreationConverter<Matrix3D>
    {
        /// <summary>
        /// 序列化后保留小数位数
        /// </summary>
        public virtual int Digits { get; private set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public JsonCustomMatrix3DConvert()
        {
            this.Digits = 3;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="digits">序列化后保留小数位数</param>
        public JsonCustomMatrix3DConvert(int digits)
        {
            this.Digits = digits;
        }

        /// <summary>
        /// 重载是否可写
        /// </summary>
        public override bool CanWrite { get { return true; } }

        /// <summary>
        /// 重载创建方法
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override Matrix3D Create(Type objectType)
        {
            return new Matrix3D();
        }

        /// <summary>
        /// 重载序列化方法
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                string formatter = string.Empty;
                Matrix3D temp = (Matrix3D)value;
                formatter += (temp.M11).ToString("0.##########") ; formatter += ",";
                formatter += (temp.M12).ToString("0.##########"); formatter += ",";
                formatter += (temp.M13).ToString("0.##########"); formatter += ",";
                formatter += (temp.M14).ToString("0.##########"); formatter += ",";
                formatter += (temp.M21).ToString("0.##########"); formatter += ",";
                formatter += (temp.M22).ToString("0.##########"); formatter += ",";
                formatter += (temp.M23).ToString("0.##########"); formatter += ",";
                formatter += (temp.M24).ToString("0.##########"); formatter += ",";
                formatter += (temp.M31).ToString("0.##########"); formatter += ",";
                formatter += (temp.M32).ToString("0.##########"); formatter += ",";
                formatter += (temp.M33).ToString("0.##########"); formatter += ",";
                formatter += (temp.M34).ToString("0.##########"); formatter += ",";
                formatter += (temp.OffsetX).ToString("0.###"); formatter += ",";
                formatter += (temp.OffsetY).ToString("0.###"); formatter += ",";
                formatter += (temp.OffsetZ).ToString("0.###"); formatter += ",";
                formatter += (temp.M44).ToString("0.##########"); 
                writer.WriteValue(formatter);
            }

        }
    }

    /// <summary>
    /// 自定义数值类型序列化转换器(无小数位)
    /// </summary>
    public class JsonCustomDoubleWith0DigitsConvert : JsonCustomDoubleConvert
    {
        public override int Digits
        {
            get { return 0; }
        }
    }

    /// <summary>
    /// 自定义数值类型序列化转换器(保留1位)
    /// </summary>
    public class JsonCustomDoubleWith1DigitsConvert : JsonCustomDoubleConvert
    {
        public override int Digits
        {
            get { return 1; }
        }
    }

    /// <summary>
    /// 自定义数值类型序列化转换器(保留2位)
    /// </summary>
    public class JsonCustomDoubleWith2DigitsConvert : JsonCustomDoubleConvert
    {
        public override int Digits
        {
            get { return 2; }
        }
    }

    /// <summary>
    /// 自定义数值类型序列化转换器(保留3位)
    /// </summary>
    public class JsonCustomDoubleWith3DigitsConvert : JsonCustomDoubleConvert
    {
        public override int Digits
        {
            get { return 3; }
        }
    }

    /// <summary>
    /// 自定义数值类型序列化转换器(保留4位)
    /// </summary>
    public class JsonCustomDoubleWith4DigitsConvert : JsonCustomDoubleConvert
    {
        public override int Digits
        {
            get { return 4; }
        }
    }

    /// <summary>
    /// 自定义数值类型序列化转换器(保留5位)
    /// </summary>
    public class JsonCustomDoubleWith5DigitsConvert : JsonCustomDoubleConvert
    {
        public override int Digits
        {
            get { return 5; }
        }
    }

}
