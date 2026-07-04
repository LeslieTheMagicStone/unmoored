using UnityEngine;

namespace CGJ2026
{
	[ExecuteAlways]
	public class TileFiller : MonoBehaviour
	{
		[Header("Sprites")]
		[SerializeField] Sprite leftEdge;
		[SerializeField] Sprite rightEdge;
		[SerializeField] Sprite middleA;
		[SerializeField] Sprite middleB;

		[Header("Settings")]
		[SerializeField] Color tileColor = Color.white;

		Vector3 lastScale;
		Sprite lastLeftEdge;
		Sprite lastRightEdge;
		Sprite lastMiddleA;
		Sprite lastMiddleB;
		Transform tilesRoot;

		void OnEnable()
		{
			Rebuild();
		}

		void Update()
		{
			if (transform.localScale != lastScale || SpritesChanged())
				Rebuild();
		}

		bool SpritesChanged()
		{
			return leftEdge != lastLeftEdge || rightEdge != lastRightEdge
				|| middleA != lastMiddleA || middleB != lastMiddleB;
		}

		public void Rebuild()
		{
			Clear();

			if (leftEdge == null || rightEdge == null || middleA == null || middleB == null) return;

			Vector3 s = transform.localScale;
			if (s.x <= 0f || s.y <= 0f) return;

			int cols = SnapToEven(s.x);
			int rows = Mathf.Max(1, Mathf.RoundToInt(s.y));

			float ppu = leftEdge.pixelsPerUnit;

			tilesRoot = new GameObject("Tiles").transform;
			tilesRoot.SetParent(transform);
			tilesRoot.localPosition = new Vector3(-0.5f, -0.5f, 0f);
			tilesRoot.localRotation = Quaternion.identity;
			tilesRoot.localScale = Vector3.one;

			for (int x = 0; x < cols; x++)
			{
				Sprite sprite = PickSprite(x, cols);

				for (int y = 0; y < rows; y++)
				{
					float px = (x + 0.5f) / cols;
					float py = (y + 0.5f) / rows;

					float overlapX = 1f / (ppu * s.x);
					float overlapY = 1f / (ppu * s.y);

					var go = new GameObject($"Tile_{x}_{y}");
					go.transform.SetParent(tilesRoot);
					go.transform.localPosition = new Vector3(px, py, 0f);
					go.transform.localScale = new Vector3(1f / cols + overlapX, 1f / rows + overlapY, 1f);

					var sr = go.AddComponent<SpriteRenderer>();
					sr.sprite = sprite;
					sr.color = tileColor;
				}
			}

			lastScale = s;
			lastLeftEdge = leftEdge;
			lastRightEdge = rightEdge;
			lastMiddleA = middleA;
			lastMiddleB = middleB;
		}

		static int SnapToEven(float v)
		{
			int even = Mathf.RoundToInt(v / 2f) * 2;
			return Mathf.Max(2, even);
		}

		Sprite PickSprite(int x, int totalCols)
		{
			if (x == 0) return leftEdge;
			if (x == totalCols - 1) return rightEdge;

			int middleIndex = x - 1;
			return middleIndex % 2 == 0 ? middleA : middleB;
		}

		public void Clear()
		{
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				var child = transform.GetChild(i);
				if (child.name == "Tiles")
					DestroyImmediate(child.gameObject);
			}
			tilesRoot = null;
		}
	}
}
