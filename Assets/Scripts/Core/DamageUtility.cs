using UnityEngine;

namespace Game
{
    public static class DamageUtility
    {
        public static IDamageable FindDamageable(Component source)
        {
            if (source == null) return null;
            MonoBehaviour[] behaviours = source.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IDamageable damageable) return damageable;
            }
            return null;
        }
    }
}
