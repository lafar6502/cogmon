import System
import CogMon.Agent

job as JobBase = InputParams.Job
mq_table as string = job.Options["mq_table"]
cstring as string = job.Options["cstring"]
sd = job.GetJobPersistValue of DateTime("lastquery", DateTime.Now)

raise "missing mq_table option" if string.IsNullOrEmpty(mq_table)
raise "missing cstring" if string.IsNullOrEmpty(cstring)

sql = """
	declare @dstart datetime
	set @dstart = '${sd.ToString('yyyy-MM-dd HH:mm:ss')}'
	select queue_length, new_messages, errors, avg_latency_ms, messages_handled
	from 
	(
	select COUNT(*) as queue_length
	from ${mq_table} with(nolock) where subqueue='I'
	) t0,
	(
	select COUNT(*) as new_messages
	from ${mq_table} with(nolock) where retry_time >= @dstart
	) t1,
	(
	select COUNT(*) as errors from ${mq_table} with(nolock)
	where 
	retry_time >= @dstart and subqueue='R' and retry_count > 0
	) t2,
	(
	select coalesce(avg(DATEDIFF(millisecond, retry_time, last_processed)), 0) 
	as avg_latency_ms,
	COUNT(*) as messages_handled
	from ${mq_table} with(nolock) where retry_time >= @dstart and subqueue='X'
	) t3
"""

sql_select(cstring, sql, {r|
	add_data_record({"queue_length": r.queue_length, "new_messages" : r.new_messages, "errors": r.errors, "avg_latency_ms": r.avg_latency_ms, "messages_handled":r.messages_handled})
})

job.SetJobPersistValue("lastquery", DateTime.Now)

