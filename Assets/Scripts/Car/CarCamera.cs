using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCamera : MonoBehaviour
{
	public 	GameObject			car_camera			= null;
	public 	Material			line_renderer_mat	= null;

	private	float[]				car_rays;
	private LineRenderer[]		line_renderers;

	private void Start()
	{
		car_rays 		= new float[5];
		line_renderers 	= new LineRenderer[5];
		for (int i = 0; i < line_renderers.Length; i++)
		{
			line_renderers[i]	= new GameObject("LineRenderer").AddComponent<LineRenderer>();
			line_renderers[i].transform.parent = transform;
			line_renderers[i].startWidth 	= 0.025f;
			line_renderers[i].endWidth 		= 0.025f;
			line_renderers[i].material		= line_renderer_mat;
		}
	}

	private void Update()
	{
		Vector3 direction;
		//Forward
		direction = car_camera.transform.forward;
		direction.Normalize();
		CreateRay(direction, 0);

		//Forward left
		direction = car_camera.transform.forward - car_camera.transform.right;
		direction.Normalize();
		CreateRay(direction, 1);

		//Forward right
		direction = car_camera.transform.forward + car_camera.transform.right;
		direction.Normalize();
		CreateRay(direction, 2);

		//Left
		direction = -car_camera.transform.right;
		direction.Normalize();
		CreateRay(direction, 3);

		//Right
		direction = car_camera.transform.right;
		direction.Normalize();
		CreateRay(direction, 4);
	}

	private void CreateRay(Vector3 direction, int id)
	{
		RaycastHit hit;
		float line_max_length = 20.0f;
		//Create Ray
		if (Physics.Raycast(car_camera.transform.position, direction, out hit, line_max_length))
		{
			car_rays[id] = hit.distance;
		}
		else
		{
			car_rays[id] = line_max_length;
		}
		//Debug the ray
		line_renderers[id].SetPositions(new Vector3[2]{
			car_camera.transform.position,
			car_camera.transform.position + (direction * car_rays[id])
		});
	}

	public float[] GetRays()
	{
		return car_rays;
	}
}
