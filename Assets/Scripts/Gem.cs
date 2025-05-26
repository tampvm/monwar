using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GemType
{
	Atk,
	Hp,
	Mp,
	Pp,
	Shield
}

public class Gem : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;
	private GameObject currentHighlight;
	private GemType _gemType;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	} 

	public void SetGemSprite(Sprite gemSprite)
	{
		spriteRenderer.sprite = gemSprite;
	}

	public Sprite GetGemSprite()
	{
		return spriteRenderer.sprite;
	}

	public void SetGemType(Sprite gemSprite)
	{
		if (gemSprite.name == "Attack")
		{
			_gemType = GemType.Atk;
		}
		else if (gemSprite.name == "Health")
		{
			_gemType = GemType.Hp;
		}
		else if (gemSprite.name == "Mana")
		{
			_gemType = GemType.Mp;
		}
		else if (gemSprite.name == "Power")
		{
			_gemType = GemType.Pp;
		}
		else _gemType = GemType.Shield;
	}

	public GemType GetGemType()
	{
		return _gemType;
	}

	public void SetHighlightPrefab(GameObject highlightPrefab)
	{
		// Xóa khung viền highlight hiện tại (nếu có)
		if (currentHighlight != null)
		{
			Destroy(currentHighlight);
		}

		// Tạo khung viền highlight mới
		currentHighlight = Instantiate(highlightPrefab, transform.position, Quaternion.identity);
		currentHighlight.transform.SetParent(transform);
	}

	public void RemoveHighlight()
	{
		// Xóa khung viền highlight hiện tại (nếu có)
		if (currentHighlight != null)
		{
			Destroy(currentHighlight);
			currentHighlight = null;
		}
	}

	public Vector2 GetPositionInBoard()
	{
		// Lấy vị trí cục bộ của viên gem trong game object board
		Vector3 localPosition = transform.localPosition;

		// Trả về vị trí nguyên tố
		return new Vector2(localPosition.x, localPosition.y);
	}

	// Phương thức khởi tạo để thiết lập vị trí của viên gem
	public void SetPosition(float gemX, float gemY)
	{
		// Cập nhật vị trí cục bộ của viên gem
		//transform.position = transform.TransformPoint(new Vector3(0, 0, 0));

		// Cập nhật vị trí cục bộ của viên gem
		//transform.localPosition = new Vector3(gemX, gemY, transform.localPosition.z);

		float smoothness = 1f; // Điều chỉnh mức độ mượt
		transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(gemX, gemY, transform.localPosition.z), smoothness);
		
		//float smoothness = 0.5f; // Điều chỉnh mức độ mượt
		//transform.position = Vector3.Lerp(transform.position, new Vector3(gemX, gemY, transform.position.z), smoothness);
	}

}
