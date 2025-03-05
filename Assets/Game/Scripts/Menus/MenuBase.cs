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

        public GameObject gameResultPanel;
        public GameObject pauseMenuPanel;
        public TMP_Text gameScoreTMP;

        [Header("Buttons")]
        public Button rollDiceButton;
        public Button bankScoreButton;
        public Button pauseButton;
        public Button resumeGameButton;
        public Button leaveGameButton;

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
            if(bankScoreButton) bankScoreButton.onClick.AddListener(BankScore);
            rollDiceButton.onClick.AddListener(RollDice);
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
            resumeGameButton.onClick.AddListener(OnResumeGameButtonClicked);
            leaveGameButton.onClick.AddListener(OnLeaveGameButtonClicked);
            updateScoresUI.executeAction += OnUpdateScoresUI;
            gameScore.onValueChange += OnUpdateGameScore;
            diceRollingVariable.onValueChange += OnDiceRollChanged;
        }

        public virtual void OnDisable()
        {
            if (bankScoreButton) bankScoreButton.onClick.RemoveListener(BankScore);
            rollDiceButton.onClick.RemoveListener(RollDice);
            pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            resumeGameButton.onClick.RemoveListener(OnResumeGameButtonClicked);
            leaveGameButton.onClick.RemoveListener(OnLeaveGameButtonClicked);
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
            if(val)
                rollDiceButton.gameObject.SetActive(false);
        }

        public virtual void OnPauseButtonClicked()
        {
            pauseMenuPanel.SetActive(true);
        }

        public virtual void OnLeaveGameButtonClicked()
        {
            DiceGame.Network.NetworkManager.Instance.LeaveGame();
        }

        public virtual void OnResumeGameButtonClicked()
        {
            pauseMenuPanel.SetActive(false);
        }

        public abstract void EndGame();
        public abstract void OnMasterChanged();
    }
}
