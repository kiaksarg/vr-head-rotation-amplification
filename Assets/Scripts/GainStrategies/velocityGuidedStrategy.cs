using Techniques;

public class VelocityGuidedStrategy : IGainStrategy
{
    public float CalculateGain(GainCalculationContext context)
    {
        return RotationTechniques.velocityGuidedGain(context.Velocity);
    }
}
