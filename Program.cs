using Microsoft.Win32;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace WeaselSettings
{
    internal static class Program
    {
        private static readonly Mutex Mutex = new(true, "{8F6F0AC4-B9A1-45fd-A8CF-0XF04E6BDE8F}");
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Mutex.WaitOne(TimeSpan.Zero, true))
            {
                string installDir = YamlHelper.GetInstallDir();
                string RimeUserDir = YamlHelper.GetUserDataDir();
                string DataDir = Path.Combine(RimeUserDir, "tables", "custom", "wubi.dict.yaml");
                string FontPath = Path.Combine(RimeUserDir, "font", "zigen.ttf");
                if (File.Exists(DataDir))
                {
                    // 2. 获取当前程序所在的实际目录
                    // AppContext.BaseDirectory 是 AOT 模式下获取路径最可靠的方法
                    string currentDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

                    // 3. 比对路径（忽略大小写）
                    if (!string.Equals(currentDirectory, installDir.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                    {
                        // 如果不在指定目录，弹出提示并退出
                        //MessageBox.Show($"非法运行位置！程序必须在指定目录下运行。\n当前目录：{currentDirectory}",
                        //                "安全检查", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        Environment.Exit(0); // 彻底退出
                        return;
                    }

                    // To customize application configuration such as set high DPI settings or default font,
                    // see https://aka.ms/applicationconfiguration.
                    ApplicationConfiguration.Initialize();
                    Application.Run(new Form1());
                }
                else
                {
                    string zipPath = "./data.zip";
                    using ZipArchive archive = ZipFile.OpenRead(zipPath);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // 拼接完整路径并标准化
                        string fullPath = Path.GetFullPath(Path.Combine(RimeUserDir, entry.FullName));

                        // 安全检查：防止 Zip Slip 攻击
                        if (!fullPath.StartsWith(Path.GetFullPath(RimeUserDir), StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(fullPath);
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                            // overwrite: true 实现覆盖功能
                            entry.ExtractToFile(fullPath, overwrite: true);
                        }
                    }
                    InstallFont(FontPath).GetAwaiter().GetResult();
                    string flagPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".installed");
                    ApplicationConfiguration.Initialize();
                    Application.Run(new Form1());
                    Thread.Sleep(2000); // 等待界面完全加载
                    YamlHelper.RunDeploy();
                }
            }
            else
            {
                ActivateExistingWindow();
                return;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_RESTORE = 9;

        private static void ActivateExistingWindow()
        {
            var current = Process.GetCurrentProcess();

            foreach (var process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id)
                {
                    IntPtr handle = process.MainWindowHandle;

                    if (handle != IntPtr.Zero)
                    {
                        ShowWindow(handle, SW_RESTORE);
                        SetForegroundWindow(handle);
                    }
                    break;
                }
            }
        }


        // 修改 InstallFont 方法为 async Task 以便可以被 await
        public static async Task InstallFont(string sourceFile)
        {
            string fontName = "黑体字根"; // 这里填入字体的真实名称（通常是字体的 Family Name）
            string destinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), Path.GetFileName(sourceFile));

            try
            {
                // 1. 检查字体是否已存在
                if (File.Exists(destinationPath))
                {
                    Console.WriteLine("字体已存在，跳过安装。");
                    return;
                }

                // 2. 复制文件到系统字体目录
                await Task.Run(() => File.Copy(sourceFile, destinationPath, true));

                // 3. 写入注册表（针对所有用户）
                string registryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey, true))
                {
                    // 注意：".ttf" 结尾的条目值通常为文件名，名称为 "字体名 (TrueType)"
                    key.SetValue(fontName + " (TrueType)", Path.GetFileName(sourceFile));
                }

                Console.WriteLine("字体安装成功！");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("错误：请以管理员权限运行此程序。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"安装失败: {ex.Message}");
            }
        }
    }
}