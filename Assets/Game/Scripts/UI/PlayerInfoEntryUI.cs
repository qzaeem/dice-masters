using UnityEngine;
using TMPro;

namespace DiceGame.UI
{
    public class PlayerInfoEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameTMP, bankedScoreTMP;

        public void UpdateEntry(string name, int bankedScore)
        {
            playerNameTMP.text = name;
            bankedScoreTMP.text = bankedScore.ToString();
        }
    }
}
