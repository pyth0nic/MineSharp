﻿using MineSharp.Core.Logging;
using System.Security.Cryptography;

namespace MineSharp.Protocol.Crypto {
    public class RSAHelper {
		public static RSA? DecodePublicKey(byte[] publicKeyBytes) {
			MemoryStream ms = new MemoryStream(publicKeyBytes);
			BinaryReader rd = new BinaryReader(ms);
			byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
			byte[] seq = new byte[15];

			try {
				byte byteValue;
				ushort shortValue;

				shortValue = rd.ReadUInt16();

				switch (shortValue) {
					case 0x8130:
						rd.ReadByte();

						break;

					case 0x8230:
						rd.ReadInt16();

						break;

					default:
						Logger.GetLogger().Warning($"PublicKey Decode Returning null!");

						return null;
				}

				seq = rd.ReadBytes(15);

				if (!CompareBytearrays(seq, SeqOID)) return null;

				shortValue = rd.ReadUInt16();

				if (shortValue == 0x8103) rd.ReadByte();
				else if (shortValue == 0x8203)
					rd.ReadInt16();
				else {
					Logger.GetLogger().Warning($"PublicKey Decode Returning null! (shortvalue 1)");

					return null;
				}

				byteValue = rd.ReadByte();

				if (byteValue != 0x00) {
					Logger.GetLogger().Warning($"PublicKey Decode Returning null! (bytevalue)");

					return null;
				}

				shortValue = rd.ReadUInt16();

				if (shortValue == 0x8130) rd.ReadByte();
				else if (shortValue == 0x8230)
					rd.ReadInt16();
				else {
					Logger.GetLogger().Warning($"PublicKey Decode Returning null! (Shortvalue 2)");

					return null;
				}


				CspParameters parms = new CspParameters();
				parms.Flags = CspProviderFlags.NoFlags;
				parms.KeyContainerName = Guid.NewGuid().ToString().ToUpperInvariant();

				parms.ProviderType =
					((Environment.OSVersion.Version.Major > 5) || ((Environment.OSVersion.Version.Major == 5)
																   && (Environment.OSVersion.Version.Minor >= 1))) ?
						0x18 : 1;

				var rsa = RSA.Create();
				//RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(parms);
				RSAParameters rsAparams = new RSAParameters();

				rsAparams.Modulus = rd.ReadBytes(DecodeIntegerSize(rd));

				GetTraits(rsAparams.Modulus.Length * 8, out int sizeMod, out int sizeExp);

				rsAparams.Modulus = AlignBytes(rsAparams.Modulus, sizeMod);
				rsAparams.Exponent = AlignBytes(rd.ReadBytes(DecodeIntegerSize(rd)), sizeExp);

				rsa.ImportParameters(rsAparams);

				return rsa;
			} catch (Exception e) {
				Logger.GetLogger().Warning($"PublicKey Decode Exception: {e}");

				return null;
			} finally {
				rd.Close();
			}
		}

		private static bool CompareBytearrays(byte[] a, byte[] b) {
			if (a.Length != b.Length)
				return false;

			int i = 0;

			foreach (byte c in a) {
				if (c != b[i])
					return false;

				i++;
			}

			return true;
		}

		private static byte[] AlignBytes(byte[] inputBytes, int alignSize) {
			int inputBytesSize = inputBytes.Length;

			if ((alignSize != -1) && (inputBytesSize < alignSize)) {
				byte[] buf = new byte[alignSize];

				for (int i = 0; i < inputBytesSize; ++i) {
					buf[i + (alignSize - inputBytesSize)] = inputBytes[i];
				}

				return buf;
			} else {
				return inputBytes;
			}
		}

		private static int DecodeIntegerSize(System.IO.BinaryReader rd) {
			byte byteValue;
			int count;

			byteValue = rd.ReadByte();

			if (byteValue != 0x02) return 0;

			byteValue = rd.ReadByte();

			if (byteValue == 0x81) {
				count = rd.ReadByte();
			} else if (byteValue == 0x82) {
				byte hi = rd.ReadByte();
				byte lo = rd.ReadByte();
				count = BitConverter.ToUInt16(new[] { lo, hi }, 0);
			} else {
				count = byteValue;
			}

			while (rd.ReadByte() == 0x00) {
				count -= 1;
			}

			rd.BaseStream.Seek(-1, System.IO.SeekOrigin.Current);

			return count;
		}

		private static void GetTraits(int modulusLengthInBits, out int sizeMod, out int sizeExp) {
			int assumedLength = -1;
			double logbase = System.Math.Log(modulusLengthInBits, 2);

			if (logbase == (int)logbase) {
				assumedLength = modulusLengthInBits;
			} else {
				assumedLength = (int)(logbase + 1.0);
				assumedLength = (int)(System.Math.Pow(2, assumedLength));
				System.Diagnostics.Debug.Assert(false);
			}

			switch (assumedLength) {
				case 512:
					sizeMod = 0x40;
					sizeExp = -1;

					break;

				case 1024:
					sizeMod = 0x80;
					sizeExp = -1;

					break;

				case 2048:
					sizeMod = 0x100;
					sizeExp = -1;

					break;

				case 4096:
					sizeMod = 0x200;
					sizeExp = -1;
					break;

				default:
					System.Diagnostics.Debug.Assert(false);

					break;
			}

			sizeMod = -1;
			sizeExp = -1;
		}
	}
}
