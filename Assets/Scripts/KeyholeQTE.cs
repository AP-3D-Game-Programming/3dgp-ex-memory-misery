using System;
using System.Collections;
using UnityEngine;

public class KeyholeQTE : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Root panel for the QTE (inactive at start).")]
    public GameObject qtePanel;
    [Tooltip("Moving icon RectTransform (Image inside panel).")]
    public RectTransform icon;
    [Tooltip("Target zone RectTransform (Image inside panel).")]
    public RectTransform targetZone;

    [Header("Behavior")]
    public KeyCode pressKey = KeyCode.Space;
    public float duration = 6f;
    public float speed = 600f; // pixels per second
    [Range(0f, 2f)]
    public float targetTolerance = 1f; // multiplier for target width used as tolerance

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
        yield return null; // wait a frame for layout

        RectTransform parentRect = qtePanel.GetComponent<RectTransform>();
        if (parentRect == null || icon == null || targetZone == null)
        {
            Debug.LogWarning("KeyholeQTE: Missing UI references.");
            FinishFail(onFail);
            yield break;
        }

        float parentWidth = parentRect.rect.width;
        float iconW = Mathf.Max(1f, icon.rect.width);
        float left = -parentWidth * 0.5f + iconW * 0.5f;
        float right = parentWidth * 0.5f - iconW * 0.5f;
        float playRange = Mathf.Max(0.001f, right - left);

        // place icon at left
        Vector2 anchored = icon.anchoredPosition;
        anchored.x = left;
        icon.anchoredPosition = anchored;

        float startTime = Time.realtimeSinceStartup;
        bool finished = false;

        while (!finished && Time.realtimeSinceStartup - startTime < duration)
        {
            float t = (Time.realtimeSinceStartup - startTime) * (speed / playRange);
            float ping = Mathf.PingPong(t, 1f);
            anchored.x = Mathf.Lerp(left, right, ping);
            icon.anchoredPosition = anchored;

            if (Input.GetKeyDown(pressKey))
            {
                bool isIn = IsIconInTarget_Debug();
                Debug.Log($"KeyholeQTE: Pressed {pressKey}. iconInTarget={isIn}");
                if (isIn)
                {
                    FinishSuccess(onSuccess);
                }
                else
                {
                    FinishFail(onFail);
                }
                finished = true;
            }

            yield return null;
        }

        if (!finished)
        {
            FinishFail(onFail);
        }
    }

    // New robust screen-space overlap check with debug logging
    private bool IsIconInTarget_Debug()
    {
        Canvas canvas = qtePanel != null ? qtePanel.GetComponentInParent<Canvas>() : null;
        Camera cam = null;
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;
        if (cam == null)
            cam = Camera.main;

        Rect iconRect = GetScreenRect(icon, cam);
        Rect targetRect = GetScreenRect(targetZone, cam);

        // expand target rect by tolerance multiplier (1 = no change)
        float expandX = (targetRect.width * (targetTolerance - 1f)) * 0.5f;
        float expandY = (targetRect.height * (targetTolerance - 1f)) * 0.5f;
        targetRect.xMin -= expandX;
        targetRect.xMax += expandX;
        targetRect.yMin -= expandY;
        targetRect.yMax += expandY;

        bool overlaps = iconRect.Overlaps(targetRect);

        // Debug: print rects so you can tune UI/tolerance quickly
        Debug.LogFormat("KeyholeQTE Debug - IconRect: {0} TargetRect(expanded): {1} Overlaps: {2} (tol:{3})",
            RectToString(iconRect), RectToString(targetRect), overlaps, targetTolerance);

        return overlaps;
    }

    private Rect GetScreenRect(RectTransform rt, Camera cam)
    {
        if (rt == null) return new Rect();

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners); // order: 0 = bottom-left, 2 = top-right

        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

        // convert Unity's screen (0,0 at bottom-left) to consistent Rect (x,y,width,height)
        float x = bottomLeft.x;
        float y = bottomLeft.y;
        float w = Mathf.Max(0.001f, topRight.x - bottomLeft.x);
        float h = Mathf.Max(0.001f, topRight.y - bottomLeft.y);

        return new Rect(x, y, w, h);
    }

    private string RectToString(Rect r)
    {
        return $"(x:{Mathf.RoundToInt(r.x)}, y:{Mathf.RoundToInt(r.y)}, w:{Mathf.RoundToInt(r.width)}, h:{Mathf.RoundToInt(r.height)})";
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