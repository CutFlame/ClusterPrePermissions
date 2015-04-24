
using System;

using Foundation;
using UIKit;

namespace ClusterPrePermissions
{
	public partial class PermissionsTestViewController : UIViewController
	{
		public PermissionsTestViewController () : base ("PermissionsTestViewController", null)
		{
		}

		partial void onPhotoPermissionsButtonTapped (NSObject sender)
		{
			var permissions = ClusterPrePermissions.SharedPermissions;
			permissions.ShowPhotoPermissionsWithTitle ("Access your photos?",
				"Your message here",
				"Not Now",
				"Give Access",
				(hasPermission, userDialogResult, systemDialogResult) =>
				UpdateResultLabel (photoPermissionResultLabel, hasPermission, userDialogResult, systemDialogResult));
		}

		partial void onContactsButtonPermissionTapped (NSObject sender)
		{
			var permissions = ClusterPrePermissions.SharedPermissions;
			permissions.ShowContactsPermissionsWithTitle ("Access your contacts?",
				"Your message here",
				"Not Now",
				"Give Access",
				(hasPermission, userDialogResult, systemDialogResult) =>
				UpdateResultLabel (contactsPermissionResultLabel, hasPermission, userDialogResult, systemDialogResult));
		}

		partial void onLocationButtonPermissionTapped (NSObject sender)
		{
			var permissions = ClusterPrePermissions.SharedPermissions;
			permissions.ShowLocationPermissionsWithTitle ("Access your location?",
				"Your message here",
				"Not Now",
				"Give Access",
				(hasPermission, userDialogResult, systemDialogResult) =>
				UpdateResultLabel (locationPermissionResultLabel, hasPermission, userDialogResult, systemDialogResult));
		}


		void UpdateResultLabel (UILabel resultLabel, bool hasPermission, ClusterDialogResult userDialogResult, ClusterDialogResult systemDialogResult)
		{
			resultLabel.Text = @"haha";
			resultLabel.Alpha = 0.0f;

			if (hasPermission)
			{
				resultLabel.TextColor = UIColor.FromRGBA (0.1f, 1.0f, 0.1f, 1.0f);
			}
			else
			{
				resultLabel.TextColor = UIColor.FromRGBA (0.1f, 0.1f, 0.1f, 1.0f);
			}
			string text = null;
			if (userDialogResult == ClusterDialogResult.NoActionTaken &&
			    systemDialogResult == ClusterDialogResult.NoActionTaken)
			{
				string prefix = null;
				if (hasPermission)
				{
					prefix = @"Granted.";
				}
				else if (systemDialogResult == ClusterDialogResult.ParentallyRestricted)
				{
					prefix = @"Restricted.";
				}
				else
				{
					prefix = @"Denied.";
				}
				text = string.Format (@"{0} Dialogs not shown, system choice already made.", prefix);
			}
			else
			{
				string userResultString = StringFromDialogResult (userDialogResult);
				string systemResultString = StringFromDialogResult (systemDialogResult);
				text = string.Format (@"User Action: {0}\nSystem Action: {1}", userResultString, systemResultString);
			}
			resultLabel.Text = text;

			UIView.Animate (0.35, delegate
			{
				resultLabel.Alpha = 1.0f;
			});
		}

		string StringFromDialogResult (ClusterDialogResult result)
		{
			switch (result)
			{
				case ClusterDialogResult.NoActionTaken:
					return @"No Action Taken";
				case ClusterDialogResult.Granted:
					return @"Granted";
				case ClusterDialogResult.Denied:
					return @"Denied";
				case ClusterDialogResult.ParentallyRestricted:
					return @"Restricted";
				default:
					return string.Empty;
			}
		}


		protected override void Dispose (bool disposing)
		{
			ReleaseDesignerOutlets ();
			base.Dispose (disposing);
		}
	}
}

