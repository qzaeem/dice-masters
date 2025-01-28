using UnityEngine;
using UnityEngine.UI;

public class CheckMark : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject checkMark;
    public bool defaultValue;
    public bool currentValue { get; private set; }

    private void OnEnable()
    {
        button.onClick.AddListener(ChangeValue);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(ChangeValue);
    }

    void Start()
    {
        currentValue = defaultValue;
        SetGraphic();
    }

    private void SetGraphic()
    {
        checkMark.SetActive(currentValue);
    }

    private void ChangeValue()
    {
        currentValue = !currentValue;
        SetGraphic();
    }
}
