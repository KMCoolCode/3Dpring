using AngleDLP.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AngelDLP
{
	[Serializable]
	public class DEMData
	{
		public double[,] values {  set; get; }//高程数据
		public int row {  set; get; }//行
		public int column {  set; get; }//列
		public int DEMCellSize {  set; get; }//DEM图像绘制的格网边长

		public double max {  set; get; }//高程值最高点
		public double min {  set; get; }//高程值最低点

		public DEMData()
		{

		}
		public DEMData(string allData)
		{
			DEMCellSize = 10;

			this.row = allData.Split('\n').Count() - 1;
			this.column = allData.Split('\n')[0].Split(',').Count();

			values = GetDataFromString(allData);
			AnalyseData();
        }

		public DEMData(double[,] data)
		{
			DEMCellSize = 10;

			this.row = data.GetLength(0);
			this.column = data.GetLength(1);

			values = data;
			AnalyseData();
		}

		/// <summary>
		/// 根据DEM高程值绘制图像
		/// </summary>
		/// <returns>DEM图像</returns>
		public Bitmap DEMBitmap()
		{
			Bitmap bitmap = new Bitmap(this.column * DEMCellSize, this.row * DEMCellSize);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

			Bitmap colorRamp =  Resources.ColorRamp;

			for (int y = 0; y < this.row; y++)
			{
				for (int x = 0; x < this.column; x++)
				{
					int length = (int)((colorRamp.Width - 1) * (this.max - values[y, x]) / (this.max - this.min));
					Color color = colorRamp.GetPixel(colorRamp.Width-length-1, 1);
					graphics.FillRectangle(new SolidBrush(color), new Rectangle(x * DEMCellSize, y * DEMCellSize, DEMCellSize, DEMCellSize));
				}
			}
			//bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);//旋转图片
			//graphics.DrawString("10130422 张驰", new Font("Arial", bitmap.Width / 30), new SolidBrush(Color.Black), 10, 10);

			graphics.Dispose();
			return bitmap;
		}

		///// <summary>
		///// 在DEM中随机取点，默认取100个
		///// </summary>
		///// <param name="count">所取点的个数</param>
		///// <returns>随机点的合集</returns>
		//public List<AltitudePoint> GetRandomPoints(int count = 100)
		//{
		//	List<AltitudePoint> list = new List<AltitudePoint>();

		//	Random random = new Random();

		//	for (int n = 0; n < count; n++)
		//	{
		//		int randomNumber = random.Next(this.row * this.column);
		//		int x = randomNumber / this.column;
		//		int y = randomNumber - x * this.column;

		//		list.Add(new AltitudePoint(x, y, values[x, y]));
  //          }

		//	list.Sort(delegate (AltitudePoint p1, AltitudePoint p2)
		//	{
		//		return p1.X * this.column + p1.Y > p2.X * this.column + p2.Y ? 1 : -1;
  //          });
		//	return list;
  //      }

		/// <summary>
		/// 读取allData中的高程数据
		/// </summary>
		/// <param name="allData">文件中所有的字符</param>
		/// <returns>高程数组</returns>
		private double[,] GetDataFromString(string allData)
		{
			double[,] data = new double[row, column];

			string[] lines = allData.Split('\n');

			for (int r = 0; r < this.row; r++)
			{
				for (int c = 0; c < this.column; c++)
				{
					data[r, c] = Convert.ToDouble(lines[r].Split(',')[c]);
                }
			}

			return data;
        }

		/// <summary>
		/// 基础值初始化,最大值，最小值
		/// </summary>
		private void AnalyseData()
		{
			min = values[0, 0];
			max = values[0, 0];

			for (int x = 0; x < this.row; x++)
			{
				for (int y = 0; y < this.column; y++)
				{
					min = min < values[x, y] ? min : values[x, y];
					max = max > values[x, y] ? max : values[x, y];
				}
			}
		}
	}

	/// <summary>
	/// 关于高程点的类
	/// </summary>
	class AltitudePoint
	{
		//X坐标
		public double X { private set; get; }
		//Y坐标
		public double Y { private set; get; }
		//高程值
		public double AltitudeValue { private set; get; }

		public AltitudePoint(double x, double y, double altitudeValue)
		{
			this.X = x;
			this.Y = y;
			this.AltitudeValue = altitudeValue;
		}

		public override string ToString()
		{
			return X.ToString() + "," + Y.ToString() + "," + AltitudeValue.ToString();
        }
	}
}
