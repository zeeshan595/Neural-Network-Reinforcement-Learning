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
		Vector3 target_pos = new Vector3(target.transform.position.x, 0, target.transform.position.z);
		transform.position = target_pos + offset;
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