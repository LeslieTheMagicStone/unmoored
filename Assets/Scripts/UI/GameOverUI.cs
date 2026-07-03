using UnityEngine;

namespace CGJ2026
{
    public class GameOverUI : MonoBehaviour
    {
        public void OnRetryClicked()
        {
            GameManager.Instance.LoadGame();
        }

        public void OnMainMenuClicked()
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
}
