using System;
using System.IO;

namespace Cryptochrome
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Криптохром готов!\n");
            string userInput = "";
            while (userInput != "6")
            {
                Console.WriteLine("Теперь вы можете:\n");
                Console.WriteLine("0. Зашифровать текст TextToEncrypt.txt и сохранить результат в TextEncrypted.txt\n");
                Console.WriteLine("1. Получить матрицы встречаемости букв, диграмм и триграмм в русском языке на основе текста TextRussian.txt и сохранить их в MatricesOfRussian.txt\n");
                Console.WriteLine("2. Получить матрицы встречаемости букв, диграмм и триграмм в зашифрованном тексте TextEncrypted.txt и сохранить их в MatricesOfEncrypted.txt\n");
                Console.WriteLine("3. Найти соответствие между матрицами MatricesOfRussian.txt и MatricesOfEncrypted.txt и сохранить его в EncryptionKey.txt\n");
                Console.WriteLine("4. На основе соответствия матриц из EncryptionKey.txt расшифровать текст TextEncrypted.txt и сохранить результат в TextDecrypted.txt\n");
                Console.WriteLine("5. Сделать всё вышеперечисленное на основе TextToEncrypt.txt и TextRussian.txt\n");
                Console.WriteLine("6. Завершить работу Криптохрома\n");
                userInput = Console.ReadLine();

                if (userInput == "0" || userInput == "5") File.WriteAllLines("TextEncrypted.txt", Cryptochrome.Encrypt(File.ReadAllLines("TextToEncrypt.txt")));
                if (userInput == "1" || userInput == "5") Cryptochrome.MakeMatrices(File.ReadAllLines("TextRussian.txt")).Save("MatricesOfRussian.txt");
                if (userInput == "2" || userInput == "5") Cryptochrome.MakeMatrices(File.ReadAllLines("TextEncrypted.txt")).Save("MatricesOfEncrypted.txt");
                if (userInput == "3" || userInput == "5") Cryptochrome.SaveEncryptionKey("EncryptionKey.txt", Cryptochrome.FindEncryptionKey(Matrices.Load("MatricesOfRussian.txt"), Matrices.Load("MatricesOfEncrypted.txt")));
                if (userInput == "4" || userInput == "5") File.WriteAllLines("TextDecrypted.txt", Cryptochrome.Decrypt(File.ReadAllLines("TextEncrypted.txt"), Cryptochrome.LoadEncryptionKey("EncryptionKey.txt")));
            }
        }
    }
}