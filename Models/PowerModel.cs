using MathNet.Numerics.LinearAlgebra;
using System;

namespace AngelDLP
{
	/// <summary>
	///   Common model function.
	/// </summary>
	class PowerModel
	{
		/// <summary>
		///   Get value of the model function for the specified parameters.
		/// </summary>
		/// <param name = "x">X-coordinate of the function point.</param>
		/// <param name = "parameters">Model function parameters.</param>
		/// <param name = "y">Y-coordinate of the function point.</param>
		public void GetValue(double x, Vector<double> parameters, out double y)
		{
			y = parameters[0] * (1 - Math.Exp(-(x / parameters[1])));
		}

		/// <summary>
		///   Get gradient of the model function for the specified parameters.
		/// </summary>
		/// <param name = "x">X-coordinate of the function point.</param>
		/// <param name = "parameters">Model function parameters.</param>
		/// <param name = "gradient">Model function gradient.</param>
		public void GetGradient(double x, Vector<double> parameters, ref Vector<double> gradient)
		{
			gradient[0] = Math.Pow(x, parameters[1]);
			gradient[1] = (parameters[0] * Math.Pow(x, parameters[1]) * Math.Log(x));
		}

		/// <summary>
		///   Get vector of residuals for the specified parameters.
		/// </summary>
		/// <param name = "pointCount">Number of data points.</param>
		/// <param name = "dataX">X-coordinates of the data points.</param>
		/// <param name = "dataY">Y-coordinates of the data points.</param>
		/// <param name = "parameters">Model function parameters.</param>
		/// <param name = "residual">Vector of residuals.</param>
		public void GetResidualVector(int pointCount, Vector<double> dataX, Vector<double> dataY, Vector<double> parameters, ref Vector<double> residual)
		{
			double y;

			for (int j = 0; j < pointCount; j++)
			{
				GetValue(dataX[j], parameters, out y);

				residual[j] = (y - dataY[j]);
			}
		}
	}
}
