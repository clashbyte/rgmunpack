using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RGMUnpack {

	/// <summary>
	/// File unpacking class
	/// </summary>
	public static partial class Unpacker {

		/// <summary>
		/// Current unpacker status
		/// </summary>
		public static UnpackStatus Status {
			get;
			private set;
		}

		/// <summary>
		/// Current unpacking file
		/// </summary>
		public static string CurrentFile {
			get;
			private set;
		}

		/// <summary>
		/// Unpacking progress
		/// </summary>
		public static float Progress {
			get;
			private set;
		}

		/// <summary>
		/// Error
		/// </summary>
		public static string ErrorInfo {
			get;
			private set;
		}

		/// <summary>
		/// Main unpacking method - run in background
		/// </summary>
		/// <param name="file">Path to file</param>
		public static void Unpack(string file) {
			try {
				Status = UnpackStatus.Unpacking;

				// Reading PAK file and creating folders
				BinaryReader pak = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read));
				string root = Path.GetDirectoryName(file);
				if (!Directory.Exists(Path.Combine(root, "rgmsys"))) {
					Directory.CreateDirectory(Path.Combine(root, "rgmsys"));
				}
				if (!Directory.Exists(Path.Combine(root, "Graphics"))) {
					Directory.CreateDirectory(Path.Combine(root, "Graphics"));
				}
				if (!Directory.Exists(Path.Combine(root, "Sounds"))) {
					Directory.CreateDirectory(Path.Combine(root, "Sounds"));
				}

				// Iterating files
				for (int i = 0; i < fileNames.Length; i++) {

					// Checking for end
					if (pak.BaseStream.Position >= pak.BaseStream.Length - 1) {
						break;
					}

					// Reading entry
					string name = fileNames[i];
					int size = pak.ReadInt32();
					int compSize = pak.ReadInt32();
					CurrentFile = name;
					Progress = (float)i / (float)fileNames.Length;

					if (size > 1024 * 1024 * 500) {
						throw new Exception("Too big size for item - maybe wrong PAK file");
					}

					// Reading entry data
					byte[] entry = pak.ReadBytes(compSize);

					// Decompressing if neccesary
					if (compSize < size) {
						entry = DecompressEntry(entry, size);
					}

					// Writing file to RGMSYS
					File.WriteAllBytes(Path.Combine(root, "rgmsys", name), entry);

					// Detecting graphics
					if (entry.Length > 4) {
						string magic = new string(new char[] {
							(char)entry[0], (char)entry[1], (char)entry[2], (char)entry[3]
						});

						// Unpacking BMPs
						if (magic.StartsWith("BM")) {

							// Loading file
							Image img = Image.FromStream(new MemoryStream(entry));
							if (img.Width == 64 && img.Height == 64 || img.Width == 32 && img.Height == 32) {
								img.RotateFlip(RotateFlipType.Rotate90FlipX);
							}

							// Writing BMP
							img.Save(Path.Combine(root, "Graphics", name + ".bmp"), ImageFormat.Bmp);
							
						} else {

							// Unpacking sounds
							string ext = "";
							if (magic == "RIFF") {

								// Wav file
								ext = "wav";

							} else if(magic == "MThd" || magic == "MTrk") {

								// Midi
								ext = "mid";
							}

							// Saving
							if (ext != "") {
								File.WriteAllBytes(Path.Combine(root, "Sounds", name + "." + ext), entry);
							}
						}
					}
				}
				Status = UnpackStatus.Complete;
			} catch (Exception ex) {
				ErrorInfo = ex.ToString();
				Status = UnpackStatus.Error;
			}
		}

		/// <summary>
		/// Decompressing single entry with JCALG1
		/// </summary>
		/// <param name="entry">Compressed data</param>
		/// <param name="fullSize">Decompressed size</param>
		/// <returns>Array of decompressed data</returns>
		static byte[] DecompressEntry(byte[] entry, int fullSize) {

			// Calling library
			byte[] decomp = new byte[fullSize];
			GCHandle srcHandle = GCHandle.Alloc(entry, GCHandleType.Pinned);
			GCHandle dstHandle = GCHandle.Alloc(decomp, GCHandleType.Pinned);
			int dsize = JCALG1_Decompress_Fast(srcHandle.AddrOfPinnedObject(), dstHandle.AddrOfPinnedObject());
			srcHandle.Free();
			dstHandle.Free();

			// Processing size
			if (dsize < fullSize) {
				byte[] temp = new byte[dsize];
				Array.Copy(decomp, temp, dsize);
				return temp;
			}
			return decomp;
		}


		/// <summary>
		/// External decompression func
		/// </summary>
		/// <param name="src">Source data pointer</param>
		/// <param name="dst">Destination data pointer</param>
		/// <returns>Number of decompressed bytes</returns>
		[DllImport(@"JCALG1.dll",
		CallingConvention = CallingConvention.StdCall,
		EntryPoint = "JCALG1_Decompress_Fast",
		ExactSpelling = false)]
		extern static int JCALG1_Decompress_Fast(IntPtr src, IntPtr dst);

		/// <summary>
		/// Status enumeration
		/// </summary>
		public enum UnpackStatus {
			None,
			Unpacking,
			Complete,
			Error,
		}

	}
}
