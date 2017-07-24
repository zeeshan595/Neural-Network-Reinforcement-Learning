using UnityEngine;

public class MFNN : BaseNetwork
{
	private 	float[][]			neurons;
	private 	float[][]			biases;
	private 	float[][][]			synapsis;
	private		ActivationType[]	activation_per_layer;
	private		int					weights_length				= 0;
	private		float				network_score				= float.MinValue;

	public override float GetNetworkScore()
	{
		return network_score;
	}
	public override void SetNetworkScore(float s)
	{
		network_score = s;
	}

	public override int GetWeightsLength()
	{
		return weights_length;
	}

	public MFNN(int[] neurons_per_layer, ActivationType[] activation_per_layer)
	{
		//Error checking
		Debug.Assert(neurons_per_layer.Length > 0, "There must be atleast 1 neuron in each layer.");
		Debug.Assert(activation_per_layer.Length == neurons_per_layer.Length, "Activation array does not match neurons array.");
		for (int i = 0; i < neurons_per_layer.Length; i++)
			Debug.Assert(neurons_per_layer[i] > 0, "There must be atleast 1 node per layer.");

		//Setup variables
		this.activation_per_layer = activation_per_layer;
		weights_length = 0;

		//Setup neurons
		neurons = new float[neurons_per_layer.Length][];
		for (int i = 0; i < neurons.Length; i++)
		{
			Debug.Assert(neurons_per_layer[i] > 0, "There must be atleast 1 node per layer.");
			neurons[i] = new float[neurons_per_layer[i]];
			for(int j = 0; j < neurons[i].Length; j++)
			{
				neurons[i][j] = Random.Range(-1.0f, 1.0f);
			}
		}
		//Setup biases
		biases = new float[neurons_per_layer.Length - 1][];
		for (int i = 0; i < biases.Length; i++)
		{
			biases[i] = new float[neurons_per_layer[i + 1]];
			for(int j = 0; j < biases[i].Length; j++)
			{
				biases[i][j] = Random.Range(-1.0f, 1.0f);
				weights_length++;
			}
		}
		//Setup synapsis
		synapsis = new float[neurons_per_layer.Length - 1][][];
		for (int i = 0; i < synapsis.Length; i++)
		{
			synapsis[i] = new float[neurons_per_layer[i + 1]][];
			for (int j = 0; j < synapsis[i].Length; j++)
			{
				synapsis[i][j] = new float[neurons_per_layer[i]];
				for (int k = 0; k < synapsis[i][j].Length; k++)
				{
					synapsis[i][j][k] = Random.Range(-1.0f, 1.0f);
					weights_length++;
				}
			}
		}
	}

	public override float[] Compute(float[] inputs)
	{
		//Error checking
		Debug.Assert(inputs.Length == neurons[0].Length, "Input does not match the network's input layer.");

		System.Array.ConstrainedCopy(inputs, 0, neurons[0], 0, inputs.Length);

		for (int i = 0; i < neurons.Length - 1; i++)
		{
			for (int j = 0; j < neurons[i + 1].Length; j++)
			{
				//Apply weights
				float sum = 0.0f;
				for (int k = 0; k < neurons[i].Length; k++)
				{
					sum += (neurons[i][k] * synapsis[i][j][k]);
				}
				//Apply bias
				sum += biases[i][j];

				//Apply activation function
				switch(activation_per_layer[i + 1])
				{
					case ActivationType.LOGISTIC_SIGMOID:
						neurons[i + 1][j] = Activation.LogisticSigmoid(sum);
						break;
					case ActivationType.HYPERBOLIC_TANGENT:
						neurons[i + 1][j] = Activation.HyperbolicTangent(sum);
						break;
					case ActivationType.HEAVISIDESTEP:
						neurons[i + 1][j] = Activation.HeavisideStep(sum);
						break;
					case ActivationType.ReLU:
						neurons[i + 1][j] = Activation.ReLU(sum);
						break;
					default:
						neurons[i + 1][j] = sum;
						break;
				}
			}
			if (activation_per_layer[i + 1] == ActivationType.SOFTMAX)
				neurons[i + 1] = Activation.Softmax(neurons[i + 1]);
		}

		return neurons[neurons.Length - 1];
	}

	public override float[] GetWeightsData()
	{
		int Z = 0;
		float[] result = new float[GetWeightsLength()];
		for (int i = 0; i < biases.Length; i++)
		{
			for(int j = 0; j < biases[i].Length; j++)
			{
				result[Z] = biases[i][j];
				Z++;
			}
		}
		for (int i = 0; i < synapsis.Length; i++)
		{
			for (int j = 0; j < synapsis[i].Length; j++)
			{
				for (int k = 0; k < synapsis[i][j].Length; k++)
				{
					result[Z] = synapsis[i][j][k];
					Z++;
				}
			}
		}
		return result;
	}

	public override void SetWeightsData(float[] weights)
	{
		Debug.Assert(GetWeightsLength() == weights.Length, "Weight data does not match the network.");

		int Z = 0;
		for (int i = 0; i < biases.Length; i++)
		{
			for(int j = 0; j < biases[i].Length; j++)
			{
				biases[i][j] = weights[Z];
				Z++;
			}
		}
		for (int i = 0; i < synapsis.Length; i++)
		{
			for (int j = 0; j < synapsis[i].Length; j++)
			{
				for (int k = 0; k < synapsis[i][j].Length; k++)
				{
					synapsis[i][j][k] = weights[Z];
					Z++;
				}
			}
		}
	}
}