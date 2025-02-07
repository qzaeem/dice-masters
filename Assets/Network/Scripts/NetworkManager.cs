using System;
using System.Collections.Generic;
using DiceGame.ScriptableObjects;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Events;

namespace DiceGame.Network
{
    public class NetworkManager : Models.MonoBehaviourSingleton<NetworkManager>, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkRunner networkRunnerPrefab;
        [SerializeField] private GameModeVariable currentGameMode;
        [SerializeField] private int gameSceneIndex;
        private NetworkRunner _networkRunner;
        private NetworkEvents _networkEvents;
        public string NewRoomKey;
        public int PlayerCount;

        #region Callback Actions
        public Action<PlayerRef> onPlayerJoined;
        #endregion

        //Arguments added roomkey and player count
        public async void StartGame(string roomKey, int playerCount, GameMode gameMode)
        {
            _networkRunner = Instantiate(networkRunnerPrefab); 
            _networkEvents = _networkRunner.GetComponent<NetworkEvents>();
            // Add listeners
            AddListeners();

            NewRoomKey = roomKey;
            PlayerCount = playerCount;
            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(SceneRef.FromIndex(1));

            var startArguments = new StartGameArgs()
            {
                //GameMode = currentGameMode.value.isMultiplayer ? GameMode.Shared : GameMode.Single,
                GameMode = gameMode,
                //SessionName = "dice",
                SessionName = roomKey,
                //PlayerCount = playerCount,
                // We need to specify a session property for matchmaking to decide where the player wants to join.
                // Otherwise players from Platformer scene could connect to ThirdPersonCharacter game etc.
                //SessionProperties = new Dictionary<string, SessionProperty> { ["GameMode"] = GameModeIdentifier },
                Scene = sceneInfo,
            };
            // Exclude PlayerCount if game mode is Client
            if (gameMode != GameMode.Client && gameMode!= GameMode.AutoHostOrClient)
            {
                startArguments.PlayerCount = playerCount;
            }
            else
            {
                startArguments.Scene = null; // Clients should not override the scene
            }

            //var startTask = _networkRunner.StartGame(startArguments);
            //await startTask;
            var startTask = await _networkRunner.StartGame(startArguments);
            if (startTask.Ok)
            {
                Debug.Log($"Game Started: ");
            }
            else
            {
                Debug.LogError($"Failed to start game : {startTask.ShutdownReason}");
            }
        }

        private void AddListeners()
        {
            _networkRunner.AddCallbacks(this);
            AddShutdownListener(OnShutdown);
            AddConnectedToServerListener(OnConnectedToServer);
            AddDisconnectedFromServerListener(OnDisconnectedFromServer);
            AddFailedToConnectListener(OnConnectionFailed);
        }
        //Set Scene Index
        public void SetGameSceneIndex(int index)
        {
            gameSceneIndex = index;
        }

        #region NetworkEventCallbacks
        private void OnShutdown(NetworkRunner runner, ShutdownReason reason)
        {
            Debug.Log("Shutdown!");
        }

        private void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("Connected to Server!");
        }

        private void OnDisconnectedFromServer(NetworkRunner runner, Fusion.Sockets.NetDisconnectReason reason)
        {
            Debug.Log("Disconnected from Server!");
        }

        private void OnConnectionFailed(NetworkRunner runner, Fusion.Sockets.NetAddress netAddress, Fusion.Sockets.NetConnectFailedReason reason)
        {
            Debug.Log("Connection to server failed!");
        }
        #endregion

        #region AddListeners
        public void AddShutdownListener(UnityAction<NetworkRunner, ShutdownReason> call)
        {
            if (_networkEvents != null)
            {
                _networkEvents.OnShutdown.AddListener(call);
            }
        }

        public void AddConnectedToServerListener(UnityAction<NetworkRunner> call)
        {
            if (_networkEvents != null)
            {
                _networkEvents.OnConnectedToServer.AddListener(call);
            }
        }

        public void AddDisconnectedFromServerListener(UnityAction<NetworkRunner, Fusion.Sockets.NetDisconnectReason> call)
        {
            if (_networkEvents != null)
            {
                _networkEvents.OnDisconnectedFromServer.AddListener(call);
            }
        }

        public void AddFailedToConnectListener(UnityAction<NetworkRunner, Fusion.Sockets.NetAddress, Fusion.Sockets.NetConnectFailedReason> call)
        {
            if (_networkEvents != null)
            {
                _networkEvents.OnConnectFailed.AddListener(call);
            }
        }
        #endregion

        #region RemoveListeners
        public void RemoveShutdownListener(UnityAction<NetworkRunner, ShutdownReason> call)
        {
            if (_networkEvents != null)
            {
                _networkEvents.OnShutdown.RemoveListener(call);
            }
        }

        public void RemoveConnectedToServerListener(UnityAction<NetworkRunner> call)
        {
            if (_networkEvents != null)
            {
                _networkEvents.OnConnectedToServer.RemoveListener(call);
            }
        }

        public void RemoveDisconnectedFromServerListener(UnityAction<NetworkRunner, Fusion.Sockets.NetDisconnectReason> call)
        {
            if (_networkEvents != null)
            {
                _networkEvents.OnDisconnectedFromServer.RemoveListener(call);
            }
        }

        public void RemoveFailedToConnectListener(UnityAction<NetworkRunner, Fusion.Sockets.NetAddress, Fusion.Sockets.NetConnectFailedReason> call)
        {
            if (_networkEvents != null)
            {
                _networkEvents.OnConnectFailed.RemoveListener(call);
            }
        }

        #endregion

        #region Runner Callbacks

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {

        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {

        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            onPlayerJoined?.Invoke(player);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {

        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {

        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {

        }

        void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {

        }

        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
        {

        }

        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {

        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {

        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {

        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {

        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {

        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {

        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {

        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {

        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {

        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {

        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {

        }
        #endregion
    }
}
