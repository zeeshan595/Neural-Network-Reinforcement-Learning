using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSO
{
	private 	Particle[]			particles;
	private 	double				MIN								= -10.0;
	private 	double				MAX								= +10.0;
	private 	double				inertia_weight					= 0.729;
    private 	double				cognitive_weight				= 1.49445;
    private 	double				social_weight					= 1.49445;
	private		uint				weights_length					= 0;
	private 	double[]			global_best_position;
	private		double				global_best_score				= 0;

	public PSO(BaseNetwork[] networks)
	{
		//Error Checking
		Debug.Assert(networks.Length > 0, "There must be atleast 1 particle");

		//Setup Particles
		particles = new Particle[networks.Length];
		weights_length = networks[0].GetWeightsLength();
		for (uint i = 0; i < networks.Length; i++)
		{
			particles[i] = new Particle();
			particles[i].velocity = new double[weights_length];
			for (uint j = 0; j < weights_length; j++)
			{
				particles[i].velocity[j] = Random.Range(0.1f * (float)MIN, 0.1f * (float)MAX);
			}
			particles[i].position = networks[i].GetWeightsData();
			particles[i].best_position = particles[i].position;
			particles[i].score = networks[i].GetNetworkScore();
			particles[i].best_score = particles[i].score;
			particles[i].network = networks[i];
		}
		global_best_position	= particles[0].position;
		global_best_score 		= particles[0].score;
	}

	public void ComputeEpoch()
	{
		for (uint i = 0; i < particles.Length; i++)
		{
			particles[i].score = particles[i].network.GetNetworkScore();

			//Compare current score with best particle score
			if (particles[i].score > particles[i].best_score)
			{
				particles[i].best_score = particles[i].score;
				particles[i].best_position = particles[i].position;
			}

			//Compare current score with global best score
			if (particles[i].score > global_best_score)
			{
				global_best_score = particles[i].best_score;
				global_best_position = particles[i].best_position;
			}
		}
	}

	public void UpdateWeights()
	{
		for (uint i = 0; i < particles.Length; i++)
		{
			double[] new_velocity = new double[weights_length];
			double[] new_position = new double[weights_length];
			for (uint j = 0; j < weights_length; j++)
			{
				//Update Particle Velocity
				double r1 = CommonFunctions.GetRandomNumber(0.0f, 1.0f);
				double r2 = CommonFunctions.GetRandomNumber(0.0f, 1.0f);
				new_velocity[j] 		=	((inertia_weight  	* particles[i].velocity[j]) +
											(cognitive_weight 	* r1 * (particles[i].best_position[j] 	- particles[i].position[j])) +
								    		(social_weight 		* r2 * (global_best_position[j] 		- particles[i].position[j])));
			}
			particles[i].velocity = new_velocity;

			for (uint j = 0; j < weights_length; j++)
			{
				//Update Particle Positions
				new_position[j] = particles[i].position[j] + new_velocity[j];
                //Make sure particle does not go out of bounds.
                //using MIN and MAX variables
                if (new_position[j] < MIN)
                    new_position[j] = MIN;
                else if (new_position[j] > MAX)
                    new_position[j] = MAX;
			}
			particles[i].position = new_position;
			
			particles[i].network.SetWeightsData(particles[i].position);				
		}
	}

	public double[] GetBestWeights()
	{
		return global_best_position;
	}

	public double GetBestScore()
	{
		return global_best_score;
	}
}