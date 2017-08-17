using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
	public GameObject target;

	private Vector3 offset;

	private void Start()
	{
		offset = transform.position - target.transform.position;
	}

	private void Update()
	{
		transform.position = Vector3.Lerp(
			transform.position,
			target.transform.position + offset,
			Time.deltaTime * 3.0f
		);
	}

	//=============================

	public void ButtonQuit()
	{
		Application.Quit();
	}

	public void ButtonLoadLevel(int id)
	{
		SceneManager.LoadScene(id);
	}
}