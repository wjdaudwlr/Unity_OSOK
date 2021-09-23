using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class Grenade : MonoBehaviourPunCallbacks
{
    Rigidbody2D rigid;

    public GameObject effect;
    public PhotonView PV;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        StartCoroutine(Boom());

        Vector2 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

        rigid.AddForce(dir, ForceMode2D.Impulse);
    }

    IEnumerator Boom()
    {
        yield return new WaitForSeconds(0.5f);

        rigid.velocity /= 2;

        yield return new WaitForSeconds(0.5f);

        rigid.velocity = Vector2.zero;
        effect.SetActive(true);

        RaycastHit2D[] rayHit = Physics2D.CircleCastAll(transform.position, 2f, Vector2.zero, 0f, LayerMask.GetMask("Player"));

        foreach (RaycastHit2D hitObj in rayHit)
        {
            if (hitObj.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                hitObj.collider.GetComponent<Player>().Hit();
            }
        }

        yield return new WaitForSeconds(0.25f);
        PV.RPC("DestroyRPC", RpcTarget.All);
    }

    [PunRPC]
    public void DestroyRPC() => Destroy(gameObject);

}
