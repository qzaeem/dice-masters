using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class BankrollSPSettings : MonoBehaviour
    {
        [SerializeField] private MainMenuCanvas mainMenu;
        [SerializeField] private GameModeBankrollBattleSP modeBankrollBattleSP;
        //[SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Dropdown maxRoundsDropDown;
        [SerializeField] private PlayerNamesPanel playerNamesPanel;
        [SerializeField] private CheckMark scoresAtEndCheck;
        [SerializeField] private Button startButton;

        private int maxRounds;
        private void OnEnable()
        {
            //inputField.onValueChanged.AddListener(OnRoundValueChanged);
            maxRoundsDropDown.onValueChanged.AddListener(UpdateMaxRounds);
            startButton.onClick.AddListener(StartGame);
        }

        private void OnDisable()
        {
            //inputField.onValueChanged.RemoveListener(OnRoundValueChanged);
            maxRoundsDropDown.onValueChanged.RemoveListener(UpdateMaxRounds);
            startButton.onClick.RemoveListener(StartGame);
        }

        private void Start()
        {
            startButton.interactable = false;
            //inputField.text = "";
            //default value
            maxRoundsDropDown.value = 1;
            playerNamesPanel.onFieldValueChanged += CheckAllFields;
        }
        private void DefaultDropdownValue()
        {
            maxRoundsDropDown.value = 1;
            maxRounds = maxRounds = int.Parse(maxRoundsDropDown.options[1].text); ;
        }
        //private void OnRoundValueChanged(string val)
        //{
        //    maxRounds = int.Parse(val);

        //    CheckAllFields();
        //}

        public void CheckAllFields()
        {
            startButton.interactable = maxRounds > 0 && playerNamesPanel.AllFieldsHaveNames();
        }

        public void StartGame()
        {
            if (!playerNamesPanel.AllFieldsHaveNames())
                return;

            playerNamesPanel.SetNamesSO();
            modeBankrollBattleSP.SetSettings(maxRounds, scoresAtEndCheck.currentValue);
            mainMenu.CreateGame();
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
            playerNamesPanel.onFieldValueChanged -= CheckAllFields;
            maxRoundsDropDown.onValueChanged.RemoveListener(UpdateMaxRounds);
        }
    }
}
