using BriLib;

public class UIManager : Singleton<UIManager>
{
  public LongPressButton LongPressButton;
  public FadingAndSlidingScreen TakePictureButton;
  public FadingAndSlidingScreen PortalSpawn;
  public PortalActivater PortalActivater;
  public StoryResultScreen StoryResult;
  public TitleScreen TitleScreen;
  public YesNoScreen YesNoScreen;
  public LoadingScreen LoadingScreen;
}