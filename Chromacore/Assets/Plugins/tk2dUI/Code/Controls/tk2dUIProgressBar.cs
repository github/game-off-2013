using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/UI/tk2dUIProgressBar")]
public class tk2dUIProgressBar : MonoBehaviour
{
    /// <summary>
    /// Event, if progress becomes 1, is complete
    /// </summary>
    public event System.Action OnProgressComplete;

    /// <summary>
    /// Transform that will be scaled from 0 to 1 on X-axis, used to move bar (this will be used instead of clippedSpriteBar)
    /// </summary>
    public Transform scalableBar;

    /// <summary>
    /// This will clip the sprite from right to left based on the progress (this will be used instead of scalableBar)
    /// </summary>
    public tk2dClippedSprite clippedSpriteBar; 

    /// <summary>
    /// This will clip the sprite from right to left based on the progress (this will be used instead of scalableBar or clippedSpriteBar)
    /// </summary>
    public tk2dSlicedSprite slicedSpriteBar;

    bool initializedSlicedSpriteDimensions = false;
    Vector2 emptySlicedSpriteDimensions = Vector2.zero;
    Vector2 fullSlicedSpriteDimensions = Vector2.zero;
    Vector2 currentDimensions = Vector2.zero;

    [SerializeField]
    private float percent = 0; //0 - 1

    private bool isProgressComplete = false;

    /// <summary>
    /// Target GameObject to SendMessage to. Use only if you want to use SendMessage, recommend using events instead if possble
    /// </summary>
    public GameObject sendMessageTarget = null;

    public string SendMessageOnProgressCompleteMethodName = "";

    void Start() 
    {
        InitializeSlicedSpriteDimensions();
        Value = percent;
    }

    /// <summary>
    /// Percent complete, between 0-1
    /// </summary>
    public float Value
    {
        get { return percent; }
        set
        {
            percent = Mathf.Clamp(value, 0f, 1f);
            if (Application.isPlaying) {
                if (clippedSpriteBar != null)
                {
                    clippedSpriteBar.clipTopRight = new Vector2(Value, 1);
                }
                else if (scalableBar != null)
                {
                    scalableBar.localScale = new Vector3(Value, scalableBar.localScale.y, scalableBar.localScale.z);
                }
                else if (slicedSpriteBar != null)
                {
                    InitializeSlicedSpriteDimensions();
                    float slicedLength = Mathf.Lerp( emptySlicedSpriteDimensions.x, fullSlicedSpriteDimensions.x, Value );
                    currentDimensions.Set( slicedLength, fullSlicedSpriteDimensions.y );
                    slicedSpriteBar.dimensions = currentDimensions;
                }
                
                if (!isProgressComplete && Value == 1)
                {
                    isProgressComplete = true;
                    if (OnProgressComplete != null) { OnProgressComplete(); }
    
                    if (sendMessageTarget != null && SendMessageOnProgressCompleteMethodName.Length > 0)
                    {
                        sendMessageTarget.SendMessage( SendMessageOnProgressCompleteMethodName, this, SendMessageOptions.RequireReceiver );
                    }     
                }
                else if (isProgressComplete && Value < 1)
                {
                    isProgressComplete = false;
                }
            }
        }
    }

    void InitializeSlicedSpriteDimensions() {
        if (!initializedSlicedSpriteDimensions) {
            if (slicedSpriteBar != null) 
            {
                // Until there is a better way to do this.
                tk2dSpriteDefinition spriteDef = slicedSpriteBar.CurrentSprite;
                Vector3 extents = spriteDef.boundsData[1];
                fullSlicedSpriteDimensions = slicedSpriteBar.dimensions;
                emptySlicedSpriteDimensions.Set( (slicedSpriteBar.borderLeft + slicedSpriteBar.borderRight) * extents.x / spriteDef.texelSize.x,
                                                 fullSlicedSpriteDimensions.y );
            }
            initializedSlicedSpriteDimensions = true;
        }
    }
}
