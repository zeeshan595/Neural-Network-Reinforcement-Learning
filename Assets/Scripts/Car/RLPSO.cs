using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine;

public class RLPSO : MonoBehaviour
{
	public GameObject car_spawner;
    public Text text_log;
	public int max_particles = 40;
	public int next_particle_wait = 300;
	public float reward_decay = 0.9f;

	[System.NonSerialized]
	public bool abort_learning = true;
    
    private string[] actions_name = new string[]{
        "No Action", "Break", "Turn Left", "Turn Right", "Gas", "Turn Left & Gas", "Turn Right & Gas"
    };
    private int[][] actions = new int[][]{
        new int[]{ 0, 0 }, //No action
        new int[]{-1, 0 }, //Break
        new int[]{ 0,-1 }, //Turn Left
        new int[]{ 0, 1 }, //Turn Right
        new int[]{ 1, 0 }, //Accelerate
        new int[]{ 1,-1 }, //Turn Left & Accelerate
        new int[]{ 1, 1 }, //Turn Right & Accelerate
    };

	private 	PSO 			particle_swarm;
	private 	MFNN			network_target;
	private		MFNN[]			particles;
	private 	int 			working_particle;
	private 	CarCamera		car_camera;
	private 	CarController 	car_controller;
	private 	Rigidbody		car_body;
	private 	int 			action_index;
	private 	float[]			last_q_values;
	private 	int 			current_step;
	private 	int				reset_step;
	private 	float 			current_reward = 0;
	private 	int				particle_step;

	private MFNN CreateMFNN()
	{
		return new MFNN(new int[]{ 5, 20, actions.Length },
		new ActivationType[]{
			ActivationType.NONE,
			ActivationType.ReLU,
			ActivationType.ReLU
		});
	}

	public void StopTraining()
	{
		abort_learning = true;
	}

	public void StartTraining()
	{
		last_q_values = new float[actions.Length];
		abort_learning = false;
		current_step = 0;
		current_reward = 0;
		particle_step = 0;

		//Setup car components
		car_camera = GetComponent<CarCamera>();
		car_controller = GetComponent<CarController>();
		car_body = transform.GetChild(0).GetComponent<Rigidbody>();		

		//Setup PSO
		working_particle = 0;
		particles = new MFNN[max_particles];
		for (int i = 0; i < particles.Length; i++)
			particles[i] = CreateMFNN();
		particle_swarm = new PSO(particles);
		network_target = CreateMFNN();

		StartCoroutine(TakeStep());
	}

	private void Update()
	{
		if (!abort_learning)
		{
			if (actions[action_index][0] == -1)
            {
                car_controller.Accelerate(0);
                car_controller.Brake();
            }
            else
            {
                car_controller.Accelerate(actions[action_index][0]);
            }
            car_controller.Steer(actions[action_index][1]);
		
			if (text_log != null)
			{
				string full_log = "";
				full_log += "Working Particle: " + working_particle + "\n";
				full_log += "Current Reward: " + current_reward + "\n";
				full_log += "Best Score: " + particle_swarm.GetBestScore() + "\n";
				full_log += "Actions Q Value: \n";
				for (int i = 0; i < actions.Length; i++)
				{
					full_log += "       " + actions_name[i] + ": " + last_q_values[i] + "\n";
				}
				text_log.text   = full_log;
			}
		}
	}

	private IEnumerator TakeStep()
	{
		//Get current car state
		float[] current_state = car_camera.GetRays();

		//Get action from current state
		float[] q_values = particles[working_particle].Compute(current_state);
		last_q_values = q_values;
		action_index = SelectAction(q_values);

		//Wait for action to complete
		yield return new WaitForSeconds(0.1f);

		//Get next state
		float[] next_state = car_camera.GetRays();
		//Get max `a` of `q_target`
		float[] q_target = network_target.Compute(next_state);
		int max_q_target = 0;
		for (int i = 1; i < max_q_target; i++)
			if (q_target[max_q_target] < q_target[i])
				max_q_target = i;
		//Get rward for action
		float velocity = car_body.gameObject.transform.InverseTransformDirection(car_body.velocity).z;
        current_reward = velocity + reward_decay * q_target[max_q_target];
		particles[working_particle].SetNetworkScore(current_reward);

		//Reset car if stuck after 100 steps
		if (car_body.velocity.magnitude < 0.3f && current_step - reset_step > 100)
		{
			reset_step = current_step;
			car_body.transform.position = car_spawner.transform.position;
			car_body.transform.rotation = car_spawner.transform.rotation;
			car_body.velocity 			= Vector3.zero;
			car_body.angularVelocity 	= Vector3.zero;
		}
		//After 300 steps go to next particle
		if (current_step - particle_step > next_particle_wait)
		{
			working_particle++;
			//reset reward
			current_reward = 0;
			particle_step = current_step;
		}
		//Do a pso update after all particles
		if (working_particle == max_particles)
		{
			network_target.SetWeightsData(particle_swarm.GetBestWeights());
			//PSO Update Step
			particle_swarm.ComputeEpoch();
			particle_swarm.UpdateWeights();
			working_particle = 0;
		}
		current_step++;
		if (!abort_learning)
		{
			StartCoroutine(TakeStep());
		}
	}

	private int SelectAction(float[] q_values)
	{
		int q_index = 0;
		for (int i = 1; i < q_values.Length; i++)
		{
			if (q_values[q_index] < q_values[i])
				q_index = i;
		}
		return q_index;
	}

    /* ================ MATH FUNCTIONS ================ */

    private int[] ShuffleArray(int array_length)
    {
        System.Random random = new System.Random();
        int[] shuffle = new int[array_length];
        for (int i = 0; i < shuffle.Length; i++)
            shuffle[i] = i;
        for (int i = 0; i < shuffle.Length; i++)
        {
            int r = random.Next(0, shuffle.Length);
            int temp = shuffle[r];
            shuffle[i] = shuffle[r];
            shuffle[r] = temp;
        }
        return shuffle;
    }
};