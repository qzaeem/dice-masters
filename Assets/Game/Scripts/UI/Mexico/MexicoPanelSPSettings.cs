using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DiceGame.UI
{
    public class MexicoPanelSPSettings : MonoBehaviour
    {
        [SerializeField] private MainMenuCanvas mainMenu;
        [SerializeField] private GameModeMexicoSP modeMexicoSP;
        [SerializeField] private PlayerNamesPanel playerNamesPanel;
        [SerializeField] private TMP_Dropdown maxLiveDropdown;
        [SerializeField] private Button startButton;
        private uint maxLive;

        private void OnEnable()
        {
            startButton.onClick.AddListener(StartGame);
            maxLiveDropdown.onValueChanged.AddListener(UpdateMaxLives);
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(StartGame);
            maxLiveDropdown.onValueChanged.RemoveListener(UpdateMaxLives);
        }

        private void Start()
        {
            SetupDropdownOptions();
        }
        private void SetupDropdownOptions()
        {
            //get the last selected max tiles
            maxLive = modeMexicoSP.MaxLives;
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
        public void StartGame()
        {
            if (!playerNamesPanel.AllFieldsHaveNames())
                return;

            modeMexicoSP.SetMaxLives = maxLive;
            mainMenu.playerCount = playerNamesPanel.numberOfPlayers;
            playerNamesPanel.SetNamesSO();
            mainMenu.CreateGame();
            gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            if (maxLiveDropdown != null)
                maxLiveDropdown.onValueChanged.RemoveListener(UpdateMaxLives);
        }
    }
}