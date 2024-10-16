using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace DiceGame.Network
{
    public class NetworkManager : Models.MonoBehaviourSingleton<NetworkManager>, INetworkRunnerCallbacks
    {
		[SerializeField] private NetworkRunner networkRunnerPrefab;
		[SerializeField] private int gameSceneIndex;
		private NetworkRunner _networkRunner;
		private NetworkEvents _networkEvents;

        #region Callback Actions
        public Action<PlayerRef> onPlayerJoined;
        #endregion

        public async void StartGame()
        {
			_networkRunner = Instantiate(networkRunnerPrefab);

			// Add listeners
			_networkEvents = _networkRunner.GetComponent<NetworkEvents>();
			AddListeners();

			var sceneInfo = new NetworkSceneInfo();
			sceneInfo.AddSceneRef(SceneRef.FromIndex(gameSceneIndex));

			var startArguments = new StartGameArgs()
			{
				GameMode = GameMode.Shared,
				SessionName = "dice",
                PlayerCount = 4,
				// We need to specify a session property for matchmaking to decide where the player wants to join.
				// Otherwise players from Platformer scene could connect to ThirdPersonCharacter game etc.
				//SessionProperties = new Dictionary<string, SessionProperty> { ["GameMode"] = GameModeIdentifier },
				Scene = sceneInfo,
			};

			var startTask = _networkRunner.StartGame(startArguments);
			await startTask;
		}

		private void AddListeners()
        {
			AddShutdownListener(OnShutdown);
			AddConnectedToServerListener(OnConnectedToServer);
			AddDisconnectedFromServerListener(OnDisconnectedFromServer);
			AddFailedToConnectListener(OnConnectionFailed);
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
