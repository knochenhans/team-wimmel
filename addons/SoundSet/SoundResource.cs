using Godot;
using Godot.Collections;

[GlobalClass, Icon("res://addons/SoundSet/addons/SoundSet/Resources/Icons/AudioStream.svg"), Tool]
public partial class SoundResource : Resource
{
    [Export] public Dictionary<string, SoundSetResource> SoundSets = [];
}