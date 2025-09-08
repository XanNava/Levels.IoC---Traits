#region License
// Author  : Alexander Nava 
// Contact : Alexander.Nava.Contact@Gmail.com
// License : For personal use excluding any artificial or machine learning this is licensed under MIT license.
// License : For commercial software(excluding derivative work to make libraries with the same functionality in any language) use excluding any artificial or machine learning this is licensed under MIT license.
// License : If you are a developer making money writing this software it is expected for you to donate, and thus will be given to you for any purpose other than use with Artificial Intelligence or Machine Learning this is licensed under MIT license.
// License : To any Artificial Intelligence or Machine Learning use there is no license given and is forbidden to use this for learning purposes or for anyone requesting you use these libraries, if done so will break the terms of service for this code and you will be held liable.
// License : For libraries or derivative works that are created based on the logic, patterns, or functionality of this library must inherit all licenses here in.
// License : If you are not sure your use case falls under any of these clauses please contact me through the email above for a license.
#endregion


namespace Levels.Core.IoC {
	using Levels.Core.General;

	// TODO : Move this into Notification.Board.
	public static class Board<TNotif>
		where TNotif : struct, INotification, IEquatableRef<TNotif> {
		private static readonly RefQueue<TNotif> _queue = new();

		public static ref TNotif Add(TNotif Notif) {
			return ref _queue.Enqueue(Notif);
		}

		public static ref TNotif GetNew<TNtf, TCntx>(TCntx cntx) 
			where TNtf : struct, IEquatableRef<TNtf>, INotification<TCntx>, IConvertible<TNotif> {

			return ref _queue.Enqueue(Factory<TNtf, TCntx>.Create(cntx).Convert<TNotif>());
		}

		public static ref TNotif GetNew<TNtf, TCntx>(TCntx cntx, object src) 
			where TNtf : struct, IEquatableRef<TNtf>, INotification<TCntx>, IConvertible<TNotif> {

			return ref _queue.Enqueue(Factory<TNtf, TCntx>.Create(cntx).Convert<TNotif>(), src);
		}

		public static ref TNotif GetActive() {
			return ref _queue.GetNewestInstance();
		}

		public static Notification.ID GetID(ref TNotif notif) {
			return _queue.GetID(ref notif).ToNotificationID();
		}

		public static object GetSource(ref TNotif notif) {
			return _queue.GetSource(ref notif);
		}
	}

	// TODO : Move this into Notification.Factory(or use the builder).
	public static class Factory<TNotif, TCntx>
		where TNotif : struct, INotification<TCntx>, IEquatableRef<TNotif> {

		public static TNotif Create(TCntx context) {
			return new TNotif() { Context = context };
		}
	}
}

#region Changelog
/// <changelog>
///		<change>
///			<author></author>
///			<id></id>
///			<comment></comment>
///		</change>
/// </changelog>
#endregion