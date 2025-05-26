using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Paused }

//public enum GameMode { Tutorial, Play }

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
	[SerializeField] private BattleSystem battleSystem;
	[SerializeField] private Camera worldCamera;

    GameState state;

	GameState stateBeforePause;

	//GameMode gameMode;

	public static GameController Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		playerController.OnEncountered += StartBattle;
		battleSystem.OnBattleOver += EndBattle;
	}

	private void StartBattle()
	{
		state = GameState.Battle;
		battleSystem.gameObject.SetActive(true);
		worldCamera.gameObject.SetActive(false);

		battleSystem.StartGame();
	}

	private void EndBattle(bool isEnd)
	{
		state = GameState.FreeRoam;
		battleSystem.gameObject.SetActive(false);

		//if (gameMode == GameMode.Tutorial && battleSystem.isWin)
		//{
		//	//StartCoroutine(WatchVideo.Instance.PlayWinOuttroVideo());
		//	gameMode = GameMode.Play;
		//}
		//else if (gameMode == GameMode.Tutorial && !battleSystem.isWin)
		//{
		//	//StartCoroutine(WatchVideo.Instance.PlayLoseOuttroVideo());
		//	gameMode = GameMode.Play;
		//}

		worldCamera.gameObject.SetActive(true);
	}

	public void PauseGame(bool pause)
	{
		if (pause)
		{
			stateBeforePause = state;
			state = GameState.Paused;
		}
		else
		{
			state = stateBeforePause;
		}
	}

	private void Update()
	{
		if (state == GameState.FreeRoam)
		{
			playerController.HandleUpdate();
		}
		else if (state == GameState.Battle)
		{
			//battleSystem.HandleStartGame();
			
		}
	}


    //[SerializeField]
    //private float moveSpeed;

    //private bool isMoving;
    //private Vector2 input;
    //private Rigidbody2D rb;
    //private Animator animator;

    //private void Awake()
    //{
    //    rb = GetComponent<Rigidbody2D>();
    //    animator = GetComponent<Animator>();
    //}

    //private void Update()
    //{
    //    // Nhận input từ bàn phím
    //    input.x = Input.GetAxisRaw("Horizontal"); // A/D hoặc ←/→
    //    input.y = Input.GetAxisRaw("Vertical");   // W/S hoặc ↑/↓

    //    input = input.normalized; // Đảm bảo tốc độ không nhanh hơn khi đi chéo

    //    // Cập nhật biến isMoving
    //    isMoving = input != Vector2.zero;

    //    // Cập nhật animation
    //    animator.SetBool("isMoving", isMoving);

    //    if (isMoving)
    //    {
    //        animator.SetFloat("moveX", input.x);
    //        animator.SetFloat("moveY", input.y);
    //    }
    //}

    //private void FixedUpdate()
    //{
    //    // Di chuyển nhân vật
    //    rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime);
    //}
}
