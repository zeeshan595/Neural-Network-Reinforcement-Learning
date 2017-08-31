using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine;

public class CarRL : MonoBehaviour
{
    public GameObject car_spawner;
    public Text text_log;
    public int max_batch_size = 32;
    public int memory_size = 200;
    public float epsilon_increment = 0.001f;
    public int learn_wait = 200;
    public int replace_target_iteration = 30;
    public float reward_decay = 0.9f;
    public float reaction_time = 0.1f;

    [System.NonSerialized]
    public bool is_activated = false;
    [System.NonSerialized]
    public float current_reward = 0.0f;

    private struct MemoryStructure
    {
        public      float[]     current_state;
        public      float[]     next_state;
        public      int         picked_action;
        public      float       reward;
    };

    private string[] actions_name = new string[]{
        "No Action", "Break", "Turn Left", "Turn Right", "Gas", "Turn Left & Gas", "Turn Right & Gas"
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
    private     MemoryStructure[]   network_memory;
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
    private     float[]             last_q_values;

    public void StopTraining()
    {
        Time.timeScale = 1;
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
        last_q_values = new float[actions.Length];

        //Setup memory
        memory_index = 0;
        network_memory = new MemoryStructure[memory_size];

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
        current_reward = 0;

        //Start loop
        StartCoroutine(DQNStep());
        log.Add("Training Started");
    }

    private void Start()
    {
        log = new List<string>();
        last_q_values = new float[actions.Length];
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
            if (actions[action_index][0] == -1)
            {
                car_controller.Accelerate(0);
                car_controller.Brake();
            }
            else
            {
                car_controller.Accelerate(actions[action_index][0]);
            }
            car_controller.Steer(actions[action_index][1]);
        }

        string full_log = "";
        full_log += "Last Reward Recived: " + current_reward + "\n";
        full_log += "Epsilon: " + epsilon + "\n";
        full_log += "Actions Q Value: \n";
        for (int i = 0; i < actions.Length; i++)
        {
            full_log += "       " + actions_name[i] + ": " + last_q_values[i] + "\n";
        }
        //Log
        for (int i = 0; i < log.Count; i++)
        {
            full_log += ">>" + log[i] + "\n";
        }
        text_log.text   = full_log;
    }

    private System.Collections.IEnumerator DQNStep()
    {
        //Observe
        float[] current_state = car_camera.GetRays();

        //Chose action
        int action = ChoseAction(current_state);

        //Take action
        action_index = action;
        yield return new WaitForSeconds(reaction_time);

        //Get reward
        float velocity = car_body.gameObject.transform.InverseTransformDirection(car_body.velocity).z;
        current_reward = velocity;

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
        if (car_body.velocity.magnitude < 0.3f && current_step - reset_step > 100)
        {
            car_body.transform.position = car_spawner.transform.position;
            car_body.transform.rotation = car_spawner.transform.rotation;
            car_body.velocity = Vector3.zero;
            car_body.angularVelocity = Vector3.zero;
            action_index = 0;
            reset_step = current_step;
            current_reward = 0;
        }

        if (load_weights)
        {
            LoadWeights();
            load_weights = false;
        }

        if (is_activated)
            StartCoroutine(DQNStep());
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
        if (batch_index.Length == 0)
            return;

        //Compute network error
        for (int i = 0; i < batch_index.Length; i++)
        {
            //Compute `q_eval` and `q_target`
            float[] q_target    = network_target.Compute(network_memory[batch_index[i]].next_state);
            float[] q_eval      = network_eval.Compute(network_memory[batch_index[i]].current_state);

            //Compute reward
            float reward = network_memory[batch_index[i]].reward;            

            //Add reward and reward decay
            int max_q_target = network_memory[batch_index[i]].picked_action;
            q_target[max_q_target] =  reward + reward_decay * q_target[max_q_target];
            //Compute error
            float[] error = new float[actions.Length];
            for (int j = 0; j < actions.Length; j++)
            {
                error[j] = q_target[j] - q_eval[j];
            }
            //Update weights using RMS-PROP
            network_eval.UpdateWeights(error);
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

        int batch_size = max_batch_size;
        if (memory_index <= max_batch_size)
            batch_size = memory_index;

        int[] shuffle = ShuffleArray(batch_size);
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
        network_memory[memory_index] = new MemoryStructure();
        network_memory[memory_index].current_state  = current_state;
        network_memory[memory_index].next_state     = next_state;
        network_memory[memory_index].picked_action  = action;
        network_memory[memory_index].reward         = reward;
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
            last_q_values = q_values;
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