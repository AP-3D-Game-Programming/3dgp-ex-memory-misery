using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeyholeQTE : MonoBehaviour
{
    [Header("UI Setup")]
    [Tooltip("Het paneel dat aan/uit gaat (De container van de minigame).")]
    public GameObject qtePanel;

    [Tooltip("Het icoontje dat heen en weer beweegt.")]
    public RectTransform icon;

    [Tooltip("Het vakje waar je in moet stoppen.")]
    public RectTransform targetZone;

    [Header("Instellingen")]
    // pressKey is weggehaald omdat we nu hardcoded Muisklik gebruiken
    public float duration = 6f;
    public float baseSpeed = 600f;

    [Range(1f, 2f)]
    public float targetTolerance = 1.0f;

    private Coroutine running;

    void Awake()
    {
        if (qtePanel != null) qtePanel.SetActive(false);
    }

    public bool IsRunning => running != null;

    public void StartQTE(Action onSuccess, Action onFail)
    {
        if (running != null) return;
        running = StartCoroutine(QTECoroutine(onSuccess, onFail));
    }

    public void CancelQTE()
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }
        if (qtePanel != null) qtePanel.SetActive(false);
    }

    private IEnumerator QTECoroutine(Action onSuccess, Action onFail)
    {
        if (qtePanel != null) qtePanel.SetActive(true);
        yield return null;

        // Check referenties
        RectTransform parentRect = qtePanel.GetComponent<RectTransform>();
        if (parentRect == null || icon == null || targetZone == null)
        {
            Debug.LogError("KeyholeQTE: UI referenties ontbreken!");
            FinishFail(onFail);
            yield break;
        }

        // Berekeningen
        float parentWidth = parentRect.rect.width;
        float iconW = icon.rect.width;
        float left = -parentWidth * 0.5f + iconW * 0.5f;
        float right = parentWidth * 0.5f - iconW * 0.5f;
        float playRange = Mathf.Max(0.001f, right - left);

        float responsiveSpeed = baseSpeed * (parentWidth / 1920f);

        float startTime = Time.realtimeSinceStartup;
        bool finished = false;

        Vector2 anchored = icon.anchoredPosition;
        anchored.x = left;
        icon.anchoredPosition = anchored;

        while (!finished && Time.realtimeSinceStartup - startTime < duration)
        {
            float t = (Time.realtimeSinceStartup - startTime) * (responsiveSpeed / playRange);
            float ping = Mathf.PingPong(t, 1f);

            anchored.x = Mathf.Lerp(left, right, ping);
            icon.anchoredPosition = anchored;

            // === HIER IS DE AANPASSING NAAR MUISKLIK ===
            if (Input.GetMouseButtonDown(0)) // 0 is Linker Muisknop
            {
                if (IsIconInTarget())
                {
                    FinishSuccess(onSuccess);
                }
                else
                {
                    FinishFail(onFail);
                }
                finished = true;
            }
            // ============================================

            yield return null;
        }

        if (!finished)
        {
            FinishFail(onFail);
        }
    }

    private bool IsIconInTarget()
    {
        Canvas canvas = qtePanel != null ? qtePanel.GetComponentInParent<Canvas>() : null;
        Camera cam = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = canvas.worldCamera;
            if (cam == null) cam = Camera.main;
        }

        Rect iconRect = GetScreenRect(icon, cam);
        Rect targetRect = GetScreenRect(targetZone, cam);

        float expandX = (targetRect.width * (targetTolerance - 1f)) * 0.5f;
        float expandY = (targetRect.height * (targetTolerance - 1f)) * 0.5f;

        targetRect.xMin -= expandX;
        targetRect.xMax += expandX;
        targetRect.yMin -= expandY;
        targetRect.yMax += expandY;

        return iconRect.Overlaps(targetRect);
    }

    private Rect GetScreenRect(RectTransform rt, Camera cam)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    private void FinishSuccess(Action onSuccess)
    {
        if (qtePanel != null) qtePanel.SetActive(false);
        running = null;
        onSuccess?.Invoke();
    }

    private void FinishFail(Action onFail)
    {
        if (qtePanel != null) qtePanel.SetActive(false);
        running = null;
        onFail?.Invoke();
    }
}