using Godot;

public partial class SelectionRect : Control
{
    private Line2D Rect => GetNode<Line2D>("Rect");

    public Vector2 StartPosition { get; private set; }
    public Vector2 EndPosition { get; private set; }

    public void UpdateSelectionStart(Vector2 start)
    {
        StartPosition = start;
        UpdateRect();
    }

    public void UpdateSelectionRectEnd(Vector2 end)
    {
        EndPosition = end;
        UpdateRect();
    }

    private void UpdateRect()
    {
        // Calculate the four corners of the rectangle
        var topLeft = new Vector2(Mathf.Min(StartPosition.X, EndPosition.X), Mathf.Min(StartPosition.Y, EndPosition.Y));
        var bottomRight = new Vector2(Mathf.Max(StartPosition.X, EndPosition.X), Mathf.Max(StartPosition.Y, EndPosition.Y));
        var topRight = new Vector2(bottomRight.X, topLeft.Y);
        var bottomLeft = new Vector2(topLeft.X, bottomRight.Y);

        // Set the points in order to form a rectangle (closed shape)
        Rect.Points = [topLeft, topRight, bottomRight, bottomLeft, topLeft];
    }

    public new Vector2 GetSize()
    {
        return EndPosition - StartPosition;
    }

    public new Rect2 GetRect()
    {
        var position = new Vector2(Mathf.Min(StartPosition.X, EndPosition.X), Mathf.Min(StartPosition.Y, EndPosition.Y));
        var size = new Vector2(Mathf.Abs(EndPosition.X - StartPosition.X), Mathf.Abs(EndPosition.Y - StartPosition.Y));
        return new Rect2(position, size);
    }
}
