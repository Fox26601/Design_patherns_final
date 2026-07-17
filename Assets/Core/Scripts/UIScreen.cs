using UnityEngine;

namespace Core
{
    /// <summary>
    /// Base class for all menu screens.
    /// </summary>
    public abstract class UIScreen : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public virtual void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
