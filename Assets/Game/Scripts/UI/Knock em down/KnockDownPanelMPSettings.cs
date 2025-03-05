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
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Dropdown totalTilesDropDown, roundsDropdown;
        private uint maxRounds;
        private uint maxTiles;
        private void OnEnable()
        {
            nextButton.onClick.AddListener(NextMenu);
            totalTilesDropDown.onValueChanged.AddListener(UpdateNumberOfTiles);
            roundsDropdown.onValueChanged.AddListener(UpdateRoundValue);
        }

        private void OnDisable()
        {
            nextButton.onClick.RemoveListener(NextMenu);
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
            maxTiles = modeTileKnockMP.TotalTiles;
            maxRounds = modeTileKnockMP.MaxRounds;
            SetDropDownOption(roundsDropdown, maxRounds);
            SetDropDownOption(totalTilesDropDown, maxTiles);
        }
        private void SetDropDownOption(TMP_Dropdown dropdown, uint value)
        {
            //check value index in drop down 
            int index = totalTilesDropDown.options.FindIndex(option => option.text == value.ToString());
            if (index != -1) totalTilesDropDown.value = index;
            totalTilesDropDown.RefreshShownValue();
        }
        private void UpdateNumberOfTiles(int index)
        {
            if (index >= 0 && index < totalTilesDropDown.options.Count)
            {
                if (index == 0) maxTiles = 9; // 3x3 
                else if (index == 0) maxTiles = 9; // 3x4
                else if (index == 0) maxTiles = 16; // 4x4
                else if (index == 0) maxTiles = 25; // 5x5
            }
        }

        private void UpdateRoundValue(int index)
        {
            if (index >= 0 && index < roundsDropdown.options.Count)
            {
                maxRounds = uint.Parse(roundsDropdown.options[index].text);
            }
        }

        public void CheckAllFields()
        {
            nextButton.interactable = maxRounds > 0 /* && playerNamesPanel.AllFieldsHaveNames()*/;
        }

        public void NextMenu()
        {
            modeTileKnockMP.MaxRounds = maxRounds;
            modeTileKnockMP.TotalTiles = maxTiles;
            mainMenu.OpenSelectedModeMenu();
        }
        private void OnDestroy()
        {
            totalTilesDropDown.onValueChanged.RemoveListener(UpdateNumberOfTiles);
        }
    }
}