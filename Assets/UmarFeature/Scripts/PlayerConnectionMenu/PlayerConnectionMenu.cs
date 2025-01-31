using UnityEngine;
using UnityEngine.UI;

public class PlayerConnectionMenu : MonoBehaviour
{
    [SerializeField] private Button randomMatchBtn;
    [SerializeField] private Button privateRoomBtn;
    [SerializeField] private GameObject randomMatchMenu;
    [SerializeField] private GameObject privateRoomMenu;
    private void OnEnable()
    {
        randomMatchBtn.onClick.AddListener(() => EnableMenu(1));
        privateRoomBtn.onClick.AddListener(() => EnableMenu(2));
    }
    private void OnDisable()
    {
        randomMatchBtn.onClick.RemoveAllListeners();
        privateRoomBtn.onClick.RemoveAllListeners();
    }
    private void OnDestroy()
    {
        randomMatchBtn.onClick.RemoveAllListeners();
        privateRoomBtn.onClick.RemoveAllListeners();
    }

    private void EnableMenu(int menu)
    {
        this.gameObject.SetActive(false);
        switch (menu)
        {
            case 1:
                randomMatchMenu.SetActive(true);
                break;
            case 2:
                privateRoomMenu.SetActive(true);
                break;
        }
    }
}
