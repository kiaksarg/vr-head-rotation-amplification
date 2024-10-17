using Techniques;

public class StepGainStrategy : IGainStrategy
{
    public float CalculateGain(GainCalculationContext context)
    {
        return RotationTechniques.stepGain(context.Angle);
    }
}
