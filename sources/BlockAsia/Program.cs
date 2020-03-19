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
            // 処理開始
            requiredConfirmation();
            Console.WriteLine("Hello,World");
        }

        /// <summary>
        /// 引数必須確認
        /// </summary>
        private void requiredConfirmation()
        {
        }

    }
}
