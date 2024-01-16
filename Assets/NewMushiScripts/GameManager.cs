using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

using ExitGames.Client.Photon;


public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    private PhotonView photonView;

    public Text roomCodeText;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject gameplayPanel;

    ///
    public GameObject dummyPanel;

    public Button readyButton;
    private const int roomCodeLength = 6;
    private InputField roomCodeInput;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        roomCodeInput = GetComponent<InputField>();

        dummyPanel.SetActive(false);


    }

    private void Start()
    {
        ConnectToPhoton();
    }

    private void ConnectToPhoton()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        // Implement logic for UI, e.g., showing the main menu
    }

    public void CreateGame()
    {
        Debug.Log("Starting to join Room");
        string newRoomCode = GenerateRandomRoomCode(roomCodeLength);

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 3,
            CustomRoomProperties = new Hashtable
            {
                { "AutoSyncScene", true }
            },
            CustomRoomPropertiesForLobby = new[] { "AutoSyncScene" }
        };

        PhotonNetwork.CreateRoom(newRoomCode, roomOptions);
        PhotonNetwork.JoinRoom(newRoomCode);

        Debug.Log("Room Joined" + newRoomCode);
        roomPanel.SetActive(false);
        //to be deleted later
        dummyPanel.SetActive(true);
    }


    public void JoinGame()
    {
        string roomCode = roomCodeInput.text.ToUpper();
        PhotonNetwork.JoinRoom(roomCode);
    }

    public override void OnJoinedRoom()
    {
        roomCodeText.text = "Room Code: " + PhotonNetwork.CurrentRoom.Name;

        //if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        //{
            photonView.RPC("LoadGameplayScene", RpcTarget.AllBuffered);
        //}

    }

    public void Ready()
    {
        readyButton.interactable = false;
        photonView.RPC("PlayerReady", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void PlayerReady()
    {
        if (LobbyManager.Instance.AllPlayersReady())
        {
            photonView.RPC("LoadGameplayScene", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void LoadGameplayScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            lobbyPanel.SetActive(false);
            gameplayPanel.SetActive(true);
            PhotonNetwork.LoadLevel("GameplayScene");
        }
    }

    private string GenerateRandomRoomCode(int length)
    {
        StringBuilder roomCodeBuilder = new StringBuilder();
        const string roomCodeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        for (int i = 0; i < length; i++)
        {
            int index = Random.Range(0, roomCodeCharacters.Length);
            roomCodeBuilder.Append(roomCodeCharacters[index]);
        }

        return roomCodeBuilder.ToString();
    }
}