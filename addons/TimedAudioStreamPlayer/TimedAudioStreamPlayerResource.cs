using Godot;

[GlobalClass]
public partial class TimedAudioStreamPlayerResource : Resource
{
    [Export] public bool Loop = false;
    [Export] public bool Autostart = true;
    [Export] public bool FinishOnStop = false;

    [ExportGroup("Randomization")]
    [Export] public bool Randomize = true;
    [Export] public float RandomVolumeAdded = 0.0f;
    [Export] public float RandomPitch = 1.0f;

    [ExportGroup("Delay")]
    [Export] public bool AlwaysRespectBaseDelay = false;
    [Export] public double BaseDelay = 0.0;
    [Export] public double RandomDelayAdded = 0.0;
    [Export] public double MaxDelay = 0.0;
    [Export] public double MinDelay = 0.0;

    public void Init()
    {
        if (MaxDelay < BaseDelay)
            MaxDelay = BaseDelay;

        if (MinDelay > BaseDelay)
            MinDelay = BaseDelay;
    }
}