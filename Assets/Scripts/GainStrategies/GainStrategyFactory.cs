using Techniques;

public static class GainStrategyFactory
{
    public static IGainStrategy Create(GainTechniquesEnum technique, ScaleHeadRotation context)
    {
        switch (technique)
        {
            case GainTechniquesEnum.constant:
                return new ConstantGainStrategy(context.ConstantGain);
            case GainTechniquesEnum.dynamicLinear:
                return new DynamicGainStrategy(context.minGain, context.maxGain, context.halfRotation);
            case GainTechniquesEnum.dynamicNonLinear:
                return new DynamicNonLinearGainStrategy(context.targetRotation);
            case GainTechniquesEnum.controller:
                return new ControllerGainStrategy(context.gainControllerInput);
            case GainTechniquesEnum.controllerDeamplify:
                return new ControllerGainDeamplifyStrategy(context.gainControllerInput);
            case GainTechniquesEnum.velocityGuided:
                return new VelocityGuidedStrategy();
            case GainTechniquesEnum.step:
                return new StepGainStrategy();
            // Add other strategies as needed
            default:
                return new ConstantGainStrategy(1f);
        }
    }
}
