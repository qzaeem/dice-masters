using DiceGame.ScriptableObjects;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Fusion;

namespace DiceGame.UI
{
    public class MP_BankrollBattleCanvas : MenuBase
    {
        [Header("Local Player")]
        [SerializeField] private TMP_Text localPlayerNameTMP, localPlayerScoreTMP;
        [SerializeField] private TMP_Text roundTMP;

        [Header("Other Players")]
        [SerializeField] private PlayerInfoEntryUI playerInfoEntryUIPrefab;
        [SerializeField] private Transform playerInfoEntryContainer;
        private GameModeBankrollBattle gameModeBankrollBattle;
        private Dictionary<PlayerRef, PlayerInfoEntryUI> otherPlayersEntries
            = new Dictionary<PlayerRef, PlayerInfoEntryUI>();

        public override void Start()
        {
            base.Start();
            gameModeBankrollBattle = gameMode as GameModeBankrollBattle;
            SpawnPlayerInfoEntries();
        }

        private void SpawnPlayerInfoEntries()
        {
            var playerRefs = gameMode.players.value.Select(p => p.playerRef).ToList();

            foreach(var playerRef in playerRefs)
            {
                var entry = Instantiate(playerInfoEntryUIPrefab, playerInfoEntryContainer);
                otherPlayersEntries.TryAdd(playerRef, entry);
            }

            OnUpdateScoresUI();
        }

        public override void OnUpdateScoresUI()
        {
            foreach(var entry in otherPlayersEntries)
            {
                var player = gameMode.players.value.FirstOrDefault(p => p.playerRef == entry.Key);

                if (player)
                {
                    entry.Value.UpdateEntry(player.playerName, player.totalScore);
                }
            }
        }
    }
}
