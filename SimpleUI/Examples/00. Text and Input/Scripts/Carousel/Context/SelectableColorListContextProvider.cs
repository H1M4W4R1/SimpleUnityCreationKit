using System.Collections.Generic;
using Systems.SimpleUI.Context.Abstract;
using UnityEngine;

namespace Systems.SimpleUI.Examples._00._Text_and_Input.Scripts.Carousel.Context
{
    public sealed class SelectableColorListContextProvider : ContextProviderBase<SelectableColorListContext>
    {
        [field: SerializeField] private List<Color> Colors { get; set; } = new()
        {
            Color.red,
            Color.orange,
            Color.yellow,
            Color.limeGreen,
            Color.green,
            Color.darkGreen,
            Color.deepSkyBlue,
            Color.blue,
            Color.purple,
            Color.deepPink
        };
        
        private SelectableColorListContext _context;
        
        private void Awake()
        {
            _context = new SelectableColorListContext(Colors);
        }

        public override SelectableColorListContext GetContext() => _context;
    }
}