using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Bonfire : MonoBehaviour, IInteractable
{
    [SerializeField] private string hintText = "[E] зажечь факел";
    [SerializeField] private Light2D bonfireLight;

    private bool IsInteractable = true;

    public void Interact(PlayerController player)
    {
        if (!IsInteractable) return;
        Debug.Log("Зажечь лучину от костра");

        player.InginteTorch();

        UIManager.Instance.ShowHint("Факел зажжен!", 1.5f);   
    }



    private void OnTriggerEnter2D(Collider2D other)
    { 
        if(other.CompareTag("Player") && IsInteractable) HintManager.Instance.RegisterHint(this, hintText, priotiry: 4f, duration: 0);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) HintManager.Instance.RemoveHintsFromSource(this);
    }
}
