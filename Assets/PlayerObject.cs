using Lofle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerObject : NetworkBehaviour
{
	public enum eSprite
	{
		Idle,
		Run,
		Jump
	}

	[SerializeField]
	private Rigidbody2D _rigidbody = null;

	[SerializeField]
	private SpriteRenderer _spriteIdle = null;
	[SerializeField]
	private SpriteRenderer _spriteRun = null;
	[SerializeField]
	private SpriteRenderer _spriteJump = null;

	private StateMachine<PlayerObject> _stateMachine = null;

	[SerializeField]
	private float _speed = 5.0f;
	[SerializeField]
	private float _jump = 10.0f;

	private bool isJump
	{
		get
		{
			return 0 != _rigidbody.velocity.y;
		}
	}

	void Start ()
	{
		ShowSprite( eSprite.Idle );
		if( !isLocalPlayer ) return;

		_stateMachine = new StateMachine<PlayerObject>( this );
		StartCoroutine( _stateMachine.Coroutine<IdleState>() );
	}

	[Command]
	private void CmdJump()
	{
		RpcJump();
	}

	[ClientRpc]
	private void RpcJump()
	{
		if( !isClient ) return;
		if( !isJump )
		{
			_rigidbody.AddForce( new Vector2( 0, _jump ) );
		}
	}

	[Command]
	private void CmdMove( bool bLeft )
	{
		RpcMove( bLeft );
	}
	[ClientRpc]
	private void RpcMove( bool bLeft )
	{
		if( !isClient ) return;
		_rigidbody.AddForce( new Vector2( ( bLeft ? -_speed : _speed ), 0 ) );
	}

	[Command]
	private void CmdLookAt( bool bLeft )
	{
		RpcLookAt( bLeft );
	}
	[ClientRpc]
	private void RpcLookAt( bool bLeft )
	{
		if( !isClient ) return;
		transform.rotation = Quaternion.Euler( new Vector3( transform.rotation.x, bLeft ? 180 : 0, transform.rotation.z ) );
	}

	private void HideAllSprite()
	{
		_spriteIdle.enabled = false;
		_spriteRun.enabled = false;
		_spriteJump.enabled = false;
	}

	[Command]
	private void CmdShowSprite(eSprite type)
	{
		RpcShowSprite( type );
	}

	[ClientRpc]
	private void RpcShowSprite( eSprite type )
	{
		if( !isClient ) return;
		ShowSprite( type );
	}

	private void ShowSprite( eSprite type )
	{
		HideAllSprite();

		switch( type )
		{
			case eSprite.Idle:
				_spriteIdle.enabled = true;
				break;

			case eSprite.Run:
				_spriteRun.enabled = true;
				break;

			case eSprite.Jump:
				_spriteJump.enabled = true;
				break;
		}
	}

	private class IdleState : State<PlayerObject>
	{
		protected override void Begin()
		{
			Owner.CmdShowSprite( eSprite.Idle );
		}

		protected override void Update()
		{
			if( Input.GetKey( KeyCode.LeftArrow ) || Input.GetKey( KeyCode.RightArrow ) )
			{
				Invoke<RunState>();
			}

			if( Input.GetKey( KeyCode.Space ) )
			{
				Invoke<JumpState>();
			}
		}

		protected override void End()
		{
		}
	}

	private class RunState : State<PlayerObject>
	{
		protected override void Begin()
		{
			Owner.CmdShowSprite( eSprite.Run );
		}

		protected override void Update()
		{
			if( Input.GetKey( KeyCode.LeftArrow ) )
			{
				Owner.CmdMove( true );
				Owner.CmdLookAt( true );
			}
			else if( Input.GetKey( KeyCode.RightArrow ) )
			{
				Owner.CmdMove( false );
				Owner.CmdLookAt( false );
			}
			else
			{
				Invoke<IdleState>();
			}

			if( Input.GetKey( KeyCode.Space ) )
			{
				Invoke<JumpState>();
			}
		}

		protected override void End()
		{
		}
	}

	private class JumpState : State<PlayerObject>
	{
		protected override void Begin()
		{
			Owner.CmdJump();
			Owner.CmdShowSprite( eSprite.Jump );
		}

		protected override void Update()
		{
			if( Input.GetKey( KeyCode.LeftArrow ) )
			{
				Owner.CmdMove( true );
			}
			else if( Input.GetKey( KeyCode.RightArrow ) )
			{
				Owner.CmdMove( false );
			}

			if(!Owner.isJump)
			{
				Invoke<IdleState>();
			}
		}

		protected override void End()
		{
		}
	}
}
