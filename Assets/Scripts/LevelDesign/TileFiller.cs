using UnityEngine;

namespace CGJ2026
{
	public enum TileFillMode
	{
		SingleRow,
		RandomFill
	}

	[ExecuteAlways]
	public class TileFiller : MonoBehaviour
	{
		[Header("Mode")]
		[SerializeField] TileFillMode mode;

		[Header("Single Row Sprites")]
		[SerializeField] Sprite leftEdge;
		[SerializeField] Sprite rightEdge;
		[SerializeField] Sprite middleA;
		[SerializeField] Sprite middleB;

		[Header("Random Fill")]
		[SerializeField] Sprite[] palette;

		[Header("Settings")]
		[SerializeField] Color tileColor = Color.white;

		Vector3 lastScale;
		TileFillMode lastMode;
		Sprite lastLeftEdge;
		Sprite lastRightEdge;
		Sprite lastMiddleA;
		Sprite lastMiddleB;
		Sprite[] lastPalette;
		Transform tilesRoot;

		void OnEnable()
		{
			Rebuild();
		}

		void Update()
		{
			if (transform.localScale != lastScale || mode != lastMode || SpritesChanged())
				Rebuild();
		}

		bool SpritesChanged()
		{
			if (mode == TileFillMode.SingleRow)
				return leftEdge != lastLeftEdge || rightEdge != lastRightEdge
					|| middleA != lastMiddleA || middleB != lastMiddleB;

			return !ArraysEqual(palette, lastPalette);
		}

		static bool ArraysEqual(Sprite[] a, Sprite[] b)
		{
			if (a == null || b == null) return a == b;
			if (a.Length != b.Length) return false;
			for (int i = 0; i < a.Length; i++)
				if (a[i] != b[i]) return false;
			return true;
		}

		public void Rebuild()
		{
			Clear();

			Vector3 s = transform.localScale;
			if (s.x <= 0f || s.y <= 0f) return;

			int cols, rows;
			float ppu;

			if (mode == TileFillMode.RandomFill)
			{
				if (palette == null || palette.Length == 0) return;

				cols = Mathf.Max(1, Mathf.RoundToInt(s.x));
				rows = Mathf.Max(1, Mathf.RoundToInt(s.y));
				ppu = palette[0].pixelsPerUnit;
			}
			else
			{
				if (leftEdge == null || rightEdge == null || middleA == null || middleB == null) return;

				cols = SnapToEven(s.x);
				rows = 1;
				ppu = leftEdge.pixelsPerUnit;
			}

			tilesRoot = new GameObject("Tiles").transform;
			tilesRoot.SetParent(transform);
			tilesRoot.localPosition = new Vector3(-0.5f, -0.5f, 0f);
			tilesRoot.localRotation = Quaternion.identity;
			tilesRoot.localScale = Vector3.one;

			for (int x = 0; x < cols; x++)
			{
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
					sr.sprite = PickSprite(x, cols);
					sr.color = tileColor;
				}
			}

			lastScale = s;
			lastMode = mode;
			lastLeftEdge = leftEdge;
			lastRightEdge = rightEdge;
			lastMiddleA = middleA;
			lastMiddleB = middleB;
			lastPalette = palette != null ? (Sprite[])palette.Clone() : null;
		}

		static int SnapToEven(float v)
		{
			int even = Mathf.RoundToInt(v / 2f) * 2;
			return Mathf.Max(2, even);
		}

		Sprite PickSprite(int x, int totalCols)
		{
			if (mode == TileFillMode.RandomFill)
				return palette[Random.Range(0, palette.Length)];

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
