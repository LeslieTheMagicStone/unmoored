using Cinemachine;
using UnityEngine;

namespace CGJ2026
{
    public class CameraSetup : MonoBehaviour
    {
        [SerializeField] Transform playerA;
        [SerializeField] Transform playerB;

        void Start()
        {
            var tgGo = new GameObject("CM TargetGroup");
            var tg = tgGo.AddComponent<CinemachineTargetGroup>();
            tg.m_Targets = new[]
            {
                new CinemachineTargetGroup.Target { target = playerA, weight = 1f, radius = 2f },
                new CinemachineTargetGroup.Target { target = playerB, weight = 1f, radius = 2f }
            };
            tg.m_PositionMode = CinemachineTargetGroup.PositionMode.GroupCenter;

            var vcamGo = new GameObject("CM Vcam");
            var vcam = vcamGo.AddComponent<CinemachineVirtualCamera>();
            vcam.m_Lens.OrthographicSize = 6f;
            vcam.m_Follow = tgGo.transform;
            vcam.m_Priority = 10;

            var framing = vcam.AddCinemachineComponent<CinemachineFramingTransposer>();
            framing.m_CameraDistance = 10f;
            framing.m_DeadZoneWidth = 0.2f;
            framing.m_DeadZoneHeight = 0.2f;
            framing.m_SoftZoneWidth = 0.8f;
            framing.m_SoftZoneHeight = 0.8f;
            framing.m_MinimumOrthoSize = 5f;
            framing.m_MaximumOrthoSize = 10f;
            framing.m_XDamping = 0.5f;
            framing.m_YDamping = 0.5f;
        }
    }
}
