using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private float _cigaretteRiseAmount = 80f;
    [SerializeField] private float _riseDuration        = 0.45f;
    [SerializeField] private float _returnDuration      = 0.2f;

    private RectTransform _rt;
    private int           _currentCount;
    private Vector2[]     _slotHomePositions;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    private void Start()
    {
        _slotHomePositions = new Vector2[_slots.Length];
        for (int i = 0; i < _slots.Length; i++)
            _slotHomePositions[i] = _slots[i].anchoredPosition;

        _rt.anchoredPosition = _defaultPos;

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

    // Aktif olan en sağdaki slotu bul — index math'e güvenme
    private int GetExitSlotIndex()
    {
        for (int i = _slots.Length - 1; i >= 0; i--)
            if (_slots[i].gameObject.activeSelf) return i;
        return -1;
    }

    private IEnumerator HealSequence()
    {
        int exitIndex = GetExitSlotIndex();
        if (exitIndex < 0) yield break;

        StartCoroutine(ExitCigarette(exitIndex));
        yield return StartCoroutine(MovePacket(_openPos, _riseDuration));

        _healSystem.OnHealFinished += ReturnToDefault;
    }

    private IEnumerator PickupSequence(int newCount)
    {
        yield return StartCoroutine(MovePacket(_openPos, _riseDuration));

        int fillIndex = newCount - 1;
        if (fillIndex >= 0 && fillIndex < _slots.Length)
        {
            var slot = _slots[fillIndex];
            slot.anchoredPosition = _slotHomePositions[fillIndex];
            var img = slot.GetComponent<Image>();
            if (img)
            {
                slot.gameObject.SetActive(true);
                yield return StartCoroutine(FadeSlot(img, 0f, 1f, 0.3f, easeIn: true));
            }
        }

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(MovePacket(_defaultPos, _returnDuration));
    }

    private void ReturnToDefault()
    {
        _healSystem.OnHealFinished -= ReturnToDefault;
        StartCoroutine(MovePacket(_defaultPos, _returnDuration));
    }

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

    private IEnumerator ExitCigarette(int index)
    {
        if (_slotHomePositions == null) yield break;
        if (index < 0 || index >= _slots.Length) yield break;

        var rt  = _slots[index];
        var img = rt.GetComponent<Image>();
        if (img == null) yield break;

        rt.anchoredPosition = _slotHomePositions[index];
        img.color = Color.white;

        Vector2 startPos = _slotHomePositions[index];
        Vector2 endPos   = startPos + Vector2.up * _cigaretteRiseAmount;
        float   elapsed  = 0f;

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

    private void RefreshSlots()
    {
        if (_slotHomePositions == null) return;

        for (int i = 0; i < _slots.Length; i++)
        {
            bool active = i < _currentCount;
            _slots[i].gameObject.SetActive(active);
            if (active)
            {
                _slots[i].anchoredPosition = _slotHomePositions[i];
                var img = _slots[i].GetComponent<Image>();
                if (img) img.color = Color.white;
            }
        }
    }

    private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
    private static float EaseInQuart(float t)  => t * t * t * t;
}