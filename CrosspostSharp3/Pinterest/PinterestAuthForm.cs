﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrosspostSharp3 {
	public class PinterestAuthForm : Form {
		public string Code { get; private set; }

		public PinterestAuthForm(string clientId, string callbackUrl, IEnumerable<string> scopes = null) {
			this.Width = 515;
			this.Height = 750;

			var webBrowser1 = new WebBrowser {
				Dock = DockStyle.Fill
			};
			this.Controls.Add(webBrowser1);

			this.Shown += (o, e) => {
				StringBuilder sb = new StringBuilder();
				sb.Append($"response_type=code&");
				sb.Append($"client_id={WebUtility.UrlEncode(clientId)}&");
				sb.Append($"redirect_uri={WebUtility.UrlEncode(callbackUrl)}");
				if (scopes != null) {
					sb.Append($"&scope={WebUtility.UrlEncode(string.Join(" ", scopes))}");
				}
				webBrowser1.Navigate("https://api.pinterest.com/oauth/?" + sb);
			};

			Uri ret = new Uri(callbackUrl);
			webBrowser1.Navigated += (o, e) => {
				if (e.Url.Authority == ret.Authority && e.Url.AbsolutePath == ret.AbsolutePath) {
					int index = e.Url.Query.IndexOf("code=");
					if (index > -1) {
						string code = e.Url.Query.Substring(index + 5);
						if (code.Contains("&")) code = code.Substring(0, code.IndexOf("&"));
						Code = code;
						this.Close();
					}
				}
			};

			webBrowser1.DocumentTitleChanged += (o, e) => {
				this.Text = webBrowser1.DocumentTitle;
			};

			webBrowser1.Navigating += (o, e) => {
				if (e.Url.OriginalString.StartsWith("javascript:void")) e.Cancel = true;
			};
		}
	}
}
