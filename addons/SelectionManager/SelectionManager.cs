using System;
using System.Linq;

using Godot;
using Godot.Collections;

#nullable enable

public partial class SelectionManager() : CanvasLayer
{
    #region [Fields and Properties]
    [Export] private PackedScene cursorScene = null!;
    [Export] private PackedScene selectionScene = null!;

    [Export] public int MinimalSelectionSize = 5;

    [Signal] public delegate void SelectionChangedEventHandler(Array<NodeEntry> selectedNodeEntries);
    [Signal] public delegate void SelectionFinishedEventHandler(Array<NodeEntry> selectedNodeEntries);
    [Signal] public delegate void TargetPositionSetEventHandler(Node2D character, Vector2 position);

    Background Background => GetNode<Background>("Background");
    public bool ignoreNextInput = false;
    Vector2 lastSelectionRectSize = Vector2.Zero;

    public enum SelectionRectStateEnum
    {
        None,
        StartSelecting,
        Selecting,
        EndSelecting
    }

    public partial class NodeEntry(Node2D node, Vector2 size, Vector2 offset) : GodotObject
    {
        public Node2D Node = node;
        public Vector2 Size = size;
        public Vector2 Offset = offset;
    }

    public Array<NodeEntry> RegisteredNodes = [];
    public Array<NodeEntry> SelectedNodes = [];

    Dictionary<string, CursorRect> cursorRects = [];
    SelectionRect? selectionRect;

    Vector2 bufferedTargetPosition = Vector2.Zero;
    public SelectionRectStateEnum SelectionRectState = SelectionRectStateEnum.None;
    #endregion

    #region [Godot]
    public override void _Ready() => Background.selectionManager = this;
    #endregion

    #region [Lifecycle]
    public void RegisterNode(Node2D node, Vector2 size, Vector2 offset = default)
    {
        if (RegisteredNodes.Any(entry => entry.Node == node))
            return;

        RegisteredNodes.Add(new NodeEntry(node, size, offset));
        Logger.Log($"Node {node.Name} registered with Selection Manager.", Logger.LogTypeEnum.UI);
    }

    public void UnregisterNode(Node2D node)
    {
        var nodeEntry = RegisteredNodes.FirstOrDefault(entry => entry.Node == node);
        if (nodeEntry != null)
            RegisteredNodes.Remove(nodeEntry);

        RemoveSelectionFor(nodeEntry!);
        Logger.Log($"Node {node.Name} unregistered from Selection Manager.", Logger.LogTypeEnum.UI);
    }

    private void CreateSelectionFor(NodeEntry nodeEntry)
    {
        if (SelectedNodes.Any(entry => entry.Node == nodeEntry.Node))
            return;

        SelectedNodes.Add(nodeEntry);

        var cursor = cursorScene.Instantiate<CursorRect>();

        AddChild(cursor);
        cursor.SetTargetNode(nodeEntry.Node, nodeEntry.Size, nodeEntry.Offset);
        cursorRects[nodeEntry.Node.Name] = cursor;
        Logger.Log($"Node {nodeEntry.Node.Name} added to selection.", Logger.LogTypeEnum.UI);
    }

    private void RemoveSelectionFor(NodeEntry nodeEntry)
    {
        if (SelectedNodes.Any(entry => entry.Node == nodeEntry.Node))
            SelectedNodes.Remove(nodeEntry);

        if (cursorRects.TryGetValue(nodeEntry.Node.Name, out var cursor))
        {
            cursor.QueueFree();
            cursorRects.Remove(nodeEntry.Node.Name);
        }
        Logger.Log($"Node {nodeEntry.Node.Name} removed from selection.", Logger.LogTypeEnum.UI);
        EmitSignal(SignalName.SelectionChanged, SelectedNodes);
    }

    public void AddNodeToSelection(Node2D node)
    {
        try
        {
            var nodeEntry = RegisteredNodes.First(entry => entry.Node == node);
            CreateSelectionFor(nodeEntry);
            EmitSignal(SignalName.SelectionChanged, SelectedNodes);
        }
        catch (InvalidOperationException)
        {
            Logger.Log($"Attempted to add unregistered node {node.Name} to selection.", Logger.LogTypeEnum.UI);
        }
    }

    public void ToggleNodeInSelection(Node2D node)
    {
        var nodeEntry = RegisteredNodes.First(entry => entry.Node == node);
        if (SelectedNodes.Any(entry => entry.Node == node))
            RemoveSelectionFor(nodeEntry);
        else
            CreateSelectionFor(nodeEntry);
        EmitSignal(SignalName.SelectionChanged, SelectedNodes);
    }

    public void ClearSelection()
    {
        if (ignoreNextInput)
        {
            ignoreNextInput = false;
            return;
        }

        Logger.Log("Clearing selection.", Logger.LogTypeEnum.UI);

        for (int i = SelectedNodes.Count - 1; i >= 0; i--)
        {
            var entry = SelectedNodes[i];
            RemoveSelectionFor(entry);
        }

        SelectedNodes.Clear();
    }

    public void SetTargetPosition(Vector2 position, bool force = false)
    {
        if (SelectionRectState != SelectionRectStateEnum.None && !force)
            return;

        foreach (var entry in SelectedNodes)
        {
            if (entry.Node is Node2D character)
                EmitSignal(SignalName.TargetPositionSet, character, position);
        }
    }

    public void UpdateSelectionRect(Vector2 mousePosition)
    {
        if (SelectionRectState == SelectionRectStateEnum.StartSelecting)
            SelectionRectState = SelectionRectStateEnum.Selecting;

        if (SelectionRectState == SelectionRectStateEnum.Selecting)
        {
            selectionRect?.UpdateSelectionRectEnd(mousePosition + Background.Position);

            // Check for entities within the selection rectangle
            if (selectionRect != null)
            {
                if (Mathf.Abs(selectionRect.GetSize().X) < MinimalSelectionSize || Mathf.Abs(selectionRect.GetSize().Y) < MinimalSelectionSize)
                    return;

                if (selectionRect.GetSize() < lastSelectionRectSize)
                    ClearSelection();

                foreach (var node in RegisteredNodes)
                {
                    if (node == null)
                        continue;

                    var rect = new Rect2(node.Node.GlobalPosition - (node.Size / 2) + node.Offset, node.Size);

                    if (selectionRect.GetRect().Intersects(rect))
                        AddNodeToSelection(node.Node);
                }
            }
            lastSelectionRectSize = selectionRect?.GetSize() ?? Vector2.Zero;
        }
    }

    public void StartSelectionRect(Vector2 position)
    {
        SelectionRectState = SelectionRectStateEnum.StartSelecting;
        selectionRect = selectionScene.Instantiate<SelectionRect>();
        selectionRect.UpdateSelectionStart(position);
        selectionRect.UpdateSelectionRectEnd(position);
        AddChild(selectionRect);
    }

    public void EndSelectionRect()
    {
        selectionRect?.QueueFree();
        selectionRect = null;
        SelectionRectState = SelectionRectStateEnum.None;
    }

    public void SelectNode(Node2D node, bool additive = false)
    {
        if (!additive)
        {
            ignoreNextInput = false;
            ClearSelection();
        }

        AddNodeToSelection(node);
        ignoreNextInput = true;
    }
    #endregion

    #region [Events]
    public void OnNodeClicked(Node2D node, InputEventMouseButton mouseButtonEvent, bool ShiftPressed = false, bool CtrlPressed = false)
    {
        if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.IsPressed())
        {
            if (!SelectedNodes.Any(entry => entry.Node == node))
            {
                if (SelectedNodes.Any(entry => entry.Node == node))
                    return;

                if (node.IsInGroup("selectable"))
                    SelectNode(node, ShiftPressed || CtrlPressed);
            }
        }
        else if (mouseButtonEvent.ButtonIndex == MouseButton.Right && mouseButtonEvent.IsPressed())
        {
            ClearSelection();
        }
    }

    public void MouseClicked(InputEventMouseButton mouseButtonEvent, bool ShiftPressed, bool CtrlPressed = false)
    {
        var clearSelection = false;
        if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
        {
            if (mouseButtonEvent.IsPressed())
            {
                // Left mouse button pressed
                if (SelectionRectState == SelectionRectStateEnum.None)
                    StartSelectionRect(mouseButtonEvent.Position + Background.Position);
            }
            else
            {
                // Left mouse button released
                if (SelectionRectState != SelectionRectStateEnum.None)
                {
                    // Nodes have been selected and mouse was released, finish selection
                    var smallSelection = selectionRect != null && (selectionRect.GetSize().X < MinimalSelectionSize || selectionRect.GetSize().Y < MinimalSelectionSize);
                    EndSelectionRect();

                    EmitSignal(SignalName.SelectionFinished, SelectedNodes);

                    if (!smallSelection)
                        return;
                }

                if (SelectedNodes.Count > 0)
                {
                    // No nodes were selected with a selection rectangle, set target position for selected nodes
                    if (ignoreNextInput)
                    {
                        ignoreNextInput = false;
                        return;
                    }

                    SetTargetPosition(mouseButtonEvent.Position + Background.Position);
                    return;
                }
            }
        }
        else if (mouseButtonEvent.ButtonIndex == MouseButton.Right && !mouseButtonEvent.IsPressed())
        { clearSelection = true; }

        if (!ShiftPressed && !CtrlPressed && clearSelection)
            ClearSelection();
    }

    public void HandleInputEvent(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            if (SelectionRectState == SelectionRectStateEnum.Selecting || SelectionRectState == SelectionRectStateEnum.StartSelecting)
                UpdateSelectionRect(mouseMotionEvent.Position);
        }
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            var ShiftPressed = Input.IsKeyPressed(Key.Shift);
            var CtrlPressed = Input.IsKeyPressed(Key.Ctrl);
            MouseClicked(mouseButtonEvent, ShiftPressed, CtrlPressed);
        }
    }
    #endregion

    #region [Utility]
    public Array<Node2D> NodesInGroupSelected(string groupName)
    {
        var nodesInGroup = new Array<Node2D>();
        foreach (var entry in SelectedNodes)
        {
            if (!entry.Node.IsInGroup(groupName))
                continue;

            nodesInGroup.Add(entry.Node);
        }
        return nodesInGroup;
    }
    #endregion
}
