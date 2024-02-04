using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private EUIType[] windowTypes;
        [SerializeField] private GameObject[] windowPrefabs;

        [SerializeField] private EUIType initialMenu;

        private readonly Stack<IUIWindow> _openedWindows = new Stack<IUIWindow>();
        private bool _isPerformingOperation = false;

        public void PushUI(EUIType windowType, object param = null)
        {
            if (windowType == EUIType.None)
            {
                return;
            }

            if (!TryStartOperation())
            {
                Debug.Log("Can't push UI window, because another operation is in progress");
                return;
            }
            int index = Array.FindIndex(windowTypes, 0, windowTypes.Length, (type) => type == windowType);
            if (index == -1)
            {
                EndOperation();
                return;
            }

            GameObject prefab = windowPrefabs[index];

            if (!_openedWindows.TryPeek(out IUIWindow currentWindow))
            {
                OnBottomWindowHidden(prefab, param);
                return;
            }

            currentWindow.Hide(() => OnBottomWindowHidden(prefab, param));
        }

        public void PopUI()
        {
            if (!TryStartOperation())
            {
                Debug.Log("Can't pop UI window, because another operation is in progress");
                return;
            }

            if (!_openedWindows.TryPop(out IUIWindow previousWindow))
            {
                EndOperation();
                Debug.LogWarning("Tried to pop window, when the UI stack is empty");
                return;
            }

            previousWindow.Hide(() => OnTopWindowHidden(previousWindow));
        }

        private void Start()
        {
            Assert.AreEqual(windowTypes.Length, windowPrefabs.Length);
            
            PushUI(initialMenu);
        }

        private bool TryStartOperation()
        {
            if (_isPerformingOperation)
            {
                return false;
            }

            _isPerformingOperation = true;
            return true;
        }

        private void EndOperation()
        {
            _isPerformingOperation = false;
        }

        private void OnBottomWindowHidden(GameObject prefab, object param)
        {
            GameObject newWindowObject = Instantiate(prefab);
            IUIWindow window = newWindowObject.GetComponent<IUIWindow>();
            if (window == null)
            {
                EndOperation();
                Debug.LogError($"The pushed UI window prefab has no {nameof(IUIWindow)} component");
                return;
            }
            _openedWindows.Push(window);
            window.Init(param);
            window.Show(EndOperation);
        }

        private void OnTopWindowHidden(IUIWindow previousWindow)
        {
            var windowScript = previousWindow as MonoBehaviour;
            if (windowScript != null)
            {
                Destroy(windowScript.gameObject);
            }
            else
            {
                Debug.LogWarning($"The hidden UI window was not a {nameof(MonoBehaviour)} script");
            }

            if (!_openedWindows.TryPeek(out IUIWindow newWindow))
            {
                EndOperation();
                return;
            }
            newWindow.Show(EndOperation);
        }
    }
}

