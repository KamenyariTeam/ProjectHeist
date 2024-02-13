
namespace UI
{
    public class PauseMenuScript : BaseUIWindow
    {
        public void OnPressResume()
        {
            _uiManager.PopUI();
        }

        public void OnPressMainMenu()
        {
        }

    }
}