using UnityEngine;
using System.Collections;

/// <summary>
/// UpDown Button, changes state based on if it is up or down, plus hover
/// </summary>
[AddComponentMenu("2D Toolkit/UI/tk2dUIUpDownHoverButton")]
public class tk2dUIUpDownHoverButton : tk2dUIBaseItemControl
{
    /// <summary>
    /// State that will be active if up and deactivated if down
    /// </summary>
    public GameObject upStateGO;

    /// <summary>
    /// State that will be active if down and deactivated if up
    /// </summary>
    public GameObject downStateGO;

    /// <summary>
    /// State that will be active is up and hovering over
    /// </summary>
    public GameObject hoverOverStateGO;

    /// <summary>
    /// Use OnRelase instead of OnUp to toggle state
    /// </summary>
    [SerializeField]
    private bool useOnReleaseInsteadOfOnUp = false;

    public bool UseOnReleaseInsteadOfOnUp
    {
        get { return useOnReleaseInsteadOfOnUp; }
    }

    private bool isDown = false;
    private bool isHover = false;

    public string SendMessageOnToggleOverMethodName = "";

    /// <summary>
    /// Is mouse currently over
    /// </summary>
    public bool IsOver
    {
        get { return isDown || isHover; }
        set
        {
            if (value!=isDown||isHover)
            {
                if (value)
                {
                    isHover = true;
                    SetState();
                    if (OnToggleOver != null) { OnToggleOver(this); }
                }
                else
                {
                    if (isDown && isHover)
                    {
                        isDown = false;
                        isHover = false;
                        SetState();
                        if (OnToggleOver != null) { OnToggleOver(this); }
                    }
                    else if (isDown)
                    {
                        isDown = false;
                        SetState();
                        if (OnToggleOver != null) { OnToggleOver(this); }
                    }
                    else
                    {
                        isHover = false;
                        SetState();
                        if (OnToggleOver != null) { OnToggleOver(this); }
                    }
                }
                base.DoSendMessage( SendMessageOnToggleOverMethodName, this );
            }
        }
    }

    /// <summary>
    /// Event on hover status change
    /// </summary>
    public event System.Action<tk2dUIUpDownHoverButton> OnToggleOver;

    void Start()
    {
        SetState();
    }

    void OnEnable()
    {
        if (uiItem)
        {
            uiItem.OnDown += ButtonDown;
            if (useOnReleaseInsteadOfOnUp)
            {
                uiItem.OnRelease += ButtonUp;
            }
            else
            {
                uiItem.OnUp += ButtonUp;
            }

            uiItem.OnHoverOver += ButtonHoverOver;
            uiItem.OnHoverOut += ButtonHoverOut;
        }
    }

    void OnDisable()
    {
        if (uiItem)
        {
            uiItem.OnDown -= ButtonDown;
            if (useOnReleaseInsteadOfOnUp)
            {
                uiItem.OnRelease -= ButtonUp;
            }
            else
            {
                uiItem.OnUp -= ButtonUp;
            }

            uiItem.OnHoverOver -= ButtonHoverOver;
            uiItem.OnHoverOut -= ButtonHoverOut;
        }
    }

    private void ButtonUp()
    {
        if (isDown)
        {
            isDown = false;
            SetState();

            if (!isHover)
            {
                if (OnToggleOver != null) { OnToggleOver(this); }
            }
        }
    }

    private void ButtonDown()
    {
        if (!isDown)
        {
            isDown = true;
            SetState();

            if (!isHover)
            {
                if (OnToggleOver != null) { OnToggleOver(this); }
            }
        }
    }

    private void ButtonHoverOver()
    {
        if (!isHover)
        {
            isHover = true;
            SetState();

            if (!isDown)
            {
                if (OnToggleOver != null) { OnToggleOver(this); }
            }
        }
    }

    private void ButtonHoverOut()
    {
        if (isHover)
        {
            isHover = false;
            SetState();

            if (!isDown)
            {
                if (OnToggleOver != null) { OnToggleOver(this); }
            }
        }
    }

    public void SetState()
    {
        ChangeGameObjectActiveStateWithNullCheck(upStateGO, !isDown && !isHover);
        if (downStateGO == hoverOverStateGO)
        {
            ChangeGameObjectActiveStateWithNullCheck(downStateGO, isDown || isHover);
        }
        else
        {
            ChangeGameObjectActiveStateWithNullCheck(downStateGO, isDown);
            ChangeGameObjectActiveStateWithNullCheck(hoverOverStateGO, isHover);
        }
    }

    /// <summary>
    /// Internal do not use
    /// </summary>
    public void InternalSetUseOnReleaseInsteadOfOnUp(bool state)
    {
        useOnReleaseInsteadOfOnUp = state;
    }
}
