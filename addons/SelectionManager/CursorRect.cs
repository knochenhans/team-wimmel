using Godot;

public partial class CursorRect : Control
{
    CursorRectEdge[] CursorEdges;
    Node2D TargetNode2D;
    Vector2 Offset;
    Camera2D Camera;

    public override void _Ready()
    {
        CursorEdges = new CursorRectEdge[4];
        for (int i = 0; i < 4; i++)
            CursorEdges[i] = GetNodeOrNull<CursorRectEdge>($"CursorEdge{i + 1}");
    }

    public void SetTargetNode(Node2D targetNode, Vector2 size, Vector2 offset = default, float padding = 0f)
    {
        TargetNode2D = targetNode;
        Size = size + new Vector2(padding, padding);
        Offset = offset;
        UpdatePosition(targetNode);
        InitializeEdges(Size.X / 8);
    }

    private void UpdatePosition(Node2D targetNode) => Position = targetNode.Position - Size / 2 + Offset;

    public void InitializeEdges(float length, float width = 1f)
    {
        SetEdgeLength(length);
        SetEdgeWidth(width);
    }

    public void SetEdgeLength(float length)
    {
        foreach (var edge in CursorEdges)
            edge?.SetLength(length);
    }

    public void SetEdgeWidth(float width)
    {
        foreach (var edge in CursorEdges)
            edge?.SetWidth(width);
    }

    public override void _Process(double delta) => UpdatePosition(TargetNode2D);
}
