
[System.Serializable]
abstract public class BaseNetwork
{
    abstract public float[] Compute(float[] inputs);

    abstract public int GetWeightsLength();
    abstract public float[] GetWeightsData();
    abstract public void SetWeightsData(float[] weights);
    abstract public void RandomizeWeights(int seed = 0);

    abstract public float GetNetworkScore();
    abstract public void SetNetworkScore(float s);
}