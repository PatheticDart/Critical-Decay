using UnityEngine;
using TMPro;

public class ScoreHandler : MonoBehaviour
{
    public int score = 0;
    public TMP_Text scoreText;

    public void AddScore(int points)
    {
        score += points;
        scoreText.text = score.ToString();
    }
}
