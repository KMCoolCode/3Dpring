using AngleDLP;
using AngleDLP.Models;
using Prism.Commands;
using Prism.Events;
using ReadIDCard;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UsingCompositeCommands.Core;
using static AngelDLP.ViewModels.MainWindowViewModel;

namespace AngelDLP.Views
{
    /// <summary>
    /// PrintTaskSetup.xaml 的交互逻辑
    /// </summary>
    public partial class UserLogin : Window
    {
       
        public bool GetResult { get; set; } = false;
        public bool IsClosed { get; set; } = false;
        ReadIDCardHelper readIDCardHelper = new ReadIDCardHelper();
       
        public UserLogin()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Closed += MyWindow_Closed;
            InitializeComponent();
           
            
            if (!ClassValue.AccessAppSettings())
            {
                //DialogHelper.ShowMessageDialog("错误", "Ini 文件读取错误!!", DialogHelper.DialogType.Yes);
                return;
            }//获取配置文件路径及MES头
            string CardReaderCom = IniConfigHelper.ReadSetting(ClassValue.IniFile, "PrinterConfigs", "CardReaderCom", "");
            string CardReaderBandrate = IniConfigHelper.ReadSetting(ClassValue.IniFile, "PrinterConfigs", "CardReaderBandrate", "");
            if (CardReaderCom != "" && CardReaderBandrate != "")
            {
                if (readIDCardHelper.Open(CardReaderCom, int.Parse(CardReaderBandrate)))
                {
                    readIDCardHelper.EventCardMes += new ReadIDCardHelper.SendMessage(ReadIdData);
                }
            }
        }
        public void ReadIdData(string data)
        {
            
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.login.passwordBox.Password = "";
                App.login.id_TextBox.Text = data;
                App.login.Confirm_Click(new object(), new RoutedEventArgs());
                //if (res.Item1)
                //{
                //    App.login.Close();
                //}
                //else
                //{
                //    App.login.id_TextBox.Text = "";
                //    App.login.tips_Label.Text = res.Item2;
                //    App.login.tips_Label.Foreground = new SolidColorBrush(Colors.Red);
                //    //显示信息,显示错误动画
                //    WindowAnimation.WindowShake(App.login);
                //}
            }));
            
            
            



        }
        private void MyWindow_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
            readIDCardHelper.Close();
        }
       
        public void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password == "")
            {
                //var c = LiteDBHelper.UserInfoDB.GetUsers();
                var resLocal = LiteDBHelper.UserInfoDB.GetUser(id_TextBox.Text);
                if (resLocal.Item1)
                {
                    ClassValue.userinfo = resLocal.Item2;
                         this.DialogResult = true;
                    GetResult = true;

                }
                var res = UserInfo.CheckMES(id_TextBox.Text);

                if (res.Item1)
                {
                    this.DialogResult = true;
                    GetResult = true;
                    //this.Close();

                }
                else
                {
                    id_TextBox.Text = "";
                    tips_Label.Text = res.Item2;
                    tips_Label.Foreground = new SolidColorBrush(Colors.Red);
                    //显示信息,显示错误动画
                    WindowAnimation.WindowShake(this);

                }
            }
            else
            {
                //tips_Label.Text = "暂时不支持账号密码登录";
                //tips_Label.Foreground = new SolidColorBrush(Colors.Red);
                ////显示信息,显示错误动画
                //WindowAnimation.WindowShake(this);
                if (id_TextBox.Text == "simulate" && passwordBox.Password == "simulate")
                {
                    this.DialogResult = true;
                    ClassValue.userinfo = new UserInfo();
                    ClassValue.userinfo.userRole = UserRole.administrator;
                    ClassValue.userinfo.name = "simulate";
                    ClassValue.userinfo.userId = "001";
                    ClassValue.EnableSimulate = true;
                    GetResult = true;
                    return;
                }
                else
                {
                    tips_Label.Text = "账号密码错误";
                    tips_Label.Foreground = new SolidColorBrush(Colors.Red);
                    //显示信息,显示错误动画
                    WindowAnimation.WindowShake(this);

                }
            }


        }

        private void passwordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Return && e.Key != Key.Enter)
                return;
            else
            {
                Confirm_Click(sender,e);
            }
            
        }
        void story_Completed(object sender, EventArgs e)
        {
            DoubleAnimation DAnimation = new DoubleAnimation();
            DAnimation.From = 280;//起点
            DAnimation.To = 100;//终点
            DAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));//时间

            Storyboard.SetTarget(DAnimation, Confirm_Button);
            Storyboard.SetTargetProperty(DAnimation, new PropertyPath(Canvas.LeftProperty));
            Storyboard story = new Storyboard();

            story.Completed += new EventHandler(storyCompleted);//完成后要做的事
            //story.RepeatBehavior = RepeatBehavior.Forever;//无限次循环，需要的自己加上
            story.Children.Add(DAnimation);
            story.Begin();
        }

        void storyCompleted(object sender, EventArgs e)
        {
            DoubleAnimation DAnimation = new DoubleAnimation();
            DAnimation.From = 100;//起点
            DAnimation.To = 200;//终点
            DAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));//时间

            Storyboard.SetTarget(DAnimation, Confirm_Button);
            Storyboard.SetTargetProperty(DAnimation, new PropertyPath(Canvas.LeftProperty));
            Storyboard story = new Storyboard();

            //story.Completed += new EventHandler(storyCompleted);//完成后要做的事
            //story.RepeatBehavior = RepeatBehavior.Forever;//无限次循环，需要的自己加上
            story.Children.Add(DAnimation);
            story.Begin();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            GetResult = false;
            this.Close();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            GetResult = false;
            this.Close();
        }

        private void id_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return && e.Key != Key.Enter)
                return;
            else
            {
                Confirm_Click(sender, e);
            }
        }
    }
}
