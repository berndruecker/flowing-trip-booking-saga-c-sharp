using System;
using System.Collections.Generic;
using CamundaClient.Dto;
using CamundaClient.Worker;

namespace FlowingTripBookingSaga.adapter
{
    [ExternalTaskTopic("cancel-hotel")]
    class CancelHotelAdapter : IExternalTaskAdapter
    {

        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            Console.WriteLine();
            Console.WriteLine("Cancelling hotel now...");
            Console.WriteLine();

        }

    }
}
