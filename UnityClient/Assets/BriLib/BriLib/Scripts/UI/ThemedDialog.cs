using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BriLib.UI
{
  /// <summary>
  /// A dialog that, when shown with DialogButtonData list, will
  /// automatically add a set of styled buttons to the layout and, on
  /// button click, delete all the created buttons and hide itself.
  /// </summary>
  public class ThemedDialog : MonoBehaviour
  {
    public Text Title;
    public Text DialogBody;
    public List<Button> ButtonsFromLeftToRight;

    private List<ButtonData> _buttonData;

    [SerializeField] private Button _backgroundButton;


    /// <summary>
    /// Prevent the Dialog being destroyed when a new scene is loaded
    /// </summary>
    private void Awake()
    {
      DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Show a dialog with a dynamic set of buttons spawned equal in count to the list
    /// of DialogButtonData passed in
    /// </summary>
    /// <param name="bodyText"></param>
    /// <param name="buttons"></param>
    public void Show(string titleText, string bodyText, List<ButtonData> buttons, Action onBackgroundClicked = null)
    {
      _buttonData = buttons;

      if (Title != null) Title.text = titleText;
      if (DialogBody != null) DialogBody.text = bodyText;

      if (buttons.Count != ButtonsFromLeftToRight.Count)
      {
        LogManager.Error("Attempted to supply mismatched amount of button data " + buttons.Count
          + " to button count " + ButtonsFromLeftToRight.Count);
        return;
      }

      for (int i = 0; i < buttons.Count; i++)
      {
        var button = ButtonsFromLeftToRight[i];
        var index = i;
        button.onClick.AddListener(() => OnClickButton(index));
        var buttonText = button.gameObject.GetComponentInChildren<Text>();
        if (buttonText != null) buttonText.text = buttons[i].ButtonText;
      }

      if (_backgroundButton != null && onBackgroundClicked != null)
      {
        _backgroundButton.onClick.AddListener(() => onBackgroundClicked());
      }

      InputManager.Instance.AddKeyListener(KeyCode.Escape, OnBack);

      gameObject.SetActive(true);
    }

    /// <summary>
    /// Break down the buttons we've constructed and hide the dialog
    /// </summary>
    public void Hide()
    {
      // Loop through all buttons and clear listeners
      for (int i = 0; i < ButtonsFromLeftToRight.Count; i++)
      {
        ButtonsFromLeftToRight[i].onClick.RemoveAllListeners();
      }

      if (_backgroundButton != null) _backgroundButton.onClick.RemoveAllListeners();
      _buttonData.Clear();
      _buttonData = null;

      InputManager.Instance.RemoveKeyListener(KeyCode.Escape, OnBack);

      gameObject.SetActive(false);
    }

    public void OnClickButton(int index)
    {
      // Defensive check here in lieu of supporting multiple instances of a given template
      // Revisit later if we want to implement some kind of dialog stacking functionality
      if (_buttonData != null && index <= _buttonData.Count)
      {
        var action = _buttonData[index].OnClick;
        if (_buttonData[index].HideOnClick) Hide();
        action.Execute();
      }
    }

    private bool OnBack()
    {
      if (_backgroundButton != null) _backgroundButton.onClick.Invoke();
      else OnClickButton(0);
      return true;
    }
  }

  /// <summary>
  /// Descriptor of a given button for the dialog, including content and style
  /// </summary>
  public struct ButtonData
  {
    public string ButtonText;
    public Action OnClick;
    public bool HideOnClick;

    /// <summary>
    /// Craft a button using target text, action, style, and close on click behavior
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick"></param>
    /// <param name="style"></param>
    public ButtonData(string text, Action onClick, bool hideOnClick)
    {
      ButtonText = text;
      OnClick = onClick;
      HideOnClick = hideOnClick;
    }

    /// <summary>
    /// Craft a button using target text, action, style, and close on click behavior
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick"></param>
    /// <param name="style"></param>
    public ButtonData(string text, Action onClick)
    {
      ButtonText = text;
      OnClick = onClick;
      HideOnClick = true;
    }
  }
}