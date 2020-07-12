using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public Button SeparateButton;
    
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI cashText;

    public void UpdateScore(float score)
    {
        this.scoreText.text = score.ToString("F0");
        Debug.Log("Score : " + score.ToString("F0"));
    }

    public void UpdateCash(float cash)
    {
        this.cashText.text = cash.ToString("F0"); 
    }

    public void SeparateButtonPress()
    {
        BoidManager.Instance.ActivateSeparation();
    }

    public void SeparateButtonRelease()
    {
        BoidManager.Instance.DeactivateSeparation();
    }

    public void ColorCashText(Color col)
    {
        this.cashText.color = col;
    }
}

