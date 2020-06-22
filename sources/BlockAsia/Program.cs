using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Compute.v1;
using Google.Apis.Services;
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

        [Option(Description = "Specify the asia block rules count.")]
        public String Count { get; }

        [Option(Description = "Specify the project id for the service account.")]
        public String ProjectId { get; }

        [Option(Description = "Specify the key file path for the service account.")]
        public String KeyPath { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        private ILogger Logger { get; set; }

        /// <summary>
        /// Program information.
        /// </summary>
        private ProgramInfo ProgramInfomation { get; set; }

        /// <summary>
        /// Maximum number of IP addresses that can be added to the block list.
        /// </summary>
        private const int CAN_ADD_MAX_IPADDRESS_COUNT = 250;

        // The entry point executes the command line parser.
        static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        /// <summary>
        /// Main process.
        /// </summary>
        private void OnExecute()
        {
            // Set up a logger provider.
            var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .AddConsole(console => {
                            console.Format = ConsoleLoggerFormat.Default;
                            console.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fffffff] ";
                        });
                }
            );

            // Generate a logger.
            Logger = loggerFactory.CreateLogger<Program>();

            try
            {
                // Obtaining program information.
                ProgramInfomation = new ProgramInfo();

                switch(Execute)
                {
                    case @"list":
                        List();
                        break;
                    case @"create":
                        Create();
                        break;
                    case @"delete":
                        Delete();
                        break;
                    case @"version":
                        Console.Out.WriteLine($"Version is {ProgramInfomation.Version}");
                        break;
                    default:
                        Logger.LogError(@"Can't recognize the command.");
                        break;
                }
            }
            catch(Exception exception)
            {
                Logger.LogError(exception.ToString());
            }
        }

        /// <summary>
        /// Get the list of firewall rules set in the project.
        /// </summary>
        private void List()
        {
            RequiredConfirmation();

            // Creating a Request Service.
            ComputeService computeService = GenerateComputeService(KeyPath);
            FirewallsResource.ListRequest request = computeService.Firewalls.List(ProjectId);

            Data.FirewallList response;

            do
            {
                // Execute a request asynchronously.
                response = request.Execute();

                // If there is no item, skip to the next request.
                if (response.Items == null)
                {
                    continue;
                }

                // Display the list of firewall rules.
                foreach (Data.Firewall firewall in response.Items)
                {
                    // Outputs a list to standard output.
                    Console.Out.WriteLine(JsonConvert.SerializeObject(firewall, Formatting.Indented));
                }

            } while( response.NextPageToken != null );
        }

        /// <summary>
        /// Create firewall rules for blocking IP addresses at once.
        /// </summary>
        private void Create()
        {
            RequiredConfirmation();
            CreateRequiredConfirmation();

            // Get a list of IP addresses.
            List<String> addresses = IpAddresses(Filter);

            int divide = addresses.Count / CAN_ADD_MAX_IPADDRESS_COUNT;
            int remainder = addresses.Count % CAN_ADD_MAX_IPADDRESS_COUNT;
            if(remainder > 0)
            {
                divide += 1;
            }

            // Creating a Request Service.
            ComputeService computeService = GenerateComputeService(KeyPath);

            for(int i = 0; i < divide; i++)
            {
                // Prepare a container for the request data.
                Data.Firewall firewall = new Data.Firewall();

                // Build the request data.
                // Rule Name.
                firewall.Name = $"{RulePrefix}-{String.Format("{0:000}", (i+1))}";

                // All rejected.
                Data.Firewall.DeniedData deniedData = new Data.Firewall.DeniedData();
                deniedData.IPProtocol = "all";
                firewall.Denied = new List<Data.Firewall.DeniedData>(){ deniedData };

                // Priority.
                // It's almost a top priority to fundamentally block it.
                firewall.Priority = 100;

                int start = i * CAN_ADD_MAX_IPADDRESS_COUNT;
                int end = (start + CAN_ADD_MAX_IPADDRESS_COUNT);

                List<String> divIpaddresses = new List<String>();
                for(int j = start; j < end; j++)
                {
                    if(j < addresses.Count)
                    {
                        divIpaddresses.Add(addresses[j]);
                    }
                }

                // Target IP addresses.
                firewall.SourceRanges = new List<string>(divIpaddresses);

                // Create a resource.
                FirewallsResource.InsertRequest request = computeService.Firewalls.Insert(firewall, ProjectId);

                // Sending a request.
                Data.Operation response = request.Execute();

                // Standard output of the results.
                Console.Out.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
            }
        }

        /// <summary>
        /// Batch deletion of firewall rules for blocking IP addresses.
        /// </summary>
        private void Delete()
        {
            RequiredConfirmation();
            DeleteRequiredConfirmation();

            // Creating a Request Service.
            ComputeService computeService = GenerateComputeService(KeyPath);

            for(int i = 1; i <= Int32.Parse(Count); i++){
                String ruleName = $"{RulePrefix}-{String.Format("{0:000}", i)}";

                // Create a resource.
                FirewallsResource.DeleteRequest request = computeService.Firewalls.Delete(ProjectId, ruleName);

                // Sending a request.
                Data.Operation response = request.Execute();

                // Standard output of the results.
                Console.Out.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
            }
        }

        /// <summary>
        /// Confirmation of Required Arguments for Common Commands.
        /// </summary>
        private void RequiredConfirmation()
        {
            if(String.IsNullOrEmpty(KeyPath)) throw new ArgumentException(String.Format("--key-path is required."));
            if(String.IsNullOrEmpty(ProjectId)) throw new ArgumentException(String.Format("--project-id is required."));
        }

        /// <summary>
        /// Confirmation of required arguments for the rule creation command.
        /// </summary>
        private void CreateRequiredConfirmation()
        {
            if(String.IsNullOrEmpty(Filter)) throw new ArgumentException(String.Format("--filter is required."));
            if(String.IsNullOrEmpty(RulePrefix)) throw new ArgumentException(String.Format("--rule-prefix is required."));
        }

        /// <summary>
        /// Confirmation of required arguments of the rule deletion command.
        /// </summary>
        private void DeleteRequiredConfirmation()
        {
            if(String.IsNullOrEmpty(RulePrefix)) throw new ArgumentException(String.Format("--rule-prefix is required."));
            if(String.IsNullOrEmpty(Count)) throw new ArgumentException(String.Format("--count is required."));
        }

        /// <summary>
        /// Returning the IP address list.
        /// </summary>
        /// <param name="path">Full path to the IP address list file to be filtered.</param>
        /// <returns>
        /// List of IP addresses.
        /// </returns>
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
                        // Add only lines whose first character is a number to the list.
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
        /// Returning the ComputeService Client.
        /// </summary>
        /// <param name="key">Full path to an authentication key file.</param>
        /// <returns>
        /// Request Service.
        /// </returns>
        private ComputeService GenerateComputeService(String key)
        {
            // API client initialization.
            BaseClientService.Initializer initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = GoogleCredential.FromFile($"{key}").CreateScoped(ComputeService.Scope.Compute),
                ApplicationName = $"{ProgramInfomation.Product}/{ProgramInfomation.Version}"
            };

            // Initialization of ComputeService.
            ComputeService computeService = new ComputeService(initializer);
            return computeService;
        }
    }
}
