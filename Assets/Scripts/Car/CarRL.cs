﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine;

public class CarRL : MonoBehaviour
{
    public GameObject car_spawner;
    public Text text_log;
    public float current_reward;
    public int max_batch_size = 32;
    public int memory_size = 200;
    public float epsilon_increment = 0.01f;
    public int learn_wait = 200;
    public int replace_target_iteration = 100;
    public float reward_decay = 0.9f;
    public float reaction_time = 0.1f;

    [System.NonSerialized]
    public bool is_activated = false;

    private struct MemoryStructure
    {
        public      float[]     current_state;
        public      float[]     next_state;
        public      int         picked_action;
        public      float       reward;
    };

    private int[][] actions = new int[][]{
        new int[]{ 0, 0 }, //No action
        new int[]{-1, 0 }, //Break
        new int[]{ 0,-1 }, //Turn Left
        new int[]{ 0, 1 }, //Turn Right
        new int[]{ 1, 0 }, //Accelerate
        new int[]{ 1,-1 }, //Turn Left & Accelerate
        new int[]{ 1, 1 }, //Turn Right & Accelerate
    };
    private     int                 action_index;

    private     MFNN                network_eval;
    private     MFNN                network_target;
    private     MemoryStructure[]   car_memory;
    private     int                 memory_index;
    private     float               max_epsilon;
    private     float               epsilon;
    private     CarCamera           car_camera;
    private     CarController       car_controller;
    private     Rigidbody           car_body;
    private     int                 current_step;
    private     int                 learn_step;
    private     int                 reset_step; 
    private     List<string>        log;
    private     bool                load_weights;

    public void StopTraining()
    {
        is_activated = false;
    }

    public void StartTraining()
    {
        //Setup
        action_index = 0;
        current_step = 0;
        learn_step = 0;
        is_activated = true;
        load_weights = false;
        log = new List<string>();

        //Setup memory
        memory_index = 0;
        car_memory = new MemoryStructure[memory_size];

        //Setup networks
        network_eval = new MFNN(
            new int[]{ 5, 20, actions.Length },
            new ActivationType[]{
                ActivationType.NONE,
                ActivationType.ReLU,
                ActivationType.ReLU
        });
        network_target = new MFNN(
            new int[]{ 5, 20, actions.Length },
            new ActivationType[]{
                ActivationType.NONE,
                ActivationType.ReLU,
                ActivationType.ReLU
        });

        //Setup epsilon
        max_epsilon = 0.9f;
        epsilon     = 0.0f;

        //Setup car components
        car_camera = GetComponent<CarCamera>();
        car_controller = GetComponent<CarController>();
        car_body = gameObject.transform.GetChild(0).gameObject.GetComponent<Rigidbody>();

        //Reset Car
        car_body.transform.position = car_spawner.transform.position;
        car_body.transform.rotation = car_spawner.transform.rotation;
        car_body.velocity = Vector3.zero;
        car_body.angularVelocity = Vector3.zero;
        action_index = 0;
        reset_step = current_step;

        //Start loop
        StartCoroutine(LearnStep());
        log.Add("Training Started");
    }

    private void Start()
    {
        log = new List<string>();
        StartCoroutine(LogDestroy());
    }

    private System.Collections.IEnumerator LogDestroy()
    {
        if (log.Count > 0)
        {
            log.RemoveAt(0);
        }
        
        if (log.Count < 5)
            yield return new WaitForSeconds(10.0f);

        StartCoroutine(LogDestroy());
    }

    private void Update()
    {
        if (is_activated)
        {
            car_controller.Accelerate(actions[action_index][0]);
            if (actions[action_index][0] == -1)
                car_controller.Brake();
            car_controller.Steer(actions[action_index][1]);
        }

        string full_log = "";
        for (int i = 0; i < log.Count; i++)
        {
            full_log += ">>" + log[i] + "\n";
        }
        text_log.text   = "Last Reward Recived: " + current_reward + "\n"
                        + full_log;
    }

    private System.Collections.IEnumerator LearnStep()
    {
        //Observe
        float[] current_state = car_camera.GetRays();

        //Chose action
        int action = ChoseAction(current_state);

        //Take action
        action_index = action;
        yield return new WaitForSeconds(reaction_time);

        //Get reward
        float current_reward = car_body.gameObject.transform.InverseTransformDirection(car_body.velocity).z;
        this.current_reward = current_reward;

        //Get next state
        float[] next_state = car_camera.GetRays();

        //Store information
        StoreInformation(current_state, next_state, action, current_reward);

        //Learn (every 15 steps after the inital `learn_wait` steps)
        if (current_step > learn_wait && current_step % 15 == 0)
        {
            Learn();
        }

        //Update step
        current_step++;

        //Reset trapped car
        if (car_body.velocity.magnitude < 1.0f && current_step - reset_step > 100)
        {
            car_body.transform.position = car_spawner.transform.position;
            car_body.transform.rotation = car_spawner.transform.rotation;
            car_body.velocity = Vector3.zero;
            car_body.angularVelocity = Vector3.zero;
            action_index = 0;
            reset_step = current_step;
        }

        if (load_weights)
        {
            LoadWeights();
            load_weights = false;
        }

        if (is_activated)
            StartCoroutine(LearnStep());
        else
            log.Add("Training Stopped");
    }

    private void Learn()
    {
        //Copy eval net to target net after `replace_target_iteration` iterations
        if (learn_step % replace_target_iteration == 0)
        {
            network_target.SetWeightsData(network_eval.GetWeightsData());
            log.Add("Replacing `target net` with `eval net`");
        }

        //Get batch from memory
        int[] batch_index = CreateMemoryBatch();

        //Compute network error
        for (int i = 0; i < batch_index.Length; i++)
        {
            //Compute `q_eval` and `q_target`
            float[] q_eval = network_eval.Compute(car_memory[batch_index[i]].current_state);
            float[] q_target = network_target.Compute(car_memory[batch_index[i]].next_state);

            //Compute reward per action
            float[] reward = new float[actions.Length];
            int max_index = 0;
            for (int j = 1; j < actions.Length; j++)
            {
                if (q_eval[j] > q_eval[max_index])
                    max_index = j;
            }
            reward[max_index] = car_memory[batch_index[i]].reward;
            
            q_target = AddArray(reward, MultiplyArray(reward_decay, q_target));
            float[] error = SquareArray(SubtractArray(q_target, q_eval));

            network_eval.UpdateWeights(error, 0.01f, 0.00001f, 0.05f);
        }

        //Update epsilon
        if (epsilon < max_epsilon)
            epsilon += epsilon_increment;
        else
            epsilon = max_epsilon;
        //Update learn step
        learn_step++;
    }

    private int[] CreateMemoryBatch()
    {
        System.Random random = new System.Random();

        int[] shuffle = ShuffleArray(memory_index);

        int batch_size = max_batch_size;
        if (memory_index <= max_batch_size)
            batch_size = memory_index;

        int[] batch = new int[batch_size];
        int batch_index = random.Next(0, memory_index - batch_size + 1);
        for (int i = batch_index; i < batch_size; i++)
        {
            batch[i] = shuffle[i];
        }

        return batch;
    }

    private void StoreInformation(float[] current_state, float[] next_state, int action, float reward)
    {
        car_memory[memory_index] = new MemoryStructure();
        car_memory[memory_index].current_state  = current_state;
        car_memory[memory_index].next_state     = next_state;
        car_memory[memory_index].picked_action  = action;
        car_memory[memory_index].reward         = reward;
        memory_index++;
        
        if (memory_index == memory_size)
            memory_index = 0;
    }

    private int ChoseAction(float[] current_state)
    {
        System.Random rnd = new System.Random();
        if (rnd.NextDouble() < epsilon)
        {
            float[] q_values = network_eval.Compute(current_state);
            int q_index = 0;
            for (int i = 1; i < q_values.Length; i++)
            {
                if (q_values[i] > q_values[q_index])
                    q_index = i;
            }
            return q_index;
        }
        else
        {
            return rnd.Next(0, actions.Length);
        }
    }

    /* ================ MATH FUNCTIONS ================ */

    private int[] ShuffleArray(int array_length)
    {
        System.Random random = new System.Random();
        int[] shuffle = new int[array_length];
        for (int i = 0; i < shuffle.Length; i++)
            shuffle[i] = i;
        for (int i = 0; i < shuffle.Length; i++)
        {
            int r = random.Next(0, shuffle.Length);
            int temp = shuffle[r];
            shuffle[i] = shuffle[r];
            shuffle[r] = temp;
        }
        return shuffle;
    }

    private float[] MultiplyArray(float v1, float[] v2)
    {
        float[] rtn = v2;
        for (int i = 0; i < rtn.Length; i++)
        {
            rtn[i] *= v1;
        }
        return rtn;
    }

    private float[] AddArray(float[] v1, float[] v2)
    {
        float[] rtn = new float[v1.Length];
        for (int i = 0; i < rtn.Length; i++)
        {
            rtn[i] = v1[i] + v2[i];
        }
        return rtn;
    }

    private float[] SubtractArray(float[] v1, float[] v2)
    {
        float[] rtn = new float[v1.Length];
        for (int i = 0; i < rtn.Length; i++)
        {
            rtn[i] = (v1[i] - v2[i]) * (v1[i] - v2[i]);
        }
        return rtn;
    }

    private float[] SquareArray(float[] v1)
    {
        float[] rtn = new float[v1.Length];
        for (int i = 0; i < rtn.Length; i++)
        {
            rtn[i] = v1[i] * v1[i];
        }
        return rtn;
    }

    /* ================ SAVE/LOAD WEIGHTS ================ */

    public void SaveWeights()
	{
        float[] best_weights = network_eval.GetWeightsData();
        TextWriter write = new StreamWriter("Weights.txt");
        for (int i = 0; i < best_weights.Length; i++)
        {
            write.WriteLine(best_weights[i] + ",");
        }
        write.Close();
        log.Add("Weights stored in 'Weights.txt'");
	}

    public void SetLoadWeights()
    {
        load_weights = true;
    }

    private void LoadWeights()
	{
        TextReader read = new StreamReader("Weights.txt");
        string[] str_weights = read.ReadToEnd().Split(',');
        read.Close();
        if (str_weights.Length - 1 != network_target.GetWeightsLength())
        {
            log.Add("'Weights.txt' file does not contain matching network weights");
            return;
        }
        float[] weights = new float[str_weights.Length - 1];
        for(int i = 0; i < weights.Length; i++)
        {
            if (!float.TryParse(str_weights[i], out weights[i]))
            {
                log.Add("Could not convert weight " + i + "(" + str_weights[i] + ") into a float");
                return;
            }
        }
        network_eval.SetWeightsData(weights);
        network_target.SetWeightsData(weights);
        log.Add("Weights successfully loaded from 'Weights.txt'");
	}
};