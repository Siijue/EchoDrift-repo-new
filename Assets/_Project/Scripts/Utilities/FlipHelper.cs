using UnityEngine;

public class FlipHelper : MonoBehaviour
{
    public static void Flip(Transform t, bool facingLeft)
    {
        float absX = Mathf.Abs(t.localScale.x);
        t.localScale = new Vector3(facingLeft ? -absX : absX, t.localScale.y, t.localScale.z);
    }
}
