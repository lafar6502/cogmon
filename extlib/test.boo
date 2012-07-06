import System
import System.Text.RegularExpressions

re = Regex("""((s|e|now|start|end)$|(s|e|now|start|end)?(\+|\-)(\d+)(y|year|years|m|month|months|wk|week|weeks|h|hour|hours|d|day|days)$)""")

sis = "-2d"
m = re.Match(sis)
print("${m.Success} ${m.Groups[1].Value} ${m.Groups[2].Value} ${m.Groups[3].Value} ${m.Groups[4].Value}")
for i in range(0, m.Groups.Count):
	print "${i}:  ${m.Groups[i].Success} ${m.Groups[i].Value}"
	