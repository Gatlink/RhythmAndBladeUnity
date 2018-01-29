using UnityEngine;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Rail : MonoBehaviour
{
    public const float GIZMORADIUS = 0.1f;
    public static readonly Color IDLECOLOR = new Color(0.4f, 0.4f, 0.5f);
    public static readonly Color ACTIVECOLOR = new Color(0.4f, 0.5f, 0.7f);
    public static readonly Color SELECTEDCOLOR = new Color(0.5f, 0.8f, 1f);
    public static readonly Color WALLCOLOR = new Color(0.7f, 0.5f, 0.4f);
    public static readonly Color NEWCOLOR = new Color(0.5f, 0.8f, 1f, 0.5f);


// STATIC

    public static readonly List<Rail> all = new List<Rail>();

    public static bool GetRailProjection(Vector3 pos, out Vector3 result)
    {
        result = Vector3.zero;
        var points = new List<Vector3>();
        foreach (var rail in Rail.all)
        {
            Vector3 onRail;
            if (rail.GetClosestPointOnRail(pos, out onRail))
                points.Add(onRail);
        }

        if (points.Count == 0)
            return false;

        var lastDis = float.MaxValue;
        foreach (var p in points)
        {
            var curDis = Vector2.SqrMagnitude(p - pos);
            if (curDis < lastDis)
            {
                result = p;
                lastDis = curDis;
            }
        }

        return true;
    }

    public static List<float> GetAllCollidingWalls(Vector3 pos, float radius)
    {
        var walls = new List<float>();
        foreach (var rail in all)
        {
            var collidingWalls = rail.GetCollidingWalls(pos, radius);
            foreach (var wall in collidingWalls)
                walls.Add(wall);
        }

        return walls;
    }


// METHODS

    [HideInInspector, SerializeField]
    public List<Vector3> points;

    public void Reset()
    {
        if (points == null)
            points = new List<Vector3>();
        
        points.Clear();
        points.Add(new Vector2(-1, 0));
        points.Add(new Vector2(1, 0));
    }

    public void Awake()
    {
        all.Add(this);
    }

    public bool GetProjection(int idx, Vector3 pos, out Vector3 proj)
    {
        proj = Vector3.negativeInfinity;
        if (idx > points.Count - 1)
            return false;

        var prev = points[idx] + transform.position;
        var next = points[idx + 1] + transform.position;

        if ((prev.x <= pos.x && next.x >= pos.x)
            || (prev.x >= pos.x && next.x <= pos.x))
        {
            var a = (next.y - prev.y) / (next.x - prev.x);
            var b = next.y - a * next.x;
            proj = new Vector3(pos.x, a * pos.x + b, 0f);
            return true;
        }

        return false;
    }

    public void GetPreviousAndNextPoint(Vector3 pos, out int prev, out int next)
    {
        prev = next = -1;
        if (points.Count == 0)
            return;
        
        if (points.Count == 1)
        {
            prev = next = 0;
            return;
        }

        var pts = new Dictionary<int, float>();
        for(var i = 0; i < points.Count - 1; ++i)
        {
            Vector3 proj;
            if (GetProjection(i, pos, out proj))
                pts.Add(i, Vector2.Distance(proj, pos));
        }

        if (pts.Count <= 0)
            return;

        prev = pts.OrderBy(kvp => kvp.Value).FirstOrDefault().Key;
        if (prev >= 0 && prev < points.Count - 1)
            next = prev + 1;
    }

    public bool GetClosestPointOnRail(Vector3 pos, out Vector3 projection)
    {
        projection = Vector3.negativeInfinity;
        if (points.Count == 0)
            return false;
        
        if (points.Count == 1)
        {
            projection = points[0];
            return true;
        }

        var pts = new List<Vector3>();
        for(var i = 0; i < points.Count - 1; ++i)
        {
            Vector3 proj;
            if (GetProjection(i, pos, out proj))
                pts.Add(proj);
        }

        var lastSqrDistance = float.MaxValue;
        var result = false;
        foreach (var point in pts)
        {
            var currentSqrDistance = Vector2.Distance(point, pos);
            if (currentSqrDistance < lastSqrDistance)
            {
                projection = point;
                lastSqrDistance = currentSqrDistance;
                result = true;
            }
        }

        return result;
    }

    private List<float> GetCollidingWalls(Vector3 pos, float radius)
    {
        var result = new List<float>();
        if (points.Count < 2)
            return result;

        for (var i = 0; i < points.Count - 1; ++i)
        {
            var prev = points[i] + transform.position;
            var next = points[i + 1] + transform.position;
            if (prev.x != next.x)
                continue;

            var wall = prev.x;
            if (wall < pos.x - radius || wall > pos.x + radius)
                continue;
            
            var maxY = Mathf.Max(prev.y, next.y);
            var minY = Mathf.Min(prev.y, next.y);
            if (maxY > pos.y && minY < pos.y)
                result.Add(wall);
        }

        return result;
    }

#if UNITY_EDITOR
    private const KeyCode ADDKEY = KeyCode.LeftControl;
    private const KeyCode DELKEY = KeyCode.Delete;

    private static Vector3 dragWorldStart;
    private static Vector2 dragMouseCurrent;
    private static Vector2 dragMouseStart;
    private static bool addKeyDown;

    public void OnDrawGizmos()
    {
        if (Selection.activeTransform != transform)
            DrawHandles();
    }

    private void DrawHandles()
    {
        Handles.color = IDLECOLOR;
        
        DrawLines();

        for (var i = 0; i < points.Count; ++i)
        {
            var cur = transform.position + points[i];
            Handles.DrawSolidDisc(cur, Vector3.forward, GIZMORADIUS);
        }
    }

    public void DrawHandlesEditable()
    {
        Handles.color = ACTIVECOLOR;
        DrawLines();

        var newList = new List<Vector3>();
        var newIndex = -1;
        var newPoint = DisplayNewPoint(out newIndex);

        for (var i = 0; i < points.Count; ++i)
        {
            var cur = transform.position + points[i];
            if (DrawPointHandle(ref cur, i))
                newList.Add(cur - transform.position);
        }

        if (newIndex != -1)
        {
            newList.Insert(newIndex, newPoint - transform.position);
            Undo.RecordObject(this, "Add new point");
        }

        points.Clear();
        points.AddRange(newList);
    }

    private void DrawLines()
    {
        var oldColor = Handles.color;
        for (var i = 0; i < points.Count - 1; ++i)
        {
            var cur = transform.position + points[i];
            var next = transform.position + points[i + 1];
            
            if (next.x == cur.x)
                Handles.color = WALLCOLOR;
            
            Handles.DrawLine(cur, next);
            Handles.color = oldColor;
        }
    }

    private bool ShouldBeAWall(int idx, Vector3 position)
    {
        if (idx < 0 || idx >= points.Count)
            return false;
        
        var point = points[idx] + transform.position;
        var prevSlope = point.Slope(position);
        return prevSlope > 45;
    }

    private Vector3 DisplayNewPoint(out int newIndex)
    {
        newIndex = -1;

        var mousePosition = Event.current.mousePosition;
        mousePosition.y = Camera.current.pixelHeight - mousePosition.y;
        
        var position = Handles.inverseMatrix.MultiplyPoint(Camera.current.ScreenToWorldPoint(mousePosition));
        position.z = 0f;
        
        var prev = 0;
        var next = 0;
        if (addKeyDown)
        {
            GetPreviousAndNextPoint(position, out prev, out next);
            if ((prev == -1 || next == -1) && points.Count > 1)
                prev = next = Vector2.Distance(points[0] + transform.position, position) > Vector2.Distance(points[points.Count - 1] + transform.position, position) ? points.Count - 1 : 0;

            if (prev == -1)
                prev = next = 0;

            if (ShouldBeAWall(prev, position))
                position.x = points[prev].x + transform.position.x;

            if (ShouldBeAWall(next, position))
                position.x = points[next].x + transform.position.x;
        }

        switch (Event.current.type)
        {
        case EventType.KeyDown:
            if (Event.current.keyCode == ADDKEY)
                addKeyDown = true;
            break;

        case EventType.KeyUp:
            if (Event.current.keyCode == ADDKEY)
                addKeyDown = false;
            break;

        case EventType.Repaint:
            if (addKeyDown)
            {
                var oldColor = Handles.color;
                Handles.color = NEWCOLOR;

                Handles.DrawLine(points[prev] + transform.position, position);
                if (next != -1) Handles.DrawLine(points[next] + transform.position, position);
                Handles.DrawSolidDisc(position, Vector3.forward, GIZMORADIUS);
                
                Handles.color = oldColor;
                HandleUtility.Repaint();
            }
            break;

        case EventType.MouseDown:
            if (addKeyDown && Event.current.button == 0)
            {
                newIndex = next == 0 ? 0 : prev + 1;
                Event.current.Use();
                return position;
            }
            break;
        }

        return Vector3.zero;
    }

    private bool DrawPointHandle(ref Vector3 position, int index)
    {
        var id = GUIUtility.GetControlID(FocusType.Passive);
        var screenPosition = Handles.matrix.MultiplyPoint(position);
        var cachedMatrix = Handles.matrix;

        switch (Event.current.GetTypeForControl(id))
        {
        case EventType.MouseDown:
            if (Event.current.button != 0) break;
            if (HandleUtility.nearestControl == id)
            {
                GUIUtility.hotControl = id;

                dragMouseCurrent = dragMouseStart = Event.current.mousePosition;
                dragWorldStart = position;
                
                Event.current.Use();
                EditorGUIUtility.SetWantsMouseJumping(1);
            }
            break;

        case EventType.MouseUp:
            if (Event.current.button != 0) break;
            if (GUIUtility.hotControl == id && Event.current.button == 0)
            {
                GUIUtility.hotControl = 0;
                Event.current.Use();
                EditorGUIUtility.SetWantsMouseJumping(0);
            }
            break;

        case EventType.MouseDrag:
            if (GUIUtility.hotControl == id)
            {
                dragMouseCurrent += new Vector2(Event.current.delta.x, -Event.current.delta.y);

                position = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(dragWorldStart)) + (Vector3) (dragMouseCurrent - dragMouseStart);
                position = Handles.matrix.inverse.MultiplyPoint(Camera.current.ScreenToWorldPoint(position));
                
                if (index > 0 && ShouldBeAWall(index - 1, position))
                    position.x = points[index - 1].x + transform.position.x;

                if (index < points.Count - 1 && ShouldBeAWall(index + 1, position))
                    position.x = points[index + 1].x + transform.position.x;

                GUI.changed = true;
                Event.current.Use();
                Undo.RecordObject(this, "Moved points");
            }
            break;

        case EventType.KeyDown:
            if (GUIUtility.hotControl == id && Event.current.keyCode == DELKEY)
            {
                Event.current.Use();
                Undo.RecordObject(this, "Delete point");
                return false;
            }
            break;

        case EventType.Repaint:
            var currentColour = Handles.color;
            if (id == GUIUtility.hotControl)
                Handles.color = SELECTEDCOLOR;

            Handles.matrix = Matrix4x4.identity;
            Handles.DrawSolidDisc(position, Vector3.forward, GIZMORADIUS);
            Handles.matrix = cachedMatrix;
            Handles.color = currentColour;
            break;

        case EventType.Layout:
            Handles.matrix = Matrix4x4.identity;
            HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(screenPosition, GIZMORADIUS));
            Handles.matrix = cachedMatrix;
            break;
        }

        return true;
    }
#endif
}