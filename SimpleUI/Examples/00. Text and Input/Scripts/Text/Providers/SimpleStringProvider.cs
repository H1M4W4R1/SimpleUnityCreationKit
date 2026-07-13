using Systems.SimpleUI.Context.Abstract;
using UnityEngine;

namespace Systems.SimpleUI.Examples._00._Text_and_Input.Scripts.Text.Providers
{
    /// <summary>
    ///     Provides a simple string to be displayed
    /// </summary>
    public sealed class SimpleStringProvider : ContextProviderBase<string>
    {
        [SerializeField] private string stringToProvide;

        public override string GetContext() => stringToProvide;
    }
}