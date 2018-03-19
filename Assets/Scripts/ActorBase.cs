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

    public IActorState CurrentState { get; private set; }

    private IActorController<TActor> _controller;

    protected abstract IActorState CreateInitialState();

    private void OnStateChangeEvent( IActorState previousState, IActorState nextState )
    {
        var handler = StateChangeEvent;
        if ( handler != null ) handler( previousState, nextState );
    }

    protected virtual void Start()
    {
        CurrentState = CreateInitialState();
        CurrentState.OnEnter();
        StateName = CurrentState.Name;
    }

    protected virtual void Update()
    {
        FindController();

        if ( _controller != null && _controller.Enabled )
        {
            _controller.UpdateActorIntent( this as TActor );
        }
        
        if ( CurrentState == null )
        {
            Debug.LogError( "Actor has no current state", this );
        }
        else
        {
            var nextState = CurrentState.Update();
            if ( nextState != null )
            {
                TransitionToState( nextState );
            }
        }
    }

    private void FindController()
    {
        if ( _controller == null || !_controller.Enabled )
        {
            _controller = this.GetInterfaceComponents<IActorController<TActor>>()
                .FirstOrDefault( controller => controller.Enabled );
        }
    }

    protected void TransitionToState( IActorState nextState )
    {
#if DEBUG_ACTOR_STATE
        Debug.Log( string.Format( "{0} Going from {1} to {2}", this, CurrentState.Name, nextState.Name ), this );
#endif
        CurrentState.OnExit();
        OnStateChangeEvent( CurrentState, nextState );
        nextState.OnEnter();
        CurrentState = nextState;
        StateName = CurrentState.Name;
    }
}