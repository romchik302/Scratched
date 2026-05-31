using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>
    /// Представляет интерактивный тайл кнопки. 
    /// Поддерживает состояния покоя, анимацию нажатия и анимацию ожидания в нажатом состоянии.
    /// </summary>
    public sealed class ButtonTile : Tile
    {
        private const float PressAnimationSeconds = 0.18f;
        private const float ActiveIdleFrameSeconds = 0.45f;

        private float _pressAnimationTimer;
        private float _activeIdleTimer;
        private int _activeIdleFrame;

        /// <summary>
        /// Инициализирует новый экземпляр класса ButtonTile.
        /// </summary>
        /// <param name="gridPosition">Позиция кнопки в сетке уровня.</param>
        public ButtonTile(Point gridPosition) : base(gridPosition)
        {
        }

        /// <summary>Указывает, находится ли кнопка в нажатом состоянии.</summary>
        public bool IsPressed { get; private set; }

        /// <summary>Возвращает true, если в данный момент проигрывается анимация нажатия.</summary>
        public bool IsPressAnimationPlaying => _pressAnimationTimer > 0f;

        /// <summary>Текущий индекс кадра анимации для активного состояния (0 или 1).</summary>
        public int ActiveIdleFrame => _activeIdleFrame;

        /// <inheritdoc />
        public override bool HasButton => true;

        /// <inheritdoc />
        public override bool CanHostEnemy => false;

        /// <inheritdoc />
        public override Color Tint => IsPressed ? GameColors.ButtonUnlocked : GameColors.ButtonLocked;

        /// <summary>
        /// Переводит кнопку в нажатое состояние и инициирует воспроизведение анимации нажатия.
        /// </summary>
        public void Press()
        {
            if (IsPressed)
                return;

            IsPressed = true;
            _pressAnimationTimer = PressAnimationSeconds;
            _activeIdleTimer = 0f;
            _activeIdleFrame = 0;
        }

        /// <summary>
        /// Обновляет таймеры анимации нажатия и циклической анимации нажатого состояния.
        /// </summary>
        /// <param name="deltaSeconds">Время, прошедшее с последнего кадра в секундах.</param>
        public void Update(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
                return;

            // Обработка анимации нажатия (приоритетная)
            if (_pressAnimationTimer > 0f)
            {
                _pressAnimationTimer = MathHelper.Max(0f, _pressAnimationTimer - deltaSeconds);
                return;
            }

            // Обработка циклической анимации после нажатия
            if (!IsPressed)
                return;

            _activeIdleTimer += deltaSeconds;
            if (_activeIdleTimer >= ActiveIdleFrameSeconds)
            {
                _activeIdleTimer -= ActiveIdleFrameSeconds;
                _activeIdleFrame = 1 - _activeIdleFrame;
            }
        }
    }
}