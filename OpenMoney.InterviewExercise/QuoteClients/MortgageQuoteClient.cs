using System;
using System.Linq;
using System.Threading.Tasks;
using OpenMoney.InterviewExercise.CustomExceptions.MortgageException;
using OpenMoney.InterviewExercise.Models;
using OpenMoney.InterviewExercise.Models.Quotes;
using OpenMoney.InterviewExercise.QuoteClients.Interfaces;
using OpenMoney.InterviewExercise.ThirdParties;

namespace OpenMoney.InterviewExercise.QuoteClients
{
    public class MortgageQuoteClient : IMortgageQuoteClient
    {
        private readonly IThirdPartyMortgageApi _api;

        public MortgageQuoteClient(IThirdPartyMortgageApi api)
        {
            _api = api;
        }
        
        public async Task<MortgageQuote> GetQuote(GetQuotesRequest getQuotesRequest)
        {
            try
            {
                // check if mortgage request is eligible
                var loanToValueFraction = getQuotesRequest.Deposit / getQuotesRequest.HouseValue;

                if (loanToValueFraction < 0.1d)
                    return null;

                var mortgageAmount = getQuotesRequest.HouseValue - getQuotesRequest.Deposit;
            
                var request = new ThirdPartyMortgageRequest
                {
                    MortgageAmount = (decimal) mortgageAmount
                };

                var response = await _api.GetQuotes(request);

                if (response == null)
                    throw new MortgageException("No response from mortgage client");

                var cheapestQuote = response.OrderBy(x => x.MonthlyPayment)
                    .FirstOrDefault();

                var mortgageQuote = new MortgageQuote
                {
                    MonthlyPayment = (float) cheapestQuote.MonthlyPayment
                };

                return mortgageQuote;
            }
            catch (MortgageException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}