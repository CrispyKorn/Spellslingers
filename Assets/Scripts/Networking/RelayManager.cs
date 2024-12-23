using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class RelayManager : NetworkBehaviour
{
    public string JoinCode { get => _joinCode; }
    public ulong Player2ClientId { get => _player2ClientId; }

    private string _joinCode;
    private ulong _player2ClientId;

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);

        DontDestroyOnLoad(this);
    }

    private async void Start()
    {
        try
        {
            // Sign in to unity services
            await UnityServices.InitializeAsync();
            AuthenticationService.Instance.SignedIn += () => { Debug.Log("Signed In! ID: " + AuthenticationService.Instance.PlayerId); };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1); // Create allocation with one connection (player 2)
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); // Generate join code based on created allocation

            // Print join code
            _joinCode = joinCode.ToUpper();
            Debug.Log("Join Code: " + joinCode);
            Locator.Instance.DebugMenu.WriteToDebugMenu(DebugMenu.DebugSection.JoinCode, joinCode);

            // Create relay server and start hosting
            var relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            // Setup connection callbacks
            NetworkManager.Singleton.OnClientConnectedCallback += RegisterSecondPlayer;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OutputLoadedSceneClients;
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            // Join allocation (slot) using join code, and initialize relay connection as client
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode); 
            var relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

            // Print join code
            _joinCode = joinCode.ToUpper();
            Locator.Instance.DebugMenu.WriteToDebugMenu(DebugMenu.DebugSection.JoinCode, JoinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private void OutputLoadedSceneClients(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        var output = $"{sceneName} loaded in mode {loadSceneMode} for clients: ";
        foreach (ulong id in clientsCompleted)
        {
            output += $"{id}, ";
        }

        Debug.Log(output);
    }

    private void RegisterSecondPlayer(ulong clientId)
    {
        _player2ClientId = clientId;
        MoveToGameScene();
    }

    private void MoveToGameScene()
    {
        if (NetworkManager.Singleton.IsServer) NetworkManager.Singleton.SceneManager.LoadScene("GameBoard", LoadSceneMode.Single);
    }

    public void HostGame()
    {
        CreateRelay();
    }

    public void JoinGame(TMP_InputField inf)
    {
        JoinRelay(inf.text);
    }
}

