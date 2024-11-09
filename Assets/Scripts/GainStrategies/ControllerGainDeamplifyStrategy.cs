public class ControllerGainDeamplifyStrategy : IGainStrategy
{
    private GainControllerInput gainControllerInput;

    public ControllerGainDeamplifyStrategy(GainControllerInput input)
    {
        gainControllerInput = input;
    }

    public float CalculateGain(GainCalculationContext context)
    {
        return gainControllerInput.GetCurrentGain(true);
    }
}
