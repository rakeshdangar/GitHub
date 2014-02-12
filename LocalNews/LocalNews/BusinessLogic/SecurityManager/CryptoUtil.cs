using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;


namespace LocalNews.BusinessLogic.Security
{
    /// <summary>
    /// </summary>
    public enum CryptoKeyTypeEnum
    {
        /// <summary>
        /// Represents empty value
        /// </summary>
        Null = -1,
        /// <summary>
        /// Value has not been set 
        /// </summary>
        NotInitialized = 0,

        /// <summary>
        /// Key that is used when encrypting/decrypting credit card data
        /// </summary>
        CRYPTO_KEY_FOR_CC = 1,

        /// <summary>
        /// Key that is used when encrypting/decrypting customer related data
        /// </summary>
        CRYPTO_KEY_FOR_CUSTOMER_DATA = 2,

        /// <summary>
        /// Key that is rotated on a schedule to protect the other keys and hence the data
        /// </summary>
        CRYPTO_KEY_MASTER = 3,

        /// <summary>
        /// Key that is used when encrypting/decrypting webservice auth credentials
        /// </summary>
        CRYPTO_KEY_FOR_AUTH_CREDENTIALS = 4,

        /// <summary>
        /// Key that is used when encrypting via PGP
        /// </summary>
        CRYPTO_KEY_PGP_PUBLIC = 5,

        /// <summary>
        /// Key that is used when decrypting via PGP
        /// </summary>
        CRYPTO_KEY_PGP_PRIVATE = 6,
    }

    /// <summary>
    /// Common support methods for encrypting and decrypting data.
    /// </summary>
    public class CryptoUtil
    {
        #region Constants
        private const int SHA256SIZE = 32,
                          AESKEYSIZE = 32,
                          AESSALTSIZE = 24;
        private const string TIME_ENCODING_FORMAT = "TIME|{0}|INTERVAL|{1}|$${2}";
        #endregion

        #region Public Methods

        /// <summary>
        /// Computes a SHA hash on the input string, and converts the result to a Base64 string
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static string ComputeHash(string inputStr)
        {
            if (string.IsNullOrEmpty(inputStr))
                return string.Empty;
            byte[] resultHV = getHash(Encoding.UTF8.GetBytes(inputStr));
            return Convert.ToBase64String(resultHV);
        }

        /// <summary>
        /// Converts the keyPhrase string to byte array and calls the
        /// actual encryption routine to encrypt the text by passing
        /// the text to be encrypted and the keyphrase converted to byte array
        /// </summary>
        /// <param name="plainText">The text to be encrypted</param>
        /// <param name="plainKeyPhrase">The key phrase which will be used to encrypt the text</param>
        /// <returns>The encryted text as string</returns>
        public static string Encrypt(string plainText, string plainKeyPhrase)
        {
            if (string.IsNullOrEmpty(plainKeyPhrase))
                throw new ArgumentException("KeyPhrase value cannot be null or empty!");

            byte[] keyPhraseBytes = Encoding.UTF8.GetBytes(plainKeyPhrase);
            return encrypt(plainText, keyPhraseBytes);
        }

        /// <summary>
        /// Converts the keyPhrase string to byte array and calls the
        /// actual encryption routine to encrypt the text by passing
        /// the text to be encrypted and the keyphrase converted to byte array.
        /// After encryption if embedKey=true then the keyPhrase is pre-appended to 
        /// the encrypted string in a base-64 encoding format and also the length
        /// of the encoded keyPhrase is appended at the start of the string as a 2 character 
        /// numerical value.
        /// </summary>
        /// <param name="plainText">The text to be encrypted</param>
        /// <param name="plainKeyPhrase">The key phrase which will be used to encrypt the text</param>
        /// <param name="embedKey">Checking parameter whether to append the keyPhrase in the cipher.</param>
        /// <returns>The encryted text as string</returns>
        public static string Encrypt(string plainText, string plainKeyPhrase, bool embedKey)
        {
            if (embedKey)
            {
                string encryptedString = Encrypt(plainText, plainKeyPhrase);
                byte[] keyPhraseBytes = Encoding.UTF8.GetBytes(plainKeyPhrase);
                string encodedKey = Convert.ToBase64String(keyPhraseBytes);
                string lengthOfKey = encodedKey.Length.ToString();
                if (lengthOfKey.Length == 1)
                    lengthOfKey = "0" + lengthOfKey;
                encryptedString = lengthOfKey + encodedKey + encryptedString;
                return encryptedString;
            }
            else
                return Encrypt(plainText, plainKeyPhrase);
        }

        /// <summary>
        /// Converts the keyPhrase string to a byte array and invokes the 
        /// actual decryption routine to perform the decryption by passing the 
        /// cipherText to be decrypted and the keyPhrase converted to byte array
        /// </summary>
        /// <param name="cipherText">The string to be decrypted</param>
        /// <param name="plainKeyPhrase">The key with which the decryption will be done</param>
        /// <returns>The decrypted text</returns>
        public static string Decrypt(string cipherText, string plainKeyPhrase)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;
            if (string.IsNullOrEmpty(plainKeyPhrase))
                throw new ArgumentException("KeyPhrase value cannot be null or empty!");

            byte[] keyPhraseBytes = Encoding.UTF8.GetBytes(plainKeyPhrase);
            return decrypt(cipherText, keyPhraseBytes);
        }

        /// <summary>
        /// Decrypt the cipherText, with the keyphrase embedded in the ciphertext.
        /// </summary>
        /// <param name="cipherText">The string to be decrypted</param>
        /// <returns>The decrypted text</returns>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            if (cipherText.Length < 2)
                throw new ArgumentException("Invalid length for cipherText.");
            int keyLength = Convert.ToInt16(cipherText.Substring(0, 2));
            cipherText = cipherText.Remove(0, 2);

            if (cipherText.Length < keyLength)
                throw new ArgumentException("Invalid length for cipherText.");
            string encryptedKeyPhrase = cipherText.Substring(0, keyLength);
            byte[] keyPhrase = Convert.FromBase64String(encryptedKeyPhrase);
            cipherText = cipherText.Remove(0, keyLength);

            if (cipherText.Length == 0)
                throw new ArgumentException("Invalid length for cipherText.");
            return decrypt(cipherText, keyPhrase);
        }

        /// <summary>
        /// Encrypts the input plainText
        /// </summary>
        /// <param name="plainText">String to be encrypted</param>
        /// <param name="userId">Id of the user whoes data is being encrypted</param>
        /// <returns>The encrypted string</returns>
        public static string EncryptData(string plainText, long userId)
        {
            return EncryptHelper(plainText, CryptoKeyTypeEnum.CRYPTO_KEY_FOR_CUSTOMER_DATA, userId);
        }

        /// <summary>
        /// Decrypts the input cipherText
        /// </summary>
        /// <param name="cipherText">String to be decrypted</param>
        /// <param name="userId">Id of the user whoes data is being encrypted</param>
        /// <returns>The decrypted string</returns>
        public static string DecryptData(string cipherText, long userId)
        {
            return DecryptHelper(cipherText, CryptoKeyTypeEnum.CRYPTO_KEY_FOR_CUSTOMER_DATA, userId);
        }

        /// <summary>
        /// Encrypts plainText data using the crypto-key for Credit Cards
        /// </summary>
        /// <param name="plainTextCcData">String to be encrypted</param>
        /// <param name="userId">Id of the user whoes data is being encrypted</param>
        /// <returns>The encrypted string</returns>
        public static string EncryptCreditCardData(string plainTextCcData, long userId)
        {
            return EncryptHelper(plainTextCcData, CryptoKeyTypeEnum.CRYPTO_KEY_FOR_CC, userId);
        }

        /// <summary>
        /// Decrypts the cipherText using the crypto-key for Credit Cards
        /// </summary>
        /// <param name="cipherText">String to be decrypted</param>
        /// <param name="userId">Id of the user whoes data is being encrypted</param>
        /// <returns>The decrypted string</returns>
        public static string DecryptCreditCardData(string cipherText, long userId)
        {
            return DecryptHelper(cipherText, CryptoKeyTypeEnum.CRYPTO_KEY_FOR_CC, userId);
        }

        /// <summary>
        /// Encrypts a string that is valid for a time period.
        /// </summary>
        /// <param name="plainText">Text to be encrypted.</param>
        /// <param name="minutes">Number of minutes for which the encrypted value is valid.</param>
        /// <param name="userId">Id of the user whoes data is being encrypted</param>
        /// <returns>Type: <see cref="System.String"/><br />
        /// The encrypted version of the plain text.</returns>
        public static string EncryptDataWithExpiration(string plainText, int minutes, long userId)
        {
            string timeEncodedString = string.Format(TIME_ENCODING_FORMAT, DateTime.Now, minutes, plainText);
            return EncryptHelper(timeEncodedString, CryptoKeyTypeEnum.CRYPTO_KEY_FOR_CUSTOMER_DATA, userId);
        }

        /// <summary>
        /// Returns a decrypted string from an encrypted string that has a timer on it.
        /// </summary>
        /// <param name="cipherText">The encrypted string.</param>
        /// <param name="userId">Id of the user whoes data is being encrypted</param>
        /// <returns>Type: <see cref="System.String"/><br />
        /// The decrypted plain text.</returns>
        public static string DecryptDataWithExpiration(string cipherText, long userId)
        {
            string timeEncodedString = DecryptHelper(cipherText, CryptoKeyTypeEnum.CRYPTO_KEY_FOR_CUSTOMER_DATA, userId);
            string[] options = timeEncodedString.Split('|');
            if (options.Length >= 5)
            {
                string timeSegment = options[1];
                DateTime encodedDate;
                if (!DateTime.TryParse(timeSegment, out encodedDate))
                    throw new ArgumentException("Time not encoded");
                int interval;
                if (!Int32.TryParse(options[3], out interval))
                    throw new ArgumentException("Time interval not encoded");
                if (DateTime.Now < encodedDate.AddMinutes(-interval) || DateTime.Now > encodedDate.AddMinutes(interval))
                    throw new ArgumentException("Encrypted data has expired");
                return timeEncodedString.Substring(timeEncodedString.IndexOf("$$") + 2);
            }
            else
                throw new ArgumentException("Incorrect encoding");
        }

        /// <summary>
        /// Encrypts plainText data using the crypto-key for Auth Credentials
        /// </summary>
        /// <param name="plainText">String to be encrypted</param>
        /// <param name="userId">Id of the user whoes data is being encrypted</param>
        /// <returns>The encrypted string</returns>
        public static string EncryptAuthCredential(string plainText, long userId)
        {
            return EncryptHelper(plainText, CryptoKeyTypeEnum.CRYPTO_KEY_FOR_AUTH_CREDENTIALS, userId);
        }

        /// <summary>
        /// Decrypts the cipherText using the crypto-key for Auth Credentials
        /// </summary>
        /// <param name="cipherText">String to be decrypted</param>
        /// <param name="userId">Id of the user whoes data is being encrypted</param>
        /// <returns>The decrypted string</returns>
        public static string DecryptAuthCredential(string cipherText, long userId)
        {
            return DecryptHelper(cipherText, CryptoKeyTypeEnum.CRYPTO_KEY_FOR_AUTH_CREDENTIALS, userId);
        }

        /// <summary>
        /// Signs a content and returns a base 64 string.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="keyForSigning"></param>
        /// <returns></returns>
        public static string ComputeSignedContent(string content, string keyForSigning)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            HMACSHA1 hmacsha1 = new HMACSHA1(encoding.GetBytes(keyForSigning));
            byte[] hashMessage = hmacsha1.ComputeHash(encoding.GetBytes(content));
            string computedSignature = System.Convert.ToBase64String(hashMessage);
            return computedSignature;
        }

        /// <summary>
        /// Signs the content and returns as a hex string
        /// </summary>
        /// <param name="content"></param>
        /// <param name="keyForSigning"></param>
        /// <returns></returns>
        public static string ComputeSignedContentAsHexString(string content, string keyForSigning)
        {
            string computedSignature = ComputeSignedContent(content, keyForSigning);
            return ConvertAsciiToHex(computedSignature);
        }

        /// <summary>
        /// Encrypt the input keyphrase using the Master keyphrase modified by the microsite_id
        /// </summary>
        /// <param name="plainKeyPhrase">The keyphrase to be decrypted</param>
        /// <param name="masterKeyPhrase">The Master keyphrase</param>
        /// <param name="micrositeId">Id for the current microsite</param>
        /// <returns>The decrypted text</returns>
        public static string EncryptKeyPhraseWithMasterKeyPhrase(string plainKeyPhrase, string masterKeyPhrase, short micrositeId)
        {
            string finalMasterKeyPhrase = masterKeyPhrase + micrositeId.ToString();
            return Encrypt(plainKeyPhrase, finalMasterKeyPhrase);
        }

        /// <summary>
        /// Retrieves the decoded string based on the type from the specified encoded string.
        /// </summary>
        ///<remarks>The first 2 characters specify the decode type.
        ///</remarks>
        /// <param name="stringToDecode">A string to decode.</param>
        /// <returns>Type: System.String<br/>
        /// Returns the decoded string.</returns>
        public static string Decode(string stringToDecode)
        {
            if (null == stringToDecode || 1 == stringToDecode.Length)
                return string.Empty;
            string decodeType = stringToDecode.Substring(0, 2);
            switch (decodeType)
            {
                case "01":
                    return SimpleDecode(stringToDecode.Substring(2,stringToDecode.Length-2)); 
                default:
                    // You can use the default case.
                    return SimpleDecode(stringToDecode.Substring(2, stringToDecode.Length-2)); 
            }
        }
       
        #endregion

        #region Private Methods

        /// <summary>
        /// Computes the hash value from a <c>byte</c>  array by 
        /// calling the ComputeHash function of SHA256Managed library
        /// </summary>
        /// <param name="inputByteArr">A <c>byte</c> array type</param>
        /// <returns>Byte array</returns>
        private static byte[] getHash(byte[] inputByteArr)
        {
            if (inputByteArr.Length <= 0)
                throw new ArgumentException();
            SHA256Managed shaM = new SHA256Managed();
            return shaM.ComputeHash(inputByteArr);
        }

        /// <summary>
        /// The actual encryption routine which performs the encryption
        /// </summary>
        /// <param name="plainText">The text to be encrypted</param>
        /// <param name="inpKey">The key with which the encryption will be done</param>
        /// <returns>The encrypted text as a string</returns>
        private static string encrypt(string plainText, byte[] inpKey)
        {
            byte[] aesKey = getKey(inpKey, AESKEYSIZE);
            byte[] aesIv = getNewSalt(AESSALTSIZE);

            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = AESKEYSIZE * 8; // in bits
            aes.BlockSize = AESSALTSIZE * 8;
            MemoryStream outStream = new MemoryStream();

            CryptoStream encStream = new CryptoStream(outStream, aes.CreateEncryptor(aesKey, aesIv), CryptoStreamMode.Write);

            byte[] inpBytes = Encoding.UTF8.GetBytes(plainText);
            encStream.Write(inpBytes, 0, inpBytes.Length);
            encStream.FlushFinalBlock();

            int byteCnt = (int)outStream.Length;
            byte[] outBytes = new byte[byteCnt + AESSALTSIZE];
            outStream.Position = 0;
            outStream.Read(outBytes, 0, byteCnt);  // stuff only the encrypted data

            storeSalt(aesIv, outBytes); // add the IV to the enc data

            encStream.Close();
            outStream.Close();

            // return the cipherText as a Base64 encoded string (includes the IV)
            return Convert.ToBase64String(outBytes, 0, outBytes.Length);
        }

        /// <summary>
        /// The actual decryption routine.
        /// </summary>
        /// <param name="cipherText">The string to be decrypted</param>
        /// <param name="inpKey">The key with which the decryption will be done</param>
        /// <returns>The dcrypted string</returns>
        private static string decrypt(string cipherText, byte[] inpKey)
        {
            // the cipherText was Base64 encoded
            byte[] inpBytes = Convert.FromBase64String(cipherText);

            byte[] aesKey = getKey(inpKey, AESKEYSIZE);
            byte[] aesIv = retrieveSalt(inpBytes, AESSALTSIZE); // fetch the IV from the cipherText bytearray

            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = AESKEYSIZE * 8; // in bits
            aes.BlockSize = AESSALTSIZE * 8;
            MemoryStream outStream = new MemoryStream();

            CryptoStream decStream = new CryptoStream(outStream, aes.CreateDecryptor(aesKey, aesIv), CryptoStreamMode.Write);

            // remember, the total bytes of cipherText includes the IV
            // we should only decrypt the encrypted data
            decStream.Write(inpBytes, 0, (inpBytes.Length - AESSALTSIZE));
            decStream.FlushFinalBlock();

            int byteCnt = (int)outStream.Length;
            byte[] outBytes = new byte[byteCnt];
            outStream.Position = 0;
            outStream.Read(outBytes, 0, byteCnt);

            decStream.Close();
            outStream.Close();

            // return the plainText
            return Encoding.UTF8.GetString(outBytes, 0, byteCnt); ;
        }

        /// <summary>
        /// Creates the secret key to be used for the symmetric algorithm
        /// </summary>
        /// <param name="inpKey">The keyphrase byte array</param>
        /// <param name="keySize"></param>
        /// <returns>The secret key <c>byte[]</c></returns>
        private static byte[] getKey(byte[] inpKey, int keySize)
        {
            // since we are using SHA256, max keysize cannot exceed that hash size
            if (inpKey.Length == 0 || keySize <= 0 || keySize > SHA256SIZE)
                throw new ArgumentException("The input key length should be greated than zero and key size should be between 0 and the hashsize as defined by SHA256SIZE!");

            byte[] sha256Hash = getHash(inpKey);
            byte[] finalKey = new byte[keySize];
            for (int i = 0; i < keySize; i++)
                finalKey[i] = sha256Hash[i];
            return finalKey;
        }

        /// <summary>
        /// Generates the IV to be used for the symmetric algorithm based on the
        /// salt size as the input
        /// </summary>
        /// <param name="saltSize">The salt size <c>int</c></param>
        /// <returns>A byte array</returns>
        private static byte[] getNewSalt(int saltSize)
        {
            if (saltSize <= 0)
                throw new ArgumentException("Salt size must be greater that zero!");

            byte[] randBytes = new byte[saltSize];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randBytes);
            return randBytes;
        }

        /// <summary>
        /// Adds the salt at the end of the encrypted data
        /// </summary>
        /// <param name="salt">The byte array representing the salt generated</param>
        /// <param name="cipherText">The encrypted byte array</param>
        private static void storeSalt(byte[] salt, byte[] cipherText)
        {
            int saltSize = salt.Length;
            int startIdx = (cipherText.Length - saltSize);

            // if total cipherText size is less than or same as the salt, there is something wrong
            if (saltSize == 0 || startIdx <= 0)
                throw new ArgumentException("Salt size cannot be zero and the cipherText size must be greater than the salt size!");

            for (int i = 0; i < saltSize; i++)
                cipherText[startIdx + i] = salt[i];
        }

        /// <summary>
        /// Retrieves the salt from the end of the original
        /// cipherText
        /// </summary>
        /// <param name="cipherText">The cipherText byte array</param>
        /// <param name="saltSize">The salt size</param>
        /// <returns></returns>
        private static byte[] retrieveSalt(byte[] cipherText, int saltSize)
        {
            // IMP: This needs to do exactly the reverse of what storeSalt does
            // storeSalt adds the salt at the very end of the original cipherText
            // so just fetch it from there
            int startIdx = (cipherText.Length - saltSize);

            // if total cipherText size is less than or same as the salt, there is something wrong
            if (saltSize == 0 || startIdx <= 0)
                throw new ArgumentException("Salt size cannot be zero and the cipherText size must be greater than the salt size!");

            byte[] salt = new byte[saltSize];
            for (int i = 0; i < saltSize; i++)
                salt[i] = cipherText[startIdx + i];
            return salt;
        }

        private static string EncryptHelper(string plainTextData, CryptoKeyTypeEnum keyType, long userId)
        {
            //fetch the CryptoKeys
            string encryptedCryptoKey = string.Empty; //retrieve from stored location

            switch (keyType)
            {
                case CryptoKeyTypeEnum.CRYPTO_KEY_FOR_CC:
                    encryptedCryptoKey = string.Empty; //retrieve from stored location
                    break;
                case CryptoKeyTypeEnum.CRYPTO_KEY_FOR_CUSTOMER_DATA:
                    encryptedCryptoKey = string.Empty; //retrieve from stored location
                    break;
                case CryptoKeyTypeEnum.CRYPTO_KEY_FOR_AUTH_CREDENTIALS:
                    encryptedCryptoKey = string.Empty; //retrieve from stored location
                    break;
                default:
                    throw new ArgumentException("Unknown Key Type : " + keyType.ToString());
            }

            string masterKey = string.Empty; //retrieve from stored location
            return EncryptUsingMasterKey(plainTextData, encryptedCryptoKey, masterKey, userId);
        }

        private static string DecryptHelper(string cipherText, CryptoKeyTypeEnum keyType, long userId)
        {
            //fetch the CryptoKey
            string encryptedCryptoKey = string.Empty; //retrieve from stored location

            switch (keyType)
            {
                case CryptoKeyTypeEnum.CRYPTO_KEY_FOR_CC:
                    encryptedCryptoKey = string.Empty; //retrieve from stored location
                    break;
                case CryptoKeyTypeEnum.CRYPTO_KEY_FOR_CUSTOMER_DATA:
                    encryptedCryptoKey = string.Empty; //retrieve from stored location
                    break;
                case CryptoKeyTypeEnum.CRYPTO_KEY_FOR_AUTH_CREDENTIALS:
                    encryptedCryptoKey = string.Empty; //retrieve from stored location
                    break;
                default:
                    throw new ArgumentException("Unknown Key Type : " + keyType.ToString());
            }

            string masterKey = string.Empty; //retrieve from stored location
            return DecryptUsingMasterKey(cipherText, encryptedCryptoKey, masterKey, userId);
        }

        private static string EncryptUsingMasterKey(string plainText, string keyPhrase, string masterKeyPhrase, long userId)
        {
            if (string.IsNullOrEmpty(keyPhrase))
                throw new ArgumentException("KeyPhrase value cannot be null or empty!");
            if (string.IsNullOrEmpty(masterKeyPhrase))
                throw new ArgumentException("MasterKeyPhrase value cannot be null or empty!");

            string rawKey = DecryptKeyPhraseWithMasterKeyPhrase(keyPhrase, masterKeyPhrase, userId);

            return Encrypt(plainText, rawKey);
        }

        /// <summary>
        /// Wrapper method: (a) Handles the keyphrase as a delimited string of multiple keys (b) preprocesses each key using the MasterKey
        /// </summary>
        /// <param name="cipherText">The string to be decrypted</param>
        /// <param name="keyPhrase">A delimited string of one or more keys</param>
        /// <param name="masterKeyPhrase">Each key is to be decrypted using this</param>
        /// <param name="userId">Id for the current user</param>
        /// <returns>The decrypted text</returns>
        private static string DecryptUsingMasterKey(string cipherText, string keyPhrase, string masterKeyPhrase, long userId)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;
            if (string.IsNullOrEmpty(keyPhrase))
                throw new ArgumentException("KeyPhrase value cannot be null or empty!");
            if (string.IsNullOrEmpty(masterKeyPhrase))
                throw new ArgumentException("MasterKeyPhrase value cannot be null or empty!");

            string[] keys = keyPhrase.Split(new string[] { "$$" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < keys.Length; i++)
            {
                try
                {
                    string rawKey = DecryptKeyPhraseWithMasterKeyPhrase(keys[i], masterKeyPhrase, userId);
                    return Decrypt(cipherText, rawKey);
                }
                catch
                {
                    // handle the exception and continue with the next key in the delimited list
                }
            }
            // IF we reached here, NONE of the keys in the delimited list were good
            throw new ArgumentException("KeyPhrase does not successfully decrypt the cipherText!");
        }

        /// <summary>
        /// Decrypt the input keyphrase using the Master keyphrase modified by the microsite_id
        /// </summary>
        /// <param name="cipherKeyPhrase">The keyphrase to be decrypted</param>
        /// <param name="masterKeyPhrase">The Master keyphrase</param>
        /// <param name="userId">Id for the current user</param>
        /// <returns>The decrypted text</returns>
        private static string DecryptKeyPhraseWithMasterKeyPhrase(string cipherKeyPhrase, string masterKeyPhrase, long userId)
        {
            // Check if we need to decrypt the input key phrase, or return it as-is.
            // NOTE: As long as this logic is in place, the encrypted string cannot start with "Dmi" : 99.9999% it will not. Should be checked manually before installing.
            if (cipherKeyPhrase.StartsWith("Dmi", false, null))
                return cipherKeyPhrase;
            else
            {
                string finalMasterKeyPhrase = masterKeyPhrase + userId.ToString();
                return Decrypt(cipherKeyPhrase, finalMasterKeyPhrase);
            }
        }

        /// <summary>
        /// Returns the decoded string from the specified encoded string.
        /// </summary>
        /// <param name="stringToDecode">A string to decode.</param>
        /// <returns>Type: System.String<br/>
        /// Returns the decoded string.</returns>
        private static string SimpleDecode(string stringToDecode)
        {
            string decode = "";
            string abfrom = "jE8Par2g5MXozCJhU6fNwY@p0i7OZq1kAFHQSbdsu39lGRct4DKVmxLWnyBITev.";
            string abto = "YOFsRVy.85Jw7AdGKnvarXU@ZHucmBo6pqQ3txIj2zf01S94LTEgCNikblDWePMh";
            for (int x = 0; x < stringToDecode.Length; x++)
            {
                int y = abto.IndexOf(stringToDecode.Substring(x, 1));
                if (y == 0)
                    decode += stringToDecode.Substring(x, 1);
                else
                    decode += abfrom.Substring(y, 1);
            }
            return decode;
        }

        /// <summary>
        /// Returns the encoded string from the specified decoded string.
        /// </summary>
        /// <param name="stringToEncode">A string to encode.</param>
        /// <returns>Type: System.String<br/>
        /// Returns the encoded string.</returns>
        private static string SimpleEncode(string stringToEncode)
        {
            string encode = "";
            string abfrom = "jE8Par2g5MXozCJhU6fNwY@p0i7OZq1kAFHQSbdsu39lGRct4DKVmxLWnyBITev.";
            string abto = "YOFsRVy.85Jw7AdGKnvarXU@ZHucmBo6pqQ3txIj2zf01S94LTEgCNikblDWePMh";
            for (int x = 0; x < stringToEncode.Length; x++)
            {
                int y = abfrom.IndexOf(stringToEncode.Substring(x, 1));
                if (y == 0)
                    encode += stringToEncode.Substring(x, 1);
                else
                    encode += abto.Substring(y, 1);
            }
            return encode;
        }
        
        /// <summary>
        /// Convert a string to a hex string
        /// </summary>
        /// <param name="asciiString"></param>
        /// <returns></returns>
        private static string ConvertAsciiToHex(string asciiString)
        {
            string hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }

        #endregion
    }
}
