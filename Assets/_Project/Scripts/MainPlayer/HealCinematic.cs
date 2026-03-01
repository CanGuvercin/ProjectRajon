using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// RAJON — HealCinematic
/// Sigara yakma ritüeli: dünya solar, kamera zoom yapar, Emmi yalnız kalır.
/// 
/// Dünya karartma: SpriteRenderer ("Player" / "UI" tag hariç) + tüm Tilemap'ler solar.
/// Time.timeScale dokunulmaz — IsHealing guard Emmi'yi korur.
/// </summary>
public class HealCinematic : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Kamera
    // -------------------------------------------------------------------------
    [Header("Kamera")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float  _zoomInSize   = 3.0f;
    [SerializeField] private float  _normalSize   = 5.0f;
    [SerializeField] private float  _zoomInSpeed  = 8f;
    [SerializeField] private float  _zoomOutSpeed = 4f;

    // -------------------------------------------------------------------------
    // Fade
    // -------------------------------------------------------------------------
    [Header("Fade")]
    [SerializeField] private float _fadeDuration   = 0.3f;
    [SerializeField] private float _fadeInDuration = 0.5f;

    // -------------------------------------------------------------------------
    // Animasyon Süresi
    // -------------------------------------------------------------------------
    [Header("Süre")]
    [SerializeField] private float _smokingDuration = 1.333f; // Smoking_Emmi animasyon süresi

    // -------------------------------------------------------------------------
    // Müzik
    // -------------------------------------------------------------------------
    [Header("Müzik")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip   _healClip;

    // -------------------------------------------------------------------------
    // İç Durum — SpriteRenderer
    // -------------------------------------------------------------------------
    private List<SpriteRenderer>              _worldSprites  = new List<SpriteRenderer>();
    private Dictionary<SpriteRenderer, float> _spriteAlphas  = new Dictionary<SpriteRenderer, float>();

    // -------------------------------------------------------------------------
    // İç Durum — Tilemap
    // -------------------------------------------------------------------------
    private List<Tilemap>              _worldTilemaps = new List<Tilemap>();
    private Dictionary<Tilemap, float> _tilemapAlphas = new Dictionary<Tilemap, float>();

    private Coroutine _routine;

    // -------------------------------------------------------------------------
    // HealSystem'dan çağrılır
    // -------------------------------------------------------------------------
    public void Play()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(CinematicRoutine());
    }

    public void Stop()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        RestoreWorld();
        if (_camera) _camera.orthographicSize = _normalSize;
        if (_audioSource && _audioSource.isPlaying) _audioSource.Stop();
    }

    // -------------------------------------------------------------------------
    // Ana Ritüel
    // -------------------------------------------------------------------------
    private IEnumerator CinematicRoutine()
    {
        CollectWorldObjects();

        if (_audioSource && _healClip)
        {
            _audioSource.clip = _healClip;
            _audioSource.Play();
        }

        // Dünya solar + zoom in (paralel)
        StartCoroutine(FadeWorld(0f, _fadeDuration));
        yield return StartCoroutine(ZoomTo(_zoomInSize, _zoomInSpeed));

        // Emmi sigara animasyonu bekle
        yield return new WaitForSeconds(_smokingDuration);

        // Dünya geri + zoom out (paralel)
        StartCoroutine(FadeWorld(1f, _fadeInDuration));
        yield return StartCoroutine(ZoomTo(_normalSize, _zoomOutSpeed));

        if (_audioSource && _audioSource.isPlaying) _audioSource.Stop();

        _routine = null;
    }

    // -------------------------------------------------------------------------
    // Sahne Objelerini Topla
    // -------------------------------------------------------------------------
    private void CollectWorldObjects()
    {
        _worldSprites.Clear();
        _spriteAlphas.Clear();
        _worldTilemaps.Clear();
        _tilemapAlphas.Clear();

        foreach (var sr in FindObjectsOfType<SpriteRenderer>())
        {
            if (sr.CompareTag("Player") || sr.CompareTag("UI")) continue;
            _worldSprites.Add(sr);
            _spriteAlphas[sr] = sr.color.a;
        }

        foreach (var tm in FindObjectsOfType<Tilemap>())
        {
            _worldTilemaps.Add(tm);
            _tilemapAlphas[tm] = tm.color.a;
        }
    }

    // -------------------------------------------------------------------------
    // Fade (0 = solar, 1 = geri gel)
    // -------------------------------------------------------------------------
    private IEnumerator FadeWorld(float targetAlpha, float duration)
    {
        // Başlangıç değerlerini snapshot al
        var spriteStart  = new Dictionary<SpriteRenderer, float>();
        var tilemapStart = new Dictionary<Tilemap, float>();

        foreach (var sr in _worldSprites)
            if (sr) spriteStart[sr] = sr.color.a;

        foreach (var tm in _worldTilemaps)
            if (tm) tilemapStart[tm] = tm.color.a;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float e = targetAlpha == 0f ? EaseOutQuart(t) : EaseInQuart(t);

            foreach (var sr in _worldSprites)
            {
                if (!sr) continue;
                Color c = sr.color;
                c.a = Mathf.Lerp(spriteStart[sr], targetAlpha, e);
                sr.color = c;
            }

            foreach (var tm in _worldTilemaps)
            {
                if (!tm) continue;
                Color c = tm.color;
                c.a = Mathf.Lerp(tilemapStart[tm], targetAlpha, e);
                tm.color = c;
            }

            yield return null;
        }

        // Kesin değer
        foreach (var sr in _worldSprites)
        {
            if (!sr) continue;
            Color c = sr.color; c.a = targetAlpha; sr.color = c;
        }
        foreach (var tm in _worldTilemaps)
        {
            if (!tm) continue;
            Color c = tm.color; c.a = targetAlpha; tm.color = c;
        }
    }

    // -------------------------------------------------------------------------
    // Acil Restore (Stop çağrılırsa)
    // -------------------------------------------------------------------------
    private void RestoreWorld()
    {
        foreach (var pair in _spriteAlphas)
        {
            if (!pair.Key) continue;
            Color c = pair.Key.color; c.a = pair.Value; pair.Key.color = c;
        }
        foreach (var pair in _tilemapAlphas)
        {
            if (!pair.Key) continue;
            Color c = pair.Key.color; c.a = pair.Value; pair.Key.color = c;
        }
    }

    // -------------------------------------------------------------------------
    // Zoom
    // -------------------------------------------------------------------------
    private IEnumerator ZoomTo(float target, float speed)
    {
        if (_camera == null) yield break;

        while (!Mathf.Approximately(_camera.orthographicSize, target))
        {
            _camera.orthographicSize = Mathf.MoveTowards(
                _camera.orthographicSize, target, speed * Time.deltaTime);
            yield return null;
        }

        _camera.orthographicSize = target;
    }

    // -------------------------------------------------------------------------
    // Easing
    // -------------------------------------------------------------------------
    private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
    private static float EaseInQuart(float t)  => t * t * t * t;
}