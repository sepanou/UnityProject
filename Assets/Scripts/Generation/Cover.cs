using System;
using System.Collections.Generic;
using Entity.DynamicEntity.LivingEntity.Player;
using UnityEngine;
using Mirror;

namespace Generation{
    public class Cover : NetworkBehaviour{
        private List<Player> _players = new List<Player>();
        private CustomNetworkManager _customNetworkManager = CustomNetworkManager.Instance;

        [Server]
        private void OnTriggerEnter2D(Collider2D other) {
            Debug.Log("Salut");
            if (other.gameObject.TryGetComponent(out Player player)) {
                _players.Add(player);
                Debug.Log("Stp :'(");
            }
        }

        [Server]
        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.TryGetComponent(out Player player)) {
                _players.Remove(player);
            }
        }

        [Server]
        private void Update() {
            Debug.Log(_players.Count);
            Debug.Log(_customNetworkManager.PlayerPrefabs.Count);
            if (_players.Count == _customNetworkManager.PlayerPrefabs.Count) {
                NetworkServer.Destroy(gameObject);
                RoomInGame roomInGame = gameObject.GetComponentInParent<RoomInGame>();
                roomInGame.hasBeenDiscovered = true;
            }
        }
    }
}
