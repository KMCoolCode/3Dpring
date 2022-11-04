using Prism.Commands;
using Prism.Events;
using System;
using System.Windows;
using System.Windows.Controls;
using UsingCompositeCommands.Core;
using static AngelDLP.ViewModels.MainWindowViewModel;

namespace AngelDLP.Views
{
    /// <summary>
    /// PrintTaskSetup.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : Window
    {
        public bool GetResult { get; set; } = false;
        public bool IsClosed { get; set; } = false;
        public InputDialog()
        {
            Closed += MyWindow_Closed;
            InitializeComponent();
        }
        private void MyWindow_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            GetResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            GetResult = false;
            this.Close();
        }

        private void inputbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GetResult = true;
                this.Close();
            }
        }
    }
}
