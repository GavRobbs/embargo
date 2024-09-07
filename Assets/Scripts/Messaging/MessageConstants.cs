/* This enum has all the possible messages that can be sent by the game event system, 
 * helps with decoupling, and the GameMessage class carries the extra data needed by the message, which can be retrieved by a cast */

public enum MessageConstants : int
{
    NullMessage,
    HoverInfoDisplayMessage,
    HideHoverPopupMessage,
    CreateTurret,
    EngageBuildMode,
    DisengageBuildMode,
    WaveAlertMessage,
    BeginWaveMessage,
    EndWaveMessage,
    TriggerFirstWaveMessage,
    AddScrap,
    RemoveScrap,
    GameOverMessage,
    RegisterSupportTurretMessage,
    UnregisterSupportTurretMessage,
    RegisterOffensiveTurretMessage,
    CreateDroneMessage,
    DisplayAlertMessage,
    EngageUpgradeMode,
    DisengageUpgradeMode,
    UpgradeTurret,
    EngageScrapMode,
    DisengageScrapMode,
    ScrapTurret
}