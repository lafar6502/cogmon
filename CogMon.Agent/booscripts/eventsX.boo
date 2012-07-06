import System
import CogMon.Agent

job as JobBase = InputParams.Job

lid = job.GetJobPersistValue of int("lastid", 0)
log.Info("Last id is ${lid}")
sql_select("atmodb", "select id from Activity where id>${lid} order by id", {at|
    #print("${at.id} => ${at.name}");
	lid = at.id
})
log.Info("Last id is ${lid}")
job.SetJobPersistValue("lastid", lid)
add_data_record(1, 3, 17)