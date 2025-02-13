namespace Tickblaze.Scripts.Drawings;

public class VolumeProfileExtended : VolumeProfile
{
	protected override bool ExtendRight => true;

	public VolumeProfileExtended()
	{
		Name = "Volume Profile - Realtime";
	}
}
