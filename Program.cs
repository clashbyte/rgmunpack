using System;
using System.Windows.Forms;

namespace RGMUnpack {
	static class Program {

		/// <summary>
		/// Main entry point
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);


			// Detecting file 
			string file = null;
			if (args.Length > 0) {
				if (System.IO.File.Exists(args[0])) {
					if (System.IO.Path.GetExtension(args[0]).ToLower() == ".pak") {
						file = args[0];
					} else {
						MessageBox.Show("Specified file is not PAK.", "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}
				}
			}
			if (string.IsNullOrEmpty(file)) {

				// Opening dialog
				OpenFileDialog ofd = new OpenFileDialog();
				ofd.Filter = "PAK Files (*.pak)|*.pak";
				ofd.DefaultExt = ".pak";
				if (ofd.ShowDialog() == DialogResult.OK) {
					file = ofd.FileName;
				}

			}

			// Running window
			if (!string.IsNullOrEmpty(file)) {
				StatusForm statusForm = new StatusForm(file);
				Application.Run(statusForm);
			}
		}
	}
}
