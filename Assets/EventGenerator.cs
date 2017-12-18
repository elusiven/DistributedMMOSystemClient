using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class EventGenerator : MonoBehaviour
{

    private EventListener _eventListener;

	// Use this for initialization
	void Awake ()
	{
	    if (_eventListener == null)
	    {
	        _eventListener = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<EventListener>();
        } 

        SubscribeAreaOfInterest();
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.anyKey)
	    {
	        _eventListener.SendMovement(transform.position.x, transform.position.y, transform.position.z,
                transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
	    }	
	}

    void SubscribeAreaOfInterest()
    {
        var aoi = GameObject.FindGameObjectWithTag("AreaOfInterest").GetComponent<SphereCollider>();

        aoi.OnCollisionEnterAsObservable().Subscribe(otherPlayer =>
        {
            if(otherPlayer.transform.CompareTag("OtherPlayer"))
            {
                var otherplayerId = otherPlayer.gameObject.GetComponent<Player>().Id;
                Debug.LogWarning(otherPlayer + " ID: " + otherplayerId + " --> Has Entered Area of Interest");
                _eventListener.SendMeetPlayer(otherplayerId);
            }
        });

        aoi.OnCollisionExitAsObservable().Subscribe(otherPlayer =>
        {
            if (otherPlayer.transform.CompareTag("OtherPlayer"))
            {
                var otherplayerId = otherPlayer.gameObject.GetComponent<Player>().Id;
                Debug.LogWarning(otherPlayer + " ID: " + otherplayerId + " --> Has Exit Area of Interest");
                _eventListener.SendMeetPlayer(otherplayerId);
            }
        });

        aoi.OnTriggerEnterAsObservable().Subscribe(otherPlayer =>
        {
            if (otherPlayer.CompareTag("OtherPlayer"))
            {
                var otherplayerId = otherPlayer.gameObject.GetComponent<Player>().Id;
                Debug.LogWarning(otherPlayer + " ID: " + otherplayerId + " --> Has Entered Area of Interest");
                _eventListener.SendMeetPlayer(otherplayerId);
            }
        });

        aoi.OnTriggerExitAsObservable().Subscribe(otherPlayer =>
        {
            if (otherPlayer.CompareTag("OtherPlayer"))
            {
                var otherplayerId = otherPlayer.gameObject.GetComponent<Player>().Id;
                Debug.LogWarning(otherPlayer + " ID: " + otherplayerId + " --> Has Exit Area of Interest");
                _eventListener.SendMeetPlayer(otherplayerId);
            }
        });
    }
}
