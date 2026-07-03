-- MySQL dump 10.13  Distrib 8.0.41, for Win64 (x86_64)
--
-- Host: localhost    Database: phishguard_db
-- ------------------------------------------------------
-- Server version	8.0.41

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `emailscanrecords`
--

DROP TABLE IF EXISTS `emailscanrecords`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `emailscanrecords` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `SenderEmail` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Subject` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `EmailBody` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Link` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `AttachmentName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RiskScore` int NOT NULL,
  `RiskLevel` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Summary` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DetectedIssuesJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RecommendationsJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `emailscanrecords`
--

LOCK TABLES `emailscanrecords` WRITE;
/*!40000 ALTER TABLE `emailscanrecords` DISABLE KEYS */;
INSERT INTO `emailscanrecords` VALUES (1,'support.verify.account@gmail.com','Urgent: Your Account Will Be Suspended','Dear user, your account will be suspended. Click here to verify your account and enter your password immediately.','http://bit.ly/secure-login','invoice.zip',100,'Phishing','This email is highly suspicious and likely to be phishing. The calculated risk score is 100/100.','[\"Sender uses a free email domain, which may be suspicious for official business emails.\",\"Sender pretends to be support but uses a free email account.\",\"Sender email contains suspicious security-related wording.\",\"Subject contains suspicious phrase: \\u0027urgent\\u0027.\",\"Email body contains phishing-related phrase: \\u0027click here\\u0027.\",\"Email body contains phishing-related phrase: \\u0027verify your account\\u0027.\",\"Email body contains phishing-related phrase: \\u0027your account will be suspended\\u0027.\",\"Email body contains phishing-related phrase: \\u0027enter your password\\u0027.\",\"Email asks about account/password information.\",\"The link uses HTTP instead of HTTPS.\",\"The link uses a shortened URL, which can hide the real destination.\",\"The link contains suspicious login or verification wording.\",\"Attachment has a risky file extension: .zip.\"]','[\"Do not click suspicious links before verifying the sender.\",\"Check the sender domain carefully before replying.\",\"Do not share passwords, OTP codes, or banking details through email.\",\"Contact the official company through its verified website if unsure.\"]','2026-06-27 23:59:09.559592'),(2,'support.verify.account@gmail.com','Urgent: Your Account Will Be Suspended','im gay','http://bit.ly/secure-login','notscam.zip',100,'Phishing','This email is highly suspicious and likely to be phishing. The calculated risk score is 100/100.','[\"Sender uses a free email domain, which may be suspicious for official business emails.\",\"Sender pretends to be support but uses a free email account.\",\"Sender email contains suspicious security-related wording.\",\"Subject contains suspicious phrase: \\u0027urgent\\u0027.\",\"The link uses HTTP instead of HTTPS.\",\"The link uses a shortened URL, which can hide the real destination.\",\"The link contains suspicious login or verification wording.\",\"Attachment has a risky file extension: .zip.\"]','[\"Do not click suspicious links before verifying the sender.\",\"Check the sender domain carefully before replying.\",\"Do not share passwords, OTP codes, or banking details through email.\",\"Contact the official company through its verified website if unsure.\"]','2026-06-29 11:25:22.803490'),(3,'info@apu.edu.my','Relocation of APU SWEATZONE Entrance','Dear Students,\n \nPlease be informed that the entrance to the APU SWEATZONE has been relocated to the side facing auditorium 5. \n \nThis change follows the recent installation of turnstiles at the Residences entry point and is intended to ensure convenient and uninterrupted access to the gym for all students.\n \nAll students are required to use their student ID to tap the security door access prior to entry.\n \nThank you for your cooperation and understanding.\n \nAPU Management\n\n\nCaution: This email originated from outside the organisation. Do not click links or open attachments unless you recognise the sender and know the content is safe. Click “Report Phishing” if you think it is malicious.','-','-',0,'Safe','This email has a low phishing risk based on the current checks. The calculated risk score is 0/100.','[]','[\"Do not click suspicious links before verifying the sender.\",\"Check the sender domain carefully before replying.\",\"Do not share passwords, OTP codes, or banking details through email.\",\"Contact the official company through its verified website if unsure.\"]','2026-06-29 11:26:45.549579'),(4,'info@apu.edu.my','Relocation of APU SWEATZONE Entrance','Dear Students,\n \nPlease be informed that the entrance to the APU SWEATZONE has been relocated to the side facing auditorium 5. \n \nThis change follows the recent installation of turnstiles at the Residences entry point and is intended to ensure convenient and uninterrupted access to the gym for all students.\n \nAll students are required to use their student ID to tap the security door access prior to entry.\n \nThank you for your cooperation and understanding.\n \nAPU Management\n\n\nCaution: This email originated from outside the organisation. Do not click links or open attachments unless you recognise the sender and know the content is safe. Click “Report Phishing” if you think it is malicious.','http://bit.ly/secure-login','-',45,'Suspicious','This email contains several suspicious indicators. The calculated risk score is 45/100.','[\"The link uses HTTP instead of HTTPS.\",\"The link uses a shortened URL, which can hide the real destination.\",\"The link contains suspicious login or verification wording.\"]','[\"Do not click suspicious links before verifying the sender.\",\"Check the sender domain carefully before replying.\",\"Do not share passwords, OTP codes, or banking details through email.\",\"Contact the official company through its verified website if unsure.\"]','2026-06-29 11:27:00.850253'),(5,'info@apu.edu.my','Relocation of APU SWEATZONE Entrance','Dear Students,\n \nPlease be informed that the entrance to the APU SWEATZONE has been relocated to the side facing auditorium 5. \n \nThis change follows the recent installation of turnstiles at the Residences entry point and is intended to ensure convenient and uninterrupted access to the gym for all students.\n \nAll students are required to use their student ID to tap the security door access prior to entry.\n \nThank you for your cooperation and understanding.\n \nAPU Management\n\n\nCaution: This email originated from outside the organisation. Do not click links or open attachments unless you recognise the sender and know the content is safe. Click “Report Phishing” if you think it is malicious.','-','invoice.zip',20,'Safe','This email has a low phishing risk based on the current checks. The calculated risk score is 20/100.','[\"Attachment has a risky file extension: .zip.\"]','[\"Do not click suspicious links before verifying the sender.\",\"Check the sender domain carefully before replying.\",\"Do not share passwords, OTP codes, or banking details through email.\",\"Contact the official company through its verified website if unsure.\"]','2026-06-29 11:27:11.767583'),(6,'info@apu.edu.my','Relocation of APU SWEATZONE Entrance','Dear Students,\n \nPlease be informed that the entrance to the APU SWEATZONE has been relocated to the side facing auditorium 5. \n \nThis change follows the recent installation of turnstiles at the Residences entry point and is intended to ensure convenient and uninterrupted access to the gym for all students.\n \nAll students are required to use their student ID to tap the security door access prior to entry.\n \nThank you for your cooperation and understanding.\n \nAPU Management\n\n\nCaution: This email originated from outside the organisation. Do not click links or open attachments unless you recognise the sender and know the content is safe. Click “Report Phishing” if you think it is malicious.','-','invoice.exe',20,'Safe','This email has a low phishing risk based on the current checks. The calculated risk score is 20/100.','[\"Attachment has a risky file extension: .exe.\"]','[\"Do not click suspicious links before verifying the sender.\",\"Check the sender domain carefully before replying.\",\"Do not share passwords, OTP codes, or banking details through email.\",\"Contact the official company through its verified website if unsure.\"]','2026-06-29 11:27:25.018073'),(7,'info@apu.edu.my','Relocation of APU SWEATZONE Entrance','Dear Students,\n \nPlease be informed that the entrance to the APU SWEATZONE has been relocated to the side facing auditorium 5. \n \nThis change follows the recent installation of turnstiles at the Residences entry point and is intended to ensure convenient and uninterrupted access to the gym for all students.\n \nAll students are required to use their student ID to tap the security door access prior to entry.\n \nThank you for your cooperation and understanding.\n \nAPU Management\n\n\nCaution: This email originated from outside the organisation. Do not click links or open attachments unless you recognise the sender and know the content is safe. Click “Report Phishing” if you think it is malicious.','http://bit.ly/secure-login','invoice.exe',65,'Suspicious','This email contains several suspicious indicators. The calculated risk score is 65/100.','[\"The link uses HTTP instead of HTTPS.\",\"The link uses a shortened URL, which can hide the real destination.\",\"The link contains suspicious login or verification wording.\",\"Attachment has a risky file extension: .exe.\"]','[\"Do not click suspicious links before verifying the sender.\",\"Check the sender domain carefully before replying.\",\"Do not share passwords, OTP codes, or banking details through email.\",\"Contact the official company through its verified website if unsure.\"]','2026-06-29 11:27:36.092638');
/*!40000 ALTER TABLE `emailscanrecords` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-06-30 14:54:08
