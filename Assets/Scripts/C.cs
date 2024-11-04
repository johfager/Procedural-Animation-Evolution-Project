using UnityEngine;
using System.Collections.Generic;

public class Chain
{
    public List<Point> Joints;
    public float SegmentLength;

    public Chain(Vector2 origin, int numSegments, float segmentLength)
    {
        Joints = new List<Point>();
        SegmentLength = segmentLength;

        // Initialize points along the chain
        for (int i = 0; i < numSegments; i++)
        {
            Joints.Add(new Point(origin + new Vector2(i * segmentLength, 0)));
        }
    }

    // Resolves each point towards the target using FABRIK or simple IK logic
    public void Resolve(Vector2 target)
    {
        Joints[0].Position = target; // Head follows target

        // Forward pass (from head to tail)
        for (int i = 1; i < Joints.Count; i++)
        {
            Vector2 direction = (Joints[i].Position - Joints[i - 1].Position).normalized;
            Joints[i].Position = Joints[i - 1].Position + direction * SegmentLength;
        }

        // Backward pass (from tail to head)
        for (int i = Joints.Count - 2; i >= 0; i--)
        {
            Vector2 direction = (Joints[i].Position - Joints[i + 1].Position).normalized;
            Joints[i].Position = Joints[i + 1].Position + direction * SegmentLength;
        }
    }
}
