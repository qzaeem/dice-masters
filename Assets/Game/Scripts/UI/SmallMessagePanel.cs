using DG.Tweening;
using TMPro;
using UnityEngine;

namespace DiceGame.UI
{
    public class SmallMessagePanel : RollMessagePanel
    {
        public override void OnEnable()
        {
            animationSequence.Kill();
            animatedPanel.localScale = new Vector3(0.2f, 0.1f, 0.25f);
            canvasGroup.alpha = 0.02f;
            animatedPanel.gameObject.SetActive(true);
            animationSequence = DOTween.Sequence()
                .Append(animatedPanel.DOScale(new Vector3(1, 0.1f, 1), 0.5f))
                .Join(canvasGroup.DOFade(0.3f, 0.5f))
                .Append(animatedPanel.DOScale(Vector3.one, 0.05f))
                .Join(canvasGroup.DOFade(1, 0.05f))
                .AppendInterval(2)
                .Append(animatedPanel.DOScale(new Vector3(0.75f, 0.1f, 1), 0.05f))
                .Join(canvasGroup.DOFade(0.3f, 0.05f))
                .Append(animatedPanel.DOScale(new Vector3(0.2f, 0.1f, 0.25f), 0.25f))
                .Join(canvasGroup.DOFade(0.02f, 0.25f))
                .AppendCallback(() =>
                {
                    gameObject.SetActive(false);
                });
        }
    }
}
