using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
	[SerializeField] private LayerMask playerLayer;
	[SerializeField] private LayerMask solidObjectLayer;
	[SerializeField] private LayerMask grassLayer;
	//[SerializeField] private LayerMask interactableLayer;
	[SerializeField] private LayerMask portalLayer;

	public static GameLayers Instance { get; set; }

	private void Awake()
	{
		Instance = this;
	}

	public LayerMask PlayerLayer { get => playerLayer; }

	public LayerMask SolidLayer
	{
		get => solidObjectLayer;
	}

	public LayerMask GrassLayer { get => grassLayer; }

	//public LayerMask InteractableLayer { get => interactableLayer; }

	public LayerMask PortalLayer { get => portalLayer; }

	public LayerMask TriggerableLayers
	{
		get => grassLayer /*| fovLayer*/ | portalLayer;
	}
}
