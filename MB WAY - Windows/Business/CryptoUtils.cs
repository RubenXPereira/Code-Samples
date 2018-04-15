using System;
using System.Text;

using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using SIBS.MBWAY.Business.Network.Utils.Storage;
using SIBS.MBWAY.Business.Utils.MBWAYExceptions;

namespace SIBS.MBWAY.Business.Security
{
    public class CryptoUtils
    {
        private static string HEX = "0123456789ABCDEF";
        public static uint PKBKF2_DKLEN = 32;
        public static uint PKBKF2_ITERATION_COUNT = 10000;
        public static string defaultTDA = "0000000000000000000000000000000000000000";
        
        // The Lock Code must be a Session variable
        public static string lockCode = null;
        
        public static byte[] generateRandomBytes(int nrBytes)
        {
            // Define the length, in bytes, of the buffer.
            UInt32 length = (uint)nrBytes;

            // Generate random data and copy it to a buffer.
            IBuffer buffer = CryptographicBuffer.GenerateRandom(length);

            byte[] randomIv = buffer.ToArray();
            
            return randomIv;
        }

        public static byte[] toBytesFromString(string txt)
        {
            return Encoding.UTF8.GetBytes(txt);
        }
        
        public static string toStringFromBytes(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
        
        public static string toHexStringFromBytes(byte[] byteArray)
        {
            if (byteArray == null)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder(2 * byteArray.Length);
            for (int i = 0; i < byteArray.Length; i++)
            {
                sb.Append(HEX[(byteArray[i] >> 4) & 0x0f]).Append(HEX[byteArray[i] & 0x0f]);
            }
            return sb.ToString();
        }

        public static byte[] toBytesFromHexString(string hexString)
        {
            byte[] bytes = new byte[hexString.Length >> 1];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                int highDigit = HEX.IndexOf(Char.ToUpperInvariant(hexString[i]));
                int lowDigit = HEX.IndexOf(Char.ToUpperInvariant(hexString[i + 1]));
                if (highDigit == -1 || lowDigit == -1)
                {
                    throw new ArgumentException("The string contains an invalid digit.", "s");
                }
                bytes[i >> 1] = (byte)((highDigit << 4) | lowDigit);
            }
            return bytes;
        }

        public static void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            LogManager.WriteLine(sb.ToString());
        }

        public static byte[] getCCD()
        {
            LogManager.WriteLine("--- Get CCD ---");
            byte[] ccdcBytes = (byte[])StorageUtils.getData(StorageUtils.kCCDC);
            LogManager.WriteLine("CCDC: ");
            PrintByteArray(ccdcBytes);
            byte[] cccBytes;
            if (!StorageUtils.IsStoredFlagActive(StorageUtils.kIsBlockingCodeActive))
            {
                cccBytes = PBKDF2.calculateCCCwithIDA(StorageUtils.getIDA());
            }
            else
            {
                try
                {
                    cccBytes = PBKDF2.calculateCCCwithCodBlq(lockCode, StorageUtils.getIDA());
                }
                catch (Exception)
                {
                    throw new LockCodeException();
                }
            }

            LogManager.WriteLine("CCC: ");
            PrintByteArray(cccBytes);
            byte[] ccdBytes = AES256.DecryptAES256CBC(StorageUtils.getAppIV(), ccdcBytes, cccBytes);
            LogManager.WriteLine("CCD: ");
            PrintByteArray(ccdBytes);
            return ccdBytes;
        }

        public static string getCurrentTDA()
        {
            return TDA.getCurrentTDA();
        }
    }
}
