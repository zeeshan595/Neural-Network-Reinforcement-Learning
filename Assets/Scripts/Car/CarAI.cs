using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{	
	[System.NonSerialized]
	public 	bool			activate_car 		= false;

	private BaseNetwork		network;
	private CarController 	controller;
	private CarCamera		car_camera;

	private int[][] actions = new int[][]{
		new int[]{ 0, 0 }, //No action
        new int[]{-1, 0 }, //Break
        new int[]{ 0,-1 }, //Turn Left
        new int[]{ 0, 1 }, //Turn Right
        new int[]{ 1, 0 }, //Accelerate
        new int[]{ 1,-1 }, //Turn Left & Accelerate
        new int[]{ 1, 1 }, //Turn Right & Accelerate
    };

	public void Start()
	{
		network = new MFNN(new int[]{ 5, 20, actions.Length }, new ActivationType[]{
			ActivationType.NONE,
			ActivationType.ReLU,
			ActivationType.ReLU
		});
		controller = GetComponent<CarController>();
		car_camera = GetComponent<CarCamera>();
	}

	private void Update()
	{
		if (activate_car)
		{
			float[] 	network_inputs		= car_camera.GetRays();
			float[] 	network_outputs 	= network.Compute(network_inputs);

			int action_index = ChoseAction(network_outputs);
			float accel = actions[action_index][0];
			float steer = actions[action_index][1];

			controller.Accelerate(accel);
			if (accel == -1)
				controller.Brake();

			controller.Steer(steer);
		}
	}

	public BaseNetwork GetNetwork()
	{
		return network;
	}

	public void SetNetwork(BaseNetwork network)
	{
		this.network = network;
	}

	private int ChoseAction(float[] q_values)
    {
		int q_index = 0;
		for (int i = 1; i < q_values.Length; i++)
		{
			if (q_values[i] > q_values[q_index])
				q_index = i;
		}
		return q_index;
    }
}