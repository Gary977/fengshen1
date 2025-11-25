using UnityEngine;
using System.Collections.Generic;

public class HandCurveLayout : MonoBehaviour
{
    public float Radius = 800f;
    public float AngleSpread = 30f;
    public float VerticalOffset = -60f;

    // 外部调用重新排版
    public void RefreshLayout()
    {
        ApplyLayout();
    }

    void ApplyLayout()
    {
        // ① 收集所有真实卡牌（跳过 ghost）
        List<Transform> realCards = new List<Transform>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform c = transform.GetChild(i);

            // 只有 CardUI 才是实际手牌
            if (c.GetComponent<CardUI>() != null)
                realCards.Add(c);
        }

        // 没有卡牌就不排
        int count = realCards.Count;
        if (count == 0) return;

        // ② 以真实卡数量为基础计算角度（绝不使用 childCount）
        float startAngle = -AngleSpread * (count - 1) / 2f;

        // ③ 按真实卡牌的 index 排布局
        for (int i = 0; i < count; i++)
        {
            Transform card = realCards[i];

            // 计算角度
            float angle = startAngle + AngleSpread * i;
            float rad = angle * Mathf.Deg2Rad;

            // 根据圆弧计算位置
            Vector3 pos = new Vector3(
                Mathf.Sin(rad) * Radius,
                VerticalOffset,
                0
            );

            // 设置 UI 位置 & 旋转
            card.localPosition = pos;
            card.localRotation = Quaternion.Euler(0, 0, -angle);
        }
    }
}
