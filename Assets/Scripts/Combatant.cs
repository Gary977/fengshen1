
using UnityEngine;

[System.Serializable]
public class Combatant {
    public string name;
    public Element innateElement;
    public Polarity innatePolarity;

    public Element currentElement;

    public int maxHP=100;
    public int currentHP=100;
    public int block=0;

    public int wetTurns=0;
    public int weakTurns=0;
    public int burnTurns=0;
    public int burnDamagePerTurn=0;

    public int shieldHitsLeft=0;
    public int shieldReducePerHit=0;

    public void ResetElement(){ currentElement=innateElement; }

    public void TakeDamage(int amount){
        int dmg = amount;
        if(shieldHitsLeft>0){
            dmg = Mathf.Max(dmg - shieldReducePerHit, 0);
            shieldHitsLeft--;
        }
        int final = Mathf.Max(dmg - block, 0);
        block = Mathf.Max(block - dmg, 0);
        currentHP -= final;
        if(currentHP<0) currentHP=0;
        Debug.Log(name+" took "+final+" dmg, HP="+currentHP);
    }

    public void GainBlock(int amt){
        block += amt;
        Debug.Log(name+" gained "+amt+" block ("+block+")");
    }

    public void Heal(int amt){
        currentHP = Mathf.Min(maxHP, currentHP+amt);
        Debug.Log(name+" healed "+amt+", HP="+currentHP);
    }

    public void TickEnd(){
        if(burnTurns>0 && burnDamagePerTurn>0){
            currentHP -= burnDamagePerTurn;
            burnTurns--;
            if(currentHP<0) currentHP=0;
            Debug.Log(name+" burned for "+burnDamagePerTurn+", HP="+currentHP);
            if(burnTurns<=0) burnDamagePerTurn=0;
        }
        if(weakTurns>0) weakTurns--;
        if(wetTurns>0) wetTurns--;
    }
}
