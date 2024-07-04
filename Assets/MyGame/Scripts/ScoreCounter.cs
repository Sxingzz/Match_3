using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    public static ScoreCounter Instance {  get; private set; }

    private int _score;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    public int Score
    {
        get => _score;

        set
        {
            if (_score == value)
            {
                return;
            }
            _score = value;

            scoreText.SetText($"Score = {_score}");

        }
    }

    private void Awake()
    {
        Instance = this;
    }
}
