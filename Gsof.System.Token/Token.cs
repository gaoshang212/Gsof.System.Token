using Gsof.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Gsof.System.Token
{
    public class Token
    {
        private static void SaveToken(string p_token)
        {
            var filename = PathExtension.GetHome(".token", "token");
            var dir = Path.GetDirectoryName(filename);

            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(filename, p_token);
        }

        private static string? GetToken()
        {
            var filename = PathExtension.GetHome(".token", "token");
            if (!File.Exists(filename))
            {
                return null;
            }

            return File.ReadAllText(filename);
        }

        public static Task<string> GetSystemToken(bool useCache = true)
        {
            var token = Environment.GetEnvironmentVariable("SYSTEMTOKEN");
            if (!string.IsNullOrEmpty(token))
            {
                return Extensions.TaskExtensions.FromResult(token);
            }

            var tcs = new TaskCompletionSource<string>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (useCache)
                    {
                        token = GetToken();
                    }
                    if (!string.IsNullOrEmpty(token))
                    {
                        tcs.SetResult(token);
                        return;
                    }

                    //var task = ProcessEx.Exec(@"dmidecode.exe", "-s system-uuid", 1000);
                    //Task.WaitAll(task);
                    //var result = task.Result.Trim();
                    var result = SMBios.UUID();
                    if (string.IsNullOrEmpty(result))
                    {
                        result = Guid.NewGuid().ToString().ToUpper();
                    }

                    if (!string.IsNullOrEmpty(result))
                    {
                        SaveToken(result);
                    }

                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            return tcs.Task;
        }
    }
}
