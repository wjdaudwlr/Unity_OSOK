using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolTiem : MonoBehaviour
{
    public Player player;

    public Image skillFilter1;
    public Image skillFilter2;
    //public Text coolTimeCounter; //남은 쿨타임을 표시할 텍스트

    public float[] skill1CoolTiem;
    public float[] skill2CoolTiem;

    float[] skill1CurTime;
    float[] skill2CurTime; //남은 쿨타임을 추적 할 변수

    public bool canUseSkill1 = true; //스킬을 사용할 수 있는지 확인하는 변수
    public bool canUseSkill2 = true;

    void Start()
    {
        skillFilter1.fillAmount = 0; //처음에 스킬 버튼을 가리지 않음
        skillFilter2.fillAmount = 0; //처음에 스킬 버튼을 가리지 않음

        skill1CurTime = new float[skill1CoolTiem.Length];
        skill2CurTime = new float[skill2CoolTiem.Length];
    }

    public void UseSkill1(int champ)
    {
        if (canUseSkill1)
        {
            Debug.Log("Use Skill");
            skillFilter1.fillAmount = 1; //스킬 버튼을 가림
            StartCoroutine("Cooltime1");

            skill1CurTime[champ] = skill1CoolTiem[champ];
            //coolTimeCounter.text = "" + skill1CoolTiem[(int)player.champ]; ;

            StartCoroutine("CoolTimeCounter1");

            canUseSkill1 = false; //스킬을 사용하면 사용할 수 없는 상태로 바꿈
        }
        else
        {
            Debug.Log("아직 스킬을 사용할 수 없습니다.");
        }
    }

    public void UseSkill2(int champ)
    {
        if (canUseSkill2)
        {
            Debug.Log("Use Skill");
            skillFilter2.fillAmount = 1; //스킬 버튼을 가림
            StartCoroutine("Cooltime2");

            skill2CurTime[champ] = skill2CoolTiem[champ];
            //coolTimeCounter.text = "" + skill1CoolTiem[(int)player.champ]; ;

            StartCoroutine("CoolTimeCounter2");

            canUseSkill2 = false; //스킬을 사용하면 사용할 수 없는 상태로 바꿈
        }
        else
        {
            Debug.Log("아직 스킬을 사용할 수 없습니다.");
        }
    }


    IEnumerator Cooltime1()
    {
        while (skillFilter1.fillAmount > 0)
        {
            skillFilter1.fillAmount -= 1 * Time.smoothDeltaTime / skill1CoolTiem[(int)player.champ]; 

            yield return null;
        }

        canUseSkill1 = true; //스킬 쿨타임이 끝나면 스킬을 사용할 수 있는 상태로 바꿈

        yield break;
    }

    //남은 쿨타임을 계산할 코르틴을 만들어줍니다.
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

        canUseSkill2 = true; //스킬 쿨타임이 끝나면 스킬을 사용할 수 있는 상태로 바꿈

        yield break;
    }

    //남은 쿨타임을 계산할 코르틴을 만들어줍니다.
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





