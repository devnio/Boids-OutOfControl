using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingZone : MonoBehaviour
{
    private bool exitState;

    private float timePassed;
    public float activeTime;
    public float rotSpeed;

    public float zoomAnimationTime;

    public void StartHealingZone() {
        this.exitState = false;
        this.timePassed = 0f;
        this.gameObject.SetActive(true);
        StartCoroutine(ZoomInOutAnimation(false));
    }

    public void EndHealingZone() {
        this.exitState = true;
        this.timePassed = 0f;
        StartCoroutine(ZoomInOutAnimation(true));
    }

    // Update is called once per frame
    void Update()
    {
        if (!exitState)
        {
            this.transform.Rotate(Vector3.forward, rotSpeed * Time.deltaTime);

            timePassed += Time.deltaTime;
            if (timePassed > activeTime)
            {
                this.EndHealingZone();
            }
        }
    }

    public IEnumerator ZoomInOutAnimation(bool outOrIn)
    {
        float start = 0f;
        float end = 1f;
        if (outOrIn)
        {
            start = 1f;
            end = 0f;
        }

        float sPassed = 0f;
        float percent = 0f;

        while (sPassed < zoomAnimationTime)
        {
            percent = Mathf.Clamp01(sPassed/zoomAnimationTime);

            Vector3 scale = this.transform.localScale;
            scale.x = Mathf.Lerp(start, end, percent);
            scale.y = Mathf.Lerp(start, end, percent);

            this.transform.localScale = scale;

            sPassed += Time.deltaTime;

            yield return null;
        }

        if (outOrIn)
        {
            this.gameObject.SetActive(false);
        }
    }
}
