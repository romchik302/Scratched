using Microsoft.Xna.Framework;
using Vibe_Game.Core.Services;

namespace Vibe_Game.Gameplay.Weapons;

/// <summary>
/// Оружие дальнего боя, выпускающее одиночные снаряды по прямой линии в направлении взгляда или движения владельца.
/// </summary>
public sealed class ForwardProjectileWeapon : WeaponBase
{
    /// <inheritdoc />
    public override WeaponFireMode FireMode => WeaponFireMode.AutoWhileDirectionHeld;

    private readonly float _projectileSpeed;
    private readonly int _damage;
    private readonly float _spawnOffset;
    private readonly float _lifetime;
    private readonly float _radius;
    private readonly float _recoilForce;

    /// <summary>
    /// Текущий множитель скорости полета создаваемых снарядов.
    /// </summary>
    public float ProjectileSpeedMultiplier { get; set; } = 1f;
    /// <summary>
    /// Дополнительный бонус к базовому урону, получаемый от характеристик персонажа или внешних эффектов (баффов).
    /// </summary>
    public int ExternalDamageBonus { get; set; }

    /// <summary>
    /// Инициализирует новый экземпляр линейного стрелкового оружия с заданными характеристиками снаряда.
    /// </summary>
    /// <param name="cooldownSeconds">Время перезарядки оружия между атаками в секундах.</param>
    /// <param name="projectileSpeed">Базовая скорость полета выпускаемого снаряда.</param>
    /// <param name="damage">Базовый урон, наносимый снарядом при столкновении с целью.</param>
    /// <param name="spawnOffsetPixels">Смещение точки появления снаряда относительно центра позиции владельца в пикселях.</param>
    /// <param name="lifetimeSeconds">Максимальное время жизни снаряда в секундах до его автоматического уничтожения.</param>
    /// <param name="radius">Радиус окружности коллизии снаряда в пикселях.</param>
    /// <param name="recoilForce">Сила импульса отдачи, прикладываемая к хитбоксу цели при попадании.</param>
    public ForwardProjectileWeapon(
    float cooldownSeconds,
    float projectileSpeed,
    int damage,
    float spawnOffsetPixels,
    float lifetimeSeconds,
    float radius = 4f,
    float recoilForce = 150f)
    : base("Forward Shot", cooldownSeconds)
    {
        _projectileSpeed = projectileSpeed;
        _damage = damage;
        _spawnOffset = spawnOffsetPixels;
        _lifetime = lifetimeSeconds;
        _radius = radius;
        _recoilForce = recoilForce;
    }

    /// <inheritdoc />
    public override bool TryPrimaryAttack(IAttackContext context, Vector2 ownerPosition, Vector2 facingDirection)
    {
        if (facingDirection == Vector2.Zero)
            return false;
        if (!TryStartCooldown())
            return false;

        var dir = Vector2.Normalize(facingDirection);
        var spawn = ownerPosition + dir * _spawnOffset;

        context.SpawnProjectile(new ProjectileSpawnArgs
        {
            Position = spawn,
            Direction = dir,
            Speed = _projectileSpeed * ProjectileSpeedMultiplier,
            Damage = _damage + ExternalDamageBonus,
            LifetimeSeconds = _lifetime,
            Radius = _radius,
            RecoilForce = _recoilForce,
            IsFriendlyToPlayer = true
        });

        GameplayAudio.PlayRangedAttack();

        return true;
    }
}