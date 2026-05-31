using Microsoft.Xna.Framework;
using Vibe_Game.Gameplay.Entities.Collectables;

namespace Vibe_Game.Core.Settings
{
    /// <summary>Глобальные настройки сетки мира, размеров комнат и тайлов.</summary>
    public static class WorldConfig
    {
        /// <summary>Базовый размер одного квадратного тайла в пикселях.</summary>
        public const int TileSize = 32;

        /// <summary>Ширина комнаты в количестве тайлов.</summary>
        public const int RoomWidthTiles = 15;
        /// <summary>Высота комнаты в количестве тайлов.</summary>
        public const int RoomHeightTiles = 9;

        public const int RoomWidthPx = RoomWidthTiles * TileSize;
        public const int RoomHeightPx = RoomHeightTiles * TileSize;

        /// <summary>Сколько клеток от внутренней кромки двери остаётся свободным от камней и зарослей.</summary>
        public const int DoorObstacleClearanceTiles = 2;

        /// <summary>Минимальное расстояние (радиус по Чебышёву) между камнями при генерации комнаты.</summary>
        public const int ObstacleMinSeparationTiles = 2;

        /// <summary>Максимальный размер сетки этажа (комнат по ширине и высоте).</summary>
        public const int GridSize = 13;
        /// <summary>
        /// Координата центральной точки матрицы этажа, определяющая фиксированное местоположение стартовой комнаты.
        /// </summary>
        public const int CenterGrid = 6;
    }

    /// <summary>Настройки генерации этажей и общей прогрессии забега.</summary>
    public static class FloorConfig
    {
        public const int FirstFloorIndex = 1;
        public const int BossFloorIndex = 2;
        public const int MaxFloorIndex = 2;
    }

    /// <summary>Базовые параметры главного героя (хитбокс и скорость перемещения).</summary>
    public static class PlayerConfig
    {
        public const int Size = 24;
        public const int Radius = Size / 2;
        /// <summary>
        /// Внутреннее смещение хитбокса коллизии относительно графического спрайта для реализации эффекта глубины и наложения объектов (Y-sorting).
        /// </summary>
        public const float CollisionOffset = 11.9f;

        /// <summary>Базовая скорость перемещения игрока в пикселях в секунду.</summary>
        public const float BaseSpeed = 200f;
    }

    /// <summary>Характеристики всех противников и босса (здоровье, скорости, шансы спавна и параметры атак).</summary>
    public static class EnemyConfig
    {
        /// <summary>Спрайт-лист анимации смерти (все враги, горизонтальные кадры).</summary>
        public const string EnemyDeathAnimation = "enemy_death_sheet";
        public const int DeathAnimationFramesCount = 4;
        public const float DeathAnimationFrameDurationSeconds = 0.1f;
        public const float DeathAnimationStartOpacity = 1f;
        public const float DeathAnimationEndOpacity = 0f;

        /// <summary>
        /// Задержка активации логики ИИ врага после его появления, предотвращающая мгновенное нанесение урона игроку на входе в комнату.
        /// </summary>
        public const float EnemyActivationDelaySeconds = 0.6f;

        public const int DefaultFlyingRadius = 8;
        public const float DefaultFlyingMoveSpeed = 85f;
        public const int DefaultFlyingMaxHealth = 8;
        public const float FlyingSpawnChancePerRoom = 0.45f;

        public const int DefaultChasingRadius = 12;
        public const float DefaultChasingMoveSpeed = 50f;
        public const int DefaultChasingMaxHealth = 10;
        public const float ChasingSpawnChancePerRoom = 0.35f;

        public const float AdaptiveChasingMoveSpeed = 70f;
        public const int AdaptiveChasingMaxHealth = 20;
        public const float AdaptiveChasingRadius = 15f;
        /// <summary>
        /// Исходный радиус обнаружения игрока, при входе в который адаптивный противник переходит в состояние погони.
        /// </summary>
        public const float AdaptiveChasingInitialRadius = 90f;
        /// <summary>
        /// Расширенный радиус удержания цели, позволяющий противнику не терять игрока из виду при резких маневрах или выходе из базовой зоны агро.
        /// </summary>
        public const float AdaptiveChasingExpandedRadius = 200f;
        public const float AdaptiveChasingSpawnChance = 0.3f;

        public const int AdaptiveChasingFrameCount = 4;
        public const float AdaptiveChasingAnimationSpeed = 0.05f;
        public const int AdaptiveChasingAnimationRows = 2;

        public const float ShooterRadius = 10f;
        public const float ShooterMoveSpeed = 80f;
        public const int ShooterMaxHealth = 12;
        public const float ShooterAggroRadius = 100f;
        public const float ShooterShotIntervalSeconds = 0.7f;
        /// <summary>
        /// Кулдаун перед первым выстрелом стрелка после смены позиции или входа в состояние атаки.
        /// </summary>
        public const float ShooterReentryShotCooldownSeconds = 0.55f;
        public const float ShooterProjectileSpeed = 150f;
        public const int ShooterProjectileDamage = 1;
        public const float ShooterProjectileLifetime = 2.2f;
        public const float ShooterProjectileRadius = 4f;
        public const float ShooterProjectileRecoilForce = 0f;
        public const float ShooterSpawnChancePerRoom = 0.4f;

        /// <summary>Базовое максимальное здоровье финального босса.</summary>
        public const int BossMaxHealth = 180;
        public const float BossMoveSpeed = 35f;
        public const float BossRadius = 26f;
        /// <summary>Доля высоты спрайта босса, участвующей в хитбоксе (верхние 15% отрезаны).</summary>
        public const float BossHitboxVisibleHeightFraction = 0.85f;
        public const float BossAttackPauseMin = 0.95f;
        public const float BossAttackPauseMax = 1.55f;
        /// <summary>
        /// Математический вес (вероятность выбора) атаки призыва миньонов в дереве поведений босса.
        /// </summary>
        public const float BossSummonAttackWeight = 0.35f;
        public const float BossSummonShooterChance = 0.2f;
        public const int BossSummonMinCount = 2;
        public const int BossSummonMaxCount = 4;
        public const float BossSummonSpawnRadius = 30f;
        /// <summary>Сколько полных циклов анимации строки BossSheetFliesAttackIdleRow играет во время призыва.</summary>
        public const int BossSummonFliesAnimLoopCount = 3;

        public const int BossSpikeBurstProjectileCount = 10;
        public const float BossSpikeBurstProjectileSpeed = 210f;
        public const float BossSpikeBurstProjectileLifetime = 2.5f;
        public const float BossSpikeBurstProjectileRadius = 6f;
        public const float BossSpikeBurstSpawnRadius = 22f;

        /// <summary>Длинные шипы burst-атаки: отдельный лист BossLongProjectileTexture.</summary>
        public const string BossLongProjectileTexture = "boss_long_projectile";
        public const int BossLongProjectileFrameColumns = 4;
        public const float BossLongProjectileDrawSize = 30f;

        public const int BossBurstSpecialProjectileCount = 4;
        public const float BossBurstSpecialProjectileSpeed = 260f;
        public const float BossBurstSpecialProjectileLifetime = 2.2f;
        public const float BossBurstSpecialProjectileRadius = 5f;
        public const float BossBurstSpecialProjectileLength = 34f;
        /// <summary>
        /// Половина угла общего конуса разлета (в радианах) для веерной атаки особыми снарядами.
        /// </summary>
        public const float BossBurstSpecialSpreadHalfAngle = 0.35f;

        /// <summary>Строк в спрайт-листе босса (0..N-1).</summary>
        public const int BossSheetRowCount = 7;

        public const float BossCommonAnimFrameDurationSeconds = 0.11f;
        public const float BossFlyIdleAnimFrameDurationSeconds = 0.09f;
        public const float BossIdleRowFrameDurationSeconds = 0.14f;

        public const int BossDeathAnimationFramesCount = 4;
        public const float BossDeathAnimationFrameDurationSeconds = 0.12f;

        public const int BossSpinningSpikeCount = 7;
        public const float BossSpinningSpikeOrbitRadius = 70f;
        public const float BossSpinningSpikeAngularSpeed = 2.2f;
        public const float BossSpinningSpikeOrbitDuration = 2f;
        public const float BossSpinningSpikeReleaseSpeed = 150f;

        public const float BossBurrowTravelDuration = 1.5f;
        public const float BossBurrowTrailSpeed = 70f;
        /// <summary>
        /// Радиус зоны поражения при выныривании босса из-под земли под персонажем игрока.
        /// </summary>
        public const float BossBurrowStrikeRadius = 44f;
        public const bool BossInvulnerableDuringBurrow = true;

        public const string BossTexture = "boss_sheet";
        public const int BossSheetRotateRow = 0;
        public const int BossSheetAttackRow = 1;
        public const int BossSheetFliesAttackIdleRow = 2;
        public const int BossSheetBurrowRow = 3;
        public const int BossSheetDiggingRow = 4;
        public const int BossSheetIdleRow = 5;
        public const int BossSheetDeathRow = 6;

        public const int BossSheetFramesCount = 8;
        public const int BossSheetCommonFramesCount = 4;
        public const int BossSheetFliesAttckFramesCount = 8;
    }

    /// <summary>Настройки оружия (урон, кулдауны, скорости снарядов и параметры визуальных эффектов).</summary>
    public static class WeaponConfig
    {
        public const string SwordTexture = "sword_sheet";
        public const string SwordTrailTexture = "sword_trail_particles";
        public const string PlayerProjectileTexture = "player_projectile_sheet";
        public const string EnemyProjectileTexture = "enemy_projectile_sheet";

        public const float SwordLength = 40f;
        public const float SwordWidth = 10f;
        public const float SwordAttackAngle = System.MathF.PI / 1.5f;
        public const float SwordAttackDuration = 0.2f;

        /// <summary>Базовый урон меча при атаке в ближнем бою.</summary>
        public const int SwordDamage = 5;
        public const float SwordRecoilForce = 500f;

        public const int SwordTrailParticleCount = 10;
        public const int SwordTrailFrameCount = 8;
        public const float SwordTrailParticleSize = 6f;
        public const float SwordTrailAnimationSpeed = 0.1f;
        public const float SwordTrailParticleLifetime = 0.7f;
        /// <summary>
        /// Максимальная амплитуда случайного отклонения яркости генерируемой частицы шлейфа от её базового цвета.
        /// </summary>
        public const float SwordTrailBrightnessVariation = 0.6f;

        public const int ProjectileFrameCount = 4;
        public const float ProjectileSize = 16f;
        public const float ProjectileAnimationSpeed = 0.06f;

        public const float PlayerProjectileSpeed = 300f;
        public const float PlayerProjectileLifetime = 1.8f;
        /// <summary>Базовый урон от одного выстрела игрока.</summary>
        public const int PlayerProjectileDamage = 3;

        public const float EnemyProjectileSpeed = 150f;
        public const float EnemyProjectileLifetime = 2.2f;
        public const int EnemyProjectileDamage = 1;
    }

    /// <summary>Параметры подбираемых предметов, артефактов и хила (баффы, шансы дропа).</summary>
    public static class CollectibleConfig
    {
        /// <summary>Один спрайт-лист: строка CollectablesSheetPedestalRow — idle пьедестала (4 кадра в ряд), далее клыки, перо, тотем, малый/большой хил, пьедесталы оружия.</summary>
        public const string CollectablesTexture = "collectables_sheet";

        /// <summary>Кадров в одной строке (idle пьедестала и предмета).</summary>
        public const int CollectablesSheetFramesPerRow = 4;

        /// <summary>Строки листа сверху вниз (индекс 0 — верх файла).</summary>
        public const int CollectablesSheetPedestalRow = 0;
        public const int CollectablesSheetFangRow = 1;
        public const int CollectablesSheetFeatherRow = 2;
        public const int CollectablesSheetTotemRow = 3;
        public const int CollectablesSheetHealthSmallRow = 4;
        public const int CollectablesSheetHealthLargeRow = 5;
        public const int CollectablesSheetWeaponProjectileRow = 6;
        public const int CollectablesSheetWeaponSwordRow = 7;

        public const int CollectablesSheetRowCount = 8;

        public const float FeatherSpeedMultiplierBonus = 0.12f;
        public const float FeatherProjectileSpeedMultiplierBonus = 0.1f;
        /// <summary>
        /// Мультипликативный множитель изменения времени перезарядки меча за каждый стек предмета «Перо» (уменьшает кулдаун).
        /// </summary>
        public const float FeatherSwordCooldownMultiplierPerStack = 0.92f;

        public const int FangBonusDamage = 1;

        /// <summary>Вероятность выпадения здоровья после смерти врага.</summary>
        public const float EnemyHealthDropChance = 0.15f;
        /// <summary>Доля дропа, при котором выпадает +2 HP (остальное — +1).</summary>
        public const float EnemyTwoHpDropWeight = 0.5f;

        public const int FloorPickupSize = 18;
        /// <summary>
        /// Частота вертикального покачивания (анимации левитации) предмета на полу, используемая в функции синуса.
        /// </summary>
        public const float FloorPickupBobSpeed = 4.2f;

        /// <summary>
        /// Максимальное отклонение по оси Y (в пикселях) при покачивании предмета на полу.
        /// </summary>
        public const float FloorPickupBobAmplitude = 2.5f;

        /// <summary>Скорость idle-анимации сердец на полу (кадры из строки хила в спрайт-листе).</summary>
        public const float FloorPickupIdleAnimFps = 5f;

        public const float BasePlayerControllerMaxSpeed = 150f;
    }

    /// <summary>Настройки генерации и отрисовки пьедесталов для артефактов и стартового оружия.</summary>
    public static class PedestalConfig
    {
        public const int SpriteFrameCount = 4;
        public const float IdleAnimFps = 5f;
        public const float PickupAnimDurationSeconds = 0.42f;

        /// <summary>Случайный лут на обычных пьедесталах (не стартовое оружие).</summary>
        public static readonly CollectableKind[] StandardLootKinds =
        {
            CollectableKind.Totem,
            CollectableKind.Feather,
            CollectableKind.Fang
        };

        /// <summary>Смещения плиток пьедесталов выбора оружия относительно центра комнаты [слева, справа].</summary>
        public static readonly Point[] StartingWeaponPedestalOffsetsFromCenter =
        {
            new Point(-2, 1),
            new Point(2, 1)
        };

        /// <summary>Порядок соответствует StartingWeaponPedestalOffsetsFromCenter.</summary>
        public static readonly CollectableKind[] StartingWeaponPedestalKinds =
        {
            CollectableKind.WeaponProjectile,
            CollectableKind.WeaponSword
        };

        /// <summary>Множитель масштаба спрайта основания пьедестала относительно размера тайла (после деления на размер кадра).</summary>
        public const float PedestalBaseScaleMultiplier = 0.65f;

        /// <summary>Смещение основания пьедестала в пикселях (от центра тайла).</summary>
        public const float PedestalBaseOffsetXPixels = 0f;
        public const float PedestalBaseOffsetYPixels = 4f;

        /// <summary>Множитель масштаба предмета на пьедестале (относительно размера тайла).</summary>
        public const float CollectableOnPedestalScaleMultiplier = 0.7f;

        /// <summary>Смещение предмета на пьедестале в пикселях (от центра тайла).</summary>
        public const float CollectableOnPedestalOffsetXPixels = 0f;

        /// <summary>Поднять предмет над центром тайла на столько пикселей (+Y вверх на экране).</summary>
        public const float CollectableOnPedestalOffsetYUpPixels = 4f;
    }

    /// <summary>Параметры экземпляров оружия при выборе на первом этаже.</summary>
    public static class StartingWeaponConfig
    {
        public const float SwordCooldownSeconds = 0.4f;

        public const float ProjectileCooldownSeconds = 0.35f;
        public const float ProjectileSpeed = 205f;
        public const int ProjectileDamage = 3;
        /// <summary>
        /// Расстояние в пикселях от центра игрока, на котором порождается снаряд, предотвращающее мгновенную коллизию с хитбоксом стреляющего.
        /// </summary>
        public const float ProjectileSpawnOffsetPixels = 2f;
        public const float ProjectileLifetimeSeconds = 1.5f;
        public const float ProjectileRadius = 4f;
        public const float ProjectileRecoilForce = 100f;
    }

    /// <summary>Конфиг отрисовки полоски здоровья игрока из спрайт-листа 8x4.</summary>
    public static class HealthHudConfig
    {
        public const string TextureAsset = "healthbar_sheet";

        public const int Columns = 4;
        public const int Rows = 8;

        public const int FullIdleRow = 0;
        public const int HalfIdleRow = 1;
        public const int EmptyIdleRow = 2;
        public const int EmptyToHalfRow = 3;
        public const int HalfToEmptyRow = 4;
        public const int FullToHalfRow = 5;
        public const int EmptyToFullRow = 6;
        public const int HalfToFullRow = 7;

        public const float IdleFrameDurationSeconds = 0.08f;
        public const float TransitionFrameDurationSeconds = 0.05f;
        /// <summary>Пауза между анимациями соседних ячеек в одном проходе.</summary>
        public const float IdleCellIntervalSeconds = 0.1f;
        /// <summary>Пауза перед запуском нового полного прохода по всем ячейкам.</summary>
        public const float IdleCycleIntervalSeconds = 3f;

        public const int CellWidth = 50;
        public const int CellHeight = 50;
        public const int CellSpacing = 4;
        public const int MarginRight = 20;
        public const int MarginTop = 20;
        public const int CellOffsetX = 2;
        public const int CellOffsetY = 2;

        public const float ExtraLivesTextScale = 0.55f;
        public const int ExtraLivesTextOffsetY = 4;
    }

    /// <summary>Имена аудиофайлов и настройки звуковой системы (громкость, шаги, музыка).</summary>
    public static class SoundConfig
    {
        // Эффекты игрока
        public const string PlayerSwordAttack1 = "mc_sword_attack1";
        public const string PlayerSwordAttack2 = "mc_sword_attack2";
        public const string PlayerRangedAttack = "mc_ranged_attack";
        public const string PlayerFootstepsGrass = "mc_footsteps_grass";
        public const string PlayerFootstepsStone = "mc_footsteps_stone";
        public const string PlayerGetHit = "mc_get_hit";
        public const string PlayerHeal = "mc_heal";
        public const string PlayerDeath = "mc_death";

        // Эффекты врагов
        public const string EnemyDeath = "enemy_death";
        public const string EnemyFly = "enemy_fly";
        public const string EnemySlime = "enemy_slime";
        public const string EnemyTreant = "enemy_treant";

        // Эффекты босса
        public const string BossDeath = "boss_death";
        public const string BossEntering = "boss_emerge";
        public const string BossBurrow = "boss_burrow";
        public const string BossEmerge = "boss_emerge";
        public const string BossStatic = "boss_static";
        public const string BossAttack = "boss_attack";

        // Эффекты UI и карты
        public const string UIConfirm = "ui_confirm";
        public const string UISelect = "ui_select";
        public const string MapBossEntry = "map_boss_entry";
        public const string MapButton = "map_button";
        public const string MapClosedDoor = "map_closeddoor";
        public const string MapDoorUnlock = "map_door_unlock";
        public const string MapOpenDoor = "map_opendoor";

        // Музыка
        public const string MusicMainMenu = "music_main_menu";
        public const string MusicEmptyRoom = "music_empty_room";
        public const string MusicCombatRoom = "music_combat_room";
        public const string MusicBossRoom = "music_boss_room";
        public const string MusicCredits = "music_credits";

        // Переходы (звуки очистки комнаты)
        public const string RoomCleared = "room_cleared";

        public const float DefaultSoundEffectVolume = 0.3f;

        /// <summary>Вероятность звука «камень» при шаге на обычной поверхности (трава — дополнение до 1).</summary>
        public const float FootstepStoneProbability = 0.32f;

        /// <summary>Вероятность «камня», если рядом с ногами есть каменный тайл (камень всё ещё не доминирует).</summary>
        public const float FootstepStoneBiasWhenOnStone = 0.55f;

        /// <summary>Накопленное смещение в пикселях для одного шага.</summary>
        public const float FootstepStridePixels = 26f;

        /// <summary>Минимальный интервал между шагами (сек), даже при быстром движении.</summary>
        public const float FootstepMinIntervalSeconds = 0.4f;
    }

    /// <summary>Глобальная палитра цветов, используемая для отрисовки интерфейса, миникарты и базовой геометрии.</summary>
    public static class GameColors
    {
        public static readonly Color Background = new Color(15, 10, 20);
        public static readonly Color Floor = new Color(35, 25, 40);
        public static readonly Color Wall = new Color(70, 60, 80);
        public static readonly Color Rock = new Color(82, 78, 88);
        public static readonly Color Overgrowth = new Color(58, 92, 58);
        public static readonly Color Pedestal = new Color(126, 108, 84);
        public static readonly Color CollectablePlaceholder = new Color(238, 210, 104);
        public static readonly Color Trapdoor = new Color(120, 82, 44);
        public static readonly Color TrapdoorRim = new Color(200, 160, 95);

        public static readonly Color ButtonLocked = Color.Yellow;
        public static readonly Color ButtonUnlocked = Color.Lime;

        public static readonly Color MinimapStart = new Color(210, 212, 218);
        public static readonly Color MinimapBattle = new Color(96, 98, 108);
        public static readonly Color MinimapBoss = new Color(224, 42, 48);
        public static readonly Color MinimapTreasure = new Color(240, 202, 58);
        public static readonly Color MinimapCurrent = Color.Red;
        public static readonly Color MinimapDefault = new Color(96, 98, 108);
        public static readonly Color MinimapVisitedOutline = new Color(216, 188, 88);
        public static readonly Color MinimapCurrentOutline = new Color(236, 232, 148);
        public static readonly Color RoomLabel = new Color(245, 245, 235);
        public static readonly Color RoomLabelShadow = new Color(20, 20, 26, 180);
        public static readonly Color FloorHint = new Color(232, 216, 160);

        public static readonly Color MenuBackground = new Color(18, 24, 16);
        public static readonly Color MenuPanel = new Color(36, 44, 30, 238);
        public static readonly Color MenuOutline = new Color(118, 103, 76);
        public static readonly Color MenuSelection = new Color(126, 138, 70);
        public static readonly Color MenuMuted = new Color(164, 166, 124);
        public static readonly Color MenuOverlay = new Color(8, 12, 8, 184);

        public static readonly Color DeathBackground = new Color(10, 2, 4);
        public static readonly Color DeathPanel = new Color(38, 8, 12, 242);
        public static readonly Color DeathOutline = new Color(142, 24, 30);
        public static readonly Color DeathText = new Color(238, 42, 48);
        public static readonly Color DeathMuted = new Color(154, 90, 88);

        public static readonly Color VictoryBackground = new Color(24, 25, 10);
        public static readonly Color VictoryPanel = new Color(55, 50, 24, 238);
        public static readonly Color VictoryOutline = new Color(236, 190, 82);
        public static readonly Color VictoryText = new Color(255, 235, 132);
        public static readonly Color VictoryAccent = new Color(142, 190, 92);
    }
}