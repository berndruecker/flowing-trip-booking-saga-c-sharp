using System;
using System.Collections.Generic;
using CamundaClient.Dto;
using CamundaClient.Worker;

namespace FlowingTripBookingSaga.adapter
{
    [ExternalTaskTopic("reserve-car")]
    class BookCarAdapter : IExternalTaskAdapter
    {

        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            Console.WriteLine();
            Console.WriteLine("Reserving car now...");
            Console.WriteLine();

        }

    }
}
