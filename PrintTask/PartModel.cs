using DemoCore;
using HelixToolkit.Wpf.SharpDX;
using MatterHackers.Agg;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.VertexSource;
using Prism.Commands;
using Prism.Events;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using static AngelDLP.PrintTask;
using static AngelDLP.ViewModels.PrintScene3DViewModel;
namespace AngelDLP
{

    public class PartModel : ObservableObject
    {
        IEventAggregator _ea;
        public int index { get; private set; } = -1;//导入序号
        public string fileName { get; private set; } = "init Name";

        public string filePath { get; private set; }

        public string modelId { get; private set; }

        public bool isManual { get; set; }

        public bool isStandard { get; set; }


        public int printLayerOffset = 0;
        public int printInWhitchProjector = 0;
        public int layoutColumn = -1;
        public int layoutRow = -1;
        public List<List<List<double>>> layoutYRanges;//层-X-minmax
        public List<List<List<double>>> layoutXRanges;//层-Y-minmax
        
        public Vector3 bondsMin = new Vector3();
        public Vector3 bondsMax = new Vector3();
        public bool isRendering { get; set; } = true;

        public double DX { get; set; }
        public double DY { get; set; }
        public double DZ { get; set; }
        public double TX { get; set; }
        public double TY { get; set; }
        public double TZ { get; set; }
        public double RZ { get; set; }
        public double SliceHeight { get; set; }
        public double MaxSliceHeight { get; set; }

       
        public PointF connectPointForward_1 { get; set; }

        public PointF connectPointBackward_1 { get; set; }

        public PointF connectPointForward_2 { get; set; }

        public PointF connectPointBackward_2 { get; set; }
        public DelegateCommand ResetZ { get; private set; }
        public DelegateCommand ResetXYZR { get; private set; }
        public DelegateCommand SliceLayerCommand { get; private set; }
        public HelixToolkit.Wpf.SharpDX.MeshGeometry3D model;
        public HelixToolkit.Wpf.SharpDX.MeshGeometry3D Model { set { SetValue(ref model, value); } get => model; }

        public bool EnablePlane1 { get; set; } = true;

        public Plane Plane1 { get; set; } = new Plane(new Vector3(0, 0, -1), -2);

        public bool IsToPrint { get; set; } = true;

        public Transform3D ModelTransform { get; set; } = new TranslateTransform3D();
        public CuttingOperation CuttingOperation { get; set; } = CuttingOperation.Intersect;
        //public readonly Media3D.ScaleTransform3D scaleTransform = new Media3D.ScaleTransform3D();
        //public readonly Media3D.TranslateTransform3D translateTransform = new Media3D.TranslateTransform3D();
        //public Media3D.Transform3DGroup DynamicTransform { get; private set; } = new Media3D.Transform3DGroup();

        public PhongMaterial ItemMaterial { get; set; }

        private bool highlight = false;
        public bool Highlight
        {
            set
            {
                if (highlight == value) { return; }
                highlight = value;
                if (highlight)
                {
                    //orgMaterial = material;
                    //Material = PhongMaterials.PolishedBronze;

                }
                else
                {
                    //Material.EmissiveColor = Color.Transparent;
                    //Material = orgMaterial;
                    //Material = PhongMaterials.Bronze;
                }
            }
            get
            {
                return highlight;
            }
        }

        public PartModel()
        {

        }

        [Obsolete]
        public PartModel(IEventAggregator ea, int ind, FileSelected fi, Stream stream, Matrix3D matrix  ,bool ismanual, HelixToolkit.Wpf.SharpDX.PhongMaterial material,bool isSTL)
        {
            _ea = ea;
            ResetZ = new DelegateCommand(ResetZV);
            SliceLayerCommand = new DelegateCommand(SliceLayerV);
            ResetXYZR = new DelegateCommand(ResetXYZRV);
            index = ind;
            fileName = fi.modelName;
            modelId = fi.modelId;
            filePath = fi.stl;
            isManual = ismanual;
            ModelTransform = new MatrixTransform3D(matrix);
            //Material = PhongMaterials.Bronze;
            HelixToolkit.Wpf.SharpDX.MeshGeometry3D[] list = new HelixToolkit.Wpf.SharpDX.MeshGeometry3D[10];
            if (isSTL)
            {
                list = LoadSTL3ds(stream).Select(x => x.Geometry as HelixToolkit.Wpf.SharpDX.MeshGeometry3D).ToArray();
            }
            else
            {
                list = LoadOBJ3ds(stream).Select(x => x.Geometry as HelixToolkit.Wpf.SharpDX.MeshGeometry3D).ToArray();
            }

            foreach (var re in list)
            {
                if (re.Indices.Count > 0)
                {
                    Model = re;
                    ItemMaterial = material;
                    bondsMin = re.Bound.Minimum;
                    bondsMax = re.Bound.Maximum;
                    DX = re.Bound.Width;
                    DY = re.Bound.Height;
                    DZ = re.Bound.Depth;
                    TX = matrix.OffsetX;
                    TY = matrix.OffsetY;
                    TZ = matrix.OffsetZ;
                    RZ = 180 * Math.Atan2(matrix.M12, matrix.M11) / Math.PI;
                    MaxSliceHeight = Math.Floor(DZ) + 1;
                    SliceHeight = 0;
                    //_ea.GetEvent<MessageEvent>().Publish($"time{sw.Elapsed.TotalMilliseconds}ms");
                    //double time = sw.Elapsed.TotalMilliseconds;
                    break;
                }
            }
            Plane1 = new Plane(new Vector3(0, 0, -1), (float)-DZ - 1);//默认切平面在模型上空1mm
                                                                      //尝试切层为轨迹

        }
        public class P_pointRx
        {
            public int x;
            public double y;
            public P_pointRx() { }
            public P_pointRx(double xx, double yy)
            {
                this.x = (int)Math.Round(xx * 5) + 200;
                this.y = yy;
            }
        }

        public class P_pointRy
        {
            public double x;
            public int y;
            public P_pointRy() { }
            public P_pointRy(double xx, double yy)
            {
                this.x = xx;
                this.y = (int)Math.Round(yy * 5) + 200;
            }
        }
        public List<List<double>> DrawSegments(IEnumerable<IList<Vector3>> segmentList)
        {
            List<List<double>> double2x = new List<List<double>>();


            ImageBuffer testImage = new ImageBuffer(400, 400, 32, new BlenderBGRA());
            foreach (var se in segmentList)
            {
                VertexStorage vertexStorage = new VertexStorage();
                foreach (var p in se)
                {
                    vertexStorage.Add(5 * p.X + 200, 5 * p.Y + 200, ShapePath.FlagsAndCommand.LineTo);
                }
                Contour contour = new Contour(vertexStorage);
                testImage.NewGraphics2D().Render(contour, ColorF.rgba_pre(1, 1, 1));
            }
            for (int j = 0; j < 400; j++)
            {
                List<double> double1x = new List<double>();
                int minInt = 401;
                int maxInt = -1;
                for (int k = 0; k < 400; k++)
                {
                    MatterHackers.Agg.Color color = testImage.GetPixel(j, k);
                    if (color.red == 0)
                    {
                        continue;
                    }
                    if (k > maxInt) { maxInt = k; }
                    if (k < minInt) { minInt = k; }
                }
                double1x.Add((double)minInt / 5 - 40);
                double1x.Add((double)maxInt / 5 - 40);
                double2x.Add(double1x);
            }
            return double2x;
        }
        public void ResetZV()
        {
            _ea.GetEvent<PartSelectByListViewEvent>().Publish(this.index);
            this.TZ = - bondsMin[2];
            Transform3D temp = new TranslateTransform3D(new Vector3D(ModelTransform.Value.OffsetX, ModelTransform.Value.OffsetY, -bondsMin[2]));
            _ea.GetEvent<UpdateTransform3DsEvent>().Publish(new UI2PartModel(this.index, temp));
        }
        public void ResetXYZRV()
        {
            _ea.GetEvent<PartSelectByListViewEvent>().Publish(this.index);
            this.TX = 0;
            this.TY = 0;
            this.TZ = 0;
            Transform3D temp = new TranslateTransform3D(new Vector3D(0, 0, 0));
            _ea.GetEvent<UpdateTransform3DsEvent>().Publish(new UI2PartModel(this.index, temp));
        }
        public void SliceLayerV()
        {
            _ea.GetEvent<PartSelectByListViewEvent>().Publish(this.index);

            Plane1 = new Plane(new Vector3(0, 0, -1), (float)-SliceHeight);

        }

        [Obsolete]
        public List<Object3D> LoadSTL3ds(Stream stream)
        {
            var reader = new StLReader();
            var list = reader.Read(stream);
            return list;

        }
        [Obsolete]
        public List<Object3D> LoadOBJ3ds(Stream stream)
        {

            var readerx = new ObjReader();
            var list = readerx.Read(stream);
            return list;

        }
        public void GetLayoutXYRange()
        {
            //新建排版辅助切层文件
            List<List<List<double>>> double3x = new List<List<List<double>>>();
            List<List<List<double>>> double3y = new List<List<List<double>>>();
            for (int i = 0; i < 6; i++)
            {
                double3x.Add(new List<List<double>>());
                double3y.Add(new List<List<double>>());
            }
            System.Threading.Tasks.Parallel.For(0, 6, i =>//for(int i = 0;i <6; i++)
            {//6个切层并行计算
                var segments = MeshGeometryHelper.GetContourSegments(Model, new Vector3(0, 0, (float)(0.5 + 2 * i)), new Vector3(0, 0, -1)).ToList(); //calculates the contour                                                                                                                      //DrawSegments(testb);
                List<P_pointRx> psRx = new List<P_pointRx>();
                List<P_pointRy> psRy = new List<P_pointRy>();
                foreach (var p in segments)
                {
                    psRx.Add(new P_pointRx(p.X, p.Y));
                    psRy.Add(new P_pointRy(p.X, p.Y));
                }
                var psRxG = psRx.GroupBy(x => x.x).ToList();
                var psRyG = psRy.GroupBy(x => x.y).ToList();
                List<List<double>> double2x = new List<List<double>>();
                List<List<double>> double2x2 = new List<List<double>>();
                for (int j = 0; j < 400; j++)
                {
                    List<double> double1x = new List<double>();
                    double1x.Add(40.2);
                    double1x.Add(-40.2);
                    double2x.Add(double1x);
                }
                foreach (var li in psRxG)
                {
                    var maxValue = li.Max(x => x.y);
                    var minValue = li.Min(x => x.y);
                    double2x[li.Key][0] = minValue;
                    double2x[li.Key][1] = maxValue;
                }
                double2x2.Add(double2x[0]);
                for (int l = 1; l < 399; l++)
                {
                    List<double> double1x2 = new List<double>();
                    double1x2.Add(double2x[l][0]);
                    double1x2.Add(double2x[l][1]);
                    double twosideMax = Math.Max(double2x[l - 1][1], double2x[l + 1][1]);
                    double twosideMin = Math.Min(double2x[l - 1][0], double2x[l + 1][0]);
                    if (double2x[l][1] < twosideMax) { double1x2[1] = twosideMax; }
                    if (double2x[l][0] > twosideMin) { double1x2[0] = twosideMin; }
                    double2x2.Add(double1x2);
                }
                double2x2.Add(double2x[0]);
                double3x[i] = double2x2;

                List<List<double>> double2y = new List<List<double>>();
                List<List<double>> double2y2 = new List<List<double>>();
                for (int j = 0; j < 400; j++)
                {
                    List<double> double1y = new List<double>();
                    double1y.Add(40.2);
                    double1y.Add(-40.2);
                    double2y.Add(double1y);
                }
                foreach (var li in psRyG)
                {
                    var maxValue = li.Max(x => x.x);
                    var minValue = li.Min(x => x.x);
                    double2y[li.Key][0] = minValue;
                    double2y[li.Key][1] = maxValue;
                }
                double2y2.Add(double2y[0]);
                for (int l = 1; l < 399; l++)
                {
                    List<double> double1y2 = new List<double>();
                    double1y2.Add(double2y[l][0]);
                    double1y2.Add(double2y[l][1]);
                    double twosideMax = Math.Max(double2y[l - 1][1], double2y[l + 1][1]);
                    double twosideMin = Math.Min(double2y[l - 1][0], double2y[l + 1][0]);
                    if (double2y[l][1] < twosideMax) { double1y2[1] = twosideMax; }
                    if (double2y[l][0] > twosideMin) { double1y2[0] = twosideMin; }
                    double2y2.Add(double1y2);
                }
                double2y2.Add(double2y[0]);
                double3y[i] = double2y2;

            });
            layoutYRanges = double3x;
            layoutXRanges = double3y;
        }
    }
    public class UI2PartModel
    {
        public int index { get; set; }
        public Transform3D trans { get; set; }
        public UI2PartModel(int ind, Transform3D tr)
        {
            trans = tr;
            index = ind;
        }
    }

}
