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
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private PlayerNamesPanel playerNamesPanel;
        [SerializeField] private CheckMark scoresAtEndCheck;
        [SerializeField] private Button startButton;

        private int maxRounds;
        private void OnEnable()
        {
            inputField.onValueChanged.AddListener(OnRoundValueChanged);
            startButton.onClick.AddListener(StartGame);
        }

        private void OnDisable()
        {
            inputField.onValueChanged.RemoveListener(OnRoundValueChanged);
            startButton.onClick.RemoveListener(StartGame);
        }

        private void Start()
        {
            startButton.interactable = false;
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

        private void OnDestroy()
        {
            playerNamesPanel.onFieldValueChanged -= CheckAllFields;
        }
    }
}
