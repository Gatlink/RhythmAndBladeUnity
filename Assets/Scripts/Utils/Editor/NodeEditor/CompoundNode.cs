﻿using System;
using System.Collections.Generic;
using Controllers;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class CompoundNode : Node
    {
        public new CompoundBehaviourNode BehaviourNode { get; protected set; }

        public readonly List<ConnectionPoint> OutPoints;

        private readonly GUIStyle _outPointStyle;
        private readonly Action<ConnectionPoint> _onClickOutPoint;
        private readonly Action<ConnectionPoint> _onClickRemoveConnectionPoint;
        private readonly Action<ConnectionPoint, int> _onClickMoveConnectionPoint;

        public CompoundNode( CompoundBehaviourNode behaviourNode, Vector2 position, GUIStyle nodeStyle,
            GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle,
            Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
            Action<Node> onClickRemoveNode, Action<Node> onDoubleClickNode,
            Action<ConnectionPoint> onClickRemoveConnectionPoint, Action<ConnectionPoint, int> onClickMoveConnectionPoint, Func<Node, bool> onClickNode,
            Action<Node> onClickMainNode )
            : base( behaviourNode, position, nodeStyle, selectedStyle, inPointStyle, onClickInPoint, onClickRemoveNode,
                onDoubleClickNode, onClickNode, onClickMainNode )
        {
            BehaviourNode = behaviourNode;
            OutPoints = new List<ConnectionPoint>();
            _outPointStyle = outPointStyle;
            _onClickOutPoint = onClickOutPoint;
            _onClickRemoveConnectionPoint = onClickRemoveConnectionPoint;
            _onClickMoveConnectionPoint = onClickMoveConnectionPoint;
        }

        public override void Draw()
        {
            if ( OutPoints != null )
            {
                foreach ( var outPoint in OutPoints )
                {
                    outPoint.Draw();
                }
            }

            base.Draw();

            using ( new GUI.GroupScope( Rect, GUIStyle.none ) )
            {
                const int iconSize = 16;
                const int iconSpacing = 4;
                const int iconCount = 3;
                var iconsRect = new Rect(
                    Rect.width / 2 - 0.5f * iconCount * iconSize - 0.5f * ( iconCount - 1 ) * iconSpacing,
                    Rect.height / 2 - 0.5f * iconSize + 6,
                    iconSize, iconSize );
                if ( BehaviourNode.Randomize )
                {
                    GUI.DrawTexture( iconsRect, EditorGUIUtility.Load( "random.png" ) as Texture );
                    iconsRect.x += iconSize + iconSpacing;
                }

                if ( BehaviourNode.LoopRepeat )
                {
                    GUI.DrawTexture( iconsRect, EditorGUIUtility.Load( "loop.png" ) as Texture );
                    iconsRect.x += iconSize + iconSpacing;
                }

                if ( BehaviourNode.UseHealthEndCondition )
                {
                    GUI.DrawTexture( iconsRect, EditorGUIUtility.Load( "health.png" ) as Texture );
                }
            }
        }

        public Vector2 GetOutConnectionPointPosition( int indexInNode )
        {
            var pos = Vector2.zero;

            var step = Rect.width / ( OutPoints.Count + 1 );
            pos.x = Rect.x + ( indexInNode + 1 ) * step;
            pos.y = Rect.y + Rect.height;

            return pos;
        }

        public override Vector2 GetConnectionPointPosition( ConnectionPoint point )
        {
            if ( point == InPoint )
            {
                return GetInConnectionPointPosition();
            }

            var index = OutPoints.IndexOf( point );
            if ( index >= 0 )
            {
                return GetOutConnectionPointPosition( index );
            }

            throw new ArgumentException( "Connection point not found" );
        }

        public override IEnumerable<ConnectionPoint> EnumerateConnectionPoints()
        {
            yield return InPoint;
            foreach ( var outPoint in OutPoints )
            {
                yield return outPoint;
            }
        }

        public ConnectionPoint AddOutConnectionPoint()
        {
            var connectionPoint = new ConnectionPoint( this, ConnectionPointType.Out, _outPointStyle, _onClickOutPoint,
                _onClickRemoveConnectionPoint, _onClickMoveConnectionPoint );
            OutPoints.Add( connectionPoint );
            return connectionPoint;
        }

        public void RemoveConnectionPoint( ConnectionPoint connectionPoint )
        {
            OutPoints.Remove( connectionPoint );
        }
    }
}