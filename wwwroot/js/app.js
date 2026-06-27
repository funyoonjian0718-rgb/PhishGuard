document.addEventListener("DOMContentLoaded", function () {
  setupMobileMenu();
  setupAnalyzeForm();
  loadAnalysisResultPage();
  loadHistoryPage();
  loadScanDetailsPage();
  loadStats();
  loadReportsPage();
});

document.addEventListener("DOMContentLoaded", function () {
  setupRegisterForm();
  setupLoginForm();
  setupLogoutButtons();
  protectPrivatePages();
  showCurrentUser();
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
    return;
  }

  const user = localStorage.getItem("loggedInUser");

  if (!user) {
    window.location.href = "login.html";
  }
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

function setupAnalyzeForm() {
  const analyzeForm = document.getElementById("analyzeForm");

  if (!analyzeForm) return;

  analyzeForm.addEventListener("submit", async function (event) {
    event.preventDefault();

    const emailData = {
      senderEmail: document.getElementById("senderEmail")?.value || "",
      subject: document.getElementById("subject")?.value || "",
      emailBody: document.getElementById("emailBody")?.value || "",
      link: document.getElementById("link")?.value || "",
      attachmentName: document.getElementById("attachmentName")?.value || ""
    };

    try {
      const response = await fetch("/api/emailanalysis/analyze", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(emailData)
      });

      if (!response.ok) {
        throw new Error("Failed to analyze email.");
      }

      const result = await response.json();

      localStorage.setItem("latestAnalysisResult", JSON.stringify(result));
      localStorage.setItem("latestEmailData", JSON.stringify(emailData));

      window.location.href = "analysis-result.html";
    } catch (error) {
      alert("Error: " + error.message);
    }
  });
}

function loadAnalysisResultPage() {
  if (document.body.dataset.page !== "result") return;

  const resultData = localStorage.getItem("latestAnalysisResult");
  const emailData = localStorage.getItem("latestEmailData");

  if (!resultData) return;

  const result = JSON.parse(resultData);
  const email = emailData ? JSON.parse(emailData) : {};

  setText("riskLevel", result.riskLevel || "-");
  setText("riskScore", (result.riskScore || 0) + "/100");
  setText("summary", result.summary || "-");

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
  if (document.body.dataset.page !== "history") return;

  const tableBody = document.getElementById("historyTableBody");
  if (!tableBody) return;

  try {
    const response = await fetch("/api/emailanalysis/history");
    if (!response.ok) throw new Error("Failed to load history.");

    const history = await response.json();
    tableBody.innerHTML = "";

    if (history.length === 0) {
      tableBody.innerHTML = `
        <tr>
          <td colspan="6">No scan history found.</td>
        </tr>
      `;
      return;
    }

    history.forEach(function (item) {
      const row = document.createElement("tr");

      row.innerHTML = `
        <td>${item.id}</td>
        <td>${escapeHtml(item.senderEmail)}</td>
        <td>${escapeHtml(item.subject)}</td>
        <td><span class="badge ${getBadgeClass(item.riskLevel)}">${item.riskLevel}</span></td>
        <td>${item.riskScore}/100</td>
        <td><a class="btn small" href="scan-details.html?id=${item.id}">View</a></td>
      `;

      tableBody.appendChild(row);
    });
  } catch (error) {
    tableBody.innerHTML = `
      <tr>
        <td colspan="6">Error loading scan history.</td>
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
    if (!response.ok) throw new Error("Failed to load scan details.");

    const details = await response.json();

    setText("detailId", details.id);
    setText("detailSender", details.senderEmail || "-");
    setText("detailSubject", details.subject || "-");
    setText("detailBody", details.emailBody || "-");
    setText("detailLink", details.link || "No link provided");
    setText("detailAttachment", details.attachmentName || "No attachment");
    setText("detailRiskLevel", details.riskLevel || "-");
    setText("detailRiskScore", (details.riskScore || 0) + "/100");
    setText("detailSummary", details.summary || "-");

    renderList("detailIssues", details.detectedIssues);
    renderList("detailRecommendations", details.recommendations);
  } catch (error) {
    alert("Error: " + error.message);
  }
}

async function loadStats() {
  try {
    const response = await fetch("/api/emailanalysis/stats");
    if (!response.ok) throw new Error("Failed to load stats.");

    const stats = await response.json();

    setText("statTotal", stats.total ?? 0);
    setText("statSafe", stats.safe ?? 0);
    setText("statSuspicious", stats.suspicious ?? 0);
    setText("statPhishing", stats.phishing ?? 0);
  } catch (error) {
    console.log(error.message);
  }
}

async function loadReportsPage() {
  if (document.body.dataset.page !== "reports") return;

  try {
    const response = await fetch("/api/emailanalysis/stats");
    if (!response.ok) throw new Error("Failed to load report stats.");

    const stats = await response.json();

    const total = stats.total ?? 0;
    const safe = stats.safe ?? 0;
    const suspicious = stats.suspicious ?? 0;
    const phishing = stats.phishing ?? 0;

    const safePercent = total > 0 ? Math.round((safe / total) * 100) : 0;
    const suspiciousPercent = total > 0 ? Math.round((suspicious / total) * 100) : 0;
    const phishingPercent = total > 0 ? Math.round((phishing / total) * 100) : 0;

    setText("safePercent", safePercent + "%");
    setText("suspiciousPercent", suspiciousPercent + "%");
    setText("phishingPercent", phishingPercent + "%");

    const safeBar = document.getElementById("safeBar");
    const suspiciousBar = document.getElementById("suspiciousBar");
    const phishingBar = document.getElementById("phishingBar");

    if (safeBar) safeBar.style.width = safePercent + "%";
    if (suspiciousBar) suspiciousBar.style.width = suspiciousPercent + "%";
    if (phishingBar) phishingBar.style.width = phishingPercent + "%";
  } catch (error) {
    console.log(error.message);
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