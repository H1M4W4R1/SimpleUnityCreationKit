using JetBrains.Annotations;

namespace Systems.SimpleSettings.Settings.Audio
{
    /// <summary>Voice / dialogue volume channel setting.</summary>
    public sealed class VoiceVolumeSetting : AudioMixerVolumeSetting
    {
        /// <inheritdoc/>
        protected override string MixerParameterName => "VoiceVolume";

        /// <param name="mixer">The AudioMixer that exposes the <c>VoiceVolume</c> parameter.</param>
        public VoiceVolumeSetting([CanBeNull] UnityEngine.Audio.AudioMixer mixer) : base(mixer) { }
    }
}
