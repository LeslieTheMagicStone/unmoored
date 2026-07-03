using UnityEngine;

namespace CGJ2026
{
    public class MainMenuUI : MonoBehaviour
    {
        public void OnPlayClicked()
        {
            GameManager.Instance.LoadGame();
        }

        public void OnQuitClicked()
        {
            Application.Quit();
        }
    }
}
