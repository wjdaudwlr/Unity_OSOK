using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        Screen.SetResolution(960, 540,false);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, null);

    public override void OnJoinedRoom() {
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
    }
}
