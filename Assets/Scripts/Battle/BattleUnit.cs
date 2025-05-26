using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] private GameObject damageEffectObject;
    [SerializeField] private Text damageText;
    [SerializeField] private GameObject pointEffectObject;
    [SerializeField] private Text point;
    //private Animator animator;
    public GameObject healEffectPrefab;
    public GameObject shieldEffectObject;
    public GameObject manaEffectPrefab;
    public GameObject powerEffectPrefab;

    public Color healColor = Color.green;
    public Color manaColor = new Color(0.3f, 0.6f, 1f); // xanh dương
    public Color powerColor = Color.red;   // đỏ
    public Color shieldColor = new Color(0.4f, 0.0f, 0.6f); // tím

    public Pokemon Pokemon { get; set; }
    //public Transform effectSpawnPoint;

    public void Setup()
    {
        Pokemon = new Pokemon(_base, level);
        if (isPlayerUnit)
        {
			//GetComponent<Image>().sprite = Pokemon.Base.Sprite;
            AnimationClip animationClip = Pokemon.Base.AllyAnimation; // Gán Animation Clip từ PokemonBase
            GetComponent<Animator>().Play(animationClip.name);
			//animator = GetComponent<Animator>();          
			//animator.Play(animationClip.name);
		}
        else
        {
			AnimationClip animationClip = Pokemon.Base.EnemyAnimation; 
			GetComponent<Animator>().Play(animationClip.name);
		}
    }

    // Show effect when heal, shield, mana, power
    public void PlayEffect(GameObject effect, float duration = 1.5f)
    {
        if (effect != null)
        {
            effect.SetActive(true);
            StartCoroutine(HideEffectAfterTime(effect, duration));
        }
    }

    private IEnumerator HideEffectAfterTime(GameObject effect, float duration)
    {
        yield return new WaitForSeconds(duration);
        effect.SetActive(false);
    }

    // Show effect when player attack
    public void ShowDamageEffect(int damageAmount, float duration = 1.5f)
    {
        if (damageEffectObject != null && damageText != null)
        {
            damageText.text = "-" + damageAmount.ToString(); // hoặc "+50", "-30", vv.
            damageEffectObject.SetActive(true);
            StartCoroutine(HideEffectAfterTime(damageEffectObject, duration));
        }
    }

    // Show stat point effect
    public void ShowStatPoint(int amount, Color color, float duration = 1.5f)
    {
        if (pointEffectObject != null && point != null)
        {
            point.text = "+" + amount.ToString();
            point.color = color;
            pointEffectObject.SetActive(true);
            StartCoroutine(AnimateStatText(pointEffectObject, duration));
        }
    }

    private IEnumerator AnimateStatText(GameObject effectObject, float duration)
    {
        Vector3 startPos = effectObject.transform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, 50f, 0); // Trượt lên

        float elapsed = 0f;
        CanvasGroup canvasGroup = effectObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = effectObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            effectObject.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            canvasGroup.alpha = 1 - t;

            elapsed += Time.deltaTime;
            yield return null;
        }

        effectObject.SetActive(false);
        effectObject.transform.localPosition = startPos;
        canvasGroup.alpha = 1;
    }
}
