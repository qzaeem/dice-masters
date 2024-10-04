using UnityEngine;
using DG.Tweening;
using TMPro;

namespace DiceGame.UI
{
    public class RollMessagePanel : MonoBehaviour
    {
        [SerializeField] private RectTransform animatedPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text messageTMP;
        private Sequence animationSequence;

        private void OnEnable()
        {
            animationSequence.Kill();
            animatedPanel.localScale = Vector3.one * 0.25f;
            canvasGroup.alpha = 0.2f;
            animatedPanel.gameObject.SetActive(true);
            animationSequence = DOTween.Sequence()
                .Append(animatedPanel.DOScale(Vector3.one * 1.2f, 0.2f))
                .Join(canvasGroup.DOFade(1, 0.2f))
                .Append(animatedPanel.DOScale(Vector3.one, 0.05f))
                .AppendInterval(2)
                .Append(animatedPanel.DOScale(Vector3.one * 0.25f, 0.1f))
                .Join(canvasGroup.DOFade(0.2f, 0.1f))
                .AppendCallback(() =>
                {
                    gameObject.SetActive(false);
                });
        }

        private void OnDisable()
        {
            animatedPanel.gameObject.SetActive(false);
        }

        private void Awake()
        {
            animationSequence.Kill();
            gameObject.SetActive(false);
        }

        public void ShowMessage(string msg)
        {
            messageTMP.text = msg;
            gameObject.SetActive(true);
        }
    }
}
