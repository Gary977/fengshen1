using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    IDragHandler
{
    public CardInstance instance;
    public Transform handArea;
    private Transform originalParent;
    private int originalIndex;
    // 外层由 HandCurveLayout 控制位置
    private RectTransform outer;

    // 内层是我们真正动的部分（缩放、抬高、旋转）
    public RectTransform visual;

    public CanvasGroup canvasGroup;

    // Ghost
    public GameObject ghostPrefab;
    private GameObject ghost;

    // Hover animation
    public float hoverScale = 1.12f;
    public float hoverLift = 40f;
    public float smooth = 12f;

    private bool isHover = false;
    private bool isDrag = false;

    private Vector3 visualBasePos;
    private Vector3 visualTargetPos;
    private Vector3 visualBaseScale;
    private Vector3 visualTargetScale;

    public static bool globalDragging = false;

    private DropZone dropZone;

    [Header("UI References")]
    public Image cardArt;
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI cardCost;
    public TextMeshProUGUI cardPolarity;
    public static Transform hoverCanvas;

    void Awake()
    {
        outer = GetComponent<RectTransform>();
        visualBasePos = Vector3.zero;
        visualTargetPos = visualBasePos;
        handArea = GameObject.Find("HandArea").transform;

        visualBaseScale = visual.localScale;
        visualTargetScale = visualBaseScale;
    }


    void Update()
    {
        visual.localScale =
            Vector3.Lerp(visual.localScale, visualTargetScale, Time.deltaTime * smooth);

        visual.localPosition =
            Vector3.Lerp(visual.localPosition, visualTargetPos, Time.deltaTime * smooth);
    }


    // ---------------- HOVER ----------------
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDrag || globalDragging) return;

        isHover = true;

        visualTargetScale = visualBaseScale * hoverScale;
        visualTargetPos = new Vector3(0, hoverLift, 0);
        transform.SetSiblingIndex(999);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDrag && !globalDragging)

            if (!isDrag)
        {
            visualTargetScale = visualBaseScale;
            visualTargetPos = Vector3.zero;
        }
    }


    // ---------------- DRAG ----------------
    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag = true;
        globalDragging = true;

        originalParent = transform.parent;
        originalIndex = transform.GetSiblingIndex();

        // ★ Ghost 放在 HandArea，而不是 Canvas
        ghost = Instantiate(ghostPrefab, handArea);
        ghost.transform.position = eventData.position;
        ghost.GetComponent<GhostCard>().Setup(instance.definition);

        // ★ 同步 RectTransform
        var ghostRect = ghost.GetComponent<RectTransform>();
        var cardRect = GetComponent<RectTransform>();

        if (ghostRect != null && cardRect != null)
        {
            ghostRect.sizeDelta = cardRect.sizeDelta;
            ghostRect.localScale = cardRect.localScale;
            ghostRect.pivot = cardRect.pivot;
            ghostRect.anchorMin = cardRect.anchorMin;
            ghostRect.anchorMax = cardRect.anchorMax;
        }
        Debug.Log("HAND AREA = " + handArea);
        // 隐藏真实卡牌
        canvasGroup.alpha = 0f;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            ghost.transform.position = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrag = false;
        globalDragging = false;
        if (ghost != null)
            Destroy(ghost);

        canvasGroup.alpha = 1f;

         // ★ 如果放开时指针在 DropZone → 打出卡
        if (dropZone != null && dropZone.isPointerInside)
            {
            PlayCard();   // 我们下一步创建
            return;
            }
        // hover 放大
        if (isHover)
        {
            visualTargetScale = visualBaseScale * hoverScale;
            visualTargetPos = new Vector3(0, hoverLift, 0);
        }
        else
        {
            visualTargetScale = visualBaseScale;
            visualTargetPos = Vector3.zero;
        }


        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalIndex);
        // ★ 刷新排版
        if (handArea != null)
            handArea.GetComponent<HandCurveLayout>().RefreshLayout();
        else
            Debug.LogError("CardUI 缺少 handArea 引用！");
    }

    public void Init(CardInstance inst)
    {
        instance = inst;

        cardArt.sprite = inst.definition.cardSprite;
        cardName.text = inst.definition.cardName;
        cardCost.text = inst.definition.cost.ToString();
        cardPolarity.text = inst.definition.polarity.ToString(); // Yin / Yang
    }
    private void ReparentKeepWorldPos(RectTransform target, Transform newParent)
    {
        Vector3 worldPos = target.position;    // 1. 记录世界坐标
        target.SetParent(newParent, false);    // 2. 改父物体，但不保留本地坐标
        target.position = worldPos;            // 3. 恢复世界坐标
    }
    private void PlayCard()
    {
        Debug.Log("Played card: " + instance.definition.cardName);

        // 1. 从手牌区移除 UI
        Destroy(gameObject);

        // 2. 从 HandArea 排版
        if (handArea != null)
            handArea.GetComponent<HandCurveLayout>().RefreshLayout();

        // 3. 通知战斗系统（下一步实现）
        // BattleManager.Instance.PlayCard(instance);
    }

}
