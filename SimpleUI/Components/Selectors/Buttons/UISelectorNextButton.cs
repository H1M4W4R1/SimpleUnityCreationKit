namespace Systems.SimpleUI.Components.Selectors.Buttons
{
    public sealed class UISelectorNextButton : UISelectorButtonBase
    {
        protected override void OnClick()
        {
            Selector.TrySelectNext();
        }

        protected override void OnTick()
        {
            base.OnTick();
            SetInteractable(Selector.HasNext || Selector.IsLooping);
        }
    }
}