using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using UnityEngine;

public class Experiment : MonoBehaviour
{
	public 		GUISkin				gui_skin						= null;
	public 		GameObject 			car_prefab						= null;
	public 		Transform			car_spawner						= null;
	public		ObserverCamera		observer_cam					= null;
	public		uint				amount							= 20;
	public 		float				session_time					= 20.0f;

	private		GameObject[]		car_objects;
	private		CarAI[]				car_ai_objects;
	private		CarScoreManager[]	car_score_objects;
	private 	PSO					particle_swarm_optimisation;
	private 	string				output_message					= "";
	private 	float				session_timer					= 0.0f;
	private 	uint				current_epoch					= 0;
	private 	Thread				thread_session					= null;
	private 	List<Action>		queued_functions				= new List<Action>();

	private void Start()
	{
		session_timer = session_time;
		output_message = "Setting Up Project";
		//Create multiple cars for training
		car_objects = new GameObject[amount];
		car_ai_objects = new CarAI[amount];
		car_score_objects = new CarScoreManager[amount];
		for (uint i = 0; i < amount; i++)
		{
			car_objects[i] = (GameObject)Instantiate(
				car_prefab,
				car_spawner.position,
				car_spawner.rotation
			);
			car_ai_objects[i] = car_objects[i].GetComponent<CarAI>();
			car_score_objects[i] = car_objects[i].GetComponent<CarScoreManager>();
			car_ai_objects[i].Start();
		}
		output_message = "Waiting for cars to initialize...";
		particle_swarm_optimisation = new PSO(GetAllNetworks());
		thread_session = new Thread(StartSession);
		thread_session.Start();
	}

	private void Update()
	{
		session_timer -= Time.deltaTime;
		//Execute queued functions from thread
		if (queued_functions.Count > 0)
		{
			Action function_to_execute = queued_functions[0];
			queued_functions.RemoveAt(0);

			function_to_execute();
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			thread_session.Abort();
			Debug.Log("Killed Thread");
		}
	}

	private void StartSession()
	{
		while(true)
		{
			output_message = "Epoch Started";
			//Reset car for next session
			output_message = "Reset Cars";
			queued_functions.Add(()=>{
				for (uint i = 0; i < amount; i++)
				{
					Rigidbody car_body = car_objects[i].GetComponent<CarController>().body;
					car_body.gameObject.transform.position = Vector3.zero;
					car_body.gameObject.transform.rotation = Quaternion.identity;
					car_body.velocity = Vector3.zero;
					car_body.angularVelocity = Vector3.zero;
					car_score_objects[i].ResetScore();
				}
			});
			//Get new weights to be tested in the session
			output_message = "Updating Weights";
			particle_swarm_optimisation.UpdateWeights();
			//Start session
			output_message = "Session Started";
			for (uint i = 0; i < amount; i++)
			{
				car_ai_objects[i].activate_car = true;
				car_score_objects[i].activate_car = true;
			}
			//Wait for session to finish
			Thread.Sleep(Mathf.RoundToInt(session_time) * 1000);
			//Stop Session
			for (uint i = 0; i < amount; i++)
			{
				car_ai_objects[i].activate_car = false;
				car_score_objects[i].activate_car = false;
			}
			//Compare results from session
			output_message = "Computing Score";
			particle_swarm_optimisation.ComputeEpoch();
			output_message = "End of Epoch";
			//Update session & epoch info
			if (session_time < 120)
				session_time += 5;
			session_timer = session_time;
			current_epoch++;
		}
	}
/*
	private void OnGUI()
	{
		if (gui_skin)
			GUI.skin = gui_skin;
		if (output_message == "Session Started")
			GUILayout.Box(output_message + ": " + session_timer);
		else
			GUILayout.Box(output_message);
		
		GUILayout.Box("Current Epoch: " + current_epoch);

		if (GUILayout.Button("SAVE WEIGHTS", GUILayout.Width(500.0f)))
		{
			double[] best_weights = particle_swarm_optimisation.GetBestWeights();
			TextWriter fs = new StreamWriter("Weights.txt");
			for (uint i = 0; i < best_weights.Length; i++)
				fs.WriteLine(best_weights[i].ToString() + ", ");
			fs.Close();
		}
	}
 */
	private BaseNetwork[] GetAllNetworks()
	{
		BaseNetwork[] networks = new BaseNetwork[amount];
		for (uint i = 0; i < amount; i++)
		{
			networks[i] = car_objects[i].GetComponent<CarAI>().GetNetwork();
		}
		return networks;
	}
}