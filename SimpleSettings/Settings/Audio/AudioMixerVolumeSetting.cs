using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using UnityEngine;
using UnityEngine.Audio;

namespace Systems.SimpleSettings.Settings.Audio
{
    /// <summary>
    ///     Abstract base for a volume setting that writes to an
    ///     <see cref="AudioMixer"/> exposed parameter.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Values are in the range [0, 1]. On apply, the value is converted to
    ///         decibels using <c>20 × log₁₀(value)</c> and written to
    ///         <see cref="AudioMixer.SetFloat"/> with <see cref="MixerParameterName"/>.
    ///     </para>
    ///     <para>
    ///         Concrete subclasses (e.g. <see cref="MasterVolumeSetting"/>) specify the
    ///         mixer parameter name and receive a unique <see cref="ISetting.Key"/> via
    ///         the auto-key mechanism.
    ///     </para>
    /// </remarks>
    public abstract class AudioMixerVolumeSetting : Setting<float>, ISliderSetting
    {
        private readonly AudioMixer _mixer;

        /// <summary>
        ///     Name of the exposed AudioMixer float parameter this setting controls.
        /// </summary>
        [NotNull] protected abstract string MixerParameterName { get; }

        // ─────────────────────── ISliderSetting ───────────────────────────
        /// <inheritdoc/>
        public float MinValue => 0f;

        /// <inheritdoc/>
        public float MaxValue => 1f;

        /// <inheritdoc/>
        public float Step => 0f;

        // ──────────────────────── Constructor ─────────────────────────────

        /// <summary>
        ///     Creates a new volume setting for the given <paramref name="mixer"/>.
        /// </summary>
        /// <param name="mixer">
        ///     The AudioMixer that exposes this volume parameter.
        ///     May be <c>null</c> — the setting still tracks the value but won't write to the mixer.
        /// </param>
        /// <param name="defaultVolume">Initial volume in [0, 1]; defaults to 1.</param>
        protected AudioMixerVolumeSetting([CanBeNull] AudioMixer mixer,
                                          float defaultVolume = 1f) : base(defaultVolume)
        {
            _mixer = mixer;
        }

        // ─────────────────────── Overrides ────────────────────────────────

        /// <inheritdoc/>
        protected override void OnApplyInternal(float value)
        {
            if (!_mixer) return;
            float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            _mixer.SetFloat(MixerParameterName, db);
        }
    }
}
