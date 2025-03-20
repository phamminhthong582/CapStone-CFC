namespace Service.Interface;

public interface IChatgptService
{
    Task<string> GetChatGptResponse(string content);
}