using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class KnockDownPanelMPSettings : MonoBehaviour
    {
        [SerializeField] private MainMenuCanvas mainMenu;
        [SerializeField] private GameModeTileKnock modeTileKnockMP;
        [SerializeField] private PlayerNamesPanel playerNamesPanel;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Dropdown totalTilesDropDown;
        private uint maxRounds;
        private void OnEnable()
        {
            inputField.onValueChanged.AddListener(OnRoundValueChanged);
            nextButton.onClick.AddListener(NextMenu);
            totalTilesDropDown.onValueChanged.AddListener(OnTilesValueChange);
        }

        private void OnDisable()
        {
            inputField.onValueChanged.RemoveListener(OnRoundValueChanged);
            nextButton.onClick.RemoveListener(NextMenu);
            totalTilesDropDown.onValueChanged.RemoveListener(OnTilesValueChange);
        }

        private void Start()
        {
            nextButton.interactable = false;
            inputField.text = "";
            playerNamesPanel.onFieldValueChanged += CheckAllFields;
            SetupDropdownOptions();
        }
        private void SetupDropdownOptions()
        {
            totalTilesDropDown.value = 0;
            totalTilesDropDown.RefreshShownValue();
        }

        private void OnTilesValueChange(int val)
        {
            //TODO

            switch (val)
            {
                case 0:
                    modeTileKnockMP.totalTiles = 9;
                    break;
                case 1:
                    modeTileKnockMP.totalTiles = 12;
                    break;
                case 2:
                    modeTileKnockMP.totalTiles = 16;
                    break;
                case 3:
                    modeTileKnockMP.totalTiles = 25;
                    break;
            }
        }

        private void OnRoundValueChanged(string val)
        {
            maxRounds = uint.Parse(val);
            modeTileKnockMP.maxRounds = maxRounds;
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
            //mainMenu.playerCount = playerNamesPanel.numberOfPlayers;
            playerNamesPanel.SetNamesSO();
            mainMenu.OpenMenu(mainMenu.MPMenus.playerConnectionMenu);
        }
        private void OnDestroy()
        {
            playerNamesPanel.onFieldValueChanged -= CheckAllFields;
            totalTilesDropDown.onValueChanged.RemoveListener(OnTilesValueChange);
        }
    }
}