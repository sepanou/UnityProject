using UnityEngine;

namespace Behaviour {
	public class Idle: IBehaviour {
		public Vector2 NextDirection()
			=> Vector2.zero;
	}
}