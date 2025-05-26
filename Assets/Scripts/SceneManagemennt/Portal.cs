using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DestinationIdentifier {  A, B, C, D, E, F }

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;
    Fader fader;

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        StartCoroutine(SwitchScene());
    }

	private void Start()
	{
       fader = FindObjectOfType<Fader>();
	}

	IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

		GameController.Instance.PauseGame(true);

		yield return fader.FadeIn(0.5f);

		yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        player.transform.position = destPortal.SpawnPoint.position;

		GameController.Instance.PauseGame(false);

		yield return fader.FadeOut(0.5f);

		Destroy(gameObject);
    }

    public Transform SpawnPoint => spawnPoint;

}
