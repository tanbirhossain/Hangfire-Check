using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobManage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HangfireController : ControllerBase
    {
        //Fire-and-forget Jobs
        [HttpPost]
        [Route("welcome")]
        public IActionResult Welcome(string userName)
        {
            var jobId = BackgroundJob.Enqueue(() => SendWelcomeMail(userName));
            return Ok($"Job Id {jobId} Completed. Welcome Mail Sent!");
        }

        public void SendWelcomeMail(string userName)
        {
            //Logic to Mail the user
            Console.WriteLine($"Welcome to our application, {userName}");
        }

        //Delayed Jobs
        [HttpPost]
        [Route("delayedWelcome")]
        public IActionResult DelayedWelcome(string userName)
        {
            var jobId = BackgroundJob.Schedule(() => SendDelayedWelcomeMail(userName), TimeSpan.FromSeconds(10));
            return Ok($"Job Id {jobId} Scheduled. Delayed Welcome Mail will be sent in 2 minutes!");
        }

        public void SendDelayedWelcomeMail(string userName)
        {
            //Logic to Mail the user
            Console.WriteLine($"Welcome to our application, {userName}");
        }

        [HttpPost]
        [Route("subscribe")]
        public IActionResult Subscribe(string userName)
        {
            //RecurringJob.AddOrUpdate(() => SendDelayedWelcomeMail(userName), Cron.Minutely);
            var identifier = $"ri-{userName}";
            RecurringJob.AddOrUpdate(identifier, () => SendDelayedWelcomeMail(userName), Cron.Minutely);
            return Ok($"Recurring Job Scheduled. Invoice will be mailed Monthly for {userName}!");
        }
        [DisableConcurrentExecution(3600)] // argument in seconds, e.g., an hour
        public void SendInvoiceMail(string userName)
        {
            //Logic to Mail the user
            Console.WriteLine($"Here is your invoice, {userName}");
        }
        [HttpPost]
        [Route("unsubscribe")]
        public IActionResult Unsubscribe(string userName)
        {
            
            var jobId = BackgroundJob.Enqueue(() => UnsubscribeUser(userName));
            BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine($"Sent Confirmation Mail to {userName}"));

            //// remove all recurring job
            //List<RecurringJobDto> recurringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
            //foreach (var item in recurringJobs)
            //{
            //    RecurringJob.RemoveIfExists(item.Id);
            //}

            // remove specific recurring job
            var identifier = $"ri-{userName}";
            RecurringJob.RemoveIfExists(identifier);

            return Ok($"Unsubscribed");
        }

        public void UnsubscribeUser(string userName)
        {
            //Logic to Unsubscribe the user
            Console.WriteLine($"Unsubscribed {userName}");
        }
    }
}
