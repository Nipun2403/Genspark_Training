# 🟩 Off Brand Wordle V2: Now with 100% More Database

An attempt to copy the NY Times Greatest Game, now upgraded from a fragile in-memory script to a persistent, database. It is built using **Object-Oriented Programming (OOP)** standards, including Dependency Injection, ADO.NET, and Interface Segregation.



---

## ✨ Key Features (V2 Database Edition)

* **JIT User Authentication:** You can now log in! We create accounts Just-In-Time. Passwords are saved in glorious, unencrypted plaintext, because like we are yet to undergo the InfoSec training :P
* **Raw ADO.NET PostgreSQL:** We are talking directly to the database here. No Entity Framework Core touched.
* **Fired the Admin:** We completely scrapped the Admin UI module for adding words. We just write raw `INSERT INTO` SQL scripts to database, and hope the database doesn't catch fire.
* **Progress Tracking For Scores (Old V1):** Again, why miss bonus points?? Track your scores across various games in a Session History database.
* **Difficulty Levels (Old V1):** Levels are: Easy, Medium, and Hard tiers because we need all the extra credit we can get.
* **Visual Jazz (old V1):** Colorful and slightly wonky keyboard visual with colors to show you that it was indeed copied from wordle.
* **Hints (old V1):** 2 Hints per word, because how much more help do you need for a 5-letter word??
* **Attempt Based Comments (old V1):** Copied the exact table from the assignment :'O
* **Custom Exception Handling: (old V1)** Custom exception handling.



---

## 🏗️ Folder Structure

The application strictly adheres to the one file does one work rule, aka the **Single Responsibility Principle**.

```text
wordle/
│
├── Program.cs                         # Main Program (Now with a Login Screen)
├── wordle.csproj
│
├── Exceptions/
│   └── InvalidGuessException.cs       # Custom error handling
│
├── Models/
│   ├── User.cs                        # Tracks your bad scores
│   ├── Level.cs                       # Levels (Difficulty)
│   └── GameResult.cs                  
│
├── Interfaces/                       
│   ├── IAuthService.cs
│   ├── IWordProvider.cs
│   ├── IGuessValidator.cs
│   ├── IFeedbackGenerator.cs
│   ├── IHintManager.cs
│   ├── ISessionHistory.cs
│   └── IPraiseProvider.cs
│
├── Services/                          
│   ├── AuthService.cs                 # Plaintext password are stored here :P
│   ├── WordProvider.cs                # ADO.NET Postgres Word Fetcher
│   ├── GuessValidator.cs
│   ├── FeedbackGenerator.cs
│   ├── HintManager.cs
│   ├── SessionHistory.cs              # ADO.NET Postgres State Tracker
│   ├── PraiseProvider.cs
│   └── KeyboardTracker.cs             # For that fancy and crooked keyboard design
│
└── Core/
    └── GameEngine.cs                  

```

---

## 🚀 How to Play the Off-Brand Wordle V2

1. Make sure you have PostgreSQL running locally
2. Clone the repository to your local machine.
3. Navigate to the root directory containing the `.csproj` file.
4. Run the command:
```bash
dotnet run 
```



---

## 📜 My Game, My Rules

1. The system secretly chooses one 5-letter word.

2. You get **6 attempts**, take it or leave it.
3. After each guess, the terminal will colorize your letters:
* **Green:** The letter is in the word and in the correct spot.
* **Yellow:** The letter is in the word, but in the wrong spot.
* **Red:** The letter is not in the word.


4. Type `?` instead of a guess to use a Hint (Max 2 per round).
5. Type `0` to rage-quit back to the main menu without ruining your database score.

---

## 🖥️ Output

### 1. Login Screen
![alt text](<output_images/Screenshot 2026-05-12 at 9.04.59 PM.png>)

### 2. Main Menu
![alt text](<output_images/Screenshot 2026-05-12 at 9.05.14 PM.png>)

### 3. Session History
![alt text](<output_images/Screenshot 2026-05-12 at 9.05.31 PM.png>)
![alt text](<output_images/Screenshot 2026-05-12 at 9.05.42 PM.png>)

### 4. Leaderboard
![alt text](<output_images/Screenshot 2026-05-12 at 9.05.51 PM.png>)

### 5. Rules of the Game
![alt text](<output_images/Screenshot 2026-05-12 at 9.05.59 PM.png>)