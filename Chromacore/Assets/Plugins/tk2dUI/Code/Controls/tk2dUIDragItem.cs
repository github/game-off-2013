using UnityEngine;
using System.Collections;

/// <summary>
/// UIItem you wish be able to drag on press
/// </summary>
[AddComponentMenu("2D Toolkit/UI/tk2dUIDragItem")]
public class tk2dUIDragItem : tk2dUIBaseItemControl
{
    /// <summary>
    /// Active tk2dUIManager in scene
    /// </summary>
    public tk2dUIManager uiManager = null;

    private Vector3 offset = Vector3.zero; //offset on touch/click
    private bool isBtnActive = false; //if currently active

    void OnEnable()
    {
        if (uiItem)
        {
            uiItem.OnDown += ButtonDown;
            uiItem.OnRelease += ButtonRelease;
        }
    }

    void OnDisable()
    {
        if (uiItem)
        {
            uiItem.OnDown -= ButtonDown;
            uiItem.OnRelease -= ButtonRelease;
        }

        if (isBtnActive)
        {
            if (tk2dUIManager.Instance__NoCreate != null)
            {
                tk2dUIManager.Instance.OnInputUpdate -= UpdateBtnPosition;
            }
            isBtnActive = false;
        }
    }


    private void UpdateBtnPosition()
    {
        transform.position = CalculateNewPos();
    }

    private Vector3 CalculateNewPos()
    {
        Vector2 pos = uiItem.Touch.position;

        Camera viewingCamera = tk2dUIManager.Instance.GetUICameraForControl( gameObject );
        Vector3 worldPos = viewingCamera.ScreenToWorldPoint(new Vector3(pos.x, pos.y, transform.position.z - viewingCamera.transform.position.z));
        worldPos.z = transform.position.z;
        worldPos += offset;
        return worldPos;
    }

    /// <summary>
    /// Set button to be down (drag can begin)
    /// </summary>
    public void ButtonDown()
    {
        if (!isBtnActive)
        {
            tk2dUIManager.Instance.OnInputUpdate += UpdateBtnPosition;
        }
        isBtnActive = true;
        offset = Vector3.zero;
        Vector3 newWorldPos = CalculateNewPos();
        offset = transform.position - newWorldPos;
    }

    /// <summary>
    /// Set button release (so drag will stop)
    /// </summary>
    public void ButtonRelease()
    {
        if (isBtnActive)
        {
            tk2dUIManager.Instance.OnInputUpdate -= UpdateBtnPosition;
        }
        isBtnActive = false;
    }

}
