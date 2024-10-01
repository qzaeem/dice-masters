using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DiceGame.Game;
using DiceGame.ScriptableObjects;

namespace DiceGame.UI
{
    public abstract class MenuBase : MonoBehaviour
    {
        public PlayerManager localPlayerManager;

        public RollMessagePanel rollMsgPanel;
        public TMP_Text gameScoreTMP;

        [Header("Buttons")]
        public Button bankScoreButton;

        [Header("Scriptable Objects")]
        public GameModeBase gameMode;
        public ActionSO updateScoresUI;

        private void OnEnable()
        {
            bankScoreButton.onClick.AddListener(BankScore);
            updateScoresUI.executeAction += OnUpdateScoresUI;
            gameMode.gameScore.onValueChange += OnUpdateGameScore;
        }

        private void OnDisable()
        {
            bankScoreButton.onClick.RemoveListener(BankScore);
            updateScoresUI.executeAction -= OnUpdateScoresUI;
            gameMode.gameScore.onValueChange -= OnUpdateGameScore;
        }

        public virtual void Start()
        {
            rollMsgPanel.gameObject.SetActive(false);
        }

        public virtual void OnUpdateGameScore(int score)
        {
            gameScoreTMP.text = score.ToString();
        }

        public virtual void ShowRollMessage(string message)
        {
            rollMsgPanel.SetMessage(message);
            rollMsgPanel.gameObject.SetActive(true);
        }

        public virtual void BankScore()
        {
            localPlayerManager.totalScore += gameMode.gameScore.value;
            gameMode.ResetGameScore();
        }

        public virtual void NextTurn()
        {

        }

        public virtual void OnUpdateScoresUI()
        {

        }
    }
}
