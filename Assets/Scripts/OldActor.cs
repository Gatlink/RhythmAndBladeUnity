//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//
//public class OldActor : MonoBehaviour
//{
//// INTERN CLASSES
//	[System.Serializable]
//	public class SpeedData
//	{
//		private const float MIN_SPEED = 0.001f;
//
//		[Range(0f, 1f)]
//		public float acceleration = 0.2f;
//		public float maxSpeed = 4f;
//
//		[HideInInspector]
//		public float factor = 1f;
//
//		public float UpdateVelocity(float oldSpeed, float desiredSpeed)
//		{
//			var current = acceleration * desiredSpeed * factor + (1 - acceleration) * oldSpeed;
//			if ( Mathf.Abs( current ) < MIN_SPEED )
//			{
//				current = 0f;
//			}
//
//			return current;
//		}
//	}
//
//	[System.Serializable]
//	public class JumpParameter
//	{
//		public float duration = 0.2f;
//		public float verticalSpeed = 5f;
//		public float horizontalSpeedFactor = 0.5f;
//	}
//
//	[System.Serializable]
//	public class WallSlideParameter
//	{
//		public float gravityFactor = 0.1f;
//	}
//
//	[System.Serializable]
//	public class DashParameters
//	{
//		public float speed = 10f;
//	}
//
//// PARAMETERS
//	private const float RAIL_STICKINESS = 0.1f;
//
//	public bool ignoreCollisions;
//	public float collisionRadius = 1f;
//	public SpeedData horizontalMovement;
//	[Space]
//	public bool ignoreGravity;
//	public Transform railConnector;
//	public SpeedData falling;
//	[Space]
//	public JumpParameter jumpParameters;
//	[Space]
//	public WallSlideParameter wallSlideParameters;
//	[Space]
//	public DashParameters dashParameters;
//	
//	[HideInInspector]
//	public Vector2 desiredVelocity = new Vector2();
//	[HideInInspector]
//	public Vector2 currentVelocity = new Vector2();
//	[HideInInspector]
//	public Animator animator;
//
//	private GamepadControllerBase _controllerBase;
//	private ActorState currentState;
//
//#if UNITY_EDITOR
//	[Space]
//	public bool showTrajectory = true;
//
//	private readonly List<Vector2> prevPositions = new List<Vector2>();
//#endif
//
//
//// PROPERTY
//
//	public bool IsGrounded { get; private set; }
//	public float Direction { get; private set; }
//
//
//// UNITY MESSAGES
//
//	public void Start()
//	{
//		if (railConnector == null)
//			railConnector = transform;
//
//		//controller = new GamepadController(this);
//		TransitionTo<StateNormal>();
//		Direction = 1f;
//
//		animator = GetComponentInChildren<Animator>();
//	}
//
//	public void Update()
//	{
//		Vector3 railProj;
//		var hasProj = Rail.GetRailProjection(railConnector.position, out railProj);
//
//		desiredVelocity.Set(0f, 0f);
//		//controller.Update();
//		currentState.Update();
//
//		currentVelocity.x = horizontalMovement.UpdateVelocity(currentVelocity.x, desiredVelocity.x);
//		if (currentVelocity.x != 0f)
//			Direction = Mathf.Sign(currentVelocity.x);
//		
//		if (!ignoreGravity)
//			currentVelocity.y = falling.UpdateVelocity(currentVelocity.y, falling.maxSpeed);
//
//		transform.position = (Vector2) transform.position + currentVelocity * Time.deltaTime;
//
//		CheckGround(hasProj, railProj);
//		CheckCollisions();
//
////		var scale = transform.localScale;
////		if (currentVelocity.x > 0)
////			scale.x = 1;
////		else if (currentVelocity.x < 0)
////			scale.x = -1;
////		transform.localScale = scale;
//
//		if (animator != null)
//		{
//			animator.SetBool("Grounded", IsGrounded);
//			animator.SetFloat("HorizontalSpeed", Mathf.Abs(currentVelocity.x / horizontalMovement.maxSpeed));
//		}
//
//#if UNITY_EDITOR
//		if (!showTrajectory)
//			return;
//
//		prevPositions.Add(transform.position);
//		if (prevPositions.Count > 256)
//			prevPositions.RemoveAt(0);
//#endif
//	}
//
//#if UNITY_EDITOR
//	public void OnDrawGizmos()
//	{
//		Gizmos.color = Color.red;
//		Gizmos.DrawWireSphere(transform.position, collisionRadius);
//
//		for (var i = 0; showTrajectory &&  i < prevPositions.Count - 1; ++i)
//			Gizmos.DrawLine(prevPositions[i], prevPositions[i + 1]);
//	}
//#endif
//
//
//// FSM
//
//	public void TransitionTo<T>() where T : ActorState, new()
//	{
//		if (currentState is T)
//			return;
//
//		if (currentState != null)
//			currentState.OnExit();
//		
//		currentState = new T();
//		currentState.OnEnter(this);
//	}
//
//
//// UTILITY
//
//	public void Jump()
//	{
//		if (IsGrounded)
//			TransitionTo<StateJump>();
//	}
//
//	private void CheckGround(bool hasProj, Vector3 projection)
//	{
//		var delta = projection.y - railConnector.position.y;
//		if (hasProj && (projection.y > railConnector.position.y || Mathf.Abs(delta) <= RAIL_STICKINESS))
//		{
//			transform.position = transform.position + Vector3.up * delta;
//			currentVelocity.y = 0f;
//			IsGrounded = true;
//		}
//		else
//			IsGrounded = false;
//	}
//
//	private void CheckCollisions()
//	{
//		if (ignoreCollisions)
//			return;
//
//		var walls = Rail.GetAllCollidingWalls(transform.position, collisionRadius);
//		foreach (var wall in walls)
//		{
//			var offset = 0f;
//			if (transform.position.x > wall)
//				offset = wall - (transform.position.x - collisionRadius);
//			else
//				offset = wall - (transform.position.x + collisionRadius);
//
//			currentVelocity.x = 0f;
//			transform.position += Vector3.right * offset;
//		}
//
//		if (walls.Count > 0)
//			TransitionTo<StateWallSlide>();
//		else if (!(currentState is StateJump))
//			TransitionTo<StateNormal>();
//	}
//}
//
//
//// ACTOR STATE
//
//public abstract class ActorState
//{
//	protected OldActor actor;
//
//	public virtual void OnEnter(OldActor parent) { actor = parent; }
//	public virtual void Update() {}
//	public virtual void OnExit() {}
//}
//
//public class StateNormal : ActorState {}
//
//public class StateJump : ActorState
//{
//	private float startTime;
//
//	public override void OnEnter(OldActor parent)
//	{
//		base.OnEnter(parent);
//
//		actor.ignoreGravity = true;
//		startTime = Time.time;
//		actor.horizontalMovement.factor *= actor.jumpParameters.horizontalSpeedFactor;
//		actor.currentVelocity.y = actor.jumpParameters.verticalSpeed;
//
//		if (!actor.IsGrounded)
//			actor.TransitionTo<StateNormal>();
//		else if (actor.animator != null)
//			actor.animator.SetTrigger("Jump");
//	}
//
//	public override void Update()
//	{
//		actor.currentVelocity.y = Mathf.Lerp(actor.currentVelocity.y, 0, 0.2f);
//
//		if (Time.time >= startTime + actor.jumpParameters.duration)
//			actor.TransitionTo<StateNormal>();
//	}
//
//	public override void OnExit()
//	{
//		actor.ignoreGravity = false;
//		actor.horizontalMovement.factor /= actor.jumpParameters.horizontalSpeedFactor;
//	}
//}
//
//public class StateWallSlide : ActorState
//{
//	public override void OnEnter(OldActor parent)
//	{
//		base.OnEnter(parent);
//
//		actor.falling.factor *= actor.wallSlideParameters.gravityFactor;
//		
//		if (actor.IsGrounded)
//			actor.TransitionTo<StateNormal>();
//		else if (actor.animator != null)
//			actor.animator.SetBool("WallSlide", true);
//	}
//
//	public override void Update()
//	{
//		if (actor.IsGrounded)
//			actor.TransitionTo<StateNormal>();
//	}
//
//	public override void OnExit()
//	{
//		actor.falling.factor /= actor.wallSlideParameters.gravityFactor;
//		
//		if (actor.animator != null)
//			actor.animator.SetBool("WallSlide", false);
//	}
//}
//
//public class StateDash : ActorState
//{
//	private bool oldIgnoreGravity;
//	private float oldFactor;
//
//	public override void OnEnter(OldActor parent)
//	{
//		base.OnEnter(parent);
//
//		actor.currentVelocity.x = actor.dashParameters.speed * actor.Direction;
//		
//		oldIgnoreGravity = actor.ignoreGravity;
//		actor.ignoreGravity = true;
//		
//		oldFactor = actor.horizontalMovement.factor;
//		actor.horizontalMovement.factor = 0f;
//
//		if (actor.animator != null)
//			actor.animator.SetTrigger("Dash");
//	}
//
//	public override void Update()
//	{
//		if (actor.currentVelocity.x == 0f)
//			actor.TransitionTo<StateNormal>();
//	}
//
//	public override void OnExit()
//	{
//		actor.ignoreGravity = oldIgnoreGravity;
//		actor.horizontalMovement.factor = oldFactor;
//	}
//}
//
//public class StateAttack : ActorState
//{
//	private bool oldIgnoreGravity;
//	private float oldFactor;
//
//	public override void OnEnter(OldActor parent)
//	{
//		base.OnEnter(parent);
//
//		actor.currentVelocity.x = actor.dashParameters.speed * actor.Direction;
//		
//		oldIgnoreGravity = actor.ignoreGravity;
//		actor.ignoreGravity = true;
//		
//		oldFactor = actor.horizontalMovement.factor;
//		actor.horizontalMovement.factor = 0f;
//	}
//
//	public override void Update()
//	{
//		if (actor.currentVelocity.x == 0f)
//			actor.TransitionTo<StateNormal>();
//	}
//
//	public override void OnExit()
//	{
//		actor.ignoreGravity = oldIgnoreGravity;
//		actor.horizontalMovement.factor = oldFactor;
//	}
//}