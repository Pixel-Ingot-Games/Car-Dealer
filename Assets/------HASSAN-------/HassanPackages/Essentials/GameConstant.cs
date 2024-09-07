using UnityEngine;

public class GameConstant
{
    public static int LevelSelection;
    public static string Coins = "Coins";
    public static bool Haptics;

    public static string UnlockedLevels = "UnlockedLevels";

    public static int CurrentLevel { get => PlayerPrefs.GetInt("CurrentLevel"); set => PlayerPrefs.SetInt("CurrentLevel", value); }
    public static int HorseBodyMaterial { get => PlayerPrefs.GetInt("HorseBodyMaterial", 0); set => PlayerPrefs.SetInt("HorseBodyMaterial", value); }
    public static int HorseManeMaterial { get => PlayerPrefs.GetInt("HorseManeMaterial", 0); set => PlayerPrefs.SetInt("HorseManeMaterial", value); }
}
