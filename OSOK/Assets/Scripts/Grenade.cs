using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class Grenade : MonoBehaviourPunCallbacks
{
    Rigidbody2D rigid;

    public CircleCollider2D collider;

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

    private void Update()
    {
        
    }

    IEnumerator Boom()
    {
        yield return new WaitForSeconds(1f);

        rigid.velocity /= 2;

        yield return new WaitForSeconds(1f);

        rigid.velocity = Vector2.zero;
        effect.SetActive(true);
        collider.enabled = true;

       

        RaycastHit2D[] rayHit = Physics2D.CircleCastAll(transform.position, 6, Vector2.up, 0f, LayerMask.GetMask("Player"));

        foreach (RaycastHit2D hitObj in rayHit)
        {
            Debug.Log("¾ÆÀ¸¾ß");
            if (hitObj.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                hitObj.collider.GetComponent<Player>().Hit();
            }
        }

        yield return new WaitForSeconds(0.25f);
        collider.enabled = false;
        Destroy(gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && collision.GetComponent<PhotonView>().IsMine && !PV.IsMine)
        {
            collision.GetComponent<Player>().Hit();

            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }
}
