using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput instance;
    public Controls inputActions;

    private void Awake()
    {
        instance = this;
        inputActions = new Controls();
    }

    private void OnEnable()
    {
        inputActions.Battle.Enable();
    }

    private void OnDisable()
    {
        inputActions.Battle.Disable();
    }
}
