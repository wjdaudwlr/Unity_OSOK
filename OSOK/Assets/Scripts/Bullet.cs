using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView PV;


    public int speed;

    void Start()
    {
        Destroy(gameObject, 3.0f);
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Box")
        {
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
        else if(collision.gameObject.tag == "Player" && collision.GetComponent<PhotonView>().IsMine && !PV.IsMine)
        {
            collision.GetComponent<Player>().Hit();

            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }
    

    [PunRPC]
    public void DestroyRPC() => Destroy(gameObject);



}
