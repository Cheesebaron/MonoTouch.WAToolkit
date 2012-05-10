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
using System.Drawing;
using System.Text;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.WAToolkit.Library.Utilities;

namespace MonoTouch.WAToolkit.Library.Login
{
	public class AccessControlWebAuthController : UIViewController
	{
		public NSUrl Url;
		public UIWebView WebView;
		public string ScriptNotify = "<script type=\"text/javascript\">window.external = { 'Notify': function(s) { document.location = 'acs://settoken?token=' + s; }, 'notify': function(s) { document.location = 'acs://settoken?token=' + s; } };</script>";
		
		public AccessControlWebAuthController (string loginUrl)
		{
			Url = new NSUrl(loginUrl);
		}
		
		public override void LoadView ()
		{
			base.LoadView ();
			var webFrame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height - NavigationController.NavigationBar.Frame.Height);
			WebView = new UIWebView(webFrame);
			
			var urlRequest = new NSUrlRequest(Url);
			
			WebView.LoadRequest(urlRequest);
			WebView.ScalesPageToFit = true;
			
			WebView.Delegate = new AccessControlWebAuthDelegate(this);
			
			View.AddSubview(WebView);
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			NavigationItem.RightBarButtonItem = null;
			
		}
		
/*
		private void ShowProgress()
		{
		    var view = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White)
		    {
		        Frame = new RectangleF(0, 0, 32, 32)
		    };
		    NavigationItem.RightBarButtonItem = new UIBarButtonItem(view);
		}
*/
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}
	}
	
	public class AccessControlWebAuthDelegate : UIWebViewDelegate
	{
		private readonly AccessControlWebAuthController _controller;
		private readonly AccessControlWebAuthConnectionDelegate _urlDelegate;
		
		public AccessControlWebAuthDelegate(AccessControlWebAuthController controller) 
		{
			_controller = controller;
			_urlDelegate = new AccessControlWebAuthConnectionDelegate(controller);
		}
		
		public override bool ShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			if (null != _controller && _controller.Url != null)
			{
				if (request.Url.Equals(_controller.Url))
					return true;
			}

		    if (_controller != null)
		    {
		        _controller.Url = request.Url;

		        var scheme = _controller.Url.Scheme;

		        if (scheme.Equals("acs"))
		        {
		            var b = new StringBuilder(Uri.UnescapeDataString(request.Url.ToString()));
		            b.Replace("acs://settoken?token=", string.Empty);
				
		            RequestSecurityTokenResponseStore.Instance.RequestSecurityTokenResponse = RequestSecurityTokenResponse.FromJSON(b.ToString());
				
		            _controller.NavigationController.PopToRootViewController(true);
		        }
		    }

		    NSUrlConnection.FromRequest(request, _urlDelegate);
			
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
		private readonly AccessControlWebAuthController _controller;
		private NSMutableData _data;
		
		public AccessControlWebAuthConnectionDelegate(AccessControlWebAuthController controller)
		{
			_controller = controller;
		}
		
		public override void FailedWithError (NSUrlConnection connection, NSError error)
		{
        	_data = null;
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
				string scriptNotifyAndContent = _controller.ScriptNotify;
				scriptNotifyAndContent += NSString.FromData(_data, NSStringEncoding.UTF8).ToString();
				
				_data = null;
				
				_controller.WebView.LoadHtmlString(scriptNotifyAndContent, _controller.Url);
			}
		}
	}
}

