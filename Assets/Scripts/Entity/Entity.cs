using System;
using DataBanks;
using Mirror;
using UI_Audio;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity {
	public abstract class Entity: NetworkBehaviour {
		[SerializeField] protected SpriteRenderer spriteRenderer;

		[NonSerialized] protected static LocalGameManager Manager;
		[NonSerialized] protected static LanguageManager LanguageManager;
		[NonSerialized] protected static InputManager InputManager;
		[NonSerialized] protected static PlayerInfoManager PlayerInfoManager;
		[NonSerialized] protected static InventoryManager InventoryManager;

		public static void InitClass(LocalGameManager manager) {
			if (!(Manager is null)) throw new Exception("InitClass called multiple times");
			Manager = manager;
			LanguageManager = Manager.languageManager;
			PlayerInfoManager = Manager.playerInfoManager;
			InventoryManager = Manager.inventoryManager;
			InputManager = Manager.inputManager;
		}

		public static void SetRenderingLayersInChildren(int sortingLayerID, string sortingLayerName, int layerMask, GameObject gameObject) {
			if (!gameObject.activeInHierarchy) return;
			gameObject.layer = layerMask;
			if (gameObject.TryGetComponent(out SpriteRenderer renderer)) {
				renderer.sortingLayerName = sortingLayerName;
				renderer.sortingLayerID = sortingLayerID;
			}
			if (gameObject.TryGetComponent(out ParticleSystemRenderer psRenderer)) {
				psRenderer.sortingLayerName = sortingLayerName;
				psRenderer.sortingLayerID = sortingLayerID;
			}
			for (int i = 0; i < gameObject.transform.childCount; ++i) {
				GameObject child = gameObject.transform.GetChild(i).gameObject;
				SetRenderingLayersInChildren(sortingLayerID, sortingLayerName, layerMask, child);
			}
		}
		
		protected static void SetSameRenderingParameters(Entity reference, Entity toChange) {
			// Two GO with the same layer are assumed to share the same renderer parameters
			if (toChange.gameObject.layer == reference.gameObject.layer)
				return;
			if (!(reference.spriteRenderer is null)) {
				SetRenderingLayersInChildren(reference.spriteRenderer.sortingLayerID,
					reference.spriteRenderer.sortingLayerName, reference.gameObject.layer, toChange.gameObject);
				return;
			}
			if (!reference.TryGetComponent(out ParticleSystemRenderer psRenderer)) return;
			SetRenderingLayersInChildren(psRenderer.sortingLayerID, psRenderer.sortingLayerName, 
				reference.gameObject.layer, toChange.gameObject);
		}
		
		protected void InstantiateEntity() {
			if (spriteRenderer is null) return;
			spriteRenderer = GetComponent<SpriteRenderer>();
		}

		public Vector2 GetPosition2D() => transform.position;
		
		public SpriteRenderer GetSpriteRenderer() => spriteRenderer;

		[ServerCallback]
		public void SetPosition2D(Vector2 newPos) => transform.position = newPos;
	}
}
