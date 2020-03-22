using System;
using McMaster.Extensions.CommandLineUtils;

namespace BlockAsia
{
    class Program
    {
        [Option(Description = "Specify the client id for the service account.")]
        public String ClientId { get; }

        [Option(Description = "Specify the client secret for the service account.")]
        public String Secret { get; }

        [Option(Description = "Specify the project id for the firewall installation project.")]
        public String ProjectId { get; }

        [Option(Description = "Specify the IP address list file to block.")]
        public String FilePath { get; }

        // エントリポイントはコマンドラインパーサーを実行する
        static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        /// <summary>
        /// メイン処理
        /// </summary>
        private void OnExecute()
        {
            try{
                // 処理開始
                requiredConfirmation();
                #if DEBUG
                    Console.WriteLine($"ClientdId is {ClientId}");
                    Console.WriteLine($"Secret is {Secret}");
                    Console.WriteLine($"ProjectId is {ProjectId}");
                    Console.WriteLine($"FilePath is {FilePath}");
                #endif
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
            if(String.IsNullOrEmpty(ClientId)) throw new ArgumentException(String.Format("--client-id is required."));
            if(String.IsNullOrEmpty(Secret)) throw new ArgumentException(String.Format("--secret is required."));
            if(String.IsNullOrEmpty(ProjectId)) throw new ArgumentException(String.Format("--project-id is required."));
            if(String.IsNullOrEmpty(FilePath)) throw new ArgumentException(String.Format("--file-path is required."));
        }

    }
}
