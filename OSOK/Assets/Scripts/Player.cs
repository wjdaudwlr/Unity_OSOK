using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks
{
    public PhotonView PV; // 포톤뷰

    public GameObject bullet;
    private SpriteRenderer renderer;
    public float speed;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();

        renderer.color = PV.IsMine ? Color.green : Color.red;
    }

    private void Update()
    {
        if (PV.IsMine)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            transform.Translate(h * speed * Time.deltaTime, v * speed * Time.deltaTime, 0);


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
    }
}
