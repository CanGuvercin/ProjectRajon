using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RAJON — HealthBarUI
/// Sol alt köşedeki kırmızı can barını PlayerHealth'e bağlar.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private PlayerHealth _health;
    [SerializeField] private Image        _fillImage;

    [Header("Renk")]
    [SerializeField] private Color _fullColor    = new Color(0.8f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color _lowColor     = new Color(0.3f, 0.0f, 0.0f, 1f);
    [SerializeField] private float _lowThreshold = 0.25f;

    private void Awake()
    {
        if (_fillImage == null)
            _fillImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.OnHealthChanged += UpdateBar;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnHealthChanged -= UpdateBar;
    }

    private void Start()
    {
        if (_health != null)
            UpdateBar(_health.Current, _health.Max);
    }

    private void UpdateBar(float current, float max)
    {
        float ratio = max > 0f ? current / max : 0f;
        _fillImage.fillAmount = ratio;
        _fillImage.color      = Color.Lerp(_lowColor, _fullColor,
                                           Mathf.InverseLerp(0f, _lowThreshold, ratio));
    }
}