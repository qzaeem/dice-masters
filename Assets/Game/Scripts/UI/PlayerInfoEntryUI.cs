using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

namespace DiceGame.UI
{
    public class PlayerInfoEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameTMP, bankedScoreTMP;
        [SerializeField] private Button bankButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color highlightColor;

        [Header("Roll Record Dropdown")]
        [SerializeField] private Button dropDownButton;
        [SerializeField] private GameObject dropdownArrowGO;
        [SerializeField] private GameObject dropdownPanelGO;
        [SerializeField] private GameObject dieRollRecordPanelPrefab;
        [SerializeField] private Image dieRollRecordPrefab;
        [SerializeField] private Color dieNormalColor, dieInactiveColor;
        [SerializeField] private List<Sprite> diceSprites;

        [Header("Lives Display")]
        [SerializeField] private Image lifeImagePrefab;
        [SerializeField] private Transform livesContainer;
        [SerializeField] private Color lifeNormalColor;
        [SerializeField] private Color lifeEmptyColor;

        private bool _isHighlighted;
        private List<GameObject> diceRollRecords = new List<GameObject>();
        private List<Image> lifeImages = new List<Image>();
        private System.Action<int> bankAction;
        public int SinglePlayerModeId { get; set; }
        public bool IsHighlighted { get { return _isHighlighted; } }

        private void OnEnable()
        {
            bankButton.onClick.AddListener(OnBankClicked);
            dropDownButton.onClick.AddListener(OnDropdownButtonClicked);
        }

        private void OnDisable()
        {
            bankButton.onClick.RemoveListener(OnBankClicked);
            dropDownButton.onClick.RemoveListener(OnDropdownButtonClicked);
        }

        private void Start()
        {
            //bankButton.gameObject.SetActive(false);
            bankButton.interactable = false;
            dropDownButton.interactable = false;
            dropdownPanelGO.SetActive(false);
            var angles = dropdownArrowGO.transform.localEulerAngles;
            angles.z = 0;
            dropdownArrowGO.transform.localEulerAngles = angles;
            dropdownArrowGO.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        }

        private void OnBankClicked()
        {
            bankAction?.Invoke(SinglePlayerModeId);
        }

        private void OnDropdownButtonClicked()
        {
            bool isOpen = !dropdownPanelGO.activeSelf;
            dropdownPanelGO.SetActive(isOpen);
            var angles = dropdownArrowGO.transform.localEulerAngles;
            angles.z = isOpen ? 180 : 0;
            dropdownArrowGO.transform.localEulerAngles = angles;

            StartCoroutine(RefreshUI());
        }

        private IEnumerator RefreshUI()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
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

        public void ShowLives(uint currentLives, uint maxLives)
        {
            if(livesContainer.childCount == 0)
            {
                livesContainer.gameObject.SetActive(true);

                for (int i = 0; i < maxLives; i++)
                {
                    var lifeImage = Instantiate(lifeImagePrefab, livesContainer);
                    lifeImages.Add(lifeImage);
                }
            }

            for (int i = 0; i < maxLives; i++)
            {
                var previousColor = lifeImages[i].color;
                var nextColor = i < currentLives ? lifeNormalColor : lifeEmptyColor;

                if(previousColor.CompareRGB(lifeNormalColor) && nextColor.CompareRGB(lifeEmptyColor))
                {
                    lifeImages[i].color = Color.red;
                    lifeImages[i].DOColor(nextColor, 0.25f).SetLoops(8, LoopType.Yoyo).OnComplete(() => lifeImages[i].color = nextColor);
                }
                else
                {
                    lifeImages[i].color = nextColor;
                }
            }
        }

        public void UpdateEntry(string name, int bankedScore, bool showScore = true, bool isPlayerActive = true)
        {
            playerNameTMP.text = name;
            bankedScoreTMP.text = bankedScore.ToString();
            bankedScoreTMP.gameObject.SetActive(showScore);
            Color nameColor = playerNameTMP.color;
            Color scoreColor = bankedScoreTMP.color;
            nameColor.a = scoreColor.a = isPlayerActive ? 1 : 0.3f;
            playerNameTMP.color = nameColor;
            bankedScoreTMP.color = scoreColor;

            // For Greed Mode
            dropDownButton.interactable = dropdownPanelGO.transform.childCount > 1;
            dropdownArrowGO.SetActive(dropdownPanelGO.transform.childCount > 1);
        }

        public void AddDropdownEntry(PlayerRollRecord playerRollRecord)
        {
            if (diceRollRecords.Count >= 5)
            {
                Destroy(diceRollRecords[0]);
                diceRollRecords.RemoveAt(0);
            }

            var dieRollRecordPanel = Instantiate(dieRollRecordPanelPrefab, dropdownPanelGO.transform);

            foreach(var die in playerRollRecord.diceInfo)
            {
                var dieRecordImg = Instantiate(dieRollRecordPrefab, dieRollRecordPanel.transform);
                dieRecordImg.sprite = diceSprites[die.dieValue - 1];
                var color = dieNormalColor;
                color.a = die.isActive ? dieNormalColor.a : dieInactiveColor.a;
                dieRecordImg.color = color;
            }

            diceRollRecords.Add(dieRollRecordPanel);

            dropdownArrowGO.SetActive(true);
            dropDownButton.interactable = true;

            StartCoroutine(RefreshUI());
        }

        public void ClearAllDropdownEntries()
        {
            foreach (var record in diceRollRecords)
                Destroy(record);
            diceRollRecords.Clear();

            dropDownButton.interactable = false;
            dropdownPanelGO.SetActive(false);
            var angles = dropdownArrowGO.transform.localEulerAngles;
            angles.z = 0;
            dropdownArrowGO.transform.localEulerAngles = angles;
            dropdownArrowGO.SetActive(false);

            StartCoroutine(RefreshUI());
        }
    }
}
