using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SIBS.MBWAY.Business.Security;
using Windows.ApplicationModel.Contacts;
using System.Collections.Generic;
using SIBS.MBWAY.Business.Utils;

namespace SIBS.MBWAY.Business.Network.Utils.Storage
{
    public static class StorageUtils
    {
        #region Operative in app

        public static readonly string mbwayProtocol = "mbway:";
        public static readonly string mbwayProtocolTests = "mbwayQLY:";

        #endregion

        public static bool isWindowsPhone;

        public static readonly string userNameVault = "Teste";

        public static readonly string kIV = "kIV";
        public static readonly string kIDA = "kIDA";
        private static string IDA;

        public static readonly string kDeviceModel = "kDeviceModel";
        public static readonly string kDeviceOS = "kDeviceOS";
        public static readonly string kDeviceSerial = "kDeviceSerial";
        public static readonly string kCCDC = "kCCDC";
        public static readonly string kCCD = "kCCD";
        public static readonly string kCCDNCC = "kCCDNCC";
        public static readonly string kTDA = "kTDA";
        public static readonly string kPreviousTDA = "kPreviousTDA";
        public static readonly string kTDASalt = "kTDASalt";
        public static readonly string kNotificationToken = "kNotificationToken";
        public static readonly string kCardsList = "kCardsList";
        public static readonly string kCardDefaultForPurchases = "kCardDefaultForPurchases";
        public static readonly string kSalt = "kSalt";
        public static readonly string kAppVersion = "kAppVersion";

        public static readonly string kIsBlockingCodeActive = "kIsBlockingCodeActive";
        public static readonly string kBlockingCodeTries = "kBlockingCodeTries";
        public static readonly string kIsAppActivated = "kIsAppActivated";
        public static readonly string kTestString = "kTestString";
        public static readonly string kAppConfirmActivationData = "kAppConfirmActivationData";
        public static readonly string kPlatformWindowsPhone = "kPlatformWindowsPhone";
        public static readonly string kDeativatedByNoCardsState = "kDeativatedByNoCardsState";
        public static readonly string kNotificationApproveTransfer = "kNotificationApproveTransfer";
        public static readonly string kBlockedNeedRegistrationAlias = "kBlockedNeedRegistrationAlias";
        public static readonly string kVirtualCardFilter = "kVirtualCardFilter";
        public static readonly string kLastUsedIDC = "kLastUsedIDC";

        public static readonly string kGamificationID = "kGamificationID";
        public static readonly string kPassword = "kPassword";
        public static readonly string kToken = "kToken";
        public static readonly string kTokenExpirationDate = "kTokenExpirationDate";

        public static readonly string kIsGamificationEnabled = "kIsGamificationEnabled";

        public static readonly string kSyncedContactsEncrypted = "kSyncedContactsEncrypted";
        public static readonly string kGamificationNrTimesAppOpened = "kGamificationNrTimesAppOpened";
        
        public static byte[] getAppIV()
        {
            string AppIV = (string)getData(kIV);

            if (AppIV == null)
            {
                AppIV = "0000000000000000000000000000000000000000";
            }

            byte[] appIVBytes = CryptoUtils.toBytesFromHexString(AppIV);

            return appIVBytes;
        }

        public static string getIDA()
        {
            IDA = (string)getData(kIDA);

            return IDA;
        }

        #region 'Storage operations to read or write'

        public static object getData(string key)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            return localSettings.Values[key];
        }

        public static void storeData(string key, byte[] data)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = data;
        }

        public static void storeData(string key, string data)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = data;
        }

        public static void removeData(string key)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = null;
        }

        #endregion

        #region 'handle files and data'

        public async static void storeDataToFile(string key, string data)
        {
            LogManager.WriteLine("Saving data to File with key: " + key);

            try
            {
                byte[] fileBytes = Encoding.UTF8.GetBytes(data.ToCharArray());

                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    stream.Write(fileBytes, 0, fileBytes.Length);
                }
            }
            catch (Exception)
            {
            }
        }

        public async static Task<object> getDataFromFile(string key)
        {
            LogManager.WriteLine("Getting data from File with key: " + key);

            try
            {
                StorageFolder local = ApplicationData.Current.LocalFolder;

                Stream stream = await local.OpenStreamForReadAsync(key);

                string text;

                using (var reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }

                return text;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region 'clean/reset all storage'

        public async static void CleanResetApplicationStorage()
        {
            storeFlagState(kIsAppActivated, false);
            
            IDA = null;
            
            storeDataToFile(kCardsList, null);

            await ApplicationData.Current.ClearAsync();

            string deviceModel = AppSetup.getDeviceModel();
            storeData(kDeviceModel, deviceModel);

            string deviceOS = AppSetup.getDeviceOS();
            storeData(kDeviceOS, deviceOS);

            string deviceSerial = AppSetup.getDeviceSerial();
            storeData(kDeviceSerial, deviceSerial);
        }

        #endregion

        #region 'storage flags'

        public static bool IsStoredFlagActive(string flag)
        {
            try
            {
                if (getData(flag) != null)
                {
                    return bool.Parse((string)getData(flag));
                }
            }
            catch (Exception) { }

            return false;
        }

        public static bool storeFlagState(string key, bool goForActive)
        {
            try
            {
                storeData(key, goForActive.ToString());
                return true;
            }
            catch (Exception) { }

            return false;
        }

        #endregion

        #region 'Gamification'

        private static int gamificationNewThreshold = 10;

        public static bool IsGamificationNew()
        {
            int nrTimesAppOpened = 0;
            try
            {
                nrTimesAppOpened = int.Parse((string)getData(kGamificationNrTimesAppOpened));
            }
            catch (Exception) { }

            bool result = nrTimesAppOpened > gamificationNewThreshold ? false : true;
            return result;
        }

        public static void UpdateGamificationNewState()
        {
            string gamificationNrTimesAppOpened = (string)getData(kGamificationNrTimesAppOpened);
            if (gamificationNrTimesAppOpened == null)
            {
                storeData(kGamificationNrTimesAppOpened, "1");
            }
            else
            {
                int nrTimesAppOpened = 0;

                try
                {
                    nrTimesAppOpened = int.Parse((string)getData(kGamificationNrTimesAppOpened));
                }
                catch (Exception) { }

                if (nrTimesAppOpened <= gamificationNewThreshold)
                {
                    nrTimesAppOpened++;
                    storeData(kGamificationNrTimesAppOpened, nrTimesAppOpened.ToString());
                }
            }
        }

        #endregion

        #region 'Contacts'

        public static List<Contact> mPhoneContacts;

        #endregion
    }
}
