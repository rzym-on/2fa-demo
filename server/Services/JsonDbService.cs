using TwoFactorLogin.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace TwoFactorLogin.Services;

public static class JsonDbService
{
    private const string DbFile = "simple-db.json";
    private static List<User> Users { get; }

    static JsonDbService()
    {
        Users = new List<User>();

        // If the file exists, read from that file
        if (!File.Exists(DbFile)) return;

        var txt = File.ReadAllText(DbFile);
        if (txt.IsNullOrEmpty()) return;

        var users = JsonSerializer.Deserialize<List<User>>(txt);
        if (users == null) return;

        Users = users;
    }

    public static void AddUser(User user)
    {
        Users.Add(user);

        string json = JsonSerializer.Serialize(Users);
        File.WriteAllText(DbFile, json);
    }

    public static User? GetUser(string email)
    {
        return Users.Find(x => x.Username == email);
    }

}