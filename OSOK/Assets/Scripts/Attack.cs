using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Attack : MonoBehaviourPunCallbacks
{

    public PhotonView PV;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<Player>().Hit();

        }
    }

   

}
