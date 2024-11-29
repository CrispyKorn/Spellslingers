using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public enum ActionMap
    {
        None,
        Player,
        Menu
    }

    public enum PlayerInputDevice
    {
        KBM,
        Gamepad
    }

    #region Input Events
    #region Battle
    public event Action OnSelect_Started;
    public event Action OnSelect;
    public event Action OnSelect_Ended;

    public event Action OnFlip_Started;
    public event Action OnFlip;
    public event Action OnFlip_Ended;
    #endregion

    #region Menu
    public event Action<Vector2> OnNavigate;

    public event Action OnSubmit_Started;
    public event Action OnSubmit;
    public event Action OnSubmit_Ended;

    public event Action OnCancel_Started;
    public event Action OnCancel;
    public event Action OnCancel_Ended;
    #endregion
    #endregion

    private PlayerInput _playerInput;

    public ActionMap CurrentActionMap { get; private set; } = ActionMap.None;
    public PlayerInputDevice CurrentInputDevice { get; private set; } = PlayerInputDevice.KBM;

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);
        _playerInput = new PlayerInput();

        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        #region Battle
        _playerInput.Battle.Select.started += Select_started;
        _playerInput.Battle.Select.performed += Select_performed;
        _playerInput.Battle.Select.canceled += Select_canceled;

        _playerInput.Battle.Flip.started += Flip_started;
        _playerInput.Battle.Flip.performed += Flip_performed;
        _playerInput.Battle.Flip.canceled += Flip_canceled;
        #endregion

        #region Menu
        _playerInput.Menu.Navigate.performed += Navigate_performed;

        _playerInput.Menu.Submit.started += Submit_started;
        _playerInput.Menu.Submit.performed += Submit_performed;
        _playerInput.Menu.Submit.canceled += Submit_canceled;

        _playerInput.Menu.Cancel.started += Cancel_started;
        _playerInput.Menu.Cancel.performed += Cancel_performed;
        _playerInput.Menu.Cancel.canceled += Cancel_canceled;
        #endregion
    }

    

    private void OnDisable()
    {
        #region Battle
        _playerInput.Battle.Select.started += Select_started;
        _playerInput.Battle.Select.performed += Select_performed;
        _playerInput.Battle.Select.canceled += Select_canceled;

        _playerInput.Battle.Flip.started += Flip_started;
        _playerInput.Battle.Flip.performed += Flip_performed;
        _playerInput.Battle.Flip.canceled += Flip_canceled;
        #endregion

        #region Menu
        _playerInput.Menu.Navigate.performed -= Navigate_performed;

        _playerInput.Menu.Submit.started -= Submit_started;
        _playerInput.Menu.Submit.performed -= Submit_performed;
        _playerInput.Menu.Submit.canceled -= Submit_canceled;

        _playerInput.Menu.Cancel.started -= Cancel_started;
        _playerInput.Menu.Cancel.performed -= Cancel_performed;
        _playerInput.Menu.Cancel.canceled -= Cancel_canceled;
        #endregion
    }

    private void Start()
    {
        SetActionMap(ActionMap.Menu);
    }

    #region Input Methods
    #region Battle
    #region Select
    private void Select_started(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnSelect_Started?.Invoke();
    }

    private void Select_performed(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnSelect?.Invoke();
    }

    private void Select_canceled(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnSelect_Ended?.Invoke();
    }
    #endregion

    #region Flip
    private void Flip_started(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnFlip_Started?.Invoke();
    }

    private void Flip_performed(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnFlip?.Invoke();
    }

    private void Flip_canceled(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnFlip_Ended?.Invoke();
    }
    #endregion
    #endregion

    #region Menu
    #region Navigate
    private void Navigate_performed(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        var inputValue = context.ReadValue<Vector2>();
        OnNavigate?.Invoke(inputValue);
    }
    #endregion

    #region Submit
    private void Submit_started(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnSubmit_Started?.Invoke();
    }
    private void Submit_performed(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnSubmit?.Invoke();
    }
    private void Submit_canceled(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnSubmit_Ended?.Invoke();
    }
    #endregion

    #region Cancel
    private void Cancel_started(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnCancel_Started?.Invoke();
    }

    private void Cancel_performed(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnCancel?.Invoke();
    }

    private void Cancel_canceled(InputAction.CallbackContext context)
    {
        UpdateInputDevice(context.control.device);
        OnCancel_Ended?.Invoke();
    }
    #endregion
    #endregion
    #endregion

    private void UpdateInputDevice(InputDevice inputDevice)
    {
        if (inputDevice != null) CurrentInputDevice = PlayerInputDevice.Gamepad;
        if (inputDevice != null || inputDevice != null) CurrentInputDevice = PlayerInputDevice.KBM;
    }

    public void SetCursorMode(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void SetActionMap(ActionMap actionMap)
    {
        switch (actionMap)
        {
            case ActionMap.None:
                {
                    _playerInput.Battle.Disable();
                    _playerInput.Menu.Disable();
                }
                break;
            case ActionMap.Player:
                {
                    _playerInput.Battle.Enable();
                    _playerInput.Menu.Disable();
                }
                break;
            case ActionMap.Menu:
                {
                    _playerInput.Battle.Disable();
                    _playerInput.Menu.Enable();
                }
                break;
        }
    }
}