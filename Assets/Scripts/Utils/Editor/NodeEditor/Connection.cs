using System;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class Connection
    {
        public readonly ConnectionPoint InPoint;
        public readonly ConnectionPoint OutPoint;
        private readonly Color _connectionColor;

        public Connection( ConnectionPoint inPoint, ConnectionPoint outPoint, Color connectionColor )
        {
            InPoint = inPoint;
            OutPoint = outPoint;
            _connectionColor = connectionColor;
        }

        public void Draw()
        {
            Handles.DrawBezier(
                InPoint.Rect.center,
                OutPoint.Rect.center,
                InPoint.Rect.center - Vector2.up * 50f,
                OutPoint.Rect.center + Vector2.up * 50f,
                _connectionColor,
                null,
                2f
            );
        }
    }
}