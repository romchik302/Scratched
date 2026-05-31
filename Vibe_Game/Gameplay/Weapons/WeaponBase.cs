using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Vibe_Game.Gameplay.Weapons;

/// <summary>
/// Базовый класс оружия.
/// Предоставляет общую логику отката между атаками,
/// обновления состояния и взаимодействия с боевой системой.
/// </summary>
public abstract class WeaponBase : IWeapon
{
    /// <summary>
    /// Отображаемое название оружия.
    /// Используется в интерфейсе и уведомлениях.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Режим стрельбы оружия.
    /// По умолчанию атака выполняется при удержании направления.
    /// </summary>
    public virtual WeaponFireMode FireMode => WeaponFireMode.AutoWhileDirectionHeld;

    /// <summary>Базовая отдача оружия (сила толчка в обратную сторону от направления атаки).</summary>
    public virtual float BaseRecoil => 0f;

    /// <summary>
    /// Длительность отката между атаками в секундах.
    /// </summary>
    private float _cooldownRemaining;

    protected float CooldownSeconds { get; set; }
    /// <summary>
    /// Показывает, находится ли оружие на перезарядке между атаками.
    /// </summary>
    protected bool IsOnCooldown => _cooldownRemaining > 0f;

    /// <summary>
    /// Инициализирует базовые параметры оружия с указанием его названия и времени отката.
    /// </summary>
    /// <param name="displayName">Отображаемое имя оружия для UI.</param>
    /// <param name="cooldownSeconds">Время перезарядки в секундах.</param>
    protected WeaponBase(string displayName, float cooldownSeconds)
    {
        DisplayName = displayName;
        CooldownSeconds = cooldownSeconds;
    }

    /// <summary>
    /// Обновляет внутреннее состояние оружия и уменьшает оставшееся время отката.
    /// </summary>
    /// <param name="gameTime">Текущее игровое время.</param>
    /// <param name="context">Контекст выполнения боевых действий и взаимодействия с миром.</param>
    public virtual void Update(GameTime gameTime, IAttackContext context)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_cooldownRemaining > 0f)
            _cooldownRemaining = System.MathF.Max(0f, _cooldownRemaining - dt);
    }

    /// <summary>
    /// Запускает таймер отката оружия, если текущая перезарядка полностью завершена.
    /// </summary>
    /// <returns>Значение <see langword="true"/>, если откат был успешно запущен; <see langword="false"/>, если оружие еще не перезарядилось.</returns>
    protected bool TryStartCooldown()
    {
        if (_cooldownRemaining > 0f)
            return false;
        _cooldownRemaining = CooldownSeconds;
        return true;
    }

    /// <summary>
    /// Выполняет основную атаку оружия из указанной позиции в заданном направлении.
    /// </summary>
    /// <param name="context">Контекст боевой системы для регистрации попаданий или спавна снарядов.</param>
    /// <param name="ownerPosition">Позиция владельца оружия в мировых координатах.</param>
    /// <param name="facingDirection">Вектор направления атаки.</param>
    /// <returns>Значение <see langword="true"/>, если атака была успешно произведена; иначе — <see langword="false"/>.</returns>
    public abstract bool TryPrimaryAttack(IAttackContext context, Vector2 ownerPosition, Vector2 facingDirection);

    /// <summary>
    /// Отрисовывает визуальные элементы, спрайты или графические эффекты оружия в игровом мире.
    /// </summary>
    /// <param name="spriteBatch">Пакет спрайтов для графического вывода.</param>
    /// <param name="context">Контекст боевой системы.</param>
    public virtual void Draw(SpriteBatch spriteBatch, IAttackContext context) { }
}
