using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectSpawner : MonoBehaviour
{
	[SerializeField] GameObject essentialObjectsPrefab;

	private void Awake()
	{
		var existingObject = FindObjectsOfType<EssentialObjects>();
		if (existingObject.Length == 0)
		{
			// Set vị trí khởi tạo cho nv dựa trên vị trí của essential object
			Instantiate(essentialObjectsPrefab, new Vector3(essentialObjectsPrefab.transform.position.x, essentialObjectsPrefab.transform.position.y, essentialObjectsPrefab.transform.position.z), Quaternion.identity);
		}
	}
}
