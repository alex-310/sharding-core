using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.StreamMerge.ReWrite;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.Internal.Visitors.GroupBys;
using ShardingCore.Core.Internal.Visitors.Selects;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.DbContexts;
using ShardingCore.Sharding.Abstractions;


namespace ShardingCore.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 25 January 2021 11:38:27
* @Email: 326308290@qq.com
*/
    public class StreamMergeContext<T>
    {
        private readonly IShardingParallelDbContextFactory _shardingParallelDbContextFactory;
        //private readonly IShardingScopeFactory _shardingScopeFactory;
        private readonly IQueryable<T> _source;
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IRoutingRuleEngineFactory _tableRoutingRuleEngineFactory;

        private readonly IQueryable<T> _reWriteSource;
        //public IEnumerable<RouteResult> RouteResults { get; }
        //public DataSourceRoutingResult RoutingResult { get; }
        public int? Skip { get;}
        public int? Take { get; }
        public IEnumerable<PropertyOrder> Orders { get;}
        
        public SelectContext SelectContext { get;}
        public GroupByContext GroupByContext { get; }

        public StreamMergeContext(IQueryable<T> source,IShardingDbContext shardingDbContext,IRoutingRuleEngineFactory tableRoutingRuleEngineFactory,
            IShardingParallelDbContextFactory shardingParallelDbContextFactory,IShardingScopeFactory shardingScopeFactory)
        {
            _shardingParallelDbContextFactory = shardingParallelDbContextFactory;
            //_shardingScopeFactory = shardingScopeFactory;
            _source = source;
            _shardingDbContext = shardingDbContext;
            _tableRoutingRuleEngineFactory = tableRoutingRuleEngineFactory;
            var reWriteResult = new ReWriteEngine<T>(source).ReWrite();
            Skip = reWriteResult.Skip;
            Take = reWriteResult.Take;
            Orders = reWriteResult.Orders ?? Enumerable.Empty<PropertyOrder>();
            SelectContext = reWriteResult.SelectContext;
            GroupByContext = reWriteResult.GroupByContext;
            _reWriteSource = reWriteResult.ReWriteQueryable;
        }
        //public StreamMergeContext(IQueryable<T> source,IEnumerable<RouteResult> routeResults,
        //    IShardingParallelDbContextFactory shardingParallelDbContextFactory,IShardingScopeFactory shardingScopeFactory)
        //{
        //    _shardingParallelDbContextFactory = shardingParallelDbContextFactory;
        //    _shardingScopeFactory = shardingScopeFactory;
        //    _source = source;
        //    RouteResults = routeResults;
        //    var reWriteResult = new ReWriteEngine<T>(source).ReWrite();
        //    Skip = reWriteResult.Skip;
        //    Take = reWriteResult.Take;
        //    Orders = reWriteResult.Orders ?? Enumerable.Empty<PropertyOrder>();
        //    SelectContext = reWriteResult.SelectContext;
        //    GroupByContext = reWriteResult.GroupByContext;
        //    _reWriteSource = reWriteResult.ReWriteQueryable;
        //}

        public DbContext CreateDbContext(string tail)
        {
            return _shardingDbContext.GetDbContext(true,tail);
        }
        public IEnumerable<RouteResult> GetRouteResults()
        {
            return _tableRoutingRuleEngineFactory.Route(_source);
        }

        //public ShardingScope CreateScope()
        //{
        //    return _shardingScopeFactory.CreateScope();
        //}

        public IQueryable<T> GetReWriteQueryable()
        {
            return _reWriteSource;
        }
        public IQueryable<T> GetOriginalQueryable()
        {
            return _source;
        }

        public bool HasSkipTake()
        {
            return Skip.HasValue || Take.HasValue;
        }

        public bool HasGroupQuery()
        {
            return this.GroupByContext.GroupExpression != null;
        }

        public bool HasAggregateQuery()
        {
            return this.SelectContext.SelectProperties.Any(o => o.IsAggregateMethod);
        }

    }
}