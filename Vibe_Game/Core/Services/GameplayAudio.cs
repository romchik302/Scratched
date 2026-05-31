using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Services;

/// <summary>
/// Статический сервис для управления звуковыми эффектами (SFX) и фоновой музыкой (BGM) в игровом процессе.
/// Обеспечивает плавные переходы между треками, управление громкостью и безопасную загрузку ассетов.
/// </summary>
public static class GameplayAudio
{
    /// <summary>Хранилище загруженных звуковых эффектов.</summary>
    private static readonly Dictionary<string, SoundEffect> Effects = new(StringComparer.Ordinal);
    private static Song _songEmptyRoom;
    private static Song _songCombatRoom;
    private static Song _songBossRoom;
    private static Song _songMainMenu;
    private static Song _songCredits;

    private static float _emptyRoomVolume = 0.25f;
    private static float _combatRoomVolume = 0.08f;
    private static float _bossRoomVolume = 0.08f;
    private static float _menuVolume = 0.3f;

    private enum Bgm
    {
        None,
        MainMenu,
        EmptyRoom,
        CombatRoom,
        BossRoom,
        Credits
    }

    private static Bgm _activeBgm = Bgm.None;
    private static TimeSpan _savedEmptyRoomPosition;
    private static bool _hasSavedEmptyRoomPosition;

    private static Song _pendingSong;
    private static Bgm _pendingBgm;

    private static bool _isTransitioning;
    private static bool _fadeOutPhase = true;

    private static float _musicFadeSpeed = 0.25f;
    private static float _targetVolume = 0.3f;

    /// <summary>
    /// Загружает все необходимые звуковые эффекты и музыкальные композиции из контента игры.
    /// </summary>
    /// <param name="content">Менеджер контента MonoGame для загрузки ресурсов.</param>
    public static void Load(ContentManager content)
    {
        Effects.Clear();
        TryAddEffect(content, SoundConfig.PlayerSwordAttack1);
        TryAddEffect(content, SoundConfig.PlayerSwordAttack2);
        TryAddEffect(content, SoundConfig.PlayerRangedAttack);
        TryAddEffect(content, SoundConfig.PlayerFootstepsGrass);
        TryAddEffect(content, SoundConfig.PlayerFootstepsStone);
        TryAddEffect(content, SoundConfig.PlayerGetHit);
        TryAddEffect(content, SoundConfig.PlayerHeal);
        TryAddEffect(content, SoundConfig.PlayerDeath);
        TryAddEffect(content, SoundConfig.EnemyDeath);
        TryAddEffect(content, SoundConfig.EnemyFly);
        TryAddEffect(content, SoundConfig.EnemySlime);
        TryAddEffect(content, SoundConfig.EnemyTreant);
        TryAddEffect(content, SoundConfig.BossDeath);
        TryAddEffect(content, SoundConfig.BossEntering);
        TryAddEffect(content, SoundConfig.BossBurrow);
        TryAddEffect(content, SoundConfig.BossEmerge);
        TryAddEffect(content, SoundConfig.BossStatic);
        TryAddEffect(content, SoundConfig.BossAttack);
        TryAddEffect(content, SoundConfig.UIConfirm);
        TryAddEffect(content, SoundConfig.UISelect);
        TryAddEffect(content, SoundConfig.MapBossEntry);
        TryAddEffect(content, SoundConfig.MapButton);
        TryAddEffect(content, SoundConfig.MapClosedDoor);
        TryAddEffect(content, SoundConfig.MapDoorUnlock);
        TryAddEffect(content, SoundConfig.MapOpenDoor);
        TryAddEffect(content, SoundConfig.RoomCleared);

        _songEmptyRoom = TryLoadSong(content, SoundConfig.MusicEmptyRoom);
        _songCombatRoom = TryLoadSong(content, SoundConfig.MusicCombatRoom);
        _songBossRoom = TryLoadSong(content, SoundConfig.MusicBossRoom);
        _songMainMenu = TryLoadSong(content, SoundConfig.MusicMainMenu);
        _songCredits = TryLoadSong(content, SoundConfig.MusicCredits);

        MediaPlayer.Volume = _menuVolume;
    }

    /// <summary>
    /// Сбрасывает сохраненную позицию проигрывания музыки исследования.
    /// Полезно при начале нового забега или смене уровня.
    /// </summary>
    public static void ResetExplorationMusicBookmark()
    {
        _savedEmptyRoomPosition = TimeSpan.Zero;
        _hasSavedEmptyRoomPosition = false;
    }

    /// <summary>
    /// Полностью останавливает воспроизведение музыки и сбрасывает состояние переходов.
    /// </summary>
    public static void StopAllMusic()
    {
        MediaPlayer.Stop();

        _activeBgm = Bgm.None;

        _isTransitioning = false;
        _fadeOutPhase = true;

        MediaPlayer.Volume = 0f;
    }

    /// <summary>
    /// Запускает музыку главного меню.
    /// </summary>
    public static void OnEnterMainMenu()
    {
        StopAllMusic();

        MediaPlayer.Volume = 0f;

        _pendingSong = _songMainMenu;
        _pendingBgm = Bgm.MainMenu;

        _fadeOutPhase = false;
        _isTransitioning = true;

        MediaPlayer.Play(_songMainMenu);

        _activeBgm = Bgm.MainMenu;
    }

    /// <summary>
    /// Запускает музыку экрана титров.
    /// </summary>
    public static void OnEnterCredits()
    {
        StopAllMusic();
        TryStartBgm(_songCredits, Bgm.Credits);
    }

    /// <summary>Вызывается при входе в игровую сцену: останавливает музыку меню/титров.</summary>
    public static void OnEnterGameScene()
    {
        if (_activeBgm == Bgm.MainMenu || _activeBgm == Bgm.Credits)
            StopAllMusic();
    }

    /// <summary>
    /// Обновляет фоновую музыку (BGM) в зависимости от текущего состояния комнаты.
    /// Автоматически переключается между музыкой битвы (с боссом или обычными врагами) 
    /// и спокойной музыкой исследования. Сохраняет прогресс трека исследования при переключении.
    /// </summary>
    /// <param name="bossFightWithLivingBoss">Признак того, идет ли битва с боссом.</param>
    /// <param name="combatWithLivingEnemies">Признак того, идет ли битва с обычными врагами.</param>
    public static void UpdateGameSceneBgm(bool bossFightWithLivingBoss, bool combatWithLivingEnemies)
    {
        bool wantBoss = bossFightWithLivingBoss;
        bool wantCombat = combatWithLivingEnemies && !wantBoss;
        bool wantEmpty = !wantBoss && !wantCombat;

        if (wantBoss)
        {
            if (_activeBgm == Bgm.EmptyRoom && MediaPlayer.State == MediaState.Playing)
            {
                _savedEmptyRoomPosition = MediaPlayer.PlayPosition;
                _hasSavedEmptyRoomPosition = true;
            }

            TryStartBgm(_songBossRoom, Bgm.BossRoom);
            return;
        }

        if (wantCombat)
        {
            if (_activeBgm == Bgm.EmptyRoom && MediaPlayer.State == MediaState.Playing)
            {
                _savedEmptyRoomPosition = MediaPlayer.PlayPosition;
                _hasSavedEmptyRoomPosition = true;
            }

            TryStartBgm(_songCombatRoom, Bgm.CombatRoom);
            return;
        }

        if (wantEmpty)
        {
            if (_activeBgm == Bgm.EmptyRoom && MediaPlayer.State == MediaState.Playing)
                return;

            if (_activeBgm is Bgm.CombatRoom or Bgm.BossRoom)
            {
                TryStartBgm(_songEmptyRoom, Bgm.EmptyRoom, resumeFrom: _hasSavedEmptyRoomPosition ? _savedEmptyRoomPosition : (TimeSpan?)null);
                return;
            }

            if (_activeBgm == Bgm.None)
            {
                TryStartBgm(_songEmptyRoom, Bgm.EmptyRoom, resumeFrom: null);
                return;
            }

            if (_activeBgm != Bgm.EmptyRoom)
                TryStartBgm(_songEmptyRoom, Bgm.EmptyRoom, resumeFrom: null);
        }
    }

    /// <summary>Воспроизводит случайный звук взмаха мечом.</summary>
    public static void PlaySwordSwing()
    {
        string name = Random.Shared.Next(2) == 0
            ? SoundConfig.PlayerSwordAttack1
            : SoundConfig.PlayerSwordAttack2;
        PlayEffect(name,0.25f);
    }
    /// <summary>Воспроизводит звук атаки дальнего боя.</summary>
    public static void PlayRangedAttack() => PlayEffect(SoundConfig.PlayerRangedAttack, 0.25f);

    /// <summary>Воспроизводит звук получения урона игроком.</summary>
    public static void PlayPlayerHit() => PlayEffect(SoundConfig.PlayerGetHit);

    /// <summary>Воспроизводит звук лечения игрока.</summary>
    public static void PlayPlayerHeal() => PlayEffect(SoundConfig.PlayerHeal);

    /// <summary>Воспроизводит звук смерти обычного врага.</summary>
    public static void PlayEnemyDeath() => PlayEffect(SoundConfig.EnemyDeath);

    /// <summary>Воспроизводит звук смерти босса.</summary>
    public static void PlayBossDeath() => PlayEffect(SoundConfig.BossDeath);

    /// <summary>Воспроизводит звук зачистки комнаты.</summary>
    public static void PlayRoomCleared() => PlayEffect(SoundConfig.RoomCleared);

    /// <summary>Воспроизводит звук выбора пункта в UI.</summary>
    public static void PlayUiSelect() => PlayEffect(SoundConfig.UISelect);

    /// <summary>Воспроизводит звук подтверждения действия в UI.</summary>
    public static void PlayUiConfirm() => PlayEffect(SoundConfig.UIConfirm);

    /// <summary>Воспроизводит звук открытия двери на карте.</summary>
    public static void PlayMapOpenDoor() => PlayEffect(SoundConfig.MapOpenDoor);

    /// <summary>Воспроизводит звук закрытия двери на карте.</summary>
    public static void PlayMapClosedDoor() => PlayEffect(SoundConfig.MapClosedDoor);

    /// <summary>Воспроизводит звук нажатия кнопки на карте.</summary>
    public static void PlayMapButton() => PlayEffect(SoundConfig.MapButton);

    /// <summary>Воспроизводит звук входа в комнату босса.</summary>
    public static void PlayMapBossEntry() => PlayEffect(SoundConfig.MapBossEntry);

    /// <summary>Воспроизводит звук появления босса.</summary>
    public static void PlayBossEntering() => PlayEffect(SoundConfig.BossEntering);

    /// <summary>Воспроизводит звук закапывания босса.</summary>
    public static void PlayBossBurrow() => PlayEffect(SoundConfig.BossBurrow);

    /// <summary>Воспроизводит звук появления босса из-под земли.</summary>
    public static void PlayBossEmerge() => PlayEffect(SoundConfig.BossEmerge);

    /// <summary>Воспроизводит звук статического шума босса.</summary>
    public static void PlayBossStatic() => PlayEffect(SoundConfig.BossStatic);

    /// <summary>Воспроизводит звук атаки босса.</summary>
    public static void PlayBossAttack() => PlayEffect(SoundConfig.BossAttack);

    /// <summary>Воспроизводит звук летающего врага.</summary>
    public static void PlayEnemyFly() => PlayEffect(SoundConfig.EnemyFly, 0.3f);

    /// <summary>Воспроизводит звук слайма.</summary>
    public static void PlayEnemySlime() => PlayEffect(SoundConfig.EnemySlime);

    /// <summary>Воспроизводит звук треанта.</summary>
    public static void PlayEnemyTreant() => PlayEffect(SoundConfig.EnemyTreant, 0.25f);

    /// <summary>Воспроизводит звук смерти игрока.</summary>
    public static void PlayPlayerDeath() => PlayEffect(SoundConfig.PlayerDeath);

    /// <summary>
    /// Воспроизводит звук шага в зависимости от поверхности.
    /// </summary>
    /// <param name="preferStone">Если true, выбиравет звук по камню.</param>
    public static void PlayFootstep(bool preferStone)
    {
        bool stone = preferStone
            ? Random.Shared.NextDouble() < SoundConfig.FootstepStoneBiasWhenOnStone
            : Random.Shared.NextDouble() < SoundConfig.FootstepStoneProbability;

        if (stone)
            PlayEffect(SoundConfig.PlayerFootstepsStone);
        else
            PlayEffect(SoundConfig.PlayerFootstepsGrass);
    }

    /// <summary>
    /// Воспроизводит звуковой эффект по имени ассета с указанной громкостью.
    /// </summary>
    /// <param name="assetName">Имя ассета в контенте.</param>
    /// <param name="volume">Уровень громкости (0.0 - 1.0).</param>
    public static void PlayEffect(string assetName, float volume)
    {
        if (string.IsNullOrEmpty(assetName))
            return;

        if (Effects.TryGetValue(assetName, out SoundEffect fx))
            fx.Play(volume, 0f, 0f);
    }

    /// <summary>
    /// Воспроизводит звуковой эффект по имени ассета с громкостью по умолчанию.
    /// </summary>
    /// <param name="assetName">Имя ассета в контенте.</param>
    public static void PlayEffect(string assetName)
    { 
        PlayEffect(assetName, SoundConfig.DefaultSoundEffectVolume);
    }

    private static string ResolveAudioPath(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return assetName;
        return assetName.Contains('/') ? assetName : "sounds/" + assetName;
    }
   
    private static void TryAddEffect(ContentManager content, string assetName)
    {
        if (string.IsNullOrEmpty(assetName) || Effects.ContainsKey(assetName))
            return;

        try
        {
            SoundEffect fx = content.Load<SoundEffect>(ResolveAudioPath(assetName));
            if (fx != null)
                Effects[assetName] = fx;
        }
        catch
        {
            // ассета нет в Content — тихо пропускаем
        }
    }


    private static Song TryLoadSong(ContentManager content, string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return null;

        try
        {
            return content.Load<Song>(ResolveAudioPath(assetName));
        }
        catch
        {
            return null;
        }
    }
    private static void TryStartBgm(Song song, Bgm kind, TimeSpan? resumeFrom = null)
    {
        if (song == null)
            return;

        // Во время transition ничего нового не запускаем
        if (_isTransitioning)
            return;

        // Уже играет нужная музыка
        if (_activeBgm == kind &&
            MediaPlayer.State == MediaState.Playing)
            return;

        _pendingSong = song;
        _pendingBgm = kind;

        _fadeOutPhase = true;
        _isTransitioning = true;
    }

    private static void TrySetMediaPlayerPosition(TimeSpan position)
    {
        try
        {
            PropertyInfo prop = typeof(MediaPlayer).GetProperty(nameof(MediaPlayer.PlayPosition));
            MethodInfo setter = prop?.GetSetMethod(nonPublic: true);
            setter?.Invoke(null, new object[] { position });
        }
        catch
        {
        }
    }

    /// <summary>
    /// Обновляет логику воспроизведения музыки: обрабатывает плавные переходы (Fade In/Out).
    /// </summary>
    /// <param name="dt">Время, прошедшее с последнего кадра (Delta Time).</param>
    public static void Update(float dt)
    {
        if (!_isTransitioning)

            return;

        if (_fadeOutPhase)
        {
            MediaPlayer.Volume -= _musicFadeSpeed * dt;

            if (MediaPlayer.Volume <= 0f)
            {
                MediaPlayer.Volume = 0f;

                MediaPlayer.Play(_pendingSong);
                if (_pendingBgm == Bgm.EmptyRoom && _hasSavedEmptyRoomPosition)
                {
                    TrySetMediaPlayerPosition(_savedEmptyRoomPosition);
                }
                MediaPlayer.Volume = 0f;
                _activeBgm = _pendingBgm;

                MediaPlayer.IsRepeating = true;

                switch (_pendingBgm)
                {
                    case Bgm.EmptyRoom:
                        _targetVolume = _emptyRoomVolume;
                        break;

                    case Bgm.CombatRoom:
                        _targetVolume = _combatRoomVolume;
                        break;

                    case Bgm.BossRoom:
                        _targetVolume = _bossRoomVolume;
                        break;

                    case Bgm.MainMenu:
                        _targetVolume = _menuVolume;
                        break;

                    default:
                        _targetVolume = 0.5f;
                        break;
                }

                _fadeOutPhase = false;
            }
        }
        else
        {
            MediaPlayer.Volume += _musicFadeSpeed * dt;

            if (MediaPlayer.Volume >= _targetVolume)
            {
                MediaPlayer.Volume = _targetVolume;
                _isTransitioning = false;
            }
        }
    }

}
