using Godot;
using Godot.Collections;

[GlobalClass, Tool]
public partial class SoundDatabaseResource : Resource
{
    [Export] public string ID = string.Empty;
    [Export] public Array<SoundSetResource> SoundSets;
}
