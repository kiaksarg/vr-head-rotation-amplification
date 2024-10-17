public class ControllerGainStrategy : IGainStrategy
{
    private GainControllerInput gainControllerInput;

    public ControllerGainStrategy(GainControllerInput input)
    {
        gainControllerInput = input;
    }

    public float CalculateGain(GainCalculationContext context)
    {
        return gainControllerInput.GetCurrentGain();
    }
}
