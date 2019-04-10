using Entities;
using Entities.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;


namespace Utilities
{
    public class Common
    {
        public static string settingsPath = @"C:\ePremium";
        public static string settingsFilename = "SettingInfo.json";
        public static string timeEntriesFilename = "DTR.json";
        public static string currentMonthYear = DateTime.Now.Month.ToString() + DateTime.Now.Year;
        public static string commonSettingsPath = Common.settingsPath + "\\" + Common.settingsFilename;
        public static string timeInEntriesPath = Common.settingsPath + "\\" + currentMonthYear + "\\" + timeEntriesFilename;
        bool _isPrompt = false;

        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(System.Environment.MachineName, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(System.Environment.MachineName, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }


        public static void Encrypt(DailyRecord record)
        { }//Boss vic

        public static void Decrypt(DailyRecord record)
        { }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static TimeSheet GetTimeSheet()
        {
            var timesheet = new TimeSheet();
            var response = string.Empty;
            var userName = Environment.UserName;
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;

            var randomstring = RandomString(16);

            var url = $"http://epremium.internal.towerswatson.com" +
                $"/_vti_bin/OTService/OTService.svc/GetClaimsForCoverage?coverage=" +
                $"Claims+Details+List+For+{month}+{year}" +
                $"&lanId={userName}&RandomString={randomstring}" +
                $"&SiteURL=http%3A%2F%2Fepremium.internal.towerswatson.com" +
                $"&WebURL=http%3A%2F%2Fepremium.internal.towerswatson.com" +
                $"&LogInName={userName}";

            using (var wb = new WebClient())
            {
                response = wb.DownloadString(url);
            }

            timesheet = JsonConvert.DeserializeObject<TimeSheet>(response);

            return timesheet;
        }

        private static List<DailyRecord> GetLocalTimeToUpload(List<DailyRecord> localTime)
        {
            //get timesheet from epremium
            var epremiumTimeSheet = GetTimeSheet();

            if (epremiumTimeSheet?.GetClaimsForCoverageResult != null)
            {
                //upload only those that doesn't exist in epremium
                var result =
                    (from local in localTime
                     join epremium in epremiumTimeSheet.GetClaimsForCoverageResult
                     on local.Date equals epremium.Date
                     select new { local.Date }).ToList();

                foreach (var item in result)
                {
                    localTime.RemoveAll(x => item.Date == x.Date);
                }
            }

            RemoveNoLogout(localTime);

            return localTime;
        }

        private static List<DailyRecord> RemoveNoLogout(List<DailyRecord> localTime)
        {
            localTime.RemoveAll(x => x.TimeOut == DateTime.MinValue);
            return localTime;
        }

        public static bool UploadTimesheet()
        {
            var result = false;

            //compare records from epremium vs. local timesheet
            var localTimeSheet = GetTimeEntries(timeInEntriesPath);

            if (localTimeSheet == null)
            {
                MessageBox.Show($"You have no timesheet for {GetTimesheetCoverage()}"
                    , "Upload"
                    , MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return result;
            }

            var timeSheetToUpload = GetLocalTimeToUpload(localTimeSheet);

            if (timeSheetToUpload?.Count > 0)
            {
                //upload records to epremium

                foreach (var dtr in timeSheetToUpload)
                {
                    UploadTimesheet(dtr);
                }

                result = true;
            }
            else
            {
                MessageBox.Show($"No valid time records to upload for {GetTimesheetCoverage()}"
                   , "Upload"
                   , MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            return result;
        }

        public static string GetTimesheetCoverage()
        {
            return $"{ DateTime.Now.ToString("MMMM")} { DateTime.Now.Year}";
        }

        private static void UploadTimesheet(DailyRecord record)
        {
            var userName = record.UserName;
            var month = record.Date.Month;
            var year = record.Date.Year;
            var shift = record.Shift;
            var activity = record.Activity;
            var timein = record.TimeIn;
            var timeout = GetCalculatedTimeOut(record.TimeIn, record.TimeOut);
            var timeinperiod = timein.ToString("tt", CultureInfo.InvariantCulture);
            var timeoutperiod = timeout.ToString("tt", CultureInfo.InvariantCulture);
            var breaktime = 60;
            var mealAllowance = record.MealAllowance;
            var transpoAllowance = record.TranspoAllowance;

            var url = $"http://epremium.internal.towerswatson.com/" +
                $"_vti_bin/OTService/OTService.svc/AddNewClaim?listName=" +
                $"Claims+Details+List+For+{month}+{year}&" +
                $"lanID={userName}" +
                $"&shift={shift}" +
                $"&claimDate={record.Date.Month}%2F{record.Date.Day}%2F{record.Date.Year}" +
                $"&activity={activity}" +
                $"&from={timein.ToString("hh")}%3A{timein.ToString("mm")}+{timeinperiod}" +
                $"&to={timeout.ToString("hh")}%3A{timeout.ToString("mm")}+{timeoutperiod}" +
                $"&breakTime={breaktime}" +
                $"&mealAllowance={mealAllowance}" +
                $"&transpoAllowance={transpoAllowance}" +
                $"&SiteURL=http%3A%2F%2Fepremium.internal.towerswatson.com" +
                $"&WebURL=http%3A%2F%2Fepremium.internal.towerswatson.com";

            using (var wb = new WebClient())
            {
                var response = wb.DownloadString(url);
            }
        }

        public static DateTime GetCalculatedTimeOut(DateTime timein, DateTime timeout)
        {
            var calculatedtimeout = timein.AddHours(9);

            if (calculatedtimeout > timeout)
            {
                calculatedtimeout = timeout;
            }

            return calculatedtimeout;
        }

        public static bool PromtStartOfDay()
        {
            //[LOGIN] A.1 - Start of Day
            throw new NotImplementedException();
        }

        public static bool HasLoginForDaDay { get; set; }

        public static void SnoozeLogin()
        {
            ////[LOGIN] A.5 - 30 min snooze
        }

        //[LOGOUT] B.1 - End of day
        public static bool HasCompleteWorkDay { get; set; }

        public static List<DailyRecord> GetTimeEntries(string file)
        {
            List<DailyRecord> result = null;

            if (File.Exists(timeInEntriesPath))
            {
                using (StreamReader r = new StreamReader(timeInEntriesPath))
                {
                    string encryptedvalue = r.ReadToEnd();
                    var json = Decrypt(encryptedvalue);
                    result = JsonConvert.DeserializeObject<List<DailyRecord>>(json);
                }
            }

            return result;
        }

        public static void AddNewTimeRecord(List<DailyRecord> timesheet, DailyRecord timeRecord)
        {
            //create timein for the day
            var t1 = new DailyRecord
            {
                UserName = Environment.UserName,
                Date = DateTime.Now.Date,
                TimeIn = timeRecord.TimeIn,
                Shift = timeRecord.Shift,
                TimeOut = timeRecord.TimeOut,
                Activity = GetCurrentSetting()?.Activity
            };

            //add new time to the existing timesheet
            timesheet.Add(t1);
        }

        private static bool SaveToFile(List<DailyRecord> timesheet)
        {
            var result = true;
            try
            {
                var timesheetjson = JsonConvert.SerializeObject(timesheet);
                var encryptedtimesheet = Encrypt(timesheetjson);
                using (var sw = new StreamWriter(timeInEntriesPath, false))
                {
                    sw.Write(encryptedtimesheet);
                }
            }
            catch (Exception)
            {

                result = false;
            }

            return result;
        }

        public static void WriteScheduleSettingsToFile(ScheduleSetting scheduleSetting, string path)
        {
            Directory.CreateDirectory(settingsPath);
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                var data = JsonConvert.SerializeObject(scheduleSetting);
                sw.Write(data);
            }

        }

        public static bool WriteTimeEntriesToFile(DailyRecord timeRecord, bool isLogin)
        {
            bool result = false;
            List<DailyRecord> timesheet = new List<DailyRecord>();
            var subpath = settingsPath + "\\" + currentMonthYear;
            Directory.CreateDirectory(subpath);
            if (File.Exists(timeInEntriesPath))
            {
                try
                {
                    if (isLogin)
                    {
                        timesheet = GetTimeEntries(timeInEntriesPath);
                        AddNewTimeRecord(timesheet, timeRecord);
                        result = SaveToFile(timesheet);
                    }
                    else
                    {
                        timesheet = GetTimeEntries(timeInEntriesPath);
                        timesheet.LastOrDefault().TimeOut = DateTime.Now;
                        result = SaveToFile(timesheet);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving time entry. /n {ex.Message}");
                }
            }
            else
            {
                try
                {
                    timesheet = new List<DailyRecord>();
                    AddNewTimeRecord(timesheet, timeRecord);
                    result = SaveToFile(timesheet);
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Error saving time entry. /n {ex.Message}");
                }
            }

            return result;
        }

        public static bool HasScheduleSetting()
        {
            return File.Exists(commonSettingsPath);
        }

        public static string[] ReadLinesofTextSetting()
        {
            return File.ReadAllLines(commonSettingsPath);

        }
        public static string ReadCurrentLinesofTimeIn()
        {
            return File.ReadAllText(timeInEntriesPath);

        }
        public static bool IsCurrentTimeEntryExists()
        {
            return File.Exists(timeInEntriesPath);
        }

        public static string[] GetLinesofJson(string path)
        {
            var result = File.ReadAllLines(path);
            return result;
        }

        public static string GetCurrentShiftSetting()
        {
            if (HasScheduleSetting())
            {
                //get current settings shift
                using (StreamReader sr = new StreamReader(commonSettingsPath))
                {
                    var data = sr.ReadToEnd();
                    var json = JsonConvert.DeserializeObject<ScheduleSetting>(data);
                    return json.Shift;
                }
            }
            else
            {
                return "";
            }

        }

        public static ScheduleSetting GetCurrentSetting()
        {
            if (HasScheduleSetting())
            {
                //get current settings shift
                using (StreamReader sr = new StreamReader(commonSettingsPath))
                {
                    var data = sr.ReadToEnd();
                    var json = JsonConvert.DeserializeObject<ScheduleSetting>(data);
                    return json;
                }
            }
            else
            {
                return new ScheduleSetting();
            }

        }

        public static bool HasLoginToday()
        {
            var latestLogin = GetLatestLogIn();
            var currentDateandTime = DateTime.Now;

            if (currentDateandTime.Date != latestLogin.Date)
            {
                return false; //prompt login also consider night shift 
            }
            else
            {
                return true;
            }

        }

        public static bool HasLogoutToday()
        {
            var ss = GetLatestTimeEntryLine();

            var latestLogin = ss.TimeIn;
            var currentDateandTime = DateTime.Now;

            if (currentDateandTime.Date == latestLogin.Date
                && ss.TimeOut == DateTime.MinValue)
            {
                
                return false; //prompt login also consider night shift 
            }
            else
            {
                return true;
            }
        }

        public static DailyRecord GetLatestTimeEntryLine()
        {
            DailyRecord mydata = new DailyRecord();
            if (File.Exists(timeInEntriesPath))
            {
                using (StreamReader r = new StreamReader(timeInEntriesPath))
                {
                    string encryptedtimesheet = r.ReadToEnd();

                    var json = Decrypt(encryptedtimesheet);

                    List<DailyRecord> timesheet = JsonConvert.DeserializeObject<List<DailyRecord>>(json);
                    var lastobject = timesheet.LastOrDefault();

                    return mydata = lastobject;
                }
            }
            return mydata;

        }
        public static DateTime GetLatestLogIn()
        {
            var record = GetLatestTimeEntryLine();
            return record.TimeIn;
        }

        public static void CloseWindow<T>(string name = "") where T : Window
        {
            if (Application.Current.Windows.OfType<T>().Count() == 1)
            {
                Application.Current.Windows.OfType<T>().First().Close();
            }
        }

        public static bool IsPrompt { get; set; }
    }

}

