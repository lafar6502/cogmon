import System
import CogMon.Agent
import System.Net
import System.Text.RegularExpressions

job as JobBase = InputParams.Job

dv = 45000000
lid = job.GetJobPersistValue of int("lastid", dv)
log.Info("Last id is ${lid}")

q = "DocumentType:* AND Id:[${lid} TO *]"
baseuri = "http://192.168.1.74:9070/apache-solr-3.6.0/core0/select/"
uri = "${baseuri}?q=${Uri.EscapeDataString(q)}&sort=Id+desc&rows=1&facet=true&facet.field=DocumentType&facet.mincount=1&indent=on"
log.Info("Uri is ${uri}")
txt = http_get_text(uri)
log.Info("Response: ${txt}")

m1 = /CRQIncident#(\d+)/.Match(txt)
if m1.Success:
	dc = {
		"UM01": .0,
		"UM04" : .0,
		"UM13" : .0,
		"RO09" : .0,
		"PI02" : .0,
		"PI03" : .0,
		"WN01" : .0,
		"WN02" : .0,
		"WN04" : .0,
		"RZ01" : .0,
		"RZ02" : .0,
		"OTHER" : .0
	}
	log.Info("lid: {0}", m1.Groups[1].Value)
	nlid = Int32.Parse(m1.Groups[1].Value)
	if lid == dv:
		log.Info("first iter")
	else:
		mt = Regex.Match(txt, "numFound=\"(\\d+)\"")
		oth = 0
		if mt.Success:
			oth = double.Parse(mt.Groups[1].Value)
		for k in List of string(dc.Keys):
			mt = Regex.Match(txt, "<int name=\"${k}\">(\\d+)")
			if mt.Success:
				log.Info("Success {0}: {1}", k, mt.Groups[1].Value)
				v = double.Parse(mt.Groups[1].Value)
				oth -= v
				dc[k] = v 
		dc["OTHER"] = oth if oth >= 0
		log.Info("Ret: {0}", dc)
		add_data_record(dc)
		
	lid = nlid
	job.SetJobPersistValue("lastid", lid)

