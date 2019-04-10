using Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Utilities;
using static Utilities.Extension;


namespace BusinessLayer
{
    public class ProcessTime
    {
        public bool RecordTime(bool isLogin = true)
        {
            return SavingLogic(isLogin);
        }
        public void TimeOut(DateTime TimeOut)
        {
            TimeOut.GetRoundTime(5);
        }
        public void UploadTimesheet(DateTime CurrentDate)
        { }
        public void ViewTimesheet(int SelectedMonth, int SelectedYear)
        { }

        private bool SavingLogic(bool isLogin)
        {
            bool result = false;
            try
            {
                DailyRecord dailyRecord = new DailyRecord();
                //dailyRecord.TimeIn = !isLogin ? Common.GetLatestLogIn() : DateTime.Now.GetRoundTime(5);
                //dailyRecord.Date = !isLogin ? Common.GetLatestLogIn() : DateTime.UtcNow.Date;
                dailyRecord.TimeIn = DateTime.Now.GetRoundTime(5);
                dailyRecord.Date = DateTime.UtcNow.Date;
                dailyRecord.UserName = Environment.UserName;
                dailyRecord.TimeOut = !isLogin ? DateTime.UtcNow : DateTime.MinValue;
                dailyRecord.Shift = Common.GetCurrentShiftSetting();
                dailyRecord.Activity = Common.GetCurrentSetting().Activity;
                result = Common.WriteTimeEntriesToFile(dailyRecord, isLogin);
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return result;
        }

        public static List<DailyRecord> LoadJson(DateTime TimeIn)
        {
            List<DailyRecord> timesheet = new List<DailyRecord>();
            var subpath = Common.settingsPath + "\\" + Common.currentMonthYear;

            var existingfileame = GenerateFileName();

            if (File.Exists($@"{subpath}\\{existingfileame}"))
            {
                using (StreamReader r = new StreamReader(string.Format($@"{subpath}\{existingfileame}")))
                {
                    string json = r.ReadToEnd();
                    timesheet = JsonConvert.DeserializeObject<List<DailyRecord>>(json);

                    //create timein for the day
                    var t1 = new DailyRecord
                    {
                        UserName = Environment.UserName,
                        Date = TimeIn,
                        TimeIn = TimeIn
                    };

                    //add new time to the existing timesheet
                    timesheet.Add(t1);

                }

                var timesheetjson = JsonConvert.SerializeObject(timesheet);
                using (var sw = new StreamWriter($@"{subpath}\{GenerateFileName()}", false))
                {
                    sw.Write(timesheetjson);
                }
            }
            else
            {
                Directory.CreateDirectory(subpath);

                using (var sw = new StreamWriter($@"{subpath}\{GenerateFileName()}", false))
                {
                    timesheet = new List<DailyRecord>();

                    //create timein for the day
                    var t1 = new DailyRecord
                    {
                        UserName = Environment.UserName,
                        Date = DateTime.Now.Date,
                        TimeIn = TimeIn
                    };

                    //add new time to the existing timesheet
                    timesheet.Add(t1);

                    var timesheetjson = JsonConvert.SerializeObject(timesheet);
                    sw.Write(timesheetjson);
                }

            }

            return timesheet;
        }

        public static string GenerateFileName()
        {
            string UniqueNameGenerator = string.Format(@"DTR{0}.json", DateTime.Now.Month);

            return UniqueNameGenerator;
        }
        
        public bool SaveLogin()
        {
            bool result = false;
            try
            {
                //var dateNow = DateTime.Now.Date;
                //var dt1 = Extension.GetRoundTime(DateTime.Now, 5);

                ////var roundedTimeIn = DateTime.Now.Add(new TimeSpan(0, Extension.GetRoundTime(DateTime.Now, 5).Minutes, 0));

                DailyRecord dailyRecord = new DailyRecord();
                dailyRecord.Date = DateTime.Now.Date;
                dailyRecord.TimeIn = DateTime.Now;
                dailyRecord.UserName = Environment.UserName;
                //Common.WriteTimeEntriesToFile(dailyRecord);
                result = true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return result;
        }

        public bool IsNeedToPromptLogIn()
        {
            if (!Common.IsCurrentTimeEntryExists())
            {
                return true;
            }
            else
            {
                if (Common.HasLoginToday())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            //need further considerations for night shift
            //since they should not be prompt when the date changes on midnight
        }
    }
}
