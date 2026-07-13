using Systems.SimpleUI.Components.Features.Drag;

namespace Systems.SimpleUI.Examples._04._Drag_and_Drop.Scripts.Drag
{
    public sealed class DragObjectExample : DragFeature<DragObjectExample>
    {
        // We must change parent to canvas when dragging
        // to make this thing render nicely
        protected override bool ChangeParent => true;
    }
}