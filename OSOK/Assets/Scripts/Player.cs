using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView PV; // 포톤뷰

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
            // 이동 
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            rigid.velocity = new Vector2(h * speed, v * speed);

            // 카메라 스크린의 마우스 거리와 총과의 방향
            Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            // 마우스 거리로 부터 각도 계산
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // 축으로부터 방향과 각도의 회전값
            //Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            //transform.rotation = rotation;

            if (Input.GetMouseButtonDown(0))
            {
                PhotonNetwork.Instantiate("Bullet",transform.position,Quaternion.AngleAxis(angle - 90,Vector3.forward));
            }

        }
        // IsMine이 아닌 것들은 부드럽게 위치동기화
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
