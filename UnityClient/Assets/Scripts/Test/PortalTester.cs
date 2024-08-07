using System.Threading.Tasks;
using UnityEngine;

public class PortalTester : MonoBehaviour
{
  [SerializeField] private PortalActivater _activater;
  [SerializeField] private Texture2D _markerImage;
  [SerializeField] private Texture2D _portalImage;
  [SerializeField] private GameObject _dragonPrefab;
  private CharacterBehaviorController _dragon;
  private int _markers = 0;
  private bool _hasBigPortal;

  public async void OnSpawnMarker()
  {
    _markers++;
    await CameraManager.Instance.GetNextAvailableCameraImage();
    var camImage = await CameraManager.Instance.GetNextAvailableCameraImage();
    var captureMarker = new CaptureMarkerSequence("Test item");
    _ = captureMarker.RunAsync(camImage.Texture);
    _dragon.SetState(CharacterState.ShownObject);
    captureMarker.SetCommentary(AdventureDialog.FAILED_COMMENTARY[4]);
    
    //Simulate talking pause
    await Task.Delay(4000);
    _dragon.SetState(CharacterState.MagicingItem);
    await Task.Delay(AdventureDialog.DIALOG_PAUSE);
    _ = SpeechManager.Instance.Speak(AdventureDialog.GetRandomMagicAppliedDialog());
  }

  public void OnSetMarkersActivatable()
  {
    if (_markers == 0) return;
    for (int i = 0; i < _markers; i++)
    {
      PortalManager.Instance.SetMarkerActivatable(i, _markerImage, null);
    }
  }

  public void OnSpawnPortal()
  {
    if (_hasBigPortal) return;
    _hasBigPortal = true;
    PortalManager.Instance.SpawnHeroPortal();
  }

  public void OnSetPortalActivatable()
  {
    if (!_hasBigPortal) return;
    PortalManager.Instance.SetHeroPortalActivatable(() =>
    {
      Debug.Log("Big portal activated");
      PortalManager.Instance.SetHeroPortalClosable(() => Debug.Log("Big portal closed"));
    });
  }

  private void Update()
  {
    if (_dragon != null) { return; }
    if (PlaneManager.Instance.Ready)
    {
      _dragon = Instantiate(_dragonPrefab).GetComponent<CharacterBehaviorController>();
      _dragon.SetState(CharacterState.InitialFlyIn);
      SpeechManager.Instance.SetSpeechSource(_dragon.GetComponent<AudioSource>());
      _dragon.SetMode(CharacterMode.AvoidCenter);
    }
  }

  private void Awake()
  {
    UIManager.Instance.PortalActivater.SetShowable(true, Camera.main.transform);
  }
}