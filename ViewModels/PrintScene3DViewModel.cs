namespace AngelDLP.ViewModels
{
    using System;
    using DemoCore;
    using HelixToolkit.Wpf.SharpDX;
    using SharpDX;
    using Media3D = System.Windows.Media.Media3D;
    using Point3D = System.Windows.Media.Media3D.Point3D;
    using Vector3D = System.Windows.Media.Media3D.Vector3D;
    using Transform3D = System.Windows.Media.Media3D.Transform3D;
    using TranslateTransform3D = System.Windows.Media.Media3D.TranslateTransform3D;
    using Color = System.Windows.Media.Color;
    using Plane = SharpDX.Plane;
    using Vector3 = SharpDX.Vector3;
    using Colors = System.Windows.Media.Colors;
    using Color4 = SharpDX.Color4;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows;
    using System.Windows.Threading;
    using ICSharpCode.SharpZipLib;
    using ICSharpCode.SharpZipLib.Zip;
    using System.IO;
    using System.Xml.Serialization;
    using System.Collections.ObjectModel;
    using Prism.Events;
    using static AngelDLP.ViewModels.MainWindowViewModel;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using Prism.Commands;

    public partial class PrintScene3DViewModel : DemoCore.BaseViewModel
    {
        public IEventAggregator _ea;

        public LineGeometry3D Grid_left { get; private set; }
        /// <summary>
        /// Color of the Gridlines
        /// </summary>
        public SharpDX.Color GridColor_left { get; private set; }
        /// <summary>
        /// Color of the Triangle Lines
        /// </summary>
        public double LineThickness_left { get; set; }
        /// <summary>
        /// Transform of the Grid
        /// </summary>
        public Transform3D GridTransform_right { get; private set; }

        public LineGeometry3D Grid_right { get; private set; }
        /// <summary>
        /// Color of the Gridlines
        /// </summary>
        public SharpDX.Color GridColor_right { get; private set; }
        /// <summary>
        /// Color of the Triangle Lines
        /// </summary>
        public double LineThickness_right { get; set; }
        /// <summary>
        /// Transform of the Grid
        /// </summary>
        public Transform3D GridTransform_left { get; private set; }

        public class PartSelectByListViewEvent : PubSubEvent<int>{}
        
        public class BlurEvent : PubSubEvent<int>{}
        public class UpdateTransform3DsEvent : PubSubEvent<UI2PartModel>{}
        public ObservableCollection<PartModel> Items { set; get; }

        public int BlurRadius { get; set; } = 0;
        public Stream BackgroundTexture
        {
            private set; get;
        }


        public Element3D target;
        public Element3D Target
        {
            set
            {
                SetValue<Element3D>(ref target, value, nameof(Target));
            }
            get
            {
                return target;
            }
        }
        
        public Visibility manipulatorVisiable = Visibility.Collapsed;
        public Visibility ManipulatorVisiable
        {
            set
            {
                SetValue<Visibility>(ref manipulatorVisiable, value, nameof(Target));
            }
            get
            {
                return manipulatorVisiable;
            }
        }
        public Vector3 centerOffset;
        public Vector3 CenterOffset
        {
            set
            {
                SetValue<Vector3>(ref centerOffset, value, nameof(CenterOffset));
            }
            get
            {
                return centerOffset;
            }
        }

        public DelegateCommand ResetViewCommand { get; private set; }

        public PrintScene3DViewModel(IEventAggregator ea, IPartModelFactory partModelFactory)
        {
            _ea = ea;
            Items = partModelFactory.GetPartModels();//用于模型展示

            ResetViewCommand = new DelegateCommand(ResetView);



            Grid_left = LineBuilder.GenerateGrid(Vector3.UnitZ, 0, 9, 0, 16);
            GridColor_left = SharpDX.Color.LightGreen;

            GridTransform_left = new MatrixTransform3D(
                   new Matrix3D(24, 0, 0, 0,
                                0, 24, 0, 0,
                                0, 0, 0.001, 0,
                                -0.50, 0, 0, 1)) ;
            LineThickness_left = 0.7;

            Grid_right = LineBuilder.GenerateGrid(Vector3.UnitZ, 0, 9, 0, 16);
            GridColor_right = SharpDX.Color.LightBlue;

            GridTransform_right = new MatrixTransform3D(
                  new Matrix3D(24, 0, 0, 0,
                                0, 24, 0, 0,
                                0, 0, 0.001, 0,
                                216.5, 0, 0, 1));
            LineThickness_right = 0.7;

            _ea.GetEvent<PartSelectByListViewEvent>().Subscribe(ind =>
            {
                if (ind != -1)
                {
                    //Target = this.Items[ind] as Element3D;
                    foreach (var item in Items)
                    {
                        item.Highlight = false;
                    }

                        Items[ind].Highlight = true;
                }

            });
            _ea.GetEvent<BlurEvent>().Subscribe(blurR =>
            {
                BlurRadius = blurR;

            });
            _ea.GetEvent<UpdateTransform3DsEvent>().Subscribe(trans =>
            {
                Target.Transform = trans.trans;
                _ea.GetEvent<PartSelectByListViewEvent>().Publish(trans.index);

            });
            CenterOffset = new Vector3(0, 0, 0);
            
            //Target = moveTarget;
            EffectsManager = new DefaultEffectsManager();
            
            // ----------------------------------------------
            // titles
            this.SubTitle = "WPF & SharpDX";

            // ----------------------------------------------
            // camera setup
            ResetView();
            // ----------------------------------------------
            // setup scene
            BackgroundTexture =
                BitmapExtensions.CreateLinearGradientBitmapStream(EffectsManager, 128, 128, Direct2DImageFormat.Bmp,
                new Vector2(0, 0), new Vector2(0, 128), new SharpDX.Direct2D1.GradientStop[]
                {
                    new SharpDX.Direct2D1.GradientStop(){ Color = SharpDX.Color.LightGray, Position = 0f },
                    new SharpDX.Direct2D1.GradientStop(){ Color =SharpDX.Color.LightBlue, Position = 1f }
                });
        }

        private void ResetView()
        {
            this.Camera = new HelixToolkit.Wpf.SharpDX.PerspectiveCamera
            {
                Position = new Point3D(285, 190, 500),
                LookDirection = new Vector3D(0, 0, -500),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 2000
            };
        }

        public void OnMouseDown3DHandler(object sender, MouseDown3DEventArgs e)
        {
            var teststr = e.ToString();
            
            if (e.HitTestResult == null)
            {
                foreach (var item in Items)
                {
                    item.Highlight = false;
                }
                Target = null;
                ManipulatorVisiable= Visibility.Collapsed;
                return;
            }
            var dev = e.OriginalInputEventArgs.Device as System.Windows.Input.MouseDevice;
            if (dev != null)
            {
                if (e.HitTestResult != null && dev.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.HitTestResult.ModelHit is CrossSectionMeshGeometryModel3D m)
                {
                    foreach (var item in Items)
                    {
                        item.Highlight = false;
                    }
                    Target = null;
                    CenterOffset = m.Geometry.Bound.Center; // Must update this before updating target

                    Target = e.HitTestResult.ModelHit as Element3D;
                    ManipulatorVisiable = Visibility.Visible;
                    var model = (e.HitTestResult.ModelHit as Element3D).DataContext as PartModel;
                    model.Highlight = true;

                    var pm = Target.DataContext as PartModel;
                    //update transmatrix to partmodels
                    _ea.GetEvent<UpdateTransformUIEvent>().Publish(new UI2PartModel(pm.index, Target.Transform));
                    //update to wpf mainmodel that model is selecteds
                }
                else
                {
                    //foreach (var item in Items)
                    //{
                    //    item.Highlight = false;
                    //}
                    //CenterOffset = new Vector3(0, 0, 0);
                }
            }
            


        }
        public void OnMouseMove3DHandler(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed&& sender is Viewport3DX)
            {
                var viewport = sender as Viewport3DX;

                var point = e.GetPosition(sender as IInputElement);
                
                if (Target != null)
                {
                    Console.WriteLine(Target.Transform.ToMatrix().ToString());
                    var pm = Target.DataContext as PartModel;
                    //update transmatrix to partmodels
                    _ea.GetEvent<UpdateTransformUIEvent>().Publish(new UI2PartModel(pm.index, Target.Transform));
                }
                //if (e.OriginalSource is FrameworkElement dp && dp.DataContext is Flag flag)
                //{
                //    DataObject dragData = new DataObject("Flag", flag);
                //    DragDrop.DoDragDrop(dragSource, dragData, DragDropEffects.Move);
                //    dragSource = null;
                //}
            }


        }
        public void OnMouseUp3DHandler(object sender, MouseUp3DEventArgs e)
        {
            var dev = e.OriginalInputEventArgs.Device as System.Windows.Input.MouseDevice;
            //&& dev.LeftButton == System.Windows.Input.MouseButtonState.Pressed
            if (Target != null )
            {
                Console.WriteLine(Target.Transform.ToMatrix().ToString());
                var pm = Target.DataContext as PartModel;
                //update transmatrix to partmodels
                _ea.GetEvent<UpdateTransformUIEvent>().Publish(new UI2PartModel(pm.index, Target.Transform));
            }
        }


        #region 解压

        /// <summary>
        /// 解压功能(解压压缩文件到指定目录)
        /// </summary>
        /// <param name="fileToUnZip">待解压的文件</param>
        /// <param name="zipedFolder">指定解压目标目录</param>
        /// <param name="password">密码</param>
        /// <returns>解压结果</returns>
        public static bool UnZip(string fileToUnZip, string zipedFolder, string password)
        {
            bool result = true;
            FileStream fs = null;
            ZipInputStream zipStream = null;
            ZipEntry ent = null;
            string fileName;

            if (!File.Exists(fileToUnZip))
                return false;

            if (!Directory.Exists(zipedFolder))
                Directory.CreateDirectory(zipedFolder);

            try
            {
                zipStream = new ZipInputStream(File.OpenRead(fileToUnZip));
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                while ((ent = zipStream.GetNextEntry()) != null)
                {
                    if (!string.IsNullOrEmpty(ent.Name))
                    {
                        fileName = Path.Combine(zipedFolder, ent.Name);
                        fileName = fileName.Replace('/', '\\');//change by Mr.HopeGi

                        int index = ent.Name.LastIndexOf('/');
                        if (index != -1 || fileName.EndsWith("\\"))
                        {
                            string tmpDir = (index != -1 ? fileName.Substring(0, fileName.LastIndexOf('\\')) : fileName) + "\\";
                            if (!Directory.Exists(tmpDir))
                            {
                                Directory.CreateDirectory(tmpDir);
                            }
                            if (tmpDir == fileName)
                            {
                                continue;
                            }
                        }

                        fs = File.Create(fileName);
                        int size = 2048;
                        byte[] data = new byte[size];
                        while (true)
                        {
                            size = zipStream.Read(data, 0, data.Length);
                            if (size > 0)
                                fs.Write(data, 0, data.Length);
                            else
                                break;
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
                if (ent != null)
                {
                    ent = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
            return result;
        }

        /// <summary>
        /// 解压功能(解压压缩文件到指定目录)
        /// </summary>
        /// <param name="fileToUnZip">待解压的文件</param>
        /// <param name="zipedFolder">指定解压目标目录</param>
        /// <returns>解压结果</returns>
        public static bool UnZip(string fileToUnZip, string zipedFolder)
        {
            bool result = UnZip(fileToUnZip, zipedFolder, null);
            return result;
        }

        #endregion
        private static void SetBinding(string path, DependencyObject dobj, DependencyProperty property, object viewModel, BindingMode mode = BindingMode.TwoWay)
        {
            var binding = new Binding(path);
            binding.Source = viewModel;
            binding.Mode = mode;
            BindingOperations.SetBinding(dobj, property, binding);
        }

    }
    

}