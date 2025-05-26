using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBar : MonoBehaviour
{
	[SerializeField] GameObject mana;

	public void SetMana(float mnNormalized)
	{
		mana.transform.localScale = new Vector3(mnNormalized, 1f, 1f);
	}

	public IEnumerator SetDecreaseManaSmooth(float newMp)
	{
		float curMp = mana.transform.localScale.x;
		float changeAmt = curMp - newMp;

		while (curMp - newMp > Mathf.Epsilon)
		{
			curMp -= changeAmt * Time.deltaTime;
			mana.transform.localScale = new Vector3(curMp, 1f);
			yield return null;
		}
		mana.transform.localScale = new Vector3(curMp, 1f);
	}

    //public IEnumerator SetIncreaseManaSmooth(float newMp)
    //{
    //	float curMp = mana.transform.localScale.x;
    //	float changeAmt = newMp - curMp;

    //	while (newMp - curMp > Mathf.Epsilon)
    //	{
    //		curMp += changeAmt * Time.deltaTime;
    //		mana.transform.localScale = new Vector3(curMp, 1f);
    //		yield return null;
    //	}
    //	mana.transform.localScale = new Vector3(curMp, 1f);
    //}

    public IEnumerator SetIncreaseManaSmooth(float newMp, float duration)
    {
        float curMp = mana.transform.localScale.x;
        float elapsed = 0f;
        float startMp = curMp;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentMp = Mathf.Lerp(startMp, newMp, t);
            mana.transform.localScale = new Vector3(currentMp, 1f);
            yield return null;
        }

        mana.transform.localScale = new Vector3(newMp, 1f);
    }

}
