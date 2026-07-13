using JetBrains.Annotations;
using Systems.SimpleCore.Storage.Databases;

namespace Systems.SimpleWorld.Data
{
    /// <summary>
    ///     Addressable database containing all weather effect configurations.
    /// </summary>
    public sealed class WeatherEffectDatabase : AddressableDatabase<WeatherEffectDatabase, WeatherEffect>
    {
        public const string LABEL = "SimpleWorld.WeatherEffects";

        [NotNull] protected override string AddressableLabel => LABEL;
    }
}
