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
    public class MP_MexicoCanvas : MenuBase
    {
        [Header("Fields")]
        [SerializeField] private PlayersListVariable players;

        [Header("Local Player")]
        [SerializeField] private Transform playerLivesContainer;
        [SerializeField] private GameObject mexicoBanner;
        [SerializeField] private Image playerLifeImagePrefab;
        [SerializeField] private Color lifeNormalColor;
        [SerializeField] private Color lifeEmptyColor;
        [SerializeField] private TMP_Text localPlayerNameTMP;
        [SerializeField] private TMP_Text winnerNameTMP;
        [SerializeField] private TMP_Text winnerScoreTMP;
        [SerializeField] private TMP_Text roundTMP;
        [SerializeField] private TMP_Text rollButonTMP;

        [Header("Other Players")]
        [SerializeField] private PlayerInfoEntryUI playerInfoEntryUIPrefab;
        [SerializeField] private Transform playerInfoEntryContainer;
        private Dictionary<PlayerRef, PlayerInfoEntryUI> otherPlayersEntries
            = new Dictionary<PlayerRef, PlayerInfoEntryUI>();
        private List<Image> lifeImages = new List<Image>();
        private GameModeMexico gameModeMexico;

        public override void OnEnable()
        {
            base.OnEnable();
            players.onListValueChange += UpdatePlayerInfoEntries;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            players.onListValueChange -= UpdatePlayerInfoEntries;
        }

        public override void Start()
        {
            base.Start();
            gameModeMexico = (GameModeMexico)gameMode; 
            localPlayerNameTMP.text = localPlayerManager.playerName;
            roundTMP.text = $"ROUND {roundVariable.value}";
            UpdatePlayerInfoEntries(null);
            mexicoBanner.SetActive(false);
            ShowLives(gameModeMexico.MaxLives, gameModeMexico.MaxLives);
            if (gameMode.HasGameEnded)
            {
                EndGame();
            }
        }

        public void SetDiceRollButton(bool active, string text)
        {
            rollButonTMP.text = text;
            rollDiceButton.gameObject.SetActive(active);
        }

        public void AnnounceWinner(PlayerRef winnerRef)
        {
            gameMode.OnGameEnded();
            var winner = gameMode.players.value.FirstOrDefault(p => p.playerRef == winnerRef);
            winnerNameTMP.text = winner.playerName;
            winnerScoreTMP.text = winner.totalScore.ToString();
            rollDiceButton.gameObject.SetActive(false);
            gameResultPanel.SetActive(true);
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

        public override void OnUpdateScoresUI()
        {
            var localPlayer = players.value.FirstOrDefault(p => p.playerRef == PlayerManager.LocalPlayer.playerRef);
            if (localPlayer)
            {
                ShowLives(localPlayer.lives, gameModeMexico.MaxLives);
                mexicoBanner.SetActive(localPlayer.totalScore == 21);
            }

            foreach (var entry in otherPlayersEntries)
            {
                var player = gameMode.players.value.FirstOrDefault(p => p.playerRef == entry.Key);

                if (player)
                {
                    entry.Value.UpdateEntry(player.playerName, player.totalScore, true, player.lives > 0);
                    entry.Value.ShowLives(player.lives, gameModeMexico.MaxLives);
                    entry.Value.SetHighlight(player.totalScore == 21);
                }
            }
        }

        private void ShowLives(uint currentLives, uint maxLives)
        {
            if (playerLivesContainer.childCount == 0)
            {
                playerLivesContainer.gameObject.SetActive(true);

                for (int i = 0; i < maxLives; i++)
                {
                    var lifeImage = Instantiate(playerLifeImagePrefab, playerLivesContainer);
                    lifeImages.Add(lifeImage);
                }
            }

            for (int i = 0; i < maxLives; i++)
            {
                lifeImages[i].color = i < currentLives ? lifeNormalColor : lifeEmptyColor;
            }

            Color nameColor = localPlayerNameTMP.color;
            nameColor.a = PlayerManager.LocalPlayer.lives > 0 ? 1 : 0.3f;
            localPlayerNameTMP.color = nameColor;
        }

        private void UpdatePlayerInfoEntries(PlayerManager player)
        {
            var playerRefs = players.value.Select(p => p.playerRef).ToList();
            var removedPlayers = otherPlayersEntries.Where(op => !playerRefs.Contains(op.Key)).ToList();
            removedPlayers.ForEach(p =>
            {
                otherPlayersEntries.Remove(p.Key);
                Destroy(p.Value.gameObject);
            });

            foreach (var playerRef in playerRefs)
            {
                if (otherPlayersEntries.ContainsKey(playerRef) || localPlayerManager.playerRef == playerRef)
                    continue;

                var entry = Instantiate(playerInfoEntryUIPrefab, playerInfoEntryContainer);
                otherPlayersEntries.Add(playerRef, entry);
                if(gameModeMexico == null) gameModeMexico = (GameModeMexico)gameMode;
                entry.UpdateEntry(player.playerName, player.totalScore);
                entry.ShowLives(gameModeMexico.MaxLives, gameModeMexico.MaxLives);
            }

            //OnUpdateScoresUI();
        }
    }
}
