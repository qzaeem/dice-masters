using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DiceGame.UI
{
    public class MexicoPanelMPSettings : MonoBehaviour
    {
        [SerializeField] private MainMenuCanvas mainMenu;
        [SerializeField] private GameModeMexico modeMexicoMP;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Dropdown maxLiveDropdown;
        uint maxLive;

        private void OnEnable()
        {
            nextButton.onClick.AddListener(NextMenu);
            maxLiveDropdown.onValueChanged.AddListener(UpdateMaxLives);
        }

        private void OnDisable()
        {
            nextButton.onClick.RemoveListener(NextMenu);
            maxLiveDropdown.onValueChanged.RemoveListener(UpdateMaxLives);
        }

        private void Start()
        {
            SetupDropdownOptions();
        }
        private void SetupDropdownOptions()
        {
            //get the last selected max tiles
            maxLive = modeMexicoMP.MaxLives;
            //check value index in drop down 
            int index = maxLiveDropdown.options.FindIndex(option => option.text == maxLive.ToString());
            if (index != -1) maxLiveDropdown.value = index;
            maxLiveDropdown.RefreshShownValue();
        }

        private void UpdateMaxLives(int index)
        {
            if (index >= 0 && index < maxLiveDropdown.options.Count)
            {
                maxLive = uint.Parse(maxLiveDropdown.options[index].text);
            }
        }
        public void NextMenu()
        {
            modeMexicoMP.SetMaxLives = maxLive;
            mainMenu.OpenSelectedModeMenu();
        }
        private void OnDestroy()
        {
            maxLiveDropdown.onValueChanged.RemoveListener(UpdateMaxLives);
        }
    }
}