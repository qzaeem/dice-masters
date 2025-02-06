using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DiceGame.ScriptableObjects.GameModeGreed;
namespace DiceGame.UI
{
    public class GreedPanelMPSettings : MonoBehaviour
    {
        [SerializeField] private MainMenuCanvas mainMenu;
        [SerializeField] private GameModeGreed gameModeGreedMP;
        [SerializeField] private PlayerNamesPanel playerNamesPanel;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Dropdown selectScoreDropDown;
        int scoreValue;
        private void OnEnable()
        {
            nextButton.onClick.AddListener(NextMenu);
            selectScoreDropDown.onValueChanged.AddListener(UpdateWinningScoreOption);
        }

        private void OnDisable()
        {
            nextButton.onClick.RemoveListener(NextMenu);
            selectScoreDropDown.onValueChanged.RemoveListener(UpdateWinningScoreOption);
        }

        private void Start()
        {
            //nextButton.interactable = false;
            //playerNamesPanel.onFieldValueChanged += CheckAllFields;
            SetupDropdownOptions();
        }
        private void SetupDropdownOptions()
        {
            //get the last selected score value
            scoreValue = (int)gameModeGreedMP.MaxWinningScore;
            //check value index in drop down 
            int index = selectScoreDropDown.options.FindIndex(option => option.text == scoreValue.ToString());
            if (index != -1) selectScoreDropDown.value = index;
            selectScoreDropDown.RefreshShownValue();
        }

        private void UpdateWinningScoreOption(int index)
        {
           if(index >= 0 && index < selectScoreDropDown.options.Count)
            {
                scoreValue = int.Parse(selectScoreDropDown.options[index].text);
            }
        }
        //public void CheckAllFields()
        //{
        //    nextButton.interactable = playerNamesPanel.AllFieldsHaveNames();
        //}

        public void NextMenu()
        {
            //after changes
            //if (!playerNamesPanel.AllFieldsHaveNames())
            //    return;
            //set number of players count
            //mainMenu.playerCount = playerNamesPanel.numberOfPlayers;
            //playerNamesPanel.SetNamesSO();
            CheckWinningScoreVal(scoreValue);
            mainMenu.OpenMenu(mainMenu.MPMenus.playerConnectionMenu);
        }
        private void OnDestroy()
        {
            //playerNamesPanel.onFieldValueChanged -= CheckAllFields;
            selectScoreDropDown.onValueChanged.RemoveListener(UpdateWinningScoreOption);
        }
        private void CheckWinningScoreVal(int val)
        {
            if(System.Enum.IsDefined(typeof(WinningScore), val))
            {
                gameModeGreedMP.MaxWinningScore = (WinningScore)val;
            }
        }
    }
}