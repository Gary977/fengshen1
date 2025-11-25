
using UnityEngine;

public class BattleManager : MonoBehaviour {
    public CardDatabase database;
    public HandManager hand;
    public CombatantHolder playerObj;
    public CombatantHolder enemyObj;

    private Combatant player;
    private Combatant enemy;


    int turn =0;
    bool playerTurn;
    int pMax=3,eMax=3;
    int pEnergy,eEnergy;
    int pReserve=0,eReserve=0;
    void Start()
    {
        CardUI.hoverCanvas = GameObject.Find("HoverCanvas").transform;
        player = playerObj.data;
        enemy = enemyObj.data;

        player.ResetElement();
        enemy.ResetElement();

        //playerTurn = Random.value > 0.5f;
        playerTurn = true;
        if (playerTurn) eReserve = 1; else pReserve = 1;

        Debug.Log("PlayerTurn? " + playerTurn);
        StartTurn();
    }


    void StartTurn()
    {
        turn++;

        if (playerTurn)
        {
            pMax = turn == 1 ? 3 : Mathf.Min(6, pMax + 1);
            pEnergy = pMax + (pReserve > 0 ? 1 : 0);
            pReserve = 0;

            hand.AddCard(player, hand.playerHand);
            hand.AddCard(player, hand.playerHand);
            hand.AddCard(player, hand.playerHand);
            Debug.Log("Player Turn " + turn + " Energy=" + pEnergy);
        }
        else
        {
            eMax = turn == 1 ? 3 : Mathf.Min(6, eMax + 1);
            eEnergy = eMax + (eReserve > 0 ? 1 : 0);
            eReserve = 0;

            hand.AddCard(enemy, hand.enemyHand);
            hand.AddCard(enemy, hand.enemyHand);
            hand.AddCard(enemy, hand.enemyHand);
            Debug.Log("Enemy Turn " + turn + " Energy=" + eEnergy);
        }
    }


    public void EndTurn(){
        player.TickEnd();
        enemy.TickEnd();
        playerTurn = !playerTurn;
        StartTurn();
    }
}
