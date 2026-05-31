namespace Vibe_Game.Gameplay.Entities.Collectables;

/// <summary>Подбираемые предметы: лут на пьедесталах, стартовый выбор оружия (первый этаж), дроп здоровья с врагов.</summary>
public enum CollectableKind
{
    Totem,
    Feather,
    Fang,
    /// <summary>Малое восстановление здоровья.</summary>
    HealthSmall,
    /// <summary>Большое восстановление здоровья.</summary>
    HealthLarge,
    /// <summary>Стартовый выбор: дальнобойное оружие (только пьедестал на первом этаже).</summary>
    WeaponProjectile,
    /// <summary>Стартовый выбор: меч (только пьедестал на первом этаже).</summary>
    WeaponSword
}
