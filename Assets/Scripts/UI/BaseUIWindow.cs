using System;
using UnityEngine;

namespace UI
{
    public class BaseUIWindow : MonoBehaviour, IUIWindow
    {
        protected UIManager _uiManager;

        [SerializeField] private string managersOwnerTag;

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

        protected virtual void Awake()
        {
            GameObject managersOwner = GameObject.FindGameObjectWithTag(managersOwnerTag);
            if (managersOwner == null)
            {
                Debug.LogError($"Can't find owner of managers on the scene, tag: {managersOwnerTag}");
                return;
            }

            _uiManager = managersOwner.GetComponent<UIManager>();
            if (_uiManager == null)
            {
                Debug.LogError($"Managers owner doesn't contain {nameof(UIManager)}");
            }
        }
    }
}
