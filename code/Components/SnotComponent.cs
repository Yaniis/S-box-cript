using Sandbox;

public sealed class SnotComponent : Component
{

	[Property]
	public UnitsInfo Info { get; set; }

	[Property]
	public SkinnedModelRenderer Model { get; set; }



	protected override void OnStart()
	{
		if ( Info != null )
		{
			Info.OnDamage += DamageAnim;
			Info.OnDeath += DeathAnim;
		}
	}

	protected override void OnUpdate()
	{
		if ( Model == null ) return;
		if ( Info == null ) return;
		if ( !Info.Alive ) return;

		// Lerp health for transition to different state of life
		var currentHealth = Model.GetFloat( "health" );
		var scaledHealth = Info.Health / Info.MaxHealth * 100;
		var lerpedHealth = MathX.Lerp( currentHealth, scaledHealth, Time.Delta / 0.1f );

		Model.Set( "health", lerpedHealth );
	}

	public void DamageAnim( float damage )
	{
		Model?.Set( "hit", true );
		Model?.Set( "damage", damage );
	}

	public void DeathAnim()
	{
		Model?.Set( "dead", true );
	}

}
