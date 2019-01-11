using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RGMUnpack {
	public partial class StatusForm : Form {

		/// <summary>
		/// File path
		/// </summary>
		string filePath;

		/// <summary>
		/// Background thread
		/// </summary>
		Thread thread;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="file"></param>
		public StatusForm(string file) {
			InitializeComponent();
			filePath = file;
		}

		/// <summary>
		/// Focusing form on show
		/// </summary>
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			Focus();

			if (thread == null) {
				thread = new Thread(ThreadedUnpack);
				thread.IsBackground = true;
				thread.Priority = ThreadPriority.BelowNormal;
				thread.Start();
			}
			logicTimer.Start();
		}

		/// <summary>
		/// Closing
		/// </summary>
		void StatusForm_FormClosed(object sender, FormClosedEventArgs e) {
			logicTimer.Stop();
			if (thread != null) {
				if (thread.IsAlive) {
					thread.Abort();
				}
			}
		}

		/// <summary>
		/// Closing dialog
		/// </summary>
		void button1_Click(object sender, EventArgs e) {
			Close();
		}

		/// <summary>
		/// Internal tick
		/// </summary>
		void logicTimer_Tick(object sender, EventArgs e) {
			switch (Unpacker.Status) {
				case Unpacker.UnpackStatus.Unpacking:
					if (Unpacker.CurrentFile != nameLabel.Text) {
						nameLabel.Text = Unpacker.CurrentFile;
					}
					progressBar.Value = (int)(Unpacker.Progress * 100f);
					nameLabel.Text = Unpacker.CurrentFile;
					break;
				case Unpacker.UnpackStatus.Complete:
					logicTimer.Stop();
					progressBar.Value = 100;
					MessageBox.Show("All files are extracted!", "Complete!", MessageBoxButtons.OK, MessageBoxIcon.Information);
					Close();
					break;
				case Unpacker.UnpackStatus.Error:
					logicTimer.Stop();
					progressBar.Value = 0;
					MessageBox.Show(Unpacker.ErrorInfo, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					Close();
					break;
			}
		}
		
		/// <summary>
		/// Unpacking
		/// </summary>
		void ThreadedUnpack() {
			Unpacker.Unpack(filePath);
		}
	}
}
