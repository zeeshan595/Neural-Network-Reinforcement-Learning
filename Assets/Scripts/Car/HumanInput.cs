using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HumanInput : MonoBehaviour
{
	private CarController controller;

	private void Start()
	{
		controller = GetComponent<CarController>();
	}

	private void Update()
	{
		//Gas
		if (Input.GetKey(KeyCode.UpArrow))
		{
			controller.Accelerate(1.0f);
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			controller.Brake();
		}
		else
		{
			controller.Accelerate(0.0f);
		}

		//Steer
		float steer = 0.0f;
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			steer = -1.0f;
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			steer = 1.0f;
		}
		controller.Steer(steer);
	}

	public void BackButton()
	{
		SceneManager.LoadScene(0);
	}
}