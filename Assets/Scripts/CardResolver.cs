
using UnityEngine;
public static class CardResolver {
    public static void Play(CardInstance inst, Combatant user, Combatant target, bool enemyNextIsAtk){
        var c=inst.definition;
        float m=WuxingHelper.GetMultiplier(user,c,target);

        switch(c.id){
            case CardId.YangWood_VineSurge:
                int dmg = WuxingHelper.Apply(c.baseDamage,m);
                target.TakeDamage(dmg);
                if(user.block>0 && c.bonusDamage>0){
                    int bonus=WuxingHelper.Apply(c.bonusDamage,m);
                    target.TakeDamage(bonus);
                }
                if(target.currentElement==Element.Earth){
                    int extra = Mathf.CeilToInt(dmg*c.elementBonus);
                    target.TakeDamage(extra);
                }
                break;
        }
    }
}
