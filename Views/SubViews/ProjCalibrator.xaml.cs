using AngelDLP.ViewModels;
using Ninject;
using Prism.Events;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using static AngelDLP.ViewModels.ProjCalibratorViewModel;



namespace AngelDLP.Views
{
    /// <summary>
    /// PrintTaskSetup.xaml 的交互逻辑
    /// </summary>
    public partial class ProjCalibrator : UserControl
    {
        IEventAggregator _ea;

        [Inject]
        public ProjCalibratorViewModel ViewModel
        {
            get => DataContext as ProjCalibratorViewModel;
            set => DataContext = value;
        }
        public ProjCalibrator()
        {
            InitializeComponent();
            Btn_1.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);
            Btn_2.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);
            Btn_3.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);
            Btn_4.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);
            Btn_5.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);
            Btn_6.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);
            Btn_7.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);
            Btn_8.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);
            Btn_9.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);
            Btn_10.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);
            Btn_13.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonDown), true);

            Btn_1.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
            Btn_2.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
            Btn_3.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
            Btn_4.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
            Btn_5.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
            Btn_6.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
            Btn_7.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
            Btn_8.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
            Btn_9.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
            Btn_10.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
            Btn_13.AddHandler(TextBox.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ModbusBtn_MouseLeftButtonUp), true);
        }

        private void UserControl_Initialized(object sender, System.EventArgs e)
        {
            if (_ea == null)
            {
                _ea = ViewModel._ea;
                _ea.GetEvent<OnCalibViewInited_Even>().Publish();
                // Grid Setup

                //_ = _ea.GetEvent<PartSelectByListViewEvent>().Subscribe(ind =>
                //{
                //    if (ind != -1)
                //    {
                //        ViewModel.Target = null;
                //        var currItemsModel3D = view1.Items[3] as ItemsModel3D;
                //        var selectedCrossSectionMeshGeometryModel3D = currItemsModel3D.Children[ind] as CrossSectionMeshGeometryModel3D;
                //        //ViewModel.CenterOffset = selectedCrossSectionMeshGeometryModel3D.Geometry.Bound.Center; // Must update this before updating target
                //        var pm = selectedCrossSectionMeshGeometryModel3D.DataContext as PartModel;
                //        //ViewModel.CenterOffset = new Vector3 { X = (float)pm.TX, Y = (float)pm.TY, Z = 1 };
                //        ViewModel.Target = selectedCrossSectionMeshGeometryModel3D as Element3D;
                //        ViewModel.ManipulatorVisiable = System.Windows.Visibility.Visible;
                //    }
                //    else
                //    {
                //        ViewModel.ManipulatorVisiable = System.Windows.Visibility.Hidden;
                //    }111
                //});

            }
        }
        private void ModbusBtn_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var btn = sender as Button;
            string name = btn.Name.Substring(4);
            _ea.GetEvent<ModbusBtnDown_Even>().Publish(name);
        }
        private void ModbusBtn_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var btn = sender as Button;
            string name = btn.Name.Substring(4);
            _ea.GetEvent<ModbusBtnUp_Even>().Publish(name);
        }
    }
}
