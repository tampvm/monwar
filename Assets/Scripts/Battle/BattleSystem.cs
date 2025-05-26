using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BattleState {
	PlayerAction,
	PlayerMove,
	EnemyAction,
	EnemyMove,
	BattleOver
}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
	[SerializeField] BattleUnit enemyUnit;
	[SerializeField] BattleHud playerHud;
	[SerializeField] BattleHud enemyHud;
	[SerializeField] BoardManager board;
	[SerializeField] BattleBox battleBox;
	[SerializeField] StatusHud statusHud;
    //[SerializeField] GameObject gemCountPanel;
    [SerializeField] GemSummaryPanel gemSummaryPanel; // Giao diện hiển thị số lượng gem
    [SerializeField] GameObject boardUI; // Giao diện chứa các ô gem

    

    public event Action<bool> OnBattleOver;
	public bool isWin;

	public static BattleSystem Instance { get; private set; }
	public BattleState CurrentState => state;
    BattleState state;

    // Thêm biến này để kiểm tra xem trận chiến đã bắt đầu hay chưa
    private bool isBattleStarted = false;

    private void Awake()
    {
        Instance = this;
    }

	public void StartGame()
	{
		SetUpBattle();
		StartCoroutine(StartBattle()); // Bắt đầu trò chơi
        isBattleStarted = true;
    }

	// Set up Battle
	public void SetUpBattle()
	{
		//board.PlayerTurn();
		board.Setup();
		battleBox.Setup();
		playerUnit.Setup();
		enemyUnit.Setup();
		playerHud.SetData(playerUnit.Pokemon);
		enemyHud.SetData(enemyUnit.Pokemon);
		// Sau này làm nhớ set skill của pet ở đây
	}

	public IEnumerator StartBattle()
	{
		// check first turn
		state = BattleState.PlayerAction;

		//Debug.Log("Chờ 0.5 giây trước khi start");
		yield return new WaitForSeconds(0.5f);

		while (state != BattleState.BattleOver)
		{
			yield return StartCoroutine(ProcessTurn());
		}

		yield return StartCoroutine(CheckBattleOver());

        isBattleStarted = false; // Đặt lại trạng thái battle đã bắt đầu
    }


	// Manage turn
	private IEnumerator ProcessTurn()
	{
        statusHud.SetTurnUI(state); // Cập nhật UI trạng thái lượt chơi

        switch (state)
		{
			case BattleState.PlayerAction:
				yield return StartCoroutine(PlayerAction());
				break;

			case BattleState.PlayerMove:
				yield return StartCoroutine(PerformPlayerMove());
				break;

			case BattleState.EnemyAction:
				yield return StartCoroutine(EnemyAction());
				break;

			case BattleState.EnemyMove:
				yield return StartCoroutine(PerformEnemyMove());
				break;

			//case BattleState.BattleOver:
			//	yield return StartCoroutine(CheckBattleOver());				
			//	break;
		}

		//if (CheckBattleOver())
		//{
		//	state = BattleState.BattleOver;
		//}
	}

	// Player Action
	private IEnumerator PlayerAction()
	{
		// Display player actions/UI
		// ...

		Debug.Log("Player's turn");

		statusHud.StartCountdown(); // Bắt đầu đếm ngược thời gian 20s

        //Debug.Log("Wait for player's action");
		
		// Wait for player to select an action
		while (state == BattleState.PlayerAction)
		{
			yield return new WaitForSeconds(1f);

			//board.PlayerTurn();

			// Kiểm tra xem người chơi đã chọn hành động hay chưa
			if (board.EndTurn() || battleBox.EndTurn())
			{
				//StartCoroutine(board.GetGemTypeInDic());
				
				statusHud.StopCountdown(); // Dừng đếm ngược khi người chơi đã chọn hành động
                yield return new WaitForSeconds(1f);
				state = BattleState.PlayerMove;

                //// Chuyển sang lượt di chuyển của người chơi
                //state = BattleState.PlayerMove;
            }
            else if (statusHud.IsTimeout())
            {
                //Debug.Log("Thời gian đã hết, tự động kết thúc lượt");
				//state = BattleState.EnemyAction;
                state = BattleState.PlayerMove;
            }

            yield return null;
		}
	}

	private IEnumerator PerformPlayerMove()
	{
		bool isFainted = false;
		int point = 0;

        if (battleBox.isUseCard)
		{
			battleBox.isUseCard = false;
            isFainted = enemyUnit.Pokemon.TakeDamageWithSkill(playerUnit.Pokemon);
            yield return playerHud.UpdateCurrentMPIfDecrease();
		}
		else
		{
            var collectedGems = board.GetCollectedGemInfoList(); // Lấy thông tin gem

            if (collectedGems != null && collectedGems.Count > 0)
			{
                // 1. Ẩn board sau khi người chơi đã chọn gem
                boardUI.SetActive(false);

                // 2. Hiển thị GemCountPanel với thông tin gem vừa ăn
                gemSummaryPanel.gameObject.SetActive(true);
                gemSummaryPanel.Display(collectedGems);

                // 3. Xử lý gem
                var gemAtkCount = board.GetAtkGemCount();
                var gemHealthCount = board.GetHealthGemCount();
                var gemManaCount = board.GetManaGemCount();
                var gemPowerCount = board.GetPowerGemCount();
                var gemShieldCount = board.GetShieldGemCount();

                board.ClearGemCount();
                yield return new WaitForSeconds(0.5f);

                float effectDuration = 2f;

                if (gemHealthCount > 0)
                {
                    playerUnit.Pokemon.HealPoint(gemHealthCount, playerUnit.Pokemon, out point);
					playerUnit.PlayEffect(playerUnit.healEffectPrefab, effectDuration); // Gọi hiệu ứng hồi máu
					playerUnit.ShowStatPoint(point, playerUnit.healColor, effectDuration);
					gemSummaryPanel.RemoveGemDisplay("Hp", 1); // Xoá gem hồi máu khỏi panel sau khi đã xử lý
                    yield return playerHud.UpdateCurrentHPIfIncrease(effectDuration); // Cập nhật lại HP bar
                }

                if (gemShieldCount > 0)
                {
                    playerUnit.Pokemon.ShieldPoint(gemShieldCount, playerUnit.Pokemon, out point);
                    playerUnit.PlayEffect(playerUnit.shieldEffectObject, effectDuration); // Gọi hiệu ứng hồi giáp
                    playerUnit.ShowStatPoint(point, playerUnit.shieldColor, effectDuration);
                    gemSummaryPanel.RemoveGemDisplay("Shield", 1); // Xoá gem hồi giáp khỏi panel sau khi đã xử lý
                    yield return playerHud.UpdateCurrentShieldIfIncrease(effectDuration);
                }

                if (gemManaCount > 0)
                {
                    playerUnit.Pokemon.ManaPoint(gemManaCount, playerUnit.Pokemon, out point);
					playerUnit.PlayEffect(playerUnit.manaEffectPrefab, effectDuration); // Gọi hiệu ứng hồi mana
                    playerUnit.ShowStatPoint(point, playerUnit.manaColor, effectDuration);
                    gemSummaryPanel.RemoveGemDisplay("Mp", 1); // Xoá gem hồi mana khỏi panel sau khi đã xử lý
                    yield return playerHud.UpdateCurrentMPIfIncrease(effectDuration);
                }

                if (gemPowerCount > 0)
                {
                    playerUnit.Pokemon.PowerPoint(gemPowerCount, playerUnit.Pokemon, out point);
                    playerUnit.PlayEffect(playerUnit.powerEffectPrefab, effectDuration); // Gọi hiệu ứng hồi power
                    playerUnit.ShowStatPoint(point, playerUnit.powerColor, effectDuration);
                    gemSummaryPanel.RemoveGemDisplay("Pp", 1); // Xoá gem hồi power khỏi panel sau khi đã xử lý
                    yield return playerHud.UpdateCurrentPPIfIncrease(effectDuration);
                }

                if (gemAtkCount > 0)
                {
                    //isFainted = enemyUnit.Pokemon.TakeDamage(gemAtkCount, playerUnit.Pokemon);
					int damage = enemyUnit.Pokemon.TakeDamage(gemAtkCount, playerUnit.Pokemon, out isFainted);
                    enemyUnit.ShowDamageEffect(damage); // Hiện hiệu ứng damage
                }
            }
        }

		//yield return new WaitForSeconds(0.5f);

		yield return playerHud.UpdateCurrentPPIfDecrease();

        gemSummaryPanel.RemoveGemDisplay("Atk", 1); // Xoá gem tấn công khỏi panel sau khi đã xử lý

		yield return enemyHud.UpdateCurrentShieldIfDecrease();

		yield return enemyHud.UpdateCurrentHPIfDecrease();


        //Debug.Log("Enemy's Hp: " + enemyUnit.Pokemon.CurrentHp);

        // 4. Tắt panel sau khi xử lý xong
        yield return new WaitForSeconds(0.5f);
        gemSummaryPanel.gameObject.SetActive(false);
        boardUI.SetActive(true); // Hiện lại giao diện board sau khi người chơi đã chọn gem

        if (isFainted)
		{
			yield return new WaitForSeconds(2f);

			Debug.Log("Enemy fainted");

			//if (GameController.Instance.gameMode == GameMode.Tutorial)
			//{
			//	yield return StartCoroutine(WatchVideo.Instance.PlayVideo());
			//	GameController.Instance.gameMode = GameMode.Play;
			//}

			isWin = true;

			state = BattleState.BattleOver;
		}
		else
		{
			Debug.Log("end Player's turn");

            //Debug.Log("Chờ 1 giây trước khi sang lượt của ennemy");
			yield return new WaitForSeconds(1f);
			state = BattleState.EnemyAction;
		}
		
		yield return null;
	}

	// Enemy Action
	private IEnumerator EnemyAction()
	{
		Debug.Log("Enemy's turn");

		statusHud.StartCountdown(); // Bắt đầu đếm ngược thời gian 20s

        //Debug.Log("Enemy đang hàng động");

        yield return new WaitForSeconds(2f);

        // Wait for player to select an action
        while (state == BattleState.EnemyAction)
		{		
			yield return new WaitForSeconds(1f);

			if (enemyUnit.Pokemon.CurrentMp >= 200)
			{
				battleBox.UseSkillCard();
			}
			else
			{
				if (board.CheckSwapGemWithGemAroundByType(GemType.Atk))
				{
					board.EnemyAction();
				}
				else if (enemyUnit.Pokemon.CurrentHp < enemyUnit.Pokemon.Heal && board.CheckSwapGemWithGemAroundByType(GemType.Hp) && !board.CheckSwapGemWithGemAroundByType(GemType.Atk))
				{
					board.EnemyAction();
				}
				else if (!board.CheckSwapGemWithGemAroundByType(GemType.Hp) && !board.CheckSwapGemWithGemAroundByType(GemType.Atk) && board.CheckSwapGemWithGemAroundByType(GemType.Mp))
				{
					board.EnemyAction();
				}
				else if (!board.CheckSwapGemWithGemAroundByType(GemType.Hp) && !board.CheckSwapGemWithGemAroundByType(GemType.Atk) && !board.CheckSwapGemWithGemAroundByType(GemType.Mp) && board.CheckSwapGemWithGemAroundByType(GemType.Shield))
				{
					board.EnemyAction();
				}
				else if (!board.CheckSwapGemWithGemAroundByType(GemType.Hp) && !board.CheckSwapGemWithGemAroundByType(GemType.Atk) && !board.CheckSwapGemWithGemAroundByType(GemType.Mp) && !board.CheckSwapGemWithGemAroundByType(GemType.Shield) && board.CheckSwapGemWithGemAroundByType(GemType.Pp))
				{
					board.EnemyAction();
				}
				
				//board.EnemyAction();
			}

			// Kiểm tra xem enemy đã chọn hành động hay chưa
			if (board.EndTurn() || battleBox.EndTurn())
			{
				statusHud.StopCountdown(); // Dừng đếm ngược khi người chơi đã chọn hành động
                yield return new WaitForSeconds(1f);			
				state = BattleState.EnemyMove;
			}
            else if (statusHud.IsTimeout())
            {
                //Debug.Log("Thời gian đã hết, tự động kết thúc lượt");
                state = BattleState.EnemyMove;
            }

            yield return null;
		}
	}

	private IEnumerator PerformEnemyMove()
	{
		bool isFainted = false;
		int point = 0;

		if (battleBox.isUseCard)
		{
			battleBox.isUseCard = false;
            isFainted = playerUnit.Pokemon.TakeDamageWithSkill(enemyUnit.Pokemon);
            yield return enemyHud.UpdateCurrentMPIfDecrease();
		}
		else
		{
            var collectedGems = board.GetCollectedGemInfoList(); // Lấy thông tin gem

            if (collectedGems != null && collectedGems.Count > 0)
			{
                // 1. Ẩn board sau khi người chơi đã chọn gem
                boardUI.SetActive(false);

                // 2. Hiển thị GemCountPanel với thông tin gem vừa ăn
                gemSummaryPanel.gameObject.SetActive(true);
                gemSummaryPanel.Display(collectedGems);

                // 3. Xử lý gem
                var gemAtkCount = board.GetAtkGemCount();
                var gemHealthCount = board.GetHealthGemCount();
                var gemManaCount = board.GetManaGemCount();
                var gemPowerCount = board.GetPowerGemCount();
                var gemShieldCount = board.GetShieldGemCount();

                board.ClearGemCount();
                yield return new WaitForSeconds(0.5f);

				float effectDuration = 2f;

                if (gemHealthCount > 0)
                {
                    enemyUnit.Pokemon.HealPoint(gemHealthCount, enemyUnit.Pokemon, out point);
					enemyUnit.PlayEffect(enemyUnit.healEffectPrefab, effectDuration); // Gọi hiệu ứng hồi máu
                    enemyUnit.ShowStatPoint(point, enemyUnit.healColor, effectDuration);
                    gemSummaryPanel.RemoveGemDisplay("Hp", 1); // Xoá gem hồi máu khỏi panel sau khi đã xử lý
                    yield return enemyHud.UpdateCurrentHPIfIncrease(effectDuration);
                }

                if (gemShieldCount > 0)
                {
                    enemyUnit.Pokemon.ShieldPoint(gemShieldCount, enemyUnit.Pokemon, out point);
                    enemyUnit.PlayEffect(enemyUnit.shieldEffectObject, effectDuration); // Gọi hiệu ứng hồi giáp
                    enemyUnit.ShowStatPoint(point, enemyUnit.shieldColor, effectDuration);
                    gemSummaryPanel.RemoveGemDisplay("Shield", 1); // Xoá gem hồi giáp khỏi panel sau khi đã xử lý
                    yield return enemyHud.UpdateCurrentShieldIfIncrease(effectDuration);
                }

                if (gemManaCount > 0)
                {
                    enemyUnit.Pokemon.ManaPoint(gemManaCount, enemyUnit.Pokemon, out point);
					enemyUnit.PlayEffect(enemyUnit.manaEffectPrefab, effectDuration); // Gọi hiệu ứng hồi mana
                    enemyUnit.ShowStatPoint(point, enemyUnit.manaColor, effectDuration);
                    gemSummaryPanel.RemoveGemDisplay("Mp", 1); // Xoá gem hồi mana khỏi panel sau khi đã xử lý
                    yield return enemyHud.UpdateCurrentMPIfIncrease(effectDuration);
                }

                if (gemPowerCount > 0)
                {
                    enemyUnit.Pokemon.PowerPoint(gemPowerCount, enemyUnit.Pokemon, out point);
                    enemyUnit.PlayEffect(enemyUnit.powerEffectPrefab, effectDuration); // Gọi hiệu ứng hồi power
                    enemyUnit.ShowStatPoint(point, enemyUnit.powerColor, effectDuration);
                    gemSummaryPanel.RemoveGemDisplay("Pp",1); // Xoá gem hồi power khỏi panel sau khi đã xử lý
                    yield return enemyHud.UpdateCurrentPPIfIncrease(effectDuration);
                }

                if (gemAtkCount > 0)
                {
                    //isFainted = playerUnit.Pokemon.TakeDamage(gemAtkCount, enemyUnit.Pokemon);
                    int damage = playerUnit.Pokemon.TakeDamage(gemAtkCount, enemyUnit.Pokemon, out isFainted);
                    playerUnit.ShowDamageEffect(damage); // Hiện hiệu ứng damage
                }
            }
        }

		//yield return new WaitForSeconds(0.5f);

		yield return enemyHud.UpdateCurrentPPIfDecrease();

        gemSummaryPanel.RemoveGemDisplay("Atk", 1); // Xoá gem tấn công khỏi panel sau khi đã xử lý

		yield return playerHud.UpdateCurrentShieldIfDecrease();

		yield return playerHud.UpdateCurrentHPIfDecrease();

        //Debug.Log("Enemy's Hp: " + playerUnit.Pokemon.CurrentHp);

        // 4. Tắt panel sau khi xử lý xong
        yield return new WaitForSeconds(0.5f);
        gemSummaryPanel.gameObject.SetActive(false);
        boardUI.SetActive(true); // Hiện lại giao diện board sau khi người chơi đã chọn gem

        if (isFainted)
		{
			yield return new WaitForSeconds(2f);

			Debug.Log("Player fainted");

			isWin = false;

			state = BattleState.BattleOver;
		}
		else
		{
			Debug.Log("end Enenmy's turn");

            //Debug.Log("Chờ 1 giây trước khi sang lượt của Player");
			yield return new WaitForSeconds(1f);
			//board.PlayerTurn();
			board.SwapDuration();

			state = BattleState.PlayerAction;
		}

		yield return null;
	}



	// Check End Battle
	private IEnumerator CheckBattleOver()
	{
		board.EndGame();
        Debug.Log("End Game");

		yield return null;
		//yield return new WaitForSeconds(2f);

		OnBattleOver(true);		
	}


	private void Update()
	{
        // ⚠️ Nếu battle chưa bắt đầu thì bỏ qua
        if (!isBattleStarted || playerUnit.Pokemon == null) return;

        bool isActive;

		if (playerUnit.Pokemon.CurrentMp >= 200)
		{
			battleBox.HideCard();
			isActive = battleBox.ShowCardHighlight();
		}
		else
		{
			battleBox.ShowCard();
			isActive = battleBox.HideCardHighlight();
		}

		if (Input.GetMouseButtonDown(0) && isActive && state == BattleState.PlayerAction)
		{
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity);

			if (hit.collider != null)
			{
				Card card = hit.collider.GetComponent<Card>();
				if (card != null)
				{
					Debug.Log("use card");
					battleBox.UseSkillCard();
				}
			}
		}
	}

    //private void PlayEffect(GameObject effectPrefab, Transform spawnPoint, float duration)
    //{
    //    //if (effectPrefab != null && spawnPoint != null)
    //    //{
    //    //    GameObject effect = Instantiate(effectPrefab, spawnPoint.position, Quaternion.identity);
    //    //    effect.transform.SetParent(spawnPoint); // gắn vào player để hiệu ứng di chuyển cùng nhân vật
    //    //    Destroy(effect, duration); // huỷ sau 2s, có thể chỉnh theo thời gian hiệu ứng
    //    //}

    //    if (effectPrefab != null && spawnPoint != null)
    //    {
    //        GameObject effect = Instantiate(effectPrefab, spawnPoint.position, Quaternion.identity);
    //        effect.transform.SetParent(spawnPoint, worldPositionStays: true);

    //        // Reset transform nếu cần
    //        effect.transform.localPosition = Vector3.zero;
    //        effect.transform.localRotation = Quaternion.identity;
    //        effect.transform.localScale = Vector3.one;

    //        // Nếu là UI, đảm bảo sorting order đủ cao
    //        var canvas = effect.GetComponent<Canvas>();
    //        if (canvas != null)
    //        {
    //            canvas.overrideSorting = true;
    //            canvas.sortingOrder = 100;
    //        }

    //        Destroy(effect, duration);
    //    }
    //}
}
