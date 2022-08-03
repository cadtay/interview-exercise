using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using OpenMoney.InterviewExercise.CustomExceptions.HomeInsuranceException;
using OpenMoney.InterviewExercise.Models;
using OpenMoney.InterviewExercise.QuoteClients;
using OpenMoney.InterviewExercise.ThirdParties;
using Xunit;

namespace OpenMoney.InterviewExercise.Tests
{
    public class HomeInsuranceQuoteClientFixture
    {
        private readonly Mock<IThirdPartyHomeInsuranceApi> _apiMock = new();
        
        [Fact]
        public async Task GetQuote_ShouldReturnErrorMessage_IfHouseValue_Over10Mill()
        {
            const float houseValue = 10_000_001;
            
            var mortgageClient = new HomeInsuranceQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                HouseValue = houseValue
            });
            
            Assert.NotNull(quote.ErrorMessage);
            Assert.False(quote.IsResponseSuccess);
        }

        [Fact]
        public async Task GetQuote_ShouldReturn_AQuote()
        {
            const float houseValue = 100_000;

            _apiMock
                .Setup(api => api.GetQuotes(It.Is<ThirdPartyHomeInsuranceRequest>(r =>
                    r.ContentsValue == 50_000 && r.HouseValue == (decimal) houseValue)))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyHomeInsuranceResponse { MonthlyPayment = 30 }
                });
            
            var mortgageClient = new HomeInsuranceQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                HouseValue = houseValue
            });
            
            Assert.Equal(30m, (decimal)quote.MonthlyPayment);
        }

        [Fact]
        public async Task GetQuote_ShouldThrowHomeInsuranceException_WhenThereIsNoResponse()
        {
            const float houseValue = 100_000;

            _apiMock
                .Setup(api => api.GetQuotes(It.Is<ThirdPartyHomeInsuranceRequest>(r =>
                    r.ContentsValue == 50_000 && r.HouseValue == (decimal) houseValue)))
                .ReturnsAsync((IEnumerable<ThirdPartyHomeInsuranceResponse>) null);

            var mortgageClient = new HomeInsuranceQuoteClient(_apiMock.Object);

            var ex = await Assert.ThrowsAsync<HomeInsuranceException>(() =>
                mortgageClient.GetQuote(new GetQuotesRequest {HouseValue = houseValue}));
            
            Assert.Equal("No quotes returned from home insurance third party client", ex.Message);
        }

        [Fact]
        public async Task GetQuote_ShouldReturnTheLowestQuote_WhenThereAreMultipleQuotes()
        {
            const float houseValue = 100_000;

            _apiMock
                .Setup(api => api.GetQuotes(It.IsAny<ThirdPartyHomeInsuranceRequest>()))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyHomeInsuranceResponse { MonthlyPayment = 30 },
                    new ThirdPartyHomeInsuranceResponse { MonthlyPayment = 50 },
                    new ThirdPartyHomeInsuranceResponse { MonthlyPayment = 10 },
                    new ThirdPartyHomeInsuranceResponse { MonthlyPayment = 70 }
                });
            
            var mortgageClient = new HomeInsuranceQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                HouseValue = houseValue
            });
            
            Assert.Equal(10, (decimal)quote.MonthlyPayment);
        }
    }
}