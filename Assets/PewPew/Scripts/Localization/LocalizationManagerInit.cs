using UnityEngine;
using YG; // Подключаем плагин Яндекс Игр

namespace Serbull.GameAssets.Localization
{
	public class LocalizationManagerInit : MonoBehaviour
	{
		private void Awake()
		{
			InitializeLocalization();
		}

		private void InitializeLocalization()
		{
			LocalizationConfig config = Resources.Load<LocalizationConfig>("LocalizationConfig");
			if (config == null)
			{
				Debug.LogError("[LocalizationInit] Не удалось найти 'LocalizationConfig' в папке Resources!");
				return;
			}

			string targetLang = string.IsNullOrEmpty(YG2.lang) ? "en" : YG2.lang;

			LocalizationManager manager = new LocalizationManager(config, targetLang);
			Services.Localization = manager;

			Debug.Log($"[LocalizationInit] Система успешно запущена. Язык: {targetLang.ToUpper()}");

			EventBus.Publish(new UpdateLocalizationEvent());
		}
	}
}