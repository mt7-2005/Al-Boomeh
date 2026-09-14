namespace Al_BoomehServices
{
    public class BusinessRuleException : AppException
    {
        public BusinessRuleException(string message) : base(422, message) { }
    }
}
