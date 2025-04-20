using DiceGame.ScriptableObjects;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine.UI;
using DiceGame.Game;
using Newtonsoft.Json;

namespace DiceGame.UI
{
    public class SP_MexicoCanvas : MenuBase
    {
        [Header("Fields")]
        [SerializeField] private TMP_Text winnerNameTMP;
        [SerializeField] private TMP_Text winnerScoreTMP;
        [SerializeField] private TMP_Text roundTMP;
        [SerializeField] private TMP_Text rollButonTMP;

        [Header("Other Players")]
        [SerializeField] private PlayerInfoEntryUI playerInfoEntryUIPrefab;
        [SerializeField] private Transform playerInfoEntryContainer;

        private Dictionary<int, PlayerInfoEntryUI> playerEntries
            = new Dictionary<int, PlayerInfoEntryUI>();
        private List<Image> lifeImages = new List<Image>();
        private GameModeMexicoSP gameModeMexico;

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        public override void Start()
        {
            base.Start();
            gameModeMexico = (GameModeMexicoSP)gameMode;
            roundTMP.text = $"ROUND {roundVariable.value}";
        }

        public PlayerInfoEntryUI SpawnPlayer(int id, string name, bool showScore = true)
        {
            var entry = Instantiate(playerInfoEntryUIPrefab, playerInfoEntryContainer);
            entry.InitializeForSinglePlayer(id, null);
            entry.UpdateEntry(name, 0);
            if (gameModeMexico == null) gameModeMexico = (GameModeMexicoSP)gameMode;
            entry.ShowLives(gameModeMexico.MaxLives, gameModeMexico.MaxLives);
            entry.BankButtonVisible(false);
            playerEntries.Add(id, entry);
            return entry;
        }

        public void SetDiceRollButton(bool active, string text)
        {
            rollButonTMP.text = text;
            rollDiceButton.gameObject.SetActive(active);
        }

        public void AnnounceWinner(PlayerGameDataMexico playerData)
        {
            winnerNameTMP.text = playerData.playerName;
            winnerScoreTMP.text = playerData.totalScore.ToString();
            rollDiceButton.gameObject.SetActive(false);
            gameResultPanel.SetActive(true);

            foreach (var entry in playerEntries)
            {
                entry.Value.ShowScore();
            }
        }

        public override void OnRoundChanged(int value)
        {
            roundTMP.text = $"ROUND {value}";
            SetDiceRollButton(true, "Roll Dice");
        }

        public override void OnMasterChanged()
        {

        }

        public override void EndGame()
        {

        }

        public void OnPlayerTurnChange(int id)
        {
            rollDiceButton.gameObject.SetActive(true);
            foreach (var entry in playerEntries)
            {
                entry.Value.SetHighlight(false);
                entry.Value.SetMexicoColor(gameModeMexico.PlayersData[entry.Key].totalScore == 21);
            }
            playerEntries[id].SetHighlight(true);
        }

        public override void OnUpdateScoresUI()
        {
            if (gameModeMexico == null)
                return;

            foreach (var entry in playerEntries)
            {
                if (gameModeMexico.PlayersData.TryGetValue(entry.Key, out var player))
                {
                    entry.Value.UpdateEntry(player.playerName, player.totalScore, true, player.lives > 0);
                    entry.Value.ShowLives(player.lives, gameModeMexico.MaxLives);
                    entry.Value.SetMexicoColor(player.totalScore == 21);
                }
            }
        }
    }
}
