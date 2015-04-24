// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ClusterPrePermission
{
	[Register ("PermissionsTestViewController")]
	partial class PermissionsTestViewController
	{
		[Outlet]
		UIKit.UILabel contactsPermissionResultLabel { get; set; }

		[Outlet]
		UIKit.UILabel locationPermissionResultLabel { get; set; }

		[Outlet]
		UIKit.UILabel photoPermissionResultLabel { get; set; }

		[Action ("onContactsButtonPermissionTapped:")]
		partial void onContactsButtonPermissionTapped (Foundation.NSObject sender);

		[Action ("onLocationButtonPermissionTapped:")]
		partial void onLocationButtonPermissionTapped (Foundation.NSObject sender);

		[Action ("onPhotoPermissionsButtonTapped:")]
		partial void onPhotoPermissionsButtonTapped (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (photoPermissionResultLabel != null) {
				photoPermissionResultLabel.Dispose ();
				photoPermissionResultLabel = null;
			}

			if (contactsPermissionResultLabel != null) {
				contactsPermissionResultLabel.Dispose ();
				contactsPermissionResultLabel = null;
			}

			if (locationPermissionResultLabel != null) {
				locationPermissionResultLabel.Dispose ();
				locationPermissionResultLabel = null;
			}
		}
	}
}
