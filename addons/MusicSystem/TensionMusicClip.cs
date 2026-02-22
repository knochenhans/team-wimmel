using Godot;

[GlobalClass]
public partial class TensionMusicClip : Resource
{
    [Export] public float Probability = 1.0f;
    [Export] public AudioStream Clip;
}
