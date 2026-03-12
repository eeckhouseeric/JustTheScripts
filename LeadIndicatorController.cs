using UnityEngine;

public class LeadIndicatorController : MonoBehaviour
{
    public Transform target; // assigned dynamically
    public float projectileSpeed = 200f;

    private Rigidbody targetRb;
    private LeadIndicator indicator;

    public void Initialize(LeadIndicator indicator, Transform target)
    {
        this.indicator = indicator;
        this.target = target;
        targetRb = target.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (indicator == null || target == null || targetRb == null)
            return;

        Vector3 predicted = PredictInterceptPoint();
        indicator.SetWorldPosition(predicted);
    }

    private Vector3 PredictInterceptPoint()
    {
        Vector3 targetPos = target.position;
        Vector3 targetVel = targetRb.linearVelocity;

        Vector3 shooterPos = transform.position;
        Vector3 toTarget = targetPos - shooterPos;

        float a = Vector3.Dot(targetVel, targetVel) - projectileSpeed * projectileSpeed;
        float b = 2 * Vector3.Dot(targetVel, toTarget);
        float c = Vector3.Dot(toTarget, toTarget);

        float disc = b * b - 4 * a * c;

        if (disc < 0)
            return targetPos;

        float t1 = (-b + Mathf.Sqrt(disc)) / (2 * a);
        float t2 = (-b - Mathf.Sqrt(disc)) / (2 * a);

        float t = Mathf.Max(t1, t2);

        if (t < 0)
            return targetPos;

        return targetPos + targetVel * t;
    }
}
