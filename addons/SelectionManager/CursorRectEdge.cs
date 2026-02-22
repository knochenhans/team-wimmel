using Godot;

public partial class CursorRectEdge : Control
{
    Line2D Line => GetNodeOrNull<Line2D>("Line");

    public void SetLength(float length)
    {
        if (Line != null)
        {
            var point1 = new Vector2(length, 0);
            var point2 = new Vector2(0, length);
            Line.SetPointPosition(0, point1);
            Line.SetPointPosition(2, point2);
        }
    }

    public void SetWidth(float width)
    {
        if (Line != null)
            Line.Width = width;
    }
}
