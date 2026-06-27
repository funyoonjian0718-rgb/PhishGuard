const STORAGE_KEYS = {
  users: 'phishguard_users',
  currentUser: 'phishguard_current_user',
  scans: 'phishguard_scans',
  lastScan: 'phishguard_last_scan_id'
};

function getJSON(key, fallback) {
  try {
    return JSON.parse(localStorage.getItem(key)) ?? fallback;
  } catch {
    return fallback;
  }
}

function setJSON(key, value) {
  localStorage.setItem(key, JSON.stringify(value));
}

function seedDemoData() {
  const users = getJSON(STORAGE_KEYS.users, []);
  if (!users.some(u => u.email === 'demo@phishguard.com')) {
    users.push({ name: 'Demo User', email: 'demo@phishguard.com', password: 'password123' });
    setJSON(STORAGE_KEYS.users, users);
  }

  const scans = getJSON(STORAGE_KEYS.scans, []);
  if (scans.length === 0) {
    const demoScans = [
      makeScan({
        sender: 'security@bank-alert-login.com',
        subject: 'Urgent: Your account will be suspended',
        body: 'Dear customer, your account will be suspended today. Verify your password immediately at http://bit.ly/bank-login',
        links: 'http://bit.ly/bank-login',
        attachment: ''
      }, false),
      makeScan({
        sender: 'lecturer@apu.edu.my',
        subject: 'Assignment reminder',
        body: 'Please remember to submit your assignment before the deadline through the official portal.',
        links: 'https://lms.apu.edu.my',
        attachment: 'brief.pdf'
      }, false),
      makeScan({
        sender: 'hr.payroll@gmail.com',
        subject: 'Payroll update required',
        body: 'Click the link and provide your bank details to avoid salary delay.',
        links: 'http://payroll-update-free.com',
        attachment: 'salary_update.zip'
      }, false)
    ];
    setJSON(STORAGE_KEYS.scans, demoScans);
  }
}

function makeScan(input, save = true) {
  const result = analyseEmail(input);
  const scan = {
    id: crypto.randomUUID ? crypto.randomUUID() : String(Date.now()),
    sender: input.sender || '-',
    subject: input.subject || '-',
    body: input.body || '',
    links: input.links || '',
    attachment: input.attachment || '',
    score: result.score,
    label: result.label,
    rules: result.rules,
    createdAt: new Date().toISOString()
  };

  if (save) {
    const scans = getJSON(STORAGE_KEYS.scans, []);
    scans.unshift(scan);
    setJSON(STORAGE_KEYS.scans, scans);
    localStorage.setItem(STORAGE_KEYS.lastScan, scan.id);
  }

  return scan;
}

function analyseEmail(input) {
  let score = 0;
  const rules = [];
  const sender = (input.sender || '').toLowerCase();
  const subject = (input.subject || '').toLowerCase();
  const body = (input.body || '').toLowerCase();
  const links = (input.links || '').toLowerCase();
  const attachment = (input.attachment || '').toLowerCase();
  const allText = `${subject} ${body} ${links}`;

  const urgentWords = ['urgent', 'immediately', 'verify now', 'suspended', 'limited time', 'final warning', 'expire', 'blocked'];
  if (urgentWords.some(word => allText.includes(word))) {
    score += 20;
    rules.push('Urgent or threatening language was detected.');
  }

  const sensitiveWords = ['password', 'bank details', 'credit card', 'otp', 'pin number', 'login', 'verify your account', 'payment'];
  if (sensitiveWords.some(word => allText.includes(word))) {
    score += 25;
    rules.push('The email asks for sensitive information such as login, password, payment, or bank details.');
  }

  const shorteners = ['bit.ly', 'tinyurl', 't.co', 'shorturl', 'ow.ly'];
  if (shorteners.some(domain => links.includes(domain) || body.includes(domain))) {
    score += 20;
    rules.push('A shortened URL was found. Short links can hide the real destination.');
  }

  if (links.includes('http://')) {
    score += 15;
    rules.push('The email contains a non-secure HTTP link.');
  }

  const riskyDomains = ['free', 'login', 'verify', 'secure-update', 'account-alert', 'bank-alert'];
  if (riskyDomains.some(word => sender.includes(word) || links.includes(word))) {
    score += 15;
    rules.push('Sender or link domain contains suspicious words commonly used in phishing.');
  }

  if (sender.includes('@gmail.com') || sender.includes('@yahoo.com') || sender.includes('@outlook.com')) {
    const companyWords = ['bank', 'payroll', 'security', 'admin', 'support', 'finance'];
    if (companyWords.some(word => subject.includes(word) || body.includes(word) || sender.includes(word))) {
      score += 15;
      rules.push('A free email provider is being used for a message that appears to be official/company-related.');
    }
  }

  const riskyAttachments = ['.exe', '.zip', '.rar', '.scr', '.bat', '.js', '.vbs'];
  if (riskyAttachments.some(ext => attachment.endsWith(ext))) {
    score += 20;
    rules.push('The attachment type is risky and should not be opened without verification.');
  }

  const urlCount = (body.match(/https?:\/\//g) || []).length + (links.match(/https?:\/\//g) || []).length;
  if (urlCount >= 3) {
    score += 10;
    rules.push('Multiple links were detected in the email.');
  }

  score = Math.min(score, 100);

  let label = 'Safe';
  if (score >= 65) label = 'Phishing';
  else if (score >= 35) label = 'Suspicious';

  if (rules.length === 0) {
    rules.push('No high-risk phishing pattern was detected by the current rule set.');
  }

  return { score, label, rules };
}

function badgeClass(label) {
  const value = (label || '').toLowerCase();
  if (value === 'safe') return 'safe';
  if (value === 'suspicious') return 'suspicious';
  return 'phishing';
}

function formatDate(dateString) {
  return new Date(dateString).toLocaleString([], {
    year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit'
  });
}

function setupNav() {
  const current = document.body.dataset.page;
  document.querySelectorAll('.nav a').forEach(link => {
    if (link.dataset.page === current) link.classList.add('active');
  });

  const menuBtn = document.querySelector('[data-mobile-menu]');
  const sidebar = document.querySelector('.sidebar');
  if (menuBtn && sidebar) {
    menuBtn.addEventListener('click', () => sidebar.classList.toggle('open'));
  }
}

function setupAuthForms() {
  const loginForm = document.querySelector('#loginForm');
  if (loginForm) {
    loginForm.addEventListener('submit', event => {
      event.preventDefault();
      const email = document.querySelector('#email').value.trim().toLowerCase();
      const password = document.querySelector('#password').value;
      const users = getJSON(STORAGE_KEYS.users, []);
      const user = users.find(u => u.email === email && u.password === password);
      if (!user) {
        showAlert('Invalid login for this local demo. Try demo@phishguard.com / password123.', 'loginAlert');
        return;
      }
      setJSON(STORAGE_KEYS.currentUser, user);
      window.location.href = 'index.html';
    });
  }

  const registerForm = document.querySelector('#registerForm');
  if (registerForm) {
    registerForm.addEventListener('submit', event => {
      event.preventDefault();
      const name = document.querySelector('#name').value.trim();
      const email = document.querySelector('#emailR').value.trim().toLowerCase();
      const password = document.querySelector('#passwordR').value;
      const users = getJSON(STORAGE_KEYS.users, []);

      if (users.some(u => u.email === email)) {
        showAlert('This email already exists in the local demo storage.', 'registerAlert');
        return;
      }

      users.push({ name, email, password });
      setJSON(STORAGE_KEYS.users, users);
      setJSON(STORAGE_KEYS.currentUser, { name, email });
      window.location.href = 'index.html';
    });
  }
}

function showAlert(message, elementId) {
  const alert = document.querySelector(`#${elementId}`);
  if (alert) alert.textContent = message;
}

function setupAnalyzeForm() {
  const form = document.querySelector('#analyzeForm');
  if (!form) return;

  form.addEventListener('submit', event => {
    event.preventDefault();
    const input = {
      sender: document.querySelector('#sender').value.trim(),
      subject: document.querySelector('#subject').value.trim(),
      body: document.querySelector('#body').value.trim(),
      links: document.querySelector('#links').value.trim(),
      attachment: document.querySelector('#attachment').value.trim()
    };
    const scan = makeScan(input, true);
    window.location.href = `analysis-result.html?id=${encodeURIComponent(scan.id)}`;
  });

  const sampleBtn = document.querySelector('#sampleEmailBtn');
  if (sampleBtn) {
    sampleBtn.addEventListener('click', () => {
      document.querySelector('#sender').value = 'security@bank-alert-login.com';
      document.querySelector('#subject').value = 'Urgent: Your account will be suspended';
      document.querySelector('#body').value = 'Dear customer, your account will be suspended today. Verify your password immediately at http://bit.ly/bank-login to avoid account closure.';
      document.querySelector('#links').value = 'http://bit.ly/bank-login';
      document.querySelector('#attachment').value = '';
    });
  }
}

function renderDashboard() {
  if (document.body.dataset.page !== 'dashboard') return;
  const scans = getJSON(STORAGE_KEYS.scans, []);
  const total = scans.length;
  const phishing = scans.filter(s => s.label === 'Phishing').length;
  const suspicious = scans.filter(s => s.label === 'Suspicious').length;
  const safe = scans.filter(s => s.label === 'Safe').length;
  const avg = total ? Math.round(scans.reduce((sum, s) => sum + s.score, 0) / total) : 0;

  setText('totalScans', total);
  setText('safeScans', safe);
  setText('suspiciousScans', suspicious);
  setText('phishingScans', phishing);
  setText('avgScore', `${avg}/100`);

  const tbody = document.querySelector('#recentScans');
  if (tbody) {
    tbody.innerHTML = scans.slice(0, 5).map(scan => `
      <tr>
        <td>${formatDate(scan.createdAt)}</td>
        <td>${escapeHtml(scan.sender)}</td>
        <td>${escapeHtml(scan.subject)}</td>
        <td><span class="badge ${badgeClass(scan.label)}">${scan.label}</span></td>
      </tr>
    `).join('') || `<tr><td colspan="4" class="help-text">No analysis history yet.</td></tr>`;
  }
}

function renderHistory() {
  if (document.body.dataset.page !== 'history') return;
  const scans = getJSON(STORAGE_KEYS.scans, []);
  const tbody = document.querySelector('#historyTable');
  if (!tbody) return;

  tbody.innerHTML = scans.map(scan => `
    <tr>
      <td>${formatDate(scan.createdAt)}</td>
      <td>${escapeHtml(scan.sender)}</td>
      <td>${escapeHtml(scan.subject)}</td>
      <td><span class="badge ${badgeClass(scan.label)}">${scan.label}</span></td>
      <td>${scan.score}/100</td>
      <td><a class="btn" href="scan-details.html?id=${encodeURIComponent(scan.id)}">View</a></td>
    </tr>
  `).join('') || `<tr><td colspan="6" class="help-text">No analysis history yet. Go to Analyze Email first.</td></tr>`;
}

function renderResult() {
  if (document.body.dataset.page !== 'result' && document.body.dataset.page !== 'details') return;
  const params = new URLSearchParams(window.location.search);
  const id = params.get('id') || localStorage.getItem(STORAGE_KEYS.lastScan);
  const scans = getJSON(STORAGE_KEYS.scans, []);
  const scan = scans.find(s => s.id === id) || scans[0];
  if (!scan) return;

  setText('resultLabel', scan.label);
  setText('riskScore', `${scan.score}/100`);
  setText('resultSender', scan.sender);
  setText('resultSubject', scan.subject);
  setText('resultBody', scan.body || 'No body content provided.');
  setText('resultLinks', scan.links || 'No link provided.');
  setText('resultAttachment', scan.attachment || 'No attachment provided.');

  const badge = document.querySelector('#resultBadge');
  if (badge) {
    badge.textContent = scan.label;
    badge.className = `badge ${badgeClass(scan.label)}`;
  }

  const fill = document.querySelector('#riskFill');
  if (fill) fill.style.width = `${scan.score}%`;

  const rules = document.querySelector('#matchedRules');
  if (rules) {
    rules.innerHTML = scan.rules.map(rule => `<li>${escapeHtml(rule)}</li>`).join('');
  }
}

function renderReports() {
  if (document.body.dataset.page !== 'reports') return;
  const scans = getJSON(STORAGE_KEYS.scans, []);
  const total = scans.length || 1;
  const safe = Math.round(scans.filter(s => s.label === 'Safe').length / total * 100);
  const suspicious = Math.round(scans.filter(s => s.label === 'Suspicious').length / total * 100);
  const phishing = Math.round(scans.filter(s => s.label === 'Phishing').length / total * 100);
  setText('safePercent', `${safe}%`);
  setText('suspiciousPercent', `${suspicious}%`);
  setText('phishingPercent', `${phishing}%`);
  setWidth('safeBar', `${safe}%`);
  setWidth('suspiciousBar', `${suspicious}%`);
  setWidth('phishingBar', `${phishing}%`);
}

function renderProfile() {
  if (document.body.dataset.page !== 'profile') return;
  const user = getJSON(STORAGE_KEYS.currentUser, { name: 'Demo User', email: 'demo@phishguard.com' });
  setText('profileName', user.name || 'Demo User');
  setText('profileEmail', user.email || 'demo@phishguard.com');
}

function setupSettings() {
  const clearBtn = document.querySelector('#clearDemoData');
  if (clearBtn) {
    clearBtn.addEventListener('click', () => {
      localStorage.removeItem(STORAGE_KEYS.scans);
      localStorage.removeItem(STORAGE_KEYS.lastScan);
      seedDemoData();
      alert('Demo scan history has been reset.');
      window.location.href = 'index.html';
    });
  }
}

function setText(id, value) {
  const el = document.querySelector(`#${id}`);
  if (el) el.textContent = value;
}

function setWidth(id, value) {
  const el = document.querySelector(`#${id}`);
  if (el) el.style.width = value;
}

function escapeHtml(text) {
  return String(text)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');
}

async function analyzeEmailWithBackend(event) {
  event.preventDefault();

  const senderEmail = document.getElementById("senderEmail")?.value || "";
  const subject = document.getElementById("subject")?.value || "";
  const emailBody = document.getElementById("emailBody")?.value || "";
  const link = document.getElementById("link")?.value || "";
  const attachmentName = document.getElementById("attachmentName")?.value || "";

  const emailData = {
    senderEmail: senderEmail,
    subject: subject,
    emailBody: emailBody,
    link: link,
    attachmentName: attachmentName
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
}

document.addEventListener("DOMContentLoaded", function () {
  const analyzeForm = document.getElementById("analyzeForm");

  if (analyzeForm) {
    analyzeForm.addEventListener("submit", analyzeEmailWithBackend);
  }
});

function loadAnalysisResultPage() {
  const resultData = localStorage.getItem("latestAnalysisResult");

  if (!resultData) {
    return;
  }

  const result = JSON.parse(resultData);

  const riskScore = document.getElementById("riskScore");
  const riskLevel = document.getElementById("riskLevel");
  const summary = document.getElementById("summary");
  const detectedIssues = document.getElementById("detectedIssues");
  const recommendations = document.getElementById("recommendations");

  if (riskScore) {
    riskScore.textContent = result.riskScore + "/100";
  }

  if (riskLevel) {
    riskLevel.textContent = result.riskLevel;
  }

  if (summary) {
    summary.textContent = result.summary;
  }

  if (detectedIssues) {
    detectedIssues.innerHTML = "";

    result.detectedIssues.forEach(function (issue) {
      const li = document.createElement("li");
      li.textContent = issue;
      detectedIssues.appendChild(li);
    });
  }

  if (recommendations) {
    recommendations.innerHTML = "";

    result.recommendations.forEach(function (recommendation) {
      const li = document.createElement("li");
      li.textContent = recommendation;
      recommendations.appendChild(li);
    });
  }
}

document.addEventListener("DOMContentLoaded", function () {
  loadAnalysisResultPage();
});

seedDemoData();
setupNav();
setupAuthForms();
setupAnalyzeForm();
renderDashboard();
renderHistory();
renderResult();
renderReports();
renderProfile();
setupSettings();
