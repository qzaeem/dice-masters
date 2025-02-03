using DiceGame.Network;
using TMPro;
using UnityEngine;

public class PrivateRoomHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomKey;
    [SerializeField] private TextMeshProUGUI playerCount;

    private void OnEnable()
    {
        roomKey.text = NetworkManager.Instance.NewRoomKey;
        playerCount.text = NetworkManager.Instance.PlayerCount.ToString();
    }
}
