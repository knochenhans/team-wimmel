using Godot;

public partial class NotificationLabel : Label
{
    [Signal] public delegate void NotificationClosedEventHandler();

    Timer LifetimeTimer;
    ColorRect BackgroundRect => GetNode<ColorRect>("BackgroundRect");

    public float Lifetime = 3f;
    public bool AnimateLifetimeBar = false;

    public override void _Ready()
    {
        LifetimeTimer = new Timer();
        AddChild(LifetimeTimer);
        LifetimeTimer.WaitTime = Lifetime;
        LifetimeTimer.Timeout += OnTimerTimeout;
        LifetimeTimer.OneShot = true;
        LifetimeTimer.Start();

        if (AnimateLifetimeBar)
        {
            var tween = CreateTween();
            tween.TweenProperty(BackgroundRect, "size", new Vector2(0, Size.Y), Lifetime)
                 .SetTrans(Tween.TransitionType.Cubic)
                 .SetEase(Tween.EaseType.Out);
        }
    }

    public void SetMessage(string message)
    {
        Text = message;
    }

    async void OnTimerTimeout()
    {
        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.In, 1, 1);
        EmitSignal(SignalName.NotificationClosed);
    }
}
