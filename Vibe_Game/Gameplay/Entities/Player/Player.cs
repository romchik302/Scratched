using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Gameplay.Entities.Player
{
    /// <summary>
    /// Игровой персонаж, управляемый игроком.
    /// Отвечает за движение, получение урона, использование оружия,
    /// воспроизведение анимаций и взаимодействие с игровыми системами.
    /// </summary>
    public class Player : Entity
    {
        private readonly IPlayerRenderer _renderer;
        private readonly IInputService _inputService;
        private readonly IPlayerContentLoader _contentLoader;
        private readonly IAttackContext _attackContext;

        /// <summary>Контроллер управления игроком. Обрабатывает ввод и рассчитывает движение персонажа.</summary>
        public PlayerController Controller { get; private set; }

        /// <summary>Набор характеристик игрока: здоровье, модификаторы скорости, урона и дополнительные жизни.</summary>
        public PlayerStats Stats { get; private set; }

        /// <summary>Текущее экипированное оружие игрока. Используется для выполнения атак и обновления боевого состояния.</summary>
        public IWeapon EquippedWeapon { get; set; }

        private Vector2 _lastShootDirection;

        // Для анимации
        private PlayerRenderer _animationRenderer;

        private float _invincibilityTimer = 0f;
        private const float InvincibilityDuration = 1.4f;

        private float _flashingTimer = 0f;
        private const float FlashingDuration = 0.2f;

        /// <summary>Создает нового игрока и инициализирует все связанные системы.</summary>
        /// <param name="position">Начальная позиция игрока.</param>
        /// <param name="renderer">Объект, отвечающий за визуальное отображение игрока.</param>
        /// <param name="inputService">Сервис обработки пользовательского ввода.</param>
        /// <param name="contentLoader">Загрузчик игровых ресурсов игрока.</param>
        /// <param name="attackContext">Контекст выполнения атак и взаимодействия с боевой системой.</param>
        public Player(
            Vector2 position,
            IPlayerRenderer renderer,
            IInputService inputService,
            IPlayerContentLoader contentLoader,
            IAttackContext attackContext)
            : base()
        {
            Position = position;
            _renderer = renderer ?? throw new System.ArgumentNullException(nameof(renderer));
            _inputService = inputService ?? throw new System.ArgumentNullException(nameof(inputService));
            _contentLoader = contentLoader ?? throw new System.ArgumentNullException(nameof(contentLoader));
            _attackContext = attackContext ?? throw new System.ArgumentNullException(nameof(attackContext));

            Controller = new PlayerController(this, _inputService);
            Stats = new PlayerStats();

            Color = Color.White;

            _animationRenderer = renderer as PlayerRenderer;
        }

        /// <inheritdoc />
        public override void LoadContent(ContentManager content)
        {
            _contentLoader.LoadContent(content);
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            SyncEquipmentFromStats();

            GameplayAudio.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            Controller.Update(gameTime);

            _lastShootDirection = Controller.ShootDirection;

            if (EquippedWeapon != null)
            {
                EquippedWeapon.Update(gameTime, _attackContext);

                Vector2 dir = Controller.ShootDirection;

                switch (EquippedWeapon.FireMode)
                {
                    case WeaponFireMode.AutoWhileDirectionHeld:
                        if (IsAnyShootDirectionHeld() && dir != Vector2.Zero)
                            EquippedWeapon.TryPrimaryAttack(_attackContext, Position, dir);
                        break;

                    case WeaponFireMode.DirectionHeldPlusButtonPress:
                        if (IsAnyShootDirectionHeld()
                            && dir != Vector2.Zero
                            && _inputService.IsActionPressed(InputAction.Fire))
                            EquippedWeapon.TryPrimaryAttack(_attackContext, Position, dir);
                        break;
                }
            }

            if (EquippedWeapon is SwordWeapon sword)
                sword.UpdateOwnerPosition(Position);

            Velocity = Controller.CurrentVelocity;

            if (_invincibilityTimer > 0)
            {
                _invincibilityTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_invincibilityTimer < 0)
                    _invincibilityTimer = 0;

                _flashingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_flashingTimer > FlashingDuration)
                {
                    Color = Color == Color.White ? Color.White * 0.25f : Color.White;
                    _flashingTimer = 0f;
                }
            }
            else
            {
                if (Color != Color.White)
                    Color = Color.White;
            }

            _animationRenderer?.Update(gameTime, Velocity, _lastShootDirection);

            base.Update(gameTime);
        }

        /// <summary>Запускает анимацию подбора предмета, если поддерживается рендерером.</summary>
        public void TryPlayPickupAnimation()
        {
            _animationRenderer?.BeginPickupAnimation();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch)
        {
            _renderer.Draw(spriteBatch, Position, _lastShootDirection, Color);
        }

        /// <inheritdoc />
        public override Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X - PlayerConfig.Radius,
                (int)Position.Y - PlayerConfig.Radius,
                PlayerConfig.Size,
                PlayerConfig.Size
            );
        }

        /// <summary>Применяет урон к игроку, запускает неуязвимость и обрабатывает потерю жизней.</summary>
        /// <param name="amount">Количество получаемого урона.</param>
        public void TakeDamage(float amount)
        {
            if (_invincibilityTimer > 0) return;
            if (amount <= 0) return;

            Stats.TakeDamage(amount);
            GameplayAudio.PlayPlayerHit();

            while (Stats.Health <= 0f && Stats.ExtraLives > 0)
            {
                Stats.ExtraLives--;
                Stats.Health = 2f;
            }

            if (Stats.Health <= 0f)
                return;

            _invincibilityTimer = InvincibilityDuration;
            Color = Color.White * 0.25f;
        }

        /// <summary>Синхронизирует параметры оружия и движения с характеристиками игрока.</summary>
        private void SyncEquipmentFromStats()
        {
            Controller.MaxSpeed = CollectibleConfig.BasePlayerControllerMaxSpeed * Stats.SpeedMultiplier;

            if (EquippedWeapon is ForwardProjectileWeapon gun)
            {
                gun.ExternalDamageBonus = Stats.BonusWeaponDamage;
                gun.ProjectileSpeedMultiplier = Stats.ProjectileSpeedMultiplier;
            }
            else if (EquippedWeapon is SwordWeapon sword)
            {
                sword.ExternalDamageBonus = Stats.BonusWeaponDamage;
                sword.SetCooldownMultiplier(Stats.SwordCooldownMultiplier);
            }
        }

        /// <summary>Проверяет, находится ли игрок в состоянии неуязвимости.</summary>
        public bool IsInvincible => _invincibilityTimer > 0;

        private bool IsAnyShootDirectionHeld()
        {
            return _inputService.IsActionDown(InputAction.ShootUp)
                || _inputService.IsActionDown(InputAction.ShootDown)
                || _inputService.IsActionDown(InputAction.ShootLeft)
                || _inputService.IsActionDown(InputAction.ShootRight);
        }

        /// <summary>Устанавливает модификатор трения для передвижения (например, при ходьбе по льду или слизи).</summary>
        /// <param name="multiplier">Коэффициент трения (1.0 — стандарт).</param>
        public void SetMovementFrictionMultiplier(float multiplier)
        {
            Controller.SetFrictionMultiplier(multiplier);
        }
    }
}