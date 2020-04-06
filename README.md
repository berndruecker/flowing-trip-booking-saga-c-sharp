# Saga example: trip booking

The Saga pattern describes how to solve distributed (business) transactions without two-phase-commit as this does not scale in distributed systems. The basic idea is to break the overall transaction into multiple steps or activities. Only the steps internally can be performed in atomic transactions but the overall consistency is taken care of by the Saga. The Saga has the responsibility to either get the overall business transaction completed or to leave the system in a known termination state. So in case of errors a business rollback procedure is applied which occurs by calling compensation steps or activities in reverse order. A more detailed look on Sagas is available in [Saga: How to implement complex business transactions without two phase commit](
https://blog.bernd-ruecker.com/saga-how-to-implement-complex-business-transactions-without-two-phase-commit-e00aa41a1b1b)

In the example hotel, car and flight booking might be done by different remote services. So there is not technical transaction, but a business transaction. When the flight booking cannot be carried out succesfully you need to cancel hotel and car. 

![Saga example](docs/example-use-case.png)

Using [Camunda](https://camunda.org/) you can implement the Saga by a BPMN XML file created using the [Camunda Modeler](https://camunda.org/download/modeler/) (or by a Java DSL, but this is not available in C# at the moment). 

![Compensation in BPMN](docs/example-bpmn.png)

Run Camunda, e.g. via Docker. More details on how to run Camunda for non-Java folks can be found in [Use Camunda without touching Java and get an easy-to-use REST-based orchestration and workflow engine](https://blog.bernd-ruecker.com/use-camunda-without-touching-java-and-get-an-easy-to-use-rest-based-orchestration-and-workflow-7bdf25ac198e):

```shell
docker run -d --name camunda -p 8080:8080 camunda/camunda-bpm-platform:latest
```

Now you can connect to the engine via [REST API](https://docs.camunda.org/manual/latest/reference/rest/). You can leverage for example [this unsupported client library](https://github.com/berndruecker/camunda-dot-net-showcase) to ease the job to access REST, which results in pretty simple code:

```cs
var camunda = new CamundaEngineClient("http://localhost:8080/engine-rest/engine/default/", null, null);
            
// Deploy the BPMN XML file from the resources
camunda.RepositoryService.Deploy("trip-booking", new List<object> {
       FileParameter.FromManifestResource(Assembly.GetExecutingAssembly(), "FlowingTripBookingSaga.Models.FlowingTripBookingSaga.bpmn") 
   });

// Register workers
registerWorker("reserve-car", externalTask => {
  // here you can do the real thing! Like a sysout :-)
  Console.WriteLine("Reserving car now...");
  camunda.ExternalTaskService.Complete(workerId, externalTask.Id);
});
registerWorker("cancel-car", externalTask => {
  Console.WriteLine("Cancelling car now...");
  camunda.ExternalTaskService.Complete(workerId, externalTask.Id);
});
registerWorker("book-hotel", externalTask => {
  Console.WriteLine("Reserving hotel now...");
  camunda.ExternalTaskService.Complete(workerId, externalTask.Id);
});
// Register more workers...

StartPolling();

string processInstanceId = camunda.BpmnWorkflowService.StartProcessInstance("FlowingTripBookingSaga", new Dictionary<string, object>()
  {
    {"someBookingData", "..." }
  });

}
```

The real logic is in the callbacks.

The engine will take care of state handling, compensation and could also handle timeouts and escalations.

![Cockpit Screenshot](docs/screenshot.png)



# Get started

You need

* Visual Studio

Required steps

* Checkout or download this project
* Checkout or download https://github.com/berndruecker/camunda-dot-net-showcase to have the client library available
* Run Camunda via Docker (or other means):
```shell
docker run -d --name camunda -p 8080:8080 camunda/camunda-bpm-platform:latest
```
* Run the [Program.cs](Program.cs) class as this is a main application doing everything and starting exactly one Saga that is always "crashing" in the flight booking. 
* If you like you can access the Camunda webapplication on http://localhost:8080/
