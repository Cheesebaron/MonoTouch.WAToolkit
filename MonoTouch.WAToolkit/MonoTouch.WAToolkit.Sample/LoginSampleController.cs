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
using System.Collections.Generic;

using MonoTouch.UIKit;

using MonoTouch.WAToolkit.Library.Login;
using MonoTouch.WAToolkit.Library.Utilities;

namespace MonoTouch.WAToolkit.Sample
{
	public class LoginSampleController : UITableViewController
	{
		private LoginSampleDataSource loginDataSource;
		
		public LoginSampleController ()  : base(UITableViewStyle.Grouped)
		{
		}
		
		public override void LoadView ()
		{
			base.LoadView ();
			
			var token = RequestSecurityTokenResponseStore.Instance.RequestSecurityTokenResponse;
			loginDataSource = new LoginSampleDataSource(token);
			TableView.DataSource = loginDataSource;
			Title = "WAToolkit";
		}
		
		public override void ViewWillAppear (bool animated)
		{			
			UIBarButtonItem loginButton = new UIBarButtonItem("Log In", UIBarButtonItemStyle.Bordered, null);
			loginButton.Clicked += HandleLoginButtonClicked;
			this.NavigationItem.RightBarButtonItem = loginButton;
		}

		void HandleLoginButtonClicked (object sender, EventArgs e)
		{
			AccessControlLoginController login = new AccessControlLoginController("uri://security.noisesentinel.com/Phone7", "bruelandkjaer");
			this.NavigationController.PushViewController(login, true);
			
		}
	}
			
	public class LoginSampleDataSource : UITableViewDataSource
	{
		private RequestSecurityTokenResponse data;
		private IList<SectionData> _sectionDataList;
		
		public LoginSampleDataSource(RequestSecurityTokenResponse tokenResponse)
		{
			this.data = tokenResponse;
			CreateTable();
		}
		
		private void CreateTable()
		{
			_sectionDataList = new List<SectionData>();
			
			SectionData section1 = new SectionData("IdentityProviderInformation");
			section1.Title = "Below you should see the token from your Identity Provider. If not press Log In in the Navigation Bar to get a new token.";
			
			if (null != data)
			{
				string[] fields = {"Created", "Expires", "Is Expired", "Token Type", "Token"};
				foreach (string field in fields)
				{					
					Data section1Data = new Data();
					section1Data.Accessory = UITableViewCellAccessory.None;
					section1Data.Label = field;
					switch (field)
					{
						case "Created": section1Data.Subtitle = data.created.ToString(); break;
						case "Expires": section1Data.Subtitle = data.expires.ToString(); break;
						case "Is Expired": section1Data.Subtitle = data.IsExpired.ToString(); break;
						case "Token Type": section1Data.Subtitle = data.tokenType.ToString(); break;
						case "Token": section1Data.Subtitle = data.securityToken; break;
						default: break;
					}

					section1Data.CellStyle = UITableViewCellStyle.Subtitle;
				
					section1.sData.Add(section1Data);
				}
			}
			_sectionDataList.Add(section1);
		}
		
		public override int RowsInSection (UITableView tableView, int section)
		{
			return _sectionDataList[section].sData.Count;
		}
		
		public override int NumberOfSections (UITableView tableView)
		{
			return _sectionDataList == null ? 0 : _sectionDataList.Count;
		}
		
		public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			SectionData sectionData = _sectionDataList[indexPath.Section];
			string cellId = sectionData.CellId;
			Data row = sectionData.sData[indexPath.Row];
                        
			UITableViewCell cell = tableView.DequeueReusableCell(cellId);

			if (cell == null)
				cell = new UITableViewCell(row.CellStyle, cellId);
                        
			cell.TextLabel.Text = row.Label;
			cell.Accessory = row.Accessory;
			if (row.CellStyle == UITableViewCellStyle.Subtitle)
				cell.DetailTextLabel.Text = row.Subtitle;

			return cell;
		}
		
		private class SectionData
        {
			public string Title { get;set; }
			public string Footer { get;set; }
			public string CellId { get;set; }
			public IList<Data> sData { get;set; }

			public SectionData(string cellId)
			{
				Title = "";
				Footer = "";
				CellId = cellId;
				sData = new List<Data>();
			}
        }
                
        private class Data
        {
            public string Label { get; set; }
            public string Subtitle { get; set; }
            public UITableViewCellAccessory Accessory { get; set; }
            public UITableViewCellStyle CellStyle { get; set; }
            public UIImage Image { get; set; }
        }
	}
}

