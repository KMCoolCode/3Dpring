using System.Windows.Controls;


namespace AngelDLP.Views
{
    /// <summary>
    /// PrintTaskSetup.xaml 的交互逻辑
    /// </summary>
    public partial class PrintTaskSetup : UserControl
    {
        public PrintTaskSetup()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //仅保留切换后的有效
            TabControl tabControl = (TabControl)sender;
            foreach (TabItem tab in tabControl.Items)
            {
                if (tab.TabIndex == tabControl.SelectedIndex)
                {
                    tab.IsEnabled = true;
                }
                else
                {
                    tab.IsEnabled = false;
                }
            }
        }
    }
}
