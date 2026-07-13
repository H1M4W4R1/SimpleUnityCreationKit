using JetBrains.Annotations;
using Systems.SimpleEconomy.Currencies;
using UnityEngine;

namespace Systems.SimpleEconomy.Wallets
{
    /// <summary>
    ///     Currency wallet that is shared across the entire game
    /// </summary>
    public abstract class GlobalCurrencyWalletBase<TSelf, TCurrencyType> : CurrencyWalletBase<TCurrencyType>
        where TSelf : GlobalCurrencyWalletBase<TSelf, TCurrencyType>
        where TCurrencyType : CurrencyBase, new()
    {
        private static TSelf _instance;

        /// <summary>
        ///     Instance of the global currency wallet
        /// </summary>
        [NotNull] public static TSelf Instance
        {
            get
            {
                if (_instance) return _instance;
                _instance = FindAnyObjectByType<TSelf>(FindObjectsInactive.Include);
                if (!_instance)
                {
                    _instance = new GameObject($"Global Currency Wallet [{typeof(TCurrencyType).Name}]")
                        .AddComponent<TSelf>();
                    if (Application.isPlaying) DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }
    }
}
