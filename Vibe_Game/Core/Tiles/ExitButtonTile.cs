using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    public sealed class ExitButtonTile : Tile
    {
        private const float ActiveIdleFrameSeconds = 0.45f;

        private float _activeIdleTimer;
        private int _activeIdleFrame;

        public ExitButtonTile(Point gridPosition) : base(gridPosition)
        {
        }

        public bool IsActive { get; private set; }
        public int ActiveIdleFrame => _activeIdleFrame;
        public override bool CanHostEnemy => false;
        public override Color Tint => IsActive ? GameColors.ButtonUnlocked : GameColors.ButtonLocked;

        public void Activate()
        {
            IsActive = true;
        }

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
