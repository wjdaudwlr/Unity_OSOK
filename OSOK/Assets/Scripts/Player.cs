using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView PV; // 포톤뷰

    public Bullet bullet;
    public GameObject Attack;
    private SpriteRenderer renderer;
    Rigidbody2D rigid;

    public float speed;

    Vector3 curPos;

    bool aDown;
    bool sDown1;
    bool sDown2;

    bool isSkill1;

    public float[] skill1CoolTiem;
    public float[] skill2CoolTiem;

    float[] skill1CurTime;
    float[] skill2CurTime;
    
    public enum Champion // 챔프
    {
        Gunner,
        Warrior
    };

    public Champion champ;

    void Awake()
    {

        renderer = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();

        renderer.color = PV.IsMine ? Color.green : Color.red;

        if (PV.IsMine)
        {
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>(); // 카메라 따라다니기
            CM.Follow = transform;
            CM.LookAt = transform;
        }


        if (PV.IsMine) if (champ == Champion.Warrior) PV.RPC("WarriorAttackOn", RpcTarget.AllBuffered);

        skill1CurTime = new float[skill1CoolTiem.Length];
        skill2CurTime = new float[skill2CoolTiem.Length];
    }






    void Update()
    {
        if (PV.IsMine)
        {
            // 이동 
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            rigid.velocity = new Vector2(h * speed, v * speed);

            aDown = Input.GetMouseButtonDown(0);
            sDown1 = Input.GetMouseButtonDown(1);
            sDown2 = Input.GetButtonDown("Skill2");

            // 카메라 스크린의 마우스 거리와 총과의 방향
            Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            // 마우스 거리로 부터 각도 계산
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // 축으로부터 방향과 각도의 회전값
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = rotation;

            skill1CurTime[(int)champ] += Time.deltaTime; // 스킬 쿨타임

            switch (champ) 
            {
                case Champion.Gunner: // ---거너---

                    if(aDown && !isSkill1) // 기본공격
                        PhotonNetwork.Instantiate("Bullet", transform.position, Quaternion.AngleAxis(angle - 90, Vector3.forward));
                    else if (sDown1 && skill1CurTime[(int)champ] > skill1CoolTiem[(int)champ]) // 스나이퍼
                    {
                        StartCoroutine(SuperBullet(angle));
                        skill1CurTime[(int)champ] = 0;
                    }


                    break;   

                case Champion.Warrior: // ---워리어---


                    if (sDown1 && skill1CurTime[(int)champ] > skill1CoolTiem[(int)champ]) // 워리어 스킬1 : 대쉬
                    {
                        StartCoroutine(Dash());
                        skill1CurTime[(int)champ] = 0;
                    }

                    break;
            } // 챔프


        }
        // IsMine이 아닌 것들은 부드럽게 위치동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }


    public void Hit()
    {
        PV.RPC("DieRPC",RpcTarget.AllBuffered);
        PhotonNetwork.Disconnect();
    }

    [PunRPC]
    public void DiePC() { 

        Destroy(gameObject); 

    }

    [PunRPC]
    public void WarriorAttackOn() => Attack.SetActive(true);

    IEnumerator Dash() 
    {
        speed = 20f;
        
        yield return new WaitForSeconds(0.15f);

        speed = 6f;
    }

    IEnumerator SuperBullet(float angle)
    {
        isSkill1 = true;
        bullet.speed = 65;
        speed = 0;


        yield return new WaitForSeconds(1.2f);


        PhotonNetwork.Instantiate("Bullet", transform.position, Quaternion.AngleAxis(angle - 90, Vector3.forward));
        speed = 6;
        bullet.speed = 11;
        isSkill1 = false;
    }



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
