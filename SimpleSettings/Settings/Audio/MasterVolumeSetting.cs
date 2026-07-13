using JetBrains.Annotations;
using UnityEngine.Audio;

namespace Systems.SimpleSettings.Settings.Audio
{
    /// <summary>Master volume channel setting.</summary>
    public sealed class MasterVolumeSetting : AudioMixerVolumeSetting
    {
        /// <inheritdoc/>
        protected override string MixerParameterName => "MasterVolume";

        /// <param name="mixer">The AudioMixer that exposes the <c>MasterVolume</c> parameter. May be <c>null</c>.</param>
        public MasterVolumeSetting([CanBeNull] AudioMixer mixer) : base(mixer) { }
    }
}
