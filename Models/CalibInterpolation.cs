using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AngleDLP.Models
{
    class CalibInterpolation
    {
		private int grayDX = 40;
		private class GrayTestItem
		{
			public double X { get; set; }
			public double Y { get; set; }
			public double I { get; set; }

		}
		//public void DEM()
  //      {
		//	List<AltitudePoint>  pointList = new List<AltitudePoint>();
		//	XmlDocument doc = new XmlDocument();
		//	doc.Load("test.xml");
		//	XmlNode xn = doc.SelectSingleNode("root");


		//	// 得到根节点的所有子节点
		//	XmlNodeList xnl = xn.ChildNodes;

		//	foreach (XmlNode xn1 in xnl)
		//	{
		//		GrayTestItem grayTestItem = new GrayTestItem();
		//		// 将节点转换为元素，便于得到节点的属性值
		//		XmlElement xe = (XmlElement)xn1;
		//		// 得到Type和ISBN两个属性的属性值
		//		grayTestItem.X = Convert.ToDouble(xe.GetAttribute("X").ToString()) / grayDX;
		//		grayTestItem.Y = Convert.ToDouble(xe.GetAttribute("Y").ToString()) / grayDX;
		//		grayTestItem.I = Convert.ToDouble(xe.GetAttribute("i").ToString());
		//		AltitudePoint altitudePoint = new AltitudePoint(grayTestItem.X, grayTestItem.Y, grayTestItem.I);
		//		pointList.Add(altitudePoint);
		//	}
		//	//List<AltitudePoint> pointList;
		//	foreach (AltitudePoint point in pointList)
		//	{
		//		graphics.FillEllipse(Brushes.Black,
		//			new RectangleF((float)point.Y * demData.DEMCellSize,
		//			(float)point.X * demData.DEMCellSize,
		//			length,
		//			length));
		//	}
		//	//pictureBox_initialData.Image = bitmap;

		//	//button_SavePointsData.Enabled = true;
		//	//button_Interpolation.Enabled = true;
		//	DEMData calData ;
		//	InterpolationData interpolation = new InterpolationData(pointList, 2160 / grayDX, 3840 / grayDX);
		//	bool testbool = interpolation.IsRandomPointsOK();
		//	try
		//	{
		//		calData = new DEMData(interpolation.GetInterpolationData(this.progressBar_Deal, amplification));
		//	}
		//	catch (Exception exception)
		//	{
		//		//MessageBox.Show(exception.Message);
		//		return;
		//	}
		//	//button_SaveInterpolation.Enabled = true;
		//	//button_DrawContour.Enabled = true;
		//	//textBox_Interval.Enabled = true;

		//	interpolation.GetDrawingInfo();
		//	//pictureBox_InterpolationData.Image = calData.DEMBitmap();
		//	calData.DEMBitmap().Save("DEM_Bitimage.bmp");
		//}
    }
}
