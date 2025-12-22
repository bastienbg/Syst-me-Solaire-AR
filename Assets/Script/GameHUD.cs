using UnityEngine;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI objectivesText;
    public TextMeshProUGUI messageText;

    public void SetLevel(int levelIndex)
    {
        if (levelText != null) levelText.text = $"Niveau {levelIndex + 1}";
    }

    public void SetScore(int score)
    {
        if (scoreText != null) scoreText.text = $"Score : {score}";
    }

    public void SetTimer(float seconds)
    {
        if (timerText == null) return;
        int s = Mathf.Max(0, Mathf.CeilToInt(seconds));
        int m = s / 60;
        s = s % 60;
        timerText.text = $"{m:00}:{s:00}";
    }

    public void SetObjectives(string txt)
    {
        if (objectivesText != null) objectivesText.text = txt;
    }

    public void ShowMessage(string txt)
    {
        if (messageText != null) messageText.text = txt ?? "";
    }
}
