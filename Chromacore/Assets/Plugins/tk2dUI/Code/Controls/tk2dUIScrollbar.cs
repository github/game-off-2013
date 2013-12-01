using UnityEngine;
using System.Collections;

/// <summary>
/// Scrollbar/Slider Control
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/UI/tk2dUIScrollbar")]
public class tk2dUIScrollbar : MonoBehaviour
{
    /// <summary>
    /// XAxis - horizontal, YAxis - vertical
    /// </summary>
    public enum Axes { XAxis, YAxis }

    /// <summary>
    /// Whole bar uiItem. Used to record clicks/touches to move thumb directly to that locations
    /// </summary>
    public tk2dUIItem barUIItem;

    /// <summary>
    /// Lenght of the scrollbar
    /// </summary>
    public float scrollBarLength;

    /// <summary>
    /// Scroll thumb button. Events will be taken from this.
    /// </summary>
    public tk2dUIItem thumbBtn;

    /// <summary>
    /// Generally same as thumbBtn, but sometimes you want a thumb that user can't interactive with.
    /// </summary>
    public Transform thumbTransform;

    /// <summary>
    /// Length of the scroll thumb
    /// </summary>
    public float thumbLength;

    /// <summary>
    /// Button up, moves list up. Not required.
    /// </summary>
    public tk2dUIItem upButton;

    //direct reference to hover button of up button (if exists)
    private tk2dUIHoverItem hoverUpButton;

    /// <summary>
    /// Button down, moves list down. Not required
    /// </summary>
    public tk2dUIItem downButton;

    //direct reference to hover button of up button (if exists)
    private tk2dUIHoverItem hoverDownButton;

    /// <summary>
    /// Disable up/down buttons will scroll
    /// </summary>
    public float buttonUpDownScrollDistance = 1f;

    /// <summary>
    /// Allows for mouse scroll wheel to scroll  list while hovered. Requires hover to be active.
    /// </summary>
    public bool allowScrollWheel = true;

    /// <summary>
    /// Axes while scrolling will occur.
    /// </summary>
    public Axes scrollAxes = Axes.YAxis;

    /// <summary>
    /// Highlighted progress bar control used to move a highlighted bar. Not required.
    /// </summary>
    public tk2dUIProgressBar highlightProgressBar;

	[SerializeField]
	[HideInInspector]
	private tk2dUILayout barLayoutItem = null;

	public tk2dUILayout BarLayoutItem {
		get { return barLayoutItem; }
		set {
			if (barLayoutItem != value) {
				if (barLayoutItem != null) {
					barLayoutItem.OnReshape -= LayoutReshaped;
				}
				barLayoutItem = value;
				if (barLayoutItem != null) {
					barLayoutItem.OnReshape += LayoutReshaped;
				}
			}
		}
	}

    private bool isScrollThumbButtonDown = false;
    private bool isTrackHoverOver = false;

    private float percent = 0; //0-1

    private Vector3 moveThumbBtnOffset = Vector3.zero;

    //which, if any up down scrollbuttons are currently down
    private int scrollUpDownButtonState = 0; //0=nothing, -1=up, 1-down
    private float timeOfUpDownButtonPressStart = 0; //time of scroll up/down button press
    private float repeatUpDownButtonHoldCounter = 0; //counts how many moves are made by holding

    private const float WITHOUT_SCROLLBAR_FIXED_SCROLL_WHEEL_PERCENT = .1f; //distance to be scroll if not attached to ScrollableArea

    private const float INITIAL_TIME_TO_REPEAT_UP_DOWN_SCROLL_BUTTON_SCROLLING_ON_HOLD = .55f;
    private const float TIME_TO_REPEAT_UP_DOWN_SCROLL_BUTTON_SCROLLING_ON_HOLD = .45f;

    /// <summary>
    /// Event, on scrolling
    /// </summary>
    public event System.Action<tk2dUIScrollbar> OnScroll;

    public string SendMessageOnScrollMethodName = "";

    public GameObject SendMessageTarget
    {
        get
        {
            if (barUIItem != null)
            {
                return barUIItem.sendMessageTarget;
            }
            else return null;
        }
        set
        {
            if (barUIItem != null && barUIItem.sendMessageTarget != value)
            {
                barUIItem.sendMessageTarget = value;
            
                #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(barUIItem);
                #endif
            }
        }
    }

    /// <summary>
    /// Percent scrolled. 0-1
    /// </summary>
    public float Value
    {
        get { return percent; }
        set
        {
            percent = Mathf.Clamp(value, 0f, 1f);
            if (OnScroll != null) { OnScroll(this); }
            SetScrollThumbPosition();

            if (SendMessageTarget != null && SendMessageOnScrollMethodName.Length > 0)
            {
                SendMessageTarget.SendMessage( SendMessageOnScrollMethodName, this, SendMessageOptions.RequireReceiver );
            }     
        }
    }

    /// <summary>
    /// Manually set scrolling percent without firing OnScroll event
    /// </summary>
    public void SetScrollPercentWithoutEvent(float newScrollPercent)
    {
        percent = Mathf.Clamp(newScrollPercent, 0f, 1f);
        SetScrollThumbPosition();
    }

    void OnEnable()
    {
        if (barUIItem != null)
        {
            barUIItem.OnDown += ScrollTrackButtonDown;
            barUIItem.OnHoverOver += ScrollTrackButtonHoverOver;
            barUIItem.OnHoverOut += ScrollTrackButtonHoverOut;
        }
        if (thumbBtn != null)
        {
            thumbBtn.OnDown += ScrollThumbButtonDown;
            thumbBtn.OnRelease += ScrollThumbButtonRelease;
        }

        if (upButton != null)
        {
            upButton.OnDown += ScrollUpButtonDown;
            upButton.OnUp += ScrollUpButtonUp;
        }

        if (downButton != null)
        {
            downButton.OnDown += ScrollDownButtonDown;
            downButton.OnUp += ScrollDownButtonUp;
        }

		if (barLayoutItem != null)
		{
			barLayoutItem.OnReshape += LayoutReshaped;
		}
    }

    void OnDisable()
    {
        if (barUIItem != null)
        {
            barUIItem.OnDown -= ScrollTrackButtonDown;
            barUIItem.OnHoverOver -= ScrollTrackButtonHoverOver;
            barUIItem.OnHoverOut -= ScrollTrackButtonHoverOut;
        }
        if (thumbBtn != null)
        {
            thumbBtn.OnDown -= ScrollThumbButtonDown;
            thumbBtn.OnRelease -= ScrollThumbButtonRelease;
        }

        if (upButton != null)
        {
            upButton.OnDown -= ScrollUpButtonDown;
            upButton.OnUp -= ScrollUpButtonUp;
        }

        if (downButton != null)
        {
            downButton.OnDown -= ScrollDownButtonDown;
            downButton.OnUp -= ScrollDownButtonUp;
        }

        if (isScrollThumbButtonDown)
        {
            if (tk2dUIManager.Instance__NoCreate != null)
            {
                tk2dUIManager.Instance.OnInputUpdate -= MoveScrollThumbButton;
            }
            isScrollThumbButtonDown = false;
        }

        if (isTrackHoverOver)
        {
            if (tk2dUIManager.Instance__NoCreate != null)
            {
                tk2dUIManager.Instance.OnScrollWheelChange -= TrackHoverScrollWheelChange;
            }
            isTrackHoverOver = false;
        }

        if (scrollUpDownButtonState != 0)
        {
            tk2dUIManager.Instance.OnInputUpdate -= CheckRepeatScrollUpDownButton;
            scrollUpDownButtonState = 0;
        }

		if (barLayoutItem != null)
		{
			barLayoutItem.OnReshape -= LayoutReshaped;
		}
    }

    void Awake()
    {
        if (upButton != null)
        {
            hoverUpButton = upButton.GetComponent<tk2dUIHoverItem>();
        }
        if (downButton != null)
        {
            hoverDownButton = downButton.GetComponent<tk2dUIHoverItem>();
        }
    }

    void Start()
    {
        SetScrollThumbPosition();
    }

    private void TrackHoverScrollWheelChange(float mouseWheelChange)
    {
        if (mouseWheelChange > 0)
        {
            ScrollUpFixed();
        }
        else if(mouseWheelChange<0)
        {
            ScrollDownFixed();
        }
    }

    private void SetScrollThumbPosition()
    {
        if (thumbTransform != null)
        {
            float pos= -((scrollBarLength - thumbLength) * Value)/* + ((scrollBarLength - thumbLength) / 2.0f)*/;

            Vector3 thumbLocalPos = thumbTransform.localPosition;
            if (scrollAxes == Axes.XAxis)
            {
                thumbLocalPos.x = -pos;
            }
            else if (scrollAxes == Axes.YAxis)
            {
                thumbLocalPos.y = pos;
            }
            thumbTransform.localPosition = thumbLocalPos;
        }

        if (highlightProgressBar != null)
        {
            highlightProgressBar.Value = Value;
        }
    }

    private void MoveScrollThumbButton()
    {
        ScrollToPosition(CalculateClickWorldPos(thumbBtn) + moveThumbBtnOffset);
    }

    private Vector3 CalculateClickWorldPos(tk2dUIItem btn)
    {
        Camera viewingCamera = tk2dUIManager.Instance.GetUICameraForControl( gameObject );
        Vector2 pos = btn.Touch.position;
        Vector3 worldPos = viewingCamera.ScreenToWorldPoint(new Vector3(pos.x, pos.y, btn.transform.position.z - viewingCamera.transform.position.z));
        worldPos.z = btn.transform.position.z;
        return worldPos;
    }


    private void ScrollToPosition(Vector3 worldPos)
    {
        Vector3 localPos=thumbTransform.parent.InverseTransformPoint(worldPos);

        float axisPos = 0;

        if (scrollAxes == Axes.XAxis)
        {
            axisPos = localPos.x;
        }
        else if (scrollAxes == Axes.YAxis)
        {
            axisPos = -localPos.y;
        }

        Value = (axisPos / (scrollBarLength - thumbLength));
    }

    private void ScrollTrackButtonDown()
    {
        ScrollToPosition(CalculateClickWorldPos(barUIItem));
    }

    private void ScrollTrackButtonHoverOver()
    {
        if (allowScrollWheel)
        {
            if (!isTrackHoverOver)
            {
                tk2dUIManager.Instance.OnScrollWheelChange += TrackHoverScrollWheelChange;
            }
            isTrackHoverOver = true;
        }
    }

    private void ScrollTrackButtonHoverOut()
    {
        if (isTrackHoverOver)
        {
            tk2dUIManager.Instance.OnScrollWheelChange -= TrackHoverScrollWheelChange;
        }
        isTrackHoverOver = false;
    }

    private void ScrollThumbButtonDown()
    {
        if (!isScrollThumbButtonDown)
        {
            tk2dUIManager.Instance.OnInputUpdate += MoveScrollThumbButton;
        }
        isScrollThumbButtonDown = true;

        Vector3 newWorldPos = CalculateClickWorldPos(thumbBtn);
        moveThumbBtnOffset = thumbBtn.transform.position - newWorldPos;
        moveThumbBtnOffset.z = 0;

        if (hoverUpButton != null)
        {
            hoverUpButton.IsOver = true;
        }
        if (hoverDownButton != null)
        {
            hoverDownButton.IsOver = true;
        }
    }

    private void ScrollThumbButtonRelease()
    {
        if (isScrollThumbButtonDown)
        {
            tk2dUIManager.Instance.OnInputUpdate -= MoveScrollThumbButton;
        }
        isScrollThumbButtonDown = false;

        if (hoverUpButton != null)
        {
            hoverUpButton.IsOver = false;
        }
        if (hoverDownButton != null)
        {
            hoverDownButton.IsOver = false;
        }
    }

    private void ScrollUpButtonDown()
    {
        timeOfUpDownButtonPressStart = Time.realtimeSinceStartup;
        repeatUpDownButtonHoldCounter = 0;
        if (scrollUpDownButtonState == 0)
        {
            tk2dUIManager.Instance.OnInputUpdate += CheckRepeatScrollUpDownButton;
        }
        scrollUpDownButtonState = -1;
 
        ScrollUpFixed();
    }

    private void ScrollUpButtonUp()
    {
        if (scrollUpDownButtonState != 0)
        {
            tk2dUIManager.Instance.OnInputUpdate -= CheckRepeatScrollUpDownButton;
        }
        scrollUpDownButtonState = 0;
    }

    private void ScrollDownButtonDown()
    {
        timeOfUpDownButtonPressStart = Time.realtimeSinceStartup;
        repeatUpDownButtonHoldCounter = 0;
        if (scrollUpDownButtonState == 0)
        {
            tk2dUIManager.Instance.OnInputUpdate += CheckRepeatScrollUpDownButton;
        }
        scrollUpDownButtonState = 1;
        ScrollDownFixed();
    }

    private void ScrollDownButtonUp()
    {
        if (scrollUpDownButtonState != 0)
        {
            tk2dUIManager.Instance.OnInputUpdate -= CheckRepeatScrollUpDownButton;
        }
        scrollUpDownButtonState = 0;
    }

    public void ScrollUpFixed()
    {
        ScrollDirection(-1);
    }

    public void ScrollDownFixed()
    {
        ScrollDirection(1);
    }

    private void CheckRepeatScrollUpDownButton()
    {
        if (scrollUpDownButtonState != 0)
        {
            float timePassed = Time.realtimeSinceStartup - timeOfUpDownButtonPressStart;

            if (repeatUpDownButtonHoldCounter == 0)
            {
                if (timePassed > INITIAL_TIME_TO_REPEAT_UP_DOWN_SCROLL_BUTTON_SCROLLING_ON_HOLD)
                {
                    repeatUpDownButtonHoldCounter++;
                    timePassed -= INITIAL_TIME_TO_REPEAT_UP_DOWN_SCROLL_BUTTON_SCROLLING_ON_HOLD;
                    ScrollDirection(scrollUpDownButtonState);
                }
            }
            else //greater then 0
            {
                if (timePassed > TIME_TO_REPEAT_UP_DOWN_SCROLL_BUTTON_SCROLLING_ON_HOLD)
                {
                    repeatUpDownButtonHoldCounter++;
                    timePassed -= TIME_TO_REPEAT_UP_DOWN_SCROLL_BUTTON_SCROLLING_ON_HOLD;
                    ScrollDirection(scrollUpDownButtonState);
                }
            }
        }
    }

    public void ScrollDirection(int dir)
    {
        if (scrollAxes == Axes.XAxis)
        {
            Value = Value - CalcScrollPercentOffsetButtonScrollDistance() * dir * buttonUpDownScrollDistance;
        }
        else
        {
            Value = Value + CalcScrollPercentOffsetButtonScrollDistance() * dir * buttonUpDownScrollDistance;
        }
    }

    private float CalcScrollPercentOffsetButtonScrollDistance()
    {
        return WITHOUT_SCROLLBAR_FIXED_SCROLL_WHEEL_PERCENT;
    }

	private void LayoutReshaped(Vector3 dMin, Vector3 dMax)
	{
		scrollBarLength += (scrollAxes == Axes.XAxis) ? (dMax.x - dMin.x) : (dMax.y - dMin.y);
	}
}
