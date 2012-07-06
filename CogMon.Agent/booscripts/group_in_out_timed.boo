import System
import CogMon.Agent

job as JobBase = InputParams.Job
gid = Convert.ToInt32(job.Arguments) #group Id...
sd = job.GetJobPersistValue of DateTime("lastquery", DateTime.Now)
ed = DateTime.Now

log.Info("start date is ${sd}, group: ${gid}")

sql = """
	declare @gid int
	declare @dstart datetime
	declare @dend datetime

	set @gid = ${gid}
	set @dstart = '${sd.ToString('yyyy-MM-dd HH:mm:ss')}'
	set @dend = '${ed.ToString('yyyy-MM-dd HH:mm:ss')}'

	select incoming_cnt, all_out_cnt, closed_cnt, fwd_cnt, avg_lifetime_work, avg_lifetime_24
	from
	(select count(*) as incoming_cnt from timedparameter with(nolock)
	where definition = 200
	and value=@gid
	and start_date > @dstart and start_date <= @dend) t1
	,
	(select 
	count(*) as all_out_cnt,
	sum(case when crq.assignee_group = tp.value and crq.status = 2011 then 1 else 0 end) as closed_cnt,
	sum(case when not (crq.assignee_group = tp.value and crq.status = 2011) then 1 else 0 end) as fwd_cnt,
	avg(tp.lifetime_work) as avg_lifetime_work,
	avg(tp.lifetime_24) as avg_lifetime_24
	from timedparameter tp
	left join crqincident crq on crq.id = tp.object_id
	where definition = 200
	and value=@gid
	and end_date > @dstart and end_date <= @dend) t2
"""
sql_select("atmodb", sql, {r|
	add_data_record({"incoming_cnt": r.incoming_cnt, "all_out_cnt" : r.all_out_cnt, "closed_cnt": r.closed_cnt, "fwd_cnt": r.fwd_cnt, "avg_lifetime_work":(Double.NaN if r.all_out_cnt == 0 else Convert.ToDouble(r.avg_lifetime_work)), "avg_lifetime_24":(Double.NaN if r.all_out_cnt == 0 else Convert.ToDouble(r.avg_lifetime_24))})
})

job.SetJobPersistValue("lastquery", ed)

