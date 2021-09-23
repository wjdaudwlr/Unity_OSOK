using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView PV; // 포톤뷰

    public Bullet bullet;
    public GameObject Attack;
    SpriteRenderer renderer;
    Rigidbody2D rigid;
    NetworkManager NM ;
    CoolTiem coolTime;

    public float speed;

    Vector3 curPos;

    bool aDown;
    bool sDown1;
    bool sDown2;

    bool isSkill1;

    public float[] attackCoolTiem;
  

    float[] attackCurTime;

    public AudioSource audio;

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
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        coolTime = GameObject.Find("Skill1Btn").GetComponent<CoolTiem>();

        renderer.color = PV.IsMine ? Color.green : Color.red; // 상대는 빨간 나는 초록

        if (PV.IsMine) // 카메라 따라다니기
        {
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>(); 
            CM.Follow = transform;
            CM.LookAt = transform;
        }

        if (PV.IsMine) if (champ == Champion.Warrior) PV.RPC("WarriorAttackOn", RpcTarget.AllBuffered); // 워리어면 무기 생성

      
        attackCurTime = new float[attackCoolTiem.Length];


    }
    

    void Update()
    {
        if (PV.IsMine)
        {
            if(NM.playerNum == 1 && !NM.test) // 게임이 끝나면 플레이어를 없앰
            {
                Hit();
            }

            // 이동 
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            rigid.velocity = new Vector2(h * speed, v * speed);

            // 스킬 인풋 (마우스 좌,우클릭, E)
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

            attackCurTime[(int)champ] += Time.deltaTime; // 기본공격 쿨타임

            switch (champ) 
            {
                case Champion.Gunner: // ---거너---

                    if (aDown && !isSkill1 && attackCurTime[(int)champ] > attackCoolTiem[(int)champ])
                    {   // 기본공격
                        PhotonNetwork.Instantiate("Bullet", transform.position, Quaternion.AngleAxis(angle - 90, Vector3.forward));
                        attackCurTime[(int)champ] = 0;
                    }
                    else if (sDown1 && coolTime.canUseSkill1) 
                    {
                        // 거너 스킬1  : 슈퍼총알(일정시간 멈추고 지정한 방향에 빠른속도로 총알을 날림)
                        StartCoroutine(SuperBullet(angle));
                        coolTime.UseSkill1((int)champ);
                    }
                    else if (sDown2 && coolTime.canUseSkill2)
                    {
                        PhotonNetwork.Instantiate("Grenade", transform.position, Quaternion.identity);
                        coolTime.UseSkill2((int)champ); 
                    }
                    break;   

                case Champion.Warrior: // ---워리어---

                    if (sDown1 && coolTime.canUseSkill1) 
                    {
                        // 워리어 스킬1 : 대쉬(순간적으로 이동속도를 극대화)
                        audio.Play();
                        StartCoroutine(Dash());
                        coolTime.UseSkill1((int)champ);
                    }

                    break;
            } // 챔프

        }
        // IsMine이 아닌 것들은 부드럽게 위치동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }


    public void Hit() // 플레이어 히트
    {

        PhotonNetwork.Instantiate("DeathEffect", gameObject.transform.position, Quaternion.identity);
        NM.isLive = false; // 죽음
        PV.RPC("DieRPC", RpcTarget.All);
    }

    [PunRPC] // RPC를 사용해서 모든 사용자에게서 삭제
    public void DieRPC() { // 플레이어 죽음

        NM.playerNum--; // 생존 수를 줄인다
        Destroy(gameObject); // 오브젝트 삭제
    }


    [PunRPC] // RPC를 사용해서 모든 사용자에게 보이게
    public void WarriorAttackOn() => Attack.SetActive(true); // 워리어를 고르면 칼 SetActive(true)

    IEnumerator Dash() // 대쉬
    {
        speed = 20f;
        
        yield return new WaitForSeconds(0.15f);

        speed = 6f;
    }

    IEnumerator SuperBullet(float angle) // 슈퍼 총알
    {
        isSkill1 = true;
        bullet.speed = 33;
        speed = 0;

        yield return new WaitForSeconds(1f);

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
