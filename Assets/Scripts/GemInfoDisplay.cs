using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemInfoDisplay : MonoBehaviour
{
    public Image iconImage;
    public Text gemCountText;

    public string GemType { get; private set; } // Loại gem, có thể là "red", "blue", vv.

    public void Setup(CollectedGemInfo info)
    {
        iconImage.sprite = info.Icon;
        gemCountText.text = "x" + info.Count.ToString();
        GemType = info.GemType; // Lưu loại gem để có thể xoá sau này
    }

    /// ✨ Gọi hàm này để trượt xuống và biến mất
    public void PlayRemoveAnimation(float slideDistance = 50f, float duration = 2f)
    {
        StartCoroutine(SlideAndFadeOut(slideDistance, duration));
    }

    private IEnumerator SlideAndFadeOut(float distance, float duration)
    {
        RectTransform rect = GetComponent<RectTransform>();
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        Vector3 startPos = rect.anchoredPosition;
        Vector3 endPos = startPos - new Vector3(0, distance, 0);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            rect.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
            canvasGroup.alpha = 1f - t;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = endPos;
        canvasGroup.alpha = 0f;

        Destroy(gameObject);
    }
}
