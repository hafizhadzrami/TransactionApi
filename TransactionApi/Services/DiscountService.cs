namespace TransactionApi.Services
{
    public class DiscountService
    {
        public (long discount, long finalAmount) CalculateDiscount(long totalAmountCents)
        {
            decimal amountRM = totalAmountCents / 100m; // tukar sen → RM
            decimal discountPercent = 0;

            // Base discount
            if (amountRM >= 200 && amountRM <= 500) discountPercent = 0.05m;
            else if (amountRM >= 501 && amountRM <= 800) discountPercent = 0.07m;
            else if (amountRM >= 801 && amountRM <= 1200) discountPercent = 0.10m;
            else if (amountRM > 1200) discountPercent = 0.15m;

            // Conditional discounts
            if (amountRM > 500 && IsPrime((int)amountRM))
                discountPercent += 0.08m;

            if (amountRM > 900 && amountRM % 10 == 5)
                discountPercent += 0.10m;

            // Cap discount max 20%
            if (discountPercent > 0.20m)
                discountPercent = 0.20m;

            long discountCents = (long)Math.Round(totalAmountCents * discountPercent);
            long finalAmountCents = totalAmountCents - discountCents;

            return (discountCents, finalAmountCents);
        }

        private bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            int boundary = (int)Math.Floor(Math.Sqrt(number));
            for (int i = 3; i <= boundary; i += 2)
                if (number % i == 0) return false;

            return true;
        }
    }
}
