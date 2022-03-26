using UnityEngine;

public class PhysicsDrag : MonoBehaviour
{
    public float ForceFactor = 1;

    private Collider2D selectedCollider;
    private Vector2 localPos;

    private Vector2 GetWorldMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            var hit = Physics2D.GetRayIntersection(new Ray((Vector3)GetWorldMousePos() - Vector3.forward * 10, Vector3.forward));
            if (hit)
            {
                selectedCollider = hit.collider;
                localPos = hit.transform.InverseTransformPoint(hit.point);
            }
        }
        else if (Input.GetMouseButtonUp(2))
        {
            selectedCollider = null;
        }
    }

    private void FixedUpdate()
    {
        if (selectedCollider == null)
            return;

        var mousePos = GetWorldMousePos();
        Vector2 worldPos = selectedCollider.transform.TransformPoint(localPos);
        Vector2 dir = (mousePos - worldPos).normalized;
        float dst = (worldPos - mousePos).magnitude;
        var rb = selectedCollider.attachedRigidbody;
        rb.AddForceAtPosition(dir * dst * ForceFactor * rb.mass, worldPos);
    }

    private void OnDrawGizmos()
    {
        if (selectedCollider == null)
            return;

        Vector3 start = GetWorldMousePos();
        Vector2 end = selectedCollider.transform.TransformPoint(localPos);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);
    }
}
