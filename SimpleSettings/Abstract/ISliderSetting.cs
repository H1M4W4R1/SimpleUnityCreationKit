namespace Systems.SimpleSettings.Abstract
{
    /// <summary>
    ///     UI hint: this setting should be represented as a slider.
    /// </summary>
    public interface ISliderSetting
    {
        /// <summary>Minimum selectable value.</summary>
        float MinValue { get; }

        /// <summary>Maximum selectable value.</summary>
        float MaxValue { get; }

        /// <summary>
        ///     Discrete step between values.
        ///     Set to <c>0</c> for a continuous (unrestricted) slider.
        /// </summary>
        float Step { get; }
    }
}
