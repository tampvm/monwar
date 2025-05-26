using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusHud : MonoBehaviour
{
    [SerializeField] private float countdownTime = 20f;
    [SerializeField] private Text timerText;
    [SerializeField] private GameObject PlayerTurn1;
    [SerializeField] private GameObject PlayerTurn2;
    [SerializeField] private GameObject EnemyTurn1;
    [SerializeField] private GameObject EnemyTurn2;
    [SerializeField] private Text textTurn;
    [SerializeField] private RectTransform textTurnTransform; // Gắn RectTransform của text
    [SerializeField] private float slideDuration = 1f;
    [SerializeField] private float stayDuration = 1f;
    [SerializeField] private Vector2 offscreenLeft;
    [SerializeField] private Vector2 centerScreen;
    [SerializeField] private Vector2 offscreenRight;

    private float currentCountdown;
    private bool isCounting = false;

    private void Update()
    {
        if (!isCounting) return;

        if (currentCountdown > 0)
        {
            currentCountdown -= Time.deltaTime;
            if (currentCountdown <= 0)
            {
                currentCountdown = 0;
                isCounting = false;
            }

            timerText.text = Mathf.FloorToInt(currentCountdown).ToString(); // Format đơn giản
        }
    }

    public void StartCountdown()
    {
        currentCountdown = countdownTime;
        isCounting = true;
    }

    public void StopCountdown()
    {
        isCounting = false;
    }

    public bool IsTimeout()
    {
        return currentCountdown <= 0;
    }

    public void ShowTurnText(string message, bool isPlayer)
    {
        //textTurn.text = message;
        //StartCoroutine(AnimateTurnText(isPlayer));

        StartCoroutine(AnimateTurnTextAndStartCountdown(message, isPlayer));
    }

    private IEnumerator AnimateTurnText(bool isPlayer)
    {
        textTurnTransform.gameObject.SetActive(true); // Hiện text trước khi bắt đầu hoạt ảnh

        Vector2 start = isPlayer ? offscreenLeft : offscreenRight;
        Vector2 end = centerScreen;

        textTurnTransform.anchoredPosition = start;
        textTurn.gameObject.SetActive(true);

        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, t / slideDuration);
            textTurnTransform.anchoredPosition = Vector2.Lerp(start, end, progress);
            yield return null;
        }

        // Giữ ở giữa một lúc
        yield return new WaitForSeconds(stayDuration);

        // Trượt ra ngoài theo hướng ngược lại
        //t = 0f;
        //Vector2 exit = isPlayer ? offscreenRight : offscreenLeft;
        //while (t < slideDuration)
        //{
        //    t += Time.deltaTime;
        //    float progress = Mathf.SmoothStep(0, 1, t / slideDuration);
        //    textTurnTransform.anchoredPosition = Vector2.Lerp(end, exit, progress);
        //    yield return null;
        //}

        //textTurn.gameObject.SetActive(false);
        textTurnTransform.gameObject.SetActive(false); // Ẩn text sau khi hoàn thành
    }

    private IEnumerator AnimateTurnTextAndStartCountdown(string message, bool isPlayer)
    {
        StartCountdown(); // Bắt đầu đếm sau khi hoàn tất hiệu ứng
        textTurn.text = message;
        yield return StartCoroutine(AnimateTurnText(isPlayer)); // Chờ hiệu ứng hoàn thành
    }

    public void SetTurnUI(BattleState state)
    {
        // Reset all
        PlayerTurn1.SetActive(false);
        PlayerTurn2.SetActive(false);
        EnemyTurn1.SetActive(false);
        EnemyTurn2.SetActive(false);

        // Xử lý hiển thị HUD
        bool isPlayer = state == BattleState.PlayerAction || state == BattleState.PlayerMove;
        if (isPlayer)
        {
            PlayerTurn1.SetActive(true);
            PlayerTurn2.SetActive(true);
        }
        else
        {
            EnemyTurn1.SetActive(true);
            EnemyTurn2.SetActive(true);
        }

        // Chỉ hiển thị textTurn khi bắt đầu lượt (Action)
        if (state == BattleState.PlayerAction)
            ShowTurnText("Đến lượt bạn!", true);
        else if (state == BattleState.EnemyAction)
            ShowTurnText("Đến lượt đối thủ!", false);
    }
}
