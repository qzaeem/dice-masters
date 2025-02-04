using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiceGame.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class PlayerNamesPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text numOfPlayersTMP;
        [SerializeField] private Button incrementButton;
        [SerializeField] private Button decrementButton;
        [SerializeField] private TMP_InputField nameFieldPrefab;
        [SerializeField] private StringListVariable playerNamesVariable;
        [SerializeField] private Transform nameFieldsContainer;
        [SerializeField] private int minPlayers;
        [SerializeField] private int maxPlayers;
        private List<TMP_InputField> nameFields = new();
        public int numberOfPlayers { get; private set; } // change to public
        public System.Action onFieldValueChanged;
        //New
        public bool isMultiplayer;
        private void OnEnable()
        {
            incrementButton.onClick.AddListener(IncrementNumbrOfPlayers);
            decrementButton.onClick.AddListener(DecrementNumbrOfPlayers);
        }

        private void OnDisable()
        {
            incrementButton.onClick.RemoveListener(IncrementNumbrOfPlayers);
            decrementButton.onClick.RemoveListener(DecrementNumbrOfPlayers);
        }

        private void Start()
        {
            for (int i = 0; i < maxPlayers; i++)
            {
                var field = Instantiate(nameFieldPrefab, nameFieldsContainer);
                field.onValueChanged.AddListener(OnValueChanged);
                nameFields.Add(field);
            }

            numberOfPlayers = minPlayers;
            numOfPlayersTMP.text = $"{numberOfPlayers} Players";
            ChangePlayerFields(numberOfPlayers);
        }

        private void IncrementNumbrOfPlayers()
        {
            numberOfPlayers++;
            UpdateNumberOfPlayers();
        }

        private void DecrementNumbrOfPlayers()
        {
            numberOfPlayers--;
            UpdateNumberOfPlayers();
        }

        private void UpdateNumberOfPlayers()
        {
            numberOfPlayers = Mathf.Clamp(numberOfPlayers, minPlayers, maxPlayers);
            numOfPlayersTMP.text = $"{numberOfPlayers} Players";
            if (!isMultiplayer)
                ChangePlayerFields(numberOfPlayers);
        }

        private void ChangePlayerFields(int numberOfFields)
        {
            nameFields.ForEach(f =>
            {
                f.gameObject.SetActive(false);
            });

            for (int i = 0; i < numberOfFields; i++)
            {
                nameFields[i].gameObject.SetActive(true);
            }

            StartCoroutine(RefreshUI());
        }

        private IEnumerator RefreshUI()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        }

        private void OnValueChanged(string val)
        {
            onFieldValueChanged?.Invoke();
        }

        public bool AllFieldsHaveNames()
        {
            return nameFields.Where(f => f.gameObject.activeSelf).All(f => f.text != "");
        }

        public void SetNamesSO()
        {
            playerNamesVariable.Set(nameFields.Where(f => f.gameObject.activeSelf).Select(f => f.text).ToList());
        }

        private void OnDestroy()
        {
            nameFields.ForEach(f => f.onValueChanged.RemoveListener(OnValueChanged));
        }
    }
}
