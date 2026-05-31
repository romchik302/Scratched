using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>
/// Базовый враг: здоровье, активация при входе в комнату, общий жизненный цикл.
/// Конкретное поведение (как ходит, какая коллизия) — в наследниках через <see cref="UpdateEnemy"/>.
/// Наследует <see cref="Entity"/>: позиция, IsAlive, при желании Texture для отрисовки по умолчанию.
/// </summary>
public abstract class Enemy : Entity
{

    /// <summary>
    /// Таймер для отсчета интервала между воспроизведением фоновых (эмбиент) звуков врага.
    /// </summary>
    protected float _ambientSoundTimer;

    /// <summary>
    /// Интервал времени (в секундах) между автоматическими повторениями фонового звука.
    /// </summary>
    protected virtual float AmbientSoundInterval => 1.5f;

    /// <summary>
    /// Режимы случайного модифицированного поведения ИИ врага при преследовании цели.
    /// </summary>
    protected enum RandomMovementMode
    {
        None,
        Pause,
        SideDash,
        Zigzag,
        Orbit
    }

    /// <summary>
    /// Направление, в сторону которого в данный момент повернут или движется враг.
    /// </summary>
    protected enum FacingDirection
    {
        Right,
        Left,
        Up,
        Down
    }

    private static Texture2D _sharedFlyingTexture;
    private static Texture2D _sharedChasingTexture;
    private static Texture2D _sharedAdaptiveTexture;
    private static Texture2D _sharedDeathTexture;
    private static Texture2D _sharedBossTexture;
#if DEBUG
    private static Texture2D _debugPixel;
#endif
    /// <summary>Текущее количество здоровья врага. Если здоровье равно 0, враг считается мертвым.</summary>
    public int Health { get; set; }
    /// <summary>Максимальное количество здоровья врага, используемое при инициализации.</summary>
    public int MaxHealth { get; protected set; }
    /// <summary>Номер этажа, на котором находится враг. Влияет на масштабирование характеристик (HP, урон).</summary>
    public int FloorIndex { get; private set; } = 1;

    /// <summary>Сопротивление отдаче (0.0 = полная отдача, 1.0 = не отталкивается).</summary>
    public float RecoilResistance { get; protected set; } = 0f;

    /// <summary>Радиус врезания в игрока (на какое расстояние враг может зайти в игрока).</summary>
    public float PenetrationRadius { get; protected set; } = 0f;

    /// <summary>Пока false — AI и движение не крутятся (враг «спит» до входа игрока в комнату).</summary>
    public bool IsActivated { get; private set; }
    /// <summary>
    /// Определяет, может ли враг получать урон.
    /// Переопределяется особыми типами врагов.
    /// </summary>
    public virtual bool IsInvulnerable => false;
    /// <summary>
    /// Определяет, может ли враг наносить контактный урон игроку.
    /// </summary>
    public virtual bool CanDealContactDamage => true;

    private float _activationDelayTimer = 0f;
    private bool _canMove = false;
    private bool _activationSkippedDelay;

    /// <summary>Velocity отдачи (плавное отталкивание).</summary>
    private Vector2 _recoilVelocity = Vector2.Zero;

    /// <summary>Коэффициент затухания отдачи (чем меньше, тем дольше длится отдача).</summary>
    private const float RecoilDamping = 0.85f;
    protected FacingDirection Facing { get; private set; } = FacingDirection.Right;
    private static readonly System.Random SharedRandom = new();

    private RandomMovementMode _randomMovementMode = RandomMovementMode.None;
    private float _randomMovementTimeLeft = 0f;
    private float _zigzagDirectionTimeLeft = 0f;
    private int _lateralSign = 1;

    public float RandomBehaviorChance { get; set; } = 0.58f;
    public float RandomPauseMinDuration { get; set; } = 0.1f;
    public float RandomPauseMaxDuration { get; set; } = 0.4f;
    public float RandomSideDashMinDuration { get; set; } = 0.18f;
    public float RandomSideDashMaxDuration { get; set; } = 0.35f;
    public float RandomSideDashSpeedMultiplier { get; set; } = 1.25f;
    public float RandomZigzagMinDuration { get; set; } = 0.25f;
    public float RandomZigzagMaxDuration { get; set; } = 0.4f;
    public float RandomZigzagDirectionChangeInterval { get; set; } = 0.12f;
    public float RandomZigzagAmplitude { get; set; } = 1.1f;
    public float RandomZigzagSpeedMultiplier { get; set; } = 1.1f;
    public float RandomOrbitMinDuration { get; set; } = 0.45f;
    public float RandomOrbitMaxDuration { get; set; } = 1.1f;
    public float RandomOrbitInwardWeight { get; set; } = 0.55f;
    public float RandomOrbitLateralWeight { get; set; } = 1.1f;
    public float RandomOrbitSpeedMultiplier { get; set; } = 1.15f;

    /// <summary>
    /// Инициализирует базовые параметры сущности врага.
    /// </summary>
    /// <param name="position">Начальная позиция спавна врага в мировых координатах.</param>
    /// <param name="maxHealth">Максимальное количество очков здоровья.</param>
    protected Enemy(Vector2 position, int maxHealth)
    {
        Position = position;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    /// <summary>Устанавливает номер текущего этажа для масштабирования сложности.</summary>
    /// <param name="floorIndex">Номер этажа (минимальное значение 1).</param>
    public void SetFloorIndex(int floorIndex)
    {
        FloorIndex = Math.Max(1, floorIndex);
    }

    /// <summary>
    /// Загружает и кэширует общие текстуры врагов.
    /// Вызывается один раз и переиспользует ресурсы между экземплярами.
    /// </summary>
    public static void LoadSharedTextures(ContentManager content)
    {
        if (content == null)
            return;

        _sharedFlyingTexture ??= TryLoad(content, "enemy_flying_sheet");
        _sharedChasingTexture ??= TryLoad(content, "enemy_chasing_sheet");
        _sharedAdaptiveTexture ??= TryLoad(content, "enemy_adaptive_sheet");
        _sharedDeathTexture ??= TryLoad(content, Core.Settings.EnemyConfig.EnemyDeathAnimation);
        _sharedBossTexture ??= TryLoad(content, Core.Settings.EnemyConfig.BossTexture);
    }

    public static Texture2D SharedDeathTexture => _sharedDeathTexture;

    public static Texture2D SharedBossTexture => _sharedBossTexture;

    protected static Texture2D SharedFlyingTexture => _sharedFlyingTexture;
    protected static Texture2D SharedChasingTexture => _sharedChasingTexture;
    protected static Texture2D SharedAdaptiveTexture => _sharedAdaptiveTexture;

    private static Texture2D TryLoad(ContentManager content, string assetName)
    {
        try
        {
            return content.Load<Texture2D>(assetName);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Переводит врага в активное состояние, запуская его ИИ и движение.</summary>
    /// <param name="skipDelay">Если true, пропускает задержку активации (используется для немедленного появления).</param>
    public void Activate(bool skipDelay = false)
    {
        if (IsActivated)
            return;

        IsActivated = true; // Сразу видим врага
        _activationSkippedDelay = skipDelay;

        if (skipDelay)
        {
            _canMove = true; // Сразу можем двигаться (для призванных врагов)
        }
        else
        {
            _activationDelayTimer = Core.Settings.EnemyConfig.EnemyActivationDelaySeconds; // Задержка движения при входе в комнату
        }

        OnActivated();
    }

    /// <summary>True, если активация была с <paramref name="skipDelay"/> (призыв босса и т.п.).</summary>
    protected bool ActivationSkippedDelay => _activationSkippedDelay;

    /// <summary>
    /// Виртуальный метод-хук, вызываемый в момент активации врага (например, для воспроизведения стартового звука появления).
    /// </summary>
    protected virtual void OnActivated()
    {
    }

    /// <inheritdoc />
    public virtual void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0 || IsInvulnerable)
            return;

        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            IsAlive = false;
        }
    }

    /// <summary>Применяет импульс отталкивания к врагу.</summary>
    /// <param name="recoilDirection">Вектор направления отталкивания.</param>
    /// <param name="recoilForce">Сила отталкивания (умножается на (1 - RecoilResistance)).</param>
    public virtual void ApplyRecoil(Vector2 recoilDirection, float recoilForce)
    {
        if (!IsAlive || !IsActivated)
            return;

        // Учитываем сопротивление отдаче
        float effectiveForce = recoilForce * (1.0f - RecoilResistance);
        if (effectiveForce <= 0)
            return;

        // Добавляем отдачу к velocity (плавное отталкивание)
        _recoilVelocity += recoilDirection * effectiveForce;
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (!IsAlive)
            return;
        if (!IsActivated)
            return;
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (IsActivated && _canMove)
        {
            _ambientSoundTimer += dt;

            if (_ambientSoundTimer >= AmbientSoundInterval)
            {
                _ambientSoundTimer = 0f;
                PlayAmbientSound();
            }
        }

        // Обработка задержки перед началом движения
        if (!_canMove)
        {
            if (_activationDelayTimer > 0f)
            {
                _activationDelayTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_activationDelayTimer <= 0f)
                {
                    _canMove = true;
                }
            }
            // Обновляем анимацию даже во время задержки
            EnsureSpriteConfigured();
            UpdateAnimation(gameTime);
            return; // Не двигаемся пока задержка не истекла
        }

        // Применяем velocity отдачи
        if (_recoilVelocity != Vector2.Zero)
        {
            Vector2 oldPos = Position;
            Position += _recoilVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Проверяем коллизию со стенами при отдаче
            Position = ResolveRecoilCollision(oldPos, Position);

            _recoilVelocity *= RecoilDamping; // Затухание

            // Останавливаем если очень маленькая скорость
            if (_recoilVelocity.LengthSquared() < 0.01f)
                _recoilVelocity = Vector2.Zero;
        }


        UpdateEnemy(gameTime);
    }

    /// <summary>
    /// Виртуальный метод для обновления анимационных таймеров и индексов кадров. 
    /// Переопределяется в подклассах, обладающих анимацией.
    /// </summary>
    /// <param name="gameTime">Текущее игровое время.</param>
    protected virtual void UpdateAnimation(GameTime gameTime)
    {
    }

    /// <summary>
    /// Настраивает параметры спрайта, рассчитывает размеры кадров и вычленяет текстурные атласы. 
    /// Вызывается автоматически перед отрисовкой или обновлением.
    /// </summary>
    protected virtual void EnsureSpriteConfigured()
    {
    }

    /// <summary>
    /// Подготавливает параметры анимации и отображения спрайта.
    /// Используется перед началом работы системы анимации.
    /// </summary>
    public void PrimeSpriteConfiguration() => EnsureSpriteConfigured();

    /// <summary>
    /// Рассчитывает и корректирует позицию врага при получении импульса отдачи, предотвращая прохождение сквозь стены.
    /// </summary>
    /// <param name="oldPos">Позиция врага до применения смещения от отдачи.</param>
    /// <param name="newPos">Желаемая позиция после применения смещения.</param>
    /// <returns>Скорректированная координата <see cref="Vector2"/>, безопасная для перемещения.</returns>
    protected virtual Vector2 ResolveRecoilCollision(Vector2 oldPos, Vector2 newPos)
    {
        return newPos; // По умолчанию без проверки
    }

    /// <summary>
    /// Обновляет уникальную логику поведения, ИИ и перемещения конкретного типа врага.
    /// </summary>
    /// <param name="gameTime">Текущее игровое время.</param>
    protected abstract void UpdateEnemy(GameTime gameTime);

    /// <summary>
    /// Возвращает радиус физической окружности коллизии врага, используемый по умолчанию для проверок столкновений.
    /// </summary>
    /// <returns>Радиус коллизии в пикселях.</returns>
    protected virtual float GetCollisionRadius()
    {
        return 10f;
    }

    /// <summary>Обновляет направление взгляда (Facing) на основе вектора движения.</summary>
    /// <param name="direction">Вектор направления движения.</param>
    /// <param name="allowVertical">Разрешает установку вертикальных направлений (Up/Down).</param>
    protected void UpdateFacingFromDirection(Vector2 direction, bool allowVertical = false)
    {
        if (direction.LengthSquared() < 0.0001f)
            return;

        if (allowVertical && System.MathF.Abs(direction.Y) > System.MathF.Abs(direction.X))
        {
            Facing = direction.Y > 0 ? FacingDirection.Down : FacingDirection.Up;
            return;
        }

        Facing = direction.X >= 0 ? FacingDirection.Right : FacingDirection.Left;
    }

    /// <summary>
    /// Возвращает параметры отражения спрайта
    /// для корректного отображения направления взгляда.
    /// </summary>
    protected SpriteEffects GetHorizontalSpriteEffect()
    {
        return Facing == FacingDirection.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || !IsActivated)
            return;

        base.Draw(spriteBatch);
        DrawDebugOverlay(spriteBatch);
    }

    /// <summary>Вычисляет итоговый вектор движения с учетом случайных модификаторов поведения (зигзаги, орбиты, паузы).</summary>
    /// <param name="toTarget">Вектор направления к игроку.</param>
    /// <param name="dt">Время, прошедшее с последнего кадра (DeltaTime).</param>
    /// <param name="speedMultiplier">Выходной множитель скорости для текущего поведения.</param>
    /// <returns>Нормализованный вектор направления движения.</returns>
    protected Vector2 GetMovementDirectionWithRandomBehavior(Vector2 toTarget, float dt, out float speedMultiplier)
    {
        speedMultiplier = 1f;
        if (toTarget.LengthSquared() < 0.0001f)
            return Vector2.Zero;

        Vector2 chaseDir = Vector2.Normalize(toTarget);
        AdvanceOrStartRandomMovement(dt);

        switch (_randomMovementMode)
        {
            case RandomMovementMode.Pause:
                speedMultiplier = 0f;
                return Vector2.Zero;

            case RandomMovementMode.SideDash:
                speedMultiplier = RandomSideDashSpeedMultiplier;
                return GetPerpendicular(chaseDir, _lateralSign);

            case RandomMovementMode.Zigzag:
                speedMultiplier = RandomZigzagSpeedMultiplier;
                _zigzagDirectionTimeLeft -= dt;
                if (_zigzagDirectionTimeLeft <= 0f)
                {
                    _lateralSign *= -1;
                    _zigzagDirectionTimeLeft = MathF.Max(0.02f, RandomZigzagDirectionChangeInterval);
                }

                Vector2 zigzag = chaseDir + GetPerpendicular(chaseDir, _lateralSign) * RandomZigzagAmplitude;
                return zigzag.LengthSquared() > 0.0001f ? Vector2.Normalize(zigzag) : chaseDir;

            case RandomMovementMode.Orbit:
                speedMultiplier = RandomOrbitSpeedMultiplier;
                Vector2 orbit = chaseDir * RandomOrbitInwardWeight
                    + GetPerpendicular(chaseDir, _lateralSign) * RandomOrbitLateralWeight;
                return orbit.LengthSquared() > 0.0001f ? Vector2.Normalize(orbit) : chaseDir;

            default:
                return chaseDir;
        }
    }

    /// <summary>
    /// Сбрасывает текущее случайное поведение врага.
    /// После вызова используется стандартное преследование цели.
    /// </summary>
    protected void ResetRandomMovementBehavior()
    {
        _randomMovementMode = RandomMovementMode.None;
        _randomMovementTimeLeft = 0f;
        _zigzagDirectionTimeLeft = 0f;
    }

    private void AdvanceOrStartRandomMovement(float dt)
    {
        if (_randomMovementMode != RandomMovementMode.None)
        {
            _randomMovementTimeLeft -= dt;
            if (_randomMovementTimeLeft <= 0f)
            {
                _randomMovementMode = RandomMovementMode.None;
                _randomMovementTimeLeft = 0f;
            }

            return;
        }

        float chanceRoll = MathF.Max(0f, RandomBehaviorChance) * dt;
        if (SharedRandom.NextDouble() >= chanceRoll)
            return;

        StartRandomMovement();
    }

    private void StartRandomMovement()
    {
        int modeIndex = SharedRandom.Next(4);
        _lateralSign = SharedRandom.Next(2) == 0 ? -1 : 1;

        switch (modeIndex)
        {
            case 0:
                _randomMovementMode = RandomMovementMode.Pause;
                _randomMovementTimeLeft = NextRange(RandomPauseMinDuration, RandomPauseMaxDuration);
                break;

            case 1:
                _randomMovementMode = RandomMovementMode.SideDash;
                _randomMovementTimeLeft = NextRange(RandomSideDashMinDuration, RandomSideDashMaxDuration);
                break;

            case 2:
                _randomMovementMode = RandomMovementMode.Zigzag;
                _randomMovementTimeLeft = NextRange(RandomZigzagMinDuration, RandomZigzagMaxDuration);
                _zigzagDirectionTimeLeft = MathF.Max(0.02f, RandomZigzagDirectionChangeInterval);
                break;

            default:
                _randomMovementMode = RandomMovementMode.Orbit;
                _randomMovementTimeLeft = NextRange(RandomOrbitMinDuration, RandomOrbitMaxDuration);
                break;
        }
    }

 
    private static Vector2 GetPerpendicular(Vector2 direction, int sign)
    {
        return sign >= 0
            ? new Vector2(-direction.Y, direction.X)
            : new Vector2(direction.Y, -direction.X);
    }

    private static float NextRange(float min, float max)
    {
        if (max < min)
            (min, max) = (max, min);

        double t = SharedRandom.NextDouble();
        return min + (float)t * (max - min);
    }

    /// <summary>
    /// Возвращает габариты тела врага для отрисовки отладочной рамки. По умолчанию возвращает физические границы.
    /// </summary>
    /// <returns>Прямоугольник коллизии или <see langword="null"/>, если отрисовка не требуется.</returns>
    protected virtual Rectangle? GetDebugBodyBounds()
    {
        return GetBounds();
    }

    /// <summary>
    /// Возвращает границы триггера атаки для отладочной визуализации.
    /// </summary>
    /// <returns>Прямоугольник зоны атаки или <see langword="null"/>, если зоны нет.</returns>
    protected virtual Rectangle? GetDebugAttackBounds()
    {
        return null;
    }

    /// <summary>
    /// Возвращает текущий радиус агрессии (обнаружения) для отрисовки зеленой окружности.
    /// </summary>
    /// <returns>Радиус в пикселях или <see langword="null"/>, если радиус не используется.</returns>
    protected virtual float? GetDebugAggroRadius()
    {
        return null;
    }

    /// <summary>
    /// Вычисляет центральную точку сущности, используемую в качестве центра для отрисовки отладочных кругов.
    /// </summary>
    /// <returns>Вектор координат центра врага.</returns>
    protected virtual Vector2 GetDebugCenter()
    {
        Rectangle bounds = GetBounds();
        return new Vector2(bounds.Center.X, bounds.Center.Y);
    }
    /// <summary>
    /// Воспроизводит уникальный фоновый (эмбиент) звук, если враг активен и жив. 
    /// Переопределяется в наследниках для кастомной озвучки.
    /// </summary>
    protected virtual void PlayAmbientSound() 
    {
        if (!IsActivated || !IsAlive)
            return;
    }

#if DEBUG
/// <summary>
    /// Отрисовывает поверх спрайта контуры физических хитбоксов (красный), зон атак (синий) и радиусов агрессии (зеленый).
    /// Доступно только в DEBUG-сборке.
    /// </summary>
    /// <param name="spriteBatch">Пакет спрайтов для графического вывода.</param>
    protected void DrawDebugOverlay(SpriteBatch spriteBatch)
    {
        if (spriteBatch == null)
            return;

        if (_debugPixel == null)
        {
            _debugPixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });
        }

        Rectangle? bodyBounds = GetDebugBodyBounds();
        if (bodyBounds.HasValue)
            DrawRectOutline(spriteBatch, bodyBounds.Value, Color.Red);

        Rectangle? attackBounds = GetDebugAttackBounds();
        if (attackBounds.HasValue)
            DrawRectOutline(spriteBatch, attackBounds.Value, Color.CornflowerBlue);

        float? aggroRadius = GetDebugAggroRadius();
        if (aggroRadius.HasValue && aggroRadius.Value > 0f)
            DrawCircleOutline(spriteBatch, GetDebugCenter(), aggroRadius.Value, Color.LimeGreen);
    }

    private static void DrawRectOutline(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), color);
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), color);
    }

    private static void DrawCircleOutline(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
    {
        const int segments = 40;
        if (radius <= 1f)
            return;

        Vector2 prev = center + new Vector2(radius, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float a = MathHelper.TwoPi * t;
            Vector2 next = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius;
            DrawLine(spriteBatch, prev, next, color);
            prev = next;
        }
    }

    private static void DrawLine(SpriteBatch spriteBatch, Vector2 from, Vector2 to, Color color)
    {
        Vector2 edge = to - from;
        float length = edge.Length();
        if (length < 1f)
            return;

        float angle = MathF.Atan2(edge.Y, edge.X);
        spriteBatch.Draw(
            _debugPixel,
            new Rectangle((int)from.X, (int)from.Y, (int)length, 1),
            null,
            color,
            angle,
            Vector2.Zero,
            SpriteEffects.None,
            0
        );
    }
#else
    protected void DrawDebugOverlay(SpriteBatch spriteBatch) { }
    
#endif
}
