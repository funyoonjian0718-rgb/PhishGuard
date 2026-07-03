using Microsoft.ML;
using Microsoft.ML.Data;
using PhishGuard.Models;
using System.Text.RegularExpressions;

namespace PhishGuard.Services
{
    public class PhishingAnalysisService
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<PhishingTrainingData, PhishingPrediction> _predictionEngine;

        public PhishingAnalysisService()
        {
            _mlContext = new MLContext(seed: 1);

            var trainingData = GetTrainingData();

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "Features",
                    inputColumnName: nameof(PhishingTrainingData.Text)
                )
                .Append(_mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(
                    labelColumnName: nameof(PhishingTrainingData.Label),
                    featureColumnName: "Features"
                ));

            var model = pipeline.Fit(dataView);

            _predictionEngine = _mlContext.Model.CreatePredictionEngine<PhishingTrainingData, PhishingPrediction>(model);
        }

        public AnalysisResult AnalyzeEmail(EmailAnalysis email)
        {
            var detectedIssues = new List<string>();
            var recommendations = new List<string>();

            string combinedText = BuildCombinedText(email);

            var prediction = _predictionEngine.Predict(new PhishingTrainingData
            {
                Text = combinedText
            });

            int aiScore = Convert.ToInt32(prediction.Probability * 65);
            int ruleScore = CalculateRuleScore(email, detectedIssues);

            int finalScore = Math.Min(100, aiScore + ruleScore);

            string riskLevel = GetRiskLevel(finalScore);

            AddRecommendations(riskLevel, detectedIssues, recommendations);

            string summary = GenerateSummary(riskLevel, finalScore, prediction.Probability, detectedIssues);

            return new AnalysisResult
            {
                RiskScore = finalScore,
                RiskLevel = riskLevel,
                Summary = summary,
                DetectedIssues = detectedIssues,
                Recommendations = recommendations,
                AnalyzedAt = DateTime.Now
            };
        }

        private string BuildCombinedText(EmailAnalysis email)
        {
            return $@"
                Sender: {email.SenderEmail}
                Subject: {email.Subject}
                Body: {email.EmailBody}
                Link: {email.Link}
                Attachment: {email.AttachmentName}
            ";
        }

        private int CalculateRuleScore(EmailAnalysis email, List<string> detectedIssues)
        {
            int score = 0;

            string sender = email.SenderEmail?.ToLower() ?? "";
            string subject = email.Subject?.ToLower() ?? "";
            string body = email.EmailBody?.ToLower() ?? "";
            string link = email.Link?.ToLower() ?? "";
            string attachment = email.AttachmentName?.ToLower() ?? "";

            string allText = $"{sender} {subject} {body} {link} {attachment}";

            if (IsSuspiciousSender(sender))
            {
                score += 15;
                detectedIssues.Add("Suspicious sender email address detected.");
            }

            if (ContainsUrgentLanguage(allText))
            {
                score += 15;
                detectedIssues.Add("Urgent or threatening language detected.");
            }

            if (ContainsCredentialRequest(allText))
            {
                score += 20;
                detectedIssues.Add("The email requests sensitive information such as password, OTP, login, or banking details.");
            }

            if (ContainsFinancialOrAccountWarning(allText))
            {
                score += 10;
                detectedIssues.Add("The email mentions account suspension, payment, verification, or financial action.");
            }

            if (IsSuspiciousLink(link))
            {
                score += 20;
                detectedIssues.Add("Suspicious or shortened link detected.");
            }

            if (IsRiskyAttachment(attachment))
            {
                score += 15;
                detectedIssues.Add("Risky attachment type detected.");
            }

            if (ContainsRewardOrPrizeLanguage(allText))
            {
                score += 10;
                detectedIssues.Add("Reward, prize, or unrealistic offer language detected.");
            }

            if (detectedIssues.Count == 0)
            {
                detectedIssues.Add("No major phishing indicators were detected.");
            }

            return Math.Min(score, 70);
        }

        private bool IsSuspiciousSender(string sender)
        {
            if (string.IsNullOrWhiteSpace(sender))
                return false;

            bool usesFreeEmailForSupport =
                sender.Contains("support") &&
                (sender.Contains("@gmail.com") ||
                 sender.Contains("@yahoo.com") ||
                 sender.Contains("@hotmail.com") ||
                 sender.Contains("@outlook.com"));

            bool hasVerifyKeyword =
                sender.Contains("verify") ||
                sender.Contains("security") ||
                sender.Contains("account") ||
                sender.Contains("admin");

            bool hasStrangeFormat =
                Regex.IsMatch(sender, @"\d{4,}") ||
                sender.Contains("secure-login") ||
                sender.Contains("customer-service");

            return usesFreeEmailForSupport || hasVerifyKeyword || hasStrangeFormat;
        }

        private bool ContainsUrgentLanguage(string text)
        {
            string[] urgentWords =
            {
                "urgent",
                "immediately",
                "within 24 hours",
                "last warning",
                "final warning",
                "act now",
                "limited time",
                "your account will be suspended",
                "account suspended",
                "blocked",
                "locked",
                "verify now"
            };

            return urgentWords.Any(word => text.Contains(word));
        }

        private bool ContainsCredentialRequest(string text)
        {
            string[] credentialWords =
            {
                "password",
                "otp",
                "one time password",
                "login",
                "log in",
                "username",
                "bank account",
                "credit card",
                "debit card",
                "pin number",
                "security code",
                "confirm your identity",
                "update your details"
            };

            return credentialWords.Any(word => text.Contains(word));
        }

        private bool ContainsFinancialOrAccountWarning(string text)
        {
            string[] warningWords =
            {
                "payment",
                "invoice",
                "refund",
                "transaction",
                "bank",
                "account",
                "verify your account",
                "account verification",
                "suspended",
                "unauthorized access",
                "billing"
            };

            return warningWords.Any(word => text.Contains(word));
        }

        private bool ContainsRewardOrPrizeLanguage(string text)
        {
            string[] rewardWords =
            {
                "winner",
                "congratulations",
                "claim your prize",
                "free gift",
                "reward",
                "lottery",
                "bonus",
                "cash prize"
            };

            return rewardWords.Any(word => text.Contains(word));
        }

        private bool IsSuspiciousLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
                return false;

            bool isShortened =
                link.Contains("bit.ly") ||
                link.Contains("tinyurl") ||
                link.Contains("t.co") ||
                link.Contains("goo.gl") ||
                link.Contains("ow.ly");

            bool isHttpOnly =
                link.StartsWith("http://");

            bool hasIpAddress =
                Regex.IsMatch(link, @"http[s]?://\d{1,3}(\.\d{1,3}){3}");

            bool hasSuspiciousWords =
                link.Contains("login") ||
                link.Contains("verify") ||
                link.Contains("secure") ||
                link.Contains("update") ||
                link.Contains("account");

            return isShortened || isHttpOnly || hasIpAddress || hasSuspiciousWords;
        }

        private bool IsRiskyAttachment(string attachment)
        {
            if (string.IsNullOrWhiteSpace(attachment))
                return false;

            string[] riskyExtensions =
            {
                ".zip",
                ".rar",
                ".exe",
                ".bat",
                ".cmd",
                ".scr",
                ".js",
                ".vbs",
                ".msi",
                ".iso"
            };

            return riskyExtensions.Any(ext => attachment.EndsWith(ext));
        }

        private string GetRiskLevel(int score)
        {
            if (score >= 70)
                return "Phishing";

            if (score >= 35)
                return "Suspicious";

            return "Safe";
        }

        private void AddRecommendations(string riskLevel, List<string> detectedIssues, List<string> recommendations)
        {
            if (riskLevel == "Phishing")
            {
                recommendations.Add("Do not click any links or open any attachments in this email.");
                recommendations.Add("Do not provide passwords, OTP codes, banking details, or personal information.");
                recommendations.Add("Report the email to the relevant organization or IT/security team.");
                recommendations.Add("Delete the email after reporting it.");
            }
            else if (riskLevel == "Suspicious")
            {
                recommendations.Add("Verify the sender through an official website or trusted contact method.");
                recommendations.Add("Avoid clicking links until the email is confirmed to be legitimate.");
                recommendations.Add("Check the domain name, attachment type, and message wording carefully.");
            }
            else
            {
                recommendations.Add("The email appears safe based on the current analysis.");
                recommendations.Add("Continue to be cautious with unexpected links or attachments.");
            }
        }

        private string GenerateSummary(string riskLevel, int finalScore, float aiProbability, List<string> detectedIssues)
        {
            int aiPercentage = Convert.ToInt32(aiProbability * 100);

            if (riskLevel == "Phishing")
            {
                return $"The email is classified as Phishing with a risk score of {finalScore}. The AI model estimated a phishing probability of {aiPercentage}%, and the system detected multiple phishing indicators.";
            }

            if (riskLevel == "Suspicious")
            {
                return $"The email is classified as Suspicious with a risk score of {finalScore}. The AI model estimated a phishing probability of {aiPercentage}%, and some suspicious indicators were detected.";
            }

            return $"The email is classified as Safe with a risk score of {finalScore}. The AI model estimated a phishing probability of {aiPercentage}%, and no major phishing indicators were found.";
        }

        private List<PhishingTrainingData> GetTrainingData()
        {
            return new List<PhishingTrainingData>
            {
                new PhishingTrainingData
                {
                    Text = "Urgent your account will be suspended verify your password immediately click this link login now",
                    Label = true
                },
                new PhishingTrainingData
                {
                    Text = "Your bank account has been locked click here to update your details and confirm your identity",
                    Label = true
                },
                new PhishingTrainingData
                {
                    Text = "Congratulations you won a cash prize claim your reward now by entering your personal details",
                    Label = true
                },
                new PhishingTrainingData
                {
                    Text = "Payment failed update your billing information immediately to avoid account suspension",
                    Label = true
                },
                new PhishingTrainingData
                {
                    Text = "Security alert suspicious login detected verify your account and enter your OTP",
                    Label = true
                },
                new PhishingTrainingData
                {
                    Text = "Invoice attached open the zip file and complete payment immediately",
                    Label = true
                },
                new PhishingTrainingData
                {
                    Text = "Dear customer your account has unusual activity click the secure login link to continue",
                    Label = true
                },
                new PhishingTrainingData
                {
                    Text = "Final warning your email account will be closed unless you verify your login details",
                    Label = true
                },
                new PhishingTrainingData
                {
                    Text = "Meeting reminder for tomorrow at 10am please review the agenda before the discussion",
                    Label = false
                },
                new PhishingTrainingData
                {
                    Text = "Your assignment feedback has been uploaded to the learning portal please check when available",
                    Label = false
                },
                new PhishingTrainingData
                {
                    Text = "Thank you for your order your receipt is attached for your reference",
                    Label = false
                },
                new PhishingTrainingData
                {
                    Text = "Project update the latest report is ready for review by the team",
                    Label = false
                },
                new PhishingTrainingData
                {
                    Text = "Your appointment has been confirmed please arrive 10 minutes early",
                    Label = false
                },
                new PhishingTrainingData
                {
                    Text = "Welcome to the course this email contains general information about the semester",
                    Label = false
                },
                new PhishingTrainingData
                {
                    Text = "The company newsletter for this month is now available",
                    Label = false
                },
                new PhishingTrainingData
                {
                    Text = "Please find attached the meeting minutes from yesterday",
                    Label = false
                }
            };
        }
    }

    public class PhishingTrainingData
    {
        public string Text { get; set; } = string.Empty;

        public bool Label { get; set; }
    }

    public class PhishingPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}