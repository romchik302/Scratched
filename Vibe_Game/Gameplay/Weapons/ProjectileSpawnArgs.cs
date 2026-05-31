using System;
using Microsoft.Xna.Framework;

namespace Vibe_Game.Gameplay.Weapons;

/// <summary>
/// Аргументы конфигурации и параметры инициализации для создания снаряда в игровом мире.
/// </summary>
public readonly struct ProjectileSpawnArgs
{
    public Vector2 Position { get; init; }
    public Vector2 Direction { get; init; }
    public float Speed { get; init; }
    public float Damage { get; init; }
    public float LifetimeSeconds { get; init; }
    public float Radius { get; init; }
    /// <summary>Сила импульса отдачи, прикладываемая к хитбоксу цели при физическом контакте со снарядом.</summary>
    public float RecoilForce { get; init; }
    /// <summary>Указывает, принадлежит ли снаряд игроку (наносит урон врагам) или является вражеским.</summary>
    public bool IsFriendlyToPlayer { get; init; }
    /// <summary>Активирует режим движения снаряда по круговой орбите вместо стандартного линейного полета.</summary>
    public bool UseOrbitMotion { get; init; }
    public Vector2 OrbitCenter { get; init; }
    /// <summary>
    /// Функция обратного вызова для динамического обновления центра орбиты (например, для привязки снарядов к движущемуся боссу) каждый кадр.
    /// </summary>
    public Func<Vector2> OrbitCenterFollow { get; init; }
    public float OrbitRadius { get; init; }
    public float OrbitStartAngle { get; init; }
    public float OrbitAngularSpeed { get; init; }
    public float OrbitDurationSeconds { get; init; }
    /// <summary>
    /// Указывает, должен ли снаряд переходить на линейную траекторию полета после того, как истечет время его удержания на орбите.
    /// </summary>
    public bool ReleaseAfterOrbit { get; init; }
    public Vector2 ReleaseDirection { get; init; }
    /// <summary>Игнорирует физическую геометрию уровня, позволяя снаряду беспрепятственно лететь сквозь стены.</summary>
    public bool IgnoreWallCollisions { get; init; }
    /// <summary>
    /// Длина продольной проекции снаряда в пикселях. Используется для изменения формы коллизии со сферы на капсулу (например, для вытягивания текстур шипов).
    /// </summary>
    public float Length { get; init; }

    /// <summary>Для вражеских проджектайлов: подмена текстуры (например длинный шип босса). Null — стандартный лист.</summary>
    public string HostileTextureOverride { get; init; }
    /// <summary>Количество горизонтальных кадров (столбцов) в переопределенном текстурном атласе анимации снаряда.</summary>
    public int HostileTextureFrameColumns { get; init; }
    /// <summary>Масштабный множитель размера отображения кастомного спрайта снаряда при отрисовке.</summary>
    public float HostileProjectileDrawSize { get; init; }
}