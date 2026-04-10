// Assets/Scripts/Proto/Selectable.cs
// Componente que marca qualquer GameObject como selecionável.
// Coloque em cada unidade. O círculo de seleção é um filho visual.
using UnityEngine;

namespace OPZ.Proto
{
    public class Selectable : MonoBehaviour
    {
        [Header("Visual")]
        public GameObject selectionCircle; // filho que liga/desliga

        public bool IsSelected { get; private set; }

        /// <summary>Chamado pelo SelectionManager.</summary>
        public void Select()
        {
            IsSelected = true;
            if (selectionCircle != null) selectionCircle.SetActive(true);
        }

        /// <summary>Chamado pelo SelectionManager.</summary>
        public void Deselect()
        {
            IsSelected = false;
            if (selectionCircle != null) selectionCircle.SetActive(false);
        }
    }
}
