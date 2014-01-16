using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;
using System.Drawing;

namespace Example_CoreAnimation.Screens.iPad.CustomizableAnimationViewer
{
	public partial class CustomizableAnimationViewerScreen : UIViewController, IDetailView
	{
		public event EventHandler ContentsButtonClicked;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			btnContents.TouchUpInside += (sender, e) => {
				if (ContentsButtonClicked != null)
					ContentsButtonClicked (sender, e);
			};

			btnStart.TouchUpInside += (s, e) => {
				
				// begin our animation block. the name allows us to refer to it later
				UIView.BeginAnimations ("ImageMove");

				UIView.SetAnimationDidStopSelector (new Selector ("AnimationStopped"));
				UIView.SetAnimationDelegate (this); //NOTE: you need this for the selector b.s.
				
				// animation delay
				UIView.SetAnimationDelay ((double)sldrDelay.Value);
				
				// animation duration
				UIView.SetAnimationDuration ((double)sldrDuration.Value);
				
				// animation curve
				UIViewAnimationCurve curve = UIViewAnimationCurve.EaseInOut;
				switch (sgmtCurves.SelectedSegment) {
				case 0:
					curve = UIViewAnimationCurve.EaseInOut;
					break;
				case 1:
					curve = UIViewAnimationCurve.EaseIn;
					break;
				case 2:
					curve = UIViewAnimationCurve.EaseOut;
					break;
				case 3:
					curve = UIViewAnimationCurve.Linear;
					break;
				}
				UIView.SetAnimationCurve (curve);
				
				// repeat count
				UIView.SetAnimationRepeatCount (float.Parse (txtRepeateCount.Text));
				
				// autorevese when repeating
				UIView.SetAnimationRepeatAutoreverses (swtchAutoReverse.On);
				
				// move the image one way or the other
				if (imgToAnimate.Frame.Y == 214) {
					imgToAnimate.Frame = new RectangleF (
						new PointF (imgToAnimate.Frame.X, imgToAnimate.Frame.Y + 400),
						imgToAnimate.Frame.Size);
				} else {
					imgToAnimate.Frame = new RectangleF (
						new PointF (imgToAnimate.Frame.X, imgToAnimate.Frame.Y - 400),
						imgToAnimate.Frame.Size);
				}
				
				// finish our animation block
				UIView.CommitAnimations ();
			};
		}

		[Export]
		public void AnimationStopped (string name, NSNumber numFinished, IntPtr context)
		{
			Console.WriteLine ("Animation completed");
		}
	}
}

