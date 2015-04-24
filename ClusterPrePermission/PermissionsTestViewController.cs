
using System;

using Foundation;
using UIKit;

namespace ClusterPrePermission
{
	public partial class PermissionsTestViewController : UIViewController
	{
		public PermissionsTestViewController () : base ("PermissionsTestViewController", null)
		{
		}

		partial void onPhotoPermissionsButtonTapped (NSObject sender)
		{
			
		}

		partial void onContactsButtonPermissionTapped (NSObject sender)
		{
			
		}

		partial void onLocationButtonPermissionTapped (NSObject sender)
		{
			
		}

		protected override void Dispose (bool disposing)
		{
			ReleaseDesignerOutlets ();
			base.Dispose (disposing);
		}
	}
}

