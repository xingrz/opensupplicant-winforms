namespace OpenSupplicant
{
    public enum AmtiumStatus
    {
        /// <summary>
        /// 初始化完成
        /// </summary>
        Initialized,

        /// <summary>
        /// 空闲
        /// </summary>
        Free,

        /// <summary>
        /// 正在登录
        /// </summary>
        LoggingIn,

        /// <summary>
        /// 正在注销
        /// </summary>
        LoggingOut,

        /// <summary>
        /// 正在发送状态
        /// </summary>
        Polling,

        /// <summary>
        /// 已断开
        /// </summary>
        Disconnected,

        /// <summary>
        /// 正在等待返回认证服务器IP
        /// </summary>
        RecivingServerIp,

        /// <summary>
        /// 正在等待返回接入服务列表
        /// </summary>
        RecivingServices,

        /// <summary>
        /// 正在关闭
        /// </summary>
        Closing
    }
}
