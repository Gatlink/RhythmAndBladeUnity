using System;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class Connection
    {
        public readonly ConnectionPoint InPoint;
        public readonly ConnectionPoint OutPoint;
        private readonly Action<Connection> _onClickRemoveConnection;
        private readonly Color _connectionColor;

        public Connection( ConnectionPoint inPoint, ConnectionPoint outPoint,
            Action<Connection> onClickRemoveConnection, Color connectionColor )
        {
            InPoint = inPoint;
            OutPoint = outPoint;
            _onClickRemoveConnection = onClickRemoveConnection;
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

//            if ( Handles.Button( ( InPoint.Rect.center + OutPoint.Rect.center ) * 0.5f, Quaternion.identity, 4, 8,
//                Handles.RectangleHandleCap ) )
//            {
//                if ( _onClickRemoveConnection != null )
//                {
//                    _onClickRemoveConnection( this );
//                }
//            }
        }
    }
}