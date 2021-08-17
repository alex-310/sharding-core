﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 14:22:10
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractInMemoryAsyncStreamMergeEngine<T>
    {
        private readonly StreamMergeContext<T> _mergeContext;

        public AbstractInMemoryAsyncStreamMergeEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
        }

        public async Task<List<TResult>> ExecuteAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery,CancellationToken cancellationToken = new CancellationToken())
        {
            var tableResult = _mergeContext.GetRouteResults();
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                if (routeResult.ReplaceTables.Count > 1)
                    throw new ShardingCoreException("route found more than 1 table name s");
                var tail = string.Empty;
                if (routeResult.ReplaceTables.Count == 1)
                    tail = routeResult.ReplaceTables.First().Tail;

                return Task.Run(async () =>
                {
                    try
                    {
                        //using (var scope = _mergeContext.CreateScope())
                        //{
                        //var shardingContext = ShardingContext.Create(routeResult);
                        //scope.ShardingAccessor.ShardingContext = shardingContext;

                        var shardingDbContext = _mergeContext.CreateDbContext(tail);
                        var newQueryable = (IQueryable<T>)_mergeContext.GetReWriteQueryable()
                                .ReplaceDbContextQueryable(shardingDbContext);
                        var query = await efQuery(newQueryable);
                        return query;
                        //}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
            }).ToArray();

            return  (await Task.WhenAll(enumeratorTasks)).ToList();
        }
    }
}