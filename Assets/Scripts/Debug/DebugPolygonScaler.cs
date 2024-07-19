using System.Collections.Generic;
using UnityEngine;

public class DebugPolygonScaler : MonoBehaviour
{
    public LineRenderer originalLineRenderer;
    // Used to draw the original polygon.
    public LineRenderer movedLineRenderer;
    // Used to draw the polygon after the move.
    public float moveDistance = 0.5f;
    // Distance to center
                                      // Create a simple list of polygon vertices.
  public  List<Vector2> vertices = new List<Vector2>()
        {
            new Vector2(-1, -1),
            new Vector2(1, -1),
            new Vector2(1, 1),
            new Vector2(-1, 1)
        };

    void Update()
    {


        // Calculate the polygon midpoint and move the vertices.
        List<Vector2> movedVertices = MoveVerticesTowardsCenter(vertices, moveDistance);

        // Draw the original polygon
        DrawPolygon(originalLineRenderer, vertices);

        // Draw the moved polygon.
        DrawPolygon(movedLineRenderer, movedVertices);
    }

    // Method to move polygon vertices to the center
    private List<Vector2> MoveVerticesTowardsCenter(List<Vector2> originalVertices, float distance)
    {
        Vector2 center = CalculatePolygonCenter(originalVertices);
        List<Vector2> movedVertices = new List<Vector2>();
        foreach (Vector2 vertex in originalVertices)
        {
            Vector2 direction = (center - vertex).normalized;
            // Direction from vertex to center
            Vector2 movedVertex = vertex + direction * distance;
            // Move a fixed distance to the center.
            movedVertices.Add(movedVertex);
        }
        return movedVertices;
    }

    // Method for calculating the center of a polygon
    private Vector2 CalculatePolygonCenter(List<Vector2> vertices)
    {
        Vector2 sum = Vector2.zero;
        foreach (Vector2 vertex in vertices)
        {
            sum += vertex;
        }
        return sum / vertices.Count;
    }

    // Method for drawing polygons with the LineRenderer.
    private void DrawPolygon(LineRenderer lineRenderer, List<Vector2> vertices)
    {
        lineRenderer.positionCount = vertices.Count + 1;
        // set the number of vertices (+1 to close the polygon)
        for (int i = 0; i < vertices.Count; i++)
        {
            lineRenderer.SetPosition(i, vertices[i]);
        }
        lineRenderer.SetPosition(vertices.Count, vertices[0]);
        // Closed polygon
    }
}
