namespace BriLib
{
  public class MessageManager : Singleton<MessageManager>
  {
    public MessageBus Bus = new MessageBus();
  }
}