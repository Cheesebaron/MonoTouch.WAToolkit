//---------------------------------------------------------------------------------
// Copyright 2012 Tomasz Cielecki (tomasz@ostebaronen.dk)
// Licensed under the Apache License, Version 2.0 (the "License"); 
// You may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR 
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
// MERCHANTABLITY OR NON-INFRINGEMENT. 

// See the Apache 2 License for the specific language governing 
// permissions and limitations under the License.
//---------------------------------------------------------------------------------

using System;
using System.Web;
using System.Drawing;
using System.Text;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.WAToolkit.Library.Utilities;

namespace MonoTouch.WAToolkit.Library
{
	public class AccessControlWebAuthController : UIViewController
	{
		public NSUrl _url;
		public UIWebView _webView;
		public string ScriptNotify = "<script type=\"text/javascript\">window.external = { 'Notify': function(s) { document.location = 'acs://settoken?token=' + s; }, 'notify': function(s) { document.location = 'acs://settoken?token=' + s; } };</script>";
		
		public AccessControlWebAuthController (string loginUrl)
		{
			this._url = new NSUrl(loginUrl);
		}
		
		public override void LoadView ()
		{
			base.LoadView ();
			var webFrame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height - this.NavigationController.NavigationBar.Frame.Height);
			_webView = new UIWebView(webFrame);
			
			var urlRequest = new NSUrlRequest(_url);
			
			_webView.LoadRequest(urlRequest);
			_webView.ScalesPageToFit = true;
			
			_webView.Delegate = new AccessControlWebAuthDelegate(this);
			
			View.AddSubview(_webView);
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.NavigationItem.RightBarButtonItem = null;
			
		}
		
		private void ShowProgress()
		{
			UIActivityIndicatorView view = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White);
			view.Frame = new RectangleF(0,0, 32, 32);
			this.NavigationItem.RightBarButtonItem = new UIBarButtonItem(view);
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}
	}
	
	public class AccessControlWebAuthDelegate : UIWebViewDelegate
	{
		private AccessControlWebAuthController controller;
		private AccessControlWebAuthConnectionDelegate urlDelegate;
		
		public AccessControlWebAuthDelegate(AccessControlWebAuthController controller) 
		{
			this.controller = controller;
			urlDelegate = new AccessControlWebAuthConnectionDelegate(controller);
		}
		
		public override bool ShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			if (null != controller && controller._url != null)
			{
				if (request.Url.Equals(controller._url))
					return true;
			}
			
			controller._url = request.Url;
			string scheme = controller._url.Scheme;

			if (scheme.Equals("acs"))
			{
				StringBuilder b = new StringBuilder(Uri.UnescapeDataString(request.Url.ToString()));
				b.Replace("acs://settoken?token=", string.Empty);
				
				RequestSecurityTokenResponse token = RequestSecurityTokenResponse.FromJSON(b.ToString());
				RequestSecurityTokenResponseStore.Instance.RequestSecurityTokenResponse = token;
				
				controller.NavigationController.PopToRootViewController(true);
			}
			
			NSUrlConnection.FromRequest(request, urlDelegate);
			
			return false;
		}
		
		public override void LoadStarted (UIWebView webView)
		{
			var app = UIApplication.SharedApplication;
			app.NetworkActivityIndicatorVisible = true;
		}
		
		public override void LoadingFinished (UIWebView webView)
		{
			var app = UIApplication.SharedApplication;
			app.NetworkActivityIndicatorVisible = false;
		}
		
		public override void LoadFailed (UIWebView webView, NSError error)
		{
			var app = UIApplication.SharedApplication;
			app.NetworkActivityIndicatorVisible = false;
		}
	}
	
	public class AccessControlWebAuthConnectionDelegate : NSUrlConnectionDelegate
	{
		private AccessControlWebAuthController controller;
		private NSMutableData _data;
		
		public AccessControlWebAuthConnectionDelegate(AccessControlWebAuthController controller)
		{
			this.controller = controller;
		}
		
		public override void FailedWithError (NSUrlConnection connection, NSError error)
		{
			if (_data != null) 
			{
        		_data = null;
    		}
		}
		
		public override void ReceivedData (NSUrlConnection connection, NSData data)
		{
			if (_data == null)
        		_data = new NSMutableData();
			_data.AppendData(data);
		}
		
		public override void FinishedLoading (NSUrlConnection connection)
		{
			if (_data != null)
			{
				string scriptNotifyAndContent = controller.ScriptNotify;
				scriptNotifyAndContent += NSString.FromData(_data, NSStringEncoding.UTF8).ToString();
				
				_data = null;
				
				controller._webView.LoadHtmlString(scriptNotifyAndContent, controller._url);
			}
		}
	}
}

