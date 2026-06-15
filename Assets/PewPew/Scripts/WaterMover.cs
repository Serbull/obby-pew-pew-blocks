using UnityEngine;

public class WaterMover : MonoBehaviour
{
	// Создаем типы направлений для инспектора
	public enum MovementDirection { OnlyX, OnlyY, Both }

	[Header("Настройки движения")]
	[Tooltip("Выберите, по какой оси двигать воду")]
	public MovementDirection direction = MovementDirection.OnlyX;

	[Tooltip("Скорость движения. Отрицательная крутит в обратную сторону.")]
	public float speed = 0.1f;

	// Если выберешь режим Both, сможешь настроить вторую скорость отдельно
	[HideInInspector] public float speedYForBoth = 0.1f;

	private Material targetMaterial;

	void Start()
	{
		Renderer renderer = GetComponent<Renderer>();
		if (renderer != null)
		{
			targetMaterial = renderer.material;
		}
	}

	void Update()
	{
		if (targetMaterial != null)
		{
			Vector2 offset = targetMaterial.mainTextureOffset;

			// Проверяем, какой режим выбран в выпадающем списке
			switch (direction)
			{
				case MovementDirection.OnlyX:
					offset.x += speed * Time.deltaTime;
					break;

				case MovementDirection.OnlyY:
					offset.y += speed * Time.deltaTime;
					break;

				case MovementDirection.Both:
					offset.x += speed * Time.deltaTime;
					offset.y += speedYForBoth * Time.deltaTime;
					break;
			}

			targetMaterial.mainTextureOffset = offset;
		}
	}
}