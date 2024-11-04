using UnityEngine;

public class Boss : MonoBehaviour
{
    public Chain Spine;
    public Chain[] Arms;
    private Vector2 targetPosition;
    public float[] BodyWidths = { 52, 58, 40, 60, 68, 71, 65, 50, 28, 15, 11, 9, 7, 7 };

    // Oscillation variables for spine animation
    public float spineWiggleSpeed = 3f;
    public float spineWiggleAmplitude = 0.15f;

    // Movement parameters
    public float movementSpeed = 2f; // Adjust this speed as necessary

    private LineRenderer spineRenderer;
    private LineRenderer[] armRenderers;

    void Start()
    {
        Vector2 origin = transform.position;

        // Initialize spine and limbs
        Spine = new Chain(origin, 14, 0.64f);
        Arms = new Chain[4];

        for (int i = 0; i < Arms.Length; i++)
        {
            Arms[i] = new Chain(origin, 3, i < 2 ? 0.52f : 0.36f);
        }

        // Set up LineRenderer for spine
        spineRenderer = gameObject.AddComponent<LineRenderer>();
        spineRenderer.positionCount = Spine.Joints.Count;
        spineRenderer.startWidth = 0.1f;
        spineRenderer.endWidth = 0.1f;
        spineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Set up LineRenderers for arms
        armRenderers = new LineRenderer[Arms.Length];
        for (int i = 0; i < Arms.Length; i++)
        {
            GameObject armObject = new GameObject("ArmRenderer" + i);
            armObject.transform.parent = transform;
            armRenderers[i] = armObject.AddComponent<LineRenderer>();
            armRenderers[i].positionCount = Arms[i].Joints.Count;
            armRenderers[i].startWidth = 0.1f;
            armRenderers[i].endWidth = 0.1f;
            armRenderers[i].material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    void Update()
    {
        // Check for mouse click and update target position
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;
            targetPosition = Camera.main.ScreenToWorldPoint(mousePos);
        }

        // Move towards the target position
        MoveTowardsTarget();

        // Resolve spine with oscillation effect
        Vector2 oscillatingTarget = targetPosition;
        for (int i = 0; i < Spine.Joints.Count; i++)
        {
            float offset = Mathf.Sin(Time.time * spineWiggleSpeed + i * 0.5f) * spineWiggleAmplitude;
            Vector2 offsetVector = new Vector2(offset, 0);
            Spine.Resolve(oscillatingTarget + offsetVector);

            // Shift target to flow down the spine
            oscillatingTarget = Spine.Joints[i].Position;
        }

        // Resolve arms with smooth interpolation
        for (int i = 0; i < Arms.Length; i++)
        {
            int side = i % 2 == 0 ? 1 : -1;
            int bodyIndex = i < 2 ? 3 : 7;
            float angleOffset = i < 2 ? Mathf.PI / 4 : Mathf.PI / 3;

            // Desired arm position with slight movement
            Vector2 desiredPos = GetSpinePosition(bodyIndex, angleOffset * side, 0.8f);
            Arms[i].Resolve(Vector2.Lerp(Arms[i].Joints[0].Position, desiredPos, 0.1f));
        }

        // Update the LineRenderers to draw the spine and arms
        UpdateLineRenderer(spineRenderer, Spine);
        for (int i = 0; i < Arms.Length; i++)
        {
            UpdateLineRenderer(armRenderers[i], Arms[i]);
        }
    }

    void MoveTowardsTarget()
    {
        // Calculate the direction to the target position
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Move the lizard toward the target position
        transform.position += (Vector3)direction * movementSpeed * Time.deltaTime;
    }

    Vector2 GetSpinePosition(int index, float angleOffset, float lengthOffset)
    {
        Vector2 jointPos = Spine.Joints[index].Position;
        float width = BodyWidths[index] + lengthOffset;
        return jointPos + new Vector2(Mathf.Cos(angleOffset) * width, Mathf.Sin(angleOffset) * width);
    }

    // Helper method to update a LineRenderer with chain joint positions
    void UpdateLineRenderer(LineRenderer lineRenderer, Chain chain)
    {
        for (int i = 0; i < chain.Joints.Count; i++)
        {
            lineRenderer.SetPosition(i, chain.Joints[i].Position);
        }
    }
}
