
namespace SmidtFramework.ControllerSystem
{
	/// <summary>
	/// Common class for all UI Controllers. This is the main interaction point between the ControllerMaster and all other UI Controllers.
	/// </summary>
	public abstract class BaseUIController : BaseController
	{
		/// <summary>
		/// Gets called when the UI GameObject is enabled after being disabled. Also gets called on Controller creation after Init.
		/// </summary>
		public abstract void OnEnable();

		/// <summary>
		/// Gets called when the UI GameObject is disabled after being enabled. Also gets called before OnDelete if the UI was active during delete call.
		/// </summary>
		public abstract void OnDisable();
	}

}

