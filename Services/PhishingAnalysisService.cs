using System.Text.RegularExpressions;
using PhishGuard.Models;

namespace PhishGuard.Services  //checks the sender, subject, emailbody, link and attatchment 
{
    public class PhishingAnalysisService
    {
        public AnalysisResult AnalyzeEmail(EmailAnalysis email)
        {
            int score = 0;
            List<string> issues = new List<string>();
            List<string> recommendations = new List<string>();

            string sender = email.SenderEmail.ToLower();
            string subject = email.Subject.ToLower();
            string body = email.EmailBody.ToLower();
            string link = email.Link.ToLower();
            string attachment = email.AttachmentName.ToLower();

            CheckSender(sender, ref score, issues);
            CheckSubject(subject, ref score, issues);
            CheckBody(body, ref score, issues);
            CheckLink(link, ref score, issues);
            CheckAttachment(attachment, ref score, issues);

            if (score > 100)
            {
                score = 100;
            }

            string riskLevel = GetRiskLevel(score);
            string summary = GenerateSummary(riskLevel, score);

            recommendations.Add("Do not click suspicious links before verifying the sender.");
            recommendations.Add("Check the sender domain carefully before replying.");
            recommendations.Add("Do not share passwords, OTP codes, or banking details through email.");
            recommendations.Add("Contact the official company through its verified website if unsure.");

            return new AnalysisResult
            {
                RiskScore = score,
                RiskLevel = riskLevel,
                Summary = summary,
                DetectedIssues = issues,
                Recommendations = recommendations,
                AnalyzedAt = DateTime.Now
            };
        }

        private void CheckSender(string sender, ref int score, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(sender))
            {
                score += 10;
                issues.Add("Sender email is missing.");
                return;
            }

            string[] freeDomains = { "gmail.com", "yahoo.com", "hotmail.com", "outlook.com" };

            foreach (string domain in freeDomains)
            {
                if (sender.EndsWith(domain))
                {
                    score += 10;
                    issues.Add("Sender uses a free email domain, which may be suspicious for official business emails.");
                    break;
                }
            }

            if (sender.Contains("support") && sender.Contains("gmail.com"))
            {
                score += 15;
                issues.Add("Sender pretends to be support but uses a free email account.");
            }

            if (sender.Contains("secure") || sender.Contains("verify") || sender.Contains("account"))
            {
                score += 10;
                issues.Add("Sender email contains suspicious security-related wording.");
            }
        }

        private void CheckSubject(string subject, ref int score, List<string> issues)
        {
            string[] suspiciousWords =
            {
                "urgent", "verify now", "account suspended", "password expired",
                "security alert", "limited time", "immediate action",
                "payment failed", "confirm your account"
            };

            foreach (string word in suspiciousWords)
            {
                if (subject.Contains(word))
                {
                    score += 15;
                    issues.Add($"Subject contains suspicious phrase: '{word}'.");
                }
            }
        }

        private void CheckBody(string body, ref int score, List<string> issues)
        {
            string[] phishingPhrases =
            {
                "click here", "verify your account", "login immediately",
                "your account will be suspended", "enter your password",
                "confirm your identity", "update your payment",
                "bank account", "otp", "one time password"
            };

            foreach (string phrase in phishingPhrases)
            {
                if (body.Contains(phrase))
                {
                    score += 12;
                    issues.Add($"Email body contains phishing-related phrase: '{phrase}'.");
                }
            }

            if (body.Contains("http://"))
            {
                score += 10;
                issues.Add("Email contains a non-secure HTTP link.");
            }

            if (body.Contains("password") && body.Contains("account"))
            {
                score += 15;
                issues.Add("Email asks about account/password information.");
            }
        }

        private void CheckLink(string link, ref int score, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return;
            }

            if (link.StartsWith("http://"))
            {
                score += 15;
                issues.Add("The link uses HTTP instead of HTTPS.");
            }

            if (link.Contains("bit.ly") || link.Contains("tinyurl") || link.Contains("t.co"))
            {
                score += 20;
                issues.Add("The link uses a shortened URL, which can hide the real destination.");
            }

            if (Regex.IsMatch(link, @"\d{1,3}(\.\d{1,3}){3}"))
            {
                score += 25;
                issues.Add("The link contains an IP address instead of a normal domain.");
            }

            if (link.Contains("login") || link.Contains("verify") || link.Contains("secure"))
            {
                score += 10;
                issues.Add("The link contains suspicious login or verification wording.");
            }
        }

        private void CheckAttachment(string attachment, ref int score, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(attachment))
            {
                return;
            }

            string[] riskyExtensions = { ".exe", ".zip", ".rar", ".bat", ".js", ".scr" };

            foreach (string extension in riskyExtensions)
            {
                if (attachment.EndsWith(extension))
                {
                    score += 20;
                    issues.Add($"Attachment has a risky file extension: {extension}.");
                }
            }
        }

        private string GetRiskLevel(int score)
        {
            if (score >= 70)
            {
                return "Phishing";
            }

            if (score >= 35)
            {
                return "Suspicious";
            }

            return "Safe";
        }

        private string GenerateSummary(string riskLevel, int score)
        {
            if (riskLevel == "Phishing")
            {
                return $"This email is highly suspicious and likely to be phishing. The calculated risk score is {score}/100.";
            }

            if (riskLevel == "Suspicious")
            {
                return $"This email contains several suspicious indicators. The calculated risk score is {score}/100.";
            }

            return $"This email has a low phishing risk based on the current checks. The calculated risk score is {score}/100.";
        }
    }
}