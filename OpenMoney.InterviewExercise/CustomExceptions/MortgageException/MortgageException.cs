using System;

namespace OpenMoney.InterviewExercise.CustomExceptions.MortgageException
{
    public class MortgageException : Exception
    {
        public MortgageException()
        {
        }

        public MortgageException(string message)
        :base(message)
        {
        }

        public MortgageException(string message, Exception inner)
        :base(message, inner)
        {
        }
    }
}