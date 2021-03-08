using UnityEngine;

namespace DataBanks
{
    [CreateAssetMenu(fileName = "PlayerClassAnimatorsDB", menuName = "DataBanks/PlayerClassAnimators", order = 1)]
    public class PlayerClassAnimators : ScriptableObject
    {
        public RuntimeAnimatorController archerAnimator, warriorAnimator, mageAnimator;
        public Sprite archerSprite, warriorSprite, mageSprite;
    }
}
