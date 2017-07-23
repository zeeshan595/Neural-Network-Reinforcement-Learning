public class Particle
{
    public double[]            velocity;
    public double[]            position;
    public double[]            best_position;

    public double              score           = 0;
    public double              best_score      = 0;

    public BaseNetwork         network         = null;

    public Particle()
    {

    }
}