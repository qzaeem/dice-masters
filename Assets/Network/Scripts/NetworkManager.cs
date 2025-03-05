using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        [HideInInspector] public NetworkRunner _networkRunner;
        private NetworkEvents _networkEvents;
        public string NewRoomKey;
        public int PlayerCount;
        //private List<SessionInfo> _availableSessions = new ();

        #region Callback Actions
        public Action<PlayerRef> onPlayerJoined;
        public Action<int> onJoinedGame;
        public Action<string> OnJoinFailed;
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
            sceneInfo.AddSceneRef(SceneRef.FromIndex(1)); // load main menu scene

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
                //Set the game mode for the client same as host
                if (_networkRunner.SessionInfo.IsValid)
                {
                    if (_networkRunner.SessionInfo.Properties.TryGetValue("gameType", out var gameTypeProperty))
                    {
                        int gameType = (int)gameTypeProperty;
                        Debug.Log($"Game mode set to: {gameType}");
                        onJoinedGame?.Invoke(gameType);
                    }
                }
            }
            else
            {
                string error = $"Failed to join game: {startTask.ShutdownReason}";
                Debug.LogError(error);
                OnJoinFailed?.Invoke(error);
                await _networkRunner.Shutdown();
                Destroy(_networkRunner.gameObject);
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
            else
            {
                Debug.LogError($"Failed to Start");
            }
        }

        public async Task EnterLobby()
        {
            _networkRunner = Instantiate(networkRunnerPrefab);
            _networkEvents = _networkRunner.GetComponent<NetworkEvents>();
            AddListeners();

            var result = await _networkRunner.JoinSessionLobby(SessionLobby.Shared);
            if (result.Ok)
            {
                Debug.Log($"Lobby Joined successfully!!!!");
            }
            else
            {
                Debug.LogError($"Failed to Start: {result.ShutdownReason}");
            }
        }

        public async void LeaveGame()
        {
            if (_networkRunner == null)
                return;

            await _networkRunner.Shutdown();

            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
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
            //_availableSessions = sessionList;
            SessionsListLobby.Instance.UpdateSessionUIEntry(sessionList);
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
