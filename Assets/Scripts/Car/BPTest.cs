using System;
using UnityEngine;

public class BPTest : MonoBehaviour
{
    private void Start()
    {
        
        MFNN network = new MFNN(new int[]{ 4, 7, 3 }, new ActivationType[]{
                ActivationType.NONE,
                ActivationType.LOGISTIC_SIGMOID,
                ActivationType.LOGISTIC_SIGMOID
            });

        Debug.Log("Initial Error: " + MSR(IrisData.dataset, network));
        
        int input_size = network.GetInputSize();
        int output_size = network.GetOutputSize();

        int r = 0;
        while (r < 100)
        {
            for (int i = 0; i < IrisData.dataset.Length; i++)
            {
                float[] x_values = new float[input_size];
                float[] t_values = new float[output_size];
                Array.Copy(IrisData.dataset[i], 0, x_values, 0, input_size);
                Array.Copy(IrisData.dataset[i], input_size, t_values, 0, output_size);

                float[] y_values = network.Compute(x_values);
                float[] errors = new float[output_size];
                for (int j = 0; j < output_size; j++)
                {
                    errors[j] = t_values[j] - y_values[j];
                }
                network.UpdateWeights(errors, 0.01f, 0.0001f, 0.5f);
            }
            Debug.Log("Itr. "+r+" MSR: " + MSR(IrisData.dataset, network));
            r++;
        }
    }

    private float MSR(float[][] data, MFNN network)
    {
        int input_size = network.GetInputSize();
        int output_size = network.GetOutputSize();

        //Error Checking
        Debug.Assert(data.Length > 0);
        Debug.Assert(data[0].Length == input_size + output_size);

        float msr = 0;
        for (int i = 0; i < data.Length; i++)
        {
            float[] x_values = new float[input_size];
            float[] t_values = new float[output_size];
            Array.Copy(data[i], 0, x_values, 0, input_size);
            Array.Copy(data[i], input_size, t_values, 0, output_size);

            float[] y_values = network.Compute(x_values);

            float sum = 0;
            for (int j = 0; j < output_size; j++)
            {
                sum += (t_values[j] - y_values[j]) * (t_values[j] - y_values[j]);
            }
            msr += sum;
        }

        return msr;
    }
}