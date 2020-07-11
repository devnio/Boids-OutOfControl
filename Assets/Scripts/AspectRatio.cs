using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AspectRatio : MonoBehaviour
{
    void Start()
    {
        Camera.main.aspect = 16f/9f;    
    }
}
