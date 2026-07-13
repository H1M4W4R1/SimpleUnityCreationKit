using Systems.SimpleCore.Operations;

namespace Systems.SimpleWorld.Operations
{
    public static class WorldOperations
    {
        public const ushort SYSTEM_WORLD = 0x0019;
        public const ushort SUCCESS_WEATHER_ENABLED = 1;
        public const ushort SUCCESS_WEATHER_ALREADY_ENABLED = 2;
        public const ushort SUCCESS_WEATHER_DISABLED = 3;
        public const ushort ERROR_WEATHER_IS_NULL = 1;
        public const ushort ERROR_WEATHER_NOT_FOUND = 2;
        public const ushort ERROR_WEATHER_ENABLE_DENIED = 3;
        public const ushort ERROR_WEATHER_DISABLE_DENIED = 4;
        public const ushort ERROR_INVALID_DELTA_TIME = 5;
        public const ushort ERROR_WEATHER_NOT_ACTIVE = 6;

        public static OperationResult WeatherEnabled()
            => OperationResult.Success(SYSTEM_WORLD, SUCCESS_WEATHER_ENABLED);

        public static OperationResult WeatherAlreadyEnabled()
            => OperationResult.Success(SYSTEM_WORLD, SUCCESS_WEATHER_ALREADY_ENABLED);

        public static OperationResult WeatherDisabled()
            => OperationResult.Success(SYSTEM_WORLD, SUCCESS_WEATHER_DISABLED);

        public static OperationResult WeatherIsNull()
            => OperationResult.Error(SYSTEM_WORLD, ERROR_WEATHER_IS_NULL);

        public static OperationResult WeatherNotFound()
            => OperationResult.Error(SYSTEM_WORLD, ERROR_WEATHER_NOT_FOUND);

        public static OperationResult WeatherEnableDenied()
            => OperationResult.Error(SYSTEM_WORLD, ERROR_WEATHER_ENABLE_DENIED);

        public static OperationResult WeatherDisableDenied()
            => OperationResult.Error(SYSTEM_WORLD, ERROR_WEATHER_DISABLE_DENIED);

        public static OperationResult InvalidDeltaTime()
            => OperationResult.Error(SYSTEM_WORLD, ERROR_INVALID_DELTA_TIME);

        public static OperationResult WeatherNotActive()
            => OperationResult.Error(SYSTEM_WORLD, ERROR_WEATHER_NOT_ACTIVE);
    }
}
