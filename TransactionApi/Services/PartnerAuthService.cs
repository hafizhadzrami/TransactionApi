namespace TransactionApi.Services
{
    public class PartnerAuthService
    {
        
        public bool ValidatePartner(string key, string password)
        {
            return !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(password);
        }
    }
}
