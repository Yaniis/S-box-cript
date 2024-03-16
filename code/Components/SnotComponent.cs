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

	public void DamageAnim( float damage )
	{
		Model?.Set( "hit", true );
	}

	public void DeathAnim()
	{
		Model?.Set( "dead", true );
	}

	protected override void OnUpdate()
	{
	}
}
