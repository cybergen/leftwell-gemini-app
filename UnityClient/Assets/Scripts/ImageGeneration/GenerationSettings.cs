using BriLib;
using System.Collections.Generic;

public class GenerationSettings
{
  public const int IMAGE_GEN_SAMPLES = 1;
  public const string IMAGE_GEN_ASPECT = "1:1";
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
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "mystical, fantasy, glimmering, purple clouds, magic, glowing",
        NegativePrompt = "ugly colors, concrete, grey, boring",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "watercolor, colorful",
        NegativePrompt = "scary, ugly",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "pencil sketch, drawn, sketch, charcoal",
        NegativePrompt = "scary, ugly",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "cartoon, line art, vibrant color",
        NegativePrompt = "ugly",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "low poly, geometric, toon shading, ps1, retro 3d",
        NegativePrompt = "ugly, desaturated",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "van gogh",
        NegativePrompt = "photo, desaturated, realistic",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "picasso, cubism",
        NegativePrompt = "photo, desaturated, realistic",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "hokusai, ukiyoe",
        NegativePrompt = "photo, desaturated, realistic",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "warhol",
        NegativePrompt = "painting",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "comic book, half tone, line art, primary colors, bright",
        NegativePrompt = "realistic, photo",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagen-3.0-fast-generate-001",
        Prompt = "dali",
        NegativePrompt = "realistic, photo",
        PersonGeneration = "dont_allow",
      },
      new EditOptions
      {
        Model = "imagegeneration@006",
        Prompt = "fantasy, magical, colorful smoke, waves, clouds",
        NegativePrompt = "ugly, grey",
        PersonGeneration = "dont_allow",
        EditMode = "product-image"
      },
      new EditOptions
      {
        Model = "imagegeneration@006",
        Prompt = "explosion, flames, smoke, blast, bright, colorful, orange, flare, lava, glowing, eruption, tense, centered",
        NegativePrompt = "ugly, grey, boring",
        PersonGeneration = "dont_allow",
        EditMode = "product-image"
      },
    };
    return MathHelpers.SelectFromRange(editOptions, new System.Random());
  }
}

public class EditOptions
{
  public string Model;
  public string Prompt;
  public string NegativePrompt;
  public string PersonGeneration;
  public string EditMode;
}