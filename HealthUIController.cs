using UnityEngine;
using UnityEngine.UI;
public class HealthUIController : MonoBehaviour
{
    public Slider healthBarSlider;

    private PlaneHealth boundHealth;

    public void Bind(PlaneHealth health)
    { 
    
        if(boundHealth != null)
            boundHealth.OnHealthUpdated -= UpdateHealth;
        boundHealth = health;
        boundHealth.OnHealthUpdated += UpdateHealth;

        SetMaxHealth(boundHealth.MaxHealth);
        UpdateHealth(boundHealth.Health, boundHealth.MaxHealth);

    }

    public void Unbind()
    {
        if (boundHealth != null)
            boundHealth.OnHealthUpdated -= UpdateHealth;
        boundHealth = null;
    }


    public void SetMaxHealth( float max)
    {
        if (healthBarSlider == null) return;
        healthBarSlider.maxValue = max;
        healthBarSlider.value = max;
   
    }

    public void UpdateHealth(int current , int max)

    {
        if (healthBarSlider == null) return;
        {
            healthBarSlider.maxValue = max;
            healthBarSlider.value = current;
        }
    }

}
