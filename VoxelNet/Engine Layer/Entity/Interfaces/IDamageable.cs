using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet.Entities.Interfaces
{
    interface IDamageable
    {
        void Die();
        void TakeDamage(int damage);
        void SetHealth(int health);
        int GetHealth();
    }
}
