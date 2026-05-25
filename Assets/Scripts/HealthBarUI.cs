using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    Image fill;

    void Start() => fill = GetComponent<Image>();

    void Update() => fill.fillAmount = playerHealth.HealthPercent;
}
