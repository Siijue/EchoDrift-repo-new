using UnityEngine;

public class ReverseWillAbility : IstAbility
{
    public ReverseWillAbility(IstAI own) : base("ReverseWill", 14f, priority: 5) { }
    protected override void Execute()
    {
        GlitchEffectSystem.Instance?.StartControlInvertion(5f);
        Debug.Log("Глитч-атака 'Реверс воли'");
    }
}

public class FragmentationAbility : IstAbility
{
    public FragmentationAbility(IstAI own) : base("Fragmentation", 18f, priority: 8) { }
    protected override void Execute()
    {
        GlitchEffectSystem.Instance?.StartScreenFragmentation(2.5f);
        Debug.Log("Глитч-атака 'Фрагментация иллюзий'");
    }
}

public class MirrorAbility : IstAbility
{
    public MirrorAbility(IstAI own) : base("Mirror", 16f, priority: 5) { }
    protected override void Execute()
    {
        GlitchEffectSystem.Instance?.StartPlayerMirror(6f);
        Debug.Log("Глитч-атака 'Безумие рефлексии'");
    }
}

public class CameraRiftAbility : IstAbility
{
    public CameraRiftAbility(IstAI own) : base("CameraRift", 10f, priority: 3) { }
    protected override void Execute()
    {
        GlitchEffectSystem.Instance?.StartCameraShift(4, 4);
        Debug.Log("Глитч-атака 'Разрыв и утрата'");
    }
}

public class TorchDebtAbility : IstAbility
{
    public TorchDebtAbility(IstAI own) : base("TorchDebt", 12f, priority: 8) { }
    protected override void Execute()
    {
        GlitchEffectSystem.Instance?.StartTorchBlackout(5f);
        Debug.Log("Глитч-атака 'Долг по мертвым'");
    }
}

public class SpectrumAbility : IstAbility
{
    public SpectrumAbility(IstAI own) : base("Spectrum", 20f, priority: 5) { }
    protected override void Execute()
    {
        GlitchEffectSystem.Instance?.StartColorInversion(3f);
        Debug.Log("Глитч-атака 'Обратный спектр'");
    }
}