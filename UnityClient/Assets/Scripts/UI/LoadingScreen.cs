using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Image _fillImage;

    public void Show()
    {
        _fillImage.fillAmount = 0f;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetProgress(float amount)
    {
        _fillImage.fillAmount = amount;
    }

    private void Awake()
    {
        _fillImage.fillAmount = 0f;
        gameObject.SetActive(false);
    }
}