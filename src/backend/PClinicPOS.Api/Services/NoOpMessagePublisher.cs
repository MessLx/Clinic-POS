namespace PClinicPOS.Api.Services;

public class NoOpMessagePublisher : IMessagePublisher
{
    public void Publish(string eventName, object payload) { }
}
