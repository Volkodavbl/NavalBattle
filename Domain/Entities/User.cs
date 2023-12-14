namespace Domain.Entities
{
    public enum UserType
    {
        Player,
        Observer,
        Inactive
    }

    public class User(string login)
    {
        public string Login { get; init; } = login;
        public UserType UserType { get; set; } = UserType.Inactive;
    }
}
