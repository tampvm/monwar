using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBar : MonoBehaviour
{
	[SerializeField] GameObject power;

	public void SetPower(float pwNormalized)
	{
		power.transform.localScale = new Vector3(pwNormalized, 1f, 1f);
	}

	public IEnumerator SetDecreasePowerSmooth(float newPp)
	{
		float curPp = power.transform.localScale.x;
		float changeAmt = curPp - newPp;

		while (curPp - newPp > Mathf.Epsilon)
		{
			curPp -= changeAmt * Time.deltaTime;
			power.transform.localScale = new Vector3(curPp, 1f);
			yield return null;
		}
		power.transform.localScale = new Vector3(curPp, 1f);
	}

    //public IEnumerator SetIncreasePowerSmooth(float newPp)
    //{
    //	float curPp = power.transform.localScale.x;
    //	float changeAmt = newPp - curPp;

    //	while (newPp - curPp > Mathf.Epsilon)
    //	{
    //		curPp += changeAmt * Time.deltaTime;
    //		power.transform.localScale = new Vector3(curPp, 1f);
    //		yield return null;
    //	}
    //	power.transform.localScale = new Vector3(curPp, 1f);
    //}

    public IEnumerator SetIncreasePowerSmooth(float newPp, float duration)
    {
        float curPp = power.transform.localScale.x;
        float elapsed = 0f;
        float startPp = curPp;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentPp = Mathf.Lerp(startPp, newPp, t);
            power.transform.localScale = new Vector3(currentPp, 1f);
            yield return null;
        }

        power.transform.localScale = new Vector3(newPp, 1f);
    }
}
