using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RAJON — CigaretteBarUI
/// Zazaboro paketi UI animasyonu.
/// </summary>
public class CigaretteBarUI : MonoBehaviour
{
    [Header("Sistem")]
    [SerializeField] private HealSystem _healSystem;

    [Header("Sigara Slotları (soldan sağa: S1→S4)")]
    [SerializeField] private RectTransform[] _slots;

    [Header("CigaretteUI Pozisyonları")]
    [SerializeField] private Vector2 _defaultPos = new Vector2(68f, -10f);
    [SerializeField] private Vector2 _openPos    = new Vector2(68f,  59f);

    [Header("Sigara Çıkış")]
    [SerializeField] private float _cigaretteRiseAmount = 80f; // kaç px yukarı çıkar (göreceli)
    [SerializeField] private float _riseDuration        = 0.45f;
    [SerializeField] private float _returnDuration      = 0.2f;

    private RectTransform _rt;
    private int           _currentCount;
    private Vector2[]     _slotDefaultPositions;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _rt.anchoredPosition = _defaultPos;

        _slotDefaultPositions = new Vector2[_slots.Length];
        for (int i = 0; i < _slots.Length; i++)
            _slotDefaultPositions[i] = _slots[i].anchoredPosition;
    }

    private void Start()
    {
        if (_healSystem == null) return;
        _currentCount = _healSystem.CurrentCigarettes;
        RefreshSlots();
    }

    private void OnEnable()
    {
        if (_healSystem == null) return;
        _healSystem.OnHealStarted       += OnHealStarted;
        _healSystem.OnCigarettesChanged += OnCigarettesChanged;
    }

    private void OnDisable()
    {
        if (_healSystem == null) return;
        _healSystem.OnHealStarted       -= OnHealStarted;
        _healSystem.OnCigarettesChanged -= OnCigarettesChanged;
    }

    // -------------------------------------------------------------------------
    // Event Handlers
    // -------------------------------------------------------------------------
    private void OnHealStarted()
    {
        StartCoroutine(HealSequence());
    }

    private void OnCigarettesChanged(int current, int max)
    {
        bool pickedUp = current > _currentCount;
        _currentCount = current;
        if (pickedUp)
            StartCoroutine(PickupSequence(current));
    }

    // -------------------------------------------------------------------------
    // Heal Sekansı
    // -------------------------------------------------------------------------
    private IEnumerator HealSequence()
    {
        // _currentCount zaten düşmüş — tüketilen slot = _currentCount (0-indexed)
        int exitIndex = Mathf.Clamp(_currentCount, 0, _slots.Length - 1);

        // Paket yukarı + sigara çık (paralel)
        StartCoroutine(ExitCigarette(exitIndex));
        yield return StartCoroutine(MovePacket(_openPos, _riseDuration));

        // Heal bitince paketi geri indir
        _healSystem.OnHealFinished += ReturnToDefault;
    }

    // -------------------------------------------------------------------------
    // Pickup Sekansı
    // -------------------------------------------------------------------------
    private IEnumerator PickupSequence(int newCount)
    {
        yield return StartCoroutine(MovePacket(_openPos, _riseDuration));

        int fillIndex = newCount - 1;
        if (fillIndex >= 0 && fillIndex < _slots.Length)
        {
            _slots[fillIndex].anchoredPosition = _slotDefaultPositions[fillIndex];
            var img = _slots[fillIndex].GetComponent<Image>();
            if (img)
            {
                _slots[fillIndex].gameObject.SetActive(true);
                yield return StartCoroutine(FadeSlot(img, 0f, 1f, 0.3f, easeIn: true));
            }
        }

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(MovePacket(_defaultPos, _returnDuration));
    }

    // -------------------------------------------------------------------------
    // Paket Geri Dön
    // -------------------------------------------------------------------------
    private void ReturnToDefault()
    {
        _healSystem.OnHealFinished -= ReturnToDefault;
        StartCoroutine(MovePacket(_defaultPos, _returnDuration));
    }

    // -------------------------------------------------------------------------
    // Paket Hareketi
    // -------------------------------------------------------------------------
    private IEnumerator MovePacket(Vector2 target, float duration)
    {
        Vector2 start   = _rt.anchoredPosition;
        float   elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _rt.anchoredPosition = Vector2.Lerp(start, target, EaseOutQuart(t));
            yield return null;
        }

        _rt.anchoredPosition = target;
    }

    // -------------------------------------------------------------------------
    // Sigara Çıkış — göreceli rise, startPos'tan itibaren yukarı
    // -------------------------------------------------------------------------
    private IEnumerator ExitCigarette(int index)
    {
        if (index < 0 || index >= _slots.Length) yield break;

        var rt  = _slots[index];
        var img = rt.GetComponent<Image>();
        if (img == null || !rt.gameObject.activeSelf) yield break;

        Vector2 startPos = _slotDefaultPositions[index]; // kayıtlı default pos
        Vector2 endPos   = startPos + Vector2.up * _cigaretteRiseAmount; // göreceli
        float   elapsed  = 0f;

        rt.anchoredPosition = startPos; // önce sıfırla
        img.color = Color.white;

        while (elapsed < _riseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _riseDuration);
            float e = EaseOutQuart(t);

            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, e);
            img.color = new Color(1f, 1f, 1f, 1f - e);
            yield return null;
        }

        rt.gameObject.SetActive(false);
        rt.anchoredPosition = startPos;
        img.color = Color.white;
    }

    // -------------------------------------------------------------------------
    // Slot Fade
    // -------------------------------------------------------------------------
    private IEnumerator FadeSlot(Image img, float from, float to, float duration, bool easeIn)
    {
        float elapsed = 0f;
        img.color = new Color(1f, 1f, 1f, from);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float e = easeIn ? EaseInQuart(t) : EaseOutQuart(t);
            img.color = new Color(1f, 1f, 1f, Mathf.Lerp(from, to, e));
            yield return null;
        }

        img.color = new Color(1f, 1f, 1f, to);
    }

    // -------------------------------------------------------------------------
    // Slotları Sıfırla
    // -------------------------------------------------------------------------
    private void RefreshSlots()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            bool active = i < _currentCount;
            _slots[i].gameObject.SetActive(active);
            if (active)
            {
                _slots[i].anchoredPosition = _slotDefaultPositions[i];
                var img = _slots[i].GetComponent<Image>();
                if (img) img.color = Color.white;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Easing
    // -------------------------------------------------------------------------
    private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
    private static float EaseInQuart(float t)  => t * t * t * t;
} //