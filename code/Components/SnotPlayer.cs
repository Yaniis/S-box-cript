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

	public Angles EyeAngles { get; set; }

	protected override void OnUpdate()
	{
		EyeAngles += Input.AnalogLook;
		Transform.Rotation = Rotation.FromYaw( EyeAngles.yaw );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Controller == null ) return;

		var wishSpeed = Input.Down("Run") ? RunSpeed : WalkSpeed;
		var wishVelocity = Input.AnalogMove * wishSpeed * Transform.Rotation;

		Controller.Accelerate( wishVelocity );

		if ( Controller.IsOnGround )
			Controller.ApplyFriction( 5f );
		else
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;

		Controller.Move();
	}

	protected override void OnStart()
	{
		base.OnStart();
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
