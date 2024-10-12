using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class PlayerInfoEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameTMP, bankedScoreTMP;
        [SerializeField] private Button bankButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor, highlightColor;

        private bool _isHighlighted;
        private System.Action<int> bankAction;
        public int SinglePlayerModeId { get; set; }
        public bool IsHighlighted { get { return _isHighlighted; } }

        private void OnEnable()
        {
            bankButton.onClick.AddListener(OnBankClicked);
        }

        private void OnDisable()
        {
            bankButton.onClick.RemoveListener(OnBankClicked);
        }

        private void Start()
        {
            //bankButton.gameObject.SetActive(false);
            bankButton.interactable = false;
        }

        private void OnBankClicked()
        {
            bankAction?.Invoke(SinglePlayerModeId);
        }

        public void InitializeForSinglePlayer(int id, System.Action<int> bankAction)
        {
            SinglePlayerModeId = id;
            this.bankAction = bankAction;
            bankButton.gameObject.SetActive(true);
        }

        public void BankButtonVisible(bool isVisible)
        {
            bankButton.gameObject.SetActive(isVisible);
        }

        public void BankButtonInteractable(bool isInteractable)
        {
            bankButton.interactable = isInteractable;
        }

        public void SetHighlight(bool highlight)
        {
            _isHighlighted = highlight;
            backgroundImage.color = highlight ? highlightColor : normalColor;
        }

        public void UpdateEntry(string name, int bankedScore, bool showScore = true)
        {
            playerNameTMP.text = name;
            bankedScoreTMP.text = bankedScore.ToString();
            bankedScoreTMP.gameObject.SetActive(showScore);
        }
    }
}
