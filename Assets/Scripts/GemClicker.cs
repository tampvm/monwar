using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemClicker : MonoBehaviour
{
	//[SerializeField] private LayerMask gemLayerMask;  // LayerMask để xác định layer của gem 
	[SerializeField] private GameObject highlightGemPrefab; // Prefab gem
	[SerializeField] private float swapDistance;

	private Gem highlightedGem;  // Gem được highlight
	private Gem selectedGem;  // Gem được select    
	public Dictionary<GemType, int> gemsEatenCount;
	public List<Gem> gemsToRemove;
	public bool isSwapping = false;
	private float timing = 0.5f;
	public bool swapDuration = false;

	private void Start()
	{
		gemsEatenCount = new Dictionary<GemType, int>();
		gemsToRemove = new List<Gem>();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && BattleSystem.Instance.CurrentState == BattleState.PlayerAction && !swapDuration)
		{
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity);

			if (hit.collider != null)
			{
				Gem gem = hit.collider.GetComponent<Gem>();
				//Debug.Log(gem.GetPositionInBoard().x + ", " + gem.GetPositionInBoard().y + ", " + gem.GetGemType().ToString());
				if (gem != null)
				{
					ClickGem(gem);
				}
			}
		}
	}

	public void ClickGem(Gem gem)
	{
		if (highlightedGem != null && highlightedGem != gem)
		{
			// Trả về trạng thái bình thường cho viên gem trước đó
			highlightedGem.RemoveHighlight();
		}

		if (highlightedGem == gem)
		{
			// Nếu click lại viên gem đã được click trước đó, trả về trạng thái bình thường
			highlightedGem.RemoveHighlight();
			highlightedGem = null;
			GetGemPosition(null);
		}
		else
		{
			// Xử lý viên gem mới được click
			highlightedGem = gem;
			highlightedGem.SetHighlightPrefab(highlightGemPrefab);

			GetGemPosition(highlightedGem);
		}
		
	}

	public void ClickGemForEnemy(Gem gem)
	{
		if (highlightedGem != null && highlightedGem != gem)
		{
			// Trả về trạng thái bình thường cho viên gem trước đó
			highlightedGem.RemoveHighlight();
		}

		if (highlightedGem == gem)
		{
			// Nếu click lại viên gem đã được click trước đó, trả về trạng thái bình thường
			highlightedGem.RemoveHighlight();
			highlightedGem = null;
			GetGemPosition(null);
		}
		else
		{
			// Xử lý viên gem mới được click
			highlightedGem = gem;
			highlightedGem.SetHighlightPrefab(highlightGemPrefab);

			GetGemPosition(highlightedGem);
		}

	}

	private void GetGemPosition(Gem gem)
	{
		if (selectedGem == null)
		{
			// Lưu tạm thời vị trí của viên gem được chọn
			selectedGem = gem;
		}
		else if (gem == null)
		{
			selectedGem = null;
		}
		else
		{
			bool canSwap = CanSwap(selectedGem, gem);
			//Debug.Log(canSwap);
			if (canSwap)
			{
				swapDuration = true;
				//isPlayerTurn = false;
				StartCoroutine(SwapGemPositions(selectedGem, gem));
				//Hoán đổi vị trí của hai viên gem
				//SwapGemPositions(selectedGem, gem);
			}

			gem.RemoveHighlight();
			// Đặt viên gem được chọn trở lại null
			selectedGem = null;
			//Debug.Log("highlight gem: " + highlightedGem.ToString());
			highlightedGem = null;
		}
	}

	private IEnumerator SwapGemPositions(Gem gem1, Gem gem2)
	{
		// Nếu đang thực hiện swap, không cho phép swap mới
		if (isSwapping)
		{
			yield break;
		}	

		// Lấy vị trí của viên gem hiện tại và viên gem được chọn trước đó
		Vector2 position1 = gem1.GetPositionInBoard();
		Vector2 position2 = gem2.GetPositionInBoard();

		//Debug.Log("Truoc khi doi");
		//Debug.Log(gem1.GetPositionInBoard().x + ", " + gem1.GetPositionInBoard().y + ", " + gem1.GetGemSprite().name);
		//Debug.Log(gem2.GetPositionInBoard().x + ", " + gem2.GetPositionInBoard().y + ", " + gem2.GetGemSprite().name);

		// Chờ 2 giây
		yield return new WaitForSeconds(timing);

		// Cập nhật vị trí mới cho hai viên gem
		gem1.SetPosition(position2.x, position2.y);
		gem2.SetPosition(position1.x, position1.y);

		// Chờ 2 giây
		yield return new WaitForSeconds(timing);

		//Debug.Log("Sau khi doi");
		//Debug.Log(gem1.GetPositionInBoard().x + ", " + gem1.GetPositionInBoard().y + ", " + gem1.GetGemSprite().name);
		//Debug.Log(gem2.GetPositionInBoard().x + ", " + gem2.GetPositionInBoard().y + ", " + gem2.GetGemSprite().name);

		if (!CheckMatch3(1.0f, 8, 8))
		{
			gem1.SetPosition(position1.x, position1.y);
			gem2.SetPosition(position2.x, position2.y);
			isSwapping = false;
			swapDuration = false;
		}
		else isSwapping = true; /*isPlayerTurn = false;*/
		
	}

	private bool CanSwap(Gem gem1, Gem gem2)
	{
		// Lấy tọa độ x, y của hai viên gem
		float x1 = gem1.GetPositionInBoard().x;
		float y1 = gem1.GetPositionInBoard().y;
		float x2 = gem2.GetPositionInBoard().x;
		float y2 = gem2.GetPositionInBoard().y;

		//Debug.Log($"[{x1}, {y1}] & [{x2}, {y2}]");
		
		float epsilon = 0.0001f;
		// Kiểm tra xem hai viên gem có cạnh nhau và nằm trong khoảng cách cho phép hay không
		float x = Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);

		//Debug.Log(x);

		bool isAdjacent = x <= swapDistance + epsilon;
		return isAdjacent;
	}

	private bool CheckMatch3(float gemSpacing, int boardWidth, int boardHeight)
	{
		//Transform gemContainer = transform; // Lấy transform của game object cha
		//Gem[] gems = gemContainer.GetComponentsInChildren<Gem>(); // Lấy các thành phần Gem trong game object cha
		bool check = false;
		Gem[] gems = GetComponentsInChildren<Gem>();

		//List<Gem> gemsToRemove = new List<Gem>();

		// Kiểm tra hàng
		for (int y = 0; y < boardHeight; y++)
		{
			for (int x = 0; x < boardWidth - 2; x++)
			{
				Gem gem1 = GetGemAtPosition(gems, x, y, gemSpacing, boardWidth, boardHeight);
				Gem gem2 = GetGemAtPosition(gems, x + 1, y, gemSpacing, boardWidth, boardHeight);
				Gem gem3 = GetGemAtPosition(gems, x + 2, y, gemSpacing, boardWidth, boardHeight);
				//Debug.Log(gem1.GetPositionInBoard().x + " " + gem1.GetPositionInBoard().y + gem1.GetGemSprite().name);
				if (gem1 != null && gem2 != null && gem3 != null)
				{
					if (gem1.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite())
					{
						// Tăng số lượng gem đã ăn cho loại gem tương ứng
						
						// Check vị trí của gem rồi mới add vô dmm

						// foreach tung thang trong list, roi check no da duoc add vo list chua
						// chua thi add no vo gems to remove voi gem count
						if (!gemsToRemove.Contains(gem1))
						{
							IncreaseGemCount(gem1.GetGemType(), gemsEatenCount);
							gemsToRemove.Add(gem1);
						}
						if (!gemsToRemove.Contains(gem2))
						{
							IncreaseGemCount(gem2.GetGemType(), gemsEatenCount);
							gemsToRemove.Add(gem2);
						}
						if (!gemsToRemove.Contains(gem3))
						{
							IncreaseGemCount(gem3.GetGemType(), gemsEatenCount);
							gemsToRemove.Add(gem3);
						}


						// Các gem match 3 liên tiếp
						// Thực hiện xử lý tương ứng
						//Debug.Log("True");


						check = true;
					}
				}
			}
		}

		// Kiểm tra cột
		for (int x = 0; x < boardWidth; x++)
		{
			for (int y = 0; y < boardHeight - 2; y++)
			{
				Gem gem1 = GetGemAtPosition(gems, x, y, gemSpacing, boardWidth, boardHeight);
				Gem gem2 = GetGemAtPosition(gems, x, y + 1, gemSpacing, boardWidth, boardHeight);
				Gem gem3 = GetGemAtPosition(gems, x, y + 2, gemSpacing, boardWidth, boardHeight);
				//Debug.Log(gem1.GetPositionInBoard().x + " " + gem1.GetPositionInBoard().y + gem1.GetGemSprite().name);
				if (gem1 != null && gem2 != null && gem3 != null)
				{
					//Debug.Log("True");
					if (gem1.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite())
					{
						// Các gem match 3 liên tiếp
						// Thực hiện xử lý tương ứng

						// Tăng số lượng gem đã ăn cho loại gem tương ứng
						if (!gemsToRemove.Contains(gem1))
						{
							IncreaseGemCount(gem1.GetGemType(), gemsEatenCount);
							gemsToRemove.Add(gem1);
						}
						if (!gemsToRemove.Contains(gem2))
						{
							IncreaseGemCount(gem2.GetGemType(), gemsEatenCount);
							gemsToRemove.Add(gem2);
						}
						if (!gemsToRemove.Contains(gem3))
						{
							IncreaseGemCount(gem3.GetGemType(), gemsEatenCount);
							gemsToRemove.Add(gem3);
						}

						//Debug.Log("True");
						check = true;
					}
				}
			}
		}

		// Xóa các gem trong danh sách cần xóa
		foreach (Gem gem in gemsToRemove)
		{
			//RemoveGem(gem);
			StartCoroutine(RemoveGem(gem));
		}
		
		return check;
	}

	public void IncreaseGemCount(GemType gemType, Dictionary<GemType, int> gemsEatenCount)
	{
		if (gemsEatenCount.ContainsKey(gemType))
		{
			gemsEatenCount[gemType]++;
			//Debug.Log(gemsEatenCount[gemType]);
		}
		else
		{
			gemsEatenCount[gemType] = 1;
		}

		//Debug.Log(gemsEatenCount.Keys.ToString() + ": " + gemsEatenCount.Values.ToString());
	}

	public Dictionary<GemType, int> GetGemsEatenCount()
	{
		return gemsEatenCount;
	}

	public IEnumerator RemoveGem(Gem gem)
	{
		// Chờ 2 giây trước khi xóa gem
		yield return new WaitForSeconds(timing);

		if (gem != null)
		{
			// Xóa gem khỏi ma trận gems
			// ...

			// Xóa gem từ game object cha
			Destroy(gem.gameObject);
		}

		// Chờ 2 giây trước khi xóa gem
		yield return new WaitForSeconds(timing);
	}

	public Gem GetGemAtPosition(Gem[] gems, int x, int y, float gemSpacing, float boardWidth, float boardHeight)
	{
		foreach (Gem gem in gems)
		{
			Vector2 gemPosition = gem.transform.position;
			int gemX = Mathf.RoundToInt((gemPosition.x + boardWidth / 2 - gemSpacing / 2) / gemSpacing);
			int gemY = Mathf.RoundToInt((gemPosition.y + boardHeight / 2 - gemSpacing / 2) / gemSpacing);

			if (Mathf.Approximately(gemX, x) && Mathf.Approximately(gemY, y))
			{
				return gem;
			}
			//Debug.Log(gemPosition.x + " " + gemPosition.y);
		}

		return null;

		//Vector2 gemPosition = gems[y * (int)boardWidth + x].transform.localPosition; // Lấy vị trí cục bộ của gem
		//int gemX = Mathf.RoundToInt((gemPosition.x + boardWidth / 2 - gemSpacing / 2) / gemSpacing);
		//int gemY = Mathf.RoundToInt((gemPosition.y + boardHeight / 2 - gemSpacing / 2) / gemSpacing);

		//if (gemX == x && gemY == y)
		//{
		//	return gems[y * (int)boardWidth + x];
		//}

		//return null;
	}

}
