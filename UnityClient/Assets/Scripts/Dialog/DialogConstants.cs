using System;
using System.Collections.Generic;
using BriLib;

public class DialogConstants
{
  public const string POSITION_PORTAL 
    = "I'll get these items ready. In the meantime, give me an open area where I can form the portal";
  public const string PORTAL_PLACED = "Perfect! I'll start working the big magic to get this spun up!";
  public const string ITEMS_READY 
    = "I used some magic on each of your items to reveal their true power! Go check out each one and activate it!";
  public const string PORTAL_NOT_READY = "Just a moment, I'm still trying to get this portal opened...";
  public const string PORTAL_READY = "The portal is ready!";

  public static List<string> ITEM_CAPTURE_RESPONSES = new List<string>
  {
    "Uh... Interesting. Tell me more.",
    "Wow. What do you have to say for yourself?",
    "Anything I should know...?",
    "Why this choice?",
    "Looks powerful. Care to explain?",
    "Haven't seen one of these in ages!",
    "Are you sure? Well, it's your funeral...",
    "Tell me more...",
    "Wise choice!",
    "Weird, but maybe you have some good reasoning?",
    "That's a first. Any reason why?",
    "I'm sensing a palpable aura. Why's that?",
    "Ha! Care to elaborate?",
    "That's... almost unbelievable. Want to explain?",
    "What are you showing me?",
    "You sure this is the right call to bring?"
  };

  public static List<string> ITEMS_OF_POWER = new List<string>
  {
    "A powerful weapon",
    "A supportive companion",
    "Fashionable armor",
    "A favorite tome",
    "An indispensable potion",
    "A beloved toy",
    "Speedy shoes",
    "Currency for where we're going",
    "A meaningful talisman",
    "A beautiful piece of art",
    "A treasured souvenir",
    "A faithful ally",
    "A stalwart shield",
    "An ancient scroll",
    "A robust tool",
    "A trusty steed",
    "An enchanted cloak",
    "A sacred relic",
    "A magical lantern",
    "Something to provide hope in a dire time",
    "A protective charm",
    "A powerful spellbook",
    "Something to entertain during the long nights",
    "A wise mentor",
    "A mysterious key",
    "A rare coin",
    "A mystical orb",
    "A resilient helm",
    "A powerful artifact",
    "Something to guide your way",
    "Healing herbs",
    "A legendary accessory",
    "Mythical jewelery",
    "Something to fend off the chill of night"
  };

  public static string GetRandomItemCaptureDialog()
  {
    return MathHelpers.SelectFromRange(ITEM_CAPTURE_RESPONSES, new Random());
  }

  public static List<string> GetItemStrings(int count)
  {
    var list = new List<string>();
    var itemCopy = new List<string>(ITEMS_OF_POWER);
    var rand = new Random();
    for (int i = 0; i < count; i++)
    {
      var item = MathHelpers.SelectFromRange(itemCopy, rand);
      itemCopy.Remove(item);
      list.Add(item);
    }
    return list;
  }
}