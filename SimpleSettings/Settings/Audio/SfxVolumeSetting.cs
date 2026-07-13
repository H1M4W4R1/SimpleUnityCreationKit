using JetBrains.Annotations;

namespace Systems.SimpleSettings.Settings.Audio
{
    /// <summary>Sound-effects volume channel setting.</summary>
    public sealed class SfxVolumeSetting : AudioMixerVolumeSetting
    {
        /// <inheritdoc/>
        protected override string MixerParameterName => "SfxVolume";

        /// <param name="mixer">The AudioMixer that exposes the <c>SfxVolume</c> parameter.</param>
        public SfxVolumeSetting([CanBeNull] UnityEngine.Audio.AudioMixer mixer) : base(mixer) { }
    }
}
