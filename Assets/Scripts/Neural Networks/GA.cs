using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GA : BaseTechnique
{

	private 	BaseNetwork[]       agents;
	private		int					weights_length;
    private     int[]               sequence;
    private     float               best_global_score;
    private     float[]             best_global_weights;

	public GA(BaseNetwork[] networks)
	{
		//Error checking
		Debug.Assert(networks.Length > 0, "There must be atleast 1 particle");

		//Setup agents
        agents      = networks;
        sequence    = new int[networks.Length];
        for (int i = 0; i < sequence.Length; i++)
            sequence[i] = i;

		weights_length      = networks[0].GetWeightsLength();
        best_global_score   = float.MinValue;
	}

	public override void ComputeEpoch()
	{
        //Re-arrange array with best error at front
        System.Array.Sort(agents, delegate(BaseNetwork a, BaseNetwork b){
            return b.GetNetworkScore().CompareTo(a.GetNetworkScore());
        });

        //Check & store best weights
        if (agents[sequence[0]].GetNetworkScore() > best_global_score)
        {
            best_global_score   = agents[sequence[0]].GetNetworkScore();
            best_global_weights = agents[sequence[0]].GetWeightsData();
        }
	}

	public override void UpdateWeights()
	{
        //Kill half of the weak population
        for (int i = (agents.Length / 2); i < agents.Length; i++)
        {
            agents[sequence[i]].SetWeightsData(agents[sequence[i % (agents.Length / 2)]].GetWeightsData());
        }

        //Mate everyone
        System.Random random = new System.Random();
        for (int i = 0; i < agents.Length / 2; i++)
        {
            float[] w1 = new float[weights_length];
            float[] w2 = new float[weights_length];
            for (int j = 0; j < weights_length; j++)
            {
                if (random.Next() % 2 == 0)
                {
                    w1[j] = agents[sequence[(i * 2) + 0]].GetWeightsData()[j];
                    w2[j] = agents[sequence[(i * 2) + 1]].GetWeightsData()[j];
                }
                else
                {
                    w1[j] = agents[sequence[(i * 2) + 1]].GetWeightsData()[j];
                    w2[j] = agents[sequence[(i * 2) + 0]].GetWeightsData()[j];
                }
            }

            agents[sequence[(i * 2) + 0]].SetWeightsData(w1);
            agents[sequence[(i * 2) + 1]].SetWeightsData(w2);
        }

        //Mutate randomly
        for (int i = 0; i < agents.Length; i++)
        {
            float[] w1 = agents[sequence[i]].GetWeightsData();
            for (int j = 0; j < weights_length; j++)
            {
                float random_nubmer = (float)GetRandomNumber(-10.0, 10.0);
                w1[j] = w1[j] + (random_nubmer * 1.0f);
            }
            agents[sequence[i]].SetWeightsData(w1);
        }
	}

	public override float[] GetBestWeights()
	{
		return best_global_weights;
	}

	public override float GetBestScore()
	{
		return best_global_score;
	}

    
	private double GetRandomNumber(double minimum, double maximum)
	{ 
		System.Random random = new System.Random();
		return random.NextDouble() * (maximum - minimum) + minimum;
	}
}