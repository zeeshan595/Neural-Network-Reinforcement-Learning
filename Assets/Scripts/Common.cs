public class CommonFunctions
{
    public static double GetRandomNumber(double minimum, double maximum)
	{ 
		System.Random rand = new System.Random();
		return rand.NextDouble() * (maximum - minimum) + minimum;
	}
}