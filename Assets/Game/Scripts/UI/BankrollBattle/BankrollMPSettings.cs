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
        [SerializeField] private PlayerNamesPanel playerNamesPanel;
        [SerializeField] private CheckMark scoresAtEndCheck;
        [SerializeField] private Button nextButton;
        private int maxRounds;
        private void OnEnable()
        {
            inputField.onValueChanged.AddListener(OnRoundValueChanged);
            nextButton.onClick.AddListener(NextMenu);
        }

        private void OnDisable()
        {
            inputField.onValueChanged.RemoveListener(OnRoundValueChanged);
            nextButton.onClick.RemoveListener(NextMenu);
        }

        private void Start()
        {
            nextButton.interactable = false;
            inputField.text = "";
            playerNamesPanel.onFieldValueChanged += CheckAllFields;
        }

        private void OnRoundValueChanged(string val)
        {
            maxRounds = int.Parse(val);

            CheckAllFields();
        }

        public void CheckAllFields()
        {
            nextButton.interactable = maxRounds > 0 && playerNamesPanel.AllFieldsHaveNames();
        }

        public void NextMenu()
        {
            if (!playerNamesPanel.AllFieldsHaveNames())
                return;
            //set number of players count
            mainMenu.playerCount = playerNamesPanel.numberOfPlayers;
            playerNamesPanel.SetNamesSO();
            //modeBankrollBattleMP.SetSettingsFromJson(maxRounds, scoresAtEndCheck.currentValue);
            mainMenu.OpenMenu(mainMenu.MPMenus.playerConnectionMenu);
        }
        private void OnDestroy()
        {
            playerNamesPanel.onFieldValueChanged -= CheckAllFields;
        }
    }
}
