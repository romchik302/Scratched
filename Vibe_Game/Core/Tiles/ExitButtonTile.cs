using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>
    /// Представляет кнопку выхода с уровня. 
    /// Может находиться в заблокированном или активном состоянии и имеет циклическую анимацию в активном режиме.
    /// </summary>
    public sealed class ExitButtonTile : Tile
    {
        private const float ActiveIdleFrameSeconds = 0.45f;
        private float _activeIdleTimer;
        private int _activeIdleFrame;

        /// <summary>
        /// Инициализирует новый экземпляр класса ExitButtonTile.
        /// </summary>
        /// <param name="gridPosition">Позиция кнопки в сетке уровня.</param>
        public ExitButtonTile(Point gridPosition) : base(gridPosition)
        {
        }

        /// <summary>Флаг состояния кнопки. True — кнопка доступна (активна), False — заблокирована.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Текущий индекс кадра анимации в активном состоянии (0 или 1).</summary>
        public int ActiveIdleFrame => _activeIdleFrame;

        /// <inheritdoc />
        public override bool CanHostEnemy => false;

        /// <inheritdoc />
        public override Color Tint => IsActive ? GameColors.ButtonUnlocked : GameColors.ButtonLocked;

        /// <summary>
        /// Активирует кнопку выхода, открывая возможность перехода.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Обновляет таймеры циклической анимации. 
        /// Должен вызываться системой обработки уровня для каждой активной кнопки.
        /// </summary>
        /// <param name="deltaSeconds">Время, прошедшее с последнего кадра в секундах.</param>
        public void Update(float deltaSeconds)
        {
            if (!IsActive || deltaSeconds <= 0f)
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