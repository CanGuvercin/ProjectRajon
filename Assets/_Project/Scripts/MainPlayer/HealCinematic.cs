using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HealCinematic : MonoBehaviour
{
    [Header("Kamera")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float  _zoomInSize   = 3.0f;
    [SerializeField] private float  _normalSize   = 5.0f;
    [SerializeField] private float  _zoomInSpeed  = 8f;
    [SerializeField] private float  _zoomOutSpeed = 4f;

    [Header("Fade")]
    [SerializeField] private float _fadeDuration   = 0.3f;
    [SerializeField] private float _fadeInDuration = 0.5f;

    [Header("Süre")]
    [SerializeField] private float _smokingDuration = 2.0f;

    [Header("Müzik")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip   _healClip;

    // Cinematic tamamen bitince fırlatılır — PlayerController dinler
    public event Action OnFinished;

    private List<SpriteRenderer>              _worldSprites  = new List<SpriteRenderer>();
    private Dictionary<SpriteRenderer, float> _spriteAlphas  = new Dictionary<SpriteRenderer, float>();
    private List<Tilemap>                     _worldTilemaps = new List<Tilemap>();
    private Dictionary<Tilemap, float>        _tilemapAlphas = new Dictionary<Tilemap, float>();
    private List<ItemGlow>                    _activeGlows   = new List<ItemGlow>();

    private Coroutine _routine;

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

    private IEnumerator CinematicRoutine()
    {
        CollectWorldObjects();

        if (_audioSource && _healClip)
        {
            _audioSource.clip = _healClip;
            _audioSource.Play();
        }

        StartCoroutine(FadeWorld(0f, _fadeDuration));
        yield return StartCoroutine(ZoomTo(_zoomInSize, _zoomInSpeed));

        yield return new WaitForSeconds(_smokingDuration);

        StartCoroutine(FadeWorld(1f, _fadeInDuration));
        yield return StartCoroutine(ZoomTo(_normalSize, _zoomOutSpeed));

        if (_audioSource && _audioSource.isPlaying) _audioSource.Stop();

        RestoreGlows();

        _routine = null;
        OnFinished?.Invoke();
    }

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

        // ItemGlow'ları durdur — cinematic sırasında parlamasınlar
        _activeGlows.Clear();
        foreach (var glow in FindObjectsOfType<ItemGlow>())
        {
            _activeGlows.Add(glow);
            glow.enabled = false;
        }
    }

    private IEnumerator FadeWorld(float targetAlpha, float duration)
    {
        var spriteStart  = new Dictionary<SpriteRenderer, float>();
        var tilemapStart = new Dictionary<Tilemap, float>();

        foreach (var sr in _worldSprites)  if (sr) spriteStart[sr]  = sr.color.a;
        foreach (var tm in _worldTilemaps) if (tm) tilemapStart[tm] = tm.color.a;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float e = targetAlpha == 0f ? EaseOutQuart(t) : EaseInQuart(t);

            foreach (var sr in _worldSprites)
            {
                if (!sr) continue;
                Color c = sr.color; c.a = Mathf.Lerp(spriteStart[sr], targetAlpha, e); sr.color = c;
            }
            foreach (var tm in _worldTilemaps)
            {
                if (!tm) continue;
                Color c = tm.color; c.a = Mathf.Lerp(tilemapStart[tm], targetAlpha, e); tm.color = c;
            }
            yield return null;
        }

        foreach (var sr in _worldSprites)  { if (!sr) continue; Color c = sr.color; c.a = targetAlpha; sr.color = c; }
        foreach (var tm in _worldTilemaps) { if (!tm) continue; Color c = tm.color; c.a = targetAlpha; tm.color = c; }
    }

    private void RestoreWorld()
    {
        foreach (var pair in _spriteAlphas)  { if (!pair.Key) continue; Color c = pair.Key.color; c.a = pair.Value; pair.Key.color = c; }
        foreach (var pair in _tilemapAlphas) { if (!pair.Key) continue; Color c = pair.Key.color; c.a = pair.Value; pair.Key.color = c; }
        RestoreGlows();
    }

    private void RestoreGlows()
    {
        foreach (var glow in _activeGlows)
            if (glow) glow.enabled = true;
    }

    private IEnumerator ZoomTo(float target, float speed)
    {
        if (_camera == null) yield break;

        float timeout = 5f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            _camera.orthographicSize = Mathf.MoveTowards(
                _camera.orthographicSize, target, speed * Time.deltaTime);

            if (Mathf.Abs(_camera.orthographicSize - target) < 0.01f)
            {
                _camera.orthographicSize = target;
                yield break;
            }
            yield return null;
        }

        _camera.orthographicSize = target;
    }

    private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
    private static float EaseInQuart(float t)  => t * t * t * t;
}