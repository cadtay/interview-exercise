using System;
using System.Linq;
using System.Threading.Tasks;
using OpenMoney.InterviewExercise.CustomExceptions.HomeInsuranceException;
using OpenMoney.InterviewExercise.Models;
using OpenMoney.InterviewExercise.Models.Quotes;
using OpenMoney.InterviewExercise.QuoteClients.Interfaces;
using OpenMoney.InterviewExercise.ThirdParties;

namespace OpenMoney.InterviewExercise.QuoteClients
{
    public class HomeInsuranceQuoteClient : IHomeInsuranceQuoteClient
    {
        private IThirdPartyHomeInsuranceApi _api;

        private readonly decimal contentsValue = 50_000;

        public HomeInsuranceQuoteClient(IThirdPartyHomeInsuranceApi api)
        {
            _api = api;
        }

        public async Task<HomeInsuranceQuote> GetQuote(GetQuotesRequest getQuotesRequest)
        {
            try
            {
                // check if request is eligible
                if (getQuotesRequest.HouseValue > 10_000_000d)
                    return null;

                var request = new ThirdPartyHomeInsuranceRequest
                {
                    HouseValue = (decimal) getQuotesRequest.HouseValue,
                    ContentsValue = contentsValue
                };
                
                //Using GetAwaiter().GetResult() can cause deadlocks or thread pool starvation
                //calling an asynchronous operation and blocking on it synchronously, use async/await 
                var response = await _api.GetQuotes(request);

                if (response == null)
                    throw new HomeInsuranceException("No quotes returned from home insurance third party client");

                var cheapestQuote = response.OrderBy(x => x.MonthlyPayment)
                    .FirstOrDefault();

                var homeInsuranceQuote = new HomeInsuranceQuote
                {
                    MonthlyPayment = cheapestQuote.MonthlyPayment
                };

                return homeInsuranceQuote;
            }
            catch (HomeInsuranceException e)
            {
                //Need to add logging instead of Console.WriteLine
                Console.WriteLine(e);
                throw;
            }
        }
    }
}