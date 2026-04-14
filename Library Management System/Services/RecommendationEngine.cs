using LibraryManagementSystem.Models;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace LibraryManagementSystem.Services;

public class RecommendationEngine
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private const string ModelPath = "model.zip";

    public RecommendationEngine()
    {
        _mlContext = new MLContext();
    }

    public void Train(List<Rental> rentals)
    {
        // 1. Safety check: Matrix Factorization needs at least some data to avoid crashing
        if (rentals.Count < 2)
        {
            Console.WriteLine("[!] Not enough rental data to train. Need at least 2 records.");
            return;
        }

        IDataView dataView = _mlContext.Data.LoadFromEnumerable(rentals);

        // 2. Stabilized Options
        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = "UserIdEncoded",
            MatrixRowIndexColumnName = "BookIdEncoded",
            LabelColumnName = nameof(Rental.Label),
            NumberOfIterations = 50,
            Lambda = 0.2f,       // Prevents NaN
            Alpha = 0.01f,       // Steady learning
            C = 0.0001f,
            Quiet = true         // Set to true to keep the console clean for our neat UI
        };

        // 3. Build the pipeline
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                outputColumnName: "UserIdEncoded",
                inputColumnName: nameof(Rental.UserId))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                outputColumnName: "BookIdEncoded",
                inputColumnName: nameof(Rental.BookId)))
            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));

        // 4. Train and Save
        Console.WriteLine("\n[System] ML Engine is analyzing patterns...");
        _model = pipeline.Fit(dataView);
        _mlContext.Model.Save(_model, dataView.Schema, ModelPath);
        Console.WriteLine("[System] Model updated and saved successfully.");
    }

    public float PredictScore(uint userId, uint bookId)
    {
        try
        {
            if (_model == null)
            {
                if (!System.IO.File.Exists(ModelPath)) return float.NaN;
                _model = _mlContext.Model.Load(ModelPath, out _);
            }

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<Rental, Prediction>(_model);
            var prediction = predictionEngine.Predict(new Rental { UserId = userId, BookId = bookId });

            return prediction.Score;
        }
        catch
        {
            // If the model hasn't seen this User or Book before, it might throw an error.
            // Returning NaN tells our Hybrid Logic to fall back to Genre-only mode.
            return float.NaN;
        }
    }
}

public class Prediction
{
    public float Score { get; set; }
}