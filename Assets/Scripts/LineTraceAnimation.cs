using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTraceAnimation : MonoBehaviour
{
    
    public Material material;
    public AnimationCurve widthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 2f), new Keyframe(1.0f, 0f) });
    public float lifeTime = 0.5f;
    private GameObject trail;
    void Awake()
    {
        CreateTrailObject();
    }
    private void CreateTrailObject()
    {
        trail = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
        trail.AddComponent<TrailRenderer>();
        trail.GetComponent<TrailRenderer>().material = material;
        trail.GetComponent<TrailRenderer>().widthCurve = widthCurve;
        trail.GetComponent<TrailRenderer>().time = lifeTime;
        trail.GetComponent<TrailRenderer>().sortingOrder = 1;
        trail.GetComponent<TrailRenderer>().emitting = false;
        trail.name = "TrailCursor";
    }
    private void Update()
    {
        UpdateTrailPosition();
    }
    private void UpdateTrailPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
            trail.transform.position = hit.point;
        if (Input.GetButton("Fire1"))
        {
            trail.GetComponent<TrailRenderer>().emitting = true;
        }
        else
        {
            trail.GetComponent<TrailRenderer>().emitting = false;
        }
    }
}
