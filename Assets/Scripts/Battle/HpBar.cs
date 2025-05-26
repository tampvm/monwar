using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
	[SerializeField] GameObject health;

	public void SetHP(float hpNormalized)
	{
		health.transform.localScale = new Vector3(hpNormalized, 1f, 1f);
	}

	public IEnumerator SetDecreaseHPSmooth(float newHp)
	{
		float curHp = health.transform.localScale.x;
		float changeAmt = curHp - newHp;

		while (curHp - newHp > Mathf.Epsilon)
		{
			curHp -= changeAmt * Time.deltaTime;
			health.transform.localScale = new Vector3(curHp, 1f);
			yield return null;
		}
		health.transform.localScale = new Vector3(newHp, 1f);
	}

    //public IEnumerator SetIncreaseHPSmooth(float newHp)
    //{
    //	float curHp = health.transform.localScale.x;
    //	//float maxHp = 1.0f;
    //	//float newHp = Mathf.Clamp(healAmount, 0.0f, maxHp);
    //	float changeAmt = newHp - curHp;

    //	while (newHp - curHp > Mathf.Epsilon)
    //	{
    //		curHp += changeAmt * Time.deltaTime;
    //		health.transform.localScale = new Vector3(curHp, 1f);
    //		yield return null;
    //	}
    //	health.transform.localScale = new Vector3(newHp, 1f);
    //}

    public IEnumerator SetIncreaseHPSmooth(float newHp, float duration)
    {
        float curHp = health.transform.localScale.x;
        float elapsed = 0f;
        float startHp = curHp;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentHp = Mathf.Lerp(startHp, newHp, t);
            health.transform.localScale = new Vector3(currentHp, 1f);
            yield return null;
        }

        health.transform.localScale = new Vector3(newHp, 1f);
    }
}
