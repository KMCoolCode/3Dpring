using Prism.Events;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using static AngelDLP.ViewModels.MainWindowViewModel;
using static AngleDLP.Models.LogHelper;
using System.Reflection;
using System.Windows.Interop;
using AngelDLP.ViewModels;

namespace AngelDLP.PPC
{
    /// <summary>
    /// AnotherWindows.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectorWindow : Window
    {
        private int projectIndex;
        private log4net.ILog pwLog;
        private readonly IEventAggregator _ea;
        public  ProjectorWindow( IEventAggregator eventAggregator)
        {
            this._ea = eventAggregator;
            pwLog = log4net.LogManager.GetLogger("logProjection");
        }
        private void MessageInfoUpdate(string info,LogLevel logLevel)
        {
            _ea.GetEvent<MessageEvent>().Publish(info);
            switch (logLevel)
            {
                case LogLevel.Info:
                    pwLog.Info(info);
                    break;
                case LogLevel.Debug:
                    pwLog.Debug(info);
                    break;
                case LogLevel.Warn:
                    pwLog.Warn(info);
                    break;
                case LogLevel.Error:
                    pwLog.Error(info);
                    break;
                case LogLevel.Fatal:
                    pwLog.Fatal(info);
                    break;
            }
        }
        private void MessageInfoUpdate_Thread(string info, LogLevel logLevel = LogLevel.Info)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageInfoUpdate(info, logLevel);
            }));
        }
        public Tuple<bool,string> InitProjectorWindow(int ind, int width, int height, int displayPort)
        {
            
            //获取当前屏幕集
            Screen[] screenlist = Screen.AllScreens;
            if (screenlist.Length < displayPort + 1) { return new Tuple<bool, string>(false, "没有此序号屏幕"); }
            //工作区
            Screen screen = Screen.AllScreens[displayPort];
            if (screen.Primary)
            {
                //MessageInfoUpdate_Thread("当前为主屏幕，无法初始化",LogLevel.Warn);
                return new Tuple<bool, string>(false, "当前为主屏幕，无法初始化");
            }
            ProjectorViewModel dc = (ProjectorViewModel)this.DataContext;
            if (screen.WorkingArea.Width != width || screen.WorkingArea.Height != height) {
                //MessageInfoUpdate_Thread("分辨率校验失败，无法初始化",LogLevel.Warn);

                return new Tuple<bool, string>(false, $"分辨率校验失败[{screen.WorkingArea.Width}x{screen.WorkingArea.Height}]");
            }
            System.Drawing.Rectangle rte = screen.WorkingArea;
            this.Height = screenlist[displayPort].WorkingArea.Height;
            this.Width = screenlist[displayPort].WorkingArea.Width;
            this.ShowInTaskbar = false;
            this.Left = rte.Left;
            this.Top = rte.Top;
            
            projectIndex = ind;
            
            InitializeComponent();
            return new Tuple<bool, string>(true, "初始化成功");
        }
        [DllImport("dwmapi.dll", PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd,
            DwmWindowAttribute dwmAttribute,
            IntPtr pvAttribute,
            uint cbAttribute);

        [Flags]
        private enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_LAST
        }

        private enum DwmncRenderingPolicy
        {
            DWMNCRP_USEWINDOWSTYLE,
            DWMNCRP_DISABLED,
            DWMNCRP_ENABLED,
            DWMNCRP_LAST
        }
        public void StayVisible()
        {
            var helper = new WindowInteropHelper(this);
            helper.EnsureHandle();

            if (!DwmIsCompositionEnabled()) return;

            var status = Marshal.AllocCoTaskMem(sizeof(uint));
            Marshal.Copy(new[] { (int)DwmncRenderingPolicy.DWMNCRP_ENABLED }, 0, status, 1);
            DwmSetWindowAttribute(helper.Handle,
                DwmWindowAttribute.DWMWA_EXCLUDED_FROM_PEEK,
                status,
                sizeof(uint));
        }
        public class ImageConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return new BitmapImage(new Uri(value.ToString()));
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Maximized;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.Topmost = true;
            StayVisible();
        }


    }

    public static class WindowInteropHelperExtensions
    {
        public static IntPtr EnsureHandle(this WindowInteropHelper helper)
        {
            if (helper == null) throw new ArgumentNullException("helper");
            if (helper.Handle != IntPtr.Zero) return helper.Handle;

            var window = (System.Windows.Window)typeof(WindowInteropHelper).InvokeMember("_window",
                BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
                null, helper, null);

            typeof(System.Windows.Window).InvokeMember("SafeCreateWindow",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                null, window, null);

            return helper.Handle;
        }
    }
}
