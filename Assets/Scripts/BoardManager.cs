using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum Case { one, two, three, four }

public class CollectedGemInfo
{
    public string GemType;
    public int Count;
    public Sprite Icon;
}

public class BoardManager : MonoBehaviour
{
	[SerializeField] private int width; // Số lượng cột trong bảng kim cương
	[SerializeField] private int height; // Số lượng hàng trong bảng kim cương
	[SerializeField] private float gemSpacing; // Khoảng cách mong muốn giữa các viên kim cương
	[SerializeField] private GameObject gemPrefab; // Prefab "Gem" để tạo kim cương
	[SerializeField] private Sprite[] gemSprites; // Mảng sprite của các loại kim cương

	private Gem[,] gems; // Mảng 2D lưu trữ các viên kim cương
	private GemClicker gemClicker;
	private bool isChanging; // Check if the gem in the board has changed

	private List<Tuple<Gem, int, Gem>> gemList;

	private bool isInitialized = false; // Check if the board has been initialized

    public void Setup()
	{
		ClearBoard();
		InitializeBoard();
		gemList = new List<Tuple<Gem, int, Gem>>();
		gemClicker = FindObjectOfType<GemClicker>();
		gemClicker.swapDuration = false;
        isInitialized = true; // Đánh dấu rằng bảng đã được khởi tạo
    }

    public void EndGame()
    {
        isInitialized = false;
        // thêm logic khác như hiển thị UI end game
    }

    private void Update()
	{
		if (!isInitialized) return; // Nếu bảng chưa được khởi tạo, không thực hiện gì cả

        //CheckMatch3Gem();
        StartCoroutine(CheckMatch3Gem());
		if (HasEmptySpaces())
		{
			FillEmptySpaces();
			//UpdateGemInBoard();
			StartCoroutine(UpdateGemInBoard());
		}
	}


	// this script for enemy click and swap gem in board | cần dời cái đóng củ l này sang enemy action

	private Gem GetGemAround(Gem gem)
	{
		Vector2 gemPosition = gem.GetPositionInBoard();

		// Kiểm tra các vị trí xung quanh gem
		// Xem ví dụ bên dưới để lấy gem ngẫu nhiên từ các vị trí xung quanh

		// Tạo danh sách các vị trí xung quanh gem
		List<Vector2> adjacentPositions = new List<Vector2>
		{
			new Vector2((float)(gemPosition.x - 0.125), gemPosition.y), // Vị trí bên trái
			new Vector2((float)(gemPosition.x + 0.125), gemPosition.y), // Vị trí bên phải
			new Vector2(gemPosition.x, (float)(gemPosition.y - 0.125)), // Vị trí phía dưới
			new Vector2(gemPosition.x, (float)(gemPosition.y + 0.125))  // Vị trí phía trên
		};

		// Lọc ra các vị trí hợp lệ trong biên của bảng
		List<Vector2> validPositions = new List<Vector2>();
		foreach (Vector2 position in adjacentPositions)
		{
			if (IsValidPosition(position))
			{
				//Debug.Log(position.x + " " + position.y);
				validPositions.Add(position);
			}
		}

		// Kiểm tra xem có vị trí hợp lệ hay không
		if (validPositions.Count == 0)
		{
			return null; // Không có gem xung quanh hợp lệ
		}

		// Chọn một vị trí ngẫu nhiên từ danh sách vị trí hợp lệ
		int randomIndex = UnityEngine.Random.Range(0, validPositions.Count);
		Vector2 randomPosition = validPositions[randomIndex];

		// Lấy gem tại vị trí ngẫu nhiên được chọn
		Gem gemAround = GetGemAtPosition(randomPosition);

		return gemAround;
	}

	private Gem GetGemAtPosition(Vector2 position)
	{
		// Lấy danh sách tất cả các gem trên board
		Gem[] gems = FindObjectsOfType<Gem>();

		// Duyệt qua danh sách các gem và kiểm tra vị trí của từng gem
		foreach (Gem gem in gems)
		{
			if (gem.GetPositionInBoard() == position)
			{
				return gem; // Trả về gem tại vị trí cụ thể
			}
		}

		return null; // Không tìm thấy gem tại vị trí
	}

	private bool IsValidPosition(Vector2 position)
	{
		Gem[] gems = GetComponentsInChildren<Gem>();
		foreach (Gem gem in gems)
		{
			Vector2 gemPosition = gem.transform.localPosition;
			float gemX = gemPosition.x;
			float gemY = gemPosition.y;

			if (Mathf.Approximately(gemX, position.x) && Mathf.Approximately(gemY, position.y))
			{
				return true;
			}
		}

		return false;
	}

	//public void PlayerTurn()
	//{
	//	gemClicker.isPlayerTurn = true;
	//}

	public void SwapDuration()
	{
		gemClicker.swapDuration = false;
	}

	private Gem GetRandomGem()
	{
		Gem[] gems = GetComponentsInChildren<Gem>();
		return gems[UnityEngine.Random.Range(0, gems.Length)];
	}

	public void EnemyAction()
	{
		if (gemList != null)
		{
			var maxValue = gemList.OrderByDescending(tuple => tuple.Item2).FirstOrDefault();

			gemList.Clear();

			gemClicker.ClickGemForEnemy(maxValue.Item1);
			gemClicker.ClickGemForEnemy(maxValue.Item3);
		}
	}

	public bool CheckSwapGemWithGemAroundByType(GemType gemType)
	{
		bool check = false;

		Gem[] gems = GetComponentsInChildren<Gem>();
		List<Gem> gemList = new List<Gem>();

		foreach (var gem in gems)
		{
			if (gem.GetGemType() == gemType)
			{
				gemList.Add(gem);
			}
		}

		if (gemList.Count > 3)
		{
			foreach (var gem in gemList)
			{
				Vector2 gemPosition = gem.GetPositionInBoard();

				float gemX = gemPosition.x;
				float gemY = gemPosition.y;

				if (CheckMatchGemAbove(gem, gemX, gemY)) check = true;
				if (CheckMatchGemLeft(gem, gemX, gemY)) check = true;
				if (CheckMatchGemRight(gem, gemX, gemY)) check = true;
				if (CheckMatchGemBelow(gem, gemX, gemY)) check = true;
			}
		}

		return check;
	}



	// Check all possible cases if the clicked gem's position is swapped with the gem above it
	// it can have 4 possible occurrences and 6 gem positions
	private bool CheckMatchGemAbove(Gem gem, float gemX, float gemY)
	{
		bool check = false;
		int point;

		// giả sử đặt gem được chọn ở vị trí [0, 0]
		Vector2 position1 = new Vector2((float)(gemX), (float)(gemY + 0.125)); // vị trí của gem [0, 1] so với gem được chọn


		Vector2 position2 = new Vector2((float)(gemX - 0.125), (float)(gemY + 0.125)); // vị trí của gem [-1, 1] so với gem được chọn
		Vector2 position3 = new Vector2((float)(gemX + 0.125), (float)(gemY + 0.125)); // vị trí của gem [1, 1] so với gem được chọn

		Vector2 position4 = new Vector2((float)(gemX), (float)(gemY + 0.125 + 0.125)); // vị trí của gem [0, 2] so với gem được chọn
		Vector2 position5 = new Vector2((float)(gemX), (float)(gemY + 0.125 + 0.125 + 0.125)); // vị trí của gem [0, 3] so với gem được chọn
		
		Vector2 position6 = new Vector2((float)(gemX - 0.125 - 0.125), (float)(gemY + 0.125)); // vị trí của gem [-2, 1] so với gem được chọn
		Vector2 position7 = new Vector2((float)(gemX + 0.125 + 0.125), (float)(gemY + 0.125)); // vị trí của gem [2, 1] so với gem được chọn

		Gem gem1 = GetGemAtPosition(position1);
		Gem gem2 = GetGemAtPosition(position2);
		Gem gem3 = GetGemAtPosition(position3);
		Gem gem4 = GetGemAtPosition(position4);
		Gem gem5 = GetGemAtPosition(position5);
		Gem gem6 = GetGemAtPosition(position6);
		Gem gem7 = GetGemAtPosition(position7);

		// 3 points

		if (gem2 != null && gem3 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem4 != null && gem5 != null)
		{
			if (gem.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem3 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 4 points 

		if (gem2 != null && gem3 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 4;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem3 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 4;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 5 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem2 != null && gem4 != null && gem5 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem3 != null && gem4 != null && gem5 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem3.GetGemSprite() 
				&& gem3.GetGemSprite() == gem4.GetGemSprite() 
				&& gem4.GetGemSprite() == gem5.GetGemSprite() 
				&& gem5.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem2 != null && gem3 != null && gem6 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem6.GetGemSprite() && gem6.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}


		// 6 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 6;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 6;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 7 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem6 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite() && gem6.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 7;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		return check;
	}

	private bool CheckMatchGemLeft(Gem gem, float gemX, float gemY)
	{
		bool check = false;
		int point;

		// giả sử đặt gem được chọn ở vị trí [0, 0]
		Vector2 position1 = new Vector2((float)(gemX - 0.125), (float)(gemY)); // vị trí của gem [-1, 0] so với gem được chọn

		Vector2 position2 = new Vector2((float)(gemX - 0.125), (float)(gemY - 0.125)); // vị trí của gem [-1, -1] so với gem được chọn
		Vector2 position3 = new Vector2((float)(gemX - 0.125), (float)(gemY + 0.125)); // vị trí của gem [-1, 1] so với gem được chọn

		Vector2 position4 = new Vector2((float)(gemX - 0.125 - 0.125), (float)(gemY)); // vị trí của gem [-2, 0] so với gem được chọn
		Vector2 position5 = new Vector2((float)(gemX - 0.125 - 0.125 - 0.125), (float)(gemY)); // vị trí của gem [-3, 0] so với gem được chọn

		Vector2 position6 = new Vector2((float)(gemX - 0.125), (float)(gemY - 0.125 - 0.125)); // vị trí của gem [-1, -2] so với gem được chọn
		Vector2 position7 = new Vector2((float)(gemX - 0.125), (float)(gemY + 0.125 + 0.125)); // vị trí của gem [-1, 2] so với gem được chọn

		Gem gem1 = GetGemAtPosition(position1);
		Gem gem2 = GetGemAtPosition(position2);
		Gem gem3 = GetGemAtPosition(position3);
		Gem gem4 = GetGemAtPosition(position4);
		Gem gem5 = GetGemAtPosition(position5);
		Gem gem6 = GetGemAtPosition(position6);
		Gem gem7 = GetGemAtPosition(position7);

		// 3 points

		if (gem2 != null && gem3 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem4 != null && gem5 != null)
		{
			if (gem.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem3 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 4 points 

		if (gem2 != null && gem3 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 4;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem3 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 4;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 5 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem2 != null && gem4 != null && gem5 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem3 != null && gem4 != null && gem5 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem2 != null && gem3 != null && gem6 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem6.GetGemSprite() && gem6.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}


		// 6 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 6;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 6;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 7 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem6 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite() && gem6.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 7;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		return check;
	}

	private bool CheckMatchGemRight(Gem gem, float gemX, float gemY)
	{
		bool check = false;
		int point;

		// giả sử đặt gem được chọn ở vị trí [0, 0]
		Vector2 position1 = new Vector2((float)(gemX + 0.125), (float)(gemY)); // vị trí của gem [1, 0] so với gem được chọn


		Vector2 position2 = new Vector2((float)(gemX + 0.125), (float)(gemY + 0.125)); // vị trí của gem [1, 1] so với gem được chọn
		Vector2 position3 = new Vector2((float)(gemX + 0.125), (float)(gemY - 0.125)); // vị trí của gem [1, -1] so với gem được chọn

		Vector2 position4 = new Vector2((float)(gemX + 0.125 + 0.125), (float)(gemY)); // vị trí của gem [2, 0] so với gem được chọn
		Vector2 position5 = new Vector2((float)(gemX + 0.125 + 0.125 + 0.125), (float)(gemY)); // vị trí của gem [3, 0] so với gem được chọn

		Vector2 position6 = new Vector2((float)(gemX + 0.125), (float)(gemY + 0.125 + 0.125)); // vị trí của gem [1, 2] so với gem được chọn
		Vector2 position7 = new Vector2((float)(gemX + 0.125), (float)(gemY - 0.125 - 0.125)); // vị trí của gem [1, -2] so với gem được chọn

		Gem gem1 = GetGemAtPosition(position1);
		Gem gem2 = GetGemAtPosition(position2);
		Gem gem3 = GetGemAtPosition(position3);
		Gem gem4 = GetGemAtPosition(position4);
		Gem gem5 = GetGemAtPosition(position5);
		Gem gem6 = GetGemAtPosition(position6);
		Gem gem7 = GetGemAtPosition(position7);

		// 3 points

		if (gem2 != null && gem3 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem4 != null && gem5 != null)
		{
			if (gem.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem3 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 4 points 

		if (gem2 != null && gem3 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 4;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem3 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 4;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 5 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem2 != null && gem4 != null && gem5 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem3 != null && gem4 != null && gem5 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem2 != null && gem3 != null && gem6 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem6.GetGemSprite() && gem6.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}


		// 6 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 6;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 6;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 7 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem6 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite() && gem6.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 7;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		return check;
	}

	private bool CheckMatchGemBelow(Gem gem, float gemX, float gemY)
	{
		bool check = false;
		int point;

		// giả sử đặt gem được chọn ở vị trí [0, 0]
		Vector2 position1 = new Vector2((float)(gemX), (float)(gemY - 0.125)); // vị trí của gem [0, -1] so với gem được chọn


		Vector2 position2 = new Vector2((float)(gemX + 0.125), (float)(gemY - 0.125)); // vị trí của gem [1, -1] so với gem được chọn
		Vector2 position3 = new Vector2((float)(gemX - 0.125), (float)(gemY - 0.125)); // vị trí của gem [-1, -1] so với gem được chọn

		Vector2 position4 = new Vector2((float)(gemX), (float)(gemY - 0.125 - 0.125)); // vị trí của gem [0, -2] so với gem được chọn
		Vector2 position5 = new Vector2((float)(gemX), (float)(gemY - 0.125 - 0.125 - 0.125)); // vị trí của gem [0, -3] so với gem được chọn

		Vector2 position6 = new Vector2((float)(gemX + 0.125 + 0.125), (float)(gemY - 0.125)); // vị trí của gem [2, -1] so với gem được chọn
		Vector2 position7 = new Vector2((float)(gemX - 0.125 - 0.125), (float)(gemY - 0.125)); // vị trí của gem [-2, -1] so với gem được chọn

		Gem gem1 = GetGemAtPosition(position1);
		Gem gem2 = GetGemAtPosition(position2);
		Gem gem3 = GetGemAtPosition(position3);
		Gem gem4 = GetGemAtPosition(position4);
		Gem gem5 = GetGemAtPosition(position5);
		Gem gem6 = GetGemAtPosition(position6);
		Gem gem7 = GetGemAtPosition(position7);

		// 3 points

		if (gem2 != null && gem3 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem4 != null && gem5 != null)
		{
			if (gem.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem3 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 3;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 4 points 

		if (gem2 != null && gem3 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 4;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem3 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 4;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 5 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem2 != null && gem4 != null && gem5 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem3 != null && gem4 != null && gem5 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}

		if (gem2 != null && gem3 != null && gem6 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem6.GetGemSprite() && gem6.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 5;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;
			}
		}


		// 6 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem6 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite())
			{
				point = 6;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 6;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}


		// 7 points

		if (gem2 != null && gem3 != null && gem4 != null && gem5 != null && gem6 != null && gem7 != null)
		{
			if (gem.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite() && gem3.GetGemSprite() == gem4.GetGemSprite() && gem4.GetGemSprite() == gem5.GetGemSprite() && gem5.GetGemSprite() == gem6.GetGemSprite() && gem6.GetGemSprite() == gem7.GetGemSprite())
			{
				point = 7;
				gemList.Add(new Tuple<Gem, int, Gem>(gem, point, gem1));
				check = true;

			}
		}

		return check;
	}





    public List<CollectedGemInfo> GetCollectedGemInfoList()
    {
        var gemInfos = new List<CollectedGemInfo>();
        var gemsDict = gemClicker.GetGemsEatenCount();

        foreach (var gem in gemsDict)
        {
            var gemInfo = new CollectedGemInfo
            {
                GemType = gem.Key.ToString(),
                Count = gem.Value,
                Icon = gemSprites[(int)gem.Key] // match theo enum index
            };

            gemInfos.Add(gemInfo);
        }

        // Thứ tự mong muốn: Health (Hp), Shield, Mana (Mp), Power (Pp), Atk
        var desiredOrder = new List<GemType> { GemType.Hp, GemType.Shield, GemType.Mp, GemType.Pp, GemType.Atk };

        // Sắp xếp gemInfos dựa trên thứ tự ưu tiên
        gemInfos = gemInfos.OrderBy(info =>
        {
            // Convert từ string về GemType enum
            Enum.TryParse<GemType>(info.GemType, out var gemType);
            return desiredOrder.IndexOf(gemType);
        }).ToList();


        return gemInfos;
    }



    //  This scipt manage gem eaten in board

    // Count the number of each gem eaten
    public IEnumerator GetGemTypeInDic()
	{
		var gems = gemClicker.GetGemsEatenCount();
		foreach (var gem in gems)
		{
			Debug.Log(gem.Key.ToString() + ": " + gem.Value);
		}
		gemClicker.gemsEatenCount.Clear();

		yield return null;
	}

	// Count Atk gem
	public int GetAtkGemCount()
	{
		int count = 0;
		var gems = gemClicker.GetGemsEatenCount();
		foreach (var gem in gems)
		{
			if (gem.Key.ToString() == "Atk")
			{
				count = gem.Value;
			}
		}
		
		//Debug.Log($"Count Gem Atk: {count}");
		return count;
	}

	// Count Heal gem
	public int GetHealthGemCount()
	{
		int count = 0;
		var gems = gemClicker.GetGemsEatenCount();
		foreach (var gem in gems)
		{
			if (gem.Key.ToString() == "Hp")
			{
				count = gem.Value;
			}
		}

		//Debug.Log($"Count Gem Health: {count}");
		return count;
	}

	// COunt Mana gem
	public int GetManaGemCount()
	{
		int count = 0;
		var gems = gemClicker.GetGemsEatenCount();
		foreach (var gem in gems)
		{
			if (gem.Key.ToString() == "Mp")
			{
				count = gem.Value;
			}
		}

		//Debug.Log($"Count Gem Mana: {count}");
		return count;
	}

	// Count Power gem
	public int GetPowerGemCount()
	{
		int count = 0;
		var gems = gemClicker.GetGemsEatenCount();
		foreach (var gem in gems)
		{
			if (gem.Key.ToString() == "Pp")
			{
				count = gem.Value;
			}
		}

		//Debug.Log($"Count Gem Power: {count}");
		return count;
	}

	// Count Shield gem
	public int GetShieldGemCount()
	{
		int count = 0;
		var gems = gemClicker.GetGemsEatenCount();
		foreach (var gem in gems)
		{
			if (gem.Key.ToString() == "Shield")
			{
				count = gem.Value;
			}
		}

		//Debug.Log($"Count Gem Shield: {count}");
		return count;
	}

	// Clear gem eaten
	public void ClearGemCount()
	{
		gemClicker.gemsEatenCount.Clear();
	}







	// Initialize gem in board
	private void InitializeBoard()
	{
		gems = new Gem[width, height];

		float boardWidth = width * gemSpacing; // Chiều rộng của GameObject cha
		float boardHeight = height * gemSpacing; // Chiều cao của GameObject cha

		// Tạo GameObject cha cho board
		GameObject boardObject = GameObject.Find("Board");

		// Tạo các viên kim cương trong bảng
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Vector2 position = new Vector2(
					(x * gemSpacing) - (boardWidth / 2) + (gemSpacing / 2),
					(y * gemSpacing) - (boardHeight / 2) + (gemSpacing / 2)
				);
				GameObject gemObject = Instantiate(gemPrefab, position, Quaternion.identity);
				Gem gem = gemObject.GetComponent<Gem>();

				// Chọn ngẫu nhiên sprite cho kim cương
				Sprite randomSprite = GetRandomUniqueSprite(x, y);
				gem.SetGemSprite(randomSprite);
				gem.SetGemType(gem.GetGemSprite());

				gems[x, y] = gem;

				// Gán GameObject board cho GameObject gem
				gemObject.transform.SetParent(boardObject.transform);

				//CreateNewGem(x, y);
				//Debug.Log(gemObject.transform.position.x + " " + gemObject.transform.position.y);
			}
		}
	}

	private Sprite GetRandomUniqueSprite(int x, int y)
	{
		List<Sprite> availableSprites = new List<Sprite>(gemSprites);

		// Tìm và loại bỏ các sprite đã được sử dụng trong hàng và cột hiện tại
		RemoveUsedSpritesInRow(/*x,*/ y, availableSprites);
		RemoveUsedSpritesInColumn(x, /*y,*/ availableSprites);

		//Debug.Log($"[{x},{y}] = " + availableSprites.Count);

		// Chọn ngẫu nhiên một sprite từ danh sách sprite còn lại
		int randomIndex = UnityEngine.Random.Range(0, availableSprites.Count);
		Sprite randomSprite = availableSprites[randomIndex];

		return randomSprite;
	}

	private void RemoveUsedSpritesInRow(/*int x,*/ int y, List<Sprite> availableSprites)
	{
		int consecutiveCount = 0;
		Sprite previousSprite = null;

		// Kiểm tra các sprite trong hàng hiện tại
		for (int i = 0; i < width; i++)
		{
			Gem gem = gems[i, y]; //x - i
			if (gem != null)
			{
				Sprite gemSprite = gem.GetGemSprite();

				if (gemSprite == previousSprite)
				{
					consecutiveCount++;
				}
				else
				{
					consecutiveCount = 1;
					previousSprite = gemSprite;
				}

				if (consecutiveCount >= 2)
				{
					availableSprites.Remove(gemSprite);
				}
			}
		}
	}

	private void RemoveUsedSpritesInColumn(int x, /*int y,*/ List<Sprite> availableSprites)
	{
		int consecutiveCount = 0;
		Sprite previousSprite = null;

		// Kiểm tra các sprite trong cột hiện tại
		for (int i = 0; i < height; i++)
		{
			Gem gem = gems[x, i];
			if (gem != null)
			{
				Sprite gemSprite = gem.GetGemSprite();

				if (gemSprite == previousSprite)
				{
					consecutiveCount++;
				}
				else
				{
					consecutiveCount = 1;
					previousSprite = gemSprite;
				}

				if (consecutiveCount >= 2)
				{
					availableSprites.Remove(gemSprite);
				}
			}
		}
	}

	private bool HasEmptySpaces()
	{
		Gem[] gems = GetComponentsInChildren<Gem>();

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Gem gem = gemClicker.GetGemAtPosition(gems, x, y, gemSpacing, width, height);
				if (gem == null)
				{
					return true;
				}
			}
		}

		

		return false;
	}

	private void FillEmptySpaces()
	{
		//Gem[] gems = GetComponentsInChildren<Gem>();

		//for (int x = 0; x < width; x++)
		//{
		//	for (int y = 0; y < height - 1; y++)
		//	{
		//		Gem currentGem = gemClicker.GetGemAtPosition(gems, x, y, gemSpacing, width, height);
		//		Gem gemAbove = gemClicker.GetGemAtPosition(gems, x, y + 1, gemSpacing, width, height);

		//		//Debug.Log(gemAbove.GetPositionInBoard().x + " " + gemAbove.GetPositionInBoard().y);

		//		if (currentGem == null && gemAbove != null)
		//		{
		//			MoveGemDown(gemAbove);
		//		}
		//	}
		//}


		Gem[] gems = GetComponentsInChildren<Gem>();

		for (int x = 0; x < width; x++)
		{
			int emptyCount = 0; // Số lượng gem trống trong cột

			for (int y = 0; y < height; y++)
			{
				Gem currentGem = gemClicker.GetGemAtPosition(gems, x, y, gemSpacing, width, height);

				if (currentGem == null)
				{
					emptyCount++;
				}
				else if (emptyCount > 0)
				{
					MoveGemDown(currentGem, emptyCount);
				}
			}
		}		
	}

	private void MoveGemDown(Gem gem, int steps)
	{

		Vector2 currentPosition = gem.GetPositionInBoard();
		Vector2 newPosition = new Vector2(currentPosition.x, currentPosition.y - (steps * 0.125f));

		// Cập nhật vị trí mới cho gem
		gem.SetPosition(newPosition.x, newPosition.y);

		// Nếu gem đã có highlight, hãy xóa highlight
		gem.RemoveHighlight();
	}

	private IEnumerator CheckMatch3Gem()
	{
		//Transform gemContainer = transform; // Lấy transform của game object cha
		//Gem[] gems = gemContainer.GetComponentsInChildren<Gem>(); // Lấy các thành phần Gem trong game object cha
		
		Gem[] gems = GetComponentsInChildren<Gem>();

		// Kiểm tra hàng
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width - 2; x++)
			{
				Gem gem1 = gemClicker.GetGemAtPosition(gems, x, y, gemSpacing, width, height);
				Gem gem2 = gemClicker.GetGemAtPosition(gems, x + 1, y, gemSpacing, width, height);
				Gem gem3 = gemClicker.GetGemAtPosition(gems, x + 2, y, gemSpacing, width, height);
				//Debug.Log(gem1.GetPositionInBoard().x + " " + gem1.GetPositionInBoard().y + gem1.GetGemSprite().name);
				if (gem1 != null && gem2 != null && gem3 != null)
				{
					if (gem1.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite())
					{
						// Các gem match 3 liên tiếp
						// Thực hiện xử lý tương ứng
						if (!gemClicker.gemsToRemove.Contains(gem1))
						{
							gemClicker.IncreaseGemCount(gem1.GetGemType(), gemClicker.gemsEatenCount);
							gemClicker.gemsToRemove.Add(gem1);
						}
						if (!gemClicker.gemsToRemove.Contains(gem2))
						{
							gemClicker.IncreaseGemCount(gem2.GetGemType(), gemClicker.gemsEatenCount);
							gemClicker.gemsToRemove.Add(gem2);
						}
						if (!gemClicker.gemsToRemove.Contains(gem3))
						{
							gemClicker.IncreaseGemCount(gem3.GetGemType(), gemClicker.gemsEatenCount);
							gemClicker.gemsToRemove.Add(gem3);
						}

						//Debug.Log("True");			
					}
				}
			}
		}

		// Kiểm tra cột
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height - 2; y++)
			{
				Gem gem1 = gemClicker.GetGemAtPosition(gems, x, y, gemSpacing, width, height);
				Gem gem2 = gemClicker.GetGemAtPosition(gems, x, y + 1, gemSpacing, width, height);
				Gem gem3 = gemClicker.GetGemAtPosition(gems, x, y + 2, gemSpacing, width, height);
				//Debug.Log(gem1.GetPositionInBoard().x + " " + gem1.GetPositionInBoard().y + gem1.GetGemSprite().name);
				if (gem1 != null && gem2 != null && gem3 != null)
				{
					//Debug.Log("True");
					if (gem1.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite())
					{
						// Các gem match 3 liên tiếp
						// Thực hiện xử lý tương ứng
						if (!gemClicker.gemsToRemove.Contains(gem1))
						{
							gemClicker.IncreaseGemCount(gem1.GetGemType(), gemClicker.gemsEatenCount);
							gemClicker.gemsToRemove.Add(gem1);
						}
						if (!gemClicker.gemsToRemove.Contains(gem2))
						{
							gemClicker.IncreaseGemCount(gem2.GetGemType(), gemClicker.gemsEatenCount);
							gemClicker.gemsToRemove.Add(gem2);
						}
						if (!gemClicker.gemsToRemove.Contains(gem3))
						{
							gemClicker.IncreaseGemCount(gem3.GetGemType(), gemClicker.gemsEatenCount);
							gemClicker.gemsToRemove.Add(gem3);
						}

						//Debug.Log("True");						
					}
				}
			}
		}

		// Chờ 2 giây trước khi xóa gem
		yield return new WaitForSeconds(0.5f);

		// Xóa các gem trong danh sách cần xóa
		foreach (Gem gem in gemClicker.gemsToRemove)
		{
			//gemClicker.RemoveGem(gem);
			StartCoroutine(gemClicker.RemoveGem(gem));
		}

		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator UpdateGemInBoard()
	{
		isChanging = true;

		Gem[] gems = GetComponentsInChildren<Gem>();

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Gem gem = gemClicker.GetGemAtPosition(gems, x, y, gemSpacing, width, height);
				if (gem == null)
				{
					CreateNewGem(x, y);
				}
			}
		}

		if (!CheckMatchGem())
		{
			isChanging = false;
		}

		yield return null; // Hoặc thêm các logic xử lý khác tại đây
	}

	private void CreateNewGem(int x, int y)
	{
		gems = new Gem[width, height];

		float boardWidth = width * gemSpacing; // Chiều rộng của GameObject cha
		float boardHeight = height * gemSpacing; // Chiều cao của GameObject cha

		// Tạo GameObject cha cho board
		GameObject boardObject = GameObject.Find("Board");

		Vector2 position = new Vector2(
								(x * gemSpacing) - (boardWidth / 2) + (gemSpacing / 2),
								(y * gemSpacing) - (boardHeight / 2) + (gemSpacing / 2)
							);
		GameObject gemObject = Instantiate(gemPrefab, position, Quaternion.identity);
		Gem gem = gemObject.GetComponent<Gem>();

		// Chọn ngẫu nhiên sprite cho kim cương
		Sprite randomSprite = GetRandomUniqueSprite(x, y);
		gem.SetGemSprite(randomSprite);
		gem.SetGemType(gem.GetGemSprite());

		gems[x, y] = gem;

		// Gán GameObject board cho GameObject gem
		gemObject.transform.SetParent(boardObject.transform);
	}

	public bool CheckMatchGem()
	{
		//Transform gemContainer = transform; // Lấy transform của game object cha
		//Gem[] gems = gemContainer.GetComponentsInChildren<Gem>(); // Lấy các thành phần Gem trong game object cha
		bool check = false;
		Gem[] gems = GetComponentsInChildren<Gem>();

		// Kiểm tra hàng
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width - 2; x++)
			{
				Gem gem1 = gemClicker.GetGemAtPosition(gems, x, y, gemSpacing, width, height);
				Gem gem2 = gemClicker.GetGemAtPosition(gems, x + 1, y, gemSpacing, width, height);
				Gem gem3 = gemClicker.GetGemAtPosition(gems, x + 2, y, gemSpacing, width, height);
				//Debug.Log(gem1.GetPositionInBoard().x + " " + gem1.GetPositionInBoard().y + gem1.GetGemSprite().name);
				if (gem1 != null && gem2 != null && gem3 != null)
				{
					if (gem1.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite())
					{
						check = true;
					}
				}
			}
		}

		// Kiểm tra cột
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height - 2; y++)
			{
				Gem gem1 = gemClicker.GetGemAtPosition(gems, x, y, gemSpacing, width, height);
				Gem gem2 = gemClicker.GetGemAtPosition(gems, x, y + 1, gemSpacing, width, height);
				Gem gem3 = gemClicker.GetGemAtPosition(gems, x, y + 2, gemSpacing, width, height);
				//Debug.Log(gem1.GetPositionInBoard().x + " " + gem1.GetPositionInBoard().y + gem1.GetGemSprite().name);
				if (gem1 != null && gem2 != null && gem3 != null)
				{
					//Debug.Log("True");
					if (gem1.GetGemSprite() == gem2.GetGemSprite() && gem2.GetGemSprite() == gem3.GetGemSprite())
					{
						check = true;
					}
				}
			}
		}

		return check;
	}

	public bool EndTurn()
	{
		if (!HasEmptySpaces() && gemClicker.isSwapping && !isChanging)
		{
			gemClicker.isSwapping = false;
			isChanging = true;

			return true;
		}

		return false; // Lượt chưa kết thúc
	}

	public void ClearBoard()
	{
		Gem[] gems = GetComponentsInChildren<Gem>();
		foreach (var gem in gems)
		{
			Destroy(gem.gameObject);
		}
	}
}
