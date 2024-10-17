using Techniques;

public class DynamicNonLinearGainStrategy : IGainStrategy
{
    private float targetRotation;

    public DynamicNonLinearGainStrategy(float targetRotation)
    {
        this.targetRotation = targetRotation;
    }

    public float CalculateGain(GainCalculationContext context)
    {
        return RotationTechniques.dynamicNonLinearGain(context.Angle, targetRotation);
    }
}
