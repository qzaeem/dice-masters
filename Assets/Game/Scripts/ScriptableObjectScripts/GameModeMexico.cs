using UnityEngine;
using Newtonsoft.Json;
using DiceGame.Game;
using System.Linq;
using DiceGame.UI;
using Fusion;
using System.Threading.Tasks;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Mexico", menuName = "Game Mode/Mexico")]
    public class GameModeMexico : GameModeBase
    {
        [SerializeField] private BoolVariable canBankScoreVariable;
        [SerializeField] private PlayerRefVariable changeTurnVariable;
        [SerializeField] private ActionSO playerFinishedRollAction;
        [SerializeField] private uint maxLives;
        private MP_MexicoCanvas mexicoMenu;
        private bool hasGameStarted;

        public uint MaxLives { get { return maxLives; } }

        public override void Initialize()
        {
            base.Initialize();
            hasGameStarted = false;
            playerFinishedRollAction.executeAction += OnPlayerFinishedRoll;
            mexicoMenu = gameMenu as MP_MexicoCanvas;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            playerFinishedRollAction.executeAction -= OnPlayerFinishedRoll;
        }

        private void OnPlayerFinishedRoll()
        {
            if (!players.value.All(p => p.hasFinishedRoll) || !PlayerManager.LocalPlayer.isMasterClient)
                return;

            var activePlayers = players.value.Where(p => p.lives > 0);
            var minScore = activePlayers.Min(p => p.totalScore);
            var maxScore = activePlayers.Max(p => p.totalScore);
            var lowestScorePlayers = activePlayers.Where(p => p.totalScore == minScore).ToList();
            var winners = activePlayers.Where(p => p.totalScore == maxScore).ToList();

            bool isTie = lowestScorePlayers.Count > 1;
            if(isTie)
            {
                lowestScorePlayers.ForEach(l =>
                {
                    l.ResetDiceRollRpc();
                    gameManager.EnableRerollDiceRpc(l.playerRef);
                });
                players.value.ForEach(p =>
                {
                    gameManager.ShowRollMessageRpc(p.playerRef, "TIE!");
                });
                return;
            }
            else
            {
                lowestScorePlayers.ForEach(l =>
                {
                    if (l.lives == 1)
                        gameManager.ShowRollMessageRpc(l.playerRef, "Knocked OUT!");
                    else if(l.lives > 1)
                        gameManager.ShowRollMessageRpc(l.playerRef, "Life Lost!");
                    l.UpdatePlayerLifeRpc(l.lives - 1);
                });
            }

            IncrementRound();
        }

        public void AnnounceWinner(PlayerRef winnerRef)
        {
            mexicoMenu.AnnounceWinner(winnerRef);
        }

        public override void RollDice()
        {
            diceRollManager.Execute();
        }

        public override void OnDiceRollComplete()
        {
            base.OnDiceRollComplete();

            var scores = diceRollManager.dice.Select(d => d.CurrentValue).ToList();
            int score = scores.Contains(1) && scores.Contains(2) ? 21 : scores.Sum();
            bool resurrect = false;

            if(PlayerManager.LocalPlayer.lives == 0 && score == 21)
            {
                gameMenu.ShowRollMessage("Resurrection Roll!");
                resurrect = true;
            }

            gameScore.Set(score);
            PlayerManager.LocalPlayer.CompleteRollMexicoRpc(true, resurrect, score);
        }

        public override void IncrementRound()
        {
            var winners = players.value.Where(p => p.lives > 0).ToList();
            //winners.ForEach(w => Debug.Log($"Winner: {w.playerName}, Lives: {w.lives}"));
            if(winners.Count == 1 && !players.value.Any(p => p.resurrect))
            {
                gameManager.AnnounceMexicoWinnerRPC(winners[0].playerRef);
                return;
            }

            base.IncrementRound();
        }

        public override string GetSettingsJson()
        {
            var mexicoData = new MexicoData()
            {
                isMultiplayer = isMultiplayer,
                showScoresOnEnd = showScoresOnEnd,
                hasGameEnded = HasGameEnded,
                maxLives = maxLives
            };

            var jsonString = JsonConvert.SerializeObject(mexicoData);
            return jsonString;
        }

        public override void SetSettingsFromJson(string jsonString)
        {
            var mexicoData = JsonConvert.DeserializeObject<MexicoData>(jsonString);
            isMultiplayer = mexicoData.isMultiplayer;
            showScoresOnEnd = mexicoData.showScoresOnEnd;
            HasGameEnded = mexicoData.hasGameEnded;
            maxLives = mexicoData.maxLives;
        }

        public override void EndGame()
        {
            
        }

        public override async void OnRoundChanged(int round)
        {
            await Task.Delay(hasGameStarted ? 5000 : 0);

            hasGameStarted = true;
            base.OnRoundChanged(round);
            gameScore.Set(0);
            var localPlayer = PlayerManager.LocalPlayer;

            if (localPlayer.resurrect)
            {
                if(localPlayer.lives == 0)
                    localPlayer.UpdatePlayerLifeRpc(1);
            }

            if(localPlayer.lives == 0)
            {
                gameMenu.ShowSmallAreaMessage("Roll a Mexico to get 1 life back");
            }
            localPlayer.CompleteRollMexicoRpc(false, false, 0);
        }

        public override void OnPlayerRemoved(PlayerRef playerRef)
        {

        }

        public void EnableRerollButton()
        {
            mexicoMenu.SetDiceRollButton(true, "Re-Roll Dice");
        }
    }
}

[System.Serializable]
public class MexicoData : SettingsData
{
    public uint maxLives;
}