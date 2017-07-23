abstract public class BaseNetwork
{
    abstract public double[] Compute(double[] inputs);

    abstract public void SetNetworkScore(float score);

	abstract public float GetNetworkScore();

    abstract public uint GetWeightsLength();
    abstract public double[] GetWeightsData();
    abstract public void SetWeightsData(double[] weights_data);
}