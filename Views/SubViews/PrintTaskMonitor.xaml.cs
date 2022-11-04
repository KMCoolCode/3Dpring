using AngelDLP.ViewModels;
using Ninject;
using Prism.Events;
using System.Windows.Controls;
using static AngelDLP.ViewModels.PrintTaskMonitorViewModel;

namespace AngelDLP.Views
{
    /// <summary>
    /// PrintTaskSetup.xaml 的交互逻辑
    /// </summary>
    public partial class PrintTaskMonitor : UserControl
    {
        public IEventAggregator _ea;
        [Inject]
        public PrintTaskMonitorViewModel ViewModel
        {
            get => DataContext as PrintTaskMonitorViewModel;
            set => DataContext = value;
        }

        public PrintTaskMonitor()
        {
            InitializeComponent();
            if (_ea == null)
            {
                _ea = ViewModel._ea;
                // Grid Setup


            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ea.GetEvent<PrintMonitorTabEvent>().Publish(tabControl.SelectedIndex);
        }
    }
}
