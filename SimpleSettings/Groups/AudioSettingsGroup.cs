using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Settings.Audio;
using UnityEngine.Audio;

namespace Systems.SimpleSettings.Groups
{
    /// <summary>
    ///     Built-in group that bundles all audio volume settings:
    ///     Master, Music, SFX, and Voice.
    /// </summary>
    /// <remarks>
    ///     Requires an <see cref="AudioMixer"/> that exposes the following
    ///     float parameters: <c>MasterVolume</c>, <c>MusicVolume</c>,
    ///     <c>SfxVolume</c>, <c>VoiceVolume</c>.
    ///     If mixer is <c>null</c>, volume settings still
    ///     function but will not affect the mixer.
    /// </remarks>
    public sealed class AudioSettingsGroup : SettingGroupBase
    {
        /// <inheritdoc/>
        public override string GroupId => "audio";

        // ─────────────────────── Settings ─────────────────────────────────

        /// <summary>Master output volume (0–1).</summary>
        [NotNull] public MasterVolumeSetting Master { get; }

        /// <summary>Music channel volume (0–1).</summary>
        [NotNull] public MusicVolumeSetting Music { get; }

        /// <summary>Sound effects channel volume (0–1).</summary>
        [NotNull] public SfxVolumeSetting Sfx { get; }

        /// <summary>Voice / dialogue channel volume (0–1).</summary>
        [NotNull] public VoiceVolumeSetting Voice { get; }

        // ──────────────────────── Constructor ─────────────────────────────

        /// <summary>
        ///     Creates the audio settings group.
        /// </summary>
        /// <param name="mixer">
        ///     The AudioMixer with exposed <c>MasterVolume</c>, <c>MusicVolume</c>,
        ///     <c>SfxVolume</c>, and <c>VoiceVolume</c> parameters.
        ///     May be <c>null</c> — group will still work but won't affect the mixer.
        /// </param>
        public AudioSettingsGroup([CanBeNull] AudioMixer mixer)
        {
            Master = new MasterVolumeSetting(mixer);
            Music  = new MusicVolumeSetting(mixer);
            Sfx    = new SfxVolumeSetting(mixer);
            Voice  = new VoiceVolumeSetting(mixer);

            RegisterSettings(GetSettings());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ISetting> GetSettings() => new ISetting[]
        {
            Master,
            Music,
            Sfx,
            Voice,
        };
    }
}
