using ShardingCore.Core;

namespace Sample.SqlServerShardingDataSource.Domain.Entities
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Tuesday, 26 January 2021 12:25:39
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 用户表
    /// </summary>
    public class SysUserMod : IShardingDataSource
    {
        /// <summary>
        /// 用户Id用于分库
        /// </summary>
        [ShardingDataSourceKey]
        public string Id { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 用户姓名
        /// </summary>
        public int Age { get; set; }
        public int AgeGroup { get; set; }
    }
}