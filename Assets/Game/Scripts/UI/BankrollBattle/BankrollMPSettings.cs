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
        //[SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Dropdown maxRoundsDropDown;
        [SerializeField] private CheckMark scoresAtEndCheck;
        [SerializeField] private Button nextButton;
        private int maxRounds;
        private void OnEnable()
        {
            //inputField.text = "";
            //inputField.onValueChanged.AddListener(SetMaxRoundsValue);
            scoresAtEndCheck.defaultValue = modeBankrollBattleMP.ShowScoreOnEnd;
            maxRoundsDropDown.onValueChanged.AddListener(UpdateMaxRounds);
            nextButton.onClick.AddListener(NextMenu);
        }

        private void OnDisable()
        {
            //inputField.onValueChanged.RemoveListener(SetMaxRoundsValue);
            maxRoundsDropDown.onValueChanged.RemoveListener(UpdateMaxRounds);
            nextButton.onClick.RemoveListener(NextMenu);
        }

        private void Start()
        {
            //default value
            maxRoundsDropDown.value = 1;
            maxRounds = maxRounds = int.Parse(maxRoundsDropDown.options[1].text);
        }

        //private void SetMaxRoundsValue(string val)
        //{
        //    maxRounds = int.Parse(val);
        //    CheckAllFields();
        //}

        //public void CheckAllFields()
        //{
        //    nextButton.interactable = maxRounds > 0 /* && playerNamesPanel.AllFieldsHaveNames()*/;
        //}

        public void NextMenu()
        {
            modeBankrollBattleMP.MaxRounds = maxRounds;
            modeBankrollBattleMP.ShowScoreOnEnd = scoresAtEndCheck.currentValue;
            //mainMenu.OpenMenu(mainMenu.MPMenus.playerConnectionMenu);
            mainMenu.OpenSelectedModeMenu();
        }
        private void UpdateMaxRounds(int index)
        {
            if (index >= 0 && index < maxRoundsDropDown.options.Count)
            {
                maxRounds = int.Parse(maxRoundsDropDown.options[index].text);
            }
        }
        private void OnDestroy()
        {
            //inputField.onValueChanged.RemoveListener(SetMaxRoundsValue);
            maxRoundsDropDown.onValueChanged.RemoveListener(UpdateMaxRounds);
        }
    }
}
