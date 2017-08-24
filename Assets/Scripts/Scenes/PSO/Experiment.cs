using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Experiment : MonoBehaviour
{
	public enum Techniques
	{
		PSO		= 0,
		GA		= 1
	}

	[System.NonSerialized]
	public	bool				session_activated			= false;

	public 	GameObject			car_prefab					= null;
	public 	Transform			spawner_position			= null;
	public 	Text				output_log					= null;

	private bool				signal_session_stop			= false;
	private	float				session_length				= 60.0f;
	private float				session_timer 				= 0.0f;
	private int					agents_amount				= 20;
	private int					current_epoch				= 0;
	private float				best_score					= float.MinValue;
	private float[]				best_weights;
	private string				log							= "";
	private Techniques			training_technique			= Techniques.PSO;

	//Car
	private	GameObject[]		current_cars;
	private Rigidbody[]			car_body;
	private CarAI[]				car_intelligence;
	private CarScoreManager[]	car_score_manager;
	private BaseNetwork[]		car_networks;

	//Threading
	private Thread				session_thread				= null;
	private List<Action>		functions_queued			= null;
	private ManualResetEvent	thread_wait					= null;
	private ManualResetEvent	thread_session_wait			= null;

	public void StartTraining()
	{
		current_epoch			= 0;
		best_score				= float.MinValue;
		best_weights			= null;
		session_activated 		= true;
		signal_session_stop 	= false;
		thread_wait				= new ManualResetEvent(false);
		thread_session_wait		= new ManualResetEvent(false);
		functions_queued 		= new List<Action>();
		session_thread 			= new Thread(new ThreadStart(ThreadShession));
		session_thread.Start();
	}

	public void StopTraining()
	{
		log = "Waiting for session to finish...";
		signal_session_stop 	= true;
		thread_session_wait.Set();
	}

	public void ChangeAgentNumber(float n)
	{
		agents_amount = Mathf.RoundToInt(n);
	}

	public void ChangeSessionTime(float t)
	{
		session_length = t;
	}

	public void ChangeTechnique(int t)
	{
		training_technique = (Techniques)t;
	}

	public void ChangeSpeedMultiplier(float s)
	{
		Time.timeScale = s;
	}

	public void SaveWeights()
	{
		if (best_weights == null)
		{
			log = "Please wait till atleast 1 epoch is complete.";
		}
		else
		{
			TextWriter write = new StreamWriter("Weights.txt");
			for (int i = 0; i < best_weights.Length; i++)
			{
				write.WriteLine(best_weights);
			}
			write.Close();
			log = "Weights stored in 'Weights.txt'";
		}
	}

	public void BackButton()
	{
		SceneManager.LoadScene(0);
	}

	private void Update()
	{
		if (functions_queued != null && functions_queued.Count > 0)
		{
			Action function_to_run = functions_queued[0];
			functions_queued.RemoveAt(0);
			function_to_run();
		}
		if (thread_session_wait != null && session_timer <= 0)
		{
			thread_session_wait.Set();
		}
		if (session_timer > 0)
		{
			session_timer -= Time.deltaTime;
			if (session_timer < session_length - 3.0f)
			{
				int done = 0;
				for (int i = 0; i < car_score_manager.Length; i++)
				{
					if (car_body[i].velocity.magnitude < 1.0f)
						done++;
				}
				if (done == car_score_manager.Length)
					session_timer = 0;
			}
		}
		if (output_log != null)
		{
			output_log.text = 	"Technique: " + training_technique + "\n" +
								"Epoch: " + current_epoch + "\n" +
								"Session Time: " + session_timer + "\n" + 
								"Best Score: " + best_score + "\n" +
								log;
		}
	}

	private void ThreadShession()
	{
		functions_queued.Add(ResetCars);
		thread_wait.WaitOne();
		thread_wait.Reset();
		
		BaseTechnique training;
		switch(training_technique)
		{
			case Techniques.PSO:
				training = new PSO(car_networks);
				break;
			case Techniques.GA:
				training = new GA(car_networks);
				break;
			default:
				log = "ERROR: Unkown training technique...";
				return;
		}

		while (!signal_session_stop)
		{
			log = "Reseting Cars";
			//Update car positions
			functions_queued.Add(() => {
				for (int i = 0; i < current_cars.Length; i++)
				{
					car_body[i].transform.position 	= spawner_position.transform.position;
					car_body[i].transform.rotation 	= spawner_position.transform.rotation;
					car_body[i].velocity 			= Vector3.zero;
					car_body[i].angularVelocity 	= Vector3.zero;
					car_score_manager[i].ResetScore();
					thread_wait.Set();
				}
			});
			thread_wait.WaitOne();
			thread_wait.Reset();
			log = "Updating Weights";
			//Update car weights & biases
			training.UpdateWeights();
			//Start session to get car score
			for (int i = 0; i < agents_amount; i++)
			{
				car_intelligence[i].activate_car 	= true;
				car_score_manager[i].activate_car 	= true;
				car_intelligence[i].GetNetwork().SetWeightsData(car_networks[i].GetWeightsData());
			}
			log = "Session Started";
			thread_session_wait.Reset();
			session_timer = session_length;
			thread_session_wait.WaitOne();
			if (!signal_session_stop)
			{
				//Stop session
				for (int i = 0; i < agents_amount; i++)
				{
					car_intelligence[i].activate_car 	= false;
					car_score_manager[i].activate_car 	= false;
					car_networks[i].SetNetworkScore(car_intelligence[i].GetNetwork().GetNetworkScore());
				}
				log = "Comparing Results";
				//Compare all cars scores
				training.ComputeEpoch();
				//Update log info
				best_score 		= training.GetBestScore();
				best_weights 	= training.GetBestWeights();
				current_epoch++;
			}
		}
		session_activated = false;
	}

	private void ResetCars()
	{
		if (current_cars != null)
		{
			for (int i = 0; i < current_cars.Length; i++)
			{
				Destroy(current_cars[i]);
			}
		}
		
		current_cars 		= new GameObject[agents_amount];
		car_body			= new Rigidbody[agents_amount];
		car_intelligence 	= new CarAI[agents_amount];
		car_score_manager 	= new CarScoreManager[agents_amount];		
		car_networks 		= new BaseNetwork[agents_amount];
		for (int i = 0; i < current_cars.Length; i++)
		{
			current_cars[i] 		= (GameObject)Instantiate(car_prefab, spawner_position.position, spawner_position.rotation);
			car_intelligence[i]		= current_cars[i].GetComponent<CarAI>();
			car_score_manager[i] 	= current_cars[i].GetComponent<CarScoreManager>();
			car_body[i]				= current_cars[i].transform.GetChild(0).gameObject.GetComponent<Rigidbody>();

			car_intelligence[i].Start();
			car_networks[i] 		= ObjectCopier.Clone<BaseNetwork>(car_intelligence[i].GetNetwork());
			car_networks[i].RandomizeWeights(i);
		}
		thread_wait.Set();
	}
}