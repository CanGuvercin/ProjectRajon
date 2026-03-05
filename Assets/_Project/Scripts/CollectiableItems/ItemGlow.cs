using UnityEngine;

/// <summary>
/// RAJON — ItemGlow
/// Yerden toplanabilir item'ların altındaki titreşen ışıltı. Umarım atmosferi bozmayız. Hurraa...
/// Child objeye SpriteRenderer (yumuşak daire sprite) + bu script atanır.
/// </summary>
public class ItemGlow : MonoBehaviour
{
    [Header("Renk")]
    [SerializeField] private Color _colorA = new Color(0.6f, 0.5f, 0.8f, 0.8f); // mor
    [SerializeField] private Color _colorB = new Color(0.4f, 0.4f, 0.5f, 0.3f); // gri soluk

    [Header("Hız")]
    [SerializeField] private float _pulseSpeed  = 1.8f;  // renk geçiş hızı
    [SerializeField] private float _scaleSpeed  = 1.2f;  // büyüyüp küçülme hızı
    [SerializeField] private float _scaleMin    = 0.85f;
    [SerializeField] private float _scaleMax    = 1.1f;

    private SpriteRenderer _sr;
    private Vector3        _baseScale;

    private void Awake()
    {
        _sr         = GetComponent<SpriteRenderer>();
        _baseScale  = transform.localScale;
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f; // 0-1 arası

        // Renk
        if (_sr) _sr.color = Color.Lerp(_colorB, _colorA, t);

        // Scale
        float s = Mathf.Lerp(_scaleMin, _scaleMax, t);
        transform.localScale = _baseScale * s;
    }
}