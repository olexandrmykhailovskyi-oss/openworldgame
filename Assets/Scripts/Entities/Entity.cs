using UnityEngine;

namespace OpenWorld.Entities
{
    public class Entity : MonoBehaviour
    {
        public int maxHealth = 100;
        public int health;

        public bool IsAlive => health > 0;

        protected virtual void Awake()
        {
            health = maxHealth;
        }

        public virtual void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            health -= amount;
            if (health <= 0)
            {
                health = 0;
                Die();
            }
        }

        protected virtual void Die()
        {
            Destroy(gameObject, 0.1f);
        }
    }
}
