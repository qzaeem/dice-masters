using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DiceGame.ScriptableObjects;
using DiceGame.Network;

namespace DiceGame.UI
{
    public class MainMenuCanvas : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private PlayerInfoVariable playerInfo;

        private void OnEnable()
        {
            startButton.onClick.AddListener(StartGame);
            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(StartGame);
            nameInputField.onValueChanged.RemoveListener(OnNameChanged);
        }

        private void Start()
        {
            startButton.interactable = false;
        }

        public void OnNameChanged(string value)
        {
            startButton.interactable = value.Length > 0 && value.Length < 17 && !string.IsNullOrWhiteSpace(value);
        }

        private void StartGame()
        {
            playerInfo.value.playerName = nameInputField.text;
            NetworkManager.Instance.StartGame();
            loadingPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
