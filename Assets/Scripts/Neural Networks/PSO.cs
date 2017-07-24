using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSO : BaseTechnique
{
	public struct Particle{
		public float[]				velocity;
		public float[]				position;
		public float[]				best_position;
		public float				score;
		public float				best_score;
		public BaseNetwork			network;
	};

	private 	Particle[]			particles;
	private 	float				MIN								= -10.0f;
	private 	float				MAX								= +10.0f;
	private 	float				inertia_weight					= 0.729f;
    private 	float				cognitive_weight				= 1.49445f;
    private 	float				social_weight					= 1.49445f;
	private		int					weights_length					= 0;
	private 	float[]				global_best_position;
	private		float				global_best_score				= 0;

	public PSO(BaseNetwork[] networks)
	{
		//Error Checking
		Debug.Assert(networks.Length > 0, "There must be atleast 1 particle");

		//Setup Particles
		global_best_score = float.MinValue;
		particles = new Particle[networks.Length];
		weights_length = networks[0].GetWeightsLength();
		for (uint i = 0; i < networks.Length; i++)
		{
			particles[i] = new Particle();
			particles[i].velocity = new float[weights_length];
			for (uint j = 0; j < weights_length; j++)
			{
				particles[i].velocity[j] = (float)GetRandomNumber(-1.0f, 1.0f);
			}
			particles[i].position = networks[i].GetWeightsData();
			particles[i].best_position = particles[i].position;
			particles[i].score = networks[i].GetNetworkScore();
			particles[i].best_score = particles[i].score;
			particles[i].network = networks[i];

			if (i == 0 || networks[i].GetNetworkScore() > global_best_score)
			{
				global_best_position	= particles[i].position;
				global_best_score 		= particles[i].score;
			}
		}
	}

	public override void ComputeEpoch()
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

	public override void UpdateWeights()
	{
		for (uint i = 0; i < particles.Length; i++)
		{
			float[] new_velocity = new float[weights_length];
			float[] new_position = new float[weights_length];
			for (uint j = 0; j < weights_length; j++)
			{
				//Update Particle Velocity
				float r1 = (float)GetRandomNumber(0.0f, 1.0f);
				float r2 = (float)GetRandomNumber(0.0f, 1.0f);
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

	public override float[] GetBestWeights()
	{
		return global_best_position;
	}

	public override float GetBestScore()
	{
		return global_best_score;
	}

	private double GetRandomNumber(double minimum, double maximum)
	{ 
		System.Random random = new System.Random();
		return random.NextDouble() * (maximum - minimum) + minimum;
	}
}