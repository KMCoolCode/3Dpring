using AngelDLP;
using DataService.DataServer;
using DataService.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleDLP.Models
{
    public enum UserRole { administrator, printerOperator, guest, mesUser };
    [Serializable]
    public class UserInfo : INotifyPropertyChanged
    {
        //public string userId { get; set; }
        private string _userId;
        public string userId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("userId"));
            }
        }
        //public string name { get; set; }//名字
        private string _name;
        public string name
        {
            get { return _name; }
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("name"));
            }
        }

        public string account { get; set; }//账号名称
        public Boolean isMale { get; set; }
        //public UserRole userRole { get; set; }
        private UserRole _userRole;
        public UserRole userRole
        {
            get { return _userRole; }
            set
            {
                _userRole = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("userRole"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private List<string> _functions;
        public List<string> functions
        {
            get { return _functions; }
            set
            {
                _functions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("functions"));
            }
        }
        /// <summary>
        /// 业务上的组
        /// </summary>
        private List<string> _groups;
        public List<string> groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("groups"));
            }
        }
        /// <summary>
        /// 权限
        /// </summary>
        private List<string> _roles;
        public List<string> roles
        {
            get { return _roles; }
            set
            {
                _roles = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("roles"));
            }
        }
        /// <summary>
        /// 权限code
        /// </summary>
        private List<string> _roleCodes;
        public List<string> roleCodes
        {
            get { return _roleCodes; }
            set
            {
                _roleCodes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("roleCodes"));
            }
        }
        public UserInfo(){
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static Tuple<bool,string> CheckMES(string userID)
        {
            IServer mesServer = new MesServer();
            var userData = mesServer.GetUserInfo(userID);
            if (userData.Success)
            {
                //调用成功
                //使用数据Data
                var userinfo = userData.Data;
                ClassValue.userinfo = new UserInfo()
                {
                    userId = userinfo.userId,
                    userRole = UserRole.mesUser,
                    name = userinfo.userName,
                    functions = userinfo.functions,
                    groups = userinfo.groups,
                    roles = userinfo.roles,
                    roleCodes = userinfo.roleCodes
                };
                ClassValue.userinfo.userId = userinfo.userId;
                return new Tuple<bool, string>(true, $"{ClassValue.userinfo.name},MES校验成功!");
            }
            else
            {
                //失败流程
                return new Tuple<bool, string>(false, $"MES人员校验失败，status{userData.Code.ToString()},失败原因：{userData.RawText}");
            }
        }

    }
}
