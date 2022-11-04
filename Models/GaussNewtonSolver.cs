using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;

namespace AngelDLP
{
	class GaussNewtonSolver
	{
		private double minimumDeltaValue;
		private double minimumDeltaParameters;
		private int maximumIterations;
		private Vector<double> guess;

		private Vector<double> dataX;
		private Vector<double> dataY;

		public GaussNewtonSolver(double minimumDeltaValue, double minimumDeltaParameters, int maximumIterations, Vector<double> guess)
		{
			this.minimumDeltaValue = minimumDeltaValue;
			this.minimumDeltaParameters = minimumDeltaParameters;
			this.maximumIterations = maximumIterations;
			this.guess = guess;
		}

		/// <summary>
		///   Get value of the objective function.
		/// </summary>
		/// <param name = "model">Model function.</param>
		/// <param name = "pointCount">Number of data points.</param>
		/// <param name = "dataX">X-coordinates of the data points.</param>
		/// <param name = "dataY">Y-coordinates of the data points.</param>
		/// <param name = "parameters">Model function parameters.</param>
		/// <param name = "value">Objective function value.</param>
		private void GetObjectiveValue(PowerModel model, int pointCount, Vector<double> parameters, out double value)
		{
			value = 0.0;

			double y = 0.0;

			for (int j = 0; j < pointCount; j++)
			{
				model.GetValue(
					dataX[j],
					parameters,
					out y);

				value += Math.Pow(
					y - dataY[j],
					2.0);
			}

			value *= 0.5;
		}

		/// <summary>
		///   Get Jacobian matrix of the objective function.
		/// </summary>
		/// <param name = "model">Model function.</param>
		/// <param name = "pointCount">Number of data points.</param>
		/// <param name = "dataX">X-coordinates of the data points.</param>
		/// <param name = "dataY">Y-coordinates of the data points.</param>
		/// <param name = "parameters">Model function parameters.</param>
		/// <param name = "jacobian">Jacobian matrix of the objective function.</param>
		private void GetObjectiveJacobian(PowerModel model, int pointCount, Vector<double> parameters, ref Matrix<double> jacobian)
		{
			int parameterCount = parameters.Count;

			// fill rows of the Jacobian matrix
			// j-th row of a Jacobian is the gradient of model function in j-th measurement
			for (int j = 0; j < pointCount; j++)
			{
				Vector<double> gradient = new DenseVector(parameterCount);

				model.GetGradient(
					dataX[j],
					parameters,
					ref gradient);

				jacobian.SetRow(j, gradient);
			}
		}

		/// <summary>
		///   Estimates the specified model.
		/// </summary>
		/// <param name = "model">Model function.</param>
		/// <param name = "solverOptions">Least squares solver options.</param>
		/// <param name = "pointCount">Number of data points.</param>
		/// <param name = "dataX">X-coordinates of the data points.</param>
		/// <param name = "dataY">Y-coordinates of the data points.</param>
		/// <param name = "iterations">Estimated model function parameters.</param>
		public void Estimate(PowerModel model, int pointCount, Vector<double> dataX, Vector<double> dataY, ref List<Vector<double>> iterations)
		{
			this.dataX = dataX;
			this.dataY = dataY;

			int n = guess.Count;

			Vector<double> parametersCurrent = guess;
			Vector<double> parametersNew = new DenseVector(n);

			double valueCurrent;
			double valueNew;

			GetObjectiveValue(model, pointCount, parametersCurrent, out valueCurrent);

			while (true)
			{
				Matrix<double> jacobian = new DenseMatrix(pointCount, n);
				Vector<double> residual = new DenseVector(pointCount);

				GetObjectiveJacobian(model, pointCount, parametersCurrent, ref jacobian);

				model.GetResidualVector(pointCount, dataX, dataY, parametersCurrent, ref residual);

				Vector<double> step = jacobian.Transpose().Multiply(jacobian).Cholesky().Solve(jacobian.Transpose().Multiply(residual));

				parametersCurrent.Subtract(step, parametersNew);

				GetObjectiveValue(model, pointCount, parametersNew, out valueNew);

				iterations.Add(parametersNew);

				if (ShouldTerminate(valueCurrent, valueNew, iterations.Count, parametersCurrent, parametersNew))
				{
					break;
				}

				parametersNew.CopyTo(parametersCurrent);
				valueCurrent = valueNew;
			}
		}

		/// <summary>
		///   Check whether the solver should terminate computation in current iteration.
		/// </summary>
		/// <param name = "valueCurrent">Current value of the objective function.</param>
		/// <param name = "valueNew">New value of the objective function.</param>
		/// <param name = "iterationCount">Number of computed iterations.</param>
		/// <param name = "parametersCurrent">Current estimated model parameters.</param>
		/// <param name = "parametersNew">New estimated model parameters.</param>
		/// <param name = "solverOptions">Least squares solver options.</param>
		/// <returns>The solver should terminate computation in current iteration.</returns>
		private bool ShouldTerminate(double valueCurrent, double valueNew, int iterationCount, Vector<double> parametersCurrent, Vector<double> parametersNew)
		{
			return (
					   Math.Abs(valueNew - valueCurrent) <= minimumDeltaValue ||
					   parametersNew.Subtract(parametersCurrent).Norm(2.0) <= minimumDeltaParameters ||
					   iterationCount >= maximumIterations);
		}
	}
}
