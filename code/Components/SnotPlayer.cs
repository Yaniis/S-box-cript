using Sandbox;
using Sandbox.Citizen;

public sealed class SnotPlayer : Component
{

	[Property]
	[Category("Components")]
	public GameObject Camera {  get; set; }

	[Property]
	[Category( "Components" )]
	public CharacterController Controller { get; set; }

	[Property]
	[Category( "Components" )]
	public CitizenAnimationHelper Animator { get; set; }

	/// <summary>
	/// How you can walk speed
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range( 0f, 200f, 1f )]
	public float WalkSpeed { get; set; } = 120f;

	/// <summary>
	/// How you can run  
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range(0f, 400f,1f)]
	public float RunSpeed { get; set; } = 250f;

	/// <summary>
	/// How powerful you can jump 
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range( 0f, 600f, 1f )]
	public float JumpStrength { get; set; } = 400f;
	public int JumpRemained { get; set; } = 2;


	[Property]
	[Category( "Stats" )]
	public float PunchStrength { get; set; } = 1f;

	[Property]
	[Category( "Stats" )]
	public float PunchCooldown { get; set; } = 0.5f;

	[Property]
	[Category( "Stats" )]
	public float PunchRange { get; set; } = 50f;
	TimeSince _lastPunch;

	/// <summary>
	/// Where the camero rotates around and the aim originates from
	/// </summary>
	[Property]
	public Vector3 EyePosition { get; set; }

	public Vector3 EyeWorldPosition => Transform.Local.PointToWorld( EyePosition );

	public Angles EyeAngles { get; set; }
	Transform _initialCameraTransform;


	public class CylinderData
	{
		public Vector3 position;
		public float radius;
		public float duration;

		public CylinderData( Vector3 start, float r, float d )
		{
			position = start;
			radius = r;
			duration = d;
		}
	}

	private List<CylinderData> cylinderDataList = new List<CylinderData>();

	public void AddSphere( Vector3 position,float radius, float duration )
	{
		cylinderDataList.Add( new CylinderData( position, radius, duration ) );
	}

	private void DrawCylinders()
	{
		foreach ( var data in cylinderDataList )
		{
			//Gizmo.Draw.SolidSphere( );
		}
	}

	protected override void DrawGizmos()
	{
		var draw = Gizmo.Draw;

		draw.LineSphere( EyePosition, 10f );
		// Range user
		draw.LineCylinder( EyePosition, EyePosition + Transform.Rotation.Forward * PunchRange, 5f, 5f, 15 );
	}

	protected override void OnUpdate()
	{
		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch( MathX.Clamp(EyeAngles.pitch, -80f, 80f) );
		Transform.Rotation = Rotation.FromYaw( EyeAngles.yaw );

		if ( Camera != null )
		{
			var cameraTransform = _initialCameraTransform.RotateAround( EyePosition, EyeAngles.WithYaw( 0f ) );
			var cameraPosition = Transform.Local.PointToWorld( cameraTransform.Position );
			var cameraTrace = Scene.Trace.Ray( EyeWorldPosition, cameraPosition )
				.Size( 5f )
				.IgnoreGameObjectHierarchy( GameObject )
				.WithoutTags( "player" )
				.Run();

			Camera.Transform.Position = cameraTrace.EndPosition;
			Camera.Transform.LocalRotation = cameraTransform.Rotation;
		}
	}

	public void Punch()
	{
		if( Animator != null ) 
		{
			Animator.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
			Animator.Target.Set( "b_attack", true );
		}

		var punchTrace = Scene.Trace
			.FromTo( EyeWorldPosition, EyeWorldPosition - 10f + EyeAngles.Forward * PunchRange )
			.Size( 15f )
			.WithoutTags( "player" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( punchTrace.Hit )
			if ( punchTrace.GameObject.Components.TryGet<UnitsInfo>( out var unitInfo ) )
			{
				unitInfo.Damage( PunchStrength );
			}

		_lastPunch = 0f;

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Controller == null ) return;

		var wishSpeed = Input.Down("Run") ? RunSpeed : WalkSpeed;
		var wishVelocity = Input.AnalogMove.Normal * wishSpeed * Transform.Rotation;

		Controller.Accelerate( wishVelocity );

		// Jump event
		if ( Input.Pressed( "Jump" ) && JumpRemained != 0 )
		{

			Controller.IsOnGround = false;
			Controller.Velocity = Vector3.Up * JumpStrength;

			JumpRemained--;

			if ( Animator != null )
				Animator.TriggerJump();
		}

		if ( Controller.IsOnGround )
		{
			JumpRemained = 2;

			Controller.Acceleration = 10f;
			Controller.ApplyFriction( 5f );
		}
		else
		{		
			Controller.Acceleration = 2f;
			// Apply gravity when in the air
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}

		Controller.Move();

		if ( Animator != null )
		{
			Animator.IsGrounded = Controller.IsOnGround;
			Animator.WithVelocity( Controller.Velocity );

			if( _lastPunch >= 2f )
			{
				Animator.HoldType = CitizenAnimationHelper.HoldTypes.None;
			}

		}

		if ( Input.Pressed( "Punch" ) && _lastPunch >= PunchCooldown )
			Punch();
		

	}

	protected override void OnStart()
	{

		if ( Camera != null )
		{
			_initialCameraTransform = Camera.Transform.Local;
		}

		if ( Components.TryGet<SkinnedModelRenderer>( out var skinnedModelRenderer ) )
		{
			var clothing = ClothingContainer.CreateFromLocalUser();
			clothing.Apply( skinnedModelRenderer );
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
	}

}
