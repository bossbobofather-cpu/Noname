using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Common.UI
{    
    public class UIBase : MonoBehaviour
    {
        private bool _collecteButtons = false;
        private List<Button> _buttons = new();

        public List<Button> Buttons => _buttons;


        private void Awake()
        {
            CollecteButtons();
        }

        private void OnEnable()
        {
            CollecteButtons();
        }

        public void CollecteButtons()
        {
            if(_collecteButtons) return;

            _buttons.Clear();
            _collecteButtons = true;
            
            GetComponentsInChildren(true, _buttons);
        }
    }
}