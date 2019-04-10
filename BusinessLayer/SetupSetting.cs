using Entities;
using Entities.Entities;
using System;
using System.Collections.Generic;
using Utilities;

namespace BusinessLayer
{
    public static class SetupSetting
    {
        public static bool SaveSettings(ScheduleSetting scheduleSetting)
        {
            bool result = false;
            try
            {
                var path = Common.settingsPath + "\\" + Common.settingsFilename;
                Common.WriteScheduleSettingsToFile(scheduleSetting, path);
                result = true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return result;
        }
    }
}

