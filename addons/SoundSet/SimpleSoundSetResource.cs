using Godot;
using Godot.Collections;

[GlobalClass, Icon("res://addons/SoundSet/Resources/Icons/AudioStream.svg"), Tool]
public partial class SimpleSoundSetResource : Resource
{
    [Export] public Dictionary<string, AudioStream> Sounds = [];
}
