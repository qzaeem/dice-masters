using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        public Menu previousMenu { get; set; }

        private void OnEnable()
        {
            backButton?.onClick.AddListener(GoBack);
        }

        private void OnDisable()
        {
            backButton?.onClick.RemoveListener(GoBack);
        }

        public void OpenMenu(bool open)
        {
            gameObject.SetActive(open);
        }

        public void GoBack()
        {
            if (previousMenu == null)
                return;

            OpenMenu(false);
            previousMenu.OpenMenu(true);
            //update current menu
            FindObjectOfType<MainMenuCanvas>().SetCurrentMenu(previousMenu);

        }
    }
}
