using Sandbox;

public enum UnitType
{
	[Icon( "block" )]
	None,
	[Icon( "emoji_people" )]
	Player,
	[Icon( "warning" )]
	Snot
}

public sealed class UnitsInfo : Component
{
	[Property]
	public UnitType Team { get; set; }
	[Property]
	public float MaxHealth { get; set; } = 5f;
	
	public float Health { get; private set; }

	public bool Alive { get; set; } = true;

	[Property]
	[Range( 1f, 5f, 1f )]
	public float HealthRegenAmount { get; set; } = 0.5f;

	[Property]
	[Range( 1f, 5f, 1f )]
	public float HealthRegenTimer { get; set; } = 3f;
	TimeSince _LastDamage;
	TimeUntil _NextHealth;

	protected override void OnStart()
	{
		Health = MaxHealth;
	}

	protected override void OnUpdate()
	{
		if (_LastDamage >= HealthRegenTimer && Health != MaxHealth && Alive)
		{
			if ( _NextHealth )
			{
				Damage( -HealthRegenAmount );
				_NextHealth = 1f;
			}
		}
		
	}

	public void Damage ( float damage )
	{
		if ( !Alive ) return;

		Health = MathX.Clamp( Health - damage, 0f, MaxHealth );

		if ( damage > 0 )
			_LastDamage = 0f;

		if ( Health <= 0 )
			Kill();
	}

	public void Kill () 
	{

		Health = 0;
		Alive = false;

		GameObject.Destroy();
	}

}
