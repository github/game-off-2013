using UnityEngine;
using System.Collections;

/// <summary>
/// UI primary class. All colliders that need to response to touch/mouse events need to have this attached. This will then handle and dispatch events
/// </summary>
[AddComponentMenu("2D Toolkit/UI/Core/tk2dUIItem")]
public class tk2dUIItem : MonoBehaviour
{
    /// <summary>
    /// Pressed
    /// </summary>
    public event System.Action OnDown;

    /// <summary>
    /// Unpressed, possibly on exit of collider area without releasing
    /// </summary>
    public event System.Action OnUp;

    /// <summary>
    /// Click - down and up while not leaving collider area
    /// </summary>
    public event System.Action OnClick;

    /// <summary>
    /// After on down, when touch/click is released (this could be anywhere)
    /// </summary>
    public event System.Action OnRelease;

    /// <summary>
    /// On mouse hover over (only if using mouse, hover is enabled(tk2dUIManager.areHoverEventsTracked) and if multi-touch is disabled(tk2dUIManager.useMultiTouch))
    /// </summary>
    public event System.Action OnHoverOver; //if mouse hover (only if using mouse and only if hover enabled in button)

    /// <summary>
    /// On mouse hover leaving collider (only if using mouse, hover is enabled(tk2dUIManager.areHoverEventsTracked) and if multi-touch is disabled(tk2dUIManager.useMultiTouch))
    /// </summary>
    public event System.Action OnHoverOut; //if mouse no longer hover (only if using mouse and only if hover enabled in button)

    /// <summary>
    /// Same as OnDown above, except returns this tk2dUIItem 
    /// </summary>
    public event System.Action<tk2dUIItem> OnDownUIItem;

    /// <summary>
    /// Same as OnUp above, except returns this tk2dUIItem 
    /// </summary>
    public event System.Action<tk2dUIItem> OnUpUIItem;

    /// <summary>
    /// Same as OnClick above, except returns this tk2dUIItem 
    /// </summary>
    public event System.Action<tk2dUIItem> OnClickUIItem;

    /// <summary>
    /// Same as OnRelease above, except returns this tk2dUIItem 
    /// </summary>
    public event System.Action<tk2dUIItem> OnReleaseUIItem;

    /// <summary>
    /// Same as OnHoverOver above, except returns this tk2dUIItem 
    /// </summary>
    public event System.Action<tk2dUIItem> OnHoverOverUIItem;

    /// <summary>
    /// Same as OnHoverOut above, except returns this tk2dUIItem 
    /// </summary>
    public event System.Action<tk2dUIItem> OnHoverOutUIItem;

    /// <summary>
    /// Target GameObject to SendMessage to. Use only if you want to use SendMessage, recommend using events instead if possble
    /// </summary>
    public GameObject sendMessageTarget = null;

    /// <summary>
    /// Function name to SendMessage OnDown
    /// </summary>
    public string SendMessageOnDownMethodName = "";

    /// <summary>
    /// Function name to SendMessage OnUp
    /// </summary>
    public string SendMessageOnUpMethodName = "";

    /// <summary>
    /// Function name to SendMessage OnClick
    /// </summary>
    public string SendMessageOnClickMethodName = "";
    /// <summary>
    /// Function name to SendMessage OnRelease
    /// </summary>
    public string SendMessageOnReleaseMethodName = "";

    /// <summary>
    /// If this UIItem is a hierarchy child of another UIItem and you wish to pass events between them
    /// </summary>
    [SerializeField]
    private bool isChildOfAnotherUIItem = false; //if it is a child of another MenuButton, if true will send events down to parents (only used in awake)

    /// <summary>
    /// If this UIItem is a hierarchy parent of another UIItem that is marked as isChildOfAnotherUIItem, and you wish to receive touch/click/hover events from child
    /// </summary>
    public bool registerPressFromChildren = false; //if you press a child, will this button also be marked as pressed

    /// <summary>
    /// If this UIItem should receive hover events (if hover enabled(tk2dUIManager.areHoverEventsTracked), mouse is being used and multi-touch is diabled(tk2dUIManager.useMultiTouch))
    /// </summary>
    public bool isHoverEnabled = false;

    public Transform[] editorExtraBounds = new Transform[0]; // This is used by the editor to include additional meshes when calculating bounds. Eg. label area in dropdown
    public Transform[] editorIgnoreBounds = new Transform[0]; // This is used by the editor to ignore certain meshes when calculating bounds. Eg. content in scrollable area
    private bool isPressed = false; //need to be listening to OnUp or OnClicked for this to show the current state
    private bool isHoverOver = false; //is currently hover over (only if hover enabld)
    private tk2dUITouch touch; //touch struct of the active touch
    private tk2dUIItem parentUIItem = null; //parent UIItem, only used if isChild is set

    void Awake()
    {
        if (isChildOfAnotherUIItem)
        {
            UpdateParent();
        }
    }

    void Start()
    {
        if (tk2dUIManager.Instance == null) {
            Debug.LogError("Unable to find tk2dUIManager. Please create a tk2dUIManager in the scene before proceeding.");
        }

        if (isChildOfAnotherUIItem && parentUIItem==null)
        {
            UpdateParent();
        }
    }

    /// <summary>
    /// If currently pressed (down)
    /// </summary>
    public bool IsPressed
    {
        get { return isPressed; }
    }

    /// <summary>
    /// If pressed active touch event
    /// </summary>
    public tk2dUITouch Touch
    {
        get { return touch; }
    }

    /// <summary>
    /// If set as child of another UIItem, this is that parentUIItem
    /// </summary>
    public tk2dUIItem ParentUIItem
    {
        get { return parentUIItem; }
    }

    //if you change the parent, call this, if isChild is false, will act as if it is true
    /// <summary>
    /// If you change the parent (in hierarchy) call this. If isChildOfAnotherUIItem is false, will act as if it is true
    /// </summary>
    public void UpdateParent()
    {
        parentUIItem = GetParentUIItem();
    }

    /// <summary>
    /// Manually setting specific UIItem parent
    /// </summary>
    public void ManuallySetParent(tk2dUIItem newParentUIItem)
    {
        parentUIItem = newParentUIItem;
    }

    /// <summary>
    /// Will remove parent and act as if isChildOfAnotherUIItem is false
    /// </summary>
    public void RemoveParent()
    {
        parentUIItem = null;
    }

    /// <summary>
    /// Touch press down (only call manually, if you need to simulate a touch)
    /// </summary>
    public bool Press(tk2dUITouch touch) //pressed down ontop of button
    {
        return Press(touch, null);
    }

    /// <summary>
    /// Touch press down (only call manually, if you need to simulate a touch). SentFromChild is the UIItem child it was sent from. If sentFromChild is 
    /// null that means it wasn't sent from a child
    /// </summary>
    /// /// <value>
    /// return true if newly pressed
    /// </value>
    public bool Press(tk2dUITouch touch, tk2dUIItem sentFromChild) //pressed down ontop of button
    {
        if (isPressed)
        {
            return false; //already pressed
        }
        if (!isPressed)
        {
            this.touch = touch;
            //if orginal press (not sent from child), or resgieterPressFromChildren is enabled
            if (registerPressFromChildren || sentFromChild == null)
            {
                if (enabled)
                {
                    isPressed = true;

                    if (OnDown != null) { OnDown(); }
                    if (OnDownUIItem != null) { OnDownUIItem(this); }
                    DoSendMessage( SendMessageOnDownMethodName );
                }
            }

            if (parentUIItem != null)
            {
                parentUIItem.Press(touch, this);
            }
        }
        return true; //newly touched
    }

    /// <summary>
    /// Fired every frame this touch is still down, regardless if button is down. Only call manually if you need to simulate a touch
    /// </summary>
    public void UpdateTouch(tk2dUITouch touch)
    {
        this.touch = touch;
        if (parentUIItem != null)
        {
            parentUIItem.UpdateTouch(touch);
        }
    }

    // A wrapper for sendmessage, either sends with no parameter, or this as parameter
    private void DoSendMessage( string methodName ) {
        if (sendMessageTarget != null && methodName.Length > 0) {
            sendMessageTarget.SendMessage( methodName, this, SendMessageOptions.RequireReceiver );
        }
    }

    /// <summary>
    /// Touch is released, if called without Exit being called means that it was released on top of button without leaving it.
    /// Only call manually if you need to simulate a touch.
    /// </summary>
    public void Release()
    {
        if (isPressed)
        {
            isPressed = false;

            if (OnUp != null) { OnUp(); }
            if (OnUpUIItem != null) { OnUpUIItem(this); }
            DoSendMessage( SendMessageOnUpMethodName );

            if (OnClick != null) { OnClick(); }
            if (OnClickUIItem != null) { OnClickUIItem(this); }
            DoSendMessage( SendMessageOnClickMethodName );
        }

        if (OnRelease != null) { OnRelease(); }
        if (OnReleaseUIItem != null) { OnReleaseUIItem(this); }
        DoSendMessage( SendMessageOnReleaseMethodName );

        if (parentUIItem != null)
        {
            parentUIItem.Release();
        }
    }

    /// <summary>
    /// Touch/mouse currently over UIItem. If exitting this button, but still overtop of another Button, this might be a parent. Checks if parent and
    /// does not exit.
    /// Only call manually if you need to simulate touch.
    /// </summary>
    public void CurrentOverUIItem(tk2dUIItem overUIItem)
    {
        if (overUIItem != this)
        {
            if (isPressed)
            {
                //check if overButton is child
                bool isUIItemChild = CheckIsUIItemChildOfMe(overUIItem);
                if (!isUIItemChild)
                {
                    Exit();
                    if (parentUIItem != null)
                    {
                        parentUIItem.CurrentOverUIItem(overUIItem);
                    }
                }
            }
            else
            {
                if (parentUIItem != null)
                {
                    parentUIItem.CurrentOverUIItem(overUIItem);
                }
            }
        }
    }

    /// <summary>
    /// Is uiItem a child of this current tk2dUIItem
    /// Only call manually if you need to simulate touch.
    /// </summary>
    public bool CheckIsUIItemChildOfMe(tk2dUIItem uiItem)
    {
        tk2dUIItem nextUIItem = null;
        bool result = false;

        if (uiItem != null)
        {
            nextUIItem = uiItem.parentUIItem;
        }

        while (nextUIItem != null)
        {
            if (nextUIItem == this)
            {
                result = true;
                break;
            }
            nextUIItem = nextUIItem.parentUIItem;
        }

        return result;
    }

    /// <summary>
    /// Exit uiItem. Does not cascade (need to manually call on all children if needed)
    /// Only call manually if you need to simulate touch.
    /// </summary>
    public void Exit()
    {
        if (isPressed)
        {
            isPressed = false;

            if (OnUp != null) { OnUp(); }
            if (OnUpUIItem != null) { OnUpUIItem(this); }
            DoSendMessage( SendMessageOnUpMethodName );
        }
    }

    /// <summary>
    /// Hover over item. Return true if this was prevHover.
    /// Only call manually if you need to simulate touch.
    /// </summary>
    public bool HoverOver(tk2dUIItem prevHover)
    {
        bool wasPrevHoverFound = false;
        tk2dUIItem nextUIItem = null;
        if (!isHoverOver)
        {
            if (OnHoverOver != null) { OnHoverOver(); }
            if (OnHoverOverUIItem != null) { OnHoverOverUIItem(this); }
            isHoverOver = true;
        }

        if (prevHover == this)
        {
            wasPrevHoverFound = true;
        }

        if (parentUIItem != null && parentUIItem.isHoverEnabled)
        {
            nextUIItem = parentUIItem;
        }

        if (nextUIItem == null)
        {
            return wasPrevHoverFound;
        }
        else
        {
            return nextUIItem.HoverOver(prevHover) || wasPrevHoverFound; //will return true once found
        }
    }

    /// <summary>
    /// Hover out item.
    /// Only call manually if you need to simulate touch.
    /// </summary>
    public void HoverOut(tk2dUIItem currHoverButton)
    {
        if (isHoverOver)
        {
            if (OnHoverOut != null) { OnHoverOut(); }
            if (OnHoverOutUIItem != null) { OnHoverOutUIItem(this); }
            isHoverOver = false;
        }

        if (parentUIItem != null && parentUIItem.isHoverEnabled)
        {
            if (currHoverButton == null)
            {
                parentUIItem.HoverOut(currHoverButton);
            }
            else
            {
                if (!parentUIItem.CheckIsUIItemChildOfMe(currHoverButton) && currHoverButton != parentUIItem)
                {
                    parentUIItem.HoverOut(currHoverButton);
                }
            }
        }

    }

    //determine what the parent UIItem is, is called is isChildOfUIItem is set to true
    private tk2dUIItem GetParentUIItem()
    {
        Transform next = transform.parent;
        tk2dUIItem nextUIItem;
        while (next != null)
        {
            nextUIItem = next.GetComponent<tk2dUIItem>();
            if (nextUIItem != null)
            {
                return nextUIItem;
            }
            next = next.parent;
        }
        return null;
    }

    /// <summary>
    /// Simluates a click event
    /// Only call manually if you need to simulate touch.
    /// </summary>
    public void SimulateClick()
    {
        if (OnDown != null) { OnDown(); }
        if (OnDownUIItem != null) { OnDownUIItem(this); }
        DoSendMessage( SendMessageOnDownMethodName );

        if (OnUp != null) { OnUp(); }
        if (OnUpUIItem != null) { OnUpUIItem(this); }
        DoSendMessage( SendMessageOnUpMethodName );

        if (OnClick != null) { OnClick(); }
        if (OnClickUIItem != null) { OnClickUIItem(this); }
        DoSendMessage( SendMessageOnClickMethodName );

        if (OnRelease != null) { OnRelease(); }
        if (OnReleaseUIItem != null) { OnReleaseUIItem(this); }
        DoSendMessage( SendMessageOnReleaseMethodName );
    }

    /// <summary>
    /// Internal do not call
    /// </summary>
    public void InternalSetIsChildOfAnotherUIItem(bool state)
    {
        isChildOfAnotherUIItem = state;
    }

    public bool InternalGetIsChildOfAnotherUIItem()
    {
        return isChildOfAnotherUIItem;
    }
}
