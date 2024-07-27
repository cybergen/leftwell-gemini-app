using BriLib;
using System.Collections.Generic;

public class GenerationSettings
{
  public const int IMAGE_GEN_SAMPLES = 4;
  public const string IMAGE_GEN_ASPECT = "16:9";
  public const string IMAGE_GEN_PERSON_GEN = "allow_all";
  public const string IMAGE_GEN_SAFETY = "block_few";

  public const int UPSCALE_SAMPLE_COUNT = 1;
  public const string UPSCALE_FACTOR = "2x";
  public const string UPSCALE_MODE = "upscale";

  public static EditOptions GetRandomEditOptions()
  {
    var editOptions = new List<EditOptions>
    {
      new EditOptions
      {
        Model = "imagegeneration@002",
        Prompt = "mystical, fantasy, glimmering, purple clouds, magic, glowing",
        NegativePrompt = "ugly colors, concrete, brutalism, grey, boring",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagegeneration@002",
        Prompt = "made of magical smoke, colorful, magical, fantasy, glowing",
        NegativePrompt = "ugly, bland, grey, dull",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagegeneration@002",
        Prompt = "watercolor, beautiful, artistic, colorful, stylish",
        NegativePrompt = "scary, people, ugly",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagegeneration@002",
        Prompt = "pencil sketch, drawn, black and white, cross-hatching, sketchy, charcoal",
        NegativePrompt = "scary, people, ugly",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagegeneration@002",
        Prompt = "cartoon, cartoony, line art, vibrant colors",
        NegativePrompt = "ugly",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagegeneration@006",
        Prompt = "fantasy, magical, colorful, exalted, powerful, emanating force, waves, color",
        NegativePrompt = "ugly colors, concrete, brutalism, grey, boring",
        PersonGeneration = "dont_allow",
        EditMode = "product-image"
      },
      new EditOptions
      {
        Model = "imagegeneration@006",
        Prompt = "explosion, flames, smoke, blast, bright, colorful, orange, flare, lava, glowing, eruption, tense, centered",
        NegativePrompt = "ugly colors, concrete, brutalism, grey, boring",
        PersonGeneration = "dont_allow",
        EditMode = "product-image"
      },
    };
    return MathHelpers.SelectFromRange(editOptions, new System.Random());
  }
}