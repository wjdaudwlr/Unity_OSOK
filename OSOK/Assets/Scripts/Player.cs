using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView PV; // �����

    public GameObject bullet;
    private SpriteRenderer renderer;
    Rigidbody2D rigid;
    public float speed;

    Vector3 curPos;

    void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();

        renderer.color = PV.IsMine ? Color.green : Color.red;
    }

    void Update()
    {
        if (PV.IsMine)
        {
            // �̵� 
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            rigid.velocity = new Vector2(h * speed, v * speed);

            // ī�޶� ��ũ���� ���콺 �Ÿ��� �Ѱ��� ����
            Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            // ���콺 �Ÿ��� ���� ���� ���
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // �����κ��� ����� ������ ȸ����
            //Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            //transform.rotation = rotation;

            if (Input.GetMouseButtonDown(0))
            {
                PhotonNetwork.Instantiate("Bullet",transform.position,Quaternion.AngleAxis(angle - 90,Vector3.forward));
            }

        }
        // IsMine�� �ƴ� �͵��� �ε巴�� ��ġ����ȭ
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    public void Hit()
    {
        PV.RPC("DestroyRPC",RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void DestroyRPC() => Destroy(gameObject);

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
        }
    }
}
