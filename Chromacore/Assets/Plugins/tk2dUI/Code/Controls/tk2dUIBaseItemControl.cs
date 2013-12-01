using UnityEngine;
using System.Collections;

/// <summary>
/// Button base class. Button controls can extend this to get some base level structure and inspector editor support
/// </summary>
[AddComponentMenu("2D Toolkit/UI/tk2dUIBaseItemControl")]
public abstract class tk2dUIBaseItemControl : MonoBehaviour
{
    /// <summary>
    /// Button(uiItem) for this control 
    /// </summary>
    public tk2dUIItem uiItem;

    public GameObject SendMessageTarget {
        get {
            if (uiItem != null) {
                return uiItem.sendMessageTarget;
            }
            else return null;
        }
        set {
            if (uiItem != null) {
                uiItem.sendMessageTarget = value;
            }
        }
    }

    /// <summary>
    /// Used for SetActive so easily works between Unity 3.x and Unity 4.x
    /// </summary>
    public static void ChangeGameObjectActiveState(GameObject go, bool isActive)
    {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
        go.SetActiveRecursively(isActive);
#else
        go.SetActive(isActive);
#endif
    }

    /// <summary>
    /// Changes active state, but first checks to make sure it isn't null
    /// </summary>
    public static void ChangeGameObjectActiveStateWithNullCheck(GameObject go, bool isActive)
    {
        if (go != null)
        {
            ChangeGameObjectActiveState(go, isActive);
        }
    }

    protected void DoSendMessage( string methodName, object parameter )
    {
        if (SendMessageTarget != null && methodName.Length > 0)
        {
            SendMessageTarget.SendMessage( methodName, parameter, SendMessageOptions.RequireReceiver );
        }
    }
}
