using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillNodeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillDesc;
    [SerializeField] private Image skillIcon;
    //[SerializeField] private TextMeshProUGUI skillLevel;
    [SerializeField] private TextMeshProUGUI skillCost;
    [SerializeField] private Image lockOverlay;
    [SerializeField] private GameObject[] slots;

    private SkillNodeSO currentData;
    private Checkpoint checkpointManager;
    private int currentlevel;

    public SkillNodeSO CurrentData => currentData;


    public void Initialize(SkillNodeSO data, Checkpoint manag, int startLevel)
    {
        currentData = data;
        checkpointManager = manag;
        currentlevel = startLevel;

        skillName.text = data.NodeName;
        skillDesc.text = data.NodeDescription;
        skillIcon.sprite = data.NodeIcon;
        //skillLevel.text = $"Ур. {startLevel} / {data.MaxLevel}";
        skillCost.text = $"{data.SkillPointsForOneLevel} очков";

        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    public void UpdateVisualState(bool isUnlocked, bool canAfford, bool isMaxed)
    {
        if (!isUnlocked)
        {
            if (lockOverlay) lockOverlay.gameObject.SetActive(true);
            GetComponent<Button>().interactable = false;
            //GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            return;
        }
        else
        {
            if (lockOverlay) lockOverlay.gameObject.SetActive(false);
            //GetComponent<Image>().color = Color.white;
        }

        if(slots != null)
        {
            for(int i = 0;  i < slots.Length; i++)
            {
                Image slotImage = slots[i].GetComponent<Image>();
                if (i < currentlevel) slotImage.color = Color.green;
                else slotImage.color = isMaxed ? Color.green : new Color(0.5f, 0.2f, 0.2f);
            }
        }

        var btn = GetComponent<Button>();
        btn.interactable = !isMaxed && canAfford;

        if (isMaxed)
        {
            if (skillCost) skillCost.text = "МАКС";
        }
        else
        {
            if (skillCost)
            {
                skillCost.text = $"{currentData.SkillPointsForOneLevel} очков";
                skillCost.color = canAfford ? Color.white : Color.gray;
            }
        }
    }

    public void SetCurrentLevel(int newLevel)
    {
        currentlevel = newLevel;
    }


    private void OnButtonClick()
    {
        checkpointManager?.UpgradeSkills(this);
    }

}
