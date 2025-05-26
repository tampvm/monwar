using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBar : MonoBehaviour
{
	[SerializeField] GameObject shield;

	public void SetShield(float pwNormalized)
	{
		shield.transform.localScale = new Vector3(pwNormalized, 1f, 1f);
	}

	public IEnumerator SetDecreaseShieldSmooth(float newSh)
	{
		float curSh = shield.transform.localScale.x;
		float changeAmt = curSh - newSh;

		while (curSh - newSh > Mathf.Epsilon)
		{
			curSh -= changeAmt * Time.deltaTime;
			shield.transform.localScale = new Vector3(curSh, 1f);
			yield return null;
		}
		shield.transform.localScale = new Vector3(curSh, 1f);
	}

    //public IEnumerator SetIncreaseShieldSmooth(float newSh)
    //{
    //	float curSh = shield.transform.localScale.x;
    //	float changeAmt = newSh - curSh;

    //	while (newSh - curSh > Mathf.Epsilon)
    //	{
    //		curSh += changeAmt * Time.deltaTime;
    //		shield.transform.localScale = new Vector3(curSh, 1f);
    //		yield return null;
    //	}
    //	shield.transform.localScale = new Vector3(curSh, 1f);
    //}

    public IEnumerator SetIncreaseShieldSmooth(float newSh, float duration)
    {
        float curSh = shield.transform.localScale.x;
        float elapsed = 0f;
        float startSh = curSh;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentSh = Mathf.Lerp(startSh, newSh, t);
            shield.transform.localScale = new Vector3(currentSh, 1f);
            yield return null;
        }

        shield.transform.localScale = new Vector3(newSh, 1f);
    }
}
