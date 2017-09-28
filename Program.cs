using CamundaClient;
using CamundaClient.Dto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace FlowingTripBookingSaga
{
    class Program
    {

        private static string logo =
                "   ____                                _         ____  ____  __  __ \n" +
                "  / ___|__ _ _ __ ___  _   _ _ __   __| | __ _  | __ )|  _ \\|  \\/  |\n" +
                " | |   / _` | '_ ` _ \\| | | | '_ \\ / _` |/ _` | |  _ \\| |_) | |\\/| |\n" +
                " | |__| (_| | | | | | | |_| | | | | (_| | (_| | | |_) |  __/| |  | |\n" +
                "  \\____\\__,_|_| |_| |_|\\__,_|_| |_|\\__,_|\\__,_| |____/|_|   |_|  |_|\n";

        private static int pollingIntervalInMilliseconds = 5;
        private static int pollingNumberOfTasks = 100;
        private static int pollingLockTimeInMs = 5 * 60 * 1000;
        private static int pollingMaxDegreeOfParallelism = 1;
        private static Timer pollingTimer;

        private static string workerId = "worker1";
        private static IDictionary<string, Action<ExternalTask>> workers = new Dictionary<string, Action<ExternalTask>>();
        private static CamundaEngineClient camunda;

        private static void Main(string[] args)
        {

            Console.WriteLine(logo + "\n\n" + "Deploying models and start workers.\n\nPRESS ANY KEY TO STOP WORKERS.\n\n");

            camunda = new CamundaEngineClient(
                new System.Uri("http://localhost:8080/engine-rest/engine/default/"), null, null);

            // Alternative way of doing it: Search assembly and automatically deploy all models to Camunda and start all found workers
            // camunda.Startup();

            DeployModel();
            RegisterWorker();

            // start some instances:
            for (int i = 0; i < 1; i++)
            {
                string processInstanceId = camunda.BpmnWorkflowService.StartProcessInstance("FlowingTripBookingSaga", new Dictionary<string, object>()
                    {
                        {"someBookingData", "..." }
                    });
                Console.WriteLine("Started trip booking saga " + processInstanceId);
            }

            Console.ReadLine(); // wait for ANY KEY
            camunda.Shutdown(); // Stop Task Workers
        }

        private static void DeployModel() { 
            camunda.RepositoryService.Deploy("trip-booking", new List<object> {
                FileParameter.FromManifestResource(Assembly.GetExecutingAssembly(), "FlowingTripBookingSaga.Models.FlowingTripBookingSaga.bpmn") });
        }

        private static void RegisterWorker()
        {
            registerWorker("book-hotel", externalTask => {
                Console.WriteLine("Book hotel now..."); // e.g. by calling a REST endpoint
                camunda.ExternalTaskService.Complete(workerId, externalTask.Id);
            });
            registerWorker("cancel-hotel", externalTask => {
                Console.WriteLine("Cancelling hotel now...");
                camunda.ExternalTaskService.Complete(workerId, externalTask.Id);
            });
            registerWorker("book-car", externalTask => {
                Console.WriteLine("Book car now..."); 
                //camunda.ExternalTaskService.Failure(workerId, externalTask.Id, "could not connect", (externalTask.Retries ?? 3) - 1, 5000);
            });
            registerWorker("cancel-car", externalTask => {
                Console.WriteLine("Cancelling car now...");
                camunda.ExternalTaskService.Complete(workerId, externalTask.Id);
            });       
            registerWorker("book-flight", externalTask => {
                Console.WriteLine("Reserving flight now...");
                camunda.ExternalTaskService.Complete(workerId, externalTask.Id);
                //Console.WriteLine("Oh no - we hit a glitch!");
                //camunda.ExternalTaskService.Error(workerId, externalTask.Id, "BookingFailed");
            });
            registerWorker("cancel-flight", externalTask => {
                Console.WriteLine("Cancelling flight now...");
                camunda.ExternalTaskService.Complete(workerId, externalTask.Id);
            });

            StartPolling();
        }

        private static void StartPolling()
        {
            pollingTimer = new Timer(_ => PollTasks(), null, pollingIntervalInMilliseconds, Timeout.Infinite);
        }

        private static void PollTasks()
        {
            var tasks = camunda.ExternalTaskService.FetchAndLockTasks(workerId, pollingNumberOfTasks, workers.Keys, pollingLockTimeInMs, null);
            Parallel.ForEach(
                tasks, 
                new ParallelOptions { MaxDegreeOfParallelism = pollingMaxDegreeOfParallelism },
                (externalTask) => {
                    workers[externalTask.TopicName](externalTask);
                });

            // schedule next run
            pollingTimer.Change(pollingIntervalInMilliseconds, Timeout.Infinite);
        }

        private static void registerWorker(string topicName, Action<ExternalTask> workerFunction)
        {
            workers.Add(topicName, workerFunction);
        }
    
    }
    
}
