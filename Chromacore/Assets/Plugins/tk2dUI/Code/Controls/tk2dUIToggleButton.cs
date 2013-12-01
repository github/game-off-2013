using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/UI/tk2dUIToggleButton")]
public class tk2dUIToggleButton : tk2dUIBaseItemControl
{
    /// <summary>
    /// State that will be active if toggled off, deactivated if toggle on
    /// </summary>
    public GameObject offStateGO;

    /// <summary>
    /// State to be active if toggled on, deactivated if toggle off
    /// </summary>
    public GameObject onStateGO;

    /// <summary>
    /// If false toggles on click, if true toggles on button down
    /// </summary>
    public bool activateOnPress = false;

    [SerializeField]
    private bool isOn = true;
    
    private bool isInToggleGroup = false;

    /// <summary>
    /// Event, if toggled
    /// </summary>
    public event System.Action<tk2dUIToggleButton> OnToggle;

    /// <summary>
    /// Is toggle in On state
    /// </summary>
    public bool IsOn
    {
        get { return isOn; }
        set
        {
            if (isOn != value)
            {
                isOn = value;
                SetState();
                if (OnToggle != null) { OnToggle(this); }
            }

        }
    }

    /// <summary>
    /// If toggle is parent of toggle group
    /// </summary>
    public bool IsInToggleGroup
    {
        get { return isInToggleGroup; }
        set { isInToggleGroup = value; }
    }

    public string SendMessageOnToggleMethodName = "";

    void Start()
    {
        SetState();
    }

    void OnEnable()
    {
        if (uiItem)
        {
            uiItem.OnClick += ButtonClick;
            uiItem.OnDown += ButtonDown;
        }
    }

    void OnDisable()
    {
        if (uiItem)
        {
            uiItem.OnClick -= ButtonClick;
            uiItem.OnDown -= ButtonDown;
        }
    }

    private void ButtonClick()
    {
        if (!activateOnPress)
        {
            ButtonToggle();
        }
    }

    private void ButtonDown()
    {
        if (activateOnPress)
        {
            ButtonToggle();
        }
    }

    private void ButtonToggle()
    {
        if (!isOn || !isInToggleGroup)
        {
            isOn = !isOn;
            SetState();
            if (OnToggle != null) { OnToggle(this); }

            base.DoSendMessage( SendMessageOnToggleMethodName, this );
        }
    }

    private void SetState()
    {
        ChangeGameObjectActiveStateWithNullCheck(offStateGO, !isOn);
        ChangeGameObjectActiveStateWithNullCheck(onStateGO, isOn);
    }

}
