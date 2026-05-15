using Microsoft.Xna.Framework;
using Vibe_Game.Gameplay.Entities.Collectables;

namespace Vibe_Game.Core.Settings
{
    public static class WorldConfig
    {
        public const int TileSize = 32;

        public const int RoomWidthTiles = 15;
        public const int RoomHeightTiles = 9;

        public const int RoomWidthPx = RoomWidthTiles * TileSize;
        public const int RoomHeightPx = RoomHeightTiles * TileSize;

        public const int GridSize = 13;
        public const int CenterGrid = 6;
    }

    public static class FloorConfig
    {
        public const int FirstFloorIndex = 1;
        public const int BossFloorIndex = 2;
        public const int MaxFloorIndex = 2;
    }

    public static class PlayerConfig
    {
        public const int Size = 24;
        public const int Radius = Size / 2;
        public const float CollisionOffset = 11.9f;
        public const float BaseSpeed = 200f;
    }

    public static class EnemyConfig
    {
        /// <summary>Спрайт-лист анимации смерти (все враги, горизонтальные кадры).</summary>
        public const string EnemyDeathAnimation = "enemy_death_sheet";
        public const int DeathAnimationFramesCount = 4;
        public const float DeathAnimationFrameDurationSeconds = 0.1f;
        public const float DeathAnimationStartOpacity = 1f;
        public const float DeathAnimationEndOpacity = 0f;

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
        public const float AdaptiveChasingInitialRadius = 90f;
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
        public const float ShooterReentryShotCooldownSeconds = 0.55f;
        public const float ShooterProjectileSpeed = 150f;
        public const int ShooterProjectileDamage = 1;
        public const float ShooterProjectileLifetime = 2.2f;
        public const float ShooterProjectileRadius = 3f;
        public const float ShooterProjectileRecoilForce = 0f;
        public const float ShooterSpawnChancePerRoom = 0.4f;

        public const float BossMoveSpeed = 35f;
        public const int BossMaxHealth = 180;
        public const float BossRadius = 26f;
        /// <summary>Доля высоты спрайта босса, участвующей в хитбоксе (верхние 15% отрезаны).</summary>
        public const float BossHitboxVisibleHeightFraction = 0.85f;
        public const float BossAttackPauseMin = 0.95f;
        public const float BossAttackPauseMax = 1.55f;
        public const float BossSummonAttackWeight = 0.35f;
        public const float BossSummonShooterChance = 0.2f;
        public const int BossSummonMinCount = 2;
        public const int BossSummonMaxCount = 4;
        public const float BossSummonSpawnRadius = 30f;

        public const int BossSpikeBurstProjectileCount = 10;
        public const float BossSpikeBurstProjectileSpeed = 210f;
        public const float BossSpikeBurstProjectileLifetime = 2.5f;
        public const float BossSpikeBurstProjectileRadius = 6f;
        public const float BossSpikeBurstSpawnRadius = 22f;

        /// <summary>Длинные шипы burst-атаки: отдельный лист <see cref="BossLongProjectileTexture"/>.</summary>
        public const string BossLongProjectileTexture = "boss_long_projectile";
        public const int BossLongProjectileFrameColumns = 4;
        public const float BossLongProjectileDrawSize = 30f;

        public const int BossBurstSpecialProjectileCount = 4;
        public const float BossBurstSpecialProjectileSpeed = 260f;
        public const float BossBurstSpecialProjectileLifetime = 2.2f;
        public const float BossBurstSpecialProjectileRadius = 5f;
        public const float BossBurstSpecialProjectileLength = 34f;
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
        public const float BossBurrowStrikeRadius = 44f;
        public const bool BossInvulnerableDuringBurrow = true;

        public const string BossTexture = "boss_sheet";
        public const int BossSheetRotateRow = 0; // босс поворачивается
        public const int BossSheetAttackRow = 1; // босс начинает атаку(burst/spinning)
        public const int BossSheetFliesAttackIdleRow = 2; // босс во время того как атака со спавном противников происходит
        public const int BossSheetBurrowRow = 3; // босс зарывается под землю
        public const int BossSheetDiggingRow = 4; // босс ползет под землей
        public const int BossSheetIdleRow = 5; // босс когда не атакует и стоит на месте
        public const int BossSheetDeathRow = 6; // босс умирает

        public const int BossSheetFramesCount = 8; // всего кадров в спрайт листе
        public const int BossSheetCommonFramesCount = 4;// базовое количество кадров
        public const int BossSheetFliesAttckFramesCount = 8; // только тут 8 кадров, в остальных по 4
    }

    public static class WeaponConfig
    {
        public const string SwordTexture = "sword_sheet";
        public const string SwordTrailTexture = "sword_trail_particles";
        public const string PlayerProjectileTexture = "player_projectile_sheet";
        public const string EnemyProjectileTexture = "enemy_projectile_sheet";

        public const float SwordLength = 40f;
        public const float SwordWidth = 10f;
        public const float SwordAttackAngle = System.MathF.PI / 1.5f; // 120 градусов
        public const float SwordAttackDuration = 0.2f;
        public const int SwordDamage = 5;
        public const float SwordRecoilForce = 500f;

        public const int SwordTrailParticleCount = 10;
        public const int SwordTrailFrameCount = 8;
        public const float SwordTrailParticleSize = 6f;
        public const float SwordTrailAnimationSpeed = 0.1f;
        public const float SwordTrailParticleLifetime = 0.7f;
        public const float SwordTrailBrightnessVariation = 0.6f;

        public const int ProjectileFrameCount = 4;
        public const float ProjectileSize = 16f;
        public const float ProjectileAnimationSpeed = 0.06f;
        
        public const float PlayerProjectileSpeed = 300f;
        public const float PlayerProjectileLifetime = 1.8f;
        public const int PlayerProjectileDamage = 3;
        
        public const float EnemyProjectileSpeed = 150f;
        public const float EnemyProjectileLifetime = 2.2f;
        public const int EnemyProjectileDamage = 1;
    }

    public static class CollectibleConfig
    {
        /// <summary>
        /// Один спрайт-лист: строка <see cref="CollectablesSheetPedestalRow"/> — idle пьедестала (4 кадра в ряд),
        /// далее клыки, перо, тотем, малый/большой хил, пьедесталы оружия — см. <see cref="CollectablesSheetFangRow"/> … <see cref="CollectablesSheetWeaponSwordRow"/>.
        /// </summary>
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
        public const float FeatherSwordCooldownMultiplierPerStack = 0.92f;

        public const int FangBonusDamage = 1;

        public const float EnemyHealthDropChance = 0.15f;
        /// <summary>Доля дропа, при котором выпадает +2 HP (остальное — +1).</summary>
        public const float EnemyTwoHpDropWeight = 0.5f;

        public const int FloorPickupSize = 18;
        public const float FloorPickupBobSpeed = 4.2f;
        public const float FloorPickupBobAmplitude = 2.5f;

        /// <summary>Скорость idle-анимации сердец на полу (кадры из строки хила в спрайт-листе).</summary>
        public const float FloorPickupIdleAnimFps = 5f;

        public const float BasePlayerControllerMaxSpeed = 150f;
    }

    /// <summary>Пьедесталы: раскладка, имена PNG в Content, число кадров idle-анимации.</summary>
    public static class PedestalConfig
    {
        public const int SpriteFrameCount = 4;
        public const float IdleAnimFps = 5f;
        public const float PickupAnimDurationSeconds = 0.42f;

        /// <summary>Раскладка спрайтов пьедестала и предметов — в <see cref="CollectibleConfig.CollectablesTexture"/>.</summary>

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

        /// <summary>Порядок соответствует <see cref="StartingWeaponPedestalOffsetsFromCenter"/>.</summary>
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
        public const float ProjectileSpawnOffsetPixels = 2f;
        public const float ProjectileLifetimeSeconds = 1.5f;
        public const float ProjectileRadius = 4f;
        public const float ProjectileRecoilForce = 100f;
    }

    /// <summary>Конфиг полоски здоровья игрока из спрайт-листа 8x4.</summary>
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
        public const string BossStatic = "boss_static";         // заготовка 
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
        public static readonly Color MinimapChallenge = new Color(58, 134, 238);
        public static readonly Color MinimapCurrent = Color.Red;
        public static readonly Color MinimapDefault = new Color(96, 98, 108);
        public static readonly Color MinimapVisitedOutline = new Color(235, 235, 230);
        public static readonly Color RoomLabel = new Color(245, 245, 235);
        public static readonly Color RoomLabelShadow = new Color(20, 20, 26, 180);
        public static readonly Color FloorHint = new Color(232, 216, 160);
        public static readonly Color MenuBackground = new Color(12, 10, 18);
        public static readonly Color MenuPanel = new Color(28, 24, 36, 232);
        public static readonly Color MenuOutline = new Color(170, 150, 120);
        public static readonly Color MenuSelection = new Color(214, 162, 88);
        public static readonly Color MenuMuted = new Color(170, 170, 176);
        public static readonly Color MenuOverlay = new Color(8, 8, 12, 180);
    }
}
