using System;
using System.Security.Cryptography;
using System.Text;

var password = "4660356280";
using (var md5 = MD5.Create())
{
    var inputBytes = Encoding.UTF8.GetBytes(password);
    var hashBytes = md5.ComputeHash(inputBytes);
    var hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    Console.WriteLine($"Password: {password}");
    Console.WriteLine($"MD5 Hash: {hashedPassword}");
}
