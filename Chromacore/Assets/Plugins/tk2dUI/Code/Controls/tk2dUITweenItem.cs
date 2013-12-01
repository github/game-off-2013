using UnityEngine;
using System.Collections;

/// <summary>
/// Will scale uiItem up and down, on press events
/// </summary>
[AddComponentMenu("2D Toolkit/UI/tk2dUITweenItem")]
public class tk2dUITweenItem : tk2dUIBaseItemControl
{
    private Vector3 onUpScale; //keeps track of original scale

    /// <summary>
    /// What it should scsale to onDown event
    /// </summary>
    public Vector3 onDownScale = new Vector3(.9f, .9f, .9f);

    /// <summary>
    /// How long the tween (scaling) should last in seconds. If set to 0 no tween is used, happens instantly.
    /// </summary>
    public float tweenDuration = .1f; 

    /// <summary>
    /// If button can be held down (will not be scale back to original until up/release)
    /// Can not be toggled at run-time
    /// </summary>
    public bool canButtonBeHeldDown = true; //can not be toggled at runtime

    /// <summary>
    /// If using canButtonBeHeldDown, if the scale back to original should happen onRelease event instead of onUp event
    /// </summary>
    [SerializeField]
    private bool useOnReleaseInsteadOfOnUp = false;

    public bool UseOnReleaseInsteadOfOnUp
    {
        get { return useOnReleaseInsteadOfOnUp; }
    }

    private bool internalTweenInProgress = false;
    private Vector3 tweenTargetScale = Vector3.one;
    private Vector3 tweenStartingScale = Vector3.one;
    private float tweenTimeElapsed = 0;

    void Awake()
    {
        onUpScale = transform.localScale;
    }

    void OnEnable()
    {
        if (uiItem)
        {
            uiItem.OnDown += ButtonDown;
            if (canButtonBeHeldDown)
            {
                if (useOnReleaseInsteadOfOnUp)
                {
                    uiItem.OnRelease += ButtonUp;
                }
                else
                {
                    uiItem.OnUp += ButtonUp;
                }
            }
        }
        internalTweenInProgress = false;
        tweenTimeElapsed = 0;
        transform.localScale = onUpScale;
    }

    void OnDisable()
    {
        if (uiItem)
        {
            uiItem.OnDown -= ButtonDown;
            if (canButtonBeHeldDown)
            {
                if (useOnReleaseInsteadOfOnUp)
                {
                    uiItem.OnRelease -= ButtonUp;
                }
                else
                {
                    uiItem.OnUp -= ButtonUp;
                }
            }
        }
    }

    private void ButtonDown()
    {
        if (tweenDuration <= 0)
        {
            transform.localScale = onDownScale;
        }
        else
        {
            transform.localScale = onUpScale;

            tweenTargetScale = onDownScale;
            tweenStartingScale = transform.localScale;
            if (!internalTweenInProgress)
            {
                StartCoroutine(ScaleTween());
                internalTweenInProgress = true;
            }
        }
    }

    private void ButtonUp()
    {
        if (tweenDuration <= 0)
        {
            transform.localScale = onUpScale;
        }
        else
        {
            tweenTargetScale = onUpScale;
            tweenStartingScale = transform.localScale;
            if (!internalTweenInProgress)
            {
                StartCoroutine(ScaleTween());
                internalTweenInProgress = true;
            }      
        }
    }

    private IEnumerator ScaleTween()
    {
        tweenTimeElapsed = 0;
        while (tweenTimeElapsed < tweenDuration)
        {
            transform.localScale = Vector3.Lerp(tweenStartingScale,tweenTargetScale,tweenTimeElapsed / tweenDuration);
            yield return null;
            tweenTimeElapsed += tk2dUITime.deltaTime;
        }
        transform.localScale = tweenTargetScale;
        internalTweenInProgress = false;

        //if button is not held down bounce it back
        if (!canButtonBeHeldDown)
        {
            if (tweenDuration <= 0)
            {
                transform.localScale = onUpScale;
            }
            else
            {
                tweenTargetScale = onUpScale;
                tweenStartingScale = transform.localScale;
                StartCoroutine(ScaleTween());
                internalTweenInProgress = true;
            }
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
