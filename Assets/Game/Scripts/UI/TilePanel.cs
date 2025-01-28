using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TilePanel : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text numberTMP;
    [SerializeField] private Color selectedColor, knockedColor;

    private System.Action<uint> tileClickedAction;

    public uint TileID { get; private set; }
    public uint TileValue { get; private set; }
    public bool IsSelected { get; private set; }
    public bool isKnockedDown { get; private set; }

    private void OnEnable()
    {
        button.onClick.AddListener(OnClickedTile);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClickedTile);
    }

    public void InitializeTile(uint id, uint val, System.Action<uint> tileClickedAction)
    {
        TileID = id;
        TileValue = val;
        numberTMP.text = TileValue.ToString();
        this.tileClickedAction = tileClickedAction;
    }

    private void Start()
    {
        IsSelected = false;
        isKnockedDown = false;
    }

    public void OnClickedTile()
    {
        tileClickedAction?.Invoke(TileID);
    }

    public void SelectTile()
    {
        bgImage.color = selectedColor;
        IsSelected = true;
    }

    public void DeselectTile()
    {
        bgImage.color = Color.white;
        IsSelected = false;
    }

    public void ActivateTile(bool activate)
    {
        button.interactable = activate;
    }

    public void KnockDownTile()
    {
        bgImage.color = knockedColor;
        DOTween.Sequence()
            .Append(bgImage.DOFade(0, 0.5f))
            .Join(bgImage.transform.DOScale(1.5f, 0.5f))
            .OnComplete(() =>
            {
                IsSelected = false;
                isKnockedDown = true;
                numberTMP.gameObject.SetActive(false);
                button.interactable = false;
            });
    }
}
