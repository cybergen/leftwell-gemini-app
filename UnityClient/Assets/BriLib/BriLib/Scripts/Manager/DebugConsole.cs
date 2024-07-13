using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BriLib
{
  public class DebugConsole : Singleton<DebugConsole>
  {
    public InputField Input;
    public GameObject HideButton;
    public GameObject ShowButton;
    public RectTransform ScrollRect;
    public Text ConsoleText;
    public int TargetScrollSize = 600;
    public int MaxLineCount = 75;

    private Dictionary<string, Func<List<string>, string>> _commandMap
      = new Dictionary<string, Func<List<string>, string>>();
    private Dictionary<string, string> _helpInfoMap = new Dictionary<string, string>();

    public void OnHideClick()
    {
      HideButton.SetActive(false);
      ShowButton.SetActive(true);
      ScrollRect.sizeDelta = new Vector2(ScrollRect.sizeDelta.x, 0);
    }

    public void OnShowClick()
    {
      HideButton.SetActive(true);
      ShowButton.SetActive(false);
      ScrollRect.sizeDelta = new Vector2(ScrollRect.sizeDelta.x, TargetScrollSize);
    }

    public void OnTextEnter()
    {
      var txt = Input.text;
      Input.text = string.Empty;
      var list = txt.Split(' ').ToList();
      if (list.Count <= 0)
      {
        LogManager.Error("Attempt to enter empty text");
        return;
      }
      var cmd = list[0];
      list.RemoveAt(0);
      if (!_commandMap.ContainsKey(cmd))
      {
        LogManager.Error("Invalid command: " + cmd);
        return;
      }
      AppendMsg(_commandMap[cmd](list));
    }

    public void AddCommand(string cmd, string helpInfo, Func<List<string>, string> func)
    {
      if (string.IsNullOrEmpty(cmd))
      {
        LogManager.Error("Attempted to add empty command string to debug console");
        return;
      }

      if (func == null)
      {
        LogManager.Error("Attempted to add null function to debug console");
        return;
      }

      _commandMap.Add(cmd, func);
      _helpInfoMap.Add(cmd, helpInfo);
    }

    public override void Begin()
    {
      base.Begin();
      OnHideClick();
      ConsoleText.text = string.Empty;
      Application.logMessageReceived += OnLogCallback;
      AddCommand("help", "Print a list of commands", HelpCmd);
    }

    public override void End()
    {
      base.End();
      Application.logMessageReceived -= OnLogCallback;
    }

    private void AppendMsg(string line)
    {
      var lines = line.Split('\n').ToList();
      var oldText = ConsoleText.text.Split('\n').ToList();
      while((oldText.Count + lines.Count >= MaxLineCount))
      {
        oldText.RemoveAt(0);
      }
      oldText.AddRange(lines);
      ConsoleText.text = string.Join("\n", oldText);
    }

    private string HelpCmd(List<string> arg)
    {
      var s = string.Empty;
      for (var enumerator = _commandMap.Keys.GetEnumerator(); enumerator.MoveNext();)
      {
        s += enumerator.Current + ": " + _helpInfoMap[enumerator.Current] + "\n";
      }
      return s;
    }

    private void OnLogCallback(string log, string stackTrace, LogType type)
    {
      AppendMsg(type.ToString() + ": " + log);
    }
  }
}