// ******************************************************************************************
// * 文件名：RedisEnum.cs
// * 功能描述：
// * 创建人：wangjun
// * 创建日期：2016年09月21日 8:36
// *******************************************************************************************

using System;

namespace AxCRL.Comm.Redis
{
    /// <summary>
    /// Additional options for the MIGRATE command
    /// </summary>
    [Flags]
    public enum RedisMigrateOptions
    {
        /// <summary>
        /// No options specified
        /// </summary>
        None = 0,
        /// <summary>
        /// Do not remove the key from the local instance.
        /// </summary>
        Copy = 1,
        /// <summary>
        /// Replace existing key on the remote instance.
        /// </summary>
        Replace = 2
    }

    /// <summary>
    /// Behaviour markers associated with a given command
    /// </summary>
    [Flags]
    public enum RedisCommandFlags
    {
        /// <summary>
        /// Default behaviour.
        /// </summary>
        None = 0,
        /// <summary>
        /// This command may jump regular-priority commands that have not yet been written to the redis stream.
        /// </summary>
        HighPriority = 1,
        /// <summary>
        /// The caller is not interested in the result; the caller will immediately receive a default-value
        /// of the expected return type (this value is not indicative of anything at the server).
        /// </summary>
        FireAndForget = 2,


        /// <summary>
        /// This operation should be performed on the master if it is available, but read operations may
        /// be performed on a slave if no master is available. This is the default option.
        /// </summary>
        PreferMaster = 0,

        /// <summary>
        /// This operation should only be performed on the master.
        /// </summary>
        DemandMaster = 4,

        /// <summary>
        /// This operation should be performed on the slave if it is available, but will be performed on
        /// a master if no slaves are available. Suitable for read operations only.
        /// </summary>
        PreferSlave = 8,

        /// <summary>
        /// This operation should only be performed on a slave. Suitable for read operations only.
        /// </summary>
        DemandSlave = 12,

        // 16: reserved for additional "demand/prefer" options

        // 32: used for "asking" flag; never user-specified, so not visible on the public API

        /// <summary>
        /// Indicates that this operation should not be forwarded to other servers as a result of an ASK or MOVED response
        /// </summary>
        NoRedirect = 64,

        // 128: used for "internal call"; never user-specified, so not visible on the public API

        // 256: used for "retry"; never user-specified, so not visible on the public API
    }

    /// <summary>
    /// Indicates when this operation should be performed (only some variations are legal in a given context)
    /// </summary>
    public enum RedisWhen
    {
        /// <summary>
        /// The operation should occur whether or not there is an existing value 
        /// </summary>
        Always,
        /// <summary>
        /// The operation should only occur when there is an existing value 
        /// </summary>
        Exists,
        /// <summary>
        /// The operation should only occur when there is not an existing value 
        /// </summary>
        NotExists
    }

    /// <summary>
    /// The direction in which to sequence elements
    /// </summary>
    public enum RedisOrder
    {
        /// <summary>
        /// Ordered from low values to high values
        /// </summary>
        Ascending,
        /// <summary>
        /// Ordered from high values to low values
        /// </summary>
        Descending
    }

    /// <summary>
    /// Describes an algebraic set operation that can be performed to combine multiple sets
    /// </summary>
    public enum RedisSetOperation
    {
        /// <summary>
        /// Returns the members of the set resulting from the union of all the given sets.
        /// </summary>
        Union,
        /// <summary>
        /// Returns the members of the set resulting from the intersection of all the given sets.
        /// </summary>
        Intersect,
        /// <summary>
        /// Returns the members of the set resulting from the difference between the first set and all the successive sets.
        /// </summary>
        Difference
    }

    /// <summary>
    /// Specifies how to compare elements for sorting
    /// </summary>
    public enum RedisSortType
    {
        /// <summary>
        /// Elements are interpreted as a double-precision floating point number and sorted numerically
        /// </summary>
        Numeric,
        /// <summary>
        /// Elements are sorted using their alphabetic form (Redis is UTF-8 aware as long as the !LC_COLLATE environment variable is set at the server)
        /// </summary>
        Alphabetic
    }

    /// <summary>
    /// Specifies how elements should be aggregated when combining sorted sets
    /// </summary>
    public enum RedisAggregate
    {
        /// <summary>
        /// The values of the combined elements are added
        /// </summary>
        Sum,
        /// <summary>
        /// The least value of the combined elements is used
        /// </summary>
        Min,
        /// <summary>
        /// The greatest value of the combined elements is used
        /// </summary>
        Max
    }

    /// <summary>
    /// When performing a range query, by default the start / stop limits are inclusive;
    /// however, both can also be specified separately as exclusive
    /// </summary>
    [Flags]
    public enum RedisExclude
    {
        /// <summary>
        /// Both start and stop are inclusive
        /// </summary>
        None = 0,
        /// <summary>
        /// Start is exclusive, stop is inclusive
        /// </summary>
        Start = 1,
        /// <summary>
        /// Start is inclusive, stop is exclusive
        /// </summary>
        Stop = 2,
        /// <summary>
        /// Both start and stop are exclusive
        /// </summary>
        Both = Start | Stop
    }

    /// <summary>
    /// <a href="http://en.wikipedia.org/wiki/Bitwise_operation">Bitwise operators</a>
    /// </summary>
    public enum RedisBitwise
    {
        /// <summary>
        /// <a href="http://en.wikipedia.org/wiki/Bitwise_operation#AND">And</a>
        /// </summary>
        And,
        /// <summary>
        /// <a href="http://en.wikipedia.org/wiki/Bitwise_operation#OR">Or</a>
        /// </summary>
        Or,
        /// <summary>
        /// <a href="http://en.wikipedia.org/wiki/Bitwise_operation#XOR">Xor</a>
        /// </summary>
        Xor,
        /// <summary>
        /// <a href="http://en.wikipedia.org/wiki/Bitwise_operation#NOT">Not</a>
        /// </summary>
        Not
    }
}