using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI logic. Manages all UI, raycasting, touch/mouse events passing and uiItem event notifications. 
/// For any tk2dUIItem to work there needs to be a tk2dUIManager in the scene
/// </summary>
[AddComponentMenu("2D Toolkit/UI/Core/tk2dUIManager")]
public class tk2dUIManager : MonoBehaviour
{
    public static double version = 1.0;
    public static int releaseId = 0; // < -10000 = alpha, other negative = beta release, 0 = final, positive = final patch

    /// <summary>
    ///  Used to reference tk2dUIManager without a direct reference via singleton structure (will not auto-create and only one can exist at a time)
    /// </summary>
    private static tk2dUIManager instance;
    public static tk2dUIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(tk2dUIManager)) as tk2dUIManager;
                if (instance == null) {
                    GameObject go = new GameObject("tk2dUIManager");
                    instance = go.AddComponent<tk2dUIManager>();
                }
            }
            return instance;
        }
    }
	
	public static tk2dUIManager Instance__NoCreate {
		get {
			return instance;
		}
	}
	
    /// <summary>
    /// UICamera which raycasts for mouse/touch inputs are cast from
    /// </summary>
    [SerializeField]
#pragma warning disable 649
	private Camera uiCamera;
#pragma warning restore 649
	
    public Camera UICamera
    {
        get { return uiCamera; }
        set { uiCamera = value; }
    }

    public Camera GetUICameraForControl( GameObject go ) {
        int layer = 1 << go.layer;
        foreach (tk2dUICamera camera in allCameras) {
            if ((camera.FilteredMask & layer) != 0) {
                return camera.HostCamera;
            }
        }
        Debug.LogError("Unable to find UI camera for " + go.name);
        return null;
    }

    static List<tk2dUICamera> allCameras = new List<tk2dUICamera>();
    List<tk2dUICamera> sortedCameras = new List<tk2dUICamera>();

    public static void RegisterCamera(tk2dUICamera cam) {
        allCameras.Add(cam);
    }

    public static void UnregisterCamera(tk2dUICamera cam) {
        allCameras.Remove(cam);
    }

    /// <summary>
    /// Which layer the raycast will be done using. Will ignore others
    /// </summary>
    public LayerMask raycastLayerMask = -1;

    private bool inputEnabled = true;

    /// <summary>
    /// Used to disable user input
    /// </summary>
    public bool InputEnabled
    {
        get { return inputEnabled; }
        set 
        {
            if (inputEnabled && !value) //disable current presses/hovers
            {
                SortCameras();

                inputEnabled = value;
                if (useMultiTouch)
                {
                    CheckMultiTouchInputs();
                }
                else
                {
                    CheckInputs();
                }
            }
            else
            {
                inputEnabled = value;
            }

        }
    }

    /// <summary>
    /// If hover events are to be tracked. If you don't not need hover events disable for increased performance (less raycasting).
    /// Only works when using mouse and multi-touch is disabled (tk2dUIManager.useMultiTouch)
    /// </summary>
    public bool areHoverEventsTracked = true; //if disable no hover events will be tracked (less overhead), only effects if using mouse

    private tk2dUIItem pressedUIItem = null;

    /// <summary>
    /// Most recent pressedUIItem (if one is pressed). If multi-touch gets the most recent
    /// </summary>
    public tk2dUIItem PressedUIItem
    {
        get 
        {
            //if multi-touch return the last item (most recent touch)
            if (useMultiTouch)
            {
                if (pressedUIItems.Length > 0)
                {
                    return pressedUIItems[pressedUIItems.Length-1];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return pressedUIItem;
            }
        }
    }

    /// <summary>
    /// All currently pressed down UIItems if using multi-touch
    /// </summary>
    public tk2dUIItem[] PressedUIItems
    {
        get { return pressedUIItems; }
    }

    private tk2dUIItem overUIItem = null; //current hover over uiItem
    private tk2dUITouch firstPressedUIItemTouch; //used to determine deltas

    private bool checkForHovers = true; //if internal we are checking for hovers

    /// <summary>
    /// Use multi-touch. Only enable if you need multi-touch support. When multi-touch is enabled, hover events are disabled.
    /// Multi-touch disable will provide better performance and accuracy.
    /// </summary>
    [SerializeField]
    private bool useMultiTouch = false;

    public bool UseMultiTouch
    {
        get { return useMultiTouch; }
        set 
        {
            if (useMultiTouch != value && inputEnabled) //changed
            {
                InputEnabled = false; //clears existing
                useMultiTouch = value;
                InputEnabled = true;
            }
            else
            {
                useMultiTouch = value;
            }
        }
    }

    //multi-touch specifics
    private const int MAX_MULTI_TOUCH_COUNT = 5; //number of touches at the same time
    private tk2dUITouch[] allTouches = new tk2dUITouch[MAX_MULTI_TOUCH_COUNT]; //all multi-touches
    private List<tk2dUIItem> prevPressedUIItemList = new List<tk2dUIItem>(); //previous pressed and still pressed UIItems
    private tk2dUIItem[] pressedUIItems = new tk2dUIItem[MAX_MULTI_TOUCH_COUNT]; //pressed UIItem on the this frame

    private int touchCounter = 0; //touchs counter
    private Vector2 mouseDownFirstPos = Vector2.zero; //used to determine mouse position deltas while in multi-touch
    //end multi-touch specifics

    private const string MOUSE_WHEEL_AXES_NAME = "Mouse ScrollWheel"; //Input name of mouse scroll wheel

    //for update loop
    private tk2dUITouch primaryTouch = new tk2dUITouch(); //main touch (generally began, or actively press)
    private tk2dUITouch secondaryTouch = new tk2dUITouch(); //secondary touches (generally other fingers)
    private tk2dUITouch resultTouch = new tk2dUITouch(); //which touch is selected between primary and secondary
    private tk2dUIItem hitUIItem; //current uiItem hit
    private RaycastHit hit;
    private Ray ray;

    //for update loop (multi-touch)
    private tk2dUITouch currTouch;
    private tk2dUIItem currPressedItem;
    private tk2dUIItem prevPressedItem;

    /// <summary>
    /// Only any touch began or mouse click
    /// </summary>
    public event System.Action OnAnyPress;
    /// <summary>
    /// Fired at the end of every update
    /// </summary>
    public event System.Action OnInputUpdate;

    /// <summary>
    /// On mouse scroll wheel change (return direction of mouse scroll wheel change)
    /// </summary>
    public event System.Action<float> OnScrollWheelChange;

    void SortCameras() {
        sortedCameras.Clear();
        foreach (tk2dUICamera c in allCameras) {
            if (c != null) {
                sortedCameras.Add( c );
            }
        }

        // Largest depth gets priority
        sortedCameras.Sort( (a, b) => b.camera.depth.CompareTo( a.camera.depth ) );
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (Application.isPlaying) {
                DontDestroyOnLoad( gameObject );
            }
        }
        else
        {
            //can only be one tk2dUIManager at one-time, if another one is found Destroy it
            if (instance != this)
            {
                Debug.Log("Discarding unnecessary tk2dUIManager instance.");
                return;
            }
        } 

        tk2dUITime.Init();
        Setup();
    }

    void Start() {
        if (uiCamera != null && uiCamera.GetComponent<tk2dUICamera>() == null) {
            Debug.Log("It is no longer necessary to hook up a camera to the tk2dUIManager. You can simply attach a tk2dUICamera script to the cameras that interact with UI.");

            // Handle registration properly
            tk2dUICamera cam = uiCamera.gameObject.AddComponent<tk2dUICamera>();
            cam.AssignRaycastLayerMask( raycastLayerMask );
            
            uiCamera = null;
        }

        if (allCameras.Count == 0) {
            Debug.LogError("Unable to find any tk2dUICameras, and no cameras are connected to the tk2dUIManager. You will not be able to interact with the UI.");
        }
    }

    private void Setup()
    {
        if (!areHoverEventsTracked)
        {
            checkForHovers = false;
        }
    }

    void Update()
    {
        tk2dUITime.Update();

        if (inputEnabled)
        {
            SortCameras();

            if (useMultiTouch)
            {
                CheckMultiTouchInputs();
            }
            else
            {
                CheckInputs();
            }

            if (OnInputUpdate != null) { OnInputUpdate(); }

            if (OnScrollWheelChange != null)
            {
                float mouseWheel = Input.GetAxis(MOUSE_WHEEL_AXES_NAME);
                if (mouseWheel != 0)
                {
                    OnScrollWheelChange(mouseWheel);
                }
            }
        }
    }

    //checks for inputs (non-multi-touch)
    private void CheckInputs()
    {
        bool isPrimaryTouchFound = false;
        bool isSecondaryTouchFound = false;
        bool isAnyPressBeganRecorded = false;
        primaryTouch = new tk2dUITouch();
        secondaryTouch = new tk2dUITouch();
        resultTouch = new tk2dUITouch();
        hitUIItem = null;

        if (inputEnabled)
        {
            if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        primaryTouch = new tk2dUITouch(touch);
                        isPrimaryTouchFound = true;
                        isAnyPressBeganRecorded = true;
                    }
                    else if (pressedUIItem != null && touch.fingerId == firstPressedUIItemTouch.fingerId)
                    {
                        secondaryTouch = new tk2dUITouch(touch);
                        isSecondaryTouchFound = true;
                    }
                }

                checkForHovers = false;

            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    primaryTouch = new tk2dUITouch(TouchPhase.Began, tk2dUITouch.MOUSE_POINTER_FINGER_ID, Input.mousePosition, Vector2.zero, 0);
                    isPrimaryTouchFound = true;
                    isAnyPressBeganRecorded = true;
                }
                else if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
                {
                    Vector2 deltaPosition = Vector2.zero;
                    TouchPhase mousePhase = TouchPhase.Moved;
                    if (pressedUIItem != null)
                    {
                        deltaPosition = firstPressedUIItemTouch.position - new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        mousePhase = TouchPhase.Ended;
                    }
                    else if (deltaPosition == Vector2.zero)
                    {
                        mousePhase = TouchPhase.Stationary;
                    }
                    secondaryTouch = new tk2dUITouch(mousePhase, tk2dUITouch.MOUSE_POINTER_FINGER_ID, Input.mousePosition, deltaPosition, tk2dUITime.deltaTime);
                    isSecondaryTouchFound = true;
                }
            }
        }

        if (isPrimaryTouchFound)
        {
            resultTouch = primaryTouch;
        }
        else if (isSecondaryTouchFound)
        {
            resultTouch = secondaryTouch;
        }

        if (isPrimaryTouchFound || isSecondaryTouchFound) //focus touch found
        {
            hitUIItem = RaycastForUIItem(resultTouch.position);

            if (resultTouch.phase == TouchPhase.Began)
            {
                if (pressedUIItem != null)
                {
                    pressedUIItem.CurrentOverUIItem(hitUIItem);
                    if (pressedUIItem != hitUIItem)
                    {
                        pressedUIItem.Release();
                        pressedUIItem = null;
                    }
                    else
                    {
                        firstPressedUIItemTouch = resultTouch; //just incase touch changed
                    }
                }

                if (hitUIItem != null)
                {
                    hitUIItem.Press(resultTouch);
                }
                pressedUIItem = hitUIItem;
                firstPressedUIItemTouch = resultTouch;
            }
            else if (resultTouch.phase == TouchPhase.Ended)
            {
                if (pressedUIItem != null)
                {
                    pressedUIItem.CurrentOverUIItem(hitUIItem);
                    pressedUIItem.UpdateTouch(resultTouch);
                    pressedUIItem.Release();
                    pressedUIItem = null;
                }
            }
            else
            {
                if (pressedUIItem != null)
                {
                    pressedUIItem.CurrentOverUIItem(hitUIItem);
                    pressedUIItem.UpdateTouch(resultTouch);
                }
            }
        }
        else //no touches found
        {
            if (pressedUIItem != null)
            {
                pressedUIItem.CurrentOverUIItem(null);
                pressedUIItem.Release();
                pressedUIItem = null;
            }
        }

        //only if hover events are enabled and only if no touch events have ever been recorded
        if (checkForHovers)
        {
            if (inputEnabled) //if input enabled and mouse button is not currently down
            {
                if (!isPrimaryTouchFound && !isSecondaryTouchFound && hitUIItem == null && !Input.GetMouseButton(0)) //if raycast for a button has not yet been done
                {
                    hitUIItem = RaycastForUIItem( Input.mousePosition );
                }
                else if (Input.GetMouseButton(0)) //if mouse button is down clear it
                {
                    hitUIItem = null;
                }
            }

            if (hitUIItem != null)
            {
                if (hitUIItem.isHoverEnabled)
                {
                    bool wasPrevOverFound = hitUIItem.HoverOver(overUIItem);

                    if (!wasPrevOverFound && overUIItem != null)
                    {
                        overUIItem.HoverOut(hitUIItem);
                    }
                    overUIItem = hitUIItem;
                }
                else
                {
                    if (overUIItem != null)
                    {
                        overUIItem.HoverOut(null);
                    }
                }
            }
            else
            {
                if (overUIItem != null)
                {
                    overUIItem.HoverOut(null);
                }
            }
        }

        if (isAnyPressBeganRecorded)
        {
            if (OnAnyPress != null) { OnAnyPress(); }
        }
    }

    //checks for inputs (multi-touch)
    private void CheckMultiTouchInputs()
    {
        bool isAnyPressBeganRecorded = false;
        int prevFingerID = -1;
        bool wasPrevTouchFound = false;
        bool isNewlyPressed = false;

        touchCounter = 0;
        if (inputEnabled)
        {
            if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touchCounter < MAX_MULTI_TOUCH_COUNT)
                    {
                        allTouches[touchCounter] = new tk2dUITouch(touch);
                        touchCounter++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    allTouches[touchCounter] = new tk2dUITouch(TouchPhase.Began, tk2dUITouch.MOUSE_POINTER_FINGER_ID, Input.mousePosition, Vector2.zero, 0);
                    mouseDownFirstPos = Input.mousePosition;
                    touchCounter++;
                }
                else if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
                {
                    Vector2 deltaPosition = mouseDownFirstPos - new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    TouchPhase mousePhase = TouchPhase.Moved;

                    if (Input.GetMouseButtonUp(0))
                    {
                        mousePhase = TouchPhase.Ended;
                    }
                    else if (deltaPosition == Vector2.zero)
                    {
                        mousePhase = TouchPhase.Stationary;
                    }
                    allTouches[touchCounter] = new tk2dUITouch(mousePhase, tk2dUITouch.MOUSE_POINTER_FINGER_ID, Input.mousePosition, deltaPosition, tk2dUITime.deltaTime);
                    touchCounter++;
                }
            }
        }

        for (int p = 0; p < touchCounter; p++)
        {
            pressedUIItems[p] = RaycastForUIItem(allTouches[p].position);
        }
       
        //deals with all the previous presses
        for (int f=0; f<prevPressedUIItemList.Count; f++)
        {
            prevPressedItem = prevPressedUIItemList[f];
            if (prevPressedItem != null)
            {
                prevFingerID = prevPressedItem.Touch.fingerId;

                wasPrevTouchFound=false;
                for (int t = 0; t < touchCounter; t++)
                {
                    currTouch = allTouches[t];
                    if (currTouch.fingerId == prevFingerID)
                    {
                        wasPrevTouchFound=true;
                        currPressedItem = pressedUIItems[t];
                        if (currTouch.phase == TouchPhase.Began)
                        {
                            prevPressedItem.CurrentOverUIItem(currPressedItem);

                            if (prevPressedItem != currPressedItem)
                            {
                                prevPressedItem.Release();
                                prevPressedUIItemList.RemoveAt(f);
                                f--;
                            }
                        }
                        else if (currTouch.phase == TouchPhase.Ended)
                        {
                            prevPressedItem.CurrentOverUIItem(currPressedItem);
                            prevPressedItem.UpdateTouch(currTouch);
                            prevPressedItem.Release();
                            prevPressedUIItemList.RemoveAt(f);
                            f--;
                        }
                        else
                        {
                            prevPressedItem.CurrentOverUIItem(currPressedItem);
                            prevPressedItem.UpdateTouch(currTouch);
                        }
                        break;
                    }
                }

                if(!wasPrevTouchFound)
                {
                    prevPressedItem.CurrentOverUIItem(null);
                    prevPressedItem.Release();
                    prevPressedUIItemList.RemoveAt(f);
                    f--;
                }
            }
        }

        for (int f = 0; f < touchCounter; f++)
        {
            currPressedItem = pressedUIItems[f];
            currTouch = allTouches[f];
            if (currTouch.phase == TouchPhase.Began)
            {
                if (currPressedItem != null)
                {
                    isNewlyPressed = currPressedItem.Press(currTouch);
                    if (isNewlyPressed)
                    {
                        prevPressedUIItemList.Add(currPressedItem);
                    }
                }
                isAnyPressBeganRecorded = true;
            }
        }

        if (isAnyPressBeganRecorded)
        {
            if (OnAnyPress != null) { OnAnyPress(); }
        }
    }

    tk2dUIItem RaycastForUIItem( Vector2 screenPos ) {
        foreach (tk2dUICamera currCamera in sortedCameras) {
            ray = currCamera.HostCamera.ScreenPointToRay( screenPos );
            if (Physics.Raycast( ray, out hit, currCamera.HostCamera.farClipPlane, currCamera.FilteredMask )) {
                return hit.collider.GetComponent<tk2dUIItem>();
            }
        }
        return null;
    }

    public void OverrideClearAllChildrenPresses(tk2dUIItem item)
    {
        if (useMultiTouch)
        {
            tk2dUIItem tempUIItem;
            for (int n = 0; n < pressedUIItems.Length; n++)
            {
                tempUIItem = pressedUIItems[n];
                if (tempUIItem!=null)
                {
                    if (item.CheckIsUIItemChildOfMe(tempUIItem))
                    {
                        tempUIItem.CurrentOverUIItem(item);
                    }
                }
            }
        }
        else
        {
            if (pressedUIItem != null)
            {
                if (item.CheckIsUIItemChildOfMe(pressedUIItem))
                {
                    pressedUIItem.CurrentOverUIItem(item);
                }
            }
        }
    }
}

