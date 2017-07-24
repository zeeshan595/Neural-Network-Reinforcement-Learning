using System;
using UnityEngine;

public enum ActivationType
{
	NONE					= 0,
	LOGISTIC_SIGMOID		= 1,
	HYPERBOLIC_TANGENT		= 2,
	HEAVISIDESTEP			= 3,
	SOFTMAX					= 4,
	ReLU					= 5
};

public class Activation
{
	public static float LogisticSigmoid(float x)
	{
		return 1.0f / (1.0f + Mathf.Exp(-x));
	}

	public static float HyperbolicTangent(float x)
	{
		return (float)Math.Tanh(x);
	}

	public static float HeavisideStep(float x)
	{
		if (x < 0)
			return 0;
		else
			return 1;
	}

	public static float ReLU(float x)
	{
		if (x < 0)
			return 0;
		else
			return x;
	}

	public static float[] Softmax(float[] x)
	{
		//Get max value
		float max = x[0];
		for(uint i = 1; i < x.Length; i++)
		{
			if (max < x[i])
				max = x[i];
		}

		//Determine scaling factor -- sum of exp (each val - max)
		float scale = 0.0f;
		for (uint i = 0; i < x.Length; i++)
		{
			scale += Mathf.Exp(x[i] - max);
		}

		//Compute softmax value
		float[] result = new float[x.Length];
		for (uint i = 0; i < x.Length; i++)
		{
			result[i] = Mathf.Exp(x[i] - max) / scale;
		}
		return result;
	}
}