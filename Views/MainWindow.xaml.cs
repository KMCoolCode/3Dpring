using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Commands;
using Prism.Events;
using AngelDLP.ViewModels;
using Prism.Mvvm;
using Prism.Regions;
using HelixToolkit.Wpf;
using UsingCompositeCommands.Core;
using static AngelDLP.ViewModels.MainWindowViewModel;
using HelixToolkit.Wpf.SharpDX;
using static AngelDLP.ViewModels.PrintScene3DViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static AngelDLP.ViewModels.PrintControlViewModel;
using AngleDLP.Models;
using AngelDLP.PMC;
using System.Diagnostics;
using System.Windows.Forms;
using static AngelDLP.ViewModels.ProjCalibratorViewModel;
using System.Threading;

namespace AngelDLP.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        IEventAggregator _ea;

        public List<SubViewModel> subViewModels = new List<SubViewModel>();
        public event PropertyChangedEventHandler PropertyChanged;
        
        
        List<string> subViews = new List<string>() { "ImportParts_Button", "PrintSetup_Button", "PrintTaskMonitor_Button", "printControl_Button", "Setting_Button", "Info_Button", "User_Button" };
        public class UpdateProgressViewEvent : PubSubEvent<ProgressViewModel>
        {

        }


        [Obsolete]
        public MainWindow(IEventAggregator ea)
        {
            _ea = ea;
            InitializeComponent();
            //this.DataContext = new MainWindowViewModel(ea, partModelFactory, printerProjectionControl, printerMotionControl);
            //_ea.GetEvent<ViewSwitchEvent>().Subscribe(SubViewSwitch);
            //InitializeComponent();
            _ea.GetEvent<ViewSwitchEvent>().Subscribe(SubViewSwitch);
            _ea.GetEvent<UpdateTransformUIEvent>().Subscribe(trans =>
            {
                ScrollViewer scrollViewer = GetScrollViewer(Models_ListView) as ScrollViewer;
                Models_ListView.SelectedIndex = trans.index;
                scrollViewer.ScrollToVerticalOffset(this.Models_ListView.SelectedIndex * 150);
            });
           
            _ea.GetEvent<UpdateProgressViewEvent>().Subscribe(pvm =>
            {
                //this.Dispatcher = pvm;
            });
            
            subViewModels.Add(new SubViewModel(PrintSetup_Button, printTaskSetupView));
            subViewModels.Add(new SubViewModel(PrintTaskMonitor_Button, printTaskMonitorView));
            subViewModels.Add(new SubViewModel(printControl_Button, printControlView));
            subViewModels.Add(new SubViewModel(History_Button, historyView));
            subViewModels.Add(new SubViewModel(Setting_Button, settingView));
            subViewModels.Add(new SubViewModel(Info_Button, infoView));
            subViewModels.Add(new SubViewModel(User_Button, userView));
            

        }

        

        private void Window_Initialized(object sender, EventArgs e)
        {
            
            
           
        }
        private void WindowLoaded(object sender, EventArgs e)
        {
            _ea.GetEvent<StartAPPViewEvent>().Publish();
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            _ea.GetEvent<AppLogEvent>().Publish("按下关闭按钮");

        }
        private void Window_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            try
            {
                _ea.GetEvent<AppLogEvent>().Publish("按下关闭按钮");
                _ea.GetEvent<UICloseEvent>().Publish();
            }
            catch 
            { 
            }
            
        }

        private void Window_Activated(object sender, RoutedEventArgs e)
        {
            

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            
        }
        

        private void SubViewSwitch(string viewname)
        {
            int ind = Array.IndexOf(subviews, viewname);

            for (int i = 0; i < subViewModels.Count; i++)
            {
                if (ind == i) { continue; }
                subViewModels[i].SetHidden();

            }
            if (ind == -1)
            {
                _ea.GetEvent<BlurEvent>().Publish(0);
                return;
            }
            
            if (subViewModels[ind]._userControl.Visibility == Visibility.Visible)
            {

                subViewModels[ind].SetHidden();
                _ea.GetEvent<BlurEvent>().Publish(0);
            }
            else
            {
                subViewModels[ind].SetVisiable();
                _ea.GetEvent<BlurEvent>().Publish(20);
            }
        }

        
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //通知视图选择
            _ea.GetEvent<PartSelectByListViewEvent>().Publish(this.Models_ListView.SelectedIndex);
        }
        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }


        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var test = e.NewSize;
            this.Models_ListView.Height = test.Height - 335;
        }

        private void Notification_Button_Click(object sender, RoutedEventArgs e)
        {
            if (MessageListviewPanel.Visibility == Visibility.Collapsed)
            { MessageListviewPanel.Visibility = Visibility.Visible; }
            else
            { MessageListviewPanel.Visibility = Visibility.Collapsed; }
            
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var thisButton  = sender as System.Windows.Controls.Button;
            WindowAnimation.ScaleEasingAnimationShow(thisButton, 0.5, 0.5, 1, 1.7, 5, new TimeSpan(0, 0, 0, 0, 250));
        }



        private void _Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var thisButton = sender as System.Windows.Controls.Button;
            WindowAnimation.ScaleEasingAnimationShow(thisButton, 0.5, 0.5, 1.7, 1, 5, new TimeSpan(0, 0, 0, 0, 250));
        }
        private void SidePanel_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Border_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //根据鼠标位置，确定icon 缩放效果
            var pos = e.GetPosition(this);
            double dy = pos.Y - this.Height / 2 +40;
            double ind = Math.Round(dy / 70);
            double dist = Math.Max(1- Math.Abs(dy - 70 * ind)/35-0.3,0);
            switch (ind) { 
                case -3:
                    PrintSetup_Button.Width = (1 + 1.3*dist) * 40;
                    PrintSetup_Button.Height = (1 + 1.3 * dist) * 40;

                    PrintTaskMonitor_Button.Width = 40;
                    PrintTaskMonitor_Button.Height = 40;

                    printControl_Button.Width = 40;
                    printControl_Button.Height = 40;

                    History_Button.Width = 40;
                    History_Button.Height = 40;

                    Setting_Button.Width = 40;
                    Setting_Button.Height = 40;

                    Info_Button.Width = 40;
                    Info_Button.Height = 40;

                    User_Button.Width = 40;
                    User_Button.Height = 40;
                    break;
                case -2:
                    PrintTaskMonitor_Button.Width = (1 + dist) * 40;
                    PrintTaskMonitor_Button.Height = (1 + dist) * 40;
                    PrintSetup_Button.Width = 40;
                    PrintSetup_Button.Height = 40;


                    printControl_Button.Width = 40;
                    printControl_Button.Height = 40;

                    History_Button.Width = 40;
                    History_Button.Height = 40;

                    Setting_Button.Width = 40;
                    Setting_Button.Height = 40;

                    Info_Button.Width = 40;
                    Info_Button.Height = 40;

                    User_Button.Width = 40;
                    User_Button.Height = 40;
                    break;
                case -1:
                    printControl_Button.Width = (1 + dist) * 40;
                    printControl_Button.Height = (1 + dist) * 40;
                    PrintSetup_Button.Width = 40;
                    PrintSetup_Button.Height = 40;

                    PrintTaskMonitor_Button.Width = 40;
                    PrintTaskMonitor_Button.Height = 40;


                    History_Button.Width = 40;
                    History_Button.Height = 40;

                    Setting_Button.Width = 40;
                    Setting_Button.Height = 40;

                    Info_Button.Width = 40;
                    Info_Button.Height = 40;

                    User_Button.Width = 40;
                    User_Button.Height = 40;
                    break;
                case 0:
                    History_Button.Width = (1 + dist) * 40;
                    History_Button.Height = (1 + dist) * 40;
                    PrintSetup_Button.Width = 40;
                    PrintSetup_Button.Height = 40;

                    PrintTaskMonitor_Button.Width = 40;
                    PrintTaskMonitor_Button.Height = 40;

                    printControl_Button.Width = 40;
                    printControl_Button.Height = 40;


                    Setting_Button.Width = 40;
                    Setting_Button.Height = 40;

                    Info_Button.Width = 40;
                    Info_Button.Height = 40;

                    User_Button.Width = 40;
                    User_Button.Height = 40;
                    break;
                case 1:
                    Setting_Button.Width = (1 + dist) * 40;
                    Setting_Button.Height = (1 + dist) * 40;
                    PrintSetup_Button.Width = 40;
                    PrintSetup_Button.Height = 40;

                    PrintTaskMonitor_Button.Width = 40;
                    PrintTaskMonitor_Button.Height = 40;

                    printControl_Button.Width = 40;
                    printControl_Button.Height = 40;

                    History_Button.Width = 40;
                    History_Button.Height = 40;

                  

                    Info_Button.Width = 40;
                    Info_Button.Height = 40;

                    User_Button.Width = 40;
                    User_Button.Height = 40;
                    break;
                case 2:
                    Info_Button.Width = (1 + dist) * 40;
                    Info_Button.Height = (1 + dist) * 40;
                    PrintSetup_Button.Width = 40;
                    PrintSetup_Button.Height = 40;

                    PrintTaskMonitor_Button.Width = 40;
                    PrintTaskMonitor_Button.Height = 40;

                    printControl_Button.Width = 40;
                    printControl_Button.Height = 40;

                    History_Button.Width = 40;
                    History_Button.Height = 40;

                    Setting_Button.Width = 40;
                    Setting_Button.Height = 40;



                    User_Button.Width = 40;
                    User_Button.Height = 40;
                    break;
                case 3:
                    User_Button.Width = (1 + dist) * 40;
                    User_Button.Height = (1 + dist) * 40;
                    PrintSetup_Button.Width = 40;
                    PrintSetup_Button.Height = 40;

                    PrintTaskMonitor_Button.Width = 40;
                    PrintTaskMonitor_Button.Height = 40;

                    printControl_Button.Width = 40;
                    printControl_Button.Height = 40;

                    History_Button.Width = 40;
                    History_Button.Height = 40;

                    Setting_Button.Width = 40;
                    Setting_Button.Height = 40;

                    Info_Button.Width = 40;
                    Info_Button.Height = 40;


                    break;

            }
        }

        private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PrintSetup_Button.Width = 40;
            PrintSetup_Button.Height = 40;

            PrintTaskMonitor_Button.Width = 40;
            PrintTaskMonitor_Button.Height = 40;

            printControl_Button.Width = 40;
            printControl_Button.Height = 40;

            History_Button.Width = 40;
            History_Button.Height = 40;

            Setting_Button.Width = 40;
            Setting_Button.Height = 40;

            Info_Button.Width = 40;
            Info_Button.Height = 40;

            User_Button.Width = 40;
            User_Button.Height = 40;
        }

       

        private void fullscrean_Click(object sender, RoutedEventArgs e)
        {
            if (this.Width == System.Windows.SystemParameters.PrimaryScreenWidth && this.Width == System.Windows.SystemParameters.PrimaryScreenWidth)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.Topmost = false;
                _ea.GetEvent<ChangeFullscreanIconEvent>().Publish(true);


            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;//还原窗口（非最小化和最大化）
                this.WindowStyle = System.Windows.WindowStyle.None; //仅工作区可见，不显示标题栏和边框
                this.ResizeMode = System.Windows.ResizeMode.NoResize;//不显示最大化和最小化按钮
                this.Topmost = true;    //窗口在最前

                this.Left = 0.0;
                this.Top = 0.0;
                this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                _ea.GetEvent<ChangeFullscreanIconEvent>().Publish(false);
            }
        }
    }
    
}
