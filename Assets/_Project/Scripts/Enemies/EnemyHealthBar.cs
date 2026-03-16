using UnityEngine;

/// <summary>
/// RAJON — EnemyHealthBar
/// Düşmanın üstündeki HP bar (World Space, SpriteRenderer).
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyHealth _health;
    [SerializeField] private Transform _filler;
    [SerializeField] private SpriteRenderer _fillerRenderer;

    private Vector3 _originalScale;
    private Vector3 _originalLocalPos;

    private void Awake()
    {
        if (_health == null)
            _health = GetComponentInParent<EnemyHealth>();

        if (_filler != null)
        {
            _originalScale = _filler.localScale;
            _originalLocalPos = _filler.localPosition;
        }
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
    {
        UpdateBar(_health.Current, _health.Max);
    }
    else
    {
    }
}

    private void UpdateBar(float current, float max)
{
    if (_filler == null) 
    {
        Debug.LogWarning("Filler null!");//
        return;
    }

    float ratio = max > 0f ? current / max : 0f;
    Vector3 newScale = _originalScale;
    newScale.x = _originalScale.x * ratio;
    _filler.localScale = newScale;

    Vector3 newPos = _originalLocalPos;
    newPos.x = _originalLocalPos.x - (_originalScale.x - newScale.x) * 0.5f;
    _filler.localPosition = newPos;
}
}