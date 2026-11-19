using TestTakingService.Contracts.Attempts;
using TestTakingService.Domain.Entities;
using TestTakingService.Domain.ValueObjects;

namespace TestTakingService.Services.Implementations;

public sealed record GradedQuestionItem(
    Guid QuestionUid,
    bool IsCorrect,
    decimal PointsEarned,
    QuestionResultDto ResultDto);

public sealed record GradingOutcome(
    decimal TotalScore,
    decimal MaxScore,
    decimal Percentage,
    bool Passed,
    IReadOnlyList<GradedQuestionItem> Items);

public class GradingService
{
    public GradingOutcome Grade(AttemptSnapshot snapshot, IReadOnlyCollection<Answer> answers)
    {
        var answersByQuestionUid = answers.ToDictionary(a => a.QuestionUid);
        var gradedItems = new List<GradedQuestionItem>(snapshot.Questions.Count);

        decimal totalScore = 0m;
        decimal maxScore = 0m;

        foreach (var question in snapshot.Questions.OrderBy(q => q.OrderIndex))
        {
            maxScore += question.Points;
            answersByQuestionUid.TryGetValue(question.QuestionUid, out var answer);

            var isCorrect = EvaluateQuestion(question, answer);
            var pointsEarned = isCorrect ? question.Points : 0m;
            totalScore += pointsEarned;

            var selectedOptionUids = answer?.SelectedOptionUids ?? [];
            var textAnswer = answer?.TextAnswer;

            var resultDto = new QuestionResultDto(
                QuestionUid: question.QuestionUid,
                Title: question.Title,
                QuestionType: question.QuestionType,
                SelectedOptionUids: selectedOptionUids,
                TextAnswer: textAnswer,
                IsCorrect: isCorrect,
                PointsEarned: pointsEarned,
                MaxPoints: question.Points);

            gradedItems.Add(new GradedQuestionItem(
                QuestionUid: question.QuestionUid,
                IsCorrect: isCorrect,
                PointsEarned: pointsEarned,
                ResultDto: resultDto));
        }

        var percentage = maxScore > 0m
            ? Math.Round(totalScore / maxScore * 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        var passed = percentage >= snapshot.PassingScore;

        return new GradingOutcome(
            TotalScore: totalScore,
            MaxScore: maxScore,
            Percentage: percentage,
            Passed: passed,
            Items: gradedItems);
    }

    private static bool EvaluateQuestion(AttemptQuestionSnapshot question, Answer? answer)
    {
        if (answer is null)
            return false;

        if (string.Equals(question.QuestionType, "TextInput", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(question.CorrectTextAnswer) || string.IsNullOrWhiteSpace(answer.TextAnswer))
                return false;

            return string.Equals(
                question.CorrectTextAnswer.Trim(),
                answer.TextAnswer.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        var correctOptionUids = question.Options
            .Where(o => o.IsCorrect == true)
            .Select(o => o.Uid)
            .ToHashSet();

        if (correctOptionUids.Count == 0 || answer.SelectedOptionUids.Count == 0)
            return false;

        var selectedSet = answer.SelectedOptionUids.ToHashSet();

        if (string.Equals(question.QuestionType, "SingleChoice", StringComparison.OrdinalIgnoreCase))
        {
            return selectedSet.Count == 1 && correctOptionUids.SetEquals(selectedSet);
        }

        if (string.Equals(question.QuestionType, "MultipleChoice", StringComparison.OrdinalIgnoreCase))
        {
            return correctOptionUids.SetEquals(selectedSet);
        }

        return false;
    }
}