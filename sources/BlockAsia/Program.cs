using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
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
        [Option(Description = "Specify the Project id for the service account.")]
        public String ProjectId { get; }

        [Option(Description = "Specify the key file path for the service account.")]
        public String KeyPath { get; }

        // エントリポイントはコマンドラインパーサーを実行する
        static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        /// <summary>
        /// メイン処理
        /// </summary>
        private void OnExecute()
        {
            try{
                // プログラム情報の取得
                ProgramInfo info = new ProgramInfo();
                // 処理開始
                // requiredConfirmation();

                #if DEBUG
                    Console.WriteLine($"Current directory is {info.CurrentDirectory}");
                    Console.WriteLine($"Product name is {info.Product}");
                    Console.WriteLine($"Execute path is {info.ExecutePath}");
                    Console.WriteLine($"Version is {info.Version}");
                #endif

                // APIクライアントの初期化
                // BaseClientService.Initializer initializer = new BaseClientService.Initializer { 
                //     HttpClientInitializer = GoogleCredential.FromFile($"{KeyPath}").CreateScoped(ComputeService.Scope.Compute),
                //     ApplicationName = $"{info.Product}/{info.Version}"
                // };

                // ComputeServiceの初期化
                // ComputeService computeService = new ComputeService(initializer);

                // Project ID for this request.
                string project = ProjectId;  // TODO: Update placeholder value.

                // ブロックリストの読み込み
                List<String> ipaddresses = new List<String>();
                using (FileStream file = File.OpenRead(@".\krfilter2.txt"))
                {
                    using (StreamReader stream = new StreamReader(file, Encoding.GetEncoding("utf-8")))
                    {
                        // シークを移動させる
                        // file.Seek(2, System.IO.SeekOrigin.Begin);

                        while(stream.Peek() > -1)
                        {
                            ipaddresses.Add(stream.ReadLine());
                        }

                    }
                }

                ipaddresses.ForEach((ip) => {
                    Console.WriteLine(ip);
                });
                // Console.WriteLine(ipaddresses.ToString());

                /*
                FirewallsResource.ListRequest request = computeService.Firewalls.List(project);

                Data.FirewallList response;
                do
                {
                    // To execute asynchronously in an async method, replace `request.Execute()` as shown:
                    response = request.Execute();
                    // response = await request.ExecuteAsync();

                    if (response.Items == null)
                    {
                        continue;
                    }
                    foreach (Data.Firewall firewall in response.Items)
                    {
                        // TODO: Change code below to process each `firewall` resource:
                        Console.WriteLine(JsonConvert.SerializeObject(firewall));
                    }
                    request.PageToken = response.NextPageToken;
                } while (response.NextPageToken != null);
                */

            }catch(ArgumentException error){
                Console.WriteLine(error);
            }catch(Exception error){
                Console.WriteLine(error);
            }
        }

        /// <summary>
        /// 引数必須確認
        /// </summary>
        private void requiredConfirmation()
        {
            if(String.IsNullOrEmpty(ProjectId)) throw new ArgumentException(String.Format("--project-id is required."));
            if(String.IsNullOrEmpty(KeyPath)) throw new ArgumentException(String.Format("--key-path is required."));
        }
    }
}
