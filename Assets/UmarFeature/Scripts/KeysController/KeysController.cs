using TMPro;
using UnityEngine;

public class KeysController : MonoBehaviour
{
    public static KeysController Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI keysTexts;
    [SerializeField] private int keys;
    private const string p_KeysValue = "KeysValue";
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadKeys();
    }
    private void LoadKeys()
    {
        keys = PlayerPrefs.GetInt(p_KeysValue, keys);
        keysTexts.text = $"Keys : {keys.ToString()} ";
    }

    public void AddKeys(int value)
    {
        if (value <= 0) return;
        keys += value;
        SaveKeys();
    }

    public void SubtractKeys(int value)
    {
        if (value <= 0) return;
        keys = Mathf.Max(0, keys - value);
        SaveKeys();
    }

    public int GetKeys()
    {
        return keys;
    }
    public void SetKeysValue(int value)
    {
        keys = Mathf.Max(0, value);
        SaveKeys();
    }

    private void SaveKeys()
    {
        keysTexts.text = $"Keys : {keys.ToString()} ";
        PlayerPrefs.SetInt(p_KeysValue, keys);
        PlayerPrefs.Save();
    }
}
