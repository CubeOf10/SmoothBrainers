using UnityEngine;
using System.Collections;

public class BaseProjectile : MonoBehaviour {

	public float damage;
	public float projectileSpeed;

	public float lifeTime;

	//Effects
	public GameObject trail;
	public GameObject hitEffect;
	
	//Audio
	public GameObject launchSound;
	public GameObject flightSound;
	public GameObject hitSound;

	// Use this for initialization
	protected virtual void Start () {
	
		//Set object kill time
		Destroy (gameObject, lifeTime);
	}
}
