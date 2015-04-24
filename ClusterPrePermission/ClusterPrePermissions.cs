using System;
using AddressBook;
using AssetsLibrary;
using AVFoundation;
using CoreLocation;
using EventKit;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace ClusterPrePermissions
{
	#region enums

	public enum ClusterDialogResult
	{
		/// User was not given the chance to take action.
		/// This can happen if the permission was
		/// already granted, denied, or restricted.
		NoActionTaken,
		/// User declined access in the user dialog or system dialog.
		Denied,
		/// User granted access in the user dialog or system dialog.
		Granted,
		/// The iOS parental permissions prevented access.
		/// This outcome would only happen on the system dialog.
		ParentallyRestricted
	}

	public enum ClusterAuthorizationStatus
	{
		/// Permission status undetermined.
		UnDetermined,
		/// Permission denied.
		Denied,
		/// Permission authorized.
		Authorized,
		/// The iOS parental permissions prevented access.
		Restricted
	}

	public enum ClusterLocationAuthorizationType
	{
		/// the “when-in-use” authorization grants the app to start most
		/// (but not all) location services while it is in the foreground.
		WhenInUse,
		/// the “always” authorization grants the app to start all
		/// location services
		Always,
	}

	public enum ClusterEventAuthorizationType
	{
		/// Authorization for events only
		Event,
		/// Authorization for reminders only
		Reminder
	}

	public enum ClusterAVAuthorizationType
	{
		/// Authorization for Camera only
		Camera,
		/// Authorization for Microphone only
		Microphone
	}

	enum ClusterTitleType
	{
		Request,
		Deny
	}

	#endregion

	public class ClusterPrePermissions : NSObject
	{
		public delegate void ClusterPrePermissionCompletionHandler (bool hasPermission, ClusterDialogResult userDialogResult, ClusterDialogResult systemDialogResult);

		#region Private Members

		UIAlertView _preAVPermissionAlertView;
		ClusterPrePermissionCompletionHandler _avPermissionCompletionHandler;

		UIAlertView _prePhotoPermissionAlertView;
		ClusterPrePermissionCompletionHandler _photoPermissionCompletionHandler;

		UIAlertView _preContactPermissionAlertView;
		ClusterPrePermissionCompletionHandler _contactPermissionCompletionHandler;

		UIAlertView _preEventPermissionAlertView;
		ClusterPrePermissionCompletionHandler _eventPermissionCompletionHandler;

		UIAlertView _preLocationPermissionAlertView;
		ClusterPrePermissionCompletionHandler _locationPermissionCompletionHandler;
		CLLocationManager _locationManager;

		ClusterLocationAuthorizationType _locationAuthorizationType;

		static ClusterPrePermissions _sharedInstance;

		#endregion

		public static ClusterPrePermissions SharedPermissions
		{
			get
			{
				if (_sharedInstance == null)
				{
					_sharedInstance = new ClusterPrePermissions ();
				}
				return _sharedInstance;
			}
		}

		ClusterAuthorizationStatus AVPermissionAuthorizationStatusForMediaType (NSString mediaType)
		{
			var status = AVCaptureDevice.GetAuthorizationStatus (mediaType);
			switch (status)
			{
				case AVAuthorizationStatus.Authorized:
					return ClusterAuthorizationStatus.Authorized;

				case AVAuthorizationStatus.Denied:
					return ClusterAuthorizationStatus.Denied;

				case AVAuthorizationStatus.Restricted:
					return ClusterAuthorizationStatus.Restricted;

				default:
					return ClusterAuthorizationStatus.UnDetermined;
			}
		}

		public ClusterAuthorizationStatus CameraPermissionAuthorizationStatus ()
		{
			return AVPermissionAuthorizationStatusForMediaType (AVMediaType.Video);
		}

		public ClusterAuthorizationStatus MicrophonePermissionAuthorizationStatus ()
		{
			return AVPermissionAuthorizationStatusForMediaType (AVMediaType.Audio);
		}

		public ClusterAuthorizationStatus PhotoPermissionAuthorizationStatus ()
		{
			var status = ALAssetsLibrary.AuthorizationStatus;
			switch (status)
			{
				case ALAuthorizationStatus.Authorized:
					return ClusterAuthorizationStatus.Authorized;

				case ALAuthorizationStatus.Denied:
					return ClusterAuthorizationStatus.Denied;

				case ALAuthorizationStatus.Restricted:
					return ClusterAuthorizationStatus.Restricted;

				default:
					return ClusterAuthorizationStatus.UnDetermined;
			}
		}

		public ClusterAuthorizationStatus ContactsPermissionAuthorizationStatus ()
		{
			var status = ABAddressBook.GetAuthorizationStatus ();
			switch (status)
			{
				case ABAuthorizationStatus.Authorized:
					return ClusterAuthorizationStatus.Authorized;

				case ABAuthorizationStatus.Denied:
					return ClusterAuthorizationStatus.Denied;

				case ABAuthorizationStatus.Restricted:
					return ClusterAuthorizationStatus.Restricted;

				default:
					return ClusterAuthorizationStatus.UnDetermined;
			}
		}

		public ClusterAuthorizationStatus EventPermissionAuthorizationStatus (ClusterEventAuthorizationType eventType)
		{
			var status = EKEventStore.GetAuthorizationStatus (ClusterPrePermissions.SharedPermissions.EKEquivalentEventType (eventType));
			switch (status)
			{
				case EKAuthorizationStatus.Authorized:
					return ClusterAuthorizationStatus.Authorized;

				case EKAuthorizationStatus.Denied:
					return ClusterAuthorizationStatus.Denied;

				case EKAuthorizationStatus.Restricted:
					return ClusterAuthorizationStatus.Restricted;

				default:
					return ClusterAuthorizationStatus.UnDetermined;
			}
		}

		public ClusterAuthorizationStatus LocationPermissionAuthorizationStatus ()
		{
			var status = CLLocationManager.Status;
			switch (status)
			{
				case CLAuthorizationStatus.AuthorizedAlways:
				case CLAuthorizationStatus.AuthorizedWhenInUse:
					return ClusterAuthorizationStatus.Authorized;

				case CLAuthorizationStatus.Denied:
					return ClusterAuthorizationStatus.Denied;

				case CLAuthorizationStatus.Restricted:
					return ClusterAuthorizationStatus.Restricted;

				default:
					return ClusterAuthorizationStatus.UnDetermined;
			}
		}


		#region AV Permissions Help

		public void ShowAVPermissionsWithType (ClusterAVAuthorizationType mediaType, string requestTitle, string message, string denyButtonTitle, string grantButtonTitle, ClusterPrePermissionCompletionHandler completionHandler)
		{
			if (requestTitle.Length == 0)
			{
				switch (mediaType)
				{
					case ClusterAVAuthorizationType.Camera:
						requestTitle = @"Access Camera?";
						break;

					default:
						requestTitle = @"Access Microphone?";
						break;
				}
			}
			denyButtonTitle = GetTitleForType (ClusterTitleType.Deny, denyButtonTitle);
			grantButtonTitle = GetTitleForType (ClusterTitleType.Request, grantButtonTitle);

			AVAuthorizationStatus status = AVCaptureDevice.GetAuthorizationStatus (AVMediaType.Video);
			if (status == AVAuthorizationStatus.NotDetermined)
			{
				_avPermissionCompletionHandler = completionHandler;
				_preAVPermissionAlertView = new UIAlertView (requestTitle, message, new AlertViewDelegate (this), denyButtonTitle, grantButtonTitle);
				_preAVPermissionAlertView.Tag = (nint)(int)mediaType;
				_preAVPermissionAlertView.Show ();
			}
			else
			{
				if (completionHandler != null)
				{
					completionHandler ((status == AVAuthorizationStatus.Authorized),
						ClusterDialogResult.NoActionTaken,
						ClusterDialogResult.NoActionTaken);
				}
			}
		}

		public void ShowCameraPermissionsWithTitle (string requestTitle, string message, string denyButtonTitle, string grantButtonTitle, ClusterPrePermissionCompletionHandler completionHandler)
		{
			ShowAVPermissionsWithType (ClusterAVAuthorizationType.Camera, requestTitle, message, denyButtonTitle, grantButtonTitle, completionHandler);
		}

		public void ShowMicrophonePermissionsWithTitle (string requestTitle, string message, string denyButtonTitle, string grantButtonTitle, ClusterPrePermissionCompletionHandler completionHandler)
		{
			ShowAVPermissionsWithType (ClusterAVAuthorizationType.Microphone, requestTitle, message, denyButtonTitle, grantButtonTitle, completionHandler);
		}

		void ShowActualAVPermissionAlertWithType (ClusterAVAuthorizationType mediaType)
		{
			AVCaptureDevice.RequestAccessForMediaType (AVEquivalentMediaType (mediaType), granted => BeginInvokeOnMainThread (delegate
			{
				FireAVPermissionCompletionHandlerWithType (mediaType);
			}));
		}

		void FireAVPermissionCompletionHandlerWithType (ClusterAVAuthorizationType mediaType)
		{
			AVAuthorizationStatus status = AVCaptureDevice.GetAuthorizationStatus (AVEquivalentMediaType (mediaType));
			if (_avPermissionCompletionHandler != null)
			{
				ClusterDialogResult userDialogResult = ClusterDialogResult.Granted;
				ClusterDialogResult systemDialogResult = ClusterDialogResult.Granted;
				if (status == AVAuthorizationStatus.NotDetermined)
				{
					userDialogResult = ClusterDialogResult.Denied;
					systemDialogResult = ClusterDialogResult.NoActionTaken;
				}
				else if (status == AVAuthorizationStatus.Authorized)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.Granted;
				}
				else if (status == AVAuthorizationStatus.Denied)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.Denied;
				}
				else if (status == AVAuthorizationStatus.Restricted)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.ParentallyRestricted;
				}
				_avPermissionCompletionHandler ((status == AVAuthorizationStatus.Authorized),
					userDialogResult,
					systemDialogResult);
				_avPermissionCompletionHandler = null;
			}
		}

		NSString AVEquivalentMediaType (ClusterAVAuthorizationType mediaType)
		{
			if (mediaType == ClusterAVAuthorizationType.Camera)
			{
				return AVMediaType.Video;
			}
			else
			{
				return AVMediaType.Audio;
			}
		}

		#endregion

		#region Photo Permissions Help

		public void ShowPhotoPermissionsWithTitle (string requestTitle, string message, string denyButtonTitle, string grantButtonTitle, ClusterPrePermissionCompletionHandler completionHandler)
		{
			if (requestTitle.Length == 0)
			{
				requestTitle = @"Access Photos?";
			}
			denyButtonTitle = GetTitleForType (ClusterTitleType.Deny, denyButtonTitle);
			grantButtonTitle = GetTitleForType (ClusterTitleType.Request, grantButtonTitle);

			ALAuthorizationStatus status = ALAssetsLibrary.AuthorizationStatus;
			if (status == ALAuthorizationStatus.NotDetermined)
			{
				_photoPermissionCompletionHandler = completionHandler;
				_prePhotoPermissionAlertView = new UIAlertView (requestTitle, message, new AlertViewDelegate (this), denyButtonTitle, grantButtonTitle);
				_prePhotoPermissionAlertView.Show ();
			}
			else
			{
				if (completionHandler != null)
				{
					completionHandler ((status == ALAuthorizationStatus.Authorized),
						ClusterDialogResult.NoActionTaken,
						ClusterDialogResult.NoActionTaken);
				}
			}
		}

		void ShowActualPhotoPermissionAlert ()
		{
			ALAssetsLibrary library = new ALAssetsLibrary ();
			library.Enumerate (ALAssetsGroupType.SavedPhotos, HandleALAssetsLibraryGroupsEnumerationResults, delegate (NSError obj)
			{
				FirePhotoPermissionCompletionHandler ();
			});
		}

		void HandleALAssetsLibraryGroupsEnumerationResults (ALAssetsGroup group, ref bool stop)
		{
			FirePhotoPermissionCompletionHandler ();
			stop = true;
		}

		void FirePhotoPermissionCompletionHandler ()
		{
			ALAuthorizationStatus status = ALAssetsLibrary.AuthorizationStatus;
			if (_photoPermissionCompletionHandler != null)
			{
				ClusterDialogResult userDialogResult = ClusterDialogResult.Granted;
				ClusterDialogResult systemDialogResult = ClusterDialogResult.Granted;
				if (status == ALAuthorizationStatus.NotDetermined)
				{
					userDialogResult = ClusterDialogResult.Denied;
					systemDialogResult = ClusterDialogResult.NoActionTaken;
				}
				else if (status == ALAuthorizationStatus.Authorized)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.Granted;
				}
				else if (status == ALAuthorizationStatus.Denied)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.Denied;
				}
				else if (status == ALAuthorizationStatus.Restricted)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.ParentallyRestricted;
				}
				_photoPermissionCompletionHandler ((status == ALAuthorizationStatus.Authorized),
					userDialogResult,
					systemDialogResult);
				_photoPermissionCompletionHandler = null;
			}
		}

		#endregion

		#region Contact Permissions Help

		public void ShowContactsPermissionsWithTitle (string requestTitle, string message, string denyButtonTitle, string grantButtonTitle, ClusterPrePermissionCompletionHandler completionHandler)
		{
			if (requestTitle.Length == 0)
			{
				requestTitle = @"Access Contacts?";
			}
			denyButtonTitle = GetTitleForType (ClusterTitleType.Deny, denyButtonTitle);
			grantButtonTitle = GetTitleForType (ClusterTitleType.Request, grantButtonTitle);

			ABAuthorizationStatus status = ABAddressBook.GetAuthorizationStatus ();
			if (status == ABAuthorizationStatus.NotDetermined)
			{
				_contactPermissionCompletionHandler = completionHandler;
				_preContactPermissionAlertView = new UIAlertView (requestTitle, message, new AlertViewDelegate (this), denyButtonTitle, grantButtonTitle);
				_preContactPermissionAlertView.Show ();
			}
			else
			{
				if (completionHandler != null)
				{
					completionHandler ((status == ABAuthorizationStatus.Authorized),
						ClusterDialogResult.NoActionTaken,
						ClusterDialogResult.NoActionTaken);
				}
			}
		}

		void ShowActualContactPermissionAlert ()
		{
			NSError error;
			var addressBook = ABAddressBook.Create (out error);
			addressBook.RequestAccess (delegate
			{
				BeginInvokeOnMainThread (delegate
				{
					FireContactPermissionCompletionHandler ();
				});
			});
		}

		void FireContactPermissionCompletionHandler ()
		{
			ABAuthorizationStatus status = ABAddressBook.GetAuthorizationStatus ();
			if (_contactPermissionCompletionHandler != null)
			{
				ClusterDialogResult userDialogResult = ClusterDialogResult.Granted;
				ClusterDialogResult systemDialogResult = ClusterDialogResult.Granted;
				if (status == ABAuthorizationStatus.NotDetermined)
				{
					userDialogResult = ClusterDialogResult.Denied;
					systemDialogResult = ClusterDialogResult.NoActionTaken;
				}
				else if (status == ABAuthorizationStatus.Authorized)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.Granted;
				}
				else if (status == ABAuthorizationStatus.Denied)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.Denied;
				}
				else if (status == ABAuthorizationStatus.Restricted)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.ParentallyRestricted;
				}
				_contactPermissionCompletionHandler ((status == ABAuthorizationStatus.Authorized),
					userDialogResult,
					systemDialogResult);
				_contactPermissionCompletionHandler = null;
			}
		}

		#endregion

		#region Event Permissions Help

		public void ShowEventPermissionsWithType (ClusterEventAuthorizationType eventType, string requestTitle, string message, string denyButtonTitle, string grantButtonTitle, ClusterPrePermissionCompletionHandler completionHandler)
		{
			if (requestTitle.Length == 0)
			{
				switch (eventType)
				{
					case ClusterEventAuthorizationType.Event:
						requestTitle = @"Access Calendar?";
						break;

					default:
						requestTitle = @"Access Reminders?";
						break;
				}
			}
			denyButtonTitle = GetTitleForType (ClusterTitleType.Deny, denyButtonTitle);
			grantButtonTitle = GetTitleForType (ClusterTitleType.Request, grantButtonTitle);

			var status = EKEventStore.GetAuthorizationStatus (EKEquivalentEventType (eventType));
			if (status == EKAuthorizationStatus.NotDetermined)
			{
				_eventPermissionCompletionHandler = completionHandler;
				_preEventPermissionAlertView = new UIAlertView (requestTitle, message, new AlertViewDelegate (this), denyButtonTitle, grantButtonTitle);
				_preEventPermissionAlertView.Tag = (nint)(int)eventType;
				_preEventPermissionAlertView.Show ();
			}
			else
			{
				if (completionHandler != null)
				{
					completionHandler ((status == EKAuthorizationStatus.Authorized),
						ClusterDialogResult.NoActionTaken,
						ClusterDialogResult.NoActionTaken);
				}
			}
		}

		void ShowActualEventPermissionAlert (ClusterEventAuthorizationType eventType)
		{
			EKEventStore aStore = new EKEventStore ();
			aStore.RequestAccess (EKEquivalentEventType (eventType), delegate
			{
				
				FireEventPermissionCompletionHandler (eventType);
			});
		}

		void FireEventPermissionCompletionHandler (ClusterEventAuthorizationType eventType)
		{
			EKAuthorizationStatus status = EKEventStore.GetAuthorizationStatus (EKEquivalentEventType (eventType));
			if (_eventPermissionCompletionHandler != null)
			{
				ClusterDialogResult userDialogResult = ClusterDialogResult.Granted;
				ClusterDialogResult systemDialogResult = ClusterDialogResult.Granted;
				if (status == EKAuthorizationStatus.NotDetermined)
				{
					userDialogResult = ClusterDialogResult.Denied;
					systemDialogResult = ClusterDialogResult.NoActionTaken;
				}
				else if (status == EKAuthorizationStatus.Authorized)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.Granted;
				}
				else if (status == EKAuthorizationStatus.Denied)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.Denied;
				}
				else if (status == EKAuthorizationStatus.Restricted)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.ParentallyRestricted;
				}
				_eventPermissionCompletionHandler ((status == EKAuthorizationStatus.Authorized),
					userDialogResult,
					systemDialogResult);
				_eventPermissionCompletionHandler = null;
			}
		}

		EKEntityType EKEquivalentEventType (ClusterEventAuthorizationType eventType)
		{
			if (eventType == ClusterEventAuthorizationType.Event)
			{
				return EKEntityType.Event;
			}
			else
			{
				return EKEntityType.Reminder;
			}
		}

		#endregion

		#region Location Permission Help

		public void ShowLocationPermissionsWithTitle (string requestTitle, string message, string denyButtonTitle, string grantButtonTitle, ClusterPrePermissionCompletionHandler completionHandler)
		{
			ShowLocationPermissionsForAuthorizationType (ClusterLocationAuthorizationType.Always, requestTitle, message, denyButtonTitle, grantButtonTitle, completionHandler);
		}

		public void ShowLocationPermissionsForAuthorizationType (ClusterLocationAuthorizationType authorizationType, string requestTitle, string message, string denyButtonTitle, string grantButtonTitle, ClusterPrePermissionCompletionHandler completionHandler)
		{
			if (requestTitle.Length == 0)
			{
				requestTitle = @"Access Location?";
			}
			denyButtonTitle = GetTitleForType (ClusterTitleType.Deny, denyButtonTitle);
			grantButtonTitle = GetTitleForType (ClusterTitleType.Request, grantButtonTitle);

			var status = CLLocationManager.Status;
			if (status == CLAuthorizationStatus.NotDetermined)
			{
				_locationPermissionCompletionHandler = completionHandler;
				_locationAuthorizationType = authorizationType;
				_preLocationPermissionAlertView = new UIAlertView (requestTitle, message, new AlertViewDelegate (this), denyButtonTitle, grantButtonTitle);
				_preLocationPermissionAlertView.Show ();
			}
			else
			{
				if (completionHandler != null)
				{
					completionHandler (LocationAuthorizationStatusPermitsAccess (status),
						ClusterDialogResult.NoActionTaken,
						ClusterDialogResult.NoActionTaken);
				}
			}
		}

		void ShowActualLocationPermissionAlert ()
		{
			_locationManager = new CLLocationManager ();
			_locationManager.Delegate = new LocationManagerDelegate (this);

			if (_locationAuthorizationType == ClusterLocationAuthorizationType.Always &&
			    _locationManager.RespondsToSelector (new Selector ("requestAlwaysAuthorization")))
			{
				_locationManager.RequestAlwaysAuthorization ();
			}
			else if (_locationAuthorizationType == ClusterLocationAuthorizationType.WhenInUse &&
			         _locationManager.RespondsToSelector (new Selector ("requestWhenInUseAuthorization")))
			{
				_locationManager.RequestWhenInUseAuthorization ();
			}

			_locationManager.StartUpdatingLocation ();
		}

		void FireLocationPermissionCompletionHandler ()
		{
			var status = CLLocationManager.Status;
			if (_locationPermissionCompletionHandler != null)
			{
				ClusterDialogResult userDialogResult = ClusterDialogResult.Granted;
				ClusterDialogResult systemDialogResult = ClusterDialogResult.Granted;
				if (status == CLAuthorizationStatus.NotDetermined)
				{
					userDialogResult = ClusterDialogResult.Denied;
					systemDialogResult = ClusterDialogResult.NoActionTaken;
				}
				else if (LocationAuthorizationStatusPermitsAccess (status))
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.Granted;
				}
				else if (status == CLAuthorizationStatus.Denied)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.Denied;
				}
				else if (status == CLAuthorizationStatus.Restricted)
				{
					userDialogResult = ClusterDialogResult.Granted;
					systemDialogResult = ClusterDialogResult.ParentallyRestricted;
				}
				_locationPermissionCompletionHandler ((LocationAuthorizationStatusPermitsAccess (status)),
					userDialogResult,
					systemDialogResult);
				_locationPermissionCompletionHandler = null;
			}
			if (_locationManager != null)
			{
				_locationManager.StopUpdatingLocation ();
				_locationManager = null;
			}
		}

		bool LocationAuthorizationStatusPermitsAccess (CLAuthorizationStatus authorizationStatus)
		{
			return authorizationStatus == CLAuthorizationStatus.Authorized ||
			authorizationStatus == CLAuthorizationStatus.AuthorizedAlways ||
			authorizationStatus == CLAuthorizationStatus.AuthorizedWhenInUse;
		}

		#endregion

		#region Titles

		string GetTitleForType (ClusterTitleType titleType, string title)
		{
			switch (titleType)
			{
				case ClusterTitleType.Deny:
					title = (title.Length == 0) ? @"Not Now" : title;
					break;
				case ClusterTitleType.Request:
					title = (title.Length == 0) ? @"Give Access" : title;
					break;
				default:
					title = @"";
					break;
			}
			return title;
		}

		#endregion

		class LocationManagerDelegate : CLLocationManagerDelegate
		{
			readonly ClusterPrePermissions _parent;

			public LocationManagerDelegate (ClusterPrePermissions parent)
			{
				_parent = parent;
			}

			public override void AuthorizationChanged (CLLocationManager manager, CLAuthorizationStatus status)
			{
				if (status != CLAuthorizationStatus.NotDetermined)
				{
					_parent.FireLocationPermissionCompletionHandler ();
				}
			}
		}

		class AlertViewDelegate : UIAlertViewDelegate
		{
			readonly ClusterPrePermissions _parent;

			public AlertViewDelegate (ClusterPrePermissions parent)
			{
				_parent = parent;
			}

			public override void Clicked (UIAlertView alertview, nint buttonIndex)
			{
				if (alertview == _parent._preAVPermissionAlertView)
				{
					if (buttonIndex == alertview.CancelButtonIndex)
					{
						// User said NO, jerk.
						_parent.FireAVPermissionCompletionHandlerWithType ((ClusterAVAuthorizationType)(int)alertview.Tag);
					}
					else
					{
						// User granted access, now show the REAL permissions dialog
						_parent.ShowActualAVPermissionAlertWithType ((ClusterAVAuthorizationType)(int)alertview.Tag);
					}

					_parent._preAVPermissionAlertView = null;
				}
				else if (alertview == _parent._prePhotoPermissionAlertView)
				{
					if (buttonIndex == alertview.CancelButtonIndex)
					{
						// User said NO, jerk.
						_parent.FirePhotoPermissionCompletionHandler();
					}
					else
					{
						// User granted access, now show the REAL permissions dialog
						_parent.ShowActualPhotoPermissionAlert();
					}

					_parent._prePhotoPermissionAlertView = null;
				}
				else if (alertview == _parent._preContactPermissionAlertView)
				{
					if (buttonIndex == alertview.CancelButtonIndex)
					{
						// User said NO, that jerk.
						_parent.FireContactPermissionCompletionHandler();
					}
					else
					{
						// User granted access, now try to trigger the real contacts access
						_parent.ShowActualContactPermissionAlert();
					}
				}
				else if (alertview == _parent._preEventPermissionAlertView)
				{
					if (buttonIndex == alertview.CancelButtonIndex)
					{
						// User said NO, that jerk.
						_parent.FireEventPermissionCompletionHandler ((ClusterEventAuthorizationType)((int)alertview.Tag));
					}
					else
					{
						// User granted access, now try to trigger the real contacts access
						_parent.ShowActualEventPermissionAlert ((ClusterEventAuthorizationType)((int)alertview.Tag));
					}
				}
				else if (alertview == _parent._preLocationPermissionAlertView)
				{
					if (buttonIndex == alertview.CancelButtonIndex)
					{
						// User said NO, that jerk.
						_parent.FireLocationPermissionCompletionHandler();
					}
					else
					{
						// User granted access, now try to trigger the real location access
						_parent.ShowActualLocationPermissionAlert();
					}
				}

			}
		}

	}
}

