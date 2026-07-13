using JetBrains.Annotations;

namespace Systems.SimpleSettings.Settings.Audio
{
    /// <summary>Music volume channel setting.</summary>
    public sealed class MusicVolumeSetting : AudioMixerVolumeSetting
    {
        /// <inheritdoc/>
        protected override string MixerParameterName => "MusicVolume";

        /// <param name="mixer">The AudioMixer that exposes the <c>MusicVolume</c> parameter.</param>
        public MusicVolumeSetting([CanBeNull] UnityEngine.Audio.AudioMixer mixer) : base(mixer) { }
    }
}
