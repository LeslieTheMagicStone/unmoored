using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeSystem : MonoBehaviour
    {
        [SerializeField] PlayerController playerA;
        [SerializeField] PlayerController playerB;

        LineRenderer lineRenderer;
        DistanceJoint2D joint;

        public PlayerController PlayerA => playerA;
        public PlayerController PlayerB => playerB;
        public float MaxLength { get; private set; }

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;

            if (playerA == null || playerB == null)
                throw new System.Exception("RopeSystem: both players must be assigned");

            var config = playerA.Config;
            MaxLength = config.MaxRopeLength;

            Rigidbody2D rbA = playerA.GetComponent<Rigidbody2D>();
            Rigidbody2D rbB = playerB.GetComponent<Rigidbody2D>();

            joint = rbA.gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = rbB;
            joint.distance = MaxLength;
            joint.autoConfigureDistance = false;
            joint.maxDistanceOnly = true;
            joint.enableCollision = false;
        }

        void Update()
        {
            lineRenderer.SetPosition(0, playerA.transform.position);
            lineRenderer.SetPosition(1, playerB.transform.position);
        }
    }
}
