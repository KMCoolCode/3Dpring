using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AngelDLP
{
	class InterpolationData
	{
		class RegressionPoint
		{
			public double distance;
			public double semivariogram;
			public RegressionPoint(double distance, double semivariogram)
			{
				this.distance = distance;
				this.semivariogram = semivariogram;
			}
		}

		class StartEnd
		{
			public int Start { private set; get; }
			public int End { private set; get; }

			public StartEnd(int Start, int End)
			{
				this.Start = Start;
				this.End = End;
			}
		}

		public int row { private set; get; }//行
		public int column { private set; get; }//列

		private List<AltitudePoint> pointList = new List<AltitudePoint>();

		//插值后的数据
		private double[,] interpolationDEMData;
        public InterpolationData(List<AltitudePoint> pointList, int row, int column)
		{
			this.row = row;
			this.column = column;
			foreach(AltitudePoint p in pointList)
			{
				this.pointList.Add(new AltitudePoint(p.X, p.Y, p.AltitudeValue));
			}

			GetRegressionPoints();
        }

		private double formula_c;
		private double formula_r;

		#region 计算拟合曲线中的a，r   "f(x)=a*(1-e^(-x/r))"

		//求得用于显示的点集
		private List<RegressionPoint> showPoints;
		//求得用于拟合计算的点集
		private List<RegressionPoint> calPoints;

        private void GetRegressionPoints()
		{
			List<RegressionPoint> initialPoints = new List<RegressionPoint>();

			int length = pointList.Count;

			for (int m = 0; m < length; m++)
				for (int n = 0; n < length; n++)
				{
					if(m!=n)
					{
						double distance = Math.Sqrt(Math.Pow(pointList[m].X - pointList[n].X, 2) + Math.Pow(pointList[m].Y - pointList[n].Y, 2));
						double semivariogram = 0.5 * Math.Pow((pointList[m].AltitudeValue - pointList[n].AltitudeValue), 2);
						initialPoints.Add(new RegressionPoint(distance, semivariogram));
					}
                }

			double maxDistance = 0;
            double minDistance= 1000000000000000;

			foreach(var point in initialPoints)
			{
				if (point.distance > maxDistance) maxDistance = point.distance;
				if (point.distance < minDistance) minDistance = point.distance;
			}

			showPoints = new List<RegressionPoint>();
			calPoints = new List<RegressionPoint>();

			for (int n = (int)minDistance; n <= Math.Min(100000, maxDistance); n++)
			{
				var tempPointsList = initialPoints.Where(x => (int)x.distance == n);

				if (tempPointsList.Count() > 0)
				{
					double value = 0;
					foreach (var point in tempPointsList)
					{
						value += point.semivariogram;
					}
					value = value / tempPointsList.Count();

					showPoints.Add(new RegressionPoint(n, value));
                }
            }

			for (int n = 0; n < showPoints.Count / 10; n++)
			{
				double x = 0;
				double y = 0;
				for (int m = n * 10; m < (n + 1) * 10; m++)
				{
					x += showPoints[m].distance;
					y += showPoints[m].semivariogram;
                }
				calPoints.Add(new RegressionPoint(x / 10, y / 10));
			}

			FitPoints(calPoints);
        }

		//高斯牛顿法
		private void FitPoints(List<RegressionPoint> calPoints)
		{
			PowerModel model = new PowerModel();
			GaussNewtonSolver solver =new GaussNewtonSolver(0.001, 0.001, 10000, new DenseVector(new[] { 50.0, 1.5 }));
			List<Vector<double>> solverIterations=new List<Vector<double>>();

			double[] x = new double[calPoints.Count];
			double[] y = new double[calPoints.Count];
			for (int n = 0; n < calPoints.Count; n++) 
			{
				//缩小数值，方便计算，之后会还原
				x[n] = calPoints[n].distance/5;
				y[n] = calPoints[n].semivariogram/1000;
			}

			Vector<double> dataX = new DenseVector(x);
			Vector<double> dataY = new DenseVector(y);

			solver.Estimate(model, calPoints.Count, dataX, dataY, ref solverIterations);

			formula_c = solverIterations.Last()[0] * 5;
			formula_r = solverIterations.Last()[1] * 1000;
		}

		#endregion

		//获取绘图信息
		public void GetDrawingInfo()
		{
			double[,] dataForShow = new double[showPoints.Count, 2];
			double[,] dataForCal = new double[calPoints.Count, 2];
			double[,] dataForLine = new double[showPoints.Count, 2];

			for (int n = 0; n < showPoints.Count; n++)
			{
				dataForShow[n, 0] = showPoints[n].distance;
				dataForShow[n, 1] = showPoints[n].semivariogram;
			}

			for(int n = 0; n < calPoints.Count; n++)
			{
				dataForCal[n, 0] = calPoints[n].distance;
				dataForCal[n, 1] = calPoints[n].semivariogram;
			}

			for (int n = 0; n < showPoints.Count; n++)
			{
				dataForLine[n, 0] = showPoints[n].distance;
				dataForLine[n, 1] = formula_c * 200 * (1 - Math.Exp(-dataForLine[n, 0] / (formula_r / 200)));
			}

			//var form = new Form_Semivariogram(formula_c * 200, formula_r / 200, dataForShow, dataForCal, dataForLine);
			//form.Show();
		}

		private Matrix<double> Kn;//K的逆矩阵

		//测试随机点数据是否满足条件
		public Boolean IsRandomPointsOK()
		{
			int size = pointList.Count;
			var K = new DenseMatrix(size, size);
			for (int m = 0; m < size; m++)
				for (int n = 0; n < size; n++)
					K[m, n] = CalCij(pointList[m].X, pointList[m].Y, pointList[n].X, pointList[n].Y);

			Kn = K.Inverse();
			for (int m = 0; m < size; m++)
				for (int n = 0; n < size; n++)
				{
					if (double.IsNaN(Kn[m, n])) return false;
				}
			return true;
		}

		private int amplification;
		private int size;
		private int progressBarValue;

		/// <summary>
		/// 获取插值后的数据
		/// </summary>
		/// <param name="progreaaBar">进度条</param>
		/// <param name="amplification">输出的数据比原始的数据放大的倍数</param>
		/// <returns>存有插值后的数组</returns>
		public double[,] GetInterpolationData(ProgressBar progressBar, int amplification)
		{
			this.amplification = amplification;
			this.size = pointList.Count;
			this.progressBarValue = 0;

			interpolationDEMData = new double[(row - 1) * amplification + 1, (column - 1) * amplification + 1];
			progressBar.Maximum = interpolationDEMData.Length;

            for (int m = 0; m < interpolationDEMData.GetLength(0); m++)
                for (int n = 0; n < interpolationDEMData.GetLength(1); n++)
                {
                    double realX = m / (double)amplification;
                    double realY = n / (double)amplification;

                    var D = new DenseVector(size);
                    for (int p = 0; p < size; p++)
                        D[p] = CalCij(pointList[p].X, pointList[p].Y, realX, realY);

                    var namuta = Kn.LeftMultiply(D);
                    for (int q = 0; q < size; q++)
                        interpolationDEMData[m, n] += namuta[q] * pointList[q].AltitudeValue;

                    progressBar.Value++;
                    //if (double.IsNaN(interpolationDEMData[m, n])) throw new Exception("数值异常，请重新生成随机点或选择其他DEM数据");
                }

			return interpolationDEMData;
        }
		public double[,] GetInterpolationData(int amplification)
		{
			this.amplification = amplification;
			this.size = pointList.Count;
			this.progressBarValue = 0;

			interpolationDEMData = new double[(row - 1) * amplification + 1, (column - 1) * amplification + 1];
			

			for (int m = 0; m < interpolationDEMData.GetLength(0); m++)
				for (int n = 0; n < interpolationDEMData.GetLength(1); n++)
				{
					double realX = m / (double)amplification;
					double realY = n / (double)amplification;

					var D = new DenseVector(size);
					for (int p = 0; p < size; p++)
						D[p] = CalCij(pointList[p].X, pointList[p].Y, realX, realY);

					var namuta = Kn.LeftMultiply(D);
					for (int q = 0; q < size; q++)
						interpolationDEMData[m, n] += namuta[q] * pointList[q].AltitudeValue;

					
					//if (double.IsNaN(interpolationDEMData[m, n])) throw new Exception("数值异常，请重新生成随机点或选择其他DEM数据");
				}

			return interpolationDEMData;
		}
		//计算Cij
		private double CalCij(double x1, double y1, double x2, double y2)
		{
			double distance = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

			if (distance == 0)
			{
				return formula_c;
			}
			else
			{
				return formula_c* Math.Exp(-distance / formula_r);
			}
		}

		/// <summary>
		/// 得到用于保存插值数据的byte[]
		/// </summary>
		/// <param name="length">byte[]的长度</param>
		/// <returns>byte[]数组，用于FileStream</returns>
		public byte[] GetByteInterpolationData(out int length)
		{
			string data = null;

			for (int m = 0; m < interpolationDEMData.GetLength(0); m++)
			{
				data += interpolationDEMData[m, 0];
                for (int n = 1; n < interpolationDEMData.GetLength(1); n++)
				{
					data += "," + interpolationDEMData[m, n];
				}
				data += "\n";
            }
			byte[] result = Encoding.Default.GetBytes(data);
			length = result.Length;
			return result;
		}

    }
}
