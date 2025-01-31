using DiceGame.Network;
using TMPro;
using UnityEngine;

public class PrivateRoomHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomKey;

    private void OnEnable()
    {
        roomKey.text = NetworkManager.Instance.newRoomKey;
    }
}
