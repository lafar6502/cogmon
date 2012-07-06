using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CogMon.Services
{
    public class Utils
    {
        private static Regex _re = new Regex(@"\$\{([\w._]+)\}");

        internal class Replacer
        {
            public Dictionary<string, object> Variables { get; set; }
            public string Replace(Match m)
            {
                Capture cval = m.Groups[1].Captures[0];
                string propName = cval.Value;
                if (!Variables.ContainsKey(propName)) throw new Exception("Missing variable: " + propName);
                return Convert.ToString(Variables[propName]);
            }
        }

        public static string SubstValue(string v, Dictionary<string, object> variables)
        {
            string v1;
            string v2 = v;

            var rep = new Replacer { Variables = variables };
            do
            {
                v1 = v2;
                v2 = _re.Replace(v1, new MatchEvaluator(rep.Replace));
            } while (v1 != v2);
            return v2;
        }

        public static T SubstTemplate<T>(string tpl, T defaultValue, Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(tpl)) return defaultValue;
            string sv = Utils.SubstValue(tpl, variables);
            if (string.IsNullOrEmpty(sv)) return defaultValue;
            if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), sv);
            }
            return (T)Convert.ChangeType(sv, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
        }

    }
}
