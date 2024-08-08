using System;
using System.Collections.Generic;
using BriLib;

public class AdventureDialog
{
  public const string POSITION_PORTAL 
    = "I'll get these items ready. In the meantime, give me an open area where I can form the portal";
  public const string PORTAL_PLACED = "Perfect! I'll start working the big magic to get this spun up!";
  public const string ITEMS_READY = "Your items have been transformed! Activate each one for me!";
  public const string PORTAL_NOT_READY = "Just a moment, I'm still trying to get this portal opened...";
  public const string PORTAL_READY = "The portal is ready!";
  public const string NO_STORY_CHOICE = "Picky picky! I'll choose for you then...";
  public const string STORY_OPTIONS_INTRO = "Here are your story options.";
  public const string OPENING_PORTAL = "This thing is actually working!";
  public const string CLOSE_PORTAL = "I'll just close this little rip in reality back up then.";
  public const int DIALOG_PAUSE = 500;
  public const string CLOSE_PORTAL_QUESTION = "We make a great team! Close the portal down whenever you're ready.";
  public const string GO_AGAIN_QUESTION = "So whaddaya say? Wanna go again?";

  //Error states
  public const string FAILED_TO_GET_HERO_IMAGE = "This is bad! The portal's tunneled through to a reality with no visible light spectrum. We're not gonna be able to see anything on this one!";
  public const string FAILED_TO_GET_ITEM_IMAGE = "Something's corrupted one of the items! It might not look any different...";

  public static List<string> ITEM_CAPTURE_RESPONSES = new List<string>
  {
    "Uh... Interesting. Tell me more.",
    "Wow. What do you have to say for yourself?",
    "Anything I should know...?",
    "Why this choice?",
    "Looks powerful. Care to explain?",
    "This needs an explanation...",
    "You sure? Why?",
    "Tell me more.",
    "Wise choice! Tell me about this.",
    "Weird, but maybe you have a good reason?",
    "That's a first. Why this choice?",
    "I'm sensing a palpable aura. Why's that?",
    "Ha! Care to elaborate?",
    "Unbelievable choice. Want to explain?",
    "What are you showing me?",
    "Is this the right call?",
    "More info please.",
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

  public static List<string> STORY_OPTIONS = new List<string>
  {
    "Defeat the Dark Lord",
    "Reincarnated as a Villainess",
    "The Haunted Castle",
    "The Underwater Kingdom",
    "The Cursed Forest",
    "The Wizard's Tower",
    "The Secret Society",
    "The Sacred Artifact",
    "The Time Traveler's Dilemma",
    "The Lost City of Gold",
    "The Dragon's Lair",
    "The Space Pirate's Treasure",
    "The Desert Nomad's Quest",
    "The Enchanted Garden",
    "The Ghost Ship",
    "The Rebel's Rebellion",
    "The Alien Invasion",
    "The Phantom Thief",
    "The Elven Alliance",
    "The Shadow Realm",
    "The Hero's Journey",
    "The Vampire's Masquerade",
    "The Cybernetic Uprising",
    "The Monster Hunter",
    "The Celestial Prophecy",
    "The Demon King's Revenge",
    "The Forgotten Kingdom",
    "The Immortal Sorcerer",
    "The Parallel Universe",
    "The Legendary Samurai",
    "The Golden Dirigible",
    "The Fairy Barrow",
    "The Robot Philosopher",
    "The Twelfth Monkey",
    "The Lost Heir",
    "The Pirate's Curse",
    "The Secret of the Labyrinth",
    "The Titan's Awakening",
    "The Witch's Brew",
    "The Quest for Immortality",
    "The Hollow Earth",
    "The Diamond Planet"
  };

  public static List<string> OPTION_PRECEDENTS = new List<string>
  {
    "How about... ",
    "What do you think of... ",
    "Okay, so hear me out... ",
    "What about... ",
    "Want to try... ",
    "Do you like... ",
    "Thoughts on... ",
    "What if we ran... ",
    "Hey! You want... ",
  };

  public static List<string> FAILED_COMMENTARY = new List<string>
  {
    "Uh, I hope this one works out",
    "Not much to say about this that you haven't already covered",
    "This Item speaks for itself",
    "Weird Item choice, in retrospect",
    "Well that's cool",
    "Yeah sorry, not much to add here",
    "You're sure it was vital to bring this?",
    "Hope you're right about this one...",
    "Good call",
    "Smart!",
    "Weird...",
  };

  public static List<string> MAGIC_APPLIED_RESPONSES = new List<string>
  {
    "Tossing some magic on.",
    "Just a dash of mana and...",
    "Sprinkling some magic on!",
    "Let's see how this reacts...",
    "And a little magic here...",
    "Juicing it up.",
  };

  public static List<string> TAKING_LONG_OPTIONS = new List<string>
  {
    "This is taking awhile...",
    "Complicated spell...",
    "Still working, kid!",
    "Making progress!",
    "This is hard work",
    "Almost done I think...",
    "Just a bit more...",
    "Almost there!",
    "Almost got it!",
    //"Getting closer.", 
    "Just a little more...",
  };

  public static string GetRandomOptionPrecedent()
  {
    return MathHelpers.SelectFromRange(OPTION_PRECEDENTS, new Random());
  }

  public static string GetRandomItemCaptureDialog()
  {
    return MathHelpers.SelectFromRange(ITEM_CAPTURE_RESPONSES, new Random());
  }

  public static string GetRandomMagicAppliedDialog()
  {
    return MathHelpers.SelectFromRange(MAGIC_APPLIED_RESPONSES, new Random());
  }

  public static string GetRandomTakingLong()
  {
    return MathHelpers.SelectFromRange(TAKING_LONG_OPTIONS, new Random());
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

  public static List<string> GetStoryStrings(int count)
  {
    var list = new List<string>();
    var itemCopy = new List<string>(STORY_OPTIONS);
    var rand = new Random();
    for (int i = 0; i < count; i++)
    {
      var item = MathHelpers.SelectFromRange(itemCopy, rand);
      itemCopy.Remove(item);
      list.Add(item);
    }
    return list;
  }

  public static string GetRandomFailedCommentary(Random rand)
  {
    return MathHelpers.SelectFromRange(FAILED_COMMENTARY, rand);
  }
}