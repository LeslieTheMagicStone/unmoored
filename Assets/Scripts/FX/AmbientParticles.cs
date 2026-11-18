using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
	public class AmbientParticles : MonoBehaviour
	{
		[Header("Particle")]
		[SerializeField] float particleRate = 15f;
		[SerializeField] float particleLifetime = 2f;

		[Header("Drift")]
		[SerializeField] Vector2 driftDirection = new(0.2f, 0.8f);

		Camera cam;
		float particleTimer;

		void Awake()
		{
			cam = Camera.main;
		}

		void Update()
		{
			particleTimer += Time.deltaTime;
			if (particleTimer >= 1f / particleRate)
			{
				particleTimer -= 1f / particleRate;
				SpawnParticle();
			}
		}

		void SpawnParticle()
		{
			if (cam == null) return;

			float halfH = cam.orthographicSize;
			float halfW = halfH * cam.aspect;
			Vector3 camPos = cam.transform.position;

			Vector2 pos = new(
				camPos.x + Random.Range(-halfW, halfW),
				camPos.y + Random.Range(-halfH, halfH)
			);

			var d = GameObject.CreatePrimitive(PrimitiveType.Cube);
			d.transform.position = pos;
			d.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);
			Destroy(d.GetComponent<Collider>());

			var sr = d.GetComponent<MeshRenderer>();
			sr.material = new Material(Shader.Find("Sprites/Default"));
			sr.material.color = new Color(0.85f, 0.9f, 1f, 0.4f);
			sr.material.DOFade(0f, particleLifetime);

			Vector2 drift = driftDirection.normalized * Random.Range(2f, 5f);
			d.transform.DOMove(d.transform.position + (Vector3)drift, particleLifetime).SetEase(Ease.OutQuad);
			Destroy(d, particleLifetime + 0.1f);
		}
	}
}
