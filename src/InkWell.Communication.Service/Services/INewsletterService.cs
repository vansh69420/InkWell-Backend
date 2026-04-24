using InkWell.Communication.Service.DTOs.Requests;
using InkWell.Communication.Service.DTOs.Responses;

namespace InkWell.Communication.Service.Services;

public interface INewsletterService
{
    Task<SubscriberResponse> SubscribeAsync(SubscribeRequest request);
    Task<bool> ConfirmAsync(string token);
    Task<bool> UnsubscribeAsync(string token);
    Task<List<SubscriberResponse>> GetAllSubscribersAsync();
    Task SendNewsletterAsync(SendNewsletterRequest request);
    Task<int> GetSubscriberCountAsync();
}