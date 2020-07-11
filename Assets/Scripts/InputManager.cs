using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BoidManager.Instance.InstantiateBoid();
        }
    }
}
