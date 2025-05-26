using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
	Image image;

	private void Awake()
	{
		image = GetComponent<Image>();
	}


	public IEnumerator FadeIn(float time)
	{
		image.color = new Color(image.color.r, image.color.g, image.color.b, 0f); // Đặt alpha ban đầu là 0

		float elapsedTime = 0f;
		while (elapsedTime < time)
		{
			float alpha = Mathf.Lerp(0f, 1f, elapsedTime / time); // Lerp alpha từ 0 đến 1 trong khoảng thời gian xác định
			image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		image.color = new Color(image.color.r, image.color.g, image.color.b, 1f); // Đảm bảo alpha cuối cùng là 1
	}


	//public IEnumerator FadeIn(float time)
	//{
	//	yield return image.DOFade(1f, time).WaitForCompletion();
	//}

	public IEnumerator FadeOut(float time)
	{
		float elapsedTime = 0f;
		while (elapsedTime < time)
		{
			float alpha = Mathf.Lerp(1f, 0f, elapsedTime / time); // Lerp alpha từ 1 đến 0 trong khoảng thời gian xác định
			image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		image.color = new Color(image.color.r, image.color.g, image.color.b, 0f); // Đảm bảo alpha cuối cùng là 0
	}

	//public IEnumerator FadeOut(float time)
	//{
	//	yield return image.DOFade(0f, time).WaitForCompletion();
	//}
}
