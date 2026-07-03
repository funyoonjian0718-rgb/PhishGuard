document.addEventListener("DOMContentLoaded", function () {
    setupMobileMenu();

    // Authentication-related setup
    setupRegisterForm();
    setupLoginForm();
    setupLogoutButtons();

    // Check login before loading protected page data
    const isAllowed = protectPrivatePages();

    if (!isAllowed) {
        return;
    }

    showCurrentUser();

    // Page-specific backend loading
    setupScanTabs();
    setupScreenshotAnalyzeForm();
    setupAnalyzeForm();
    setupBulkAnalyzeForm();

    loadAnalysisResultPage();
    loadHistoryPage();
    loadScanDetailsPage();
    loadStats();
    loadReportsPage();
});

function setupRegisterForm() {
  const registerForm = document.getElementById("registerForm");

  if (!registerForm) return;

  registerForm.addEventListener("submit", async function (event) {
    event.preventDefault();

    const fullName = document.getElementById("fullName")?.value || "";
    const email = document.getElementById("email")?.value || "";
    const password = document.getElementById("password")?.value || "";
    const confirmPassword = document.getElementById("confirmPassword")?.value || "";

    if (password !== confirmPassword) {
      alert("Passwords do not match.");
      return;
    }

    const requestData = {
      fullName: fullName,
      email: email,
      password: password
    };

    try {
      const response = await fetch("/api/account/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(requestData)
      });

      const result = await response.json();

      if (!result.success) {
        alert(result.message);
        return;
      }

      alert("Registration successful. Please login.");
      window.location.href = "login.html";
    } catch (error) {
      alert("Registration failed: " + error.message);
    }
  });
}

function setupLoginForm() {
  const loginForm = document.getElementById("loginForm");

  if (!loginForm) return;

  loginForm.addEventListener("submit", async function (event) {
    event.preventDefault();

    const email = document.getElementById("email")?.value || "";
    const password = document.getElementById("password")?.value || "";

    const requestData = {
      email: email,
      password: password
    };

    try {
      const response = await fetch("/api/account/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(requestData)
      });

      const result = await response.json();

      if (!result.success) {
        alert(result.message);
        return;
      }

      localStorage.setItem("loggedInUser", JSON.stringify(result.user));

      window.location.href = "index.html";
    } catch (error) {
      alert("Login failed: " + error.message);
    }
  });
}

function setupLogoutButtons() {
  const logoutButtons = document.querySelectorAll("[data-logout]");

  logoutButtons.forEach(function (button) {
    button.addEventListener("click", function () {
      localStorage.removeItem("loggedInUser");
      window.location.href = "login.html";
    });
  });
}

function protectPrivatePages() {
    const publicPages = ["landing", "login", "register"];
    const page = document.body.dataset.page;

    if (publicPages.includes(page)) {
        return true;
    }

    const user = localStorage.getItem("loggedInUser");

    if (!user) {
        window.location.href = "login.html";
        return false;
    }

    return true;
}

function showCurrentUser() {
  const userData = localStorage.getItem("loggedInUser");

  if (!userData) return;

  const user = JSON.parse(userData);

  const userNameElements = document.querySelectorAll("[data-user-name]");
  const userEmailElements = document.querySelectorAll("[data-user-email]");

  userNameElements.forEach(function (element) {
    element.textContent = user.fullName || "User";
  });

  userEmailElements.forEach(function (element) {
    element.textContent = user.email || "";
  });
}

function setupMobileMenu() {
  const menuButton = document.querySelector("[data-mobile-menu]");
  const sidebar = document.querySelector(".sidebar");

  if (menuButton && sidebar) {
    menuButton.addEventListener("click", function () {
      sidebar.classList.toggle("open");
    });
  }
}

function setupScanTabs() {
    const tabs = document.querySelectorAll("[data-scan-tab]");
    const panels = {
        screenshot: document.getElementById("screenshotScanPanel"),
        manual: document.getElementById("manualScanPanel"),
        bulk: document.getElementById("bulkScanPanel")
    };

    if (!tabs || tabs.length === 0) return;

    tabs.forEach(tab => {
        tab.addEventListener("click", function () {
            const selectedTab = tab.dataset.scanTab;

            tabs.forEach(item => item.classList.remove("active"));

            Object.values(panels).forEach(panel => {
                if (panel) {
                    panel.classList.remove("active");
                }
            });

            tab.classList.add("active");

            if (panels[selectedTab]) {
                panels[selectedTab].classList.add("active");
            }
        });
    });
}

async function setupAnalyzeForm() {
    const analyzeForm = document.getElementById("analyzeForm");

    if (!analyzeForm) return;

    analyzeForm.addEventListener("submit", async function (event) {
        event.preventDefault();

        const formData = new FormData();

        formData.append("SenderEmail", document.getElementById("senderEmail").value);
        formData.append("Subject", document.getElementById("subject").value);
        formData.append("EmailBody", document.getElementById("emailBody").value);
        formData.append("Link", document.getElementById("link").value);

        const attachmentInput = document.getElementById("attachmentFile");
        const attachmentFile = attachmentInput && attachmentInput.files.length > 0
            ? attachmentInput.files[0]
            : null;

        if (attachmentFile) {
            formData.append("AttachmentFile", attachmentFile);
        }

        try {
            const response = await fetch("/api/emailanalysis/analyze-upload", {
                method: "POST",
                body: formData
            });

            if (!response.ok) {
                alert("Failed to analyze email.");
                return;
            }

            const result = await response.json();
            const attachmentInput = document.getElementById("attachmentFile");

            const emailData = {
                senderEmail: document.getElementById("senderEmail").value,
                subject: document.getElementById("subject").value,
                emailBody: document.getElementById("emailBody").value,
                link: document.getElementById("link").value,
                attachmentName: attachmentInput && attachmentInput.files.length > 0
                    ? attachmentInput.files[0].name
                    : "No attachment"
            };

            localStorage.setItem("latestEmailData", JSON.stringify(emailData));

            localStorage.setItem("latestAnalysisResult", JSON.stringify(result));

            window.location.href = "analysis-result.html";
        } catch (error) {
            console.error("Analyze upload error:", error);
            alert("Something went wrong while analyzing the email.");
        }
    });
}

function setupScreenshotAnalyzeForm() {
    const screenshotButton = document.getElementById("runScreenshotScanBtn");

    console.log("PHISHGUARD DEBUG: setupScreenshotAnalyzeForm called. Button:", screenshotButton);

    if (!screenshotButton) return;

    screenshotButton.addEventListener("click", async function () {
        console.log("PHISHGUARD DEBUG: Screenshot scan button clicked");

        const screenshotFileInput = document.getElementById("screenshotFile");
        const screenshotResults = document.getElementById("screenshotResults");

        if (!screenshotFileInput || screenshotFileInput.files.length === 0) {
            alert("Please upload an email screenshot.");
            return;
        }

        const selectedFile = screenshotFileInput.files[0];

        const formData = new FormData();

        const senderInput = document.getElementById("screenshotSenderEmail");
        const linkInput = document.getElementById("screenshotLink");

        formData.append("SenderEmail", senderInput ? senderInput.value : "");
        formData.append("Link", linkInput ? linkInput.value : "");
        formData.append("ScreenshotFile", selectedFile);

        if (screenshotResults) {
            screenshotResults.innerHTML = "<p class='help-text'>Analyzing screenshot...</p>";
        }

        try {
            console.log("PHISHGUARD DEBUG: Sending request to /api/emailanalysis/analyze-screenshot");

            const response = await fetch("/api/emailanalysis/analyze-screenshot", {
                method: "POST",
                body: formData
            });

            console.log("PHISHGUARD DEBUG: Screenshot response status:", response.status);

            if (!response.ok) {
                const errorText = await response.text();
                console.error("PHISHGUARD DEBUG: Screenshot analysis failed:", errorText);

                if (screenshotResults) {
                    screenshotResults.innerHTML = "<p class='help-text'>Screenshot analysis failed. Check console for details.</p>";
                }

                return;
            }

            const result = await response.json();

            console.log("PHISHGUARD DEBUG: Screenshot result:", result);

            const emailData = {
                senderEmail: senderInput ? senderInput.value : "screenshot-upload@phishguard.local",
                subject: "Screenshot scan: " + selectedFile.name,
                emailBody: "Screenshot uploaded and analyzed using OCR-based text extraction.",
                link: linkInput ? linkInput.value : "",
                attachmentName: selectedFile.name
            };

            localStorage.setItem("latestAnalysisResult", JSON.stringify(result));
            localStorage.setItem("latestEmailData", JSON.stringify(emailData));

            window.location.href = "analysis-result.html";
        } catch (error) {
            console.error("PHISHGUARD DEBUG: Screenshot analysis error:", error);

            if (screenshotResults) {
                screenshotResults.innerHTML = "<p class='help-text'>Something went wrong during screenshot analysis.</p>";
            }
        }
    });
}

function setupBulkAnalyzeForm() {
    const bulkButton = document.getElementById("runBulkScanBtn");

    console.log("PHISHGUARD DEBUG: setupBulkAnalyzeForm called. Button:", bulkButton);

    if (!bulkButton) return;

    bulkButton.addEventListener("click", async function () {
        console.log("PHISHGUARD DEBUG: Bulk scan button clicked");

        const bulkFilesInput = document.getElementById("bulkFiles");
        const bulkResults = document.getElementById("bulkResults");

        if (!bulkFilesInput || bulkFilesInput.files.length === 0) {
            alert("Please upload at least one file for bulk analysis.");
            return;
        }

        const formData = new FormData();

        const senderInput = document.getElementById("bulkSenderEmail");
        const linkInput = document.getElementById("bulkLink");

        formData.append("SenderEmail", senderInput ? senderInput.value : "");
        formData.append("Link", linkInput ? linkInput.value : "");

        for (let i = 0; i < bulkFilesInput.files.length; i++) {
            formData.append("Files", bulkFilesInput.files[i]);
        }

        if (bulkResults) {
            bulkResults.innerHTML = "<p class='help-text'>Analyzing uploaded files...</p>";
        }

        try {
            console.log("PHISHGUARD DEBUG: Sending request to /api/emailanalysis/bulk-analyze");

            const response = await fetch("/api/emailanalysis/bulk-analyze", {
                method: "POST",
                body: formData
            });

            console.log("PHISHGUARD DEBUG: Bulk response status:", response.status);

            if (!response.ok) {
                const errorText = await response.text();
                console.error("PHISHGUARD DEBUG: Bulk analysis failed:", errorText);

                if (bulkResults) {
                    bulkResults.innerHTML = "<p class='help-text'>Bulk analysis failed. Check console for details.</p>";
                }

                return;
            }

            const results = await response.json();

            console.log("PHISHGUARD DEBUG: Bulk results:", results);

            renderBulkResults(results);
        } catch (error) {
            console.error("PHISHGUARD DEBUG: Bulk analysis error:", error);

            if (bulkResults) {
                bulkResults.innerHTML = "<p class='help-text'>Something went wrong during bulk analysis.</p>";
            }
        }
    });
}

function renderBulkResults(results) {
    const container = document.getElementById("bulkResults");

    if (!container) return;

    if (!results || results.length === 0) {
        container.innerHTML = "<p class='help-text'>No files were analyzed.</p>";
        return;
    }

    let html = `
        <div class="table-wrap">
            <table class="data-table">
                <thead>
                    <tr>
                        <th>File</th>
                        <th>Risk Score</th>
                        <th>Risk Level</th>
                        <th>Alert Sent</th>
                        <th>S3 Alert Report</th>
                    </tr>
                </thead>
                <tbody>
    `;

    results.forEach(item => {
        html += `
            <tr>
                <td>${escapeHtml(item.originalFileName || "-")}</td>
                <td>${item.riskScore || 0}/100</td>
                <td><span class="badge ${getBadgeClass(item.riskLevel)}">${escapeHtml(item.riskLevel || "-")}</span></td>
                <td>${item.alertSent ? "Yes" : "No"}</td>
                <td>${escapeHtml(item.alertS3ObjectKey || "-")}</td>
            </tr>
        `;
    });

    html += `
                </tbody>
            </table>
        </div>
    `;

    container.innerHTML = html;
}
function loadAnalysisResultPage() {
    if (document.body.dataset.page !== "result") return;

    const resultData = localStorage.getItem("latestAnalysisResult");
    const emailData = localStorage.getItem("latestEmailData");

    if (!resultData) return;

    const result = JSON.parse(resultData);
    const email = emailData ? JSON.parse(emailData) : {};

    console.log("Latest analysis result:", result);

    setText("riskLevel", result.riskLevel || "-");
    setText("riskScore", (result.riskScore || 0) + "/100");
    setText("summary", result.summary || "-");

    setText(
        "alertStatus",
        result.alertSent
            ? "Sent Successfully to AWS Lambda and S3"
            : (result.alertMessage || "No alert was triggered.")
    );

    setText(
        "alertS3ObjectKey",
        result.alertS3ObjectKey || "No S3 alert report generated."
    );

    setText("resultSender", email.senderEmail || "-");
    setText("resultSubject", email.subject || "-");
    setText("resultAttachment", email.attachmentName || "No attachment");
    setText("resultLinks", email.link || "No link provided");
    setText("resultBody", email.emailBody || "-");

    const badge = document.getElementById("resultBadge");

    if (badge) {
        badge.textContent = result.riskLevel || "-";
        badge.className = "badge " + getBadgeClass(result.riskLevel);
    }

    const riskFill = document.getElementById("riskFill");

    if (riskFill) {
        riskFill.style.width = (result.riskScore || 0) + "%";
    }

    renderList("detectedIssues", result.detectedIssues);
    renderList("recommendations", result.recommendations);
}

async function loadHistoryPage() {
    if (document.body.dataset.page !== "history") {
        return;
    }

    const tableBody = document.getElementById("historyTableBody");

    if (!tableBody) {
        console.log("historyTableBody not found");
        return;
    }

    try {
        const response = await fetch("/api/emailanalysis/history");

        if (!response.ok) {
            throw new Error("Failed to load history. Status: " + response.status);
        }

        const history = await response.json();

        console.log("History loaded:", history);

        tableBody.innerHTML = "";

        if (!Array.isArray(history) || history.length === 0) {
            tableBody.innerHTML = `
                <tr>
                    <td colspan="9">No scan history found.</td>
                </tr>
            `;
            return;
        }

        history.forEach(function (item) {
            const row = document.createElement("tr");

            const uploadedKey = item.uploadedFileS3Key || item.attachmentName || "-";
            const alertText = item.alertSent ? "Sent" : "Not Sent";

            row.innerHTML = `
                <td>${item.id}</td>

                <td>${escapeHtml(item.scanType || "Manual")}</td>

                <td>${escapeHtml(item.senderEmail || "-")}</td>

                <td>${escapeHtml(shortText(item.subject || "-", 45))}</td>

                <td>
                    <span class="badge ${getBadgeClass(item.riskLevel)}">
                        ${escapeHtml(item.riskLevel || "-")}
                    </span>
                </td>

                <td>${item.riskScore ?? 0}/100</td>

                <td title="${escapeHtml(uploadedKey)}">
                    ${escapeHtml(shortText(uploadedKey, 35))}
                </td>

                <td>
                    <span class="badge ${item.alertSent ? "phishing" : "safe"}">
                        ${alertText}
                    </span>
                </td>

                <td>
                    <a class="btn small" href="scan-details.html?id=${item.id}">View</a>
                </td>
            `;

            tableBody.appendChild(row);
        });
    } catch (error) {
        console.error(error);

        tableBody.innerHTML = `
            <tr>
                <td colspan="9">Error loading scan history. Please check browser console.</td>
            </tr>
        `;
    }
}

async function loadScanDetailsPage() {
    if (document.body.dataset.page !== "details") return;

    const params = new URLSearchParams(window.location.search);
    const scanId = params.get("id");

    if (!scanId) return;

    try {
        const response = await fetch(`/api/emailanalysis/history/${scanId}`);

        if (!response.ok) {
            throw new Error("Failed to load scan details.");
        }

        const details = await response.json();

        console.log("Scan details loaded:", details);

        const uploadedKey = details.uploadedFileS3Key || details.attachmentName || "-";

        setText("detailId", details.id);
        setText("detailScanType", details.scanType || "Manual");
        setText("detailSender", details.senderEmail || "-");
        setText("detailSubject", details.subject || "-");
        setText("detailBody", details.emailBody || "-");
        setText("detailLink", details.link || "No link provided");
        setText("detailAttachment", details.attachmentName || "No attachment");
        setText("detailUploadedFileS3Key", uploadedKey);
        setText("detailRiskLevel", details.riskLevel || "-");
        setText("detailRiskScore", (details.riskScore || 0) + "/100");
        setText("detailSummary", details.summary || "-");

        setText(
            "detailAlertSent",
            details.alertSent ? "Yes, alert was sent to Lambda and S3." : "No alert was triggered."
        );

        setText("detailAlertMessage", details.alertMessage || "-");
        setText("detailAlertS3ObjectKey", details.alertS3ObjectKey || "-");

        const badge = document.getElementById("detailRiskLevel");

        if (badge) {
            badge.textContent = details.riskLevel || "-";
            badge.className = "badge " + getBadgeClass(details.riskLevel);
        }

        const detectedIssues = details.detectedIssues || parseJsonArray(details.detectedIssuesJson);
        const recommendations = details.recommendations || parseJsonArray(details.recommendationsJson);

        renderList("detailIssues", detectedIssues);
        renderList("detailRecommendations", recommendations);
    } catch (error) {
        alert("Error: " + error.message);
    }
}

async function loadStats() {
    const statTotalElement = document.getElementById("statTotal");
    const statSafeElement = document.getElementById("statSafe");
    const statSuspiciousElement = document.getElementById("statSuspicious");
    const statPhishingElement = document.getElementById("statPhishing");

    const totalScansElement = document.getElementById("totalScans");
    const safeScansElement = document.getElementById("safeScans");
    const suspiciousScansElement = document.getElementById("suspiciousScans");
    const phishingScansElement = document.getElementById("phishingScans");

    const totalThreatsElement = document.getElementById("totalThreats");
    const safeEmailsElement = document.getElementById("safeEmails");
    const suspiciousEmailsElement = document.getElementById("suspiciousEmails");
    const phishingEmailsElement = document.getElementById("phishingEmails");

    if (
        !statTotalElement &&
        !statSafeElement &&
        !statSuspiciousElement &&
        !statPhishingElement &&
        !totalScansElement &&
        !safeScansElement &&
        !suspiciousScansElement &&
        !phishingScansElement &&
        !totalThreatsElement
    ) {
        return;
    }

    try {
        console.log("PHISHGUARD DEBUG: Loading dashboard stats...");

        const response = await fetch("/api/emailanalysis/stats");

        if (!response.ok) {
            throw new Error("Failed to load stats. Status: " + response.status);
        }

        const stats = await response.json();

        console.log("PHISHGUARD DEBUG: Dashboard stats loaded:", stats);

        const totalScans = stats.totalScans ?? stats.TotalScans ?? stats.total ?? stats.Total ?? 0;
        const safeScans = stats.safeScans ?? stats.SafeScans ?? stats.safe ?? stats.Safe ?? 0;
        const suspiciousScans = stats.suspiciousScans ?? stats.SuspiciousScans ?? stats.suspicious ?? stats.Suspicious ?? 0;
        const phishingScans = stats.phishingScans ?? stats.PhishingScans ?? stats.phishing ?? stats.Phishing ?? 0;

        if (statTotalElement) statTotalElement.textContent = totalScans;
        if (statSafeElement) statSafeElement.textContent = safeScans;
        if (statSuspiciousElement) statSuspiciousElement.textContent = suspiciousScans;
        if (statPhishingElement) statPhishingElement.textContent = phishingScans;

        if (totalScansElement) totalScansElement.textContent = totalScans;
        if (safeScansElement) safeScansElement.textContent = safeScans;
        if (suspiciousScansElement) suspiciousScansElement.textContent = suspiciousScans;
        if (phishingScansElement) phishingScansElement.textContent = phishingScans;

        if (totalThreatsElement) totalThreatsElement.textContent = phishingScans + suspiciousScans;
        if (safeEmailsElement) safeEmailsElement.textContent = safeScans;
        if (suspiciousEmailsElement) suspiciousEmailsElement.textContent = suspiciousScans;
        if (phishingEmailsElement) phishingEmailsElement.textContent = phishingScans;

    } catch (error) {
        console.error("PHISHGUARD DEBUG: Failed to load dashboard stats:", error);
    }
}

async function loadReportsPage() {
    if (document.body.dataset.page !== "reports") return;

    try {
        console.log("PHISHGUARD DEBUG: Loading reports page stats...");

        const response = await fetch("/api/emailanalysis/stats");

        if (!response.ok) {
            throw new Error("Failed to load report stats.");
        }

        const stats = await response.json();

        console.log("PHISHGUARD DEBUG: Reports stats loaded:", stats);

        const totalScans = stats.totalScans ?? stats.TotalScans ?? 0;
        const safeScans = stats.safeScans ?? stats.SafeScans ?? 0;
        const suspiciousScans = stats.suspiciousScans ?? stats.SuspiciousScans ?? 0;
        const phishingScans = stats.phishingScans ?? stats.PhishingScans ?? 0;

        const manualScans = stats.manualScans ?? stats.ManualScans ?? 0;
        const manualUploadScans = stats.manualUploadScans ?? stats.ManualUploadScans ?? 0;
        const screenshotScans = stats.screenshotScans ?? stats.ScreenshotScans ?? 0;
        const bulkScans = stats.bulkScans ?? stats.BulkScans ?? 0;

        const alertSentCount = stats.alertSentCount ?? stats.AlertSentCount ?? 0;
        const uploadedFileCount = stats.uploadedFileCount ?? stats.UploadedFileCount ?? 0;

        setText("reportTotalScans", totalScans);
        setText("reportSafeScans", safeScans);
        setText("reportSuspiciousScans", suspiciousScans);
        setText("reportPhishingScans", phishingScans);

        setText("reportManualScans", manualScans);
        setText("reportManualUploadScans", manualUploadScans);
        setText("reportScreenshotScans", screenshotScans);
        setText("reportBulkScans", bulkScans);

        setText("reportUploadedFileCount", uploadedFileCount);
        setText("reportAlertSentCount", alertSentCount);

        const safePercent = totalScans > 0 ? Math.round((safeScans / totalScans) * 100) : 0;
        const suspiciousPercent = totalScans > 0 ? Math.round((suspiciousScans / totalScans) * 100) : 0;
        const phishingPercent = totalScans > 0 ? Math.round((phishingScans / totalScans) * 100) : 0;

        setText("safePercent", safePercent + "%");
        setText("suspiciousPercent", suspiciousPercent + "%");
        setText("phishingPercent", phishingPercent + "%");

        const safeBar = document.getElementById("safeBar");
        const suspiciousBar = document.getElementById("suspiciousBar");
        const phishingBar = document.getElementById("phishingBar");

        if (safeBar) safeBar.style.width = safePercent + "%";
        if (suspiciousBar) suspiciousBar.style.width = suspiciousPercent + "%";
        if (phishingBar) phishingBar.style.width = phishingPercent + "%";

        const summaryText =
            `The system has completed ${totalScans} scan(s). ` +
            `${safeScans} were classified as safe, ${suspiciousScans} as suspicious, and ${phishingScans} as phishing. ` +
            `${uploadedFileCount} uploaded file record(s) were stored using S3 object keys, and ${alertSentCount} serverless phishing alert(s) were generated through API Gateway, Lambda, and S3.`;

        setText("reportSummaryText", summaryText);

    } catch (error) {
        console.error("PHISHGUARD DEBUG: Failed to load reports page:", error);
        setText("reportSummaryText", "Failed to load report statistics. Please check the browser console.");
    }
}

function setText(id, value) {
  const element = document.getElementById(id);
  if (element) {
    element.textContent = value;
  }
}

function renderList(id, items) {
  const list = document.getElementById(id);
  if (!list) return;

  list.innerHTML = "";

  if (!items || items.length === 0) {
    const li = document.createElement("li");
    li.textContent = "No items available.";
    list.appendChild(li);
    return;
  }

  items.forEach(function (item) {
    const li = document.createElement("li");
    li.textContent = item;
    list.appendChild(li);
  });
}

function getBadgeClass(riskLevel) {
  if (riskLevel === "Phishing") return "danger";
  if (riskLevel === "Suspicious") return "suspicious";
  return "safe";
}

function escapeHtml(value) {
  if (!value) return "";

  return value
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function shortText(value, maxLength) {
    const text = String(value || "");

    if (text.length <= maxLength) {
        return text;
    }

    return text.substring(0, maxLength) + "...";
}

function parseJsonArray(value) {
    if (!value) return [];

    try {
        const parsed = JSON.parse(value);

        if (Array.isArray(parsed)) {
            return parsed;
        }

        return [];
    } catch {
        return [];
    }
}