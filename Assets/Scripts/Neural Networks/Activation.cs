using System;

public enum ActivationType
{
	NONE,
	LOGISTIC_SIGMOID,
	HYPERBOLIC_TANGENT,
	HEAVISIDESTEP,
	SOFTMAX
};

public class Activation
{
	public static double LogisticSigmoid(double x)
	{
		return 1.0 / (1.0 + Math.Exp(-x));
	}

	public static double HyperbolicTangent(double x)
	{
		return Math.Tanh(x);
	}

	public static double HeavisideStep(double x)
	{
		if (x < 0)
			return 0;
		else
			return 1;
	}

	public static double[] Softmax(double[] x)
	{
		//Get max value
		double max = x[0];
		for(uint i = 1; i < x.Length; i++)
		{
			if (max < x[i])
				max = x[i];
		}

		//Determine scaling factor -- sum of exp (each val - max)
		double scale = 0.0;
		for (uint i = 0; i < x.Length; i++)
		{
			scale += Math.Exp(x[i] - max);
		}

		//Compute softmax value
		double[] result = new double[x.Length];
		for (uint i = 0; i < x.Length; i++)
		{
			result[i] = Math.Exp(x[i] - max) / scale;
		}
		return result;
	}
}