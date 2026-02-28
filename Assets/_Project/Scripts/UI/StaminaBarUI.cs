using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RAJON — StaminaBarUI
/// Sol alt köşedeki mavi stamina barını StaminaSystem'e bağlar.
/// Hierarchy: Canvas → StaminaBar (Image, Image Type: Filled) → bu script
/// </summary>
public class StaminaBarUI : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private StaminaSystem _stamina;
    [SerializeField] private Image         _fillImage;   // Image Type: Filled olmalı

    [Header("Renk")]
    [SerializeField] private Color _fullColor  = new Color(0.2f, 0.5f, 1f, 1f);  // mavi
    [SerializeField] private Color _lowColor   = new Color(1f,   0.3f, 0.1f, 1f); // kırmızı
    [SerializeField] private float _lowThreshold = 0.25f; // %25 altında renk değişir

    private void Awake()
    {
        if (_fillImage == null)
            _fillImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (_stamina != null)
            _stamina.OnStaminaChanged += UpdateBar;
    }

    private void OnDisable()
    {
        if (_stamina != null)
            _stamina.OnStaminaChanged -= UpdateBar;
    }

    private void UpdateBar(float current, float max)
    {
        float ratio = max > 0f ? current / max : 0f;
        _fillImage.fillAmount = ratio;
        _fillImage.color      = Color.Lerp(_lowColor, _fullColor,
                                           Mathf.InverseLerp(0f, _lowThreshold, ratio));
    }
}