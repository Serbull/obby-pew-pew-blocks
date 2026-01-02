using System;

public static class LocalizationManager
{
    private readonly static string _langugageCode;

    public static event Action OnLanguageChanged;

    static LocalizationManager()
    {
        _langugageCode = GetSystemLanguage();
    }

    private static string GetSystemLanguage()
    {
        var lang = YG.YG2.lang;
        if (lang == "ru") return "Russian";
        return "English";
    }

    public static string GetText(string id)
    {
        return Configs.Instance.LocalizationData.GetText(id, _langugageCode);
    }
}
