using HandyControl.Data;
using HandyControl.Themes;
using HandyControl.Tools;
using AngelDLP.Views;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Windows;
using AngelDLP.PMC;
using AngelDLP.PPC;
using System.Threading;
using AngleDLP.Models;
using System.Threading.Tasks;
using AngelDLP.ViewModels;

namespace AngelDLP
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }
        private static System.Threading.Mutex mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += UnhandledExceptionOccured;
            mutex = new System.Threading.Mutex(true, "AngleDLP");
            if (mutex.WaitOne(0, false))
            {
                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show("程序已经在运行！", "提示");
                this.Shutdown();
            }
        }
        private static void UnhandledExceptionOccured(object sender, UnhandledExceptionEventArgs args)
        {
            //全局异常记录
            var e = (Exception)args.ExceptionObject;
            //Common.Log4netHelper.WriteError("Application has crashed   " + e.ToString());
            MessageBox.Show($"something went wrong and the application must close:{e.ToString()} ", "Unhandled Error", MessageBoxButton.OK);

        }
        protected override void OnInitialized()
        {
            
            base.OnInitialized();
            Container.Resolve<IRegionManager>().RegisterViewWithRegion("ContentRegion", typeof(NoneView));
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PrintTaskSetup>();
            containerRegistry.RegisterForNavigation<NoneView>();
            containerRegistry.RegisterSingleton<IPartModelFactory, PartModelFactory>();
            containerRegistry.RegisterSingleton<IProjectFactory, ProjectFactory>();
        }
        public static UserLogin login = new UserLogin();
        public static UserLoginViewModel loginVM= new UserLoginViewModel();
        protected override void InitializeShell(Window shell)
        {
            //登录
            login.DataContext = loginVM;
            if (login.ShowDialog() == false)
            {
                if (Application.Current != null)
                {
                    Application.Current.Shutdown();
                }
                return;
            }
            base.InitializeShell(shell);
        }

    }
    
}
