using Lofelt.NiceVibrations;

public class Haptics : Singleton<Haptics>
{
    public void PlaySimple()
    {
        //  HapticPatterns.PlayEmphasis(1.0f, 0.0f);
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Warning);
    }

    public void PlayContinous()
    {
        HapticPatterns.PlayConstant(1.0f, 0.0f, 1.0f);

    }
}
