using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization;
using CogMon.Services.Dao;
using CogMon.Lib.Gui;
using CogMon.Lib.Scheduling;
using CogMon.Lib.DataSeries;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using CogMon.Lib;
using CogMon.Lib.Graph;
using MongoDB.Driver;

namespace CogMon.Services.Database
{
    public class DatabaseInit
    {
        public static void Configure()
        {
            DefaultWithStringId<UserInfo>();
            DefaultWithStringId<GroupInfo>();
            DefaultWithStringId<PortalPage>();
            DefaultWithStringId<DataSourceTemplate>();
            DefaultWithStringId<ScheduledJob>();
            DefaultWithStringId<DataSourceCreationInfo>();
            DefaultWithStringId<NavDirectory>();
            DefaultWithStringId<EventInfo>();
            DefaultWithStringId<PortletDef>();
            DefaultWithStringId<EventCategory>();
            DefaultWithIntIdAssigned<MEvent>(null);
            DefaultWithStringId<EventMapReduce>();
            DefaultWithStringId<GraphDefinition>();
            DefaultWithStringId<NavNode>();
            //DefaultWithStringId<AuthToken>();
        }

        private static void DefaultWithStringId<T>()
        {
            DefaultWithStringId<T>(null);
        }

        private static void DefaultWithStringId<T>(Action<BsonClassMap<T>> act)
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(T))) return;
            BsonClassMap.RegisterClassMap<T>(m =>
            {
                m.AutoMap();
                m.SetIdMember(m.GetMemberMap("Id"));
                m.IdMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);
                if (act != null) act(m);
            });
        }

        private static void DefaultWithIntIdAssigned<T>(Action<BsonClassMap<T>> act)
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(T))) return;
            BsonClassMap.RegisterClassMap<T>(m =>
            {
                m.AutoMap();
                m.SetIdMember(m.GetMemberMap("Id"));
                m.IdMemberMap.SetIdGenerator(null);
                if (act != null) act(m);
            });
        }

        public static void InitializeAuthTokenDatabase(MongoDatabase db)
        {
            if (!db.CollectionExists("authtokens")) db.CreateCollection("authtokens", MongoDB.Driver.Builders.CollectionOptions.SetCapped(true).SetMaxDocuments(1000).SetMaxSize(100 * 1000).SetAutoIndexId(true));
        }

        public static void InitializeCogMonDatabase(MongoDatabase db)
        {
            InitializeAuthTokenDatabase(db);
        }

    }
}
