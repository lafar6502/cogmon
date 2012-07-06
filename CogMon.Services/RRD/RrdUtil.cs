using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CogMon.Services.RRD
{
    public class RrdUtil
    {
        public static bool ParseRrdDateRange(string start, string end, out DateTime startDate, out DateTime endDate)
        {
            if (IsRelativeRRDDate(start))
            {
                if (IsRelativeRRDDate(end)) throw new Exception("Both dates can't be relative");
                int v1 = ParseRrdDate(end, null);
                int v2 = ParseRrdDate(start, v1);
                startDate = FromUnixTime(v2);
                endDate = FromUnixTime(v1);
                return true;
            }
            else
            {
                if (IsRelativeRRDDate(start)) throw new Exception("Both dates can't be relative");
                int v1 = ParseRrdDate(start, null);
                int v2 = ParseRrdDate(end, v1);
                startDate = FromUnixTime(v1);
                endDate = FromUnixTime(v2);
                return true;
            }
        }

        public static int ToUnixTime(DateTime dt)
        {
            TimeSpan ts = (dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0));
            return (int)ts.TotalSeconds;
        }

        public static DateTime FromUnixTime(int ut)
        {
            var un = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(ut);
            return un.ToLocalTime();
        }

        public static bool IsRelativeRRDDate(string rrdDate)
        {
            if (rrdDate.StartsWith("s") || rrdDate.StartsWith("e")) return true;
            return false;
        }

        public static int ParseRrdDate(string rrdDate, int? refDate)
        {
            int v;
            if (Int32.TryParse(rrdDate, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out v))
            {
                return v;
            }
            DateTime dt;
            if (DateTime.TryParse(rrdDate, out dt)) return ToUnixTime(dt);

            var re = new Regex(@"((s|e|now|start|end)$|(s|e|now|start|end)?(\+|\-)(\d+)(y|year|years|m|month|months|wk|week|weeks|h|hour|hours|d|day|days)$)");
            var m = re.Match(rrdDate);
            if (!m.Success) throw new Exception("Invalid date format: " + rrdDate);
            if (m.Groups[2].Success)
            {
                if (string.Equals("now", m.Groups[2].Value)) return ToUnixTime(DateTime.Now);
                if (!refDate.HasValue) throw new Exception("Reference date missing");
                return refDate.Value;
            }
            if (m.Groups[3].Success && !string.Equals("now", m.Groups[3].Value))
            {
                if (!refDate.HasValue) throw new Exception("Reference date missing");
            }
            else refDate = ToUnixTime(DateTime.Now);
            var num = (m.Groups[4].Value == "-" ? -1 : 1) * Int32.Parse(m.Groups[5].Value);
            var c = char.ToLower(m.Groups[6].Value[0]);
            dt = FromUnixTime(refDate.Value);
            switch (c)
            {
                case 'h': dt = dt.AddHours(num);
                    break;
                case 'm': dt = dt.AddMonths(num);
                    break;
                case 'w': dt = dt.AddDays(num * 7);
                    break;
                case 'y': dt = dt.AddYears(num);
                    break;
                case 'd': dt = dt.AddDays(num);
                    break;
                default: throw new Exception("invalid date format: " + rrdDate);
            }
            return ToUnixTime(dt);
        }
    }
}
