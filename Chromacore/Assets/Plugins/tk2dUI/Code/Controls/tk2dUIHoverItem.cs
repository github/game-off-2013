using UnityEngine;
using System.Collections;

/// <summary>
/// On HoverOver and HoverOut will switch states. Hover needs to be enabled to work (Hover actived(tk2dUIManager.areHoverEventsTracked), using a mouse
/// and mult-touch is disabled (tk2dUIManager.useMultiTouch)
/// </summary>
[AddComponentMenu("2D Toolkit/UI/tk2dUIHoverItem")]
public class tk2dUIHoverItem : tk2dUIBaseItemControl
{
    /// <summary>
    /// This GameObject will be set to active if hover is not over. Deactivated if hover is over.
    /// </summary>
    public GameObject outStateGO;

    /// <summary>
    /// This GameObject will be set to active if hover is over. Deactivated if hover is out.
    /// </summary>
    public GameObject overStateGO;

    private bool isOver = false; //is currently over

    /// <summary>
    /// Event on hover status change
    /// </summary>
    public event System.Action<tk2dUIHoverItem> OnToggleHover;

    public string SendMessageOnToggleHoverMethodName = "";

    /// <summary>
    /// Is mouse currently over
    /// </summary>
    public bool IsOver
    {
        get { return isOver; }
        set
        {
            if (isOver != value)
            {
                isOver = value;
                SetState();
                if (OnToggleHover != null) { OnToggleHover(this); }
                base.DoSendMessage( SendMessageOnToggleHoverMethodName, this );
            }
        }
    }

    void Start()
    {
        SetState();
    }


    void OnEnable()
    {
        if (uiItem)
        {
            uiItem.OnHoverOver += HoverOver;
            uiItem.OnHoverOut += HoverOut;
        }
    }

    void OnDisable()
    {
        if (uiItem)
        {
            uiItem.OnHoverOver -= HoverOver;
            uiItem.OnHoverOut -= HoverOut;
        }
    }

    private void HoverOver()
    {
        IsOver = true;
    }

    private void HoverOut()
    {
        IsOver = false;
    }

    /// <summary>
    /// Manually updates state based on IsOver
    /// </summary>
    public void SetState()
    {
        ChangeGameObjectActiveStateWithNullCheck(overStateGO, isOver);
        ChangeGameObjectActiveStateWithNullCheck(outStateGO, !isOver);
    }
}
