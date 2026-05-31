using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibe_Game.Gameplay.Entities.Player
{
    /// <summary>Хранит характеристики игрока и модификаторы, влияющие на боевые и передвижные параметры персонажа.</summary>
    public class PlayerStats
    {
        /// <summary>Текущее количество здоровья игрока.</summary>
        public float Health { get; set; } = 6;

        /// <summary>Максимально возможное здоровье игрока.</summary>
        public float MaxHealth { get; set; } = 6f;

        /// <summary>Базовый урон игрока до применения бонусов и множителей.</summary>
        public float Damage { get; set; } = 3.5f;

        /// <summary>Базовая скорость передвижения игрока.</summary>
        public float Speed { get; set; } = 1.0f;

        /// <summary>Дополнительные жизни (тотем): при 0 HP снимается одна жизнь и восстанавливается 2 HP.</summary>
        public int ExtraLives { get; set; }

        /// <summary>Аддитивный бонус к урону оружия (клык и т.п.).</summary>
        public int BonusWeaponDamage { get; set; }

        // Модификаторы (будут добавляться предметами)
        public float DamageMultiplier { get; set; } = 1.0f;

        /// <summary>Множитель скорости передвижения игрока.</summary>
        public float SpeedMultiplier { get; set; } = 1.0f;

        /// <summary>Множитель скорости снарядов игрока (перо).</summary>
        public float ProjectileSpeedMultiplier { get; set; } = 1f;

        /// <summary>Множитель перезарядки меча (меньше — быстрее; перо).</summary>
        public float SwordCooldownMultiplier { get; set; } = 1f;

        /// <summary>Уменьшает текущее здоровье игрока. Значение здоровья не может стать меньше нуля.</summary>
        public void TakeDamage(float amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;
        }

        /// <summary>Применяет характеристики предмета к игроку. Используется при подборе улучшений и артефактов.</summary>
        public void ApplyItemEffect(ItemEffect effect)
        {
            // TODO: Применение эффектов предметов
            Damage += effect.DamageModifier;
            Speed += effect.SpeedModifier;
            Health += effect.HealthModifier;
            // и т.д.
        }
    }

    /// <summary>Набор модификаторов характеристик, предоставляемых игровым предметом.</summary>
    public class ItemEffect
    {
        public float DamageModifier { get; set; }
        public float SpeedModifier { get; set; }
        public float HealthModifier { get; set; }
    }
}