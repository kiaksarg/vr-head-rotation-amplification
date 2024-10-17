using Techniques;

public class DynamicGainStrategy : IGainStrategy
{
    private float minGain;
    private float maxGain;
    private float halfRotation;

    public DynamicGainStrategy(float minGain, float maxGain, float halfRotation)
    {
        this.minGain = minGain;
        this.maxGain = maxGain;
        this.halfRotation = halfRotation;
    }

    public float CalculateGain(GainCalculationContext context)
    {
        return RotationTechniques.dynamicLinearGain(context.Angle, minGain, maxGain, halfRotation);
    }
}
