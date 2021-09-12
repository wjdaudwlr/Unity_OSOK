using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NickNameInput;
    public GameObject MainPanel;
    public GameObject RespawnPanel;
    public GameObject PickRoomPanel;
    public Player player;

    public int playerNum;


    private void Awake()
    {
        Screen.SetResolution(960, 540,false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }



    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 3 }, null);
    }

    public override void OnJoinedRoom() {
        PickRoomPanel.SetActive(false);
        Spawn();
        Debug.Log(PhotonNetwork.LocalPlayer.ActorNumber); // 플레이어 번호
    }

    public void GameStart()
    {
        MainPanel.SetActive(false);
        PickRoomPanel.SetActive(true);
    }

    public void GunnerBtn()
    {
        player.champ = Player.Champion.Gunner;
        Debug.Log("거너");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void WarriorBtn()
    {
        player.champ = Player.Champion.Warrior;
        Debug.Log("워리어");
        PhotonNetwork.ConnectUsingSettings();
    }

    void Spawn()
    {
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
    }



}
