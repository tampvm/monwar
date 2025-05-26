using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
	//[SerializeField] private LayerMask solidObjectLayer;
	//[SerializeField] private LayerMask grassLayer;
	//[SerializeField] private GameObject panel;

	public event Action OnEncountered;

    private bool isMoving;
    private Vector2 input;
	private Animator animator;
	//WatchVideo video; 

	public static PlayerController Instance;

	//public bool isTouch;

	private void Awake()
	{
		Instance = this;
		animator = GetComponent<Animator>();
		//video = FindObjectOfType<WatchVideo>();
	}

	//private void Update()
	//{
	//	var targetPos = transform.position;
	//	if (IsTv(targetPos))
	//	{
	//		//TVController.Instance.ShowRequest();
	//		isTouch = true;
	//		if (Input.GetKeyDown(KeyCode.F))
	//		{
	//			StartCoroutine(Interact());				
	//		}
	//	}
	//	else isTouch = false;
	//}

	//private IEnumerator Interact()
	//{
	//	// Script for watching video
	//	//yield return StartCoroutine(video.PlayIntroVideo());
	//	yield return null;
	//	OnEncountered();
	//}

	public void HandleUpdate()
	{
		if (!isMoving)
		{
			input.x = Input.GetAxisRaw("Horizontal");
			input.y = Input.GetAxisRaw("Vertical");

			// Xóa cái này để di chuyển chéo
			if (input.x != 0) input.y = 0;

			if (input != Vector2.zero)
			{
				animator.SetFloat("moveX", input.x);
				animator.SetFloat("moveY", input.y);

				var targetPos = transform.position;

				targetPos.x += input.x;
				targetPos.y += input.y;


				if (IsWalkable(targetPos))
				{
					StartCoroutine(Move(targetPos));
				}
			}
		}



		animator.SetBool("isMoving", isMoving);
	}




	private IEnumerator Move(Vector3 targetPos)
	{
		isMoving = true;

		while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
		{
			transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
			yield return null;
		}

		transform.position = targetPos;
		isMoving = false;

		CheckForEncouter();
		OnMoveOver();
	}



	// tạo 1 game object mới đặt trong player để xử lý va chạm, chuyển script xử lí va chạm sang đó 

	private bool IsWalkable(Vector3 targetPos)
	{
		if (Physics2D.OverlapCircle(targetPos, 0.0f, GameLayers.Instance.SolidLayer) != null)
		{
			return false;
		}
		return true;
	}

	//private bool ShowRequest(Vector3 targetPos)
	//{
	//	var collider = Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.Instance.SolidLayer);
	//	if (collider != null)
	//	{
	//		panel.SetActive(true);
	//		return true;
	//	}
	//	panel.SetActive(false);
	//	return false;
	//}

	//private bool IsTv(Vector3 targetPos)
	//{
	//	var collider = Physics2D.OverlapCircle(targetPos, 0.0f, GameLayers.Instance.InteractableLayer);
	//	if (collider != null)
	//	{
	//		return true;
	//	}
	//	return false;
	//}

	private void CheckForEncouter()
	{
		if (Physics2D.OverlapCircle(transform.position, 0.0f, GameLayers.Instance.GrassLayer) != null)
		{
			if (UnityEngine.Random.Range(1, 10) <= 10)
			{			
				//OnEncountered();

				Debug.Log("Encountered!");
            }
		}
	}

	private void OnMoveOver()
	{
		var colliders = Physics2D.OverlapCircleAll(transform.position, 0.0f, GameLayers.Instance.TriggerableLayers);
		foreach (var collider in colliders)
		{
			var triggerable = collider.GetComponent<IPlayerTriggerable>();
			if (triggerable != null)
			{
				triggerable.OnPlayerTriggered(this);
				break;
			}
		}
	}
}
