using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using OpenMoney.InterviewExercise.CustomExceptions.MortgageException;
using OpenMoney.InterviewExercise.Models;
using OpenMoney.InterviewExercise.QuoteClients;
using OpenMoney.InterviewExercise.ThirdParties;
using Xunit;

namespace OpenMoney.InterviewExercise.Tests
{
    public class MortgageQuoteClientFixture
    {
        private readonly Mock<IThirdPartyMortgageApi> _apiMock = new();
        
        [Fact]
        public async Task GetQuote_ShouldReturnNullWithErrorMessage_IfHouseValue_Over10Mill()
        {
            const float deposit = 9_000;
            const float houseValue = 100_000;
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            Assert.NotNull(quote.ErrorMessage);
            Assert.False(quote.IsResponseSuccess);
        }

        [Fact]
        public async Task GetQuote_ShouldThrowException_WhenThereIsNoClientResponse()
        {
            const float deposit = 10_000;
            const float houseValue = 100_000;
            
            _apiMock
                .Setup(api => api.GetQuotes(It.IsAny<ThirdPartyMortgageRequest>()))
                .ReturnsAsync((IEnumerable<ThirdPartyMortgageResponse>) null);
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);

            var ex = await Assert.ThrowsAsync<MortgageException>(() =>
                mortgageClient.GetQuote(new GetQuotesRequest {HouseValue = houseValue, Deposit = deposit}));
            
            Assert.Equal("No response from mortgage client", ex.Message);
        }

        [Fact]
        public async Task GetQuote_ShouldReturn_AQuote()
        {
            const float deposit = 10_000;
            const float houseValue = 100_000;

            _apiMock
                .Setup(api => api.GetQuotes(It.IsAny<ThirdPartyMortgageRequest>()))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyMortgageResponse { MonthlyPayment = 300m }
                });
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            Assert.Equal(300m, (decimal)quote.MonthlyPayment);
        }

        [Fact]
        public async Task GetQuote_ShouldReturnTheLowestMonthlyPayment_WhenThereAreMultipleQuotes()
        {
            const float deposit = 10_000;
            const float houseValue = 100_000;

            _apiMock
                .Setup(api => api.GetQuotes(It.IsAny<ThirdPartyMortgageRequest>()))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyMortgageResponse { MonthlyPayment = 300m },
                    new ThirdPartyMortgageResponse { MonthlyPayment = 400m },
                    new ThirdPartyMortgageResponse { MonthlyPayment = 600m },
                    new ThirdPartyMortgageResponse { MonthlyPayment = 100m }
                });
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            Assert.Equal(100m, (decimal)quote.MonthlyPayment);
        }
    }
}