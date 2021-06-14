using System.Linq;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.StaticEntity {
	public class Door: Entity, IInteractiveEntity {
		private int _sortingLayerId, _layerMaskId;

		[SerializeField] private Collider2D doorCollider;
		[SerializeField] private bool isOpen;
		[SerializeField] private Sprite closed;
		[SerializeField] private Sprite opened;


		private void Start() {
			Instantiate();
			InteractionCondition = player => !isOpen;
			AutoStopInteracting = true;
			_sortingLayerId = SortingLayer.NameToID("HubLayer1");
			_layerMaskId = LayerMask.NameToLayer("LAYER1");
		}

		public override void OnStartServer() {
			base.OnStartServer();
			Player.OnRemotePlayerClassChange += CheckNotOpened;
			Player.OnLocalPlayerClassChange += CheckNotOpened;
		}

		[Server]
		private void CheckNotOpened(ClassData data) {
			if (!isOpen) return; // If already closed, ignored

			// Else, replace all players to their spawn points
			CustomNetworkManager networkManager = CustomNetworkManager.Instance;
			foreach (Player player in networkManager.PlayerPrefabs) {
				player.transform.position = networkManager.GetStartPosition().position;
				player.TargetSetRenderingLayersInChildren(player.connectionToClient, _sortingLayerId, "HubLayer1",
					_layerMaskId);
			}

			PrintToAll(networkManager, LanguageManager["#respawn"]);

			// Close the door again
			RpcToggleSprite(isOpen);
			isOpen = !isOpen;
		}

		[ClientRpc] private void RpcToggleSprite(bool isOpen2) {
			AudioDB.PlayUISound("WoodenDoor");
			spriteRenderer.sprite = isOpen2 ? closed : opened;
			doorCollider.enabled = isOpen2;
			isOpen = !isOpen2;
		}

		[Server] private void PrintToAll(CustomNetworkManager networkManager, string message) {
			foreach (Player p in networkManager.PlayerPrefabs)
				p.TargetPrintInfoMessage(p.connectionToClient, message);
		}

		[Server] public void Interact(Player player) {
			if (isOpen) return;
			
			CustomNetworkManager networkManager = CustomNetworkManager.Instance;
			
			// Verify that no one has the same class
			bool[] playerClassValidation = new bool[3];
			foreach (Player p in CustomNetworkManager.Instance.PlayerPrefabs)
				playerClassValidation[(byte) p.playerClass] = true;
			if (playerClassValidation.Count(valid => valid) != networkManager.numPlayers) {
				PrintToAll(networkManager, LanguageManager["#rule-one"]);
				return;
			}
			
			// Verify that all players are near the door, ready to get to the forest
			if (!networkManager.PlayerPrefabs.All(IsPlayerInsideTrigger)) {
				PrintToAll(networkManager, 
					$"{player.playerName}" + LanguageManager["#player-itch"]);
				return;
			}

			doorCollider.enabled = isOpen;
			RpcToggleSprite(isOpen);
			isOpen = !isOpen;
		}
	}
}
