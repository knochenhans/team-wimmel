using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class NotificationManager : MarginContainer
{
    [Export] public PackedScene NotificationScene;

    [Export] public int MaxNotifications = 5;
    [Export] public int NotificationSpacing = 0;
    [Export] public Vector2 NotificationOffset = new(0, 0);

    [ExportGroup("Lifetime")]
    [Export] public float NotificationLifetime = 3f;
    [Export] public bool AnimateLifetimeBar = false;

    [ExportGroup("Movement")]
    [Export] public float NotificationMoveSpeed = 1f;
    [Export] public Vector2 MoveDirection = new(0, 0);

    [ExportGroup("Fade")]
    [Export] public float NotificationFadeInDuration = 1f;
    [Export] public float NotificationFadeOutDuration = 1f;

    private readonly Queue<NotificationLabel> NotificationQueue = new();
    private Control NotificationContainer => GetNode<Control>("NotificationContainer");

    public override void _Ready()
    {
        Logger.Log("NotificationManager is ready", "NotificationManager", Logger.LogTypeEnum.UI);
    }

    public async void ShowNotification(string message, float lifetime = -1f)
    {
        //TODO: Handle shutting down somewhere to avoid errors when quitting the game while notifications are active
        var notification = NotificationScene.Instantiate<NotificationLabel>();
        notification.SetMessage(message);
        notification.Lifetime = lifetime > 0 ? lifetime : NotificationLifetime;
        notification.NotificationClosed += () => OnNotificationClosed(notification);
        notification.Position = NotificationOffset;

        NotificationContainer.AddChild(notification);

        if (NotificationFadeInDuration > 0)
        {
            notification.Modulate = new Color(1, 1, 1, 0);
            await FadeHelper.TweenFadeModulate(notification, FadeHelper.FadeDirectionEnum.In, NotificationFadeInDuration);
        }

        NotificationQueue.Enqueue(notification);

        notification.GlobalPosition += new Vector2(0, (NotificationContainer.GetChildCount() - 1) * notification.Size.Y);
    }

    private async void OnNotificationClosed(NotificationLabel notification)
    {
        GD.Print("Closing notification");
        if (MoveDirection != Vector2.Zero)
        {
            var offset = new Vector2(MoveDirection.X * notification.Size.X,
                         MoveDirection.Y * notification.Size.Y);
            var targetPos = notification.Position + offset;
            await TweenPropertyAsync(notification, "position", targetPos, NotificationMoveSpeed);
        }

        if (NotificationFadeOutDuration > 0)
        {
            await FadeHelper.TweenFadeModulate(notification, FadeHelper.FadeDirectionEnum.Out, NotificationFadeOutDuration, 0f);
        }

        if (NotificationQueue.Count > 0 && NotificationQueue.Peek() == notification)
            NotificationQueue.Dequeue();

        notification.QueueFree();
    }

    private async Task TweenPropertyAsync(Node target, string property, Variant finalValue, float duration,
                                           Tween.TransitionType transition = Tween.TransitionType.Cubic,
                                           Tween.EaseType ease = Tween.EaseType.Out)
    {
        var tween = CreateTween();
        tween.TweenProperty(target, property, finalValue, duration)
             .SetTrans(transition)
             .SetEase(ease);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}
