using AngelDLP.ViewModels;
using Prism.Events;
using System.Windows.Controls;
using System.Windows.Input;
using static AngelDLP.ViewModels.PrintControlViewModel;

namespace AngelDLP.Views
{
    /// <summary>
    /// PrintTaskSetup.xaml 的交互逻辑
    /// </summary>
    public partial class PrintControl : UserControl
    {
        IEventAggregator _ea;
        public PrintControl()
        {
            
            InitializeComponent();
            var dc = this.DataContext as PrintControlViewModel;
            _ea = dc._ea;
        }
        //[Inject]
        //public PrintControlViewModel ViewModel
        //{
        //    get => DataContext as PrintControlViewModel;
        //    set => DataContext = value;
        //}
        //private void UserControl_Initialized(object sender, System.EventArgs e)
        //{
        //    if (_ea == null)
        //    {
        //        _ea = ViewModel._ea;
        //        //_ea.GetEvent<OnCalibViewInited_Even>().Publish();
        //        // Grid Setup

        //        //_ = _ea.GetEvent<PartSelectByListViewEvent>().Subscribe(ind =>
        //        //{
        //        //    if (ind != -1)
        //        //    {
        //        //        ViewModel.Target = null;
        //        //        var currItemsModel3D = view1.Items[3] as ItemsModel3D;
        //        //        var selectedCrossSectionMeshGeometryModel3D = currItemsModel3D.Children[ind] as CrossSectionMeshGeometryModel3D;
        //        //        //ViewModel.CenterOffset = selectedCrossSectionMeshGeometryModel3D.Geometry.Bound.Center; // Must update this before updating target
        //        //        var pm = selectedCrossSectionMeshGeometryModel3D.DataContext as PartModel;
        //        //        //ViewModel.CenterOffset = new Vector3 { X = (float)pm.TX, Y = (float)pm.TY, Z = 1 };
        //        //        ViewModel.Target = selectedCrossSectionMeshGeometryModel3D as Element3D;
        //        //        ViewModel.ManipulatorVisiable = System.Windows.Visibility.Visible;
        //        //    }
        //        //    else
        //        //    {
        //        //        ViewModel.ManipulatorVisiable = System.Windows.Visibility.Hidden;
        //        //    }111
        //        //});

        //    }
        //}

        private void Button_MouseAction(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var button = sender as Button;
            if (button == null) { return; }
            var dev = e.Device as System.Windows.Input.MouseDevice;
            if (dev == null)
            {
                return;
            }
            if (button.Name == "Button_WindowUp")
            {
                if (dev.LeftButton == MouseButtonState.Pressed)
                {
                    //_ea.GetEvent<MessageEvent>().Publish("上升按键按下");
                    _ea.GetEvent<WindowControlEvent>().Publish("WindowUp");
                    return;
                }
                if (dev.LeftButton == MouseButtonState.Released)
                {
                    //_ea.GetEvent<MessageEvent>().Publish("上升按键释放");
                    _ea.GetEvent<WindowControlEvent>().Publish("WindowStop");
                    return;
                }
            }
            if (button.Name == "Button_WindowDown")
            {
                if (dev.LeftButton == MouseButtonState.Pressed)
                {
                    //_ea.GetEvent<MessageEvent>().Publish("下降按键按下");
                    _ea.GetEvent<WindowControlEvent>().Publish("WindowDown");
                    return;
                }
                if (dev.LeftButton == MouseButtonState.Released)
                {
                    //_ea.GetEvent<MessageEvent>().Publish("下降按键释放");
                    _ea.GetEvent<WindowControlEvent>().Publish("WindowStop");
                    return;
                }
            }
        }




        //private void ProjectorSwitchButton_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    int projectorIndex = 0;
        //    if (sender.Equals(LeftProjectorSwitchButton))
        //    {
        //        //左侧光机
        //        projectorIndex = 0;
        //    }
        //    else if (sender.Equals(RightProjectorSwitchButton))
        //    {
        //        //右侧光机
        //        projectorIndex = 1;
        //    }
        //    Button thisClickedButton = sender as Button;
        //    if ((string)thisClickedButton.Content == "Off")
        //    {
        //    }
        //}

    }
}
