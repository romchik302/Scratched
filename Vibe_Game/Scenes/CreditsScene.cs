using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Scenes
{
    internal sealed class CreditsScene : BaseScene
    {
        private readonly IInputService _inputService;
        private SpriteFont _font;

        public CreditsScene(Game game, IInputService inputService)
            : base(game)
        {
            _inputService = inputService;
        }

        /// <summary>Загружает шрифт, которым рисуется временный экран титров.</summary>
        public override void LoadContent()
        {
            _font = GameInstance.Content.Load<SpriteFont>("room_font");
        }

        /// <summary>Проверяет нажатие любой игровой клавиши и возвращает игрока в главное меню.</summary>
        public override void Update(GameTime gameTime)
        {
            if (IsAnySkipActionPressed())
                ((Game1)GameInstance).ShowMainMenu();
        }

        /// <summary>Рисует отдельный экран титров с временным текстом.</summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GetSpriteBatch();
            Texture2D pixel = GetPixelTexture();
            if (spriteBatch == null || pixel == null || _font == null)
                return;

            Viewport viewport = GameInstance.GraphicsDevice.Viewport;
            Rectangle panelRect = new Rectangle(viewport.Width / 2 - 300, viewport.Height / 2 - 180, 600, 360);

            GameInstance.GraphicsDevice.Clear(GameColors.MenuBackground);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            spriteBatch.Draw(pixel, panelRect, GameColors.MenuPanel);
            DrawBorder(spriteBatch, pixel, panelRect);
            DrawCenteredText(spriteBatch, "CREDITS", new Vector2(viewport.Width / 2f, panelRect.Y + 76f), GameColors.RoomLabel, 1.25f, GameColors.RoomLabelShadow);
            DrawCenteredText(spriteBatch, "TEXT WILL BE ADDED LATER", new Vector2(viewport.Width / 2f, panelRect.Y + 158f), GameColors.MenuMuted, 0.75f);
            DrawCenteredText(spriteBatch, "PRESS ANY GAME KEY TO SKIP", new Vector2(viewport.Width / 2f, panelRect.Bottom - 78f), GameColors.FloorHint, 0.62f);

            spriteBatch.End();
        }

        /// <summary>Проверяет все игровые действия, которыми можно пропустить титры.</summary>
        private bool IsAnySkipActionPressed()
        {
            return _inputService.IsActionPressed(InputAction.MoveUp) ||
                   _inputService.IsActionPressed(InputAction.MoveDown) ||
                   _inputService.IsActionPressed(InputAction.MoveLeft) ||
                   _inputService.IsActionPressed(InputAction.MoveRight) ||
                   _inputService.IsActionPressed(InputAction.ShootUp) ||
                   _inputService.IsActionPressed(InputAction.ShootDown) ||
                   _inputService.IsActionPressed(InputAction.ShootLeft) ||
                   _inputService.IsActionPressed(InputAction.ShootRight) ||
                   _inputService.IsActionPressed(InputAction.Fire) ||
                   _inputService.IsActionPressed(InputAction.Pause) ||
                   _inputService.IsActionPressed(InputAction.Interact);
        }

        /// <summary>Рисует рамку панели титров тем же стилем, что и остальные меню.</summary>
        private static void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel, Rectangle panelRect)
        {
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Y - 3, panelRect.Width + 6, 3), GameColors.MenuOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Bottom, panelRect.Width + 6, 3), GameColors.MenuOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Y, 3, panelRect.Height), GameColors.MenuOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.Right, panelRect.Y, 3, panelRect.Height), GameColors.MenuOutline);
        }

        /// <summary>Рисует текст по центру выбранной точки с необязательной тенью.</summary>
        private void DrawCenteredText(SpriteBatch spriteBatch, string text, Vector2 center, Color color, float scale, Color? shadowColor = null)
        {
            Vector2 size = _font.MeasureString(text) * scale;
            Vector2 position = center - size / 2f;

            if (shadowColor.HasValue)
                spriteBatch.DrawString(_font, text, position + new Vector2(2f, 2f), shadowColor.Value, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            spriteBatch.DrawString(_font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
