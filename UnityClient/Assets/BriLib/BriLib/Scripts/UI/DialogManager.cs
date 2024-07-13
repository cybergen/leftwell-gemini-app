using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace BriLib.UI
{
  /// <summary>
  /// Limited manager for maintaining a themed dialog that is reusable. WIP - should be extended
  /// as we add new dialog types and more configurable dialogs
  /// </summary>
  public class DialogManager : Singleton<DialogManager>
  {
    private Dictionary<GameObject, ThemedDialog> _dialogCache = new Dictionary<GameObject, ThemedDialog>();

    /// <summary>
    /// Show a dialog with a variable list of button params
    /// </summary>
    /// <param name="template"></param>
    /// <param name="titleText"></param>
    /// <param name="bodyText"></param>
    /// <param name="buttonData"></param>
    public void ShowDialog(GameObject template, string titleText, string bodyText, Action onBackgroundClicked = null, params ButtonData[] buttonData)
    {
      ShowDialog(template, titleText, bodyText, buttonData.ToList(), onBackgroundClicked);
    }

    /// <summary>
    /// Show a dialog with a list of buttons
    /// </summary>
    /// <param name="template"></param>
    /// <param name="titleText"></param>
    /// <param name="bodyText"></param>
    /// <param name="buttonData"></param>
    private void ShowDialog(GameObject template, string titleText, string bodyText, List<ButtonData> buttonData, Action onBackgroundClicked = null)
    {
      if (!_dialogCache.ContainsKey(template))
      {
        _dialogCache.Add(template, Instantiate(template).GetComponent<ThemedDialog>());
      }
      _dialogCache[template].Show(titleText, bodyText, buttonData, onBackgroundClicked);
    }

    /// <summary>
    /// Show a dialog with a list of buttons
    /// </summary>
    /// <param name="template"></param>
    public void HideDialog(GameObject template)
    {
      if (_dialogCache.ContainsKey(template))
      {
        _dialogCache[template].Hide();
      }
      else
      {
        LogManager.Error("DialogManager.HideDialog(): Template is null or does not exist.");
      }
    }
  }
}