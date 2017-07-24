using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
	private int amount = 20;
	private MFNN[] nn;
	private GA ga;

	private void Start () 
	{
		nn = new MFNN[amount];
		for (int i = 0; i < amount; i++)
		{
			nn[i] = new MFNN(new int[]{ 4, 7, 3 }, new ActivationType[]{
				ActivationType.NONE,
				ActivationType.HYPERBOLIC_TANGENT,
				ActivationType.LOGISTIC_SIGMOID
			});
		}
		ga = new GA(nn);

		
	}
	
	private void Update()
	{
		ga.UpdateWeights();
		MSR(Iris.dataset);
		ga.ComputeEpoch();
	}

	private void MSR(float[][] train_data)
	{
		for (int i = 0; i < amount; i++)
		{
			float sum = 0.0f;
			for (int j = 0; j < train_data.Length; j++)
			{
				float[] x_values = new float[4];
				float[] t_values = new float[3];

				System.Array.ConstrainedCopy(train_data[j], 0, x_values, 0, x_values.Length);
				System.Array.ConstrainedCopy(train_data[j], x_values.Length, t_values, 0, t_values.Length);

				float[] y_values = nn[i].Compute(x_values);
				for (int k = 0; k < 3; k++)
				{
					sum -= (y_values[k] - t_values[k]) * (y_values[k] - t_values[k]);
				}
			}
			nn[i].SetNetworkScore(sum);
		}
	}
}