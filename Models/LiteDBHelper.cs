using AngelDLP;
using AngleDLP.Models;
using LiteDB;
using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleDLP
{

    public static class LiteDBHelper
    {
        public class UserInfoDB : UserInfo
         {
            private static string dbString = "UserInfoDB";

            public static UserInfoDB Guest { get; set; } = new UserInfoDB("guest", "Guest", true, UserRole.guest);
            public UserInfoDB(Guid __id, string _userid, String _name, String _account, bool _isMale, UserRole _userRole)
            {
                _id = __id;
                userId = _userid;
                name = _name;
                account = _account;
                isMale = _isMale;
                userRole = _userRole;
            }
            public UserInfoDB()
            {
            }
            public UserInfoDB(string _account, String _name, bool _isMale, UserRole _userRole)
            {
                account = _account;
                name = _name;
                isMale = _isMale;
                userRole = _userRole;
            }
            public UserInfoDB(UserInfo user)
            {
                _id = Guid.NewGuid();
                account = user.account;
                name = user.name;
                isMale = user.isMale;
                userRole = user.userRole;
                userId = user.userId;
            }
            [BsonId]//主键标识
            public Guid _id { get; set; }

            public static bool AddUser(UserInfoDB user)
            {
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<UserInfoDB>(dbString);
                        var userCheck = table.FindOne(a => a.account == user.account);
                        if (userCheck == null)
                        {
                            user.userId = table.Count().ToString("00000");
                            table.Upsert(user);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                
            }

            public static bool AddUser(UserInfo user)
            {
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<UserInfoDB>(dbString);
                        var userCheck = table.FindOne(a => a.userId == user.userId);

                        if (userCheck == null)
                        {
                            UserInfoDB userinfoDB = new UserInfoDB(user);
                            table.Upsert(userinfoDB);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }

            }

            public static Tuple<bool,UserInfoDB> GetUser(string account)
            {
                List<UserInfoDB> userInfos = new List<UserInfoDB>();
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<UserInfoDB>(dbString);
                        var userCheck = table.FindOne(a => a.account == account);

                        if (userCheck != null)
                        {
                            
                            return new Tuple<bool, UserInfoDB>(true, userCheck);
                        }
                        else
                        {
                            return new Tuple<bool, UserInfoDB>(false,new UserInfoDB());
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new Tuple<bool, UserInfoDB>(false, new UserInfoDB());
                }
            }
            public static List<UserInfoDB> GetUsers()
            {
                List<UserInfoDB> userInfos = new List<UserInfoDB>();
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<UserInfoDB>(dbString);

                        userInfos = table.FindAll().ToList();
                        int num = table.Count();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                
               
                return userInfos;
            }
            #region LiteDB
            /// <summary>
            /// 在静态构造函数中调用注册函数，
            /// 确保LiteDB引擎能够找到此类对应的序列化和反序列化函数
            /// </summary>
            static UserInfoDB() => RegisterType();

            /// <summary>
            /// 注册序列化与反序列化函数
            /// </summary>
            public static void RegisterType()
                => BsonMapper.Global.RegisterType(Serialize, Deserialize);

            /// <summary>
            /// 序列化
            /// </summary>
            public static BsonValue Serialize(UserInfoDB parameter)
                => new BsonDocument(new Dictionary<string, BsonValue>
                {
            {"_id", parameter._id}, // 定义需要存储的数据项
            {"userId", parameter.userId}, // 定义需要存储的数据项
            {"name", parameter.name},
            {"account", parameter.account},
            {"isMale", parameter.isMale},
            {"userRole", (Int32)parameter.userRole},
                });

            /// <summary>
            /// 反序列化
            /// </summary>
            public static UserInfoDB Deserialize(BsonValue bsonValue)
            {
                var __id = bsonValue["_id"].AsGuid; // 提取、解析存储的数据项
                var _userId = bsonValue["userId"].AsString; // 提取、解析存储的数据项
                var _name = bsonValue["name"].AsString;
                var _account = bsonValue["account"].AsString;
                var _isMale = bsonValue["isMale"].AsBoolean;
                var _userRole = bsonValue["userRole"].AsInt32;
                return new UserInfoDB(__id, _userId, _name, _account, _isMale, (UserRole)_userRole);
            }
            #endregion
        }

        public class PrintTaskDBInfo
        {
            private static string dbString = "PrintTaskDBInfo";
            public PrintTaskDBInfo()
            {
                
            }
            public PrintTaskDBInfo(PrintTask printTask)
            {
                //GUID = System:Guid(printTask.printtask_GUID);
                printGuid = printTask.printtask_GUID.ToString();
                startTime = printTask.printStartTime;
                layoutJsonStr = JsonConvert.SerializeObject(printTask.layoutItems);
                configName = printTask.configName;
                configVersion = printTask.configVersion;
                printTaskConfigJsonStr = JsonConvert.SerializeObject(printTask._config);
                partNum = printTask.layoutItems.Count;
                configName = printTask.configName;
                configVersion = printTask.configVersion;
                isMesTask = printTask.isMES;
                taskId = printTask.mesTaskId;
                layerCounts = printTask.maxslice;
                layerPrinted = 0;
            }
            public PrintTaskDBInfo(Guid __id, string _guid,string _name,int _partNum, 
                DateTime _startTime,DateTime _finishTime,Int32 _SNinDay,bool _isFinished,
                string _layout,string _config, string _printTaskinfos, string _configName, int _configVersion,
                bool _isMesTask,string _taskId,int _layerCounts,int _layerPrinted)
            {
                _id = __id;
                printGuid = _guid;
                name = _name;
                partNum = _partNum;
                startTime = _startTime;
                finishTime = _finishTime;
                SNinDay = _SNinDay;
                isFinished = _isFinished;
                layoutJsonStr = _layout;
                printTaskConfigJsonStr = _config;
                printTaskinfos = _printTaskinfos;
                configName = _configName;
                configVersion= _configVersion;
                isMesTask= _isMesTask;
                taskId = _taskId;
                layerCounts = _layerCounts;
                layerPrinted= _layerPrinted;
            }
            
            //[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [BsonId]
            public Guid _id { get; set; }
            public string printGuid { get; set; }
            public string name { get; set; }

            public int? partNum { get; set; }
            public DateTime startTime { get; set; }

            public DateTime finishTime { get; set; }

            public bool isFinished { get; set; }
            public int SNinDay { get; set; }

            public string configName { get; set; }
            public int configVersion { get; set; }
            public string printTaskinfos { get; set; }
            public string layoutJsonStr { get; set; }
            public string printTaskConfigJsonStr { get; set; }

            public bool isMesTask { get; set; } = false;

            public string taskId { get; set; }

            public int layerCounts { get; set; }

            public int layerPrinted { get; set; }

            public static string AddPrintTaskInfo( PrintTaskDBInfo printTaskDBInfo)
            {
                try
                {
                    //用print task 新建 printtaskDB info 以当天序号命名
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskDBInfo>(dbString);
                        int todayTaskNum = GetTodayTaskNum(db, printTaskDBInfo.startTime);
                        printTaskDBInfo.SNinDay = todayTaskNum + 1;
                        printTaskDBInfo.name = printTaskDBInfo.startTime.ToString("yyyy-MM-dd") + "-" + printTaskDBInfo.SNinDay.ToString("000");
                        printTaskDBInfo.isFinished = false;
                        table.Upsert(printTaskDBInfo);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                
                return printTaskDBInfo.name;
            }
            public static int GetTodayTaskNum(LiteDatabase db, DateTime dateTime)
            {

                    var table = db.GetCollection<PrintTaskDBInfo>(dbString);

                    var printTaskDBInfos = table.Find(a => a.startTime.Date == dateTime.Date).ToList();
                    int num =printTaskDBInfos.Count();

                return num;
            }

            public static List<PrintTaskDBInfo> GetPrintTaskDBInfos()
            {
                List < PrintTaskDBInfo > printTaskDBInfos = new List < PrintTaskDBInfo >();
                using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                {
                    var table = db.GetCollection<PrintTaskDBInfo>(dbString);

                    printTaskDBInfos = table.Find(a => a.startTime != null).ToList();
                }
                return printTaskDBInfos;
            }

            public static List<PrintTaskDBInfo> GetPrintTaskDBInfos(DateTime dateTime)
            {
                List<PrintTaskDBInfo> printTaskDBInfos = new List<PrintTaskDBInfo>();
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskDBInfo>(dbString);

                        printTaskDBInfos = table.Find(a => a.startTime.Date == dateTime.Date).ToList();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                
                
                return printTaskDBInfos;
            }
            public static PrintTaskDBInfo GetPrintTaskDBInfos( string _guid)
            {
                PrintTaskDBInfo printTaskDBInfo = new PrintTaskDBInfo();
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskDBInfo>(dbString);
                        var lists = table.FindAll().ToList();

                        printTaskDBInfo = lists.FirstOrDefault(a => a.printGuid == _guid);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                return printTaskDBInfo;
            }


            public static bool UpDatePrintTaskProgress(string _guid, int layerPrinted)
            {
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskDBInfo>(dbString);
                        var lists = table.FindAll().ToList();
                        var task = lists.FirstOrDefault(a => a.printGuid == _guid);
                        if (task == null) { return false; }
                        task.layerPrinted = layerPrinted;
                        try
                        {
                            table.Update(task);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                return true;
            }


            public static bool UpDatePrintTaskFinishTime( string _guid,DateTime dateTime,string printTaskinfo)
            {
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskDBInfo>(dbString);
                        var lists = table.FindAll().ToList();
                        var task = lists.FirstOrDefault(a => a.printGuid == _guid);
                        if (task == null) { return false; }
                        task.finishTime = dateTime;
                        task.isFinished = true;
                        task.printTaskinfos = printTaskinfo;
                        try
                        {
                            table.Update(task);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                

                return true;
            }
            public static void DellAll()
            {
                
                using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                {
                    db.Rebuild();
                }
                
            }
            #region LiteDB
            /// <summary>
            /// 在静态构造函数中调用注册函数，
            /// 确保LiteDB引擎能够找到此类对应的序列化和反序列化函数
            /// </summary>
            static PrintTaskDBInfo() => RegisterType();

            /// <summary>
            /// 注册序列化与反序列化函数
            /// </summary>
            public static void RegisterType()
                => BsonMapper.Global.RegisterType(Serialize, Deserialize);

            /// <summary>
            /// 序列化
            /// </summary>
            public static BsonValue Serialize(PrintTaskDBInfo parameter)
                => new BsonDocument(new Dictionary<string, BsonValue>
                {
            {"_id", parameter._id}, // 定义需要存储的数据项
            {"printGuid", parameter.printGuid}, // 定义需要存储的数据项
            {"name", parameter.name},
            {"partNum", parameter.partNum},
            {"startTime", parameter.startTime},
            {"finishTime", parameter.finishTime},
            {"SNinDay", parameter.SNinDay},
            {"isFinished", parameter.isFinished},
            {"layoutJsonStr", parameter.layoutJsonStr},
            {"printTaskConfigJsonStr", parameter.printTaskConfigJsonStr},
            {"printTaskinfos", parameter.printTaskinfos},
            {"configName", parameter.configName},
            {"configVersion", parameter.configVersion},
            {"isMesTask", parameter.isMesTask},
            {"taskId", parameter.taskId},
            {"layerCounts", parameter.layerCounts},
            {"layerPrinted", parameter.layerPrinted},
                });

            /// <summary>
            /// 反序列化
            /// </summary>
            public static PrintTaskDBInfo Deserialize(BsonValue bsonValue)
            {
                var __id = bsonValue["_id"].AsGuid; // 提取、解析存储的数据项
                var _guid = bsonValue["printGuid"].AsString; // 提取、解析存储的数据项
                var _name = bsonValue["name"].AsString;
                var _partNum = bsonValue["partNum"].AsInt32;
                var _startTime = bsonValue["startTime"].AsDateTime;
                DateTime _finishTime = (bsonValue["finishTime"].IsNull)?DateTime.MinValue:bsonValue["finishTime"].AsDateTime;
                var _SNinDays = bsonValue["SNinDay"].AsInt32;
                var _isFinished = (bsonValue["isFinished"].IsNull) ? false : bsonValue["isFinished"].AsBoolean;
                var _layout = bsonValue["layoutJsonStr"].AsString;
                var _config = bsonValue["printTaskConfigJsonStr"].AsString;
                var _printTaskinfos = bsonValue["printTaskinfos"].AsString;
                var _configName = bsonValue["configName"].AsString;
                var _configVersion = bsonValue["configVersion"].AsInt32;
                var _isMesTask =  (bsonValue["isMesTask"].IsNull) ? false : bsonValue["isMesTask"].AsBoolean;
                var _taskId = bsonValue["taskId"].AsString;
                var _layerCounts = bsonValue["layerCounts"].AsInt32;
                var _layerPrinted = bsonValue["layerPrinted"].AsInt32;
                return new PrintTaskDBInfo(__id,_guid,_name, _partNum, _startTime,_finishTime, _SNinDays, 
                    _isFinished,_layout,_config, _printTaskinfos,_configName,_configVersion,
                    _isMesTask,_taskId,_layerCounts,_layerPrinted);
            }
            #endregion
        }

        public class PrintTaskConfigDB : BindableBase
        {
            private static string dbString = "PrintTaskConfigDB";
            public PrintTaskConfigDB(Guid __id, string _name, string _printTaskConfigStr,DateTime _creationTime, DateTime _modifiedTime, int _version,string _creator, bool _isValid)
            {
                _id = __id;
                name = _name;
                printTaskConfigStr = _printTaskConfigStr;
                creationTime = _creationTime;
                modifiedTime = _modifiedTime;
                version = _version;
                creator = _creator;
                isValid = _isValid;
                
            }
            public PrintTaskConfigDB(string _name, string _printTaskConfigStr, DateTime _creationTime, DateTime _modifiedTime, int _version, string _creator, bool _isValid)
            {
                name = _name;
                printTaskConfigStr = _printTaskConfigStr;
                creationTime = _creationTime;
                modifiedTime = _modifiedTime;
                version = _version;
                creator = _creator;
                isValid = _isValid;
            }
            public static PrintTaskConfigDB CopyDB(PrintTaskConfigDB dB,string newName)
            {
                PrintTaskConfigDB res = new PrintTaskConfigDB();
                res.name = newName;
                res.printTaskConfigStr = dB.printTaskConfigStr;
                res.creationTime = DateTime.Now;
                res.modifiedTime = DateTime.Now;
                res.version = 0;
                res.creator = dB.creator;
                res.isValid =  true;
                return res;
            }
            public static PrintTaskConfigDB CopyVersion(PrintTaskConfigDB dB)
            {
                PrintTaskConfigDB res = new PrintTaskConfigDB();
                res.name = dB.name;
                res.printTaskConfigStr = dB.printTaskConfigStr;
                res.creationTime = res.creationTime;
                res.modifiedTime = DateTime.Now;
                res.version = dB.version + 1 ;
                res.creator = dB.creator;
                res.isValid = true;
                return res;
            }
            public PrintTaskConfigDB( )
            {
            }
            [BsonId]//主键标识
            public Guid _id { get; set; }
            public string name { get; set; }//名字

            public string printTaskConfigStr { get; set; }//账号名称
            public DateTime creationTime { get; set; }
            public DateTime modifiedTime { get; set; }
            public int version { get; set; }

            public string creator { get; set; }

            public bool isValid { get; set; }
            
            public static bool AddConfig( PrintTaskConfigDB printTaskConfigDB)
            {
                using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                {
                    var table = db.GetCollection<PrintTaskConfigDB>(dbString);
                    table.Upsert(printTaskConfigDB);
                    return true;
                }
            }
            public static bool SaveAsNewConfig( PrintTaskConfigDB printTaskConfigDB,PrintTaskConfig printTaskConfig,string newName)
            {
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskConfigDB>(dbString);
                        var check = table.FindOne(a => a.name == newName);
                        if (check == null)
                        {
                            PrintTaskConfigDB newDB = CopyDB(printTaskConfigDB, newName);
                            newDB.printTaskConfigStr = Newtonsoft.Json.JsonConvert.SerializeObject(printTaskConfig);
                            table.Upsert(newDB);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                
            }
            public static void SaveAsNewVersion( PrintTaskConfigDB printTaskConfigDB, PrintTaskConfig printTaskConfig)
            {
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskConfigDB>(dbString);
                        var check = table.Find(a => a.name == printTaskConfigDB.name).ToList();
                        var maxV = check.Max(t => t.version);
                        var maxdb = check.FirstOrDefault(t => t.version == maxV);
                        PrintTaskConfigDB newDB = CopyVersion(maxdb);
                        newDB.printTaskConfigStr = Newtonsoft.Json.JsonConvert.SerializeObject(printTaskConfig);
                        table.Upsert(newDB);

                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                
            }
            public static List<PrintTaskConfigDB> GetConfigs()
            {
                List<PrintTaskConfigDB> Infos = new List<PrintTaskConfigDB>();
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskConfigDB>(dbString);

                        Infos = table.FindAll().ToList();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                return Infos;
            }
            public static bool GetConfig( string configName, int ver, ref PrintTaskConfig printTaskConfig )
            {
                
                try
                {
                    List<PrintTaskConfigDB> Infos = new List<PrintTaskConfigDB>();
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskConfigDB>(dbString);

                        Infos = table.FindAll().ToList();
                        PrintTaskConfigDB config = Infos.FirstOrDefault(t => t.name == configName && t.version == ver);
                        if (config != null)
                        {
                            printTaskConfig = PrintTaskConfig.DeserializeObject(config.printTaskConfigStr);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                
            }
            public static bool RemoveConfig(string configName)
            {
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskConfigDB>(dbString);
                        var list = table.Find(s => s.name == configName);
                        foreach (var c in list)
                        {
                            table.Delete(c._id);
                        }

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                
            }

            public static bool RemoveConfigVersion(string configName,int _version)
            {
                try
                {
                    using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                    {
                        var table = db.GetCollection<PrintTaskConfigDB>(dbString);
                        var list = table.Find(s => s.name == configName && s.version == _version);
                        foreach (var c in list)
                        {
                            table.Delete(c._id);
                        }

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                
            }
            public static List<PrintTaskConfigDB> testUpdate()
            {
                List<PrintTaskConfigDB> Infos = new List<PrintTaskConfigDB>();
                using (var db = new LiteDatabase(ClassValue.PrinterDBFile))
                {
                    var table = db.GetCollection<PrintTaskConfigDB>(dbString);

                    var Info = table.FindOne(p=>p.name == "manual-141" && p.version==0);
                    Info.isValid = false;
                    table.Update(Info);
                }
                return Infos;
            }
            #region LiteDB
            /// <summary>
            /// 在静态构造函数中调用注册函数，
            /// 确保LiteDB引擎能够找到此类对应的序列化和反序列化函数
            /// </summary>
            static PrintTaskConfigDB() => RegisterType();

            /// <summary>
            /// 注册序列化与反序列化函数
            /// </summary>
            public static void RegisterType()
                => BsonMapper.Global.RegisterType(Serialize, Deserialize);

            /// <summary>
            /// 序列化
            /// </summary>
            public static BsonValue Serialize(PrintTaskConfigDB parameter)
                => new BsonDocument(new Dictionary<string, BsonValue>
                {
            {"_id", parameter._id}, // 定义需要存储的数据项
            {"name", parameter.name},
            {"printTaskConfigStr", parameter.printTaskConfigStr},
            {"creationTime", parameter.creationTime},
            {"modifiedTime", parameter.modifiedTime},
            {"version", parameter.version},
            {"creator", parameter.creator},
            {"isValid", parameter.isValid},
                });

            /// <summary>
            /// 反序列化
            /// </summary>
            public static PrintTaskConfigDB Deserialize(BsonValue bsonValue)
            {
                var __id = bsonValue["_id"].AsGuid; // 提取、解析存储的数据项
                var _name = bsonValue["name"].AsString;
                var _printTaskConfigStr = bsonValue["printTaskConfigStr"].AsString;
                var _creationTime = bsonValue["creationTime"].AsDateTime;
                var _modifiedTime = bsonValue["modifiedTime"].AsDateTime;
                var _version = bsonValue["version"].AsInt32;
                var _creator = bsonValue["creator"].AsString;
                var _isValid = bsonValue["isValid"].AsBoolean;
                return new PrintTaskConfigDB(__id, _name, _printTaskConfigStr, _creationTime, _modifiedTime, _version, _creator, _isValid);
            }
            #endregion
        }

        static public bool CheckDBfile()
        {
            if (File.Exists(ClassValue.PrinterDBFile))
            {
                return true;
            }
            else
            {
                try
                {
                    //Creat the folder
                    string folderPath = Path.GetDirectoryName(ClassValue.PrinterDBFile);
                    //init the db
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    var db = new LiteDatabase(ClassValue.PrinterDBFile);
                    db.Dispose();
                }
                catch (Exception ex)
                {
                    //throw new Exception(ex.ToString());
                }
                
                return false;
            }
        }

    }



}
