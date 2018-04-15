using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;
using SIBS.MBWAY.Windows.DataModels.Base;
using SIBS.MBWAY.Business.Security;
using SIBS.MBWAY.Business.Network.Utils.Storage;

namespace SIBS.MBWAY.Windows.DataModels
{
    public class Card : BaseModel
    {
        private string _idc;

        public string Idc
        {
            get { return _idc; }
            set
            {
                _idc = value;
                NotifyPropertyChanged("Idc");
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private string _maskedPan;

        public string MaskedPan
        {
            get { return _maskedPan; }
            set
            {
                _maskedPan = value;
                NotifyPropertyChanged("MaskedPan");
            }
        }

        private byte[] _image;

        public byte[] Image
        {
            get { return _image; }
            set
            {
                _image = value;
                NotifyPropertyChanged("Image");
            }
        }

        private SolidColorBrush _color;

        public SolidColorBrush Color
        {
            get { return _color; }
            set
            {
                _color = value;
                NotifyPropertyChanged("Color");
            }

        }

        private DateTime _expirationDate;

        public DateTime ExpirationDate
        {
            get { return _expirationDate; }
            set
            {
                _expirationDate = value;
                NotifyPropertyChanged("ExpirationDate");
            }
        }

        private bool _isToRemove;

        public bool IsToRemove
        {
            get { return _isToRemove; }
            set
            {
                _isToRemove = value;
                NotifyPropertyChanged("IsToRemove");
            }
        }

        private MBNETParameters _cardMBNETParameters;

        public MBNETParameters CardMBNETParameters
        {
            get { return _cardMBNETParameters; }
            set
            {
                _cardMBNETParameters = value;
                NotifyPropertyChanged("CardMBNETParameters");
            }
        }

        private P2PParameters _cardP2PParameters;

        public P2PParameters CardP2PParameters
        {
            get { return _cardP2PParameters; }
            set
            {
                _cardP2PParameters = value;
                NotifyPropertyChanged("CardP2PParameters");
            }
        }

        private InhibitionParameters _cardInhibitionParameters;

        public InhibitionParameters CardInhibitionParameters
        {
            get { return _cardInhibitionParameters; }
            set
            {
                _cardInhibitionParameters = value;
                NotifyPropertyChanged("CardInhibitionParameters");
            }
        }
        
        public class P2PParameters : BaseModel
        {
            private bool _allowedToMakeTransfers;

            public bool AllowedToMakeTransfers
            {
                get { return _allowedToMakeTransfers; }
                set
                {
                    _allowedToMakeTransfers = value;
                    NotifyPropertyChanged("AllowedToMakeTransfers");
                }
            }

            private bool _allowedToReceiveTransfers;

            public bool AllowedToReceiveTransfers
            {
                get { return _allowedToReceiveTransfers; }
                set
                {
                    _allowedToReceiveTransfers = value;
                    NotifyPropertyChanged("AllowedToReceiveTransfers");
                }
            }

            private bool _isDefaultForTransfers;

            public bool IsDefaultForTransfers
            {
                get { return _isDefaultForTransfers; }
                set
                {
                    _isDefaultForTransfers = value;
                    NotifyPropertyChanged("IsDefaultForTransfers");
                }
            }

            private bool _isDefaultForPurchases;

            public bool IsDefaultForPurchases
            {
                get { return _isDefaultForPurchases; }
                set
                {
                    _isDefaultForPurchases = value;
                    NotifyPropertyChanged("IsDefaultForPurchases");
                }
            }

            private int _maxP2PLimit;

            public int MaxP2PLimit
            {
                get { return _maxP2PLimit; }
                set
                {
                    _maxP2PLimit = value;
                    NotifyPropertyChanged("MaxP2PLimit");
                }
            }

            private int _currencyCode;

            public int CurrencyCode
            {
                get { return _currencyCode; }
                set
                {
                    _currencyCode = value;
                    NotifyPropertyChanged("CurrencyCode");
                }
            }

            private List<int> _unauthorizedAliasType;

            public List<int> UnauthorizedAliasType
            {
                get { return _unauthorizedAliasType; }
                set
                {
                    _unauthorizedAliasType = value;
                    NotifyPropertyChanged("UnauthorizedAliasType");
                }
            }

            public P2PParameters() { }
        }

        public class MBNETParameters : BaseModel
        {
            private bool _allowedToCreateVirtualCards;

            public bool AllowedToCreateVirtualCards
            {
                get { return _allowedToCreateVirtualCards; }

                set
                {
                    _allowedToCreateVirtualCards = value;
                    NotifyPropertyChanged("AllowedToCreateVirtualCards");
                }
            }

            public MBNETParameters() { }
        }

        public class InhibitionParameters : BaseModel
        {
            private bool _isAllowedToHaveMBWAY;

            public bool IsAllowedToHaveMBWAY
            {
                get { return _isAllowedToHaveMBWAY; }

                set
                {
                    _isAllowedToHaveMBWAY = value;
                    NotifyPropertyChanged("IsAllowedToHaveMBWAY");
                }
            }

            private bool _isAllowedToWithdrawalMBWAY;

            public bool IsAllowedToWithdrawalMBWAY
            {
                get { return _isAllowedToWithdrawalMBWAY; }

                set
                {
                    _isAllowedToWithdrawalMBWAY = value;
                    NotifyPropertyChanged("IsAllowedToWithdrawalMBWAY");
                }
            }

            private bool _isAllowedToMakePublicSectorPayments;

            public bool IsAllowedToMakePublicSectorPayments
            {
                get { return _isAllowedToMakePublicSectorPayments; }

                set
                {
                    _isAllowedToMakePublicSectorPayments = value;
                    NotifyPropertyChanged("IsAllowedToMakePublicSectorPayments");
                }
            }

            public InhibitionParameters() { }
        }

        private bool _isMBNetOnly;

        public bool IsMBNetOnly
        {
            get { return _isMBNetOnly; }
            set
            {
                if (value != _isMBNetOnly)
                {
                    _isMBNetOnly = value;
                    NotifyPropertyChanged("IsMBNetOnly");
                }
            }
        }

#if WINDOWS_APP
        public string NameWithoutChanges
        {
            get { return _name; }
        }
#endif

        // To store (last used) IDC
        public static void storeLastUsedIDC(byte[] ccd, string idc)
        {
            AES256.encryptDataAndStore(CryptoUtils.toBytesFromHexString(idc), StorageUtils.kLastUsedIDC, ccd);
        }

        // To retrieve (last used) IDC
        public static string getLastUsedIDC()
        {
            byte[] ccd = CryptoUtils.getCCD();
            byte[] idcData = AES256.decryptDataAndGetFromMem(StorageUtils.kLastUsedIDC, ccd);

            if (idcData != null)
            {
                string result = CryptoUtils.toHexStringFromBytes(idcData);
                result = result.Replace(" ", "").Replace("<", "").Replace(">", "");
                return result;
            }

            return null;
        }

        public Card()
        {
            CardP2PParameters = new P2PParameters();
            CardMBNETParameters = new MBNETParameters();
            CardInhibitionParameters = new InhibitionParameters();
        }
    }
}
