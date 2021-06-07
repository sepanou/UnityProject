using Mirror;
using UnityEngine;

namespace Generation{
    public class RoomInGame: NetworkBehaviour{
        public bool hasBeenDiscovered;
        public bool hasBeenCleared;
        public Room Room;

        private void Start() {
            hasBeenCleared = false;
            hasBeenDiscovered = false;
        }
    }
}