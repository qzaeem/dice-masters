using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DiceGame.UI
{
    public class KncokDownPanelSPSettings : MonoBehaviour
    {
        [SerializeField] private MainMenuCanvas mainMenu;
        [SerializeField] private GameModeTileKnockSP modeTileKnockSP;
        [SerializeField] private PlayerNamesPanel playerNamesPanel;
        [SerializeField] private TMP_Dropdown totalTilesDropDown, roundsDropdown;
        [SerializeField] private Button startButton;
        private uint maxRounds;
        private uint maxTiles;
        private void OnEnable()
        {
            startButton.onClick.AddListener(StartGame);
            totalTilesDropDown.onValueChanged.AddListener(UpdateNumberOfTiles);
            roundsDropdown.onValueChanged.AddListener(UpdateRoundValue);
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(StartGame);
            totalTilesDropDown.onValueChanged.RemoveListener(UpdateNumberOfTiles);
            roundsDropdown.onValueChanged.RemoveListener(UpdateRoundValue);
        }

        private void Start()
        {
            SetupDropdownOptions();
        }
        private void SetupDropdownOptions()
        {
            //get the last selected max tiles
            maxTiles = modeTileKnockSP.TotalTiles;
            maxRounds = modeTileKnockSP.MaxRounds;
            SetDropDownOption(roundsDropdown, maxRounds);
            SetTilesDropDownOption();
        }
        private void SetDropDownOption(TMP_Dropdown dropdown, uint value)
        {
            //check value index in drop down 
            int index = dropdown.options.FindIndex(option => option.text == value.ToString());
            if (index != -1) dropdown.value = index;
            dropdown.RefreshShownValue();
        }
        private void SetTilesDropDownOption()
        {
            int index = 0;
            if (maxTiles == 9) index = 0; // 3x3 
            else if (maxTiles == 16) index = 2; // 4x4
            else if (maxTiles == 25) index = 3; // 5x5
            totalTilesDropDown.value = index;
            totalTilesDropDown.RefreshShownValue();
        }
        private void UpdateNumberOfTiles(int index)
        {
            if (index >= 0 && index < totalTilesDropDown.options.Count)
            {
                if (index == 0) maxTiles = 9; // 3x3 
                else if (index == 1) maxTiles = 9; // 3x4
                else if (index == 2) maxTiles = 16; // 4x4
                else if (index == 3) maxTiles = 25; // 5x5
            }
        }

        private void UpdateRoundValue(int index)
        {
            if (index >= 0 && index < roundsDropdown.options.Count)
            {
                maxRounds = uint.Parse(roundsDropdown.options[index].text);
            }
        }

        public void StartGame()
        {
            if (!playerNamesPanel.AllFieldsHaveNames())
                return;
            modeTileKnockSP.MaxRounds = maxRounds;
            modeTileKnockSP.TotalTiles = maxTiles;
            mainMenu.playerCount = playerNamesPanel.numberOfPlayers;
            playerNamesPanel.SetNamesSO();
            mainMenu.CreateGame();
            gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            if (totalTilesDropDown != null)
                totalTilesDropDown.onValueChanged.RemoveListener(UpdateNumberOfTiles);
        }
    }
}