using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView PV; // �����

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
    
    public enum Champion // è��
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
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>(); // ī�޶� ����ٴϱ�
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
            // �̵� 
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            rigid.velocity = new Vector2(h * speed, v * speed);

            aDown = Input.GetMouseButtonDown(0);
            sDown1 = Input.GetMouseButtonDown(1);
            sDown2 = Input.GetButtonDown("Skill2");

            // ī�޶� ��ũ���� ���콺 �Ÿ��� �Ѱ��� ����
            Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            // ���콺 �Ÿ��� ���� ���� ���
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // �����κ��� ����� ������ ȸ����
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = rotation;

            skill1CurTime[(int)champ] += Time.deltaTime; // ��ų ��Ÿ��

            switch (champ) 
            {
                case Champion.Gunner: // ---�ų�---

                    if(aDown && !isSkill1) // �⺻����
                        PhotonNetwork.Instantiate("Bullet", transform.position, Quaternion.AngleAxis(angle - 90, Vector3.forward));
                    else if (sDown1 && skill1CurTime[(int)champ] > skill1CoolTiem[(int)champ]) // ��������
                    {
                        StartCoroutine(SuperBullet(angle));
                        skill1CurTime[(int)champ] = 0;
                    }


                    break;   

                case Champion.Warrior: // ---������---


                    if (sDown1 && skill1CurTime[(int)champ] > skill1CoolTiem[(int)champ]) // ������ ��ų1 : �뽬
                    {
                        StartCoroutine(Dash());
                        skill1CurTime[(int)champ] = 0;
                    }

                    break;
            } // è��


        }
        // IsMine�� �ƴ� �͵��� �ε巴�� ��ġ����ȭ
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
