﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarScoreManager : MonoBehaviour
{
	[System.NonSerialized]
	public 		bool			activate_car		= false;

	public		Rigidbody		body				= null;
	public 		float			car_score			= 0.0f;

	private 	CarAI			car_ai				= null;

	private void Start()
	{
		car_ai = GetComponent<CarAI>();
	}

	private void Update()
	{
		if (activate_car)
		{
			//Give the car a score besed on its local Z velocity and distance from the walls.
			car_score += body.gameObject.transform.InverseTransformDirection(body.velocity).z * Time.deltaTime;

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