﻿using ArtSourceWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrosspostSharp3 {
	public partial class Form1 : Form {
		private ISiteWrapper _currentWrapper;
		private int _currentPosition = 0;

		private async Task Populate() {
			if (_currentWrapper == null) return;

			tableLayoutPanel1.Controls.Clear();

			int i = _currentPosition;
			int stop = _currentPosition + 4;

			btnLoad.Enabled = false;
			btnPrevious.Enabled = false;
			btnNext.Enabled = false;

			try {
				while (true) {
					for (; i < stop && i < _currentWrapper.Cache.Count(); i++) {
						var item = _currentWrapper.Cache.Skip(i).First();

						Image image;
						var req = WebRequest.Create(item.ThumbnailURL);
						using (var resp = await req.GetResponseAsync())
						using (var stream = resp.GetResponseStream())
						using (var ms = new MemoryStream()) {
							await stream.CopyToAsync(ms);
							ms.Position = 0;
							image = Image.FromStream(ms);
						}

						tableLayoutPanel1.Controls.Add(new Panel {
							BackgroundImage = image,
							BackgroundImageLayout = ImageLayout.Zoom,
							Dock = DockStyle.Fill
						});
					}

					if (i == stop) break;

					await _currentWrapper.FetchAsync();
					if (_currentWrapper.IsEnded) break;
				}
			} catch (Exception ex) {
				MessageBox.Show(this, ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			btnLoad.Enabled = true;
			btnPrevious.Enabled = _currentPosition > 0;
			btnNext.Enabled = _currentWrapper.Cache.Count() > stop || !_currentWrapper.IsEnded;
		}

		public Form1() {
			InitializeComponent();

			ddlSource.Items.Add(new FurAffinityWrapper(new FurAffinityIdWrapper(
				b: "",
				a: "")));
		}

		private async void btnLoad_Click(object sender, EventArgs e) {
			_currentWrapper = ddlSource.SelectedItem as ISiteWrapper;
			_currentPosition = 0;
			await Populate();
		}

		private async void btnPrevious_Click(object sender, EventArgs e) {
			_currentPosition = Math.Max(0, _currentPosition - 4);
			await Populate();
		}

		private async void btnNext_Click(object sender, EventArgs e) {
			_currentPosition += 4;
			await Populate();
		}
	}
}
