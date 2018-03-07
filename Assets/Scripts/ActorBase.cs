using System;
using System.Linq;
using ActorStates;
using Controllers;
using Gamelogic.Extensions;
using UnityEngine;

[ SelectionBase ]
public abstract class ActorBase<TActor> : GLMonoBehaviour where TActor : ActorBase<TActor>
{
    [ ReadOnly ]
    public string StateName;

    public event Action<IActorState, IActorState> StateChangeEvent;

    private IActorState _currentState;

    private IActorController<TActor> _controller;

    protected abstract IActorState CreateInitialState();

    protected abstract void ResetIntent();

    private void OnStateChangeEvent( IActorState previousState, IActorState nextState )
    {
        var handler = StateChangeEvent;
        if ( handler != null ) handler( previousState, nextState );
    }

    protected virtual void Start()
    {
        _controller = this.GetInterfaceComponents<IActorController<TActor>>().FirstOrDefault( controller => controller.Enabled );
        _currentState = CreateInitialState();
        _currentState.OnEnter();
        StateName = _currentState.Name;
    }

    protected virtual void Update()
    {
        // update intents
        ResetIntent();

        if ( _controller == null )
        {
            Debug.LogError( "Actor has no controller", this );
        }
        else
        {
            _controller.UpdateActorIntent( this as TActor );
        }

        if ( _currentState == null )
        {
            Debug.LogError( "Actor has no current state", this );
        }
        else
        {
            var nextState = _currentState.Update();
            if ( nextState != null )
            {
                TransitionToState( nextState );
            }
        }
    }

    protected void TransitionToState( IActorState nextState )
    {
        Debug.Log( string.Format( "{0} Going from {1} to {2}", this, _currentState.Name, nextState.Name ), this );
        _currentState.OnExit();
        OnStateChangeEvent( _currentState, nextState );
        nextState.OnEnter();
        _currentState = nextState;
        StateName = _currentState.Name;
    }
}