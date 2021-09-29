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

    #region 방리스트 갱신
    public void MyListClick(int num)
    {
        // ◀버튼 -2 ▶버튼 -1 , 셀 숫자
        if (num == -2) --currentPage; // ◀버튼
        else if (num == -1) ++currentPage; // ▶버튼
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name); // 방 접속하기 

        MyListRenewal();
    }

    void MyListRenewal()
    {
        // 최대페이지
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // 이전, 다음버튼
        PreviousBtn.interactable = (currentPage <= 1) ? false : true; // 현재 페이지가 1 페이지면 버튼 비활성화
        NextBtn.interactable = (currentPage >= maxPage) ? false : true; // 현재 페이지가 max페이지면 버튼 비활성화

        // 페이지에 맞는 리시트 대입
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


    #region 서버연결
    void Awake() { 
        Screen.SetResolution(1920, 1080, true); // 해상도, 풀스크린
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 /" + PhotonNetwork.CountOfPlayers + "접속";
        // 로비 수 : (접속한 플레이어수 - 방안에 있는 플레이어수) 접속 수 : 접속한 플레어어 수
        if (playerNum == 1 && !test)
        {
            StartCoroutine(BackToRoom());
            GameEndPanel.SetActive(true);
            if (isLive)
            {
                GameWinText.SetActive(true);
                GameOverText.SetActive(false); // 게임이 끝났으면 결과창 띄우기
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
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text; // 닉네임 저장
        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다."; 
        myList.Clear();
    }
    public void Disconnect() => PhotonNetwork.Disconnect(); // 연결끊기

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (LobbyPanel.activeSelf == true || RoomPanel.activeSelf == true)
        {
            LobbyPanel.SetActive(false);
            RoomPanel.SetActive(false);
        }
    }

    #endregion


    #region 방
    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 4 });
    // 방을 만들 때 이름을 입력하지 않으면 랜덤지정 아니면 입력된 방이름

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom(); // 랜덤으로 방입장

    public void LeaveRoom() => PhotonNetwork.LeaveRoom(); // 방 삭제

    public override void OnJoinedRoom() // 방에 입장 시
    {
        RoomPanel.SetActive(true); // 방켜기
        RoomRenewal();
        ChatInput.text = ""; // 채팅 입력창 초기화
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = ""; // 채팅창 초기화
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }
    // 방을 만들거나 랜덤입장이 실패시 방이름을 초기화하고 방생성
    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) //  플레이어 입장
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) //  플레이어 퇴장
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            // 리스트 텍스트에 플레이어 닉네임 추가 만약 마지막 플레이어라면 공백
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        // 방이름과 지금 방에 있는 플레이어 수와 최대 플레이어수 초기화
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";

    }
    #endregion


    #region 채팅

    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text); // 채팅 보내기 RPC로 모두에게
        ChatInput.text = ""; // 입력창 초기화
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
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


    public void GameStart() // 게임 시작
    {
        PV.RPC("GameStartBtnOff", RpcTarget.AllBuffered);
        PV.RPC("Spawn", RpcTarget.All); 
    }

 

    IEnumerator BackToRoom() // 코루틴을 사용해 게임이 끝나면 2초뒤 모든 사용자가 방으로 돌아감
    {
        yield return null;
        playerNum = 0;
        yield return new WaitForSeconds(2f);
        PV.RPC("RoomBack", RpcTarget.All);

    }

    [PunRPC] // 플레이어를 랜덤한 좌표로 생성하고 생존한 플레이어 숫자를 플레이어 리스트 길이로 초기화
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

    [PunRPC] // RPC를 사용해 모든 플레이어를 룸으로 돌아가게함
    void RoomBack()
    {
        GameEndPanel.SetActive(false);
        DisconnectPanel.SetActive(true);
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(true);
        gameStartBtn.SetActive(true);
        RoomRenewal();
        ChatInput.text = ""; // 채팅 입력창 초기화
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = ""; // 채팅창 초기화
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
