using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Vibe_Game.Gameplay.Weapons
{
    /// <summary>
    /// Определяет контракт для игрового оружия, управляющего логикой атаки, обновлением состояния и визуальным отображением.
    /// </summary>
    public interface IWeapon
    {
        /// <summary>Имя для UI/отладки.</summary>
        string DisplayName { get; }

        /// <summary>Когда именно вызывать <see cref="TryPrimaryAttack"/> из игрока.</summary>
        WeaponFireMode FireMode { get; }

        /// <summary>Базовая отдача оружия (сила толчка в обратную сторону от направления атаки).</summary>
        float BaseRecoil { get; }

        /// <summary>
        /// Обновляет внутреннее состояние оружия, включая таймеры перезарядки и логику работы механизмов.
        /// </summary>
        /// <param name="gameTime">Текущее игровое время.</param>
        /// <param name="context">Контекст выполнения боевых действий и взаимодействия с миром.</param>
        void Update(GameTime gameTime, IAttackContext context);

        /// <summary>
        /// Пытается выполнить основную атаку оружия из указанной позиции в заданном направлении.
        /// </summary>
        /// <param name="context">Контекст боевой системы для регистрации попаданий или спавна снарядов.</param>
        /// <param name="ownerPosition">Позиция владельца оружия в мировых координатах.</param>
        /// <param name="facingDirection">Вектор направления атаки.</param>
        /// <returns>Значение <see langword="true"/>, если атака была успешно произведена; иначе — <see langword="false"/>.</returns>
        bool TryPrimaryAttack(IAttackContext context, Vector2 ownerPosition, Vector2 facingDirection);

        /// <summary>
        /// Отрисовывает визуальные элементы, спрайты или графические эффекты оружия в игровом мире.
        /// </summary>
        /// <param name="spriteBatch">Пакет спрайтов для графического вывода.</param>
        /// <param name="context">Контекст боевой системы.</param>
        void Draw(SpriteBatch spriteBatch, IAttackContext context);
    }
}
