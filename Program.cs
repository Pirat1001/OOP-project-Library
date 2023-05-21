using System;
using System.Reflection.Metadata;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace Library_Project_OOP
{
    internal class Program
    {
        // Gör om informationen från text filerna till listor som vidare används i programmet
        static List<string> personalNumbersFromDb = new List<string>();
        static List<string> passwordsFromDb = new List<string>();
        static List<string> usersDb = new List<string>();
        static List<string> booksFromDb = new List<string>();
        static List<string> rentedBooksDb = new List<string>();

        // Tilldelar de olika paths till specifika variabler för att skippa klistra in path varje gång
        static string usersPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Users.txt";
        static string personalNumbersPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Personalnumbers.txt";
        static string passwordsPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Passwords.txt";
        static string booksPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt";
        static string rentedBooksPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\RentedBooks.txt";

        // klass som hjälper admin att skapa en bok
        public class Book
        {
            public string Title { get; set; }
            public string Author { get; set; }  
            public string Genre { get; set; }
            public string Isbn { get; set; }
            public int Available { get; set; }
            public string Reserved { get; set; }

            // Det skapade bok objeketet lagras längre nedan i textfiler
            public Book(string titleBook, string authorName, string genreType, string isbnCode, int available, string reserved)
            {
                Title = titleBook;
                Author = authorName;
                Genre = genreType;
                Isbn = isbnCode;   
                Available = available;
                Reserved = reserved;
            }
        }

        public class RentedBook
        {
            public string BookISBN { get; set; }
            public string UserPn { get; set; }

            public RentedBook(string bookISBN, string userPn)
            {
                BookISBN = bookISBN;
                UserPn = userPn;
            }
        }

        // Globala variabler som gör det möjligt att använda användar specifik information
        static class Globals
        {
            public static string personalNumber;
            public static string password;
            public static string lastName;
            public static string firstName;
            public static string userType;
            public static int userIndex;
            public static int bookIndex;
            public static int rentedBookIndex;
            public static string bookIsbn;
            
        }

        // Main function
        static void Main(string[] args)
        {
            bool endApp = false;

            Console.WriteLine("-----------------------------------------------------------------\n");
            Console.WriteLine("               -- Welcome to Your Library -- \n");
            Console.WriteLine("-----------------------------------------------------------------\n");

            while (!endApp)
            {
                Console.WriteLine("     - For Logging in please press L - \n");
                Console.WriteLine("     - For Register as a new user please press R - \n\n");
                Console.WriteLine(" --- Press Q and enter to close the application --- \n\n");

                // Skapar en meny av funktioner som kan kallas genom olika inputs
                string choice = Console.ReadLine();

                if (choice == "Q" || choice == "q")
                {
                    AppClose();
                } 

                // Funktionen Register kommer att kallas när användare skriver in bokstaven R
                else if(choice == "R" || choice == "r")
                {
                    Register();
                }
                else if(choice == "L" ||choice == "l")
                {
                    Login();
                }
            }
            
        }

     
        static void Register()
        {
            string userType = "user";

            Console.WriteLine("In order to create an account enter following:");

            // Tillfälligt lagrar informationen om användaren i lokala variabler
            Console.WriteLine("Firstname: ");
            string firstName = Console.ReadLine();

            Console.WriteLine("Lastname: ");
            string lastName = Console.ReadLine();

            Console.WriteLine("Personalnumber: ");
            string personalNum = Console.ReadLine();

            Console.WriteLine("Password:");
            string password = Console.ReadLine();

            Console.WriteLine("Repeat Password: ");
            string repeatPass = Console.ReadLine();

            // Skapar en linje utav informationen med datatypen string
            var line = $"{firstName} {lastName} {personalNum} {userType}";
            string[] lines = { line };
            Console.WriteLine("");

            // Säkerhetskontroll, kollar ifall inskrivna lösenorder matchar re-enter
            if(repeatPass != password)
            {
                // Detta händer ifall lösenorden inte matchar varandra
                Console.Clear();
                Console.WriteLine("Please make sure that both passwords are the same!");
                Console.WriteLine("");
                Register();
                return;
            } 
            // Ifall lösenorden matchar händer följande:
            else
            {
                // Använder funktion för att kolla ifall användaren har skrivit in all information som krävs för att skapa en användare
                if(UserInfoIncomplete(firstName, lastName, personalNum, userType))
                {
                    // Vad som händer ifall inormationen är inkomplett
                    Console.Clear();
                    Console.WriteLine("Please enter all the required information");
                    Console.WriteLine("");
                    Register();
                    return;
                }
                // En if-sats som med hjälp av funktionen UserExists och personnumret som blivit inskriven kollar ifall en sådan användare redan finns i databasen
                else if(UserExists(personalNum)) 
                {
                    // Vad som händer ifall användaren med inskrivna personnumret finns i databasen:
                    Console.Clear();
                    Console.WriteLine("That personalnumber already exists, you should try to log-in");
                    Console.WriteLine("");
                    Register();
                    return;
                }
                else
                {
                    Console.WriteLine("Hi " + firstName + ", you've created an account and will be redirected to login-page...");

                    // Återigen skapar en linje för att lagra lösenord och personnumret i separat textfil
                    var pass = $"{password}";
                    string[] passwords = { pass };
                    File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Passwords.txt", passwords);

                    var pN = $"{personalNum}";
                    string[] personalNumbers = { pN };
                    File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Personalnumbers.txt", personalNumbers);

                    File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Users.txt", lines);

                    Thread.Sleep(5000);
                    Login();
                }
            }

        }

        // Funktionen som kallas vid input av L i main eller efter register-funktionen
        static void Login()
        {
            bool wrongPass = false;
            string personalNumber = "";
            string password = "";

            // Skapar en while loop som innebär att så länge informationen är false alltså från funktionen Authentication skall följande loopas
            while(!Authentication(personalNumber, password)) 
            {
                Console.Clear();

                if (wrongPass)
                {
                    Console.WriteLine("*Wrong password or personalnumber*");
                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("Welcome!");
                }

                Console.WriteLine("In order to log-in please enter personalnumber and password: ");

                Console.WriteLine("Personalnumber:");
                personalNumber = Console.ReadLine();

                Console.WriteLine("Password:");
                password = Console.ReadLine();

                Console.WriteLine("");

                wrongPass = true;

            }

            LoggedIn();

        }

        // Funktionen kallas när användaren har loggat in
        static void LoggedIn()
        {
            // UI skiljs åt beroende på vilken användare programmet har stött på därför krävs följande if-sats
            // User har olika möjligheter från Admin
            if(Globals.userType == "user")
            {
                bool endApp = false;

                Console.Clear();
                Console.WriteLine("**************************************");
                Console.WriteLine("");
                Console.WriteLine("Hello again " + Globals.firstName + ", you're now logged in!");
                Console.WriteLine("");
                Console.WriteLine("**************************************");
                Console.WriteLine("");

                Console.WriteLine("Go to profile: [P]\n");
                Console.WriteLine("Go to library: [L]\n");
                Console.WriteLine("Close application: [Q]\n");


                string choice = Console.ReadLine();

                if (choice == "P" || choice == "p")
                {
                    Console.WriteLine("              Loading Profile...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(50);

                    }

                    Console.Clear();
                    UserProfile();

                }

                else if (choice == "L" || choice == "l")
                {
                    Console.WriteLine("              Loading Library...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(50);

                    }

                    Console.Clear();
                    Library();

                }

                else if (choice == "Q" || choice == "q")
                {
                    Console.WriteLine("Closing application...\n");
                    AppClose();
                }

            }
            else
            {
                bool endApp = false;

                Console.Clear();

                Console.WriteLine("**************************************");
                Console.WriteLine("");
                Console.WriteLine("Hello again " + Globals.firstName + ", you're now logged in!");
                Console.WriteLine("");
                Console.WriteLine("**************************************");
                Console.WriteLine("");

                Console.WriteLine("Go to profile: [P]\n");
                Console.WriteLine("Go to library: [L]\n");
                Console.WriteLine("Browse users: [A]\n");
                Console.WriteLine("Close application: [Q]\n");


                string choice = Console.ReadLine();

                if (choice == "P" || choice == "p")
                {
                    Console.WriteLine("              Loading Profile...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(50);
                    }

                    Console.Clear();
                    UserProfile();

                }

                else if (choice == "L" || choice == "l")
                {
                    Console.WriteLine("              Loading Library...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(50);

                    }

                    Console.Clear();
                    Library();

                }
                else if(choice == "A" || choice == "a")
                {
                    Console.WriteLine("              Loading UserList...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(50);

                    }

                    Console.Clear();
                    Admin();
                }

                else if (choice == "Q" || choice == "q")
                {
                    Console.WriteLine("Closing application...\n");
                    AppClose();
                }
            }

        }
       
        static void Admin()
        {

            Console.WriteLine("User List: [U]\n");
            Console.WriteLine("Change User Type: [A]\n");
            Console.WriteLine("Delete User: [D]\n");
            Console.WriteLine("Library Management: [L]\n");
            Console.WriteLine("Back to Main: [B]\n");
            string adminChoice = Console.ReadLine();

            // Följande händer ifall användaren matar in bokstaven U
            if (adminChoice == "U" || adminChoice == "u")
            {
                // Skapar en metod för programmet att läsa igenom en textfil och även skriva ut den i consolappen
                using (FileStream fs = File.OpenRead(usersPath))
                {
                    byte[] b = new byte[1024];
                    UTF8Encoding temp = new UTF8Encoding(true);

                    while (fs.Read(b, 0, b.Length) > 0)
                    {
                        Console.WriteLine(temp.GetString(b));
                    }
                }
                Admin();
            }
            else if (adminChoice == "A" || adminChoice == "a")
            {

                Console.WriteLine("Please enter personalNum of user you wish to make an admin: ");
                string userPersonalNum = Console.ReadLine();

                if (userPersonalNum == Globals.personalNumber)
                {
                    Console.WriteLine("You can't unmake yourself being adming..");
                    Thread.Sleep(200);
                    Admin();
                }
                else if(userPersonalNum == "b" || userPersonalNum == "B")
                {
                    Console.WriteLine("              Loading Admin...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(50);

                    }

                    Console.Clear();
                    Admin();
                }
                else
                {

                    // följande händer för att hitta index av användaren
                    bool indexFound = false;

                    // Läser in de olika filerna och lagrar de i variabler
                    string[] users = System.IO.File.ReadAllLines(@usersPath);
                    string[] personalNumbersFromFile = System.IO.File.ReadAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Personalnumbers.txt");
                    string[] passwordsFromFile = System.IO.File.ReadAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Passwords.txt");

                    // For loop som kör igenom en text fil för att söka information 
                    for (int i = 0; i < users.Length; i++)
                    {
                        // Den informationen lagras i variabeln personalNumberFromFile
                        string personalNumberFromFile = personalNumbersFromFile[i];

                        // If-sats som kolla ifall input som matades in matchar det som filen hittade
                        if (userPersonalNum == personalNumberFromFile)
                        {
                            // Följande händer ifall det sker
                            Globals.userIndex = i;
                            // Värdet på indexFound som är ett bool beror på funktionen MakeAdmin som kan returnera olika värden
                            indexFound = MakeAdmin(userPersonalNum, usersPath, Globals.userIndex);
                        }
                    }
                    if (indexFound)
                    {
                        Console.WriteLine("User Type changed!");
                        Thread.Sleep(1000);
                        Console.Clear();
                        Admin();
                    }
                    else
                    {
                        Console.WriteLine("Something went wrong");
                    }
                }
            }
            else if (adminChoice == "D" || adminChoice == "d")
            {
                Console.WriteLine("Please enter personalNum of user you wish to make an admin: ");
                string userPersonalNum = Console.ReadLine();

                if (userPersonalNum == Globals.personalNumber)
                {
                    Console.WriteLine("You can't delete yourself..");
                    Thread.Sleep(200);
                    Admin();
                }
                else if(userPersonalNum == "B" || userPersonalNum == "b")
                {
                    Console.WriteLine("              Loading Admin...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(50);

                    }

                    Console.Clear();
                    Admin();
                }
                else
                {
                    bool userDeleted = false;

                    string[] users = System.IO.File.ReadAllLines(@usersPath);
                    string[] personalNumbersFromFile = System.IO.File.ReadAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Personalnumbers.txt");

                    for (int i = 0; i < users.Length; i++)
                    {
                        // Först läser programmet in informationen och lagrar aktuella värdet av i
                        string userFromFile = users[i];
                        // Den delar upp i värdet genom .Split(" ") alltså varje del som har en mellanslag mellan varandra blir till ett eget index
                        string[] userParts = userFromFile.Split(" ");
                        // UserPn lagrar ett specifikt index alltså 2 som i textfilen står för personnumret
                        string userPn = userParts[2];

                        if (userPersonalNum == userPn)
                        {

                            Globals.userIndex = i;
                            userDeleted = DeleteUser(userPersonalNum, usersPath, Globals.userIndex);
                        }
                    }
                    if (userDeleted)
                    {
                        Console.WriteLine("User has been deleted\n");
                        Thread.Sleep(1000);
                        Console.Clear();
                        Admin();
                    }
                    else
                    {
                        Console.WriteLine("User Doesn't exist");
                    }

                }
            }
            else if(adminChoice == "L" || adminChoice == "l")
            {
                Console.Clear();
                Console.WriteLine("\n\n");
                Console.WriteLine("Add a new Book: [A]\n");
                Console.WriteLine("Delete a Book: [D]\n");
                Console.WriteLine("Edit a Book: [E]\n");
                Console.WriteLine("List all Books in library: [L]\n");
                Console.WriteLine("Go Back: [B]\n");

                string libraryAction = Console.ReadLine();

                if (libraryAction == "B" || libraryAction == "b")
                {
                    Console.WriteLine("              Loading Admin...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(50);

                    }

                    Console.Clear();
                    Admin();
                }
                else if (libraryAction == "A" || libraryAction == "a")
                {
                    bool bookExists = false;
                    int available = 1;
                    string reserved = "notReserved";

                    Console.WriteLine("Please enter new book title: \n");
                    string newBookTitle = Console.ReadLine();

                    Console.WriteLine("Please enter new book author: \n");
                    string newBookAuthor = Console.ReadLine();

                    Console.WriteLine("Please enter new book genre: \n");
                    string newBookGenre = Console.ReadLine();

                    Console.WriteLine("Please enter new book ISBN: \n");
                    string newBookIsbn = Console.ReadLine();

                    Book book1 = new Book(newBookTitle, newBookAuthor, newBookGenre, newBookIsbn, available, reserved);
                    var bookLine = $"{book1.Title}{","}{book1.Author}{","}{book1.Genre}{","}{book1.Isbn}{","}{book1.Available}{","}{book1.Reserved}";
                    string[] bookLines = { bookLine };

                    if (booksFromDb.Count == 0)
                    {
                        booksFromDb = System.IO.File.ReadAllLines(booksPath).ToList();
                    }

                    for (int i = 0; i < booksFromDb.Count; i++)
                    {
                        string bookFromDb = booksFromDb[i];
                        string[] bookParts = bookFromDb.Split(",");
                        string currentIsbn = bookParts[3];

                        if (book1.Isbn == currentIsbn)
                        {
                            bookExists = true;
                        }
                    }
                    if(bookExists)
                    {
                        Console.WriteLine("Such book already exists in the library\n");
                        Console.WriteLine("Or you used wrong ISBN number\n");
                        Console.WriteLine("Returning to Admin");

                        Console.WriteLine("              Loading Admin...");
                        Console.WriteLine("");

                        string text2 = "----------------------------------------------";
                        foreach (char c in text2)
                        {
                            Console.Write(c);
                            Thread.Sleep(50);

                        }

                        Console.Clear();
                        Admin();
                    }
                    else
                    {
                        File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt", bookLines);

                        Console.WriteLine("the book " + book1.Title + " has been added!");
                        Thread.Sleep(1500);

                        Console.WriteLine("              Loading Admin...");
                        Console.WriteLine("");

                        string text = "----------------------------------------------";
                        foreach (char c in text)
                        {
                            Console.Write(c);
                            Thread.Sleep(50);

                        }

                        Console.Clear();
                        Admin();

                    }

                }
                else if(libraryAction == "L" || libraryAction == "l")
                {
                    using (FileStream fs = File.OpenRead(booksPath))
                    {
                        byte[] b = new byte[1024];
                        UTF8Encoding temp = new UTF8Encoding(true);

                        while (fs.Read(b, 0, b.Length) > 0)
                        {
                            Console.WriteLine(temp.GetString(b));
                        }
                    }
                    Admin();
                }
                else if(libraryAction == "D" || libraryAction == "d")
                {

                    Console.WriteLine("Please enter the ISBN number of the book you wish to delete: \n");
                    string deleteIsbn = Console.ReadLine();

                    if(deleteIsbn == "B" || deleteIsbn == "b")
                    {
                        Console.WriteLine("              Loading Admin...");
                        Console.WriteLine("");

                        string text = "----------------------------------------------";
                        foreach (char c in text)
                        {
                            Console.Write(c);
                            Thread.Sleep(50);

                        }

                        Console.Clear();
                        Admin();
                    } 
                    else if (BookExists(deleteIsbn) != true)
                    {
                        Console.Clear();
                        Console.WriteLine("The book you wish to delete doesn't exist in our library\n");
                        Thread.Sleep(2000);
                        Console.WriteLine("Check book isbn by listing all books\n");
                        Thread.Sleep(4000);
                        Console.WriteLine("              Loading Admin...");
                        Console.WriteLine("");

                        string text = "----------------------------------------------";
                        foreach (char c in text)
                        {
                            Console.Write(c);
                            Thread.Sleep(50);

                        }

                        Console.Clear();
                        Admin();
                    }
                    else if(BookExists(deleteIsbn))
                    {
                        if (DeleteBook(Globals.bookIndex, Globals.bookIsbn))
                        {
                            Console.WriteLine("The book has been deleted!");
                            Thread.Sleep(3000);
                            Console.WriteLine("              Returning to Admin...");
                            Console.WriteLine("");

                            string text = "----------------------------------------------";
                            foreach (char c in text)
                            {
                                Console.Write(c);
                                Thread.Sleep(50);

                            }

                            Console.Clear();
                            Admin();
                        }

                    }
                }
            }
            else if (adminChoice == "B" || adminChoice == "b")
            {
                Console.WriteLine("              Loading Main...");
                Console.WriteLine("");

                string text = "----------------------------------------------";
                foreach (char c in text)
                {
                    Console.Write(c);
                    Thread.Sleep(50);

                }

                Console.Clear();
                LoggedIn();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Please select one of the possible inputs");
                Thread.Sleep(1000);

                Console.WriteLine("              Loading Admin...");
                Console.WriteLine("");

                string text = "----------------------------------------------";
                foreach (char c in text)
                {
                    Console.Write(c);
                    Thread.Sleep(50);

                }

                Console.Clear();
                Admin();
            }
        }

        public static bool BookExists(string isbn)
        {
            bool exists = false;
            string[] booksFromFile = System.IO.File.ReadAllLines(@booksPath);

            if (booksFromDb.Count == 0)
            {
                booksFromDb = System.IO.File.ReadAllLines(booksPath).ToList();
            }

            for (int i = 0; i < booksFromDb.Count; i++)
            {
                string bookFromDb = booksFromFile[i];
                string[] bookParts = bookFromDb.Split(",");
                string currentIsbn = bookParts[3];

                if (isbn == currentIsbn)
                {
                    Globals.bookIndex = i;
                    Globals.bookIsbn = currentIsbn;
                    exists = true;
                }
            }

            if (exists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static void Library()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Welcome to the library\n");
            Console.WriteLine("Search for books by title, genre, author to find a specific book search using book isbn number\n");
            Console.WriteLine("In order to rent a book: [R]\n");
            Console.WriteLine("In order to return a book: [Return]\n");
            Console.WriteLine("Go Back to Main: [B]\n");
            string userSearch = Console.ReadLine();

            string[] fileData = File.ReadAllLines(@booksPath);

            for(int i = 0; i < fileData.Length; i++)
            {
                string currentData = fileData[i];

                // .Contains funktionen kollar ifall den lagrade datan  innehåller den inmatade inputen
                if (currentData.Contains(userSearch) == true)
                {
                    Console.WriteLine(currentData);
                    continue;
                }
            }
            if(userSearch == "R" || userSearch == "r")
            {
                Console.WriteLine("\n\n");
                Console.WriteLine("Type in the book ISBN of the specific book you wish to rent");
                Console.WriteLine("Go back to search: [B]");
                string rentISBN = Console.ReadLine();
                
                if(rentISBN == "B" || rentISBN == "b")
                {
                    Console.WriteLine("              Loading Library...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(50);

                    }

                    Console.Clear();
                    Library();
                }
                else
                {
                    string[] bookLines = File.ReadAllLines(@booksPath);

                    for (int i = 0; i < bookLines.Length; i++)
                    {
                        var testLine = bookLines[i];
                        string[] lineParts = testLine.Split(",");
                        string currentIsbn = lineParts[3];
                        string currentAvailability = lineParts[4];

                        if(rentISBN == currentIsbn && currentAvailability == "1")
                        {
                            RentedBook rentedBook1 = new RentedBook(rentISBN, Globals.personalNumber);
                            var rentedBookLine = $"{rentedBook1.BookISBN}{","}{rentedBook1.UserPn}";
                            string[] rentedBookLines = { rentedBookLine };
                            File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\RentedBooks.txt", rentedBookLines);

                            if(ChangeBookAvailability(rentISBN))
                            {
                                Console.WriteLine("You've rented a book!");
                                Thread.Sleep(2000);

                                Console.WriteLine("              Loading Library...");
                                Console.WriteLine("");

                                string text = "----------------------------------------------";
                                foreach (char c in text)
                                {
                                    Console.Write(c);
                                    Thread.Sleep(50);

                                }

                                Console.Clear();
                                Library();
                            }
                        }
                        else if(rentISBN == currentIsbn && currentAvailability == "0")
                        {
                            string[] rentedBookLines = File.ReadAllLines(@rentedBooksPath);
                            Console.WriteLine("The book you wish to rent is currently rented out\n");

                            if (BookReservation(rentISBN))
                            {
                                Console.WriteLine("The book is not reserved do you wish to make a reservation? [Yes/No]\n");
                                string userAnswer = Console.ReadLine();

                                if(userAnswer == "yes" || userAnswer == "Yes")
                                {
                                    Console.WriteLine("You've made an reservation");
                                    Thread.Sleep(2000);

                                    ChangeBookReservation(rentISBN);

                                    Console.WriteLine("              Loading Library...");
                                    Console.WriteLine("");

                                    string text = "----------------------------------------------";
                                    foreach (char c in text)
                                    {
                                        Console.Write(c);
                                        Thread.Sleep(50);

                                    }

                                    Console.Clear();
                                    Library();
                                }
                                else if(userAnswer == "no" || userAnswer == "No")
                                {
                                    Console.WriteLine("No reservation has been made");

                                    Console.WriteLine("              Loading Library...");
                                    Console.WriteLine("");

                                    string text = "----------------------------------------------";
                                    foreach (char c in text)
                                    {
                                        Console.Write(c);
                                        Thread.Sleep(50);

                                    }

                                    Console.Clear();
                                    Library();
                                }
                            }
                            else
                            {
                                Console.WriteLine("There's already a reservation made for that book...\n");
                                Thread.Sleep(3000);

                                Console.WriteLine("              Loading Library...");
                                Console.WriteLine("");

                                string text = "----------------------------------------------";
                                foreach (char c in text)
                                {
                                    Console.Write(c);
                                    Thread.Sleep(50);

                                }

                                Console.Clear();
                                Library();
                            }
                        }
                        else
                        {
                            Console.WriteLine("You might have typed in wrong bookISBN number\n");
                            Console.WriteLine("Or the book is currently unavailable(rented)\n");
                            Console.WriteLine("You can try again with the ISBN");
                            Thread.Sleep(4000);

                            Console.WriteLine("              Loading Library...");
                            Console.WriteLine("");

                            string text = "----------------------------------------------";
                            foreach (char c in text)
                            {
                                Console.Write(c);
                                Thread.Sleep(50);

                            }

                            Console.Clear();
                            Library();
                        }

                    }

                }
            }
            else if(userSearch == "return" || userSearch == "Return" || userSearch == "RETURN")
            {
                string[] bookLines = File.ReadAllLines(@booksPath);
                string[] rentedBookLines = File.ReadAllLines(@rentedBooksPath);
                bool bookReturned = false;

                Console.WriteLine("Type in the ISBN of the book you wish to return:");
                string returnIsbn = Console.ReadLine();

                for(int i = 0; i < rentedBookLines.Length; i++)
                {
                    var testLine = rentedBookLines[i];
                    string[] lineParts = testLine.Split(",");
                    string currentPN = lineParts[1];
                    string currentISBN = lineParts[0];

                    if(currentPN == Globals.personalNumber && currentISBN == returnIsbn)
                    {
                        Globals.rentedBookIndex = i;
                        bookReturned = ReturnBook(returnIsbn);
                    }
                }
                if(bookReturned)
                {
                    if (ChangeBookAvailability(returnIsbn))
                    {
                        Console.WriteLine("The book has been returned!\n");
                        Thread.Sleep(3000);

                        Console.WriteLine("              Loading Library...");
                        Console.WriteLine("");

                        string text = "----------------------------------------------";
                        foreach (char c in text)
                        {
                            Console.Write(c);
                            Thread.Sleep(50);

                        }

                        Console.Clear();
                        Library();
                    }
                    else
                    {
                        Console.WriteLine("Something went wrong...");
                    }
                } 
                else
                {
                    Console.WriteLine("The book has not been returned, please try again");
                    Thread.Sleep(3000);

                    Console.WriteLine("              Loading Library...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(50);

                    }

                    Console.Clear();
                    Library();
                }
            }
            else if(userSearch == "b" || userSearch == "B")
            {
                Console.WriteLine("              Loading Main...");
                Console.WriteLine("");

                string text = "----------------------------------------------";
                foreach (char c in text)
                {
                    Console.Write(c);
                    Thread.Sleep(50);

                }

                Console.Clear();
                LoggedIn();
            }

            Library();
        }

        public static bool ChangeBookReservation(string bookIsbn)
        {
            string[] bookLines = File.ReadAllLines(@booksPath);
            string reserved = "Reserved";
            string notReserved = "notReserved";

            for (int i = 0; i < bookLines.Length; i++)
            {
                var testLine = bookLines[i];
                string[] lineParts = testLine.Split(",");
                string currentIsbn = lineParts[3];
                string currentReservation = lineParts[5];

                if (currentIsbn == bookIsbn)
                {
                    if (currentReservation == "Reserved")
                    {
                        var bookLine = $"{lineParts[0]}{","}{lineParts[1]}{","}{lineParts[2]}{","}{lineParts[3]}{","}{lineParts[4]}{","}{notReserved}";
                        string[] bookUnavailable = { bookLine };
                        File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", bookUnavailable);
                        continue;
                    }
                    else
                    {
                        var bookLine = $"{lineParts[0]}{","}{lineParts[1]}{","}{lineParts[2]}{","}{lineParts[3]}{","}{lineParts[4]}{","}{reserved}";
                        string[] bookAvailable = { bookLine };
                        File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", bookAvailable);
                        continue;
                    }
                }
                else
                {
                    var currentLine = $"{bookLines[i]}";
                    string[] currentBookLine = { currentLine };
                    File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", currentBookLine);
                }
            }

            File.Delete(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt");
            File.Move(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt");
            usersPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt";
            return true;
        }

        public static bool BookReservation(string bookIsbn)
        {
            string[] bookLines = File.ReadAllLines(@booksPath);

            for (int i = 0; i < bookLines.Length; i++)
            {
                var testLine = bookLines[i];
                string[] lineParts = testLine.Split(",");
                string currentReservation = lineParts[5];
                string currentIsbn = lineParts[3];

                if (currentIsbn == bookIsbn && currentReservation == "notReserved")
                {
                    return true;
                }
                else if(currentIsbn == bookIsbn && currentReservation == "Reserved")
                {
                    return false;
                }
            }
            return false;
        }

        static void UserProfile()
        {
            Console.WriteLine("Welcome to your profile!" + Globals.firstName + "\n");
            Console.WriteLine("User Info: [I]\n\n");
            Console.WriteLine("Change your password: [P]\n\n");
            Console.WriteLine("In order to go back to main: [B]\n\n");

            string choice = Console.ReadLine();

            if(choice == "P" || choice == "p")
            {
                Console.WriteLine("Return to profile: [B]\n");
                Console.WriteLine("Enter old password:\n");
                string oldPass = Console.ReadLine();

                if(oldPass == Globals.password)
                {
                    Console.WriteLine("Please enter new password");
                    string newPass = Console.ReadLine();

                    Console.WriteLine("Please repeat new password");
                    string repeatPass = Console.ReadLine();


                    if (newPass == repeatPass)
                    {
                        var passSuccess = PasswordChange(newPass);
                        
                        if(passSuccess)
                        {
                            Console.WriteLine("Success");
                        }
                        else
                        {
                            Console.WriteLine("Fail");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The passwords don't match, try again..");
                        Thread.Sleep(200);
                        Console.Clear();
                        UserProfile();
                    }
                }
                else
                {
                    Console.WriteLine("Old password isn't correct...");
                    Thread.Sleep(200);
                    Console.Clear();
                    UserProfile();
                }

            }
            else if(choice == "B" || choice == "b")
            {
                Console.WriteLine("              Loading Main...");
                Console.WriteLine("");

                string text = "----------------------------------------------";
                foreach (char c in text)
                {
                    Console.Write(c);
                    Thread.Sleep(50);

                }

                Console.Clear();
                LoggedIn();
            }
            else if(choice == "I" || choice == "i")
            {
                Console.Clear();
                Console.WriteLine("\n\n **********************************************\n\n");
                Console.WriteLine("First Name: " + Globals.firstName + "\n");
                Console.WriteLine("Last Name: " + Globals.lastName + "\n");
                Console.WriteLine("User Role: " + Globals.userType + "\n");
                Console.WriteLine("\n\n **********************************************\n\n");

                Console.WriteLine("Rented books:\n");

                string[] lines = File.ReadAllLines(@rentedBooksPath);
                string[] bookLines = File.ReadAllLines(@booksPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    var testLine = lines[i];
                    string[] lineParts = testLine.Split(",");
                    string currentPN = lineParts[1];
                    string currentISBN = lineParts[0];

                    if (currentPN == Globals.personalNumber)
                    {
                        for (int x = 0; x < bookLines.Length; x++)
                        {
                            var testLine2 = bookLines[x];
                            string[] lineParts2 = testLine2.Split(",");
                            string currentIsbn = lineParts2[3];

                            if (currentIsbn == currentISBN)
                            {
                                Console.WriteLine($"{testLine2}");
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                Console.WriteLine("Back to profile: [B]");
                string input = Console.ReadLine();

                if(input == "B" || input == "b")
                {
                    Console.WriteLine("              Returning to profile...");
                    Console.WriteLine("");

                    string text = "----------------------------------------------";
                    foreach (char c in text)
                    {
                        Console.Write(c);
                        Thread.Sleep(40);

                    }

                    Console.Clear();
                    UserProfile();
                }
            }
        }

        static bool PasswordChange(string newPassword)
        {
            bool passwordChanged = false;

            if (personalNumbersFromDb.Count == 0 || passwordsFromDb.Count == 0)
            {
                personalNumbersFromDb = System.IO.File.ReadAllLines(personalNumbersPath).ToList();
                passwordsFromDb = System.IO.File.ReadAllLines(passwordsPath).ToList();
            }

            for (int i = 0; i < personalNumbersFromDb.Count; i++)
            {
                string personalNumberFromDb = personalNumbersFromDb[i];
                string passFromDb = passwordsFromDb[i];

                if (Globals.personalNumber == personalNumberFromDb)
                {
                    passwordsFromDb[i] = newPassword;
                    passwordChanged = true;
                }
            }

            if (passwordChanged)
            {
                File.WriteAllLines(passwordsPath, passwordsFromDb.ToArray());
                return true;
            }

            return false;
        }

        public static bool DeleteBook(int bookIndex, string bookIsbn)
        {
            string[] bookLines = File.ReadAllLines(@booksPath);

            for (int i = 0; i < bookLines.Length; i++)
            {
                var testLine = bookLines[i];
                string[] lineParts = testLine.Split(",");
                string currentIsbn = lineParts[3];

                if (i == bookIndex)
                {
                    continue;
                }
                else
                {
                    var currentLine = $"{bookLines[i]}";
                    string[] books = { currentLine };
                    File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", books);
                }
            } 

            File.Delete(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt");
            File.Move(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt");
            booksPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt";
            return true;
        }

        public static bool ReturnBook(string returnIsbn)
        {
            string[] lines = File.ReadAllLines(@rentedBooksPath);
            for (int i = 0; i < lines.Length; i++)
            {

                if (i == Globals.rentedBookIndex)
                {
                    continue;
                }
                else
                {
                    var currentLine = $"{lines[i]}";
                    string[] rentedBooks = { currentLine };
                    File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", rentedBooks);
                }
            }

            File.Delete(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\RentedBooks.txt");
            File.Move(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\RentedBooks.txt");
            rentedBooksPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\RentedBooks.txt";
            return true;
        }

        public static bool DeleteUser(string personalNum, string filePath, int positionOfUser)
        {
            string[] lines = File.ReadAllLines(@usersPath);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] testAdmin = lines[positionOfUser].Split(' ');

                if (i == positionOfUser)
                {
                    continue;
                }
                else
                {
                    var currentLine = $"{lines[i]}";
                    string[] users = { currentLine };
                    File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", users);
                }
            }

            File.Delete(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Users.txt");
            File.Move(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Users.txt");
            usersPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Users.txt";
            return true;
        }

        public static bool MakeAdmin(string personalNum, string filepath, int positionOfUser)
        {
            string[] lines = File.ReadAllLines(@usersPath);
            string adminUser;
            string admin = "admin";
            string user = "user";

            for (int i = 0; i < lines.Length; i++)
            {
                string[] testAdmin = lines[positionOfUser].Split(' ');

                if (i==positionOfUser)
                {
                    i++;
                    if (testAdmin[3] == "user")
                    {
                        var adminLine = $"{testAdmin[0]} {testAdmin[1]} {testAdmin[2]} {admin}";
                        string[] newAdmin = { adminLine };
                        File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", newAdmin);
                        continue;
                    }
                    else
                    {
                        var adminLine = $"{testAdmin[0]} {testAdmin[1]} {testAdmin[2]} {user}";
                        string[] newAdmin = { adminLine };
                        File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", newAdmin);
                        continue;
                    }
                }
                else
                {
                    var currentLine = $"{lines[i]}";
                    string[] users = { currentLine };
                    File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", users);
                }
            }

            File.Delete(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Users.txt");
            File.Move(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Users.txt");
            usersPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Users.txt";
            return true;
        }

        public static bool ChangeBookAvailability(string bookISBN)
        {
            string[] bookLines = System.IO.File.ReadAllLines(@booksPath);
            string availableBook = "1";
            string unavailableBook = "0";

            for (int i = 0; i < bookLines.Length; i++)
            {
                var testLine = bookLines[i];
                string[] lineParts = testLine.Split(",");
                string currentIsbn = lineParts[3];
                string currentReservation = lineParts[5];

                if(currentIsbn == bookISBN)
                {
                    if (lineParts[4] == "1")
                    {
                        var bookLine = $"{lineParts[0]}{","}{lineParts[1]}{","}{lineParts[2]}{","}{lineParts[3]}{","}{unavailableBook}{","}{currentReservation}";
                        string[] bookUnavailable = { bookLine };
                        File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", bookUnavailable);
                        continue;
                    }
                    else
                    {
                        var bookLine = $"{lineParts[0]}{","}{lineParts[1]}{","}{lineParts[2]}{","}{lineParts[3]}{","}{availableBook}{","}{currentReservation}";
                        string[] bookAvailable = { bookLine };
                        File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", bookAvailable);
                        continue;
                    }
                }
                else
                {
                    var currentLine = $"{bookLines[i]}";
                    string[] currentBookLine = { currentLine };
                    File.AppendAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", currentBookLine);
                }
            }

            File.Delete(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt");
            File.Move(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\temp.txt", @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt");
            usersPath = @"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Books.txt";
            return true;
        }
        static bool Authentication(string personalNumber, string password)
        {
            string[] personalNumbersFromFile = System.IO.File.ReadAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Personalnumbers.txt");
            string[] passwordsFromFile = System.IO.File.ReadAllLines(@"C:\Users\artur.kaminski\source\repos\Library-Project-OOP\Library-Project-OOP\Passwords.txt");
            string[] users = System.IO.File.ReadAllLines(usersPath);

            for (int i = 0; i < personalNumbersFromFile.Length; i++)
            {
                string personalNumberFromFile = personalNumbersFromFile[i];
                string passwordFromFile = passwordsFromFile[i];

                if (personalNumber == personalNumberFromFile && password == passwordFromFile)
                {
                    var testLine = users[i];
                    string[] testParts = testLine.Split(' ');

                    Globals.firstName = testParts[0];
                    Globals.lastName = testParts[1];
                    Globals.userType = testParts[3];
                    Globals.password = password;
                    Globals.personalNumber = personalNumber;
                    Globals.userIndex = i;
                    return true;
                }
            }

            return false;

        }

        static bool UserInfoIncomplete(string? firstName, string? lastName, string? personalNum, string ? userType)
        {
            if (firstName == null) return true;
            if (lastName == null) return true;
            if (personalNum == null) return true;
            if (userType == null) return true;

            return false;
        }

        static bool UserExists(string personalNumber)
        {
            string[] users = System.IO.File.ReadAllLines(usersPath);

            for (int i = 0; i < users.Length; i++)
            {
                var testLine = users[i];
                string[] testParts = testLine.Split(' ');

                if (testParts[2] == personalNumber)
                {
                    return true;
                }
            }

            return false;
        }

        static void AppClose()
        {
            Environment.Exit(0);
        }
    }
}