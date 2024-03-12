using Sandbox.Citizen;
using Sandbox.VR;

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

	/// <summary>
	/// Where the camero rotates around and the aim originates from
	/// </summary>
	[Property]
	public Vector3 EyePosition { get; set; }

	public Angles EyeAngles { get; set; }
	Transform _initialCameraTransform;

	protected override void DrawGizmos()
	{
		Gizmo.Draw.LineSphere( EyePosition, 10f );
	}

	protected override void OnUpdate()
	{
		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch( 0f );
		Transform.Rotation = Rotation.FromYaw( EyeAngles.yaw );

		if ( Camera != null )
			Camera.Transform.Local = _initialCameraTransform.RotateAround(EyePosition, EyeAngles.WithYaw(0f));
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

			if (JumpRemained == 2){

				Controller.IsOnGround = false;
				Controller.Velocity = Controller.Velocity.WithZ( JumpStrength );
				
			} else if (JumpRemained == 1 )
			{
				Log.Info( "" );
				Controller.Velocity = Vector3.Up * JumpStrength;
			}

			JumpRemained--;
			//Controller.Punch( Vector3.Up * JumpStrength );


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
			Controller.Acceleration = 5f;
			// Apply gravity when in the air
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}

		Controller.Move();

		if ( Animator != null )
		{
			Animator.IsGrounded = Controller.IsOnGround;
			Animator.WithVelocity( Controller.Velocity );
		}
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
