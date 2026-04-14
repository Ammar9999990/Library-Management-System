# 📚 Intelligent Library Management System

An advanced C# console application that combines **NoSQL database management**, **Generative AI**, and **Machine Learning** to provide a smart book rental and recommendation experience.

---

## 🌟 Key Features

- **AI-Powered Metadata**  
  Automatically generates 2-sentence book summaries using the **Gemini 2.5 Flash API** when new books are added.

- **Machine Learning Recommendations**  
  Uses **ML.NET Matrix Factorization** to predict user ratings and recommend books based on rental history.

- **Hybrid Filtering**  
  Combines:
  - Collaborative Filtering (Machine Learning)
  - Content-Based Filtering (Genre Matching)  
  → Solves the **Cold Start Problem** for new users.

- **NoSQL Storage**  
  Fully integrated with **MongoDB** for flexible and scalable data storage.

- **Clean Architecture**  
  Structured into layers for maintainability:
  - Models
  - Data
  - Services
  - UI

---

## 🏗️ Project Structure

``` plaintext
LibraryManagementSystem/
├── Data/                    # Database Logic
│   └── LibraryDbContext.cs  # MongoDB connection and CRUD operations
│
├── Models/                  # Data Structures
│   ├── Book.cs              # Book entity with AI summary field
│   └── Rental.cs            # Rental transaction entity for ML training
│
├── Services/                # Intelligence Layer
│   ├── SummaryService.cs    # Gemini AI API handler (HttpClient)
│   └── RecommendationEngine.cs # ML.NET training and prediction logic
│
├── Program.cs               # Entry Point
└── .gitignore               # Git ignore rules

```


## 🚀 Getting Started

### ✅ Prerequisites

- .NET 8.0 SDK  
- MongoDB Community Server (running on `localhost:27017`)  
- Gemini API Key (from Google AI Studio)  

---

### ⚙️ Installation & Setup

#### 1. Clone the Repository
```bash
git clone <your-repo-url>
cd "Library Management System"

git clone <your-repo-url>
cd "Library Management System"


## **Configure API Key**
Open: Services/SummaryService.cs

Replace:

"YOUR_API_KEY"

with your actual Gemini API key.

Restore Dependencies

dotnet restore

Run the Application

dotnet run

🛠️ **Usage Guide**
Add Books
Use the Admin menu to add books.
The system will call the Gemini API to generate summaries.
Record Rentals
Simulate user activity by renting books.


👉 Recommended: At least 2–5 rentals before training.
Train Model
Select the Train option to process rental data and generate model.zip.
Get Recommendations
Enter a User ID to receive:
Predicted ratings
Match scores
AI-generated summaries