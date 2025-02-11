using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiceGame.ScriptableObjects;
using Fusion;
using Fusion.Sockets;
using UnityEditor.Rendering;
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
        //private List<SessionInfo> _availableSessions = new ();

        #region Callback Actions
        public Action<PlayerRef> onPlayerJoined;
        #endregion

        //Arguments added roomkey and player count
        public async void CreateGame(string roomKey, int playerCount, GameMode gameMode)
        {
            _networkRunner = Instantiate(networkRunnerPrefab); 
            _networkEvents = _networkRunner.GetComponent<NetworkEvents>();
            // Add listeners
            AddListeners();
            //custom properties for session
            var customProperties = new Dictionary<string, SessionProperty>();
            customProperties["gameType"] = (int)currentGameMode.value.mode;
            customProperties["roomKey"] = roomKey;


            NewRoomKey = roomKey;
            PlayerCount = playerCount;
            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(SceneRef.FromIndex(1));

            var startArguments = new StartGameArgs()
            {
                GameMode = gameMode,
                SessionName = roomKey,
                PlayerCount = playerCount,
                Scene = sceneInfo,
                SessionProperties = customProperties
            };

            var startTask = await _networkRunner.StartGame(startArguments);

            if (startTask.Ok)
            {
                Debug.Log($"<color=green>Game Started with room key: {roomKey}");
            }
            else
            {
                Debug.LogError($"Failed to start game : {startTask.ShutdownReason}");
            }
        }

        public async void JoinGame(string roomKey)
        {
            if (string.IsNullOrEmpty(roomKey))
            {
                Debug.LogError("Invalid Room Key. Cannot join.");
                return;
            }

            _networkRunner = Instantiate(networkRunnerPrefab);
            _networkEvents = _networkRunner.GetComponent<NetworkEvents>();
            // Add listeners
            AddListeners();

            var startArguments = new StartGameArgs
            {
                GameMode = GameMode.Shared, 
                SessionName = roomKey,
                Scene = null // Clients should not override the scene
            };

            var startTask = await _networkRunner.StartGame(startArguments);

            if (startTask.Ok)
            {
                Debug.Log($"Successfully joined the game! Room Key: {roomKey}");
            }
            else
            {
                Debug.LogError($"Failed to join game: {startTask.ShutdownReason}");
            }
        }
        public async void RandomMatchmaking()
        {
            _networkRunner = Instantiate(networkRunnerPrefab);
            _networkEvents = _networkRunner.GetComponent<NetworkEvents>();
            AddListeners();

            //custom properties
            var customProperties = new Dictionary<string, SessionProperty>()
            {
                { "gameType",  (int)currentGameMode.value.mode }
            };

            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(SceneRef.FromIndex(1));

            var startGameArgs = new StartGameArgs
            {
                GameMode = GameMode.Shared,
                Scene = sceneInfo,
                SessionProperties = customProperties,
            };

            var result = await _networkRunner.StartGame(startGameArgs);
            if (result.Ok)
            {
                Debug.Log($"Session Started");
            }
            else {
                Debug.LogError($"Failed to Start");
            }
            //// Attempt to join the lobby
            //var lobbyResult = await _networkRunner.JoinSessionLobby(SessionLobby.Shared);
            //if (!lobbyResult.Ok)
            //{
            //    Debug.LogError($"Failed to join lobby: {lobbyResult.ShutdownReason}");
            //    return;
            //}

            //// Wait for session list update (replace Task.Delay with an event-driven approach if possible)
            //await Task.Delay(1000);

            //// Try joining an existing session
            //var sessionToJoin = _availableSessions?.FirstOrDefault()?.Name;

            //if (sessionToJoin != null)
            //{
            //    Debug.Log($"Joining existing session: {sessionToJoin}");
            //    if (await TryStartGame(sessionToJoin))
            //        return;
            //}

            //// No active sessions, create a new one
            //string newSessionKey = RoomKeyGenerator.GenerateRoomKey();
            //Debug.Log($"No active sessions found. Creating a new session: {newSessionKey}");
            //await TryStartGame(newSessionKey, true);
        }

        //private async Task<bool> TryStartGame(string sessionName, bool isNewSession = false)
        //{
        //    var sceneInfo = new NetworkSceneInfo();
        //    sceneInfo.AddSceneRef(SceneRef.FromIndex(1));

        //    var startGameArgs = new StartGameArgs
        //    {
        //        GameMode = GameMode.Shared,
        //        SessionName = sessionName,
        //        Scene = isNewSession ? sceneInfo : null // Only set scene info for new sessions
        //    };

        //    var result = await _networkRunner.StartGame(startGameArgs);
        //    if (result.Ok)
        //    {
        //        Debug.Log($"{(isNewSession ? "Created" : "Joined")} session: {sessionName}");
        //        return true;
        //    }

        //    Debug.LogError($"Failed to {(isNewSession ? "create" : "join")} session: {result.ShutdownReason}");
        //    return false;
        //}

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
            //_availableSessions = sessionList;
            Debug.Log($"Session List Updated. Found {sessionList.Count} sessions.");
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
