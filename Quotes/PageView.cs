using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreText;

namespace Quotes
{
	[Register ("PageView")]
	public class PageView : UIImageView
	{
		Page page;
		public bool UnstyledDrawing;
		RectangleF[] paragraphBounds;

		public PageView (IntPtr handle) : base (handle)
		{
		}

		public PageView (RectangleF rect) : base (rect)
		{
		}

		public void UpdatePage ()
		{
			Image = RenderPageWithSize (Bounds.Size);
		}

		public void SetPage (Page p)
		{
			if (page != p) {
				page = p;
				UpdatePage ();
			}
		}

		public void SetUnstyledDrawing (bool unstyled)
		{
			if (UnstyledDrawing != unstyled) {
				UnstyledDrawing = unstyled;
				UpdatePage ();
			}
		}

		public void SetLineHeight (float lHeight)
		{
			if (page.LineHeight != lHeight) {
				page.LineHeight = lHeight;
				UpdatePage ();
			}
		}

		public float GetLineHeight ()
		{
			return page.LineHeight;
		}

		public override bool CanBecomeFirstResponder {
			get {
				return true;
			}
		}

		/*
		 * Render the page here: we assume we are already in a normalized coordinate system which maps 
		 * 	our standard aspect ratio (3:4) to (1:1)
		 * The reason why we do this is to reuse the same drawing code for both the preview and the 
		 * 	full screen; for full screen rendering, we map the whole view, whereas the preview maps
		 * 	the whole preview image to a quarter of the page.
		 * */
		public RectangleF [] RenderPage (Page page, SizeF size, bool unstyledDrawing)
		{
			var pageRect = new RectangleF (0, 0, size.Width, size.Height);
			var paragraphBounds = new RectangleF [page.Paragraphs.Count];

			// fill background
			UIGraphics.GetCurrentContext ().SetFillColor (UIColor.FromHSBA (0.11f, 0.2f, 1, 1).CGColor);
			UIGraphics.GetCurrentContext ().FillRect (pageRect);

			pageRect = pageRect.Inset (20, 20);

			int i = 0;
			foreach (var p in page.Paragraphs) {
				var bounds = new RectangleF (pageRect.X, pageRect.Y, 0, 0);

				if (UnstyledDrawing) {

					var text = new NSString (page.StringForParagraph (p));
	
					var font = UIFont.FromName ("HoeflerText-Regular", 24);

					// draw text with the old legacy path, setting the font color to black.
					UIGraphics.GetCurrentContext ().SetFillColor (UIColor.Black.CGColor);
					bounds.Size = text.DrawString (pageRect, font);

				} else {

					// TODO: draw attributed text with new string drawing
					var text = page.AttributedStringForParagraph (p);
					var textContext = new NSStringDrawingContext ();

					text.DrawString (pageRect, NSStringDrawingOptions.UsesLineFragmentOrigin, textContext);

					bounds = textContext.TotalBounds;
					bounds.Offset (pageRect.X, pageRect.Y);
				}

				paragraphBounds [i++] = bounds;

				pageRect.Y += bounds.Height;
			}

			return paragraphBounds;
		}

		public void SelectParagraphAtPosition (PointF position, bool shouldShowMenu)
		{
			page.SelectedParagraph = NSRange.NotFound;
			var bounds = RectangleF.Empty;

			for (int i = 0; i < paragraphBounds.Length; i++) {
				bounds = paragraphBounds [i];
				if (bounds.Contains (position.X, position.Y)) {
					page.SelectedParagraph = i;
					break;
				}
			}

			if (shouldShowMenu) {
				BecomeFirstResponder ();
				var theMenu = UIMenuController.SharedMenuController;

				theMenu.SetTargetRect (bounds, this);
				theMenu.Update ();
				theMenu.SetMenuVisible (true, true);
			} else 
				UIMenuController.SharedMenuController.SetMenuVisible (false, true);

			UpdatePage ();
		}

		public UIImage RenderPagePreview (Page page, SizeF size)
		{
			UIGraphics.BeginImageContextWithOptions (size, true, 0.0f);

			var scale = CGAffineTransform.MakeScale (0.5f, 0.5f);
			UIGraphics.GetCurrentContext ().ConcatCTM (scale);

			RenderPage (page, new SizeF (1024, 768), false);

			var ret = UIGraphics.GetImageFromCurrentImageContext ();

			UIGraphics.EndImageContext ();

			return ret;
		}

		public UIImage RenderPageWithSize (SizeF size)
		{
			UIGraphics.BeginImageContextWithOptions (size, true, 0.0f);

			// render and hang on to paragraph bounds for hit testing
			paragraphBounds = RenderPage (page, size, UnstyledDrawing);

			var ret = UIGraphics.GetImageFromCurrentImageContext ();

			UIGraphics.EndImageContext ();

			return ret;
		}
	}
}

