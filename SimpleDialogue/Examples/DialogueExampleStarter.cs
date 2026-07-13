using Systems.SimpleDialogue.Components;
using Systems.SimpleDialogue.Utility;
using UnityEngine;

namespace Systems.SimpleDialogue.Examples
{
    /// <summary>
    ///     Starts the assigned dialogue when the example scene enters play mode.
    /// </summary>
    [RequireComponent(typeof(Dialogue))]
    public sealed class DialogueExampleStarter : MonoBehaviour
    {
        [SerializeField] private Dialogue _dialogue;

        private void Awake()
        {
            if (ReferenceEquals(_dialogue, null)) TryGetComponent(out _dialogue);
        }

        private void Start()
        {
            if (!_dialogue) return;
            DialogueAPI.Begin(_dialogue);
        }

        private void OnValidate()
        {
            if (ReferenceEquals(_dialogue, null)) _dialogue = GetComponent<Dialogue>();
        }
    }
}
