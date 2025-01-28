using UnityEngine;
using DiceGame.Models;

namespace DiceGame.UI
{
    public class PopupManagerCanvas : MonoBehaviourSingleton<PopupManagerCanvas>
    {
        [SerializeField] private RollMessagePanel fullScreenMessagePanel;
        [SerializeField] private RollMessagePanel smallAreaMessagePanel;

        public void ShowFullScreenMessage(string message)
        {
            fullScreenMessagePanel.ShowMessage(message);
        }

        public void ShowSmallAreaMessage(string message)
        {
            smallAreaMessagePanel.ShowMessage(message);
        }
    }
}
