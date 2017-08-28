using UnityEngine;
[System.Serializable]
public class MFNN : BaseNetwork
{
	private 	float[][]			neurons;
	private 	float[][]			biases;
	private 	float[][][]			synapsis;
	private		ActivationType[]	activation_per_layer;
	private		int					weights_length				= 0;
	private		float				network_score				= float.MinValue;

	/* ==================== BACK PROPAGATION ==================== */
	private 	float[][]			neuron_delta_error;
	private		float[][][]			synapsis_delta_error;
	private 	float[][]			biases_delta_error;

	private 	float[][][]			prev_synapsis_delta_error;
	private 	float[][]			prev_biases_delta_error;

	public int GetInputSize()
	{
		return neurons[0].Length;
	}

	public int GetOutputSize()
	{
		return neurons[neurons.Length - 1].Length;
	}

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
		neuron_delta_error = new float[neurons.Length][];
		for (int i = 0; i < neurons.Length; i++)
		{
			Debug.Assert(neurons_per_layer[i] > 0, "There must be atleast 1 node per layer.");
			neurons[i] = new float[neurons_per_layer[i]];
			neuron_delta_error[i] = new float[neurons_per_layer[i]];
			for(int j = 0; j < neurons[i].Length; j++)
			{
				neurons[i][j] = Random.Range(-1.0f, 1.0f);
				neuron_delta_error[i][j] = Random.Range(-1.0f, 1.0f);
			}
		}
		//Setup biases
		biases = new float[neurons_per_layer.Length - 1][];
		biases_delta_error = new float[neurons_per_layer.Length - 1][];
		prev_biases_delta_error = new float[neurons_per_layer.Length - 1][];
		for (int i = 0; i < biases.Length; i++)
		{
			biases[i] = new float[neurons_per_layer[i + 1]];
			biases_delta_error[i] = new float[neurons_per_layer[i + 1]];
			prev_biases_delta_error[i] = new float[neurons_per_layer[i + 1]];
			for(int j = 0; j < biases[i].Length; j++)
			{
				biases[i][j] = Random.Range(-1.0f, 1.0f);
				biases_delta_error[i][j] = Random.Range(-1.0f, 1.0f);
				prev_biases_delta_error[i][j] = Random.Range(-1.0f, 1.0f);
				weights_length++;
			}
		}
		//Setup synapsis
		synapsis = new float[neurons_per_layer.Length - 1][][];
		synapsis_delta_error = new float[neurons_per_layer.Length - 1][][];
		prev_synapsis_delta_error = new float[neurons_per_layer.Length - 1][][];
		for (int i = 0; i < synapsis.Length; i++)
		{
			synapsis[i] = new float[neurons_per_layer[i + 1]][];
			synapsis_delta_error[i] = new float[neurons_per_layer[i + 1]][];
			prev_synapsis_delta_error[i] = new float[neurons_per_layer[i + 1]][];
			for (int j = 0; j < synapsis[i].Length; j++)
			{
				synapsis[i][j] = new float[neurons_per_layer[i]];
				synapsis_delta_error[i][j] = new float[neurons_per_layer[i]];
				prev_synapsis_delta_error[i][j] = new float[neurons_per_layer[i]];
				for (int k = 0; k < synapsis[i][j].Length; k++)
				{
					synapsis[i][j][k] = Random.Range(-1.0f, 1.0f);
					synapsis_delta_error[i][j][k] = Random.Range(-1.0f, 1.0f);
					prev_synapsis_delta_error[i][j][k] = Random.Range(-1.0f, 1.0f);
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

	public override void RandomizeWeights(int seed)
	{
		System.Random rnd = new System.Random(seed);
		float[] weights = new float[weights_length];
		for (int i = 0; i < weights_length; i++)
		{
			float maximum = 1.0f;
			float minimum = -1.0f;
			float random_number = (float)rnd.NextDouble() * (maximum - minimum) + minimum;
			weights[i] = random_number;
		}
		SetWeightsData(weights);
	}

	/* ==================== BACK PROPAGATION ==================== */

	public void UpdateWeights(float[] output_error, float learning_rate, float weight_decay, float momentum)
	{
		//Error checking
		Debug.Assert(output_error.Length == neurons[neurons.Length - 1].Length);
	
		//Get neurons derivative error
		for (int i = neurons.Length - 1; i >= 0; i--)
		{
			//If output layer
			if (i + 1 == neurons.Length)
			{
				neuron_delta_error[i] = output_error;
			}
			else
			{
				for (int k = 0; k < neurons[i].Length; k++)
				{
					float sum = 0.0f;
					for (int j = 0; j < neurons[i + 1].Length; j++)
					{
						//Get activation derivative error
						float A = GetActivationDerivative(neurons[i + 1][j], activation_per_layer[i + 1]);
						//Get derivative error per synapse
						sum += A * synapsis[i][j][k] * neuron_delta_error[i + 1][j];
					}
					//Sum derivative error to compute neuron's derivative error
					neuron_delta_error[i][k] = sum;
				}
			}
		}

		//Get synapsis & bias derivative error
		//and update synapsis & biases
		for (int i = 0; i < neurons.Length - 1; i++)
		{
			for (int j = 0; j < neurons[i + 1].Length; j++)
			{
				//Get activation derivative error
				float A = GetActivationDerivative(neurons[i + 1][j], activation_per_layer[i + 1]);

				//Synapsis
				for (int k = 0; k < neurons[i].Length; k++)
				{
					//Compute synapsis derivative error
					synapsis_delta_error[i][j][k] = neuron_delta_error[i + 1][j] * A * neurons[i][k];
					//Update synapsis
					synapsis[i][j][k] += learning_rate * synapsis_delta_error[i][j][k];
					//Apply Momentum
					synapsis[i][j][k] += momentum * prev_synapsis_delta_error[i][j][k];
					//Apply decay effect
					synapsis[i][j][k] -= weight_decay * synapsis[i][j][k];
					
					//Store previous synapse derivative error
					prev_synapsis_delta_error[i][j][k] = synapsis_delta_error[i][j][k];
				}

				//Compute bias derivative error
				biases_delta_error[i][j] = neuron_delta_error[i + 1][j] * A * 1.0f;
				//Update bias
				biases[i][j] += learning_rate * biases_delta_error[i][j];
				//Apply Momentum
				biases[i][j] += momentum * prev_biases_delta_error[i][j];
				//Apply decay effect
				biases[i][j] -= weight_decay * biases[i][j];

				//Store previous bias derivative error
				prev_biases_delta_error[i][j] = biases_delta_error[i][j];
			}
		}
	}

	private float GetActivationDerivative(float x, ActivationType type)
	{
		switch(type)
		{
			case ActivationType.SOFTMAX:
			case ActivationType.LOGISTIC_SIGMOID:
				return Activation.LogisticSigmoidD(x);
			case ActivationType.HYPERBOLIC_TANGENT:
				return Activation.HyperbolicTangentD(x);
			case ActivationType.HEAVISIDESTEP:
				Debug.LogError("Impossible to compute derivative of heavisidestep.");
				return x;
			case ActivationType.ReLU:
				return Activation.ReLUD(x);
			default:
				return x;
		}
	}
}