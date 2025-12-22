using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlanetInfoUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;


    private void Awake()
    {
        if (root == null) root = gameObject;
        Hide();
    }

    public void Show(string title, string body, Sprite icon = null)
    {
        if (root != null) root.SetActive(true);
        if (titleText != null) titleText.text = title ?? "";
        if (bodyText != null) bodyText.text = body ?? "";
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}
