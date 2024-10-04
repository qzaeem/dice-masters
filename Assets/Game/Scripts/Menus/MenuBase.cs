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

        public RollMessagePanel rollMsgPanel;
        public GameObject gameResultPanel;
        public TMP_Text gameScoreTMP;

        [Header("Buttons")]
        public Button rollDiceButton;
        public Button bankScoreButton;

        [Header("Scriptable Objects")]
        public GameModeBase gameMode;
        public IntVariable roundVariable;
        public ActionSO updateScoresUI;

        public virtual void OnEnable()
        {
            bankScoreButton.onClick.AddListener(BankScore);
            rollDiceButton.onClick.AddListener(RollDice);
            updateScoresUI.executeAction += OnUpdateScoresUI;
            gameMode.gameScore.onValueChange += OnUpdateGameScore;
        }

        public virtual void OnDisable()
        {
            bankScoreButton.onClick.RemoveListener(BankScore);
            rollDiceButton.onClick.RemoveListener(RollDice);
            updateScoresUI.executeAction -= OnUpdateScoresUI;
            gameMode.gameScore.onValueChange -= OnUpdateGameScore;
        }

        public virtual void Start()
        {
            rollMsgPanel.gameObject.SetActive(false);
            gameScoreTMP.text = gameMode.gameScore.value.ToString();
            gameResultPanel.SetActive(false);
            bankScoreButton.interactable = false;
        }

        public virtual void OnUpdateGameScore(int score)
        {
            gameScoreTMP.text = score.ToString();
        }

        public virtual void ShowRollMessage(string message)
        {
            rollMsgPanel.ShowMessage(message);
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

        public virtual void ChangeRound(int val)
        {

        }

        public virtual void NextTurn()
        {

        }

        public virtual void OnUpdateScoresUI()
        {

        }

        public virtual void OnDiceRollComplete(List<Dice> dice)
        {

        }

        public abstract void OnMasterChanged();
    }
}
