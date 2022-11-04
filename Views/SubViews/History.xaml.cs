using AngelDLP.ViewModels;
using Ninject;
using Prism.Events;
using System.Windows.Controls;
using static AngelDLP.ViewModels.HistoryViewModel;

namespace AngelDLP.Views
{
    /// <summary>
    /// PrintTaskSetup.xaml 的交互逻辑
    /// </summary>
    
    public partial class History : UserControl
    {
        public IEventAggregator _ea;
        [Inject]
        public HistoryViewModel ViewModel
        {
            get => DataContext as HistoryViewModel;
            set => DataContext = value;
        }

        public History()
        {
            InitializeComponent();
            if (_ea == null)
            {
                _ea = ViewModel._ea;
                // Grid Setup


            }
        }

        private void UserControl_Initialized(object sender, System.EventArgs e)
        {
            if (_ea == null)
            {
                _ea = ViewModel._ea;
                // Grid Setup


            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void HistoryDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //通知视图选择
            DatePicker datePicker = (DatePicker)sender;
            _ea.GetEvent<HistoryDateViewEvent>().Publish(datePicker.SelectedDate.Value);
        }
        private void PrintLog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //通知视图选择
            ListView listView = (ListView)sender;
            _ea.GetEvent<PrintLogViewEvent>().Publish(listView.SelectedIndex);
        }
    }
}
