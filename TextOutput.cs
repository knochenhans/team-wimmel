using System;
using Godot;
using Godot.Collections;
using static Logger;

public partial class TextOutput : PanelContainer
{
    #region [Fields and Properties]
    // Label TextLabel => GetNode<Label>("%Label");
    TyperNode Typer => GetNode<TyperNode>("%Typer");
    #endregion

    #region [Godot]
    public override void _Ready()
    {
    }

    #endregion

    #region [Lifecycle]

    #endregion

    #region [Public]
    public void UpdateText(string text)
    {
        Typer.PushText(text);
    }

    public void Clear()
    {
        Typer.ClearText();
    }
    #endregion

    #region [Events]

    #endregion

    #region [Utility]

    #endregion

    #region [Saving and Loading]

    #endregion
}
