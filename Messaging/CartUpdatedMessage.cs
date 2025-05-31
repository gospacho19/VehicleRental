namespace LuxuryCarRental.Messaging
{
    public class CartUpdatedMessage
    {
        public int CustomerId { get; }

        public CartUpdatedMessage(int customerId)
        {
            CustomerId = customerId;
        }
    }
}
