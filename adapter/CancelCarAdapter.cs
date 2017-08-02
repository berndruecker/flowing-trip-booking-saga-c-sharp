using System;
using System.Collections.Generic;
using CamundaClient.Dto;
using CamundaClient.Worker;

namespace FlowingTripBookingSaga.adapter
{
    [ExternalTaskTopic("cancel-car")]
    class CancelCarAdapter : IExternalTaskAdapter
    {

        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            Console.WriteLine();
            Console.WriteLine("Cancelling car now...");
            Console.WriteLine();

        }

    }
}
