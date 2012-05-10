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
using System.Linq;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using MonoTouch.WAToolkit.Library.Utilities;
using MonoTouch.WAToolkit.Library.EventArguments;
using System.Collections.Generic;

namespace MonoTouch.WAToolkit.Library.Login
{
	public class AccessControlLoginController : UITableViewController
	{
		private Uri _identityProviderDiscoveryService;
		private readonly string _realm;
		private readonly string _acsNamespace;
		private const string ProviderDiscoveryUrl = "https://{0}.accesscontrol.windows.net/v2/metadata/IdentityProviders.js?protocol=javascriptnotify&realm={1}&version=1.0";
		
		public AccessControlLoginController(string realm, string acsNamespace) : base(UITableViewStyle.Grouped)
		{
			_realm = realm;
			_acsNamespace = acsNamespace;
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			Title = "Providers";
			
			TableView.DataSource = new AccessControlLoginDataSource(null);
			TableView.Delegate = new AccessControlLoginDelegate(this, null);
			
			GetSecurityToken();
		}
		
		public void GetSecurityToken(Uri identityProviderDiscoveryService)
        {
            _identityProviderDiscoveryService = identityProviderDiscoveryService;
			
			var jsonClient = new JSONIdentityProviderDiscoveryClient();
            jsonClient.GetIdentityProviderListCompleted += IdentityProviderListRefreshCompleted;
            jsonClient.GetIdentityProviderListAsync(_identityProviderDiscoveryService);
        }

        public void GetSecurityToken()
        {
            if (null == _realm)
            {
                throw new InvalidOperationException("Realm was not set");
            }

            if (null == _acsNamespace)
            {
                throw new InvalidOperationException("ServiceNamespace was not set");
            }

            var identityProviderDiscovery = new Uri(
                string.Format(
                    ProviderDiscoveryUrl,
                    _acsNamespace,
                    HttpUtility.UrlEncode(_realm)),
                UriKind.Absolute
                );

            GetSecurityToken(identityProviderDiscovery);
        }
		
		private void IdentityProviderListRefreshCompleted(object sender, GetIdentityProviderListEventArgs e)
        {
			InvokeOnMainThread(() => {
            	if (null == e.Error)
            	{
					TableView.DataSource = new AccessControlLoginDataSource(e.Result);
					TableView.Delegate = new AccessControlLoginDelegate(this, e.Result);
					TableView.ReloadData();
            	}
			});
        }
	}
	
	public class AccessControlLoginDataSource : UITableViewDataSource
	{
		private IList<SectionData> _sectionDataList;
		private readonly IEnumerable<IdentityProviderInformation> _providerInformation;
		
		public AccessControlLoginDataSource (IEnumerable<IdentityProviderInformation> providerInformation)
		{
			_providerInformation = providerInformation;
			
			CreateTableData();
		}
		
		private void CreateTableData()
		{
			_sectionDataList = new List<SectionData>();

		    var section1 = new SectionData("IdentityProviderInformation")
		    {
		        Footer = "Log in the application with your account of choice."
		    };

		    if (null != _providerInformation)
			{
				foreach (var provider in _providerInformation)
				{
				    var section1Data = new Data
				    {
				        Accessory = UITableViewCellAccessory.DisclosureIndicator,
				        Label = provider.Name,
				        CellStyle = UITableViewCellStyle.Default
				    };
				    if (provider.ImageUrl != null || provider.ImageUrl != "")
						provider.LoadImageFromImageUrl();
					section1Data.Image = provider.Image;
					
					section1.SData.Add(section1Data);
				}
			}
			
			_sectionDataList.Add(section1);
		}
		
		public override int NumberOfSections (UITableView tableView)
		{
			return _sectionDataList == null ? 0 : _sectionDataList.Count;
		}
		
		public override string TitleForHeader (UITableView tableView, int section)
        {
			return _sectionDataList == null ? "" : _sectionDataList[section].Title;
        }
                
		public override string TitleForFooter (UITableView tableView, int section)
		{
			return _sectionDataList == null ? "" : _sectionDataList[section].Footer;
		}
                
        public override int RowsInSection (UITableView tableview, int section)
        {
                return _sectionDataList[section].SData.Count;
        }
		
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {       
			var sectionData = _sectionDataList[indexPath.Section];
			var cellId = sectionData.CellId;
			var row = sectionData.SData[indexPath.Row];
                        
			var cell = tableView.DequeueReusableCell(cellId) ?? new UITableViewCell(row.CellStyle, cellId);

		    cell.TextLabel.Text = row.Label;
			cell.Accessory = row.Accessory;
			if (row.Image != null)
				cell.ImageView.Image = row.Image;

			return cell; 
        }
		
		private class SectionData
        {
			public string Title { get;set; }
			public string Footer { get;set; }
			public string CellId { get;set; }
			public IList<Data> SData { get;set; }

			public SectionData(string cellId)
			{
				Title = "";
				Footer = "";
				CellId = cellId;
				SData = new List<Data>();
			}
        }
                
        private class Data
        {
            public string Label { get; set; }
            public UITableViewCellAccessory Accessory { get; set; }
            public UITableViewCellStyle CellStyle { get; set; }
            public UIImage Image { get; set; }
        }
	}
	
	public class AccessControlLoginDelegate: UITableViewDelegate
	{
		private readonly AccessControlLoginController _controller;
		private readonly IEnumerable<IdentityProviderInformation> _providerInformation;
		
		public AccessControlLoginDelegate (AccessControlLoginController controller, IEnumerable<IdentityProviderInformation> providerInformation)
		{
			_controller = controller;
			_providerInformation = providerInformation;
		}
		
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
        {
			UIViewController nextController = null;
			
			if (_providerInformation != null)
			{
				var loginUrl = _providerInformation.ElementAt(indexPath.Row).LoginUrl;
				if (!string.IsNullOrEmpty(loginUrl))
				{
				    nextController = new AccessControlWebAuthController(loginUrl)
				    {
				        Title = _providerInformation.ElementAt(indexPath.Row).Name
				    };
				}
			}
			
			if (nextController != null)
				_controller.NavigationController.PushViewController(nextController, true);
		}
	}
}

