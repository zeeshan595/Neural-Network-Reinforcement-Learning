using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMenu : MonoBehaviour
{
	private CarController 	controller;
	private Rigidbody		body;

	private void Start()
	{
		controller = GetComponent<CarController>();
		body = transform.GetChild(0).gameObject.GetComponent<Rigidbody>();
	}

	private void Update()
	{
		controller.Accelerate(1.0f);
		body.gameObject.transform.localPosition = new Vector3(
			0,
			body.gameObject.transform.localPosition.y,
			body.gameObject.transform.localPosition.z
		);
	}
}