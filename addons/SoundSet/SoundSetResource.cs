using System.Linq;
using Godot;
using Godot.Collections;
using static Logger.LogTypeEnum;

[GlobalClass, Icon("res://addons/SoundSet/Resources/Icons/AudioStream.svg"), Tool]
public partial class SoundSetResource : Resource
{
    [Export] public string ID = string.Empty;
    [Export] public Array<SoundSetEntryResource> SoundSet;
    [Export] public bool Randomize = true;
    [Export] public float VolumeOffset = 0.0f;
    [Export] public float PitchOffset = 1.0f;

    public AudioStream GetRandomSound()
    {
        if (SoundSet.Count == 0)
        {
            Logger.LogError($"SoundSet is empty.", "SoundSet", Audio);
            return null;
        }

        if (Randomize)
        {
            var randomIndex = GD.RandRange(0, SoundSet.Count - 1);
            return SoundSet[randomIndex].AudioStream;
        }
        else
        {
            return SoundSet.FirstOrDefault()?.AudioStream;
        }
    }
}