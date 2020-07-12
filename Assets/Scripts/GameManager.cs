using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    private float score;
    private float cash;

    private float prevScore;
    private float prevCash;

    private float scoreSpeed = 6f;
    private float cashSpeed = 4f;

    public float healthZoneCost;
    public float separatorCost;

    void Start()
    {
        score = 0f;
    }

    void Update()
    {
        this.score += Time.deltaTime * this.scoreSpeed;
        this.cash += Time.deltaTime * this.cashSpeed;

        if (this.prevCash > this.cash - (Time.deltaTime * this.cashSpeed) + Time.deltaTime)
        {
            UIManager.Instance.ColorCashText(Color.red);
        }
        else
        {
            UIManager.Instance.ColorCashText(Color.white);
        }

        UIManager.Instance.UpdateScore(this.score);
        UIManager.Instance.UpdateCash(this.cash);

        this.prevScore = this.score;
        this.prevCash = this.cash;
    }

    public void AddCash(float cash)
    {
        this.cash += cash;
    }

    public bool UseHealthZone()
    {
        return WithdrawCash(healthZoneCost);
    }

    public bool UseSeparator()
    { 
        if (this.cash < 1)
        {
            BoidManager.Instance.DeactivateSeparation();  
        } 
        return WithdrawCash(separatorCost * Time.deltaTime);
    }

    private bool WithdrawCash(float cash)
    {
        if (cash < this.cash)
        {
            this.cash -= cash;
            return true;
        }
        return false;
    }
}
