using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarScoreManager : MonoBehaviour
{
	[System.NonSerialized]
	public 		bool			activate_car		= false;

	public		Rigidbody		body				= null;
	public 		float			car_score			= 0.0f;

	private 	CarCamera		car_camera			= null;
	private 	CarAI			car_ai				= null;

	private void Start()
	{
		car_camera = GetComponent<CarCamera>();
		car_ai = GetComponent<CarAI>();
	}

	private void Update()
	{
		if (activate_car)
		{
			//Give the car a score besed on its local Z velocity and distance from the walls.
			car_score += body.gameObject.transform.InverseTransformDirection(body.velocity).z * Time.deltaTime;
			
			float[] rays = car_camera.GetRays();
			
			for (int i = 0; i < rays.Length; i++)
			{
				if (rays[i] < 1.3f)
					car_score = -100;
			}

			if (car_ai)
				car_ai.GetNetwork().SetNetworkScore(car_score);
		}
	}

	public void ResetScore()
	{
		car_score = 0.0f;
		if (car_ai)
			car_ai.GetNetwork().SetNetworkScore(0.0f);
	}
}