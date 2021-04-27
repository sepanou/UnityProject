using System;
using Mirror;
using Unity;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

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