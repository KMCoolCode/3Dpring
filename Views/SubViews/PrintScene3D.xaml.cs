using AngelDLP.ViewModels;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
using Ninject;
using Prism.Events;
using System;
using System.Windows.Controls;
using static AngelDLP.ViewModels.PrintScene3DViewModel;

namespace AngelDLP.Views
{
    public partial class PrintScene3D : UserControl
    {
        private IEventAggregator _ea;


        public class SaveViewPortEvent : PubSubEvent<PrintTask> { }
        public class SliceAll3DEvent : PubSubEvent<double> { }
        [Inject]
        public PrintScene3DViewModel ViewModel
        {
            get => DataContext as PrintScene3DViewModel;
            set => DataContext = value;
        }
        public PrintScene3D()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, System.EventArgs e)
        {
            if (_ea == null)
            {
                _ea = ViewModel._ea;
                // Grid Setup
               
                _ = _ea.GetEvent<PartSelectByListViewEvent>().Subscribe(ind =>
                {
                    if (ind != -1)
                    {
                        ViewModel.Target = null;
                        var currItemsModel3D = view1.Items[3] as ItemsModel3D;
                        var selectedCrossSectionMeshGeometryModel3D = currItemsModel3D.Children[ind] as CrossSectionMeshGeometryModel3D;
                        //ViewModel.CenterOffset = selectedCrossSectionMeshGeometryModel3D.Geometry.Bound.Center; // Must update this before updating target
                        var pm = selectedCrossSectionMeshGeometryModel3D.DataContext as PartModel;
                        //ViewModel.CenterOffset = new Vector3 { X = (float)pm.TX, Y = (float)pm.TY, Z = 1 };
                        ViewModel.Target = selectedCrossSectionMeshGeometryModel3D as Element3D;
                        ViewModel.ManipulatorVisiable = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        ViewModel.ManipulatorVisiable = System.Windows.Visibility.Hidden;
                    }
                });
                _ = _ea.GetEvent<SliceAll3DEvent>().Subscribe(sliceHeight =>
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ViewModel.Target = null;
                        ItemsModel3D currItemsModel3D = view1.Items[3] as ItemsModel3D;
                        foreach (var im in currItemsModel3D.Children)
                        {
                            CrossSectionMeshGeometryModel3D selectedCrossSectionMeshGeometryModel3D = im as CrossSectionMeshGeometryModel3D;
                            PartModel pm = selectedCrossSectionMeshGeometryModel3D.DataContext as PartModel;
                            //pm.Plane1 = new SharpDX.Plane(new Vector3(0, 0, -1), -(float)(sliceHeight));
                            if (sliceHeight > pm.MaxSliceHeight)
                            {
                                pm.SliceHeight = pm.MaxSliceHeight;
                            }
                            else
                            {
                                pm.SliceHeight = sliceHeight;
                            }
                        }
                    }));

                });
                _ = _ea.GetEvent<SaveViewPortEvent>().Subscribe(printtask =>
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        printtask.layoutBitmap = view1.RenderBitmap();
                    }));
                    
                });
            }
        }

    }
}
