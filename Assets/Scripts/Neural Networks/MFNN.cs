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
	private 	float[][]			neuron_gradients;
	private 	float[][]			biases_delta;
	private		float[][][]			weights_delta;


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
		neuron_gradients = new float[neurons.Length][];
		for (int i = 0; i < neurons.Length; i++)
		{
			Debug.Assert(neurons_per_layer[i] > 0, "There must be atleast 1 node per layer.");
			neurons[i] = new float[neurons_per_layer[i]];
			neuron_gradients[i] = new float[neurons_per_layer[i]];
			for(int j = 0; j < neurons[i].Length; j++)
			{
				neurons[i][j] = Random.Range(-1.0f, 1.0f);
				neuron_gradients[i][j] = Random.Range(-1.0f, 1.0f);
			}
		}
		//Setup biases
		biases = new float[neurons_per_layer.Length - 1][];
		biases_delta = new float[neurons_per_layer.Length - 1][];
		for (int i = 0; i < biases.Length; i++)
		{
			biases[i] = new float[neurons_per_layer[i + 1]];
			biases_delta[i] = new float[neurons_per_layer[i + 1]];
			for(int j = 0; j < biases[i].Length; j++)
			{
				biases[i][j] = Random.Range(-1.0f, 1.0f);
				biases_delta[i][j] = Random.Range(-1.0f, 1.0f);
				weights_length++;
			}
		}
		//Setup synapsis
		synapsis = new float[neurons_per_layer.Length - 1][][];
		weights_delta = new float[neurons_per_layer.Length - 1][][];
		for (int i = 0; i < synapsis.Length; i++)
		{
			synapsis[i] = new float[neurons_per_layer[i + 1]][];
			weights_delta[i] = new float[neurons_per_layer[i + 1]][];
			for (int j = 0; j < synapsis[i].Length; j++)
			{
				synapsis[i][j] = new float[neurons_per_layer[i]];
				weights_delta[i][j] = new float[neurons_per_layer[i]];
				for (int k = 0; k < synapsis[i][j].Length; k++)
				{
					synapsis[i][j][k] = Random.Range(-1.0f, 1.0f);
					weights_delta[i][j][k] = Random.Range(-1.0f, 1.0f);
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
		Debug.Assert(output_error.Length == neurons[neurons.Length - 1].Length);
	
		//Compute gradiant for each neuron from output layer to input layer.
		for (int i = neurons.Length - 1; i > 0; i--)
		{
			for (int j = 0; j < neurons[i].Length; j++)
			{
				//Apply the derivative of the activation function
				float derivative   = Activation.ReLUDerivative(neurons[i][j]);
				float sum          = 0.0f;
				if (i == neurons.Length - 1) //If Output layer
				{
					//If it is the output layer the derivative error is desired - actual output
					sum = output_error[j];
				}
				else //Else other layer
				{
					//Go through each synapse going out from this neuron
					for (int k = 0; k < neurons[i + 1].Length; k++)
					{
						//Multiply the synapse weight by the node gradient to which this synapse is going to.
						float x = neuron_gradients[i + 1][k] * synapsis[i][k][j];
						//The resulting value is added to a sum
						sum += x;
					}
				}
				//The gradient of the node is then derivative multiplied by the sum.
				neuron_gradients[i][j] = derivative * sum;
			}
		}

		//Update Weights for each synapse
		for (int i = 0; i < neurons.Length - 1; i++)
		{
			for (int j = 0; j < neurons[i + 1].Length; j++)
			{
				for (int k = 0; k < neurons[i].Length; k++)
				{
					//get delta by multiply learning rate (free parameter) with synapse gradient
					//and then multiplying that by neuron value.
					float delta		= learning_rate * neurons[i + 1][j] * neurons[i][k]; 
					//Add the delta to the current synapse weight
					float weight	= synapsis[i][j][k];
					weight		   += delta;
					//add momentum multiplied by previous delta to the current weight.
					weight		   += momentum * weights_delta[i][j][k];
					//multiply weight decay by current weight and then subtract the result from
					//current weight
					weight		   -= weight_decay * weight;
					//Update all values
					synapsis[i][j][k] = weight;
					weights_delta[i][j][k] = delta;
				}
			}
		}

		//Update Biases
		for (int i = 1; i < neurons.Length - 1; i++)
		{
			for (int j = 0; j < neurons[i + 1].Length; j++)
			{
				float delta    	= learning_rate * neuron_gradients[i + 1][j] * 1.0f;
				float bias     	= biases[i][j];
				bias           += delta;
				bias           += momentum * biases_delta[i][j];
				bias           -= (weight_decay * bias);

				biases[i][j] 		= bias;
				biases_delta[i][j]	= delta;
			}
		}
	}
}