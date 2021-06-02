using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public interface INetworkObject {
    NetworkIdentity GetNetworkIdentity();
    string GetName();
}

public class CustomSyncList<T> : SyncList<uint>, IEnumerable<T> where T : INetworkObject {

    private readonly struct Change
    {
        public readonly Operation Op;
        public readonly int Index;

        public Change(Operation operation, int index) {
            Op = operation;
            Index = index;
        }
    }
    
    public delegate void CustomSyncListChanged(Operation op, int index, T item);
    public new event CustomSyncListChanged Callback;
    private const float DelayForNetworkSpawnInSeconds = 1f;
    private const int ChecksNbrForNetworkSpawn = 10;

    private static MonoBehaviour CoroutineHandler => NetworkManager.singleton;

    /// <summary>
    /// Creates a SyncList trying to avoid the issues of non-syncing items - notably when spawning them on late joining clients
    /// </summary>
    public CustomSyncList() : base(EqualityComparer<uint>.Default) => base.Callback += OnSyncListChanged;

    private static bool IsValid(T element) {
        if (element == null) return false;
        if (element.GetNetworkIdentity()) return true;
        Debug.LogWarning($"{element.GetName()} does not have a valid netIdentity");
        return false;
    }

    private IEnumerator WaitForNetworkSpawn(Change change, uint netId) {
        for (int i = 0; i < ChecksNbrForNetworkSpawn; i++) {
            if (NetworkIdentity.spawned.TryGetValue(netId, out NetworkIdentity identity)) {
                Callback?.Invoke(change.Op, change.Index, identity.gameObject.GetComponent<T>());
                yield break;
            }
            yield return new WaitForSeconds(DelayForNetworkSpawnInSeconds);
        }
    }

    public override void OnDeserializeAll(NetworkReader reader) {
        base.OnDeserializeAll(reader);
        for (int i = 0; i < Count; i++)
            NetworkManager.singleton.StartCoroutine(WaitForNetworkSpawn(new Change(Operation.OP_ADD, i), base[i]));
    }

    private void OnSyncListChanged(Operation op, int index, uint oldItem, uint newItem) {
        switch (op) {
            case Operation.OP_SET:
            case Operation.OP_ADD:
                CoroutineHandler.StartCoroutine(WaitForNetworkSpawn(new Change(op, index), newItem));
                break;
            case Operation.OP_REMOVEAT:
                CoroutineHandler.StartCoroutine(WaitForNetworkSpawn(new Change(op, index), oldItem));
                break;
            case Operation.OP_CLEAR:
                Callback?.Invoke(op, index, default);
                break;
            default:
                Debug.LogWarning($"[CustomSyncList] Unknown or not supported operation {op}!");
                break;
        }
    }

    public void Add(T element) {
        if (!IsValid(element)) return;
        base.Add(element.GetNetworkIdentity().netId);
    }

    public bool Remove(T element) {
        if (!IsValid(element)) return false;
        return base.Remove(element.GetNetworkIdentity().netId);
    }

    public new void Clear() => base.Clear();
    
    public bool Contains(T element) => IsValid(element) && base.Contains(element.GetNetworkIdentity().netId);

    public int IndexOf(T element) => base.IndexOf(element.GetNetworkIdentity().netId);

    public new T this[int index] {
        get {
            uint netId = base[index];
            if (NetworkIdentity.spawned.TryGetValue(netId, out NetworkIdentity identity))
                return identity.gameObject.GetComponent<T>();
            Debug.LogWarning($"Cannot find object with netId #{netId}...");
            return default;
        }
        set {
            if (!IsValid(value)) return;
            base[index] = value.GetNetworkIdentity().netId;
        }
    }
    
    public new CustomEnumerator GetEnumerator() => new CustomEnumerator(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new CustomEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => new CustomEnumerator(this);
    
    public struct CustomEnumerator : IEnumerator<T>
    {
        private readonly CustomSyncList<T> _list;
        private int _index;
        public T Current { get; private set; }

        public CustomEnumerator(CustomSyncList<T> list)
        {
            _list = list;
            _index = -1;
            Current = default;
        }

        public bool MoveNext() {
            if (++_index >= _list.Count) return false;
            Current = _list[_index];
            return true;
        }

        public void Reset() => _index = -1;
        object IEnumerator.Current => Current;
        public void Dispose() {}
    }
}