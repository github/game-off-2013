#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8)
    #define TOUCH_SCREEN_KEYBOARD
#endif

using UnityEngine;
using System.Collections;

/// <summary>
/// TextInput control
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/UI/tk2dUITextInput")]
public class tk2dUITextInput : MonoBehaviour
{
    /// <summary>
    /// UItem that will make cause TextInput to become selected on click
    /// </summary>
    public tk2dUIItem selectionBtn;

    /// <summary>
    /// TextMesh while text input will be displayed
    /// </summary>
    public tk2dTextMesh inputLabel;

    /// <summary>
    /// TextMesh that will be displayed if nothing in inputLabel and is not selected
    /// </summary>
    public tk2dTextMesh emptyDisplayLabel;

    /// <summary>
    /// State to be active if text input is not selected
    /// </summary>
    public GameObject unSelectedStateGO;

    /// <summary>
    /// Stated to be active if text input is selected
    /// </summary>
    public GameObject selectedStateGO;

    /// <summary>
    /// Text cursor to be displayed at next of text input on selection
    /// </summary>
    public GameObject cursor;

    /// <summary>
    /// How long the field is (visible)
    /// </summary>
    public float fieldLength = 1;

    /// <summary>
    /// Maximum number of characters allowed for input
    /// </summary>
    public int maxCharacterLength = 30;

    /// <summary>
    /// Text to be displayed when no text is entered and text input is not selected
    /// </summary>
    public string emptyDisplayText;

    /// <summary>
    /// If set to true (is a password field), then all characters will be replaced with password char
    /// </summary>
    public bool isPasswordField = false;

    /// <summary>
    /// Each character in the password field is replaced with the first character of this string
    /// Default: * if string is empty.
    /// </summary>
    public string passwordChar = "*";

	[SerializeField]
	[HideInInspector]
	private tk2dUILayout layoutItem = null;

	public tk2dUILayout LayoutItem {
		get { return layoutItem; }
		set {
			if (layoutItem != value) {
				if (layoutItem != null) {
					layoutItem.OnReshape -= LayoutReshaped;
				}
				layoutItem = value;
				if (layoutItem != null) {
					layoutItem.OnReshape += LayoutReshaped;
				}
			}
		}
	}

    private bool isSelected = false;

    private bool wasStartedCalled = false;
    private bool wasOnAnyPressEventAttached = false;

#if TOUCH_SCREEN_KEYBOARD
    private TouchScreenKeyboard keyboard = null;
#endif

    private bool listenForKeyboardText = false;

    private bool isDisplayTextShown =false;

    public System.Action<tk2dUITextInput> OnTextChange;

    public string SendMessageOnTextChangeMethodName = "";

    public GameObject SendMessageTarget
    {
        get
        {
            if (selectionBtn != null)
            {
                return selectionBtn.sendMessageTarget;
            }
            else return null;
        }
        set
        {
            if (selectionBtn != null && selectionBtn.sendMessageTarget != value)
            {
                selectionBtn.sendMessageTarget = value;
            
                #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(selectionBtn);
                #endif
            }
        }
    }

    public bool IsFocus
    {
        get
        {
            return isSelected;
        }
    }

    private string text = "";

    /// <summary>
    /// Update the input text
    /// </summary>
    public string Text
    {
        get { return text; }
        set
        {
            if (text != value)
            {
                text = value;
                if (text.Length > maxCharacterLength)
                {
                    text = text.Substring(0, maxCharacterLength);
                }
                FormatTextForDisplay(text);
                if (isSelected)
                {
                    SetCursorPosition();
                }
            }
        }
    }

    void Awake()
    {
        SetState();
        ShowDisplayText();
    }

    void Start()
    {
        wasStartedCalled = true;
        if (tk2dUIManager.Instance__NoCreate != null)
        {
            tk2dUIManager.Instance.OnAnyPress += AnyPress;
        }
        wasOnAnyPressEventAttached = true;
    }

    void OnEnable()
    {
        if (wasStartedCalled && !wasOnAnyPressEventAttached)
        {
            if (tk2dUIManager.Instance__NoCreate != null)
            {
                tk2dUIManager.Instance.OnAnyPress += AnyPress;
            }
        }

		if (layoutItem != null)
		{
			layoutItem.OnReshape += LayoutReshaped;
		}

        selectionBtn.OnClick += InputSelected;
    }

    void OnDisable()
    {
        if (tk2dUIManager.Instance__NoCreate != null)
        {
            tk2dUIManager.Instance.OnAnyPress -= AnyPress;
            if (listenForKeyboardText)
            {
                tk2dUIManager.Instance.OnInputUpdate -= ListenForKeyboardTextUpdate;
            }
        }
        wasOnAnyPressEventAttached = false;

        selectionBtn.OnClick -= InputSelected;


        listenForKeyboardText = false;

		if (layoutItem != null)
		{
			layoutItem.OnReshape -= LayoutReshaped;
		}
    }

    public void SetFocus()
    {
        if (!IsFocus)
        {
            InputSelected();
        }
    }

    private void FormatTextForDisplay(string modifiedText)
    {
        if (isPasswordField)
        {
            int charLength = modifiedText.Length;
            char passwordReplaceChar = ( passwordChar.Length > 0 ) ? passwordChar[0] : '*';
            modifiedText = "";
            modifiedText=modifiedText.PadRight(charLength, passwordReplaceChar);
        }

        inputLabel.text = modifiedText;
        inputLabel.Commit();

        while (inputLabel.renderer.bounds.extents.x * 2 > fieldLength)
        {
            modifiedText=modifiedText.Substring(1, modifiedText.Length - 1);
            inputLabel.text = modifiedText;
            inputLabel.Commit();
        }

        if (modifiedText.Length==0 && !listenForKeyboardText)
        {
            ShowDisplayText();
        }
        else
        {
            HideDisplayText();
        }
    }

    private void ListenForKeyboardTextUpdate()
    {
        bool change = false;
        string newText = text;
        //http://docs.unity3d.com/Documentation/ScriptReference/Input-inputString.html

        string inputStr = Input.inputString;
        char c;
        for (int i=0; i<inputStr.Length; i++)
        {
            c = inputStr[i];
            if (c == "\b"[0])
            {
                if (text.Length != 0)
                {
                    newText = text.Substring(0, text.Length - 1);
                    change = true;
                }
            }
            else if (c == "\n"[0] || c == "\r"[0])
            {
                
            }
            else if ((int)c!=9 && (int)c!=27) //deal with a Mac only Unity bug where it returns a char for escape and tab
            {
                newText += c;
                change = true;
            }
        }

        if (change)
        {
            Text = newText;
            if (OnTextChange != null) { OnTextChange(this); }

            if (SendMessageTarget != null && SendMessageOnTextChangeMethodName.Length > 0)
            {
                SendMessageTarget.SendMessage( SendMessageOnTextChangeMethodName, this, SendMessageOptions.RequireReceiver );
            }
        }
    }


    private void InputSelected()
    {
        if (text.Length == 0)
        {
            HideDisplayText();
        }
        isSelected = true;
        if (!listenForKeyboardText)
        {
            tk2dUIManager.Instance.OnInputUpdate += ListenForKeyboardTextUpdate;
        }
        listenForKeyboardText = true;
        SetState();
        SetCursorPosition();


#if TOUCH_SCREEN_KEYBOARD
        if (Application.platform != RuntimePlatform.WindowsEditor &&
            Application.platform != RuntimePlatform.OSXEditor) {
#if UNITY_ANDROID //due to a delete key bug in Unity Android
            TouchScreenKeyboard.hideInput = false;
#else
            TouchScreenKeyboard.hideInput = true;
#endif
            keyboard = TouchScreenKeyboard.Open(text, TouchScreenKeyboardType.Default, false, false, false, false);
            StartCoroutine(TouchScreenKeyboardLoop());
        }
#endif
    }

#if TOUCH_SCREEN_KEYBOARD
    private IEnumerator TouchScreenKeyboardLoop()
    {
        while (keyboard != null && !keyboard.done && keyboard.active)
        {
            Text = keyboard.text;
            yield return null;
        }

        if (keyboard != null)
        {
            Text = keyboard.text;
        }

        if (isSelected)
        {
            InputDeselected();
        }
    }
#endif

    private void InputDeselected()
    {
        if (text.Length == 0)
        {
            ShowDisplayText();
        }
        isSelected = false;
        if (listenForKeyboardText)
        {
            tk2dUIManager.Instance.OnInputUpdate -= ListenForKeyboardTextUpdate;
        }
        listenForKeyboardText = false;
        SetState();
#if TOUCH_SCREEN_KEYBOARD
        if (keyboard!=null && !keyboard.done)
        {
            keyboard.active = false;
        }
        keyboard = null;
#endif
    }

    private void AnyPress()
    {
        if (isSelected && tk2dUIManager.Instance.PressedUIItem != selectionBtn)
        {
            InputDeselected();
        }
    }

    private void SetState()
    {
        tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(unSelectedStateGO, !isSelected);
        tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(selectedStateGO, isSelected);
        tk2dUIBaseItemControl.ChangeGameObjectActiveState(cursor, isSelected);
    }

    private void SetCursorPosition()
    {
        float multiplier = 1;
        float cursorOffset = 0.002f;
        if (inputLabel.anchor == TextAnchor.MiddleLeft || inputLabel.anchor == TextAnchor.LowerLeft || inputLabel.anchor == TextAnchor.UpperLeft)
        {
            multiplier = 2;
        }
        else if (inputLabel.anchor == TextAnchor.MiddleRight || inputLabel.anchor == TextAnchor.LowerRight || inputLabel.anchor == TextAnchor.UpperRight)
        {
            multiplier = -2;
            cursorOffset = 0.012f;
        }

        if (text.EndsWith(" "))
        {
            tk2dFontChar chr;
            if (inputLabel.font.useDictionary)
            {
                chr = inputLabel.font.charDict[' '];
            }
            else
            {
                chr = inputLabel.font.chars[' '];
            }

            cursorOffset += chr.advance * inputLabel.scale.x/2;
        }
        cursor.transform.localPosition = new Vector3(inputLabel.transform.localPosition.x + (inputLabel.renderer.bounds.extents.x + cursorOffset) * multiplier, cursor.transform.localPosition.y, cursor.transform.localPosition.z);
    }

    private void ShowDisplayText()
    {
        if (!isDisplayTextShown)
        {
            isDisplayTextShown = true;
            if (emptyDisplayLabel != null)
            {
                emptyDisplayLabel.text = emptyDisplayText;
                emptyDisplayLabel.Commit();
                tk2dUIBaseItemControl.ChangeGameObjectActiveState(emptyDisplayLabel.gameObject, true);
            }
            tk2dUIBaseItemControl.ChangeGameObjectActiveState(inputLabel.gameObject, false);
        }
    }

    private void HideDisplayText()
    {
        if (isDisplayTextShown)
        {
            isDisplayTextShown = false;
            tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(emptyDisplayLabel.gameObject, false);
            tk2dUIBaseItemControl.ChangeGameObjectActiveState(inputLabel.gameObject, true);
        }
    }

	private void LayoutReshaped(Vector3 dMin, Vector3 dMax)
	{
		fieldLength += (dMax.x - dMin.x);
        // No way to trigger re-format yet
        string tmpText = this.text;
        text = "";
        Text = tmpText;
	}
}
