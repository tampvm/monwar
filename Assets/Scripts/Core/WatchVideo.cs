using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class WatchVideo : MonoBehaviour
{
	VideoPlayer intro;
	VideoPlayer outtroWin;
	VideoPlayer outtroLose;
	RawImage rawImage;
	public GameObject background;

	private bool isPlayingVideo = false;

	public static WatchVideo Instance;

	private void Awake()
	{
		Instance = this;
		VideoPlayer[] videoPlayers = GetComponentsInChildren<VideoPlayer>();
		intro = videoPlayers[0];
		outtroWin = videoPlayers[1];
		outtroLose = videoPlayers[2];

		rawImage = GetComponent<RawImage>();
	}

	private void Start()
	{
		intro.Pause();
		outtroWin.Pause();
		outtroLose.Pause();
	}

	public IEnumerator FadeIn(float duration)
	{
		rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 0f); // Đặt alpha ban đầu là 0

		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration); // Lerp alpha từ 0 đến 1 trong khoảng thời gian xác định
			rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, alpha);

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 1f); // Đảm bảo alpha cuối cùng là 1
	}

	public IEnumerator FadeOut(float duration)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // Lerp alpha từ 1 đến 0 trong khoảng thời gian xác định
			rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, alpha);

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 0f); // Đảm bảo alpha cuối cùng là 0
	}

	public IEnumerator PlayIntroVideo()
	{
		if (!isPlayingVideo)
		{
			isPlayingVideo = true;

			GameController.Instance.PauseGame(true);

			yield return FadeIn(0f); // Hiệu ứng fade-in

			intro.time = 0f; // set video start time at 0s

			intro.Play(); // Bắt đầu phát video
			rawImage.texture = intro.texture; // Gắn texture của video vào RawImage

			float clipDuration = (float)intro.frameCount / intro.frameRate; // Tính thời lượng của clip video

			yield return new WaitForSeconds(clipDuration); // Chờ cho đến khi video kết thúc

			GameController.Instance.PauseGame(false);

			yield return FadeOut(0f);
			
			isPlayingVideo = false;

			background.SetActive(true);

			yield return new WaitForSeconds(2f);

			background.SetActive(false);
		}
	}

	public IEnumerator PlayWinOuttroVideo()
	{
		if (!isPlayingVideo)
		{
			isPlayingVideo = true;

			GameController.Instance.PauseGame(true);

			yield return FadeIn(0f); // Hiệu ứng fade-in

			outtroWin.time = 0f; // set video start time at 0s

			outtroWin.Play(); // Bắt đầu phát video
			rawImage.texture = outtroWin.texture; // Gắn texture của video vào RawImage

			float clipDuration = (float)outtroWin.frameCount / outtroWin.frameRate; // Tính thời lượng của clip video

			yield return new WaitForSeconds(clipDuration); // Chờ cho đến khi video kết thúc

			GameController.Instance.PauseGame(false);

			yield return FadeOut(0f);

			isPlayingVideo = false;

			//yield return new WaitForSeconds(2f);
		}
	}

	public IEnumerator PlayLoseOuttroVideo()
	{
		if (!isPlayingVideo)
		{
			isPlayingVideo = true;

			GameController.Instance.PauseGame(true);

			yield return FadeIn(0f); // Hiệu ứng fade-in

			outtroLose.time = 0f; // set video start time at 0s

			outtroLose.Play(); // Bắt đầu phát video
			rawImage.texture = outtroLose.texture; // Gắn texture của video vào RawImage

			float clipDuration = (float)outtroLose.frameCount / outtroLose.frameRate; // Tính thời lượng của clip video

			yield return new WaitForSeconds(clipDuration); // Chờ cho đến khi video kết thúc

			GameController.Instance.PauseGame(false);

			yield return FadeOut(0f);

			isPlayingVideo = false;

			//yield return new WaitForSeconds(2f);
		}
	}
}
