using System;
using Fusion;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace DiceGame.Network
{
    public class NetworkManager : Models.MonoBehaviourSingleton<NetworkManager>
    {
		[SerializeField] private NetworkRunner networkRunnerPrefab;
		[SerializeField] private int gameSceneIndex;
		private NetworkRunner _networkRunner;
		private NetworkEvents _networkEvents;

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
	}
}
