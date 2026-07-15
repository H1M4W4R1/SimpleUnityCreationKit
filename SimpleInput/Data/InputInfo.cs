using System;
using JetBrains.Annotations;

namespace Systems.SimpleInput.Data
{
    public readonly struct InputInfo
    {
        [NotNull] public readonly string deviceTypeName;
        [NotNull] public readonly Type deviceType;
        [NotNull] public readonly string pathPart;
        [CanBeNull] public readonly string displayName;
        [CanBeNull] public readonly string shortName;

        public InputInfo([NotNull] Type deviceType, [NotNull] string pathPart, [CanBeNull] string displayName, [CanBeNull] string shortName)
        {
            this.deviceType = deviceType;
            this.pathPart = pathPart;
            this.displayName = displayName;
            this.shortName = shortName;
            deviceTypeName = deviceType.Name.Replace("State", "");
            if (!this.pathPart.StartsWith('/')) this.pathPart = '/' + this.pathPart;
        }

        [CanBeNull] public string DisplayName => !string.IsNullOrEmpty(displayName) ? displayName : shortName;
        [CanBeNull] public string ShortName => !string.IsNullOrEmpty(shortName) ? shortName : displayName;
    }
}
