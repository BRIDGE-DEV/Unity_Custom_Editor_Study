using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InItTest : MonoBehaviour
{
    [SerializeField] private GameObject userInputStor;
    
    private void Awake()
    {
        Instantiate(userInputStor);
    }
}
