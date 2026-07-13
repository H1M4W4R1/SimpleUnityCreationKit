using System.Collections.Generic;
using Systems.SimpleUI.Context.Abstract;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.SimpleUI.Examples._00._Text_and_Input.Scripts.Dropdown.Context
{
    public sealed class ExampleSelectableFloatArrayContextProvider : ContextProviderBase<FloatArrayListSelectableContext>
    {
        [SerializeField] private List<float> _floats = new();
        private FloatArrayListSelectableContext _context;

        private void Awake()
        {
            _context = new FloatArrayListSelectableContext(_floats);
        }

        [ContextMenu("Add Float")]
        internal void AddFloat()
        {
            _floats.Add(Random.Range(0f, 100f));
        }

        [ContextMenu("Remove At Random Index")] internal void RemoveAtRandomIndex()
        {
            int index = Random.Range(0, _floats.Count);
            if (_floats.Count > index) _floats.RemoveAt(index);
        }

        public override FloatArrayListSelectableContext GetContext()
        {
            return _context;
        }

        private void Update()
        {
            if(UnityEngine.Input.GetKeyDown(KeyCode.Space)) AddFloat();
            if(UnityEngine.Input.GetKeyDown(KeyCode.Delete)) RemoveAtRandomIndex();
        }
    }
}