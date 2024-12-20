namespace TestApi.Application.Commands.Contacts
{
    public class DeleteContactResponse
    {
        public string Status { get; set; } = "deleted";
        public Guid DeletedId {  get; set; }
    }
}
