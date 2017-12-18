using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clients : MonoBehaviour
{

    public GameObject NetworkPlayerPrefab;

    private Dictionary<string, GameObject> NetworkPlayers;

	// Use this for initialization
	void Start () {
		NetworkPlayers = new Dictionary<string, GameObject>();
	}

    public void AddPlayer(string id, Vector3 pos, Quaternion rot)
    {
        var nPlayerGo = Instantiate(NetworkPlayerPrefab, pos, rot);
        // Spawn network player
        nPlayerGo.GetComponent<Player>().Id = id;
        NetworkPlayers.Add(id, nPlayerGo);
    }

    public void RemovePlayer(string id)
    {
        NetworkPlayers.Remove(id);
    }

    public void MovePlayer(string id, Vector3 pos, Quaternion rot)
    {
        var nplayer = NetworkPlayers[id];
        nplayer.transform.position = pos;
        nplayer.transform.rotation = rot;
    }
}
