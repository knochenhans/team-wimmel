using Godot;

[GlobalClass, Icon("res://addons/SoundSet/Resources/Icons/AudioStream.svg"), Tool]
public partial class SoundSetEntryResource : Resource
{
    [Export] public AudioStream AudioStream;
    [Export] public float VolumeOffset = 0.0f;
    [Export] public float PitchOffset = 1.0f;
    [Export] public float Probability = 1.0f;
}
