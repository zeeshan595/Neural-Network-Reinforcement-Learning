using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{
	[System.NonSerialized]
	public 	bool			activate_car 		= false;

	private MFNN 			network;
	private CarController 	controller;
	private CarCamera		car_camera;

	public void Start()
	{
		network = new MFNN(new uint[]{ 5, 10, 3 }, new ActivationType[]{
			ActivationType.NONE,
			ActivationType.HYPERBOLIC_TANGENT,
			ActivationType.HYPERBOLIC_TANGENT
		});
		controller = GetComponent<CarController>();
		car_camera = GetComponent<CarCamera>();
	}

	private void Update()
	{
		if (activate_car)
		{
			float[] 	car_rays 			= car_camera.GetRays();
			double[] 	network_inputs 		= System.Array.ConvertAll(car_rays, x => (double)x);
			double[] 	network_outputs 	= network.Compute(network_inputs);
			
			controller.Accelerate(Mathf.Abs((float)network_outputs[0]));
			controller.Steer((float)network_outputs[1]);
		}
	}

	public MFNN GetNetwork()
	{
		return network;
	}
}