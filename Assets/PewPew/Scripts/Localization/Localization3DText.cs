using TMPro;
using UnityEngine;

namespace Serbull.GameAssets.Localization
{
	// Скрипт автоматически потребует наличие 3D TextMeshPro на объекте
	[RequireComponent(typeof(TextMeshPro))]
	public class Localization3DText : MonoBehaviour
	{
		[SerializeField] private string _id;

		private TextMeshPro _text;
		private string[] _args = new string[0];

		private void Awake()
		{
			_text = GetComponent<TextMeshPro>();
		}

		private void OnEnable()
		{
			// Подписываемся на глобальное событие смены языка в системе Serbull
			EventBus.Subscribe<UpdateLocalizationEvent>(OnLocalizationUpdated);
			UpdateText();
		}

		private void OnDisable()
		{
			// Обязательно отписываемся при выключении объекта
			EventBus.Unsubscribe<UpdateLocalizationEvent>(OnLocalizationUpdated);
		}

		private void OnLocalizationUpdated(UpdateLocalizationEvent e)
		{
			UpdateText();
		}

		protected void UpdateText()
		{
			// Если сервис ещё не готов или равен заглушке, ничего не делаем
			if (Services.Localization == null || Services.Localization is EmptyService)
				return;

			// Вытаскиваем переведенный текст по ID
			var locText = Services.Localization.GetText(_id);

			// Если в строке есть параметры (например, {0}), форматируем её
			if (_args.Length > 0 && locText != null)
			{
				locText = string.Format(locText, _args);
			}

			// Закидываем готовый текст в TextMeshPro
			if (_text != null && locText != null)
			{
				_text.text = locText;
			}
		}

		// Метод на случай, если захочешь поменять ID программно из других скриптов
		public void SetLocalizationId(string id)
		{
			_id = id;
			UpdateText();
		}

#if UNITY_EDITOR
        // Автоматически форматирует ID в инспекторе (убирает пробелы и делает нижний регистр)
        private void OnValidate()
        {
            if (_id != null)
            {
                _id = _id.Replace(" ", "_").ToLower();
            }
        }
#endif
	}
}