using AngelDLP;
using AngleDLP.PMC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AngleDLP.Models
{
    public static class PlatformCalib
    {
        public class PlatformTestItem
        {
            public double plateformY { get; set; }
            public double zValue { get; set; }
        }
        public class PlatformTestResult
        {
            public List<List<PlatformTestItem>> platformTestItems { get; set; } = new List<List<PlatformTestItem>>();

            public bool isPassed { get; set; }

            public bool canBeTested { get; set; }
            public double zInitComp { get; set; }

            public string plateNo { get; set; }
        }
        public class PlatformCalibResult
        {
            public BitmapSource bitmapSource { get; set; }
            public DEMData data { get; set; }
        }
        public static TransformedBitmap ToBitmapSource(BitmapImage bitmapSource)
        {
            var tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = bitmapSource;
            var transform = new ScaleTransform(1, 1, 0, 0); ;
            tb.Transform = transform;
            tb.EndInit();
            return tb;
        }
        public static BitmapImage bitmapToBitmapImage(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                BitmapImage reslut = new BitmapImage();
                reslut.BeginInit();
                reslut.CacheOption = BitmapCacheOption.OnLoad;
                reslut.StreamSource = stream;
                reslut.EndInit();
                reslut.Freeze();
                return reslut;
            }
        }
        public static PlatformCalibResult LoadBaseCalibrationTestData(List<Vector3> points)
        {
            PlatformCalibResult res = new PlatformCalibResult();
            //List<GrayCalib> calibGrayPoint = new List<GrayCalib>();
            int grayDX = 4;
            DEMData calData;
            List<AltitudePoint> pointList = new List<AltitudePoint>();

            foreach (var p in points)
            {

                AltitudePoint altitudePoint = new AltitudePoint(p.X / grayDX, p.Y / grayDX, p.Z);
                pointList.Add(altitudePoint);
            }

            InterpolationData interpolation = new InterpolationData(pointList, 432 / grayDX, 384 / grayDX);
            bool testbool = interpolation.IsRandomPointsOK();
            try
            {
                calData = new DEMData(interpolation.GetInterpolationData(1));
                interpolation.GetDrawingInfo();

                Bitmap bbm = calData.DEMBitmap();
                bbm.RotateFlip(RotateFlipType.Rotate90FlipXY);
                //bbm.Save(filename);
                //_ea.GetEvent<MessageEvent>().Publish("最高值：" + calData.max.ToString("0.##") + "\n最低值：" + calData.min.ToString("0.##"));
                res.bitmapSource = ToBitmapSource(bitmapToBitmapImage(bbm));
                res.bitmapSource.Freeze();
                //using (FileStream fs = new FileStream(filename, FileMode.Create))
                //{
                //    using (StreamWriter sw = new StreamWriter(fs))
                //    {
                //        string jsonStrout = Newtonsoft.Json.JsonConvert.SerializeObject(calData);
                //        sw.Write(jsonStrout);
                //    }
                //}
                res.data = calData;
                return res;
            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
                return new PlatformCalibResult() ;
            }





        }
        public static PlatformCalibResult GetPlatformCalibResult(PlatformTestResult platformTestResult,PrinterMotionConfig printerMotionConfig)
        {
            List<Vector3> points = new List<Vector3>();
            for (int i =0;i < printerMotionConfig.scraperConfig.zSensers.Count; i++)
            {
                foreach (var p in platformTestResult.platformTestItems[i])
                {
                    if (p.plateformY < 5 || p.plateformY > 370) { continue; }
                    points.Add(
                        new Vector3()
                        {
                            X = (float)printerMotionConfig.scraperConfig.zSensers[i].x,
                            Y = (float)p.plateformY,
                            Z = (float)(p.zValue + platformTestResult.zInitComp - printerMotionConfig.scraperConfig.zSensers[i].zValue - printerMotionConfig.scraperConfig.zSensers[i].zGap)
                        });
                }
            }


            
            var planFit = new PlaneFit();
            planFit.Fit(points);

            return LoadBaseCalibrationTestData(points);
        }
        
public class PlaneFit
{
    double[] planefunc = new double[4];
    List<Vector3> oripoints = new List<Vector3>();
    List<double> errors = new List<double>();
    /// <summary>
    /// 平面方程拟合，ax+by+cz+d=0,其中a=result[0],b=result[1],c=result[2],d=result[3]
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public void Fit(List<Vector3> points)
    {
        oripoints = points;
        double[] result = new double[4];
        int n = points.Count;
        double[,] A = new double[n, 3];
        double[,] E = new double[n, 1];
        for (int i = 0; i < n; i++)
        {
            A[i, 0] = points[i].X - points[i].Z;
            A[i, 1] = points[i].Y - points[i].Z;
            A[i, 2] = 1;
            E[i, 0] = -points[i].Z;
        }
        double[,] AT = MatrixInver(A);
        double[,] ATxA = MatrixMultiply(AT, A);
        double[,] OPPAxTA = MatrixOpp(ATxA);
        double[,] OPPATAxAT = MatrixMultiply(OPPAxTA, AT);
        double[,] DP = MatrixMultiply(OPPATAxAT, E);
        result[0] = DP[0, 0];
        result[1] = DP[1, 0];
        result[2] = 1 - result[0] - result[1];
        result[3] = DP[2, 0];
        planefunc = result;
        ErrorCaculate();
    }
    /// <summary>
    /// 平面度计算
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public void ErrorCaculate()
    {
        double A = planefunc[0];
        double B = planefunc[1];
        double C = planefunc[2];
        double D = planefunc[3];
        foreach (var p0 in oripoints)
        {
            double S = Math.Abs(A * p0.X + B * p0.Y + C * p0.Z + D) / Math.Sqrt(A * A + B * B + C * C);
            errors.Add(S);
        }

    }
    /// <summary>
    /// 矩阵转置
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    private double[,] MatrixInver(double[,] matrix)
    {
        double[,] result = new double[matrix.GetLength(1), matrix.GetLength(0)];
        for (int i = 0; i < matrix.GetLength(1); i++)
            for (int j = 0; j < matrix.GetLength(0); j++)
                result[i, j] = matrix[j, i];
        return result;
    }
    /// <summary>
    /// 矩阵相乘
    /// </summary>
    /// <param name="matrixA"></param>
    /// <param name="matrixB"></param>
    /// <returns></returns>
    private double[,] MatrixMultiply(double[,] matrixA, double[,] matrixB)
    {
        double[,] result = new double[matrixA.GetLength(0), matrixB.GetLength(1)];
        for (int i = 0; i < matrixA.GetLength(0); i++)
        {
            for (int j = 0; j < matrixB.GetLength(1); j++)
            {
                result[i, j] = 0;
                for (int k = 0; k < matrixB.GetLength(0); k++)
                {
                    result[i, j] += matrixA[i, k] * matrixB[k, j];
                }
            }
        }
        return result;
    }
    /// <summary>
    /// 矩阵的逆
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    private double[,] MatrixOpp(double[,] matrix)
    {
        double X = 1 / MatrixSurplus(matrix);
        double[,] matrixB = new double[matrix.GetLength(0), matrix.GetLength(1)];
        double[,] matrixSP = new double[matrix.GetLength(0), matrix.GetLength(1)];
        double[,] matrixAB = new double[matrix.GetLength(0), matrix.GetLength(1)];

        for (int i = 0; i < matrix.GetLength(0); i++)
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                for (int m = 0; m < matrix.GetLength(0); m++)
                    for (int n = 0; n < matrix.GetLength(1); n++)
                        matrixB[m, n] = matrix[m, n];
                {
                    for (int x = 0; x < matrix.GetLength(1); x++)
                        matrixB[i, x] = 0;
                    for (int y = 0; y < matrix.GetLength(0); y++)
                        matrixB[y, j] = 0;
                    matrixB[i, j] = 1;
                    matrixSP[i, j] = MatrixSurplus(matrixB);
                    matrixAB[i, j] = X * matrixSP[i, j];
                }
            }
        return MatrixInver(matrixAB);
    }
    /// <summary>
    /// 矩阵的行列式的值  
    /// </summary>
    /// <param name="A"></param>
    /// <returns></returns>
    public double MatrixSurplus(double[,] matrix)
    {
        double X = -1;
        double[,] a = matrix;
        int i, j, k, p, r, m, n;
        m = a.GetLength(0);
        n = a.GetLength(1);
        double temp = 1, temp1 = 1, s = 0, s1 = 0;

        if (n == 2)
        {
            for (i = 0; i < m; i++)
                for (j = 0; j < n; j++)
                    if ((i + j) % 2 > 0) temp1 *= a[i, j];
                    else temp *= a[i, j];
            X = temp - temp1;
        }
        else
        {
            for (k = 0; k < n; k++)
            {
                for (i = 0, j = k; i < m && j < n; i++, j++)
                    temp *= a[i, j];
                if (m - i > 0)
                {
                    for (p = m - i, r = m - 1; p > 0; p--, r--)
                        temp *= a[r, p - 1];
                }
                s += temp;
                temp = 1;
            }

            for (k = n - 1; k >= 0; k--)
            {
                for (i = 0, j = k; i < m && j >= 0; i++, j--)
                    temp1 *= a[i, j];
                if (m - i > 0)
                {
                    for (p = m - 1, r = i; r < m; p--, r++)
                        temp1 *= a[r, p];
                }
                s1 += temp1;
                temp1 = 1;
            }

            X = s - s1;
        }
        return X;
    }

}
    }
}
