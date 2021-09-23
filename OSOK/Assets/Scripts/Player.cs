using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView PV; // �����

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
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        coolTime = GameObject.Find("Skill1Btn").GetComponent<CoolTiem>();

        renderer.color = PV.IsMine ? Color.green : Color.red; // ���� ���� ���� �ʷ�

        if (PV.IsMine) // ī�޶� ����ٴϱ�
        {
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>(); 
            CM.Follow = transform;
            CM.LookAt = transform;
        }

        if (PV.IsMine) if (champ == Champion.Warrior) PV.RPC("WarriorAttackOn", RpcTarget.AllBuffered); // ������� ���� ����

      
        attackCurTime = new float[attackCoolTiem.Length];


    }
    

    void Update()
    {
        if (PV.IsMine)
        {
            if(NM.playerNum == 1 && !NM.test) // ������ ������ �÷��̾ ����
            {
                Hit();
            }

            // �̵� 
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            rigid.velocity = new Vector2(h * speed, v * speed);

            // ��ų ��ǲ (���콺 ��,��Ŭ��, E)
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

            attackCurTime[(int)champ] += Time.deltaTime; // �⺻���� ��Ÿ��

            switch (champ) 
            {
                case Champion.Gunner: // ---�ų�---

                    if (aDown && !isSkill1 && attackCurTime[(int)champ] > attackCoolTiem[(int)champ])
                    {   // �⺻����
                        PhotonNetwork.Instantiate("Bullet", transform.position, Quaternion.AngleAxis(angle - 90, Vector3.forward));
                        attackCurTime[(int)champ] = 0;
                    }
                    else if (sDown1 && coolTime.canUseSkill1) 
                    {
                        // �ų� ��ų1  : �����Ѿ�(�����ð� ���߰� ������ ���⿡ �����ӵ��� �Ѿ��� ����)
                        StartCoroutine(SuperBullet(angle));
                        coolTime.UseSkill1((int)champ);
                    }
                    else if (sDown2 && coolTime.canUseSkill2)
                    {
                        PhotonNetwork.Instantiate("Grenade", transform.position, Quaternion.identity);
                        coolTime.UseSkill2((int)champ); 
                    }
                    break;   

                case Champion.Warrior: // ---������---

                    if (sDown1 && coolTime.canUseSkill1) 
                    {
                        // ������ ��ų1 : �뽬(���������� �̵��ӵ��� �ش�ȭ)
                        audio.Play();
                        StartCoroutine(Dash());
                        coolTime.UseSkill1((int)champ);
                    }

                    break;
            } // è��

        }
        // IsMine�� �ƴ� �͵��� �ε巴�� ��ġ����ȭ
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }


    public void Hit() // �÷��̾� ��Ʈ
    {

        PhotonNetwork.Instantiate("DeathEffect", gameObject.transform.position, Quaternion.identity);
        NM.isLive = false; // ����
        PV.RPC("DieRPC", RpcTarget.All);
    }

    [PunRPC] // RPC�� ����ؼ� ��� ����ڿ��Լ� ����
    public void DieRPC() { // �÷��̾� ����

        NM.playerNum--; // ���� ���� ���δ�
        Destroy(gameObject); // ������Ʈ ����
    }


    [PunRPC] // RPC�� ����ؼ� ��� ����ڿ��� ���̰�
    public void WarriorAttackOn() => Attack.SetActive(true); // ����� ���� Į SetActive(true)

    IEnumerator Dash() // �뽬
    {
        speed = 20f;
        
        yield return new WaitForSeconds(0.15f);

        speed = 6f;
    }

    IEnumerator SuperBullet(float angle) // ���� �Ѿ�
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
