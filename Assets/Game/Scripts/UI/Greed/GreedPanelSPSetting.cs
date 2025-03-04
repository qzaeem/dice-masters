using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DiceGame.UI
{
    public class GreedPanelSPSetting : MonoBehaviour
    {
        [SerializeField] private MainMenuCanvas mainMenu;
        [SerializeField] private GameModeGreedSP gameModeGreedSP;
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
            SetupDropdownOptions();
        }
        private void SetupDropdownOptions()
        {
            //default 
            selectScoreDropDown.value = 1;
            scoreValue = 10000; // normal game 
        }

        private void UpdateWinningScoreOption(int index)
        {
            if (index >= 0 && index < selectScoreDropDown.options.Count)
            {
                scoreValue = int.Parse(selectScoreDropDown.options[index].text);
            }
        }

        public void NextMenu()
        {
            CheckWinningScoreVal(scoreValue);
            gameObject.SetActive(false);
            mainMenu.CreateGame();
        }
        private void OnDestroy()
        {
            selectScoreDropDown.onValueChanged.RemoveListener(UpdateWinningScoreOption);
        }
        private void CheckWinningScoreVal(int val)
        {
            if (System.Enum.IsDefined(typeof(GameModeGreedSP.WinningScore), val))
            {
                gameModeGreedSP.MaxWinningScore = (GameModeGreedSP.WinningScore)val;
            }
        }
    }
}