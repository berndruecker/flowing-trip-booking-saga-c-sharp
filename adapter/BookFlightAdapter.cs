using System;
using System.Collections.Generic;
using CamundaClient.Dto;
using CamundaClient.Worker;

namespace FlowingTripBookingSaga.adapter
{
    [ExternalTaskTopic("book-flight")]
    class BookFlightAdapter : IExternalTaskAdapter
    {

        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            Console.WriteLine();
            Console.WriteLine("Booking flight now... ON NO - there was a glitch, we cannot do it!");
            Console.WriteLine();

            throw new UnrecoverableException("BookingFailed", "Could not book flight");
            
        }

    }
}
