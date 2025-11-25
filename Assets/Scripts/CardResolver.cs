using UnityEngine;

public static class CardResolver
{
    public static void Play(CardInstance inst, Combatant user, Combatant target)
    {
        var c = inst.definition;
        float m = WuxingHelper.GetMultiplier(user, c, target);

        switch (c.id)
        {
            // ============================================================
            // 1️⃣ Yang Wood – Vine Surge
            // ============================================================
            case CardId.YangWood_VineSurge:
                {
                    target.ApplyVulnerable(c.vulnerableTurns);
                    int dmg = WuxingHelper.Apply(c.baseDamage, m);
                    target.TakeDamage(dmg);

                    if (user.block > 0)
                    {
                        int bonus = WuxingHelper.Apply(c.bonusDamage, m);
                        target.TakeDamage(bonus);
                    }

                    break;
                }

            // ============================================================
            // 2️⃣ Yin Wood – Spirit Mend
            // ============================================================
            case CardId.YinWood_SpiritMend:
                {
                    int heal = c.heal;

                    // 有任意 debuff 则加成 + 清除一个 debuff
                    bool hasDebuff =
                        user.weakTurns > 0 ||
                        user.wetTurns > 0 ||
                        user.burnTurns > 0 ||
                        user.vulnerableTurns > 0;

                    if (hasDebuff)
                    {
                        heal = Mathf.CeilToInt(heal * c.HpMultiplier);

                        // 移除一个 debuff（优先级：burn → weak → wet → vulnerable）
                        if (user.burnTurns > 0)
                        {
                            user.burnTurns = 0;
                            user.burnDamagePerTurn = 0;
                        }
                        else if (user.weakTurns > 0)
                        {
                            user.weakTurns = 0;
                        }
                        else if (user.wetTurns > 0)
                        {
                            user.wetTurns = 0;
                        }
                        else if (user.vulnerableTurns > 0)
                        {
                            user.vulnerableTurns = 0;
                        }
                    }

                    user.Heal(heal);
                    break;
                }

            // ============================================================
            // 3️⃣ Yang Fire – Heartflame Slash
            // ============================================================
            case CardId.YangFire_HeartflameSlash:
                {
                    int dmg = WuxingHelper.Apply(c.baseDamage, m);
                    target.TakeDamage(dmg);

                    if (target.wetTurns > 0)
                    {
                        target.TakeDamage(c.bonusDamage);
                    }

                    break;
                }

            // ============================================================
            // 4️⃣ Yin Fire – Scorch Mark
            // ============================================================
            case CardId.YinFire_ScorchMark:
                {
                    int dmg = WuxingHelper.Apply(c.baseDamage, m);

                    target.TakeDamage(dmg);

                    // Burn (DOT)
                    target.ApplyBurn(c.burnTick, c.burnTurns);
                    break;
                }

            // ============================================================
            // 5️⃣ Yang Earth – Rock Bulwark
            // ============================================================
            case CardId.YangEarth_RockBulwark:
                {
                    user.GainBlock(c.baseBlock);

                    break;
                }

            // ============================================================
            // 6️⃣ Yin Earth – Earthen Seal
            // ============================================================
            case CardId.YinEarth_EarthenSeal:
                {
                    // Weak (1 turn)
                    if (c.weakTurns > 0)
                        target.ApplyWeak(c.weakTurns);

                    target.block = Mathf.Max(0, target.block - 5);

                    break;
                }

            // ============================================================
            // 7️⃣ Yang Metal – Gilded Edge
            // ============================================================
            case CardId.YangMetal_GildedEdge:
                {
                    int dmg = WuxingHelper.Apply(c.baseDamage, m);
                    target.TakeDamage(dmg);

                    if (user.block > 0)
                    {
                        int bonus = WuxingHelper.Apply(c.bonusDamage, m);
                        target.TakeDamage(bonus);
                    }


                    break;
                }

            // ============================================================
            // 8️⃣ Yin Metal – Refine
            // ============================================================
            case CardId.YinMetal_Refine:
                {
                    // 你当前系统没有“选目标手牌”UI → 先对本卡进行折扣（不报错）
                    CardInstance chosen = inst;

                    int reduction = 1;

                    if (chosen.definition.element == Element.Metal)
                        reduction += 1;

                    chosen.cost = Mathf.Max(0, chosen.cost - reduction);
                    break;
                }

            // ============================================================
            // 9️⃣ Yang Water – Tide Calling
            // ============================================================
            case CardId.YangWater_TideCalling:
                {
                    int dmg = WuxingHelper.Apply(c.baseDamage, m);
                    target.TakeDamage(dmg);

                    // Wet
                    target.ApplyWet(c.wetTurns);


                    break;
                }

            // ============================================================
            // 🔟 Yin Water – Flowing Veil
            // ============================================================
            case CardId.YinWater_FlowingVeil:
                {
                    user.ApplyShield(c.reducePerHit, c.hitCount);
                    break;
                }

            default:
                Debug.LogWarning($"[CardResolver] 未实现的卡牌：{c.id}");
                break;
        }
    }
}
