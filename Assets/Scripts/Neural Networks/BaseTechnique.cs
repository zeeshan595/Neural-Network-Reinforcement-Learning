
abstract public class BaseTechnique
{
	abstract public void ComputeEpoch();

	abstract public void UpdateWeights();

	abstract public float[] GetBestWeights();

	abstract public float GetBestScore();
}