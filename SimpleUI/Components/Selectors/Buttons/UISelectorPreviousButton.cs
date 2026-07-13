namespace Systems.SimpleUI.Components.Selectors.Buttons
{
    public sealed class UISelectorPreviousButton : UISelectorButtonBase
    {
        protected override void OnClick()
        {
            Selector.TrySelectPrevious();
        }

        protected override void OnTick()
        {
            base.OnTick();
            SetInteractable(Selector.HasPrevious || Selector.IsLooping);
        }
    }
}