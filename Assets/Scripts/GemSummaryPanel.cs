using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemSummaryPanel : MonoBehaviour
{
    public GameObject gemInfoPrefab;
    public Transform contentParent;  // Panel chứa các dòng gem

    public void Display(List<CollectedGemInfo> gemInfos)
    {
        // Xoá cũ
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Thêm mới
        foreach (var info in gemInfos)
        {
            var go = Instantiate(gemInfoPrefab, contentParent);
            var display = go.GetComponent<GemInfoDisplay>();
            display.Setup(info);
        }
    }

    public void RemoveGemDisplay(string gemType, float duration = 2f)
    {
        foreach (Transform child in contentParent)
        {
            var display = child.GetComponent<GemInfoDisplay>();
            if (display != null && display.GemType == gemType)
            {
                //Destroy(child.gameObject);

                if (!child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true); // Cho phép chạy Coroutine
                }

                display.PlayRemoveAnimation(duration: duration); // Gọi hàm để trượt xuống và biến mất
                break;
            }
        }
    }
}
