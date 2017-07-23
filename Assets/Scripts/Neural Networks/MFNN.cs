using System;
using UnityEngine;

public class MFNN : BaseNetwork
{
	private		float						network_score;
	private		double[][]					neurons;
	private		double[][]					biases;
	private 	double[][][]				synapsis;
	private 	uint[] 						neurons_per_layer;
	private 	ActivationType[] 			activation_per_layer;
	private 	uint						weights_length;
	public MFNN(uint[] neurons_per_layer, ActivationType[] activation_per_layer)
	{
		//Error Checking
		Debug.Assert(neurons_per_layer.Length > 1, "Layer size must be greater than one.");
		Debug.Assert(neurons_per_layer.Length == activation_per_layer.Length, "Neurons per layer and activation per layer do not match.");
		for (uint i = 0; i < neurons_per_layer.Length; i++)
			Debug.Assert(neurons_per_layer[i] > 0, "A layer cannot contain 0 or less neurons.");

		//Save Results
		this.neurons_per_layer 		= neurons_per_layer;
		this.activation_per_layer 	= activation_per_layer;
	
		//Setup Neurons
		neurons = new double[neurons_per_layer.Length][];
		for (uint i = 0; i < neurons.Length; i++)
		{
			neurons[i] 	= new double[neurons_per_layer[i]];
			for (uint j = 0; j < neurons_per_layer[i]; j++)
			{
				neurons[i][j] 	= CommonFunctions.GetRandomNumber(-1.0f, 1.0f);
			}
		}

		weights_length = 0;
		//Setup Biases
		biases = new double[neurons_per_layer.Length - 1][];
		for (uint i = 0; i < biases.Length; i++)
		{
			biases[i] 	= new double[neurons_per_layer[i + 1]];
			for (uint j = 0; j < neurons_per_layer[i + 1]; j++)
			{
				biases[i][j] 	= CommonFunctions.GetRandomNumber(-1.0f, 1.0f);
				weights_length++;
			}
		}

		//Setup Synapsis
		synapsis = new double[neurons_per_layer.Length - 1][][];
		for (uint i = 0; i < synapsis.Length; i++)
		{
			synapsis[i] = new double[neurons_per_layer[i + 1]][];
			for (uint j = 0; j < neurons_per_layer[i + 1]; j++)
			{
				synapsis[i][j] = new double[neurons_per_layer[i]];
				for (uint k = 0; k < neurons_per_layer[i]; k++)
				{
					synapsis[i][j][k] = CommonFunctions.GetRandomNumber(-1.0f, 1.0f);
					weights_length++;
				}
			}
		}
	}

	public override double[] Compute(double[] inputs)
	{
		//Error Checking
		Debug.Assert(inputs.Length == neurons_per_layer[0], "Input does not match input layer nodes");
		//Copy Input Data
		for (uint i = 0; i < neurons_per_layer[0]; i++)
		{
			neurons[0][i] = inputs[i];
		}

		for (uint i = 0; i < neurons_per_layer.Length - 1; i++)
		{
			
			for (uint j = 0; j < neurons_per_layer[i + 1]; j++)
			{
				//Multiply weights and prev. neurons
				double sum = 0.0;
				for (uint k = 0; k < neurons_per_layer[i]; k++)
				{
					sum += neurons[i][k] * synapsis[i][j][k];
				}
				//Apply bias
				sum += biases[i][j];

 
				//Apply activation
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
					default:
						neurons[i + 1][j] = sum;
						break;
				}
			}
			//Softmax is done in parallel
			if (activation_per_layer[i + 1] == ActivationType.SOFTMAX)
			{
				neurons[i + 1] = Activation.Softmax(neurons[i + 1]);
				Debug.Log("SOFTMAX");
			}
		}

		//Return output layer
		return neurons[neurons.Length - 1];
	}

	public override void SetNetworkScore(float score)
	{
		network_score = score;
	}

	public override float GetNetworkScore()
	{
		return network_score;
	}

	public override uint GetWeightsLength()
	{
		return weights_length;
	}

	public override double[] GetWeightsData()
	{
		uint Z = 0;
		double[] result = new double[weights_length];

		//Get Biases
		for (uint i = 0; i < biases.Length; i++)
		{
			for (uint j = 0; j < neurons_per_layer[i + 1]; j++)
			{
				result[Z] = biases[i][j];
				Z++;
			}
		}

		//Get Synapsis
		for (uint i = 0; i < synapsis.Length; i++)
		{
			for (uint j = 0; j < neurons_per_layer[i + 1]; j++)
			{
				for (uint k = 0; k < neurons_per_layer[i]; k++)
				{
					result[Z] = synapsis[i][j][k];
					Z++;
				}
			}
		}

		return result;
	}

	public override void SetWeightsData(double[] weights_data)
	{
		//Error Checking
		Debug.Assert(weights_data.Length == weights_length, "Weights data length does not match the network");
		
		uint Z = 0;
		//Set Biases
		for (uint i = 0; i < biases.Length; i++)
		{
			for (uint j = 0; j < neurons_per_layer[i + 1]; j++)
			{
				biases[i][j] = weights_data[Z];
				Z++;
			}
		}

		//Set Synapsis
		for (uint i = 0; i < synapsis.Length; i++)
		{
			for (uint j = 0; j < neurons_per_layer[i + 1]; j++)
			{
				for (uint k = 0; k < neurons_per_layer[i]; k++)
				{
					synapsis[i][j][k] = weights_data[Z];
					Z++;
				}
			}
		}
	}
}