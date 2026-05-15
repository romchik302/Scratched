using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    public sealed class ButtonTile : Tile
    {
        private const float PressAnimationSeconds = 0.18f;
        private const float ActiveIdleFrameSeconds = 0.45f;

        private float _pressAnimationTimer;
        private float _activeIdleTimer;
        private int _activeIdleFrame;

        public ButtonTile(Point gridPosition) : base(gridPosition)
        {
        }

        public bool IsPressed { get; private set; }
        public bool IsPressAnimationPlaying => _pressAnimationTimer > 0f;
        public int ActiveIdleFrame => _activeIdleFrame;
        public override bool HasButton => true;
        public override bool CanHostEnemy => false;
        public override Color Tint => IsPressed ? GameColors.ButtonUnlocked : GameColors.ButtonLocked;

        public void Press()
        {
            if (IsPressed)
                return;

            IsPressed = true;
            _pressAnimationTimer = PressAnimationSeconds;
            _activeIdleTimer = 0f;
            _activeIdleFrame = 0;
        }

        public void Update(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
                return;

            if (_pressAnimationTimer > 0f)
            {
                _pressAnimationTimer = MathHelper.Max(0f, _pressAnimationTimer - deltaSeconds);
                return;
            }

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
