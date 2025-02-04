using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DiceGame.UI
{
    public class GreedPanelMPSettings : MonoBehaviour
    {
        [SerializeField] private MainMenuCanvas mainMenu;
        [SerializeField] private GameModeGreed gameModeGreedMP;
        [SerializeField] private PlayerNamesPanel playerNamesPanel;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Dropdown dropDown;

        private void OnEnable()
        {
            nextButton.onClick.AddListener(NextMenu);
            dropDown.onValueChanged.AddListener(OnScoreOption);
        }

        private void OnDisable()
        {
            nextButton.onClick.RemoveListener(NextMenu);
            dropDown.onValueChanged.RemoveListener(OnScoreOption);
        }

        private void Start()
        {
            nextButton.interactable = false;
            playerNamesPanel.onFieldValueChanged += CheckAllFields;
            SetupDropdownOptions();
        }
        private void SetupDropdownOptions()
        {
            dropDown.value = 0;
            dropDown.RefreshShownValue();
        }

        private void OnScoreOption(int val)
        {
            //TODO

            //    switch (val)
            //    {
            //        case 0:
            //            gameModeGreedMP.winningScore = WinningScore.ShortGame;
            //            break;
            //        case 1:
            //            gameModeGreedMP.winningScore = WinningScore.NormalGame;
            //            break;
            //        case 2:
            //            gameModeGreedMP.winningScore = WinningScore.LongGame;
            //            break;
            //        case 3:
            //            gameModeGreedMP.winningScore = WinningScore.SuperLongGame;
            //            break;
            //    }
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
            mainMenu.playerCount = playerNamesPanel.numberOfPlayers;
            playerNamesPanel.SetNamesSO();
            mainMenu.OpenMenu(mainMenu.MPMenus.playerConnectionMenu);
        }
        private void OnDestroy()
        {
            playerNamesPanel.onFieldValueChanged -= CheckAllFields;
            dropDown.onValueChanged.RemoveListener(OnScoreOption);
        }
    }
}