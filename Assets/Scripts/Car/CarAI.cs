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

	public void Start()
	{
		network = new MFNN(new int[]{ 5, 10, 3 }, new ActivationType[]{
			ActivationType.NONE,
			ActivationType.LOGISTIC_SIGMOID,
			ActivationType.HYPERBOLIC_TANGENT
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
			
			controller.Accelerate(Mathf.Abs((float)network_outputs[0]));
			controller.Steer((float)network_outputs[1]);
		}
	}

	public BaseNetwork GetNetwork()
	{
		return network;
	}
}