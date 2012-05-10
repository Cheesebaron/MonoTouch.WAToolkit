MonoTouch.WAToolkit
===================
This is a project attempting to port the [Windows Phone 7 Windows Azure toolkit](http://watwp.codeplex.com/) to [MonoTouch](http://ios.xamarin.com/).
It supports authenticating against Windows Azure and can obtain a token response for usage with applications that require this.

How does it work?
-------
The supplied sample briefly shows how to use the components. What happens in the code is:

1. Passing the realm and namespace to the `AccessControlLoginController` and pushing the view sends you to a list of Identity Providers to choose from.

2. Choosing one of these Providers opens up a WebView with the providers Mobile Login Page.

3. The usual JavaScript Notify function, which is on the landing page which you get to after logging in, is overridden with JavaScript that redirects the WebView to another address.

4. The address contains the token, which an overridden WebViewDelegate method takes and saves in the Token Store.

5. Sends you back to your application

After you get back to your Controller, which pushed the LoginController, you can obtain the Token from `RequestSecurityTokenResponseStore.Instance.RequestSecurityTokenResponse`

License
-------
This project is licensed under [Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0)
