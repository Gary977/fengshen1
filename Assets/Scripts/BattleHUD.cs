using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TurnOwner
{
    Player,
    Enemy
}

public class BattleHUD : MonoBehaviour
{
    [Header("Round / Turn")]
    [SerializeField] private TMP_Text roundText;   // "Round 1"
    [SerializeField] private TMP_Text phaseText;   // "Player Turn" / "Enemy Turn"

    [Header("Player UI")]
    [SerializeField] private TMP_Text playerHpText;  // "80 / 100"
    [SerializeField] private Image playerHpFill;   // 血条填充 Image (type = Filled)

    [Header("Enemy UI")]
    [SerializeField] private TMP_Text enemyHpText;
    [SerializeField] private Image enemyHpFill;

    private int playerMaxHP = 100;
    private int enemyMaxHP = 100;

    /// <summary>
    /// 在战斗开始时由 BattleManager 调用一次。
    /// </summary>
    public void Init(int playerMax, int enemyMax)
    {
        playerMaxHP = Mathf.Max(1, playerMax);
        enemyMaxHP = Mathf.Max(1, enemyMax);

        UpdatePlayerHP(playerMaxHP);
        UpdateEnemyHP(enemyMaxHP);

        SetRound(1);
        SetTurn(TurnOwner.Player);
    }

    public void SetRound(int round)
    {
        if (roundText != null)
            roundText.text = $"Round {round}";
    }

    public void SetTurn(TurnOwner owner)
    {
        if (phaseText == null) return;

        switch (owner)
        {
            case TurnOwner.Player:
                phaseText.text = "Player Turn";
                break;
            case TurnOwner.Enemy:
                phaseText.text = "Enemy Turn";
                break;
        }
    }

    public void UpdatePlayerHP(int currentHP)
    {
        currentHP = Mathf.Clamp(currentHP, 0, playerMaxHP);

        if (playerHpText != null)
            playerHpText.text = $"{currentHP} / {playerMaxHP}";

        if (playerHpFill != null)
            playerHpFill.fillAmount = (float)currentHP / playerMaxHP;
    }

    public void UpdateEnemyHP(int currentHP)
    {
        currentHP = Mathf.Clamp(currentHP, 0, enemyMaxHP);

        if (enemyHpText != null)
            enemyHpText.text = $"{currentHP} / {enemyMaxHP}";

        if (enemyHpFill != null)
            enemyHpFill.fillAmount = (float)currentHP / enemyMaxHP;
    }
}
