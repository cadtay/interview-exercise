namespace OpenMoney.InterviewExercise.Models.Quotes
{
    public class MortgageQuote
    {
        public float MonthlyPayment { get; set; }
        
        public string ErrorMessage { get; set; }

        public bool IsResponseSuccess { get; set; }
    }
}