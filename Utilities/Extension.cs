using System;



namespace Utilities
{
    public static class Extension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TimeIn"></param>
        /// <param name="Round"> Rounding time to nearest 5 mins</param>
        /// <returns></returns>
        public static DateTime GetRoundTime(this DateTime TimeIn, int Round)
        {
            // Count of round number in this total minutes...
            double CountRound = (TimeIn.Minute / Round);

            // The main formula to calculate round time...
            int Min = (int)Math.Truncate(CountRound + 0.5) * Round;

            //// Show result...
            DateTime dt = new DateTime(TimeIn.Year, TimeIn.Month, TimeIn.Day);
            TimeSpan tRes = new TimeSpan(TimeIn.Hour, Min, 0);
            Console.WriteLine(tRes.ToString());

            //TimeSpan ts = new TimeSpan(DateTime.Now.Hour, Min, 0);

            TimeIn = dt + tRes;

            return TimeIn;
        }
    }
}