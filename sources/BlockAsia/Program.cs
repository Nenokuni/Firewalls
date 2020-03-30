using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Compute.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;


using Data = Google.Apis.Compute.v1.Data;

namespace BlockAsia
{
    class ProgramInfo
    {
        public String CurrentDirectory { get; }
        public String ApplicationPath { get; }
        public String Version { get; }
        public String Product { get; }
        public String ExecutePath { get; }

        public ProgramInfo()
        {
            this.CurrentDirectory = System.Environment.CurrentDirectory;
            this.ApplicationPath = System.Reflection.Assembly.GetEntryAssembly().Location;

            System.Diagnostics.FileVersionInfo application = System.Diagnostics.FileVersionInfo.GetVersionInfo(this.ApplicationPath);
            this.Version = application.FileVersion;
            this.Product = application.ProductName;
            this.ExecutePath = System.IO.Path.GetDirectoryName(this.ApplicationPath);

        }
    }

    class Program
    {
        [Argument(0)]
        [Required]
        public String Execute { get; }

        [Option(Description = "Specify the ipaddress list file path.")]
        public String Filter { get; }

        [Option(Description = "Specify the rule prifix name.")]
        public String RulePrefix { get; }

        [Option(Description = "Specify the project id for the service account.")]
        [Required]
        public String ProjectId { get; }

        [Option(Description = "Specify the key file path for the service account.")]
        [Required]
        public String KeyPath { get; }

        /// <summary>
        /// ロガー
        /// </summary>
        private ILogger Logger { get; set; }

        private ProgramInfo ProgramInfomation { get; set; }

        // エントリポイントはコマンドラインパーサーを実行する
        static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        /// <summary>
        /// メイン処理
        /// </summary>
        private void OnExecute()
        {
            try
            {
                // ロガーのプロバイダーを設定
                var loggerFactory = LoggerFactory.Create(builder =>
                    {
                        builder
                            .AddConsole(console => {
                                console.Format = ConsoleLoggerFormat.Default;
                                console.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fffffff] ";
                            });
                    }
                );

                // ロガーを生成
                Logger = loggerFactory.CreateLogger<Program>();

                // プログラム情報の取得
                ProgramInfomation = new ProgramInfo();

                #if DEBUG
                Logger.LogInformation($"Current directory is {ProgramInfomation.CurrentDirectory}");
                Logger.LogInformation($"Product name is {ProgramInfomation.Product}");
                Logger.LogInformation($"Execute path is {ProgramInfomation.ExecutePath}");
                Logger.LogInformation($"Version is {ProgramInfomation.Version}");
                #endif

                switch(Execute)
                {
                    case @"list":
                        List();
                        break;
                    case @"create":
                        Create();
                        break;
                    default:
                        Console.WriteLine(@"Can't recognize the command.");
                        break;
                }
            }
            catch(Exception exception)
            {
                Logger.LogError(exception.ToString());
            }
        }

        /// <summary>
        /// プロジェクトに設定されたファイアウォールルール一覧を取得する
        /// </summary>
        private void List()
        {
            ComputeService computeService = GenerateComputeService(KeyPath);
            FirewallsResource.ListRequest request = computeService.Firewalls.List(ProjectId);

            Data.FirewallList response;

            do
            {
                // 非同期でリクエストを実行.
                response = request.Execute();

                // アイテムがなければ次のリクエストへ飛ばす
                if (response.Items == null)
                {
                    continue;
                }

                // ファイアウォールルール一覧を表示する
                foreach (Data.Firewall firewall in response.Items)
                {
                    // 標準出力に一覧を出力
                    Console.Out.WriteLine(JsonConvert.SerializeObject(firewall, Formatting.Indented));
                }

            } while( response.NextPageToken != null );
        }

        /// <summary>
        /// ブロックするIPアドレスのファイアウォールルールを一括して作成する
        /// </summary>
        private void Create()
        {
            // コマンド実行に必要な引数確認
            requiredConfirmation();

            Console.WriteLine("hello, world.");
            /*
            IpAddresses(Filter).ForEach((ip) =>
            {
                Console.WriteLine(ip);
            });
            */
        }

        /// <summary>
        /// 引数必須確認
        /// </summary>
        private void requiredConfirmation()
        {
            if(String.IsNullOrEmpty(Filter)) throw new ArgumentException(String.Format("--filter is required."));
            if(String.IsNullOrEmpty(RulePrefix)) throw new ArgumentException(String.Format("--rule-prefix is required."));
        }

        /// <summary>
        /// IPアドレス一覧を返却する
        /// </summary>
        private List<String> IpAddresses(String path)
        {
            List<String> ipaddresses = new List<String>();

            using (FileStream file = File.OpenRead(path))
            {
                using (StreamReader stream = new StreamReader(file, Encoding.GetEncoding("utf-8")))
                {
                    while(stream.Peek() > -1)
                    {
                        String line = stream.ReadLine();
                        // 先頭文字が数値の行のみリストに追加する
                        if(System.Text.RegularExpressions.Regex.IsMatch(line, @"^[0-9]+"))
                        {
                            ipaddresses.Add(line);
                        }
                    }
                }
            }

            return ipaddresses;
        }

        /// <summary>
        /// ComputeServiceクライアントを返却する
        /// </summary>
        private ComputeService GenerateComputeService(String key)
        {
            // APIクライアントの初期化
            BaseClientService.Initializer initializer = new BaseClientService.Initializer
            { 
                HttpClientInitializer = GoogleCredential.FromFile($"{key}").CreateScoped(ComputeService.Scope.Compute),
                ApplicationName = $"{ProgramInfomation.Product}/{ProgramInfomation.Version}"
            };

            // ComputeServiceの初期化
            ComputeService computeService = new ComputeService(initializer);
            return computeService;
        }
    }
}
