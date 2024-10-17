public class ConstantGainStrategy : IGainStrategy
{
    private float constantGain;

    public ConstantGainStrategy(float gain)
    {
        constantGain = gain;
    }

    public float CalculateGain(GainCalculationContext context)
    {
        return constantGain;
    }
}
