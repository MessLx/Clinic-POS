namespace PClinicPOS.Api.Services;

public interface IMessagePublisher
{
    void Publish(string eventName, object payload);
}
