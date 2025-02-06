using DiceGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;
namespace DiceGame.UI
{
    public class MexicoPanelMPSettings : MonoBehaviour
    {
        [SerializeField] private MainMenuCanvas mainMenu;
        [SerializeField] private GameModeMexico modeMexicoMP;
        [SerializeField] private PlayerNamesPanel playerNamesPanel;
        [SerializeField] private CheckMark reviveMexicoOption;
        [SerializeField] private Button nextButton;
        private void OnEnable()
        {
            nextButton.onClick.AddListener(NextMenu);
        }

        private void OnDisable()
        {
            nextButton.onClick.RemoveListener(NextMenu);
        }

        private void Start()
        {
            nextButton.interactable = false;
            playerNamesPanel.onFieldValueChanged += CheckAllFields;
        }

        public void CheckAllFields()
        {
            nextButton.interactable = playerNamesPanel.AllFieldsHaveNames();
        }

        public void NextMenu()
        {
            if (!playerNamesPanel.AllFieldsHaveNames())
                return;
            //set number of players count
            //mainMenu.playerCount = playerNamesPanel.numberOfPlayers;
            playerNamesPanel.SetNamesSO();
            mainMenu.OpenMenu(mainMenu.MPMenus.playerConnectionMenu);
        }
        private void OnDestroy()
        {
            playerNamesPanel.onFieldValueChanged -= CheckAllFields;
        }
    }
}