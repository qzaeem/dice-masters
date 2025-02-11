using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class BankrollMPSettings : MonoBehaviour
    {
        [SerializeField] private MainMenuCanvas mainMenu;
        [SerializeField] private GameModeBankrollBattle modeBankrollBattleMP;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private CheckMark scoresAtEndCheck;
        [SerializeField] private Button nextButton;
        private int maxRounds;
        private void OnEnable()
        {
            nextButton.interactable = false;
            inputField.text = "";
            inputField.onValueChanged.AddListener(SetMaxRoundsValue);
            nextButton.onClick.AddListener(NextMenu);
            scoresAtEndCheck.defaultValue = modeBankrollBattleMP.ShowScoreOnEnd;
        }

        private void OnDisable()
        {
            inputField.onValueChanged.RemoveListener(SetMaxRoundsValue);
            nextButton.onClick.RemoveListener(NextMenu);
        }

        private void Start()
        {
        }

        private void SetMaxRoundsValue(string val)
        {
            maxRounds = int.Parse(val);
            CheckAllFields();
        }

        public void CheckAllFields()
        {
            nextButton.interactable = maxRounds > 0 /* && playerNamesPanel.AllFieldsHaveNames()*/;
        }

        public void NextMenu()
        {
            modeBankrollBattleMP.MaxRounds = maxRounds;
            modeBankrollBattleMP.ShowScoreOnEnd = scoresAtEndCheck.currentValue;
            //mainMenu.OpenMenu(mainMenu.MPMenus.playerConnectionMenu);
            mainMenu.OpenSelectedModeMenu();
        }
        private void OnDestroy()
        {
            inputField.onValueChanged.RemoveListener(SetMaxRoundsValue);
        }
    }
}
