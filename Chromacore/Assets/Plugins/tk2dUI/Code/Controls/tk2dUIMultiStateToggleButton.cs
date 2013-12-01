using UnityEngine;
using System.Collections;

/// <summary>
/// ToggleButton which can have multi-different states which it will toggle between
/// </summary>
[AddComponentMenu("2D Toolkit/UI/tk2dUIMultiStateToggleButton")]
public class tk2dUIMultiStateToggleButton : tk2dUIBaseItemControl
{
    /// <summary>
    /// All states which toggle between. They will be actived/deactived as cycle through list.
    /// These do not need to be set to anything, you can simply set the array to required length.
    /// </summary>
    public GameObject[] states; //these don't have to be anything, you can simply set the array to required length

    /// <summary>
    /// If false toggles on click, if true toggles on down
    /// </summary>
    public bool activateOnPress = false;

    /// <summary>
    /// Event on change of state
    /// </summary>
    public event System.Action<tk2dUIMultiStateToggleButton> OnStateToggle;
    private int index = 0;

    public string SendMessageOnStateToggleMethodName = "";

    /// <summary>
    /// Currently selected index of active state
    /// </summary>
    public int Index
    {
        get { return index; }
        set
        {
            if (value >= states.Length)
            {
                value = states.Length;
            }
            if (value < 0)
            {
                value = 0;
            }
            if (index != value)
            {
                index = value;
                SetState();
                if (OnStateToggle != null) { OnStateToggle(this); }
                base.DoSendMessage( SendMessageOnStateToggleMethodName, this );
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
        if (Index + 1 >= states.Length)
        {
            Index = 0;
        }
        else
        {
            Index++;
        }
    }

    private void SetState()
    {
        GameObject go;

        for (int n = 0; n < states.Length; n++)
        {
            go = states[n];
            if (go != null)
            {
                if (n != index)
                {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
                    if (states[n].active)
                    {
                        states[n].SetActiveRecursively(false);
                    }
#else
                    if (states[n].activeInHierarchy)
                    {
                        states[n].SetActive(false);
                    }
#endif

                }
                else
                {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
                    if (!states[n].active)
                    {
                        states[n].SetActiveRecursively(true);
                    }
#else
                    if (!states[n].activeInHierarchy)
                    {
                        states[n].SetActive(true);
                    }
#endif
                }
            }
        }
    }

}
