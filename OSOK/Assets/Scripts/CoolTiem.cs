using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolTiem : MonoBehaviour
{
    public Player player;

    public Image skillFilter1;
    public Image skillFilter2;
    //public Text coolTimeCounter; //���� ��Ÿ���� ǥ���� �ؽ�Ʈ

    public float[] skill1CoolTiem;
    public float[] skill2CoolTiem;

    float[] skill1CurTime;
    float[] skill2CurTime; //���� ��Ÿ���� ���� �� ����

    public bool canUseSkill1 = true; //��ų�� ����� �� �ִ��� Ȯ���ϴ� ����
    public bool canUseSkill2 = true;

    void Start()
    {
        skillFilter1.fillAmount = 0; //ó���� ��ų ��ư�� ������ ����
        skillFilter2.fillAmount = 0; //ó���� ��ų ��ư�� ������ ����

        skill1CurTime = new float[skill1CoolTiem.Length];
        skill2CurTime = new float[skill2CoolTiem.Length];
    }

    public void UseSkill1(int champ)
    {
        if (canUseSkill1)
        {
            Debug.Log("Use Skill");
            skillFilter1.fillAmount = 1; //��ų ��ư�� ����
            StartCoroutine("Cooltime1");

            skill1CurTime[champ] = skill1CoolTiem[champ];
            //coolTimeCounter.text = "" + skill1CoolTiem[(int)player.champ]; ;

            StartCoroutine("CoolTimeCounter1");

            canUseSkill1 = false; //��ų�� ����ϸ� ����� �� ���� ���·� �ٲ�
        }
        else
        {
            Debug.Log("���� ��ų�� ����� �� �����ϴ�.");
        }
    }

    public void UseSkill2(int champ)
    {
        if (canUseSkill2)
        {
            Debug.Log("Use Skill");
            skillFilter2.fillAmount = 1; //��ų ��ư�� ����
            StartCoroutine("Cooltime2");

            skill2CurTime[champ] = skill2CoolTiem[champ];
            //coolTimeCounter.text = "" + skill1CoolTiem[(int)player.champ]; ;

            StartCoroutine("CoolTimeCounter2");

            canUseSkill2 = false; //��ų�� ����ϸ� ����� �� ���� ���·� �ٲ�
        }
        else
        {
            Debug.Log("���� ��ų�� ����� �� �����ϴ�.");
        }
    }


    IEnumerator Cooltime1()
    {
        while (skillFilter1.fillAmount > 0)
        {
            skillFilter1.fillAmount -= 1 * Time.smoothDeltaTime / skill1CoolTiem[(int)player.champ]; 

            yield return null;
        }

        canUseSkill1 = true; //��ų ��Ÿ���� ������ ��ų�� ����� �� �ִ� ���·� �ٲ�

        yield break;
    }

    //���� ��Ÿ���� ����� �ڸ�ƾ�� ������ݴϴ�.
    IEnumerator CoolTimeCounter1()
    {
        while (skill1CurTime[(int)player.champ] > 0)
        {
            yield return new WaitForSeconds(1.0f);

            skill1CurTime[(int)player.champ] -= 1.0f;
            //coolTimeCounter.text = "" + skill1CurTime[(int)player.champ];
        }

        yield break;
    }



    // ----------------------------

    IEnumerator Cooltime2()
    {
        while (skillFilter2.fillAmount > 0)
        {
            skillFilter2.fillAmount -= 1 * Time.smoothDeltaTime / skill2CoolTiem[(int)player.champ];

            yield return null;
        }

        canUseSkill2 = true; //��ų ��Ÿ���� ������ ��ų�� ����� �� �ִ� ���·� �ٲ�

        yield break;
    }

    //���� ��Ÿ���� ����� �ڸ�ƾ�� ������ݴϴ�.
    IEnumerator CoolTimeCounter2()
    {
        while (skill2CurTime[(int)player.champ] > 0)
        {
            yield return new WaitForSeconds(1.0f);

            skill2CurTime[(int)player.champ] -= 1.0f;
            //coolTimeCounter.text = "" + skill1CurTime[(int)player.champ];
        }

        yield break;
    }

}





