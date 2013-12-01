using UnityEngine;
using System.Collections;

/// <summary>
/// Scrollable Area Control. Can be actually by changing Value, external scrollbar or swipe gesture
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/UI/tk2dUIScrollableArea")]
public class tk2dUIScrollableArea : MonoBehaviour
{
    /// <summary>
    /// XAxis - horizontal, YAxis - vertical
    /// </summary>
    public enum Axes { XAxis, YAxis }

    /// <summary>
    /// Length of all the content in the scrollable area
    /// </summary>
    [SerializeField]
    private float contentLength = 1;

    public float ContentLength
    {
        get { return contentLength; }
        set 
        {
            ContentLengthVisibleAreaLengthChange(contentLength, value,visibleAreaLength,visibleAreaLength);
        }
    }

    /// <summary>
    /// Length of visible area of content, what can be seen
    /// </summary>
    [SerializeField]
    private float visibleAreaLength = 1;

    public float VisibleAreaLength
    {
        get { return visibleAreaLength; }
        set 
        {
            ContentLengthVisibleAreaLengthChange(contentLength, contentLength, visibleAreaLength, value);
        }
    }

    /// <summary>
    /// Transform the will be moved to scroll content. All content needs to be a child of this Transform
    /// </summary>
    public GameObject contentContainer;

    /// <summary>
    /// Scrollbar to be attached. Not required.
    /// </summary>
    public tk2dUIScrollbar scrollBar;

    /// <summary>
    /// Used to record swipe scrolling events
    /// </summary>
    public tk2dUIItem backgroundUIItem;

    /// <summary>
    /// Axes scrolling will happen on
    /// </summary>
    public Axes scrollAxes = Axes.YAxis;

    /// <summary>
    /// If swipe (gesture) scrolling is enabled
    /// </summary>
    public bool allowSwipeScrolling = true;

    /// <summary>
    /// If mouse will is enabled, hover needs to be active
    /// </summary>
    public bool allowScrollWheel = true;

	[SerializeField]
	[HideInInspector]
	private tk2dUILayout backgroundLayoutItem = null;

	public tk2dUILayout BackgroundLayoutItem {
		get { return backgroundLayoutItem; }
		set {
			if (backgroundLayoutItem != value) {
				if (backgroundLayoutItem != null) {
					backgroundLayoutItem.OnReshape -= LayoutReshaped;
				}
				backgroundLayoutItem = value;
				if (backgroundLayoutItem != null) {
					backgroundLayoutItem.OnReshape += LayoutReshaped;
				}
			}
		}
	}

	[SerializeField]
	[HideInInspector]
	private tk2dUILayoutContainer contentLayoutContainer = null;

	public tk2dUILayoutContainer ContentLayoutContainer {
		get { return contentLayoutContainer; }
		set {
			if (contentLayoutContainer != value) {
				if (contentLayoutContainer != null) {
					contentLayoutContainer.OnChangeContent -= ContentLayoutChangeCallback;
				}
				contentLayoutContainer = value;
				if (contentLayoutContainer != null) {
					contentLayoutContainer.OnChangeContent += ContentLayoutChangeCallback;
				}
			}
		}
	}

    private bool isBackgroundButtonDown = false;
    private bool isBackgroundButtonOver = false;

    private Vector3 swipeScrollingPressDownStartLocalPos = Vector3.zero;
    private Vector3 swipeScrollingContentStartLocalPos = Vector3.zero;
    private Vector3 swipeScrollingContentDestLocalPos = Vector3.zero;
    private bool isSwipeScrollingInProgress = false;
    private const float SWIPE_SCROLLING_FIRST_SCROLL_THRESHOLD = .02f; //at what point swipe scrolling will start moving the list
    private const float WITHOUT_SCROLLBAR_FIXED_SCROLL_WHEEL_PERCENT = .1f; //if not scrollbar attached how much scroll wheel will move list
    private Vector3 swipePrevScrollingContentPressLocalPos = Vector3.zero;
    private float swipeCurrVelocity = 0; //velocity of current frame (used for inertia swipe scrolling)
    private float snapBackVelocity = 0;

    public GameObject SendMessageTarget
    {
        get
        {
            if (backgroundUIItem != null)
            {
                return backgroundUIItem.sendMessageTarget;
            }
            else return null;
        }
        set
        {
            if (backgroundUIItem != null && backgroundUIItem.sendMessageTarget != value)
            {
                backgroundUIItem.sendMessageTarget = value;
            
                #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(backgroundUIItem);
                #endif
            }
        }
    }

    /// <summary>
    /// If scrollable area is being scrolled
    /// </summary>
    public event System.Action<tk2dUIScrollableArea> OnScroll;

    public string SendMessageOnScrollMethodName = "";

    private float percent = 0; //0-1

    /// <summary>
    /// Scroll position percent 0-1
    /// </summary>
    public float Value
    {
        get { return Mathf.Clamp01( percent ); }
        set
        {
            value = Mathf.Clamp(value, 0f, 1f);
            if (value != percent)
            {
                UnpressAllUIItemChildren();
                percent = value;
                if (OnScroll != null) { OnScroll(this); }

                TargetOnScrollCallback();
            }
            if (scrollBar != null) { scrollBar.SetScrollPercentWithoutEvent(percent); }
            SetContentPosition();
        }
    }

    /// <summary>
    /// Manually set scrolling percent without firing OnScroll event
    /// </summary>
    public void SetScrollPercentWithoutEvent(float newScrollPercent)
    {
        percent = Mathf.Clamp(newScrollPercent, 0f, 1f);
        UnpressAllUIItemChildren();
        if (scrollBar != null) { scrollBar.SetScrollPercentWithoutEvent(percent); }
        SetContentPosition();
    }

    /// <summary>
    /// Measures the content length. This isn't very fast, so if you know the content length
    /// it is often more efficient to tell it rather than asking it to measure the content.
    /// Returns the height in Unity units, of everything under the Content contentContainer.
    /// </summary>
    public float MeasureContentLength() {
        Vector3 vector3Min = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        Vector3 vector3Max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3[] minMax = new Vector3[] {
            vector3Max,
            vector3Min
        };
        Transform t = contentContainer.transform;
        GetRendererBoundsInChildren(t.worldToLocalMatrix, minMax, t);
        if (minMax[0] != vector3Max && minMax[1] != vector3Min) {
            minMax[0] = Vector3.Min(minMax[0], Vector3.zero);
            minMax[1] = Vector3.Max(minMax[1], Vector3.zero);
            return (scrollAxes == Axes.YAxis) ? (minMax[1].y - minMax[0].y) : (minMax[1].x - minMax[0].x);
        }
        else {
            Debug.LogError("Unable to measure content length");
            return VisibleAreaLength * 0.9f;
        }
    }

    void OnEnable()
    {
        if (scrollBar != null)
        {
            scrollBar.OnScroll += ScrollBarMove;
        }

        if (backgroundUIItem != null)
        {
            backgroundUIItem.OnDown += BackgroundButtonDown;
            backgroundUIItem.OnRelease += BackgroundButtonRelease;
            backgroundUIItem.OnHoverOver += BackgroundButtonHoverOver;
            backgroundUIItem.OnHoverOut += BackgroundButtonHoverOut;
        }

		if (backgroundLayoutItem != null)
		{
			backgroundLayoutItem.OnReshape += LayoutReshaped;
		}
		if (contentLayoutContainer != null)
		{
			contentLayoutContainer.OnChangeContent += ContentLayoutChangeCallback;
		}
    }

    void OnDisable()
    {
        if (scrollBar != null)
        {
            scrollBar.OnScroll -= ScrollBarMove;
        }

        if (backgroundUIItem != null)
        {
            backgroundUIItem.OnDown -= BackgroundButtonDown;
            backgroundUIItem.OnRelease -= BackgroundButtonRelease;
            backgroundUIItem.OnHoverOver -= BackgroundButtonHoverOver;
            backgroundUIItem.OnHoverOut -= BackgroundButtonHoverOut;
        }

        if (isBackgroundButtonOver)
        {
            if (tk2dUIManager.Instance__NoCreate != null)
            {
                tk2dUIManager.Instance.OnScrollWheelChange -= BackgroundHoverOverScrollWheelChange;
            }
            isBackgroundButtonOver = false;
        }

        if (isBackgroundButtonDown || isSwipeScrollingInProgress)
        {
            if (tk2dUIManager.Instance__NoCreate != null)
            {
                tk2dUIManager.Instance.OnInputUpdate -= BackgroundOverUpdate;
            }
            isBackgroundButtonDown = false;
            isSwipeScrollingInProgress = false;
        }

		if (backgroundLayoutItem != null)
		{
			backgroundLayoutItem.OnReshape -= LayoutReshaped;
		}
		if (contentLayoutContainer != null)
		{
			contentLayoutContainer.OnChangeContent -= ContentLayoutChangeCallback;
		}

        swipeCurrVelocity = 0;
    }

    void Start()
    {
        UpdateScrollbarActiveState();
    }

    private void BackgroundHoverOverScrollWheelChange(float mouseWheelChange)
    {
        if (mouseWheelChange > 0)
        {
            if (scrollBar)
            {
                scrollBar.ScrollUpFixed();
            }
            else
            {
                Value -= WITHOUT_SCROLLBAR_FIXED_SCROLL_WHEEL_PERCENT;
            }
        }
        else if (mouseWheelChange < 0)
        {
            if (scrollBar)
            {
                scrollBar.ScrollDownFixed();
            }
            else
            {
                Value += WITHOUT_SCROLLBAR_FIXED_SCROLL_WHEEL_PERCENT;
            }
        }
    }

    private void ScrollBarMove(tk2dUIScrollbar scrollBar)
    {
        Value = scrollBar.Value;
        isSwipeScrollingInProgress = false;
        if (isBackgroundButtonDown)
        {
            BackgroundButtonRelease();
        }
    }

    Vector3 ContentContainerOffset {
        get { 
            return Vector3.Scale(new Vector3(-1, 1, 1), contentContainer.transform.localPosition);
        }
        set {
            contentContainer.transform.localPosition = Vector3.Scale(new Vector3(-1, 1, 1), value);
        }
    }

    private void SetContentPosition()
    {
        Vector3 localPos = ContentContainerOffset;
        float pos = (contentLength - visibleAreaLength) * Value;

        if (pos < 0) { pos = 0; }

        if (scrollAxes == Axes.XAxis)
        {
            localPos.x = pos;
        }
        else if (scrollAxes == Axes.YAxis)
        {
            localPos.y = pos;
        }

        ContentContainerOffset = localPos;
    }

    private void BackgroundButtonDown()
    {
        if (allowSwipeScrolling && contentLength > visibleAreaLength)
        {
            if (!isBackgroundButtonDown && !isSwipeScrollingInProgress)
            {
                tk2dUIManager.Instance.OnInputUpdate += BackgroundOverUpdate;
            }
            swipeScrollingPressDownStartLocalPos = transform.InverseTransformPoint(CalculateClickWorldPos(backgroundUIItem));
            swipePrevScrollingContentPressLocalPos = swipeScrollingPressDownStartLocalPos;
            swipeScrollingContentStartLocalPos = ContentContainerOffset;
            swipeScrollingContentDestLocalPos = swipeScrollingContentStartLocalPos;
            isBackgroundButtonDown = true;
            swipeCurrVelocity = 0;
        }
    }

    private void BackgroundOverUpdate()
    {
        if (isBackgroundButtonDown)
        {
            UpdateSwipeScrollDestintationPosition();
        }
        if (isSwipeScrollingInProgress)
        {
            float newPercent = percent;
            float destValue = 0;
            if (scrollAxes == Axes.XAxis)
            {
                destValue = swipeScrollingContentDestLocalPos.x;
            }
            else if (scrollAxes == Axes.YAxis)
            {
                destValue = swipeScrollingContentDestLocalPos.y;
            }

            float minDest = 0;
            float maxDest = contentLength - visibleAreaLength;
            if (isBackgroundButtonDown)
            {
                if (destValue < minDest)
                {
                    destValue += (-destValue / visibleAreaLength) / 2;
                    if (destValue > minDest )
                    {
                        destValue =minDest;
                    }
                }
                else if (destValue > maxDest)
                {
                    destValue-= ((destValue-maxDest)/visibleAreaLength)/2;
                    
                    if (destValue< maxDest)
                    {
                        destValue=maxDest;
                    }
                }

                if (scrollAxes == Axes.XAxis)
                {
                    swipeScrollingContentDestLocalPos.x = destValue;
                }
                else if (scrollAxes == Axes.YAxis)
                {
                    swipeScrollingContentDestLocalPos.y = destValue;
                }

                newPercent = destValue / (contentLength - visibleAreaLength);
            }
            else //background button not down
            {
                float velocityThreshold = visibleAreaLength * 0.001f;
                if (destValue < minDest || destValue > maxDest)
                {
                    float target = ( destValue < minDest ) ? minDest : maxDest;
                    destValue = Mathf.SmoothDamp( destValue, target, ref snapBackVelocity, 0.05f, Mathf.Infinity, tk2dUITime.deltaTime );
                    if (Mathf.Abs(snapBackVelocity) < velocityThreshold) {
                        destValue = target;
                        snapBackVelocity = 0;
                    }
                    swipeCurrVelocity = 0;
                }
                else if (swipeCurrVelocity != 0) //velocity scrolling
                {
                    destValue += swipeCurrVelocity * tk2dUITime.deltaTime * 20; //swipe velocity multiplier
                    if (swipeCurrVelocity > velocityThreshold || swipeCurrVelocity < -velocityThreshold)
                    {
                        swipeCurrVelocity = Mathf.Lerp(swipeCurrVelocity, 0, tk2dUITime.deltaTime * 2.5f); //change multiplier to change slowdown velocity
                    }
                    else
                    {
                        swipeCurrVelocity = 0;
                    }
                }
                else
                {
                    isSwipeScrollingInProgress = false;
                    tk2dUIManager.Instance.OnInputUpdate -= BackgroundOverUpdate;
                }

                if (scrollAxes == Axes.XAxis)
                {
                    swipeScrollingContentDestLocalPos.x = destValue;
                }
                else if (scrollAxes == Axes.YAxis)
                {
                    swipeScrollingContentDestLocalPos.y = destValue;
                }

                newPercent = destValue / (contentLength - visibleAreaLength);
            }

            if (newPercent != percent) {
                percent = newPercent;
                ContentContainerOffset = swipeScrollingContentDestLocalPos;
                if (OnScroll != null) OnScroll(this);
                TargetOnScrollCallback();
            }

            if (scrollBar != null)
            {
                float scrollBarPercent = percent;
                if (scrollAxes == Axes.XAxis)
                {
                    scrollBarPercent = (ContentContainerOffset.x / (contentLength - visibleAreaLength));
                }
                else if (scrollAxes == Axes.YAxis)
                {
                    scrollBarPercent = (ContentContainerOffset.y / (contentLength - visibleAreaLength));

                }

                scrollBar.SetScrollPercentWithoutEvent(scrollBarPercent);
            }
        }
    }

    private void UpdateSwipeScrollDestintationPosition()
    {
        Vector3 currTouchPosLocal = transform.InverseTransformPoint(CalculateClickWorldPos(backgroundUIItem));
 
        // X axis is inverted
        Vector3 moveDiffVector = currTouchPosLocal - swipeScrollingPressDownStartLocalPos;
        moveDiffVector.x *= -1;

        float moveDiff = 0;
        if (scrollAxes == Axes.XAxis)
        {
            moveDiff = moveDiffVector.x;
            // Invert x axis
            swipeCurrVelocity = -(currTouchPosLocal.x - swipePrevScrollingContentPressLocalPos.x);
        }
        else if (scrollAxes == Axes.YAxis)
        {
            moveDiff = moveDiffVector.y;
            swipeCurrVelocity = currTouchPosLocal.y - swipePrevScrollingContentPressLocalPos.y;
        }
        if (!isSwipeScrollingInProgress)
        {
            if (Mathf.Abs(moveDiff) > SWIPE_SCROLLING_FIRST_SCROLL_THRESHOLD)
            {
                isSwipeScrollingInProgress = true;

                //unpress anything currently pressed in list
                tk2dUIManager.Instance.OverrideClearAllChildrenPresses(backgroundUIItem);
            }
        }
        if (isSwipeScrollingInProgress)
        {
            Vector3 destContentPos = swipeScrollingContentStartLocalPos + moveDiffVector;
            destContentPos.z = ContentContainerOffset.z;

            if (scrollAxes == Axes.XAxis)
            {
                destContentPos.y = ContentContainerOffset.y;        
            }
            else if (scrollAxes == Axes.YAxis)
            {
                destContentPos.x = ContentContainerOffset.x;
            }
            destContentPos.z = ContentContainerOffset.z;

            swipeScrollingContentDestLocalPos = destContentPos;
            swipePrevScrollingContentPressLocalPos = currTouchPosLocal;
        }
    }

    private void BackgroundButtonRelease()
    {
        if (allowSwipeScrolling)
        {
            if (isBackgroundButtonDown)
            {
                if (!isSwipeScrollingInProgress)
                {
                    tk2dUIManager.Instance.OnInputUpdate -= BackgroundOverUpdate;
                }
            }
            isBackgroundButtonDown = false;
        }
    }

    private void BackgroundButtonHoverOver()
    {
        if (allowScrollWheel)
        {
            if (!isBackgroundButtonOver)
            {
                tk2dUIManager.Instance.OnScrollWheelChange += BackgroundHoverOverScrollWheelChange;
            }
            isBackgroundButtonOver = true;
        }
    }

    private void BackgroundButtonHoverOut()
    {
        if (isBackgroundButtonOver)
        {
            tk2dUIManager.Instance.OnScrollWheelChange -= BackgroundHoverOverScrollWheelChange;
        }

        isBackgroundButtonOver = false;
    }

    private Vector3 CalculateClickWorldPos(tk2dUIItem btn)
    {
        Vector2 pos = btn.Touch.position;
        Camera viewingCamera = tk2dUIManager.Instance.GetUICameraForControl( gameObject );
        Vector3 worldPos = viewingCamera.ScreenToWorldPoint(new Vector3(pos.x, pos.y, btn.transform.position.z - viewingCamera.transform.position.z));
        worldPos.z = btn.transform.position.z;
        return worldPos;
    }

    private void UpdateScrollbarActiveState()
    {
        bool scrollBarVisible = (contentLength > visibleAreaLength);
        if (scrollBar != null)
        {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
            if (scrollBar.gameObject.active != scrollBarVisible)
#else
            if (scrollBar.gameObject.activeSelf != scrollBarVisible)
#endif
            {
                tk2dUIBaseItemControl.ChangeGameObjectActiveState(scrollBar.gameObject, scrollBarVisible);
            }
        }
    }

    private void ContentLengthVisibleAreaLengthChange(float prevContentLength,float newContentLength,float prevVisibleAreaLength,float newVisibleAreaLength)
    {
        float newValue;
        if (newContentLength-visibleAreaLength!=0)
        {
            newValue = ((prevContentLength - prevVisibleAreaLength) * Value) / (newContentLength - newVisibleAreaLength);
        }
        else
        {
            newValue = 0;
        }

        contentLength = newContentLength;
        visibleAreaLength = newVisibleAreaLength;
        UpdateScrollbarActiveState();
        Value = newValue;
    }

    private void UnpressAllUIItemChildren()
    {
    }

    private void TargetOnScrollCallback()
    {
        if (SendMessageTarget != null && SendMessageOnScrollMethodName.Length > 0)
        {
            SendMessageTarget.SendMessage( SendMessageOnScrollMethodName, this, SendMessageOptions.RequireReceiver );
        }   
    }


    private static readonly Vector3[] boxExtents = new Vector3[] {
        new Vector3(-1, -1, -1), new Vector3( 1, -1, -1), new Vector3(-1,  1, -1), new Vector3( 1,  1, -1), new Vector3(-1, -1,  1), new Vector3( 1, -1,  1), new Vector3(-1,  1,  1), new Vector3( 1,  1,  1)
    };

    private static void GetRendererBoundsInChildren(Matrix4x4 rootWorldToLocal, Vector3[] minMax, Transform t) {
        MeshFilter mf = t.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null) {
            Bounds b = mf.sharedMesh.bounds;
            Matrix4x4 relativeMatrix = rootWorldToLocal * t.localToWorldMatrix;
            for (int j = 0; j < 8; ++j) {
                Vector3 localPoint = b.center + Vector3.Scale(b.extents, boxExtents[j]);
                Vector3 pointRelativeToRoot = relativeMatrix.MultiplyPoint(localPoint);
                minMax[0] = Vector3.Min(minMax[0], pointRelativeToRoot);
                minMax[1] = Vector3.Max(minMax[1], pointRelativeToRoot);
            }
        }
        int childCount = t.childCount;
        for (int i = 0; i < childCount; ++i) {
            Transform child = t.GetChild(i);
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
            if (t.gameObject.active) {
#else
            if (t.gameObject.activeSelf) {
#endif
                GetRendererBoundsInChildren(rootWorldToLocal, minMax, child);
            }
        }
    }   

	private void LayoutReshaped(Vector3 dMin, Vector3 dMax)
	{
		VisibleAreaLength += (scrollAxes == Axes.XAxis) ? (dMax.x - dMin.x) : (dMax.y - dMin.y);
	}

	private void ContentLayoutChangeCallback()
	{
		if (contentLayoutContainer != null) {
			Vector2 contentSize = contentLayoutContainer.GetInnerSize();
			ContentLength = (scrollAxes == Axes.XAxis) ? contentSize.x : contentSize.y;
		}
	}
}
