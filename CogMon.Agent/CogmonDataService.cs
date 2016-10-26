using CogMon.Lib;
using NGinnBPM.MessageBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Agent
{
    public class CogmonDataService : ITimeSeriesDatabase
    {
        public ServiceClient CogMon { get; set; }

        protected void UpdateDataSource(DataRecord dr)
        {
            CogMon.CallService<string>(new UpdateData
            {
                Data = dr
            });

        }

        public void UpdateDataSource(IEnumerable<DataRecord> batch)
        {
            if (batch == null || batch.Count() == 0) return;
            if (batch.Count() == 1)
            {
                this.UpdateDataSource(batch.First());
            }
            else
            {
                CogMon.CallService<string>(new UpdateDataBatch
                {
                    Data = batch.ToArray()
                });
            }
        }
    }
}
