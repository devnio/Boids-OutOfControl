using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : Singleton<InputManager>
{
    // public EventSystem
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !BoidManager.Instance.HealingCircle.gameObject.activeSelf)
        {   
            if (GameManager.Instance.UseHealthZone())
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0f;
                BoidManager.Instance.PlaceHealingCircle(worldPos);
            }
        }
    }
}
