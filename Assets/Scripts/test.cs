using DG.Tweening;
using UnityEngine;

public class test : MonoBehaviour
{
    void Start()
    {
        // 1. 移动到 (5,0,0)，曲线缓动
        transform.DOMove(new Vector3(5, 0, 0), 1f).SetEase(Ease.InOutQuad);

        // 2. Sequence: 弹一下 + 转一圈，无限循环
        DOTween.Sequence()
            .Append(transform.DOPunchScale(Vector3.one * 0.5f, 0.3f))
            .Append(transform.DORotate(new Vector3(0, 360, 0), 0.6f, RotateMode.FastBeyond360))
            .SetLoops(-1);
    }
}
