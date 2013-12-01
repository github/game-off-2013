using UnityEngine;
using System.Collections;

/// <summary>
/// Dropdown Item in the list of a DropDownMenu
/// </summary>
[AddComponentMenu("2D Toolkit/UI/tk2dUIDropDownItem")]
public class tk2dUIDropDownItem : tk2dUIBaseItemControl
{
    /// <summary>
    /// Text Label for dropdown Item
    /// </summary>
    public tk2dTextMesh label;

    /// <summary>
    /// Visible height of this ui item, used for vertical spacing
    /// </summary>
    public float height;

    /// <summary>
    /// Button used for hovers
    /// </summary>
    public tk2dUIUpDownHoverButton upDownHoverBtn;

    private int index;

    /// <summary>
    /// Which item in the list is this (0-index)
    /// </summary>
    public int Index
    {
        get { return index; }
        set { index = value; }
    }

    /// <summary>
    /// Event on this item being selected
    /// </summary>
    public event System.Action<tk2dUIDropDownItem> OnItemSelected;

    /// <summary>
    /// Auto sets the label text (does commit)
    /// </summary>
    public string LabelText
    {
        get { return label.text; }
        set 
        { 
            label.text = value;
            label.Commit();
        }
    }

    void OnEnable()
    {
        if (uiItem)
        {
            uiItem.OnClick += ItemSelected;
        }
    }

    void OnDisable()
    {
        if (uiItem)
        {
            uiItem.OnClick -= ItemSelected;
        }
    }

    //if item is selected
    private void ItemSelected()
    {
        if (OnItemSelected != null) { OnItemSelected(this); }
    }
}
