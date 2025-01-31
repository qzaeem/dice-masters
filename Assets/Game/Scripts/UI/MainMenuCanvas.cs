using System.Collections.Generic;
using DiceGame.Network;
using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class MainMenuCanvas : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button singleDeviceButton;
        [SerializeField] private Button multiDeviceButton;
        [SerializeField] private Button greedModeButton;
        [SerializeField] private Button battleModeButton;
        [SerializeField] private Button mexicoModeButton;
        [SerializeField] private Button knockDownModeButton;

        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private GameObject loadingMenu;
        [SerializeField] private Menu nameMenu, modeSelectionMenu, devicesSelectionMenu, bankrollMenuSP;
        [Header("Multiplayer Modes Settings")]
        [SerializeField] private Menu bankrollMenuMP;
        [SerializeField] private PlayerInfoVariable playerInfo;
        [SerializeField] private GameModeVariable currentGameMode;
        [SerializeField] private List<GameModeBase> gameModes;
        private Menu currentMenu;
        private bool isMultiDevice;

        private void OnEnable()
        {
            startButton.onClick.AddListener(OpenDeviceSelectionPanel);
            singleDeviceButton.onClick.AddListener(() => SelectIsMultiplayer(false));
            multiDeviceButton.onClick.AddListener(() => SelectIsMultiplayer(true));
            battleModeButton.onClick.AddListener(() => SelectGameModeType(GameModeName.BankrollBattle));
            greedModeButton.onClick.AddListener(() => SelectGameModeType(GameModeName.Greed));
            mexicoModeButton.onClick.AddListener(() => SelectGameModeType(GameModeName.Mexico));
            knockDownModeButton.onClick.AddListener(() => SelectGameModeType(GameModeName.KnockEmDown));
            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(OpenDeviceSelectionPanel);
            singleDeviceButton.onClick.RemoveAllListeners();
            multiDeviceButton.onClick.RemoveAllListeners();
            battleModeButton.onClick.RemoveAllListeners();
            greedModeButton.onClick.RemoveAllListeners();
            mexicoModeButton.onClick.RemoveAllListeners();
            knockDownModeButton.onClick.RemoveAllListeners();
            nameInputField.onValueChanged.RemoveListener(OnNameChanged);
        }

        private void Start()
        {
            startButton.interactable = false;
            OpenMenu(nameMenu);
        }

        public void OnNameChanged(string value)
        {
            startButton.interactable = value.Length > 0 && value.Length < 17 && !string.IsNullOrWhiteSpace(value);
        }

        private void SelectGameModeType(GameModeName gameMode)
        {
            GameModeBase selectedGameMode = null;

            foreach (var mode in gameModes)
            {
                if (mode.mode == gameMode && mode.isMultiplayer == isMultiDevice)
                {
                    selectedGameMode = mode;
                    break;
                }
            }

            currentGameMode.Set(selectedGameMode);

            if (selectedGameMode != null)
            {
                if (currentGameMode.value.isMultiplayer)
                {
                    switch (currentGameMode.value.mode)
                    {
                        case GameModeName.BankrollBattle:
                            OpenMenu(bankrollMenuMP);
                            break;
                        case GameModeName.Greed:
                            break;
                        case GameModeName.Mexico:
                            break;
                        case GameModeName.KnockEmDown:
                            break;
                    }
                    //StartGame(); // TODO Remove
                }
                else
                {
                    switch (currentGameMode.value.mode)
                    {
                        case GameModeName.BankrollBattle:
                            OpenMenu(bankrollMenuSP);
                            break;
                        case GameModeName.Greed:
                            break;
                        case GameModeName.Mexico:
                            break;
                        case GameModeName.KnockEmDown:
                            break;
                    }
                }
            }
        }

        private void SelectIsMultiplayer(bool isMultiDevice)
        {
            this.isMultiDevice = isMultiDevice;
            OpenMenu(modeSelectionMenu);
        }

        private void OpenDeviceSelectionPanel()
        {
            playerInfo.value.playerName = nameInputField.text;
            OpenMenu(devicesSelectionMenu);
        }

        private void OpenMenu(Menu menu)
        {
            if (currentMenu != null)
            {
                menu.previousMenu = currentMenu;
                currentMenu.OpenMenu(false);
            }
            currentMenu = menu;
            currentMenu.OpenMenu(true);
        }

        public void StartGame()
        {
            GameManager.isSinglePlayerMode = !currentGameMode.value.isMultiplayer;
            loadingMenu.SetActive(true);
            gameObject.SetActive(false);
            NetworkManager.Instance.StartGame();
        }
    }
}
