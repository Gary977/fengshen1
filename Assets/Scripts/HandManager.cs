using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public CardDatabase database;

    // UI 父节点（就是你刚才做的 HandArea）
    public Transform handArea;
    // 卡牌 UI 预制体（CardUI prefab）
    public GameObject cardUIPrefab;


    public List<CardInstance> playerHand = new();
    public List<CardInstance> enemyHand = new();

    // 抽一张牌：同时加入手牌列表 + 生成一张 UI
    public void AddCard(Combatant owner, List<CardInstance> hand)
    {
        var e = DiceHelper.RollElement();
        var p = DiceHelper.RollPolarity();
        var def = database.GetByPair(e, p);

        var inst = new CardInstance(def);
        hand.Add(inst);

        Debug.Log(owner.name + " drew " + def.cardName);

        // 只给“玩家”的手牌生成 UI，敌人先不显示
        if (owner.name == "DaJi" || owner.name == "Player")
        {
            if (handArea != null && cardUIPrefab != null)
            {
                var go = GameObject.Instantiate(cardUIPrefab, handArea);
                var ui = go.GetComponent<CardUI>();


                if (ui != null)
                {
                    ui.Init(inst);
                    handArea.GetComponent<HandCurveLayout>().RefreshLayout();
                    ui.handArea = handArea;   // 注入 HandArea
                }
            }
            else
            {
                Debug.LogWarning("[HandManager] handArea 或 cardUIPrefab 没有绑定，无法生成卡 UI。");
            }
        }
    }
}

