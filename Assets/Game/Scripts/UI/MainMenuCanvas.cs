using System.Collections.Generic;
using System.Linq;
using DiceGame.Network;
using DiceGame.ScriptableObjects;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    [System.Serializable]
    public class MultiplayerMenus
    {
        public Menu bankrollMenuMP, greedMenuMP, mexicoMenuMP, knockDownMenuMP, playerConnectionMenu, randomMatch, createOrJoinRoom, createRoom, JoinRoom, LobbyMenu;
    }
    public class MainMenuCanvas : MonoBehaviour
    {
        public static MainMenuCanvas instance;
        [SerializeField] private Button startButton;
        [SerializeField] private Button singleDeviceButton;
        [SerializeField] private Button multiDeviceButton;
        [SerializeField] private Button greedModeButton;
        [SerializeField] private Button battleModeButton;
        [SerializeField] private Button mexicoModeButton;
        [SerializeField] private Button knockDownModeButton;
        [SerializeField] private Button diceSelectionButton;

        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private GameObject loadingMenu;
        [SerializeField] private Menu nameMenu, diceSelectionMenu, modeSelectionMenu, devicesSelectionMenu, bankrollMenuSP, greedModeMenuSP;
        [SerializeField] private PlayerInfoVariable playerInfo;
        [SerializeField] private GameModeVariable currentGameMode;
        [SerializeField] private List<GameModeBase> gameModes;
        [SerializeField] private TextMeshProUGUI alertText;

        //--- New ---
        [Header("Multiplayer Mode Menus")]
        public MultiplayerMenus MPMenus;
        public int playerCount;
        bool isRandomMatchSelected;
        string roomKey;

        private Menu currentMenu;
        private bool isMultiDevice;

        private void Awake()
        {
            instance = this;
            Application.runInBackground = true;
        }
        private void OnEnable()
        {
            startButton.onClick.AddListener(OpenDeviceSelectionPanel);
            singleDeviceButton.onClick.AddListener(() => SelectIsMultiplayer(false));
            multiDeviceButton.onClick.AddListener(() => SelectIsMultiplayer(true));
            battleModeButton.onClick.AddListener(() => SelectGameModeType(GameModeName.BankrollBattle));
            greedModeButton.onClick.AddListener(() => SelectGameModeType(GameModeName.Greed));
            mexicoModeButton.onClick.AddListener(() => SelectGameModeType(GameModeName.Mexico));
            knockDownModeButton.onClick.AddListener(() => SelectGameModeType(GameModeName.KnockEmDown));
            diceSelectionButton.onClick.AddListener(() => OpenMenu(diceSelectionMenu));
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
            diceSelectionButton.onClick.RemoveAllListeners();
            nameInputField.onValueChanged.RemoveListener(OnNameChanged);
            NetworkManager.Instance.OnJoinFailed -= onJoinFailed;
        }
        private void Start()
        {
            alertText.text = "";
            alertText.gameObject.SetActive(false);
            NetworkManager.Instance.OnJoinFailed += onJoinFailed;
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
                            //--- Open bankroll menu multiplayer ---
                            OpenMenu(MPMenus.bankrollMenuMP);
                            break;
                        case GameModeName.Greed:
                            OpenMenu(MPMenus.greedMenuMP);
                            break;
                        case GameModeName.Mexico:
                            OpenMenu(MPMenus.mexicoMenuMP);
                            break;
                        case GameModeName.KnockEmDown:
                            OpenMenu(MPMenus.knockDownMenuMP);
                            break;
                    }
                    //--- open mode settings instead of starting the game for multiplayer ---
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
                            OpenMenu(greedModeMenuSP);
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
            //New change
            if (!isMultiDevice)
                OpenMenu(modeSelectionMenu);
            else
                OpenMenu(MPMenus.playerConnectionMenu);
            //--- Set scene index ---
        }
        //New
        public void OpenModeSelectionMenu(bool isRandomMatch)
        {
            isRandomMatchSelected = isRandomMatch;
            OpenMenu(modeSelectionMenu);
        }
        public void OpenSelectedModeMenu()
        {
            if (isRandomMatchSelected)
                OpenMenu(MPMenus.randomMatch);
            else
                OpenMenu(MPMenus.createRoom);
        }

        private void OpenDeviceSelectionPanel()
        {
            playerInfo.value.playerName = nameInputField.text;
            OpenMenu(devicesSelectionMenu);
        }
        //Update current menu on back button
        public void SetCurrentMenu(Menu menu)
        {
            currentMenu = menu;
        }
        public void OpenMenu(Menu menu)
        {
            alertText.gameObject.SetActive(false);
            if (currentMenu != null)
            {
                currentMenu.OpenMenu(false);
                menu.previousMenu = currentMenu;
            }
            currentMenu = menu;
            currentMenu.OpenMenu(true);
        }

        public void CreateGame()
        {
            GameManager.isSinglePlayerMode = !currentGameMode.value.isMultiplayer;

            //--- Genreate random room key ---
            roomKey = RoomKeyGenerator.GenerateRoomKey();
            Debug.Log($"<color=yellow>Room Key: {roomKey}</color>");
            loadingMenu.SetActive(true);
            diceSelectionButton.gameObject.SetActive(false);

            //--- New start method arg to get roomkey and player count---
            if (isMultiDevice)
            {
                NetworkManager.Instance.CreateGame(roomKey, playerCount, GameMode.Shared);
            }
            else
            {
                NetworkManager.Instance.CreateGame(roomKey, playerCount, GameMode.Single);
            }
        }

        //--- New ---
        public void JoinRoom(string roomName)
        {
            //GameManager.isSinglePlayerMode = !currentGameMode.value.isMultiplayer;
            loadingMenu.SetActive(true);
            diceSelectionButton.gameObject.SetActive(false);
            NetworkManager.Instance.JoinGame(roomName);
        }
        public void onJoinFailed(string msg)
        {
            OpenMenu(MPMenus.createOrJoinRoom);
            loadingMenu.SetActive(false);
            alertText.gameObject.SetActive(true);
            alertText.text = msg;
        }

        public void RandomRoom()
        {
            //GameManager.isSinglePlayerMode = !currentGameMode.value.isMultiplayer;
            loadingMenu.SetActive(true);
            diceSelectionButton.gameObject.SetActive(false);
            NetworkManager.Instance.RandomMatchmaking();
        }
    }
}
