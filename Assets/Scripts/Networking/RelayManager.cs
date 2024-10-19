using System.Collections;
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

public class RelayManager : NetworkBehaviour
{
    [HideInInspector] public string JoinCode;
    [HideInInspector] public ulong player2ClientId;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private async void Start()
    {
        try
        {
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
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            JoinCode = joinCode.ToUpper();
            Debug.Log("Join Code: " + joinCode);
            DebugMenu.Instance.WriteToDebugMenu(DebugMenu.DebugSection.JoinCode, joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        string output = sceneName + " loaded in mode " + loadSceneMode + " for clients: ";
        foreach (ulong id in clientsCompleted)
        {
            output += id + ", ";
        }

        Debug.Log(output);
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        player2ClientId = clientId;
        MoveToGameScene();
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

            JoinCode = joinCode.ToUpper();
            DebugMenu.Instance.WriteToDebugMenu(DebugMenu.DebugSection.JoinCode, JoinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private void MoveToGameScene()
    {
        if (NetworkManager.Singleton.IsServer) NetworkManager.Singleton.SceneManager.LoadScene("GameBoard", LoadSceneMode.Single);
    }

    public void HostGame()
    {
        CreateRelay();
    }

    public void JoinGame(TMPro.TMP_InputField inf)
    {
        JoinRelay(inf.text);
    }
}

