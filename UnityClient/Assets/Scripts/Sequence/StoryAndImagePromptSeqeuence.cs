using System.Threading.Tasks;
using UnityEngine;
using LLM.Network;
using static ImageGenerationManager;
using System.Collections.Generic;

public class StoryAndImagePromptSeqeuence : ISequence<LLMRequestPayload, BigPortalResult>
{
  public async Task<BigPortalResult> RunAsync(LLMRequestPayload arg)
  {
    arg.contents[arg.contents.Count - 1].parts.Add(new TextPart
    {
      text = "Ready to journey!"
    });
    var payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(arg);
    arg = payloadReplyPair.Item1;
    (List<string> images, ImageGenStatus status) imageGenResponse;
    int tries = 0;

    do
    {
      tries++;
      var imagePrompts = await ImagePromptGenerator.Instance.GetPromptAndNegativePrompt(arg);
      imageGenResponse = await ImageGenerationManager.Instance.GetImagesBase64Encoded(imagePrompts.Item1, imagePrompts.Item2);
    }
    while (tries < 3 && (imageGenResponse.status == ImageGenStatus.FailedDueToSafetyGuidelines 
      || imageGenResponse.status == ImageGenStatus.FailedForOtherReason));

    Texture2D image;
    if (imageGenResponse.status != ImageGenStatus.Succeeded && imageGenResponse.status != ImageGenStatus.SucceededAfterRetry)
    {
      await SpeechManager.Instance.Speak(DialogConstants.FAILED_TO_GET_HERO_IMAGE);
      image = ErrorStateManager.Instance.FailedHeroImage;
    }
    else
    {
      image = Base64ToTexture(imageGenResponse.images[0]);
    }
    

    return new BigPortalResult
    {
      FinalImage = image,
      Reply = payloadReplyPair.Item2,
      Payload = arg,
    };
  }
}

public class BigPortalResult
{
  public Texture2D FinalImage;
  public string Reply;
  public LLMRequestPayload Payload;
}