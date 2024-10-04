using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

namespace DiceGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Bankroll Battle", menuName = "Game Mode/Bankroll Battle")]
    public class GameModeBankrollBattle : GameModeBase
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnDiceRollComplete()
        {
            base.OnDiceRollComplete();
            gameManager.EnableBankingAbility();
        }

        public override void IncrementRound()
        {
            base.IncrementRound();
            ResetGameScore();
        }

        public override string GetSettingsJson()
        {
            var bankrollBattleData = new BankrollBattleData()
            {
                isMultiplayer = isMultiplayer,
                showScoresOnEnd = showScoresOnEnd
            };

            var jsonString = JsonConvert.SerializeObject(bankrollBattleData);
            return jsonString;
        }

        public override void SetSettingsFromJson(string jsonString)
        {
            var bankrollBattleData = JsonConvert.DeserializeObject<BankrollBattleData>(jsonString);
            isMultiplayer = bankrollBattleData.isMultiplayer;
            showScoresOnEnd = bankrollBattleData.showScoresOnEnd;
        }
    }
}

[System.Serializable]
public class BankrollBattleData : SettingsData
{
    
}
