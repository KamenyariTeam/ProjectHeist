using System;

namespace UI
{
    public enum EUIType
    {
        None,
        MainMenu,
        SettingsMenu,
        HUD,
        PauseMenu
    }

    public interface IUIWindow
    {
        public void Init(object data);
        public void Show(Action onShowComplete);
        public void Hide(Action onHideComplete);
    }
}
