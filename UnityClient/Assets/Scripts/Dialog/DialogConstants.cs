using BriLib;
using System;
using System.Collections.Generic;

public class DialogConstants
{
  public const string POSITION_PORTAL = "I'll get these items ready. In the meantime, give me an open area where I can form the portal";
  public const string PORTAL_PLACED = "Perfect! I'll start working the big magic to get this spun up!";
  public const string ITEMS_READY = "I put a little bit of magic on each of your items and transformed them! Go check out each one and activate it!";
  public const string PORTAL_NOT_READY = "Just a moment, I'm still trying to get this portal opened...";
  public const string PORTAL_READY = "The big portal is ready!";
  public static List<string> ITEM_CAPTURE_RESPONSES = new List<string>
  {
    "Uh... Interesting. Tell me about this.",
    "Wow. What do you have to say about this?",
    "Anything I should know about this?",
    "Why did you pick this?",
    "That looks powerful. Care to explain?"
  };

  public static string GetRandomItemCaptureDialog()
  {    
    return MathHelpers.SelectFromRange(ITEM_CAPTURE_RESPONSES, new Random());
  }
}