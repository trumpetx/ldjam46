using UnityEngine;
using UnityEngine.UI;

public class HealthOverTime : MonoBehaviour
{
    public GameObject fct;
    public float manaCost = 10f;
    public float change;
    public string affectsTag;
    private float _changed;

    private void OnDestroy()
    {
        Debug.Log("Changed " + _changed);
        Fct();
    }

    private void Fct()
    {
        if (!fct) return;
        var floatingCombatText = Instantiate(fct, transform.position, Quaternion.identity);
        var text = floatingCombatText.GetComponentInChildren<Text>();
        text.text = "" + (int) _changed;
        text.color = (_changed > 0) ? Color.yellow : Color.red;
        
        Destroy(floatingCombatText, 3);
    }

    private void OnTriggerStay(Collider c)
    {
        if (affectsTag == null || !c.gameObject.CompareTag(affectsTag) || c.GetComponent<Reinforcements>()) return;
        var health = c.GetComponent<Health>();
        if (health)
        {
            _changed += health.ChangeHitpints(change * Time.deltaTime);
        }
    }
}
