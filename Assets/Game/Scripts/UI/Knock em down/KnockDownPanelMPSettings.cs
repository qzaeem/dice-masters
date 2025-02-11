using System;
using System.Net.NetworkInformation;
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
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Dropdown totalTilesDropDown;
        private uint maxRounds;
        private uint maxTiles;
        private void OnEnable()
        {
            inputField.text = "";
            nextButton.interactable = false;
            inputField.onValueChanged.AddListener(OnRoundValueChanged);
            nextButton.onClick.AddListener(NextMenu);
            totalTilesDropDown.onValueChanged.AddListener(UpdateNumberOfTiles);
        }

        private void OnDisable()
        {
            inputField.onValueChanged.RemoveListener(OnRoundValueChanged);
            nextButton.onClick.RemoveListener(NextMenu);
            totalTilesDropDown.onValueChanged.RemoveListener(UpdateNumberOfTiles);
        }

        private void Start()
        {
            SetupDropdownOptions();
        }
        private void SetupDropdownOptions()
        {
            //get the last selected max tiles
            maxTiles = modeTileKnockMP.TotalTiles;
            //check value index in drop down 
            int index = totalTilesDropDown.options.FindIndex(option => option.text == maxTiles.ToString());
            if (index != -1) totalTilesDropDown.value = index;
            totalTilesDropDown.RefreshShownValue();
        }

        private void UpdateNumberOfTiles(int index)
        {
            if (index >= 0 && index < totalTilesDropDown.options.Count)
            {
                maxTiles = uint.Parse(totalTilesDropDown.options[index].text);
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
            nextButton.interactable = maxRounds > 0 /* && playerNamesPanel.AllFieldsHaveNames()*/;
        }

        public void NextMenu()
        {
            modeTileKnockMP.MaxRounds = maxRounds;
            modeTileKnockMP.TotalTiles = maxTiles;
            //mainMenu.OpenMenu(mainMenu.MPMenus.playerConnectionMenu);
            mainMenu.OpenSelectedModeMenu();
        }
        private void OnDestroy()
        {
            totalTilesDropDown.onValueChanged.RemoveListener(UpdateNumberOfTiles);
        }
    }
}