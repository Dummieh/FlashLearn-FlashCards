using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Transactions;
using System.Xml.Linq;

class FlashCard  //Flashcard class: Holds individual flashcards, each with an ID, question, and answer
{
    public int Id { get; set; }  // Unique ID or just numbering for each flashcard to identify them for deletion
    public string Question { get; set; } // The text for the flashcard question
    public string Answer { get; set; }  //The flashcards answer text or input something
}

/* Represents the user playing the game, with properties like name and high score
   User class: Tracks the player’s name and high score. High scores are updated after each game if the score exceeds the previous high*/

public class User
{
    public string name { get; set; }
    public string password { get; set; }
    public string Username1 { get; set; }  // User name, used for the greeting with players name part


    public User(string Name1, string Password, string username1)
    {
        name = Name1;
        password = Password;
        Username1 = username1;
    }


    public virtual void Display()
    {
        Console.WriteLine($"Username: ");
    }
}



class Username:User
{
    
    public int HighScore { get; private set; }  //User highest score, updated if beaten previous score

    public Username(string name, string password, string username1) : base (name, password, username1)  //constructor initializes\calls on the user's name
    {
        Username1 = name;
        HighScore = 0;  // zero high score at start
        LoadHighScore();
    }

    public void UpdateHighScore(int score)  //Update the user high score if the new score is higher than previous score
    {
        if (score > HighScore)
        {
            HighScore = score;
            SaveHighScore();
        }
    }

    private void SaveHighScore()  //stores highest score
    {
        File.WriteAllText("user.txt", $"{Username1},{HighScore}");
    }

    private void LoadHighScore()  //loads highest score
    {
        if (File.Exists("user.txt"))
        {
            string[] data = File.ReadAllText("user.txt").Split(',');
            if (data[0] == Username1 && int.TryParse(data[1], out int score))
            {
                HighScore = score;
            }
        }
    }
}




class FlashCardDeck  //Manage the collection of flashcards, including CRUD operations, CRUD also begins here hell)
{
    private List<FlashCard> flashCards;  //List to store all flashcards
    private int nextId;  //ID counter for assigning unique IDs to flashcards

    public FlashCardDeck()
    {
        flashCards = new List<FlashCard>();
        nextId = 1;  // Initialize the ID counter(starting from 1 automatically)
        LoadFlashCards();
    }

    public void AddFlashCard(string question, string answer)  //Add a new flashcard to the deck
    {
        flashCards.Add(new FlashCard { Id = nextId++, Question = question, Answer = answer });
        SaveFlashCards();
        Console.WriteLine("FlashCard added successfully.");
    }

    public void DisplayAllFlashCards()  // Displays all flashcards in the deck/screen or something
    {
        if (flashCards.Count == 0)
        {
            Console.WriteLine("No flashcards available.");
            return;
        }

        Console.WriteLine("\nList of FlashCards:");
        foreach (var card in flashCards)
        {
            Console.WriteLine($"ID: {card.Id} | Question: {card.Question} | Answer: {card.Answer}");
        }
    }

    public void UpdateFlashCard(int id, string newQuestion, string newAnswer) // Updates a flashcard by ID(if existingg)
    {
        var card = flashCards.Find(c => c.Id == id);
        if (card != null)
        {
            card.Question = newQuestion;
            card.Answer = newAnswer;
            SaveFlashCards();
            Console.WriteLine("FlashCard updated successfully.");
        }
        else
        {
            Console.WriteLine("Invalid ID: FlashCard not found.");
        }
    }

    public void RemoveFlashCard(int id)  //Removes a flashcard by ID(if existing)
    {
        if (flashCards.RemoveAll(c => c.Id == id) > 0)
        {
            SaveFlashCards();
            Console.WriteLine("FlashCard deleted.");
        }
        else
        {
            Console.WriteLine("Invalid ID: FlashCard not found.");
        }
    }

    public List<FlashCard> GetAllFlashCards()  //Retrieves the list of flashcards user has made in the game
    {
        return flashCards;
    }

    private void SaveFlashCards()
    {
        using (StreamWriter writer = new StreamWriter("flashcards.txt"))
        {
            foreach (var card in flashCards)
            {
                writer.WriteLine($"{card.Id}|{card.Question}|{card.Answer}");
            }
        }
    }

    private void LoadFlashCards()
    {
        if (File.Exists("flashcards.txt"))
        {
            foreach (var line in File.ReadAllLines("flashcards.txt"))
            {
                string[] parts = line.Split('|');
                if (parts.Length == 3 &&
                    int.TryParse(parts[0], out int id))
                {
                    flashCards.Add(new FlashCard { Id = id, Question = parts[1], Answer = parts[2] });
                    nextId = Math.Max(nextId, id + 1);
                }
            }
        }
    }
}

class GameManager  //Manages flashcard game, including gameplay logic and score tracking
{
    private FlashCardDeck flashCardDeck;  //Reference to the flashcard deck
    private Username user;  //Reference to the user playing the game

    public GameManager(FlashCardDeck flashCardDeck)  //Initializes game with a user and a flashcard deck
    {
        
        this.flashCardDeck = flashCardDeck;
    }

    public GameManager(string username, FlashCardDeck deck)
    {
    }

    public void StartGame()  // Starts game, iterating through flashcards and checking answers
    {
        Console.Clear();
        Console.WriteLine($"\nWelcome {user.Username1}! Starting the game with {flashCardDeck.GetAllFlashCards().Count} flashcards.\n");

        int correctAnswers = 0;  //Counts the number of correct answers
        foreach (var card in flashCardDeck.GetAllFlashCards())
        {
            Console.WriteLine(card.Question);
            string userAnswer = Console.ReadLine();

            if (userAnswer.Equals(card.Answer, StringComparison.OrdinalIgnoreCase))  //Check if the user answer is correct(case doesnt matter)
            {
                Console.WriteLine("Correct!\n");
                correctAnswers++;
            }
            else
            {
                Console.WriteLine($"Wrong! The correct answer is {card.Answer}.\n");
            }
        }

        Console.WriteLine($"Game over! Your score: {correctAnswers}/{flashCardDeck.GetAllFlashCards().Count}.\n");
        user.UpdateHighScore(correctAnswers);  // Updates high score
    }
}



class Program  // The main program class that coordinates user interaction and the menu system
{

    static List <User> users = new List <User> ();


    static void Main(string[] args)
    {
        Mainmenu();
        
    }

   

    static void PauseForMenu() // Pauses program to allow the user to view results
    {
        Console.WriteLine("\nPress Enter to return to main menu...");
        Console.ReadLine();
        Console.Clear(); // Clears the words after pressing any button, before showing the menu again
    }


    static void Mainmenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("FLASHLEARN");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Exit");

            try
            {
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            Login();
                            break;

                        case 2:
                            Register();
                            break;

                        case 3:
                            Environment.Exit(0);
                            break;

                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("\n\t\t\t\t                     Press Enter to continue...");
            Console.ReadLine();
        }

    }

    static void HomePage(User user) {
        
        
        FlashCardDeck deck = new FlashCardDeck();  // Create anew empty flashcard deck
        

        Console.Clear();
        Console.WriteLine("////////////////////////////////////");
        Console.WriteLine($"* Welcome to FlashLearn *");
        Console.WriteLine("////////////////////////////////////\n");


        Console.WriteLine("=============================");
        Console.WriteLine("|| 1. Add a FlashCard      ||");
        Console.WriteLine("|| 2. View all FlashCards  ||");
        Console.WriteLine("|| 3. Update a FlashCard   ||");
        Console.WriteLine("|| 4. Delete a FlashCard   ||");
        Console.WriteLine("|| 5. Start the Game       ||");
        Console.WriteLine("|| 6. Exit                 ||");
        Console.WriteLine("=============================");
        Console.Write("  >Choose an option: ");

        bool keepRunning = true;
        while (keepRunning)
        {
            
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":  // Add FlashCards
                    Console.Write("Enter the question: ");
                    string question = Console.ReadLine();
                    Console.Write("Enter the answer: ");
                    string answer = Console.ReadLine();
                    deck.AddFlashCard(question, answer);
                    PauseForMenu();
                    break;

                case "2":  // View FlashCards
                    deck.DisplayAllFlashCards();
                    PauseForMenu();
                    break;

                case "3":  // Update FlashCards
                    Console.Write("Enter the ID of the FlashCard to update: ");
                    if (int.TryParse(Console.ReadLine(), out int updateId))
                    {
                        Console.Write("Enter the new question: ");
                        string newQuestion = Console.ReadLine();
                        Console.Write("Enter the new answer: ");
                        string newAnswer = Console.ReadLine();
                        deck.UpdateFlashCard(updateId, newQuestion, newAnswer);
                    }
                    else
                    {
                        Console.WriteLine("Invalid input: Please enter a numeric ID.");
                    }
                    PauseForMenu();
                    break;

                case "4":  // Delete FlashCards
                    Console.Write("Enter the ID of the FlashCard to delete: ");
                    if (int.TryParse(Console.ReadLine(), out int deleteId))
                    {
                        deck.RemoveFlashCard(deleteId);
                    }
                    else
                    {
                        Console.WriteLine("Invalid input: Please enter a numeric ID.");
                    }
                    PauseForMenu();
                    break;

                case "5":  // Start game
                    if (deck.GetAllFlashCards().Count == 0)
                    {
                        Console.WriteLine("No flashcards available to start the game. Add flashcards first."); //if no flashcards, display this
                    }
                    else
                    {
                        GameManager gameManager = new GameManager(deck); //else start the game if flashcards found
                        gameManager.StartGame();
                    }
                    PauseForMenu();
                    break;

                case "6":  //Exit game
                    keepRunning = false;
                    Console.WriteLine("Thank you for playing. Goodbye!");

                    break;

                default:
                    Console.WriteLine("Invalid option. Please choose again.");
                    PauseForMenu();
                    break;
            }
        }
    }

    static void Register()
    {
        Console.Clear();
        Console.WriteLine("SIGN UP NOW!");
        Console.WriteLine("Enter username: ");
        string name = Console.ReadLine()!;
        Console.WriteLine("Enter password: ");
        string password = ReadPassword();
        Console.WriteLine("\nEnter Account Username: ");
        string user = Console.ReadLine()!;

        User user1 = new User (name, password, user);

        if (users.Any(u => u.Username1 == name)) // Scans through the list if the same username exists
        {
            Console.Clear();
            Console.WriteLine("Username already exists. Please try again.");
        }
        else
        {
            Console.WriteLine("Account Created Successfully");
            users.Add(user1);
            SaveUserInfo(user1);
        }
    }

    static void Login()
    {

        LoadUserInfo();

        Console.Clear();
        Console.WriteLine("Welcome To Flash! Please Log in!");
        Console.WriteLine("Enter username: ");
        string name2 = Console.ReadLine();
        Console.WriteLine("Enter password: ");
        string pass2 = ReadPassword();

        User user = users.FirstOrDefault(u => u.name == name2 && u.password == pass2);

        if (user != null) 
        {
            HomePage(user);
        }
        else
        {
            Console.WriteLine("Invalid Username or Password. Please Try Again!");

        }


       

       
    }


    static void SaveUserInfo(User user)
    {
        string userData = $"{user.name}:{user.password}:{user.Username1}\n";
        File.AppendAllText("user.txt", userData);
    }

    static void LoadUserInfo()
    {
        if (File.Exists("user_data.txt"))
        {
            string[] lines = File.ReadAllLines("user_data.txt");

            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 4) // Ensure there are four parts: username, password, name, email
                {
                    users.Add(new User(parts[0], parts[1], parts[2]));
                }
            }
        }
    }
    public static string ReadPassword()
    {
        StringBuilder password = new StringBuilder();
        ConsoleKeyInfo keyInfo;

        do
        {
            keyInfo = Console.ReadKey(intercept: true);
            if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter)
            {
                password.Append(keyInfo.KeyChar);
                Console.Write("*");
            }
            else if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
        } while (keyInfo.Key != ConsoleKey.Enter);
        return password.ToString();
    }
}