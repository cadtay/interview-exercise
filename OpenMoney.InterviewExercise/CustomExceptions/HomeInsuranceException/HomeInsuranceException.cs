using System;

namespace OpenMoney.InterviewExercise.CustomExceptions.HomeInsuranceException
{
    public class HomeInsuranceException : Exception
    {
        public HomeInsuranceException()
        {
        }

        public HomeInsuranceException(string message)
        :base(message)
        {
        }

        public HomeInsuranceException(string message, Exception inner)
        :base(message, inner)
        {
        }
    }
}