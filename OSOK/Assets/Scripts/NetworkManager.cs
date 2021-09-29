using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("DisconnectPanel")]
    public GameObject DisconnectPanel;
    public InputField NickNameInput;

    [Header("LobbyPanel")]
    public GameObject LobbyPanel;
    public InputField RoomInput;
    public Text WelcomeText;
    public Text LobbyInfoText;
    public Button[] CellBtn;
    public Button PreviousBtn;
    public Button NextBtn;

    [Header("RoomPanel")]
    public GameObject RoomPanel;
    public Text ListText;
    public Text RoomInfoText;
    public Text[] ChatText;
    public InputField ChatInput;

    [Header("ETC")]
    public Text StatusText;
    public PhotonView PV;
    public Transform[] SpawnPosints;
    public GameObject GameEndPanel;
    public GameObject GameWinText;
    public GameObject GameOverText;
    public Player player;
    public AudioSource audio;
    public GameObject gameStartBtn;
    public Text winnerPlayerText;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;
    public int playerNum;
    public bool isLive;
    bool playGame;
    public bool test = false;

    #region �渮��Ʈ ����
    public void MyListClick(int num)
    {
        // ����ư -2 ����ư -1 , �� ����
        if (num == -2) --currentPage; // ����ư
        else if (num == -1) ++currentPage; // ����ư
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name); // �� �����ϱ� 

        MyListRenewal();
    }

    void MyListRenewal()
    {
        // �ִ�������
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // ����, ������ư
        PreviousBtn.interactable = (currentPage <= 1) ? false : true; // ���� �������� 1 �������� ��ư ��Ȱ��ȭ
        NextBtn.interactable = (currentPage >= maxPage) ? false : true; // ���� �������� max�������� ��ư ��Ȱ��ȭ

        // �������� �´� ����Ʈ ����
        multiple = (currentPage - 1) * CellBtn.Length;
        for(int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple].MaxPlayers : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for(int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }

    #endregion


    #region ��������
    void Awake() { 
        Screen.SetResolution(1920, 1080, true); // �ػ�, Ǯ��ũ��
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "�κ� /" + PhotonNetwork.CountOfPlayers + "����";
        // �κ� �� : (������ �÷��̾�� - ��ȿ� �ִ� �÷��̾��) ���� �� : ������ �÷���� ��
        if (playerNum == 1 && !test)
        {
            StartCoroutine(BackToRoom());
            GameEndPanel.SetActive(true);
            if (isLive)
            {
                GameWinText.SetActive(true);
                GameOverText.SetActive(false); // ������ �������� ���â ����
            }
            else
            {
                GameWinText.SetActive(false);
                GameOverText.SetActive(true);
            }
        }
    }


    public void Connect() => PhotonNetwork.ConnectUsingSettings(); 

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text; // �г��� ����
        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "�� ȯ���մϴ�."; 
        myList.Clear();
    }
    public void Disconnect() => PhotonNetwork.Disconnect(); // �������

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (LobbyPanel.activeSelf == true || RoomPanel.activeSelf == true)
        {
            LobbyPanel.SetActive(false);
            RoomPanel.SetActive(false);
        }
    }

    #endregion


    #region ��
    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 4 });
    // ���� ���� �� �̸��� �Է����� ������ �������� �ƴϸ� �Էµ� ���̸�

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom(); // �������� ������

    public void LeaveRoom() => PhotonNetwork.LeaveRoom(); // �� ����

    public override void OnJoinedRoom() // �濡 ���� ��
    {
        RoomPanel.SetActive(true); // ���ѱ�
        RoomRenewal();
        ChatInput.text = ""; // ä�� �Է�â �ʱ�ȭ
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = ""; // ä��â �ʱ�ȭ
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }
    // ���� ����ų� ���������� ���н� ���̸��� �ʱ�ȭ�ϰ� �����
    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) //  �÷��̾� ����
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) //  �÷��̾� ����
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }

    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            // ����Ʈ �ؽ�Ʈ�� �÷��̾� �г��� �߰� ���� ������ �÷��̾��� ����
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        // ���̸��� ���� �濡 �ִ� �÷��̾� ���� �ִ� �÷��̾�� �ʱ�ȭ
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "�� / " + PhotonNetwork.CurrentRoom.MaxPlayers + "�ִ�";

    }
    #endregion


    #region ä��

    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text); // ä�� ������ RPC�� ��ο���
        ChatInput.text = ""; // �Է�â �ʱ�ȭ
    }

    [PunRPC] // RPC�� �÷��̾ �����ִ� �� ��� �ο����� �����Ѵ�
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i< ChatText.Length; i++)
        {
            if(ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        }
        if (!isInput)
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }

    }


    #endregion


    public void GameStart() // ���� ����
    {
        PV.RPC("GameStartBtnOff", RpcTarget.AllBuffered);
        PV.RPC("Spawn", RpcTarget.All); 
    }

 

    IEnumerator BackToRoom() // �ڷ�ƾ�� ����� ������ ������ 2�ʵ� ��� ����ڰ� ������ ���ư�
    {
        yield return null;
        playerNum = 0;
        yield return new WaitForSeconds(2f);
        PV.RPC("RoomBack", RpcTarget.All);

    }

    [PunRPC] // �÷��̾ ������ ��ǥ�� �����ϰ� ������ �÷��̾� ���ڸ� �÷��̾� ����Ʈ ���̷� �ʱ�ȭ
    public void Spawn()
    {
        PhotonNetwork.Instantiate("Player", new Vector2(Random.Range(-10,10), Random.Range(-6.8f,6.5f)) , Quaternion.identity);
        DisconnectPanel.SetActive(false);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(false);
        playerNum = PhotonNetwork.PlayerList.Length;
        isLive = true;
        audio.Play();
       
    }
    [PunRPC]
    public void GameStartBtnOff() => gameStartBtn.SetActive(false);

    [PunRPC] // RPC�� ����� ��� �÷��̾ ������ ���ư�����
    void RoomBack()
    {
        GameEndPanel.SetActive(false);
        DisconnectPanel.SetActive(true);
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(true);
        gameStartBtn.SetActive(true);
        RoomRenewal();
        ChatInput.text = ""; // ä�� �Է�â �ʱ�ȭ
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = ""; // ä��â �ʱ�ȭ
        playerNum = 0;
        audio.Stop();
    }
       
    public void ChampChoice(int num)
    {
        switch (num)
        {
            case 0:
                player.champ = Player.Champion.Gunner;
                break;
            case 1:
                player.champ = Player.Champion.Warrior;
                break;
        }
    }




}
