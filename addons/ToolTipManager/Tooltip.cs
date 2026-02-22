using System.Threading.Tasks;

using Godot;
using Godot.Collections;

public partial class Tooltip : PanelContainer
{
    private Dictionary<string, ProgressBar> AddedProgressBars = [];
    private Container ProgressBarContainer => GetNode<Container>("%ProgressBarContainer");

    public Array<ProgressBarEntry> ProgressBarEntries = [];

    public override void _Ready()
    {
        ShowProgressBars(false);
    }

    public void AddProgressBar(string id, string displayName, Color color)
    {
        var progressBar = new ProgressBar
        {
            Name = id,
            Value = 0.0f,
            MinValue = 0.0f,
            MaxValue = 100.0f,
            ShowPercentage = false,
        };
        progressBar.AddThemeStyleboxOverride("fill", new StyleBoxFlat { BgColor = color });
        ProgressBarContainer.AddChild(progressBar);
        AddedProgressBars[id.ToLower()] = progressBar;
    }

    public void UpdateProgressBar(string id, float value)
    {
        if (AddedProgressBars.TryGetValue(id.ToLower(), out var foundBar))
            foundBar.Value = value;
    }

    public void UpdateText(string text)
    {
        var label = GetNode<Label>("%Label");
        if (label != null && label.Text != text)
            label.Text = text;
    }

    public void ShowProgressBars(bool show)
    {
        ProgressBarContainer.Visible = show;
        // if (show)
        // {
        //     foreach (var entry in AddedProgressBars)
        //     {
        //         if (ProgressBarContainer.FindChild(entry.Key) is ProgressBar progressBar)
        //             progressBar.Value = 0.0f;
        //     }
        // }
    }

    public void UpdatePosition(Vector2 position)
    {
        Position = position;
    }

    public async Task FadeIn()
    {
        Modulate = new Color(1, 1, 1, 1);
        Visible = true;
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1.0, 0.1);
        await ToSignal(tween, "finished");
    }
}
