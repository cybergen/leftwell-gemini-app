using System.Threading.Tasks;
using UnityEngine;
using LLM.Network;

public class BigPortalSequence : ISequence<LLMRequestPayload, BigPortalResult>
{
  public async Task<BigPortalResult> RunAsync(LLMRequestPayload arg)
  {
    arg.contents[arg.contents.Count - 1].parts.Add(new TextPart
    {
      text = "Ready to journey!"
    });
    var payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(arg);
    arg = payloadReplyPair.Item1;

    var imagePrompts = await ImagePromptGenerator.Instance.GetPromptAndNegativePrompt(arg);
    var images = await ImageGenerationManager.Instance.GetImagesBase64Encoded(imagePrompts.Item1, imagePrompts.Item2);

    //TODO: Actually select the best one somehow
    var image = ImageGenerationManager.Base64ToTexture(images[0]);
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