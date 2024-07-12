using System;
using UnityEngine;

namespace UI
{
    public class BaseUIWindow : MonoBehaviour, IUIWindow
    {
        private UIManager _uiManager;

        public virtual void Init(object data)
        {
        }

        public virtual void Show(Action onShowComplete)
        {
            gameObject.SetActive(true);
            onShowComplete?.Invoke();
        }

        public virtual void Hide(Action onHideComplete)
        {
            onHideComplete?.Invoke();
            gameObject.SetActive(false);
        }

        protected UIManager UserInterfaceManager
        {
            get
            {
                if (_uiManager == null)
                {
                    _uiManager = GameControllers.ManagersOwner.GetManager<UIManager>();
                    if (_uiManager == null)
                    {
                        Debug.LogError($"Managers owner doesn't contain {nameof(UIManager)}");
                    }
                }
                return _uiManager;
            }
        }
    }
}
