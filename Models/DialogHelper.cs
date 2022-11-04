using AngelDLP;
using AngelDLP.ViewModels;
using AngelDLP.Views;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static AngleDLP.LiteDBHelper;

namespace AngleDLP.Models
{
    internal class DialogHelper
    {
        public enum DialogType { 
            Yes,
            YesNo,
            No,
            None
        }
        
        private static InputDialog iDw ;
        private static InputDialogViewModel iDwVM ;
        public static bool EnableInput =true ;
        public static bool AskCloseInput = false;
        private static MessageDialog mDw;
        private static MessageDialogViewModel mDwVM;
        public static bool EnableCheckInteraction = true;
        public static bool AskCheckInput = false;
        private static MessageDialog ynDw;
        private static MessageDialogViewModel ynDwVM;
        private static UserLogin liDw;
        private static UserLoginViewModel liDwVM;

        public static Tuple<bool,string> ShowInputDialog(string tip)
        {
            if (!EnableInput)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    WindowAnimation.WindowShake(iDw);
                }));

                return new Tuple<bool, string>(false,"");
            }
           
            EnableInput = false;
            string res = null;
            
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                iDw = new InputDialog();
                iDwVM = new InputDialogViewModel();
                iDw.DataContext = iDwVM;
                AskCloseInput = false;
                iDw.IsClosed = false;
                iDw.GetResult = false;
                Window onwer = App.Current.MainWindow;
                iDw.Left = onwer.Left + onwer.Width / 2 - iDw.Width / 2;
                iDw.Top = onwer.Top + onwer.Height / 2 - iDw.Height / 2;
                iDwVM.TipMessage = tip;
                iDw.Show();
            })).Wait();
            while (!iDw.IsClosed && !AskCloseInput)
            {
                Thread.Sleep(100);
            }
            if (AskCloseInput)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    iDw.Close();
                }));
                EnableInput = true;
                return new Tuple<bool, string>(false, ""); ;
            }
            EnableInput = true;
            if (iDw.GetResult)
            { return new Tuple<bool, string>(true, iDwVM.InputText); }
            else { return new Tuple<bool, string>(false, iDwVM.InputText); }
            
            }

        public static void ShowMessageDialog(string tipTittle,string tipMessage, DialogType dialogType = DialogType.Yes ,int waitTimeSec = -1)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                DialogHelper.mDw = new MessageDialog();
                DialogHelper.mDwVM = new MessageDialogViewModel();
                mDwVM.TipMessage = tipMessage;
                mDwVM.TipTittle = tipTittle;
                mDw.DataContext = mDwVM;
                if (dialogType == DialogType.Yes)
                {
                    mDwVM.NoButtonEnable = Visibility.Collapsed;
                }
                Window onwer = App.Current.MainWindow;
                mDw.Left = onwer.Left + onwer.Width / 2 - mDw.Width / 2;
                mDw.Top = onwer.Top + onwer.Height / 2 - mDw.Height / 2;
                mDw.Show();
            })).Wait();
            if (waitTimeSec > 0)
            {
                for (int i = 0; i < waitTimeSec; i++)
                {
                    Thread.Sleep(1000);
                }
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    mDw.Close();
                }));
                
            }
        }

        public static bool ShowYONDialog(string tipTittle, string tipMessage, DialogType dialogType = DialogType.YesNo)
        {
            bool res = false;
            if (!EnableCheckInteraction)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    WindowAnimation.WindowShake(ynDw);
                }));
                
                return false;
            }
            EnableCheckInteraction = false;
            AskCheckInput = false;


            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                DialogHelper.ynDw = new MessageDialog();
                DialogHelper.ynDwVM = new MessageDialogViewModel();
                ynDw.IsClosed = false;
                ynDw.DataContext = ynDwVM;
                ynDw.InitializeComponent();
                ynDwVM.TipMessage = tipMessage;
                ynDwVM.TipTittle = tipTittle;
                Window onwer = App.Current.MainWindow;
                ynDw.Left = onwer.Left + onwer.Width / 2 - ynDw.Width / 2;
                ynDw.Top = onwer.Top + onwer.Height / 2 - ynDw.Height / 2;
                ynDw.Show();
                if (dialogType == DialogType.Yes)
                {
                    ynDwVM.NoButtonEnable = Visibility.Collapsed;
                }
                if (dialogType == DialogType.No)
                {
                    ynDwVM.YesButtonEnable = Visibility.Collapsed;
                }
                if (dialogType == DialogType.None)
                {
                    ynDwVM.YesButtonEnable = Visibility.Collapsed;
                    ynDwVM.NoButtonEnable = Visibility.Collapsed;
                }
            })).Wait();
            while (ynDw.IsClosed == false && !AskCheckInput)
            {
                Thread.Sleep(100);
            }
            if (AskCheckInput)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ynDw.Close();
                }));
                EnableCheckInteraction = true;
                return false;
            }
            res = ynDw.GetResult;
            EnableCheckInteraction = true;
            return res;
        }

      

    }
}
