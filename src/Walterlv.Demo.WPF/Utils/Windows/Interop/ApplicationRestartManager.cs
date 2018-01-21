using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using Walterlv.Annotations;

namespace Walterlv.Demo.Interop
{
    /// <summary>
    /// 为应用程序提供重启后恢复状态的功能。
    /// </summary>
    public class ApplicationRestartManager
    {
        /// <summary>
        /// 获取一个值，该值指示当前操作系统环境是否支持 Restart Manager 功能。
        /// Restart Manager 是 Windows Vista 新增的功能。在 Windows 10 秋季创意者更新之后，默认开启了 EWX_RESTARTAPPS。
        /// </summary>
        public static bool IsRestartManagerSupported => IsRestartManagerSupportedLazy.Value;

        /// <summary>
        /// 向操作系统的 Restart Manager 注册应用终止后的重启方式。
        /// </summary>
        /// <param name="pwsCommandLine">
        /// 应用程序的重启时应该使用的参数，允许为 null，表示不带参数。
        /// 请注意：如果命令行参数中的某一个参数包含空格，请加上引号。
        /// </param>
        /// <param name="dwFlags">为应用程序的重启行为添加限制，默认没有限制。</param>
        /// <returns></returns>
        public static bool RegisterApplicationRestart(
            [CanBeNull] string pwsCommandLine,
            ApplicationRestartFlags dwFlags = ApplicationRestartFlags.None)
        {
            return 0 == RegisterApplicationRestart(pwsCommandLine, (int)dwFlags);
        }

        /// <summary>
        /// Windows Vista 及以上才开启 Restart Manager。
        /// </summary>
        [ContractPublicPropertyName(nameof(IsRestartManagerSupported))]
        private static readonly Lazy<bool> IsRestartManagerSupportedLazy =
            new Lazy<bool>(() => Environment.OSVersion.Version >= new Version(6, 0));

        /// <summary>
        /// Registers the active instance of an application for restart.
        /// See also: [RegisterApplicationRestart function (Windows)](https://msdn.microsoft.com/en-us/library/windows/desktop/aa373347)
        /// </summary>
        /// <param name="pwzCommandline">
        /// A pointer to a Unicode string that specifies the command-line arguments for the application when it is restarted. The maximum size of the command line that you can specify is RESTART_MAX_CMD_LINE characters. Do not include the name of the executable in the command line; this function adds it for you.
        /// If this parameter is NULL or an empty string, the previously registered command line is removed. If the argument contains spaces, use quotes around the argument.
        /// </param>
        /// <param name="dwFlags">
        /// This parameter can be 0 or one or more of the following values:
        /// - 1: Do not restart the process if it terminates due to an unhandled exception.
        /// - 2: Do not restart the process if it terminates due to the application not responding.
        /// - 4: Do not restart the process if it terminates due to the installation of an update.
        /// - 8: Do not restart the process if the computer is restarted as the result of an update.
        /// </param>
        /// <returns>
        /// This function returns S_OK on success or one of the following error codes.
        /// - E_FAIL: Internal error.
        /// - E_INVALIDARG: The specified command line is too long.
        /// </returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint RegisterApplicationRestart(string pwzCommandline, int dwFlags);
    }

    /// <summary>
    /// 表示重启时的限制标记。
    /// </summary>
    [Flags]
    public enum ApplicationRestartFlags
    {
        /// <summary>
        /// 没有重启限制。如果仅指定 <see cref="None"/>，那么操作系统在可以重启应用程序的时候都会重启应用。
        /// </summary>
        None = 0,

        /// <summary>
        /// 指定此时不重启：因未处理的异常而导致进程停止工作。
        /// </summary>
        RestartNoCrash = 1,

        /// <summary>
        /// 指定此时不重启：因应用程序无响应而导致进程停止工作。
        /// </summary>
        RestartNoHang = 2,

        /// <summary>
        /// 指定此时不重启：因应用安装更新导致进程关闭。
        /// </summary>
        RestartNoPatch = 4,

        /// <summary>
        /// 指定此时不重启：因操作系统安装更新后重启导致进程关闭。
        /// </summary>
        RestartNoReboot = 8
    }
}
