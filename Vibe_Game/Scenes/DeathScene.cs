using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Scenes
{
    /// <summary>
    /// Сцена экрана смерти (Game Over).
    /// Выводит сообщение о проигрыше, воспроизводит звуковой эффект подтверждения 
    /// и возвращает игрока в главное меню при нажатии любой игровой клавиши.
    /// </summary>
    internal sealed class DeathScene : BaseScene
    {
        private readonly IInputService _inputService;
        private SpriteFont _font;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DeathScene"/>.
        /// </summary>
        /// <param name="game">Основной экземпляр игры MonoGame.</param>
        /// <param name="inputService">Сервис для проверки нажатия клавиш.</param>
        public DeathScene(Game game, IInputService inputService)
            : base(game)
        {
            _inputService = inputService;
        }

        /// <inheritdoc />
        public override void LoadContent()
        {
            _font = GameInstance.Content.Load<SpriteFont>("room_font");
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (IsAnySkipActionPressed())
            {
                GameplayAudio.PlayUiConfirm();
                ((Game1)GameInstance).ShowMainMenu();
            }
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GetSpriteBatch();
            Texture2D pixel = GetPixelTexture();
            if (spriteBatch == null || pixel == null || _font == null)
                return;

            Viewport viewport = GameInstance.GraphicsDevice.Viewport;
            Rectangle panelRect = new Rectangle(viewport.Width / 2 - 260, viewport.Height / 2 - 160, 520, 320);

            GameInstance.GraphicsDevice.Clear(GameColors.DeathBackground);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            spriteBatch.Draw(pixel, panelRect, GameColors.DeathPanel);
            DrawBorder(spriteBatch, pixel, panelRect);
            DrawCenteredText(spriteBatch, "x_x", new Vector2(viewport.Width / 2f, panelRect.Y + 84f), GameColors.DeathText, 1.7f, GameColors.RoomLabelShadow);
            DrawCenteredText(spriteBatch, "hah loser", new Vector2(viewport.Width / 2f, panelRect.Y + 164f), GameColors.DeathText, 1.05f, GameColors.RoomLabelShadow);
            DrawCenteredText(spriteBatch, "PRESS E TO TRY AGAIN", new Vector2(viewport.Width / 2f, panelRect.Bottom - 66f), GameColors.DeathMuted, 0.62f);

            spriteBatch.End();
        }

        /// <summary>Проверяет игровые действия, которыми можно закрыть окно смерти.</summary>
        private bool IsAnySkipActionPressed()
        {
            return _inputService.IsActionPressed(InputAction.Interact);
        }

        /// <summary>Рисует рамку панели смерти в общем стиле меню.</summary>
        private static void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel, Rectangle panelRect)
        {
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Y - 3, panelRect.Width + 6, 3), GameColors.DeathOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Bottom, panelRect.Width + 6, 3), GameColors.DeathOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Y, 3, panelRect.Height), GameColors.DeathOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.Right, panelRect.Y, 3, panelRect.Height), GameColors.DeathOutline);
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
