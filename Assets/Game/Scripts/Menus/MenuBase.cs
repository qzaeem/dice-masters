using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DiceGame.Game;
using DiceGame.ScriptableObjects;
using System.Collections.Generic;

namespace DiceGame.UI
{
    public abstract class MenuBase : MonoBehaviour
    {
        [HideInInspector] public PlayerManager localPlayerManager;

        public GameObject gameStartBlocker;
        public GameObject gameResultPanel;
        public GameObject pauseMenuPanel;
        public GameObject instructionsPanel;
        public TMP_Text gameScoreTMP;
        public TMP_Text roomKeyTMP;

        [Header("Buttons")]
        public Button rollDiceButton;
        public Button bankScoreButton;
        public Button pauseButton;
        public Button instructionsButton;
        public Button resumeGameButton;
        public Button returnToGameButton;
        public Button leaveGameButton;
        public Button leaveGameButton2;
        public Button startGameButton;

        [Header("Scriptable Objects")]
        public GameModeBase gameMode;
        public IntVariable roundVariable;
        public IntVariable rollVariable;
        public IntVariable gameScore;
        public BoolVariable diceRollingVariable;
        public ActionSO updateScoresUI;

        [Header("Dice Roll Diplay")]
        [SerializeField] protected Transform dieRecordContainer;
        [SerializeField] protected Image dieRollRecordPrefab;
        [SerializeField] protected Color dieNormalColor, dieInactiveColor;
        [SerializeField] protected List<Sprite> diceSprites;

        protected PopupManagerCanvas popupManager;
        protected Dictionary<int, Image> diceRecord = new Dictionary<int, Image>();

        public virtual void OnEnable()
        {
            if (bankScoreButton) bankScoreButton.onClick.AddListener(BankScore);
            startGameButton.onClick.AddListener(StartGame);
            rollDiceButton.onClick.AddListener(RollDice);
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
            instructionsButton.onClick.AddListener(OnInstructionsButtonClicked);
            resumeGameButton.onClick.AddListener(OnResumeGameButtonClicked);
            returnToGameButton.onClick.AddListener(OnResumeGameButtonClicked);
            leaveGameButton.onClick.AddListener(OnLeaveGameButtonClicked);
            leaveGameButton2.onClick.AddListener(OnLeaveGameButtonClicked);
            updateScoresUI.executeAction += OnUpdateScoresUI;
            gameScore.onValueChange += OnUpdateGameScore;
            diceRollingVariable.onValueChange += OnDiceRollChanged;
        }

        public virtual void OnDisable()
        {
            if (bankScoreButton) bankScoreButton.onClick.RemoveListener(BankScore);
            startGameButton.onClick.RemoveListener(StartGame);
            rollDiceButton.onClick.RemoveListener(RollDice);
            pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            instructionsButton.onClick.RemoveListener(OnInstructionsButtonClicked);
            resumeGameButton.onClick.RemoveListener(OnResumeGameButtonClicked);
            returnToGameButton.onClick.RemoveListener(OnResumeGameButtonClicked);
            leaveGameButton.onClick.RemoveListener(OnLeaveGameButtonClicked);
            leaveGameButton2.onClick.RemoveListener(OnLeaveGameButtonClicked);
            updateScoresUI.executeAction -= OnUpdateScoresUI;
            gameScore.onValueChange -= OnUpdateGameScore;
            diceRollingVariable.onValueChange -= OnDiceRollChanged;
        }

        public virtual void Start()
        {
            popupManager = PopupManagerCanvas.Instance;
            gameScoreTMP.text = gameMode.gameScore.value.ToString();
            gameResultPanel.SetActive(false);
            pauseMenuPanel.SetActive(false);
            if (bankScoreButton) bankScoreButton.interactable = false;
        }

        public virtual void SetMultiplayerCanvas(bool isMultiplayer)
        {
            roomKeyTMP.text = isMultiplayer ? $"Join Key: <color=yellow>{DiceGame.Network.NetworkManager.Instance.NewRoomKey}</color>" : "";
            gameStartBlocker.SetActive(isMultiplayer);
        }

        public virtual void OnUpdateGameScore(int score)
        {
            gameScoreTMP.text = score.ToString();
        }

        public virtual void ShowRollMessage(string message)
        {
            popupManager.ShowFullScreenMessage(message);
        }

        public virtual void ShowSmallAreaMessage(string message)
        {
            popupManager.ShowSmallAreaMessage(message);
        }

        public virtual void RollDice()
        {
            rollDiceButton.gameObject.SetActive(false);
            gameMode.RollDice();
        }

        public virtual void BankScore()
        {
            gameMode.BankScore();
        }

        public virtual void ShowStartGameButton(bool show)
        {
            startGameButton.gameObject.SetActive(show);
        }

        public virtual void DisableStartBlocker()
        {
            gameStartBlocker.SetActive(false);
        }

        public virtual void StartGame()
        {
            gameMode.StartGame();
        }

        public virtual void UpdateDiceRecord(List<IPlayableDice> dice)
        {
            foreach (var die in dice)
            {
                Image image;

                if (!diceRecord.ContainsKey(die.dieID))
                {
                    image = Instantiate(dieRollRecordPrefab, dieRecordContainer);
                    diceRecord.Add(die.dieID, image);
                }

                image = diceRecord[die.dieID];
                image.sprite = die.IsRollable ? diceSprites[die.CurrentValue - 1] : image.sprite;
                var color = dieNormalColor;
                color.a = die.IsRollable ? dieNormalColor.a : dieInactiveColor.a;
                image.color = color;
            }
        }

        public virtual void EnableRollDiceButton(bool enabled)
        {
            rollDiceButton.gameObject.SetActive(enabled);
        }

        public virtual void OnRoundChanged(int val)
        {

        }

        public virtual void OnUpdateScoresUI()
        {

        }

        public virtual void OnDiceRollComplete(List<IPlayableDice> dice)
        {
            UpdateDiceRecord(dice);
        }

        public virtual void OnDiceRollChanged(bool val)
        {
            if (val)
                rollDiceButton.gameObject.SetActive(false);
        }

        public virtual void OnPauseButtonClicked()
        {
            pauseMenuPanel.SetActive(true);
        }

        public virtual void OnInstructionsButtonClicked()
        {
            instructionsPanel.SetActive(true);
        }

        public virtual void OnLeaveGameButtonClicked()
        {
            DiceGame.Network.NetworkManager.Instance.LeaveGame();
        }

        public virtual void OnResumeGameButtonClicked()
        {
            pauseMenuPanel.SetActive(false);
            instructionsPanel.SetActive(false);
        }

        public abstract void EndGame();
        public abstract void OnMasterChanged();
    }
}
