# PowerShell script to convert Markdown documentation to HTML and then to PDF
# This script creates an HTML version that can be easily converted to PDF

$ErrorActionPreference = "Stop"

Write-Host "Converting AI Threat Detector Documentation to PDF..." -ForegroundColor Green

# Define paths
$projectRoot = Get-Location
$markdownFile = Join-Path $projectRoot "COMPLETE_PROJECT_DOCUMENTATION.md"
$htmlFile = Join-Path $projectRoot "COMPLETE_PROJECT_DOCUMENTATION.html"
$pdfFile = Join-Path $projectRoot "AI-Threat-Detector-Documentation.pdf"

# Check if markdown file exists
if (-not (Test-Path $markdownFile)) {
    Write-Error "Markdown file not found: $markdownFile"
    exit 1
}

Write-Host "Reading markdown content..." -ForegroundColor Yellow

# Read the markdown content
$markdownContent = Get-Content $markdownFile -Raw

# Create HTML template with CSS styling for PDF
$htmlTemplate = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>AI Threat Detector - Complete Documentation</title>
    <style>
        @page {
            margin: 1in;
            size: A4;
        }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 100%;
            margin: 0;
            padding: 20px;
        }
        
        h1 {
            color: #2c3e50;
            border-bottom: 3px solid #3498db;
            padding-bottom: 10px;
            page-break-before: always;
        }
        
        h1:first-of-type {
            page-break-before: auto;
        }
        
        h2 {
            color: #34495e;
            border-bottom: 2px solid #ecf0f1;
            padding-bottom: 5px;
            margin-top: 30px;
        }
        
        h3 {
            color: #2980b9;
            margin-top: 25px;
        }
        
        h4 {
            color: #27ae60;
            margin-top: 20px;
        }
        
        code {
            background-color: #f8f9fa;
            padding: 2px 4px;
            border-radius: 3px;
            font-family: 'Courier New', monospace;
            color: #e74c3c;
        }
        
        pre {
            background-color: #f8f9fa;
            border: 1px solid #e9ecef;
            border-radius: 5px;
            padding: 15px;
            overflow-x: auto;
            margin: 15px 0;
        }
        
        pre code {
            background: none;
            padding: 0;
            color: #333;
        }
        
        blockquote {
            border-left: 4px solid #3498db;
            margin: 0;
            padding-left: 20px;
            color: #7f8c8d;
            font-style: italic;
        }
        
        table {
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }
        
        th, td {
            border: 1px solid #ddd;
            padding: 12px;
            text-align: left;
        }
        
        th {
            background-color: #f2f2f2;
            font-weight: bold;
        }
        
        .toc {
            background-color: #f8f9fa;
            border: 1px solid #e9ecef;
            border-radius: 5px;
            padding: 20px;
            margin: 20px 0;
        }
        
        .toc h2 {
            margin-top: 0;
            color: #2c3e50;
        }
        
        .toc ul {
            list-style-type: decimal;
        }
        
        .toc li {
            margin: 5px 0;
        }
        
        .highlight {
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 5px;
            padding: 15px;
            margin: 15px 0;
        }
        
        .page-break {
            page-break-before: always;
        }
        
        a {
            color: #3498db;
            text-decoration: none;
        }
        
        a:hover {
            text-decoration: underline;
        }
        
        @media print {
            body {
                font-size: 12pt;
            }
            
            h1 {
                font-size: 18pt;
            }
            
            h2 {
                font-size: 16pt;
            }
            
            h3 {
                font-size: 14pt;
            }
            
            .no-print {
                display: none;
            }
            
            pre {
                white-space: pre-wrap;
                word-wrap: break-word;
            }
        }
    </style>
</head>
<body>
    <div class="header">
        <h1 style="text-align: center; border: none; color: #2c3e50; margin-bottom: 30px;">
            AI Threat Detector
            <br>
            <span style="font-size: 0.7em; color: #7f8c8d;">Complete Project Documentation</span>
        </h1>
        <div style="text-align: center; margin-bottom: 40px; color: #7f8c8d;">
            <p><strong>Version:</strong> 1.0 | <strong>Date:</strong> August 7, 2025</p>
            <p><strong>Document Length:</strong> ~60,000 words | <strong>Pages:</strong> ~200</p>
        </div>
    </div>
    
    <div class="content">
        {CONTENT}
    </div>
    
    <div class="footer" style="margin-top: 50px; text-align: center; color: #7f8c8d; border-top: 1px solid #ecf0f1; padding-top: 20px;">
        <p>© 2025 AI Threat Detector Project. This documentation is comprehensive and covers all aspects of the system.</p>
    </div>
</body>
</html>
"@

Write-Host "Converting markdown to HTML..." -ForegroundColor Yellow

# Simple markdown to HTML conversion (basic)
$htmlContent = $markdownContent

# Convert markdown syntax to HTML
$htmlContent = $htmlContent -replace '^# (.+)$', '<h1>$1</h1>' -replace '\r?\n', "`n"
$htmlContent = $htmlContent -replace '^## (.+)$', '<h2>$1</h2>' -replace '\r?\n', "`n"
$htmlContent = $htmlContent -replace '^### (.+)$', '<h3>$1</h3>' -replace '\r?\n', "`n"
$htmlContent = $htmlContent -replace '^#### (.+)$', '<h4>$1</h4>' -replace '\r?\n', "`n"

# Convert code blocks
$htmlContent = $htmlContent -replace '```(\w+)?\r?\n([\s\S]*?)\r?\n```', '<pre><code>$2</code></pre>'
$htmlContent = $htmlContent -replace '`([^`]+)`', '<code>$1</code>'

# Convert bold and italic
$htmlContent = $htmlContent -replace '\*\*([^*]+)\*\*', '<strong>$1</strong>'
$htmlContent = $htmlContent -replace '\*([^*]+)\*', '<em>$1</em>'

# Convert links
$htmlContent = $htmlContent -replace '\[([^\]]+)\]\(([^)]+)\)', '<a href="$2">$1</a>'

# Convert blockquotes
$htmlContent = $htmlContent -replace '^> (.+)$', '<blockquote>$1</blockquote>' -replace '\r?\n', "`n"

# Convert lists (basic)
$htmlContent = $htmlContent -replace '^- (.+)$', '<li>$1</li>' -replace '\r?\n', "`n"
$htmlContent = $htmlContent -replace '^(\d+)\. (.+)$', '<li>$2</li>' -replace '\r?\n', "`n"

# Convert line breaks
$htmlContent = $htmlContent -replace '\r?\n\r?\n', '</p><p>'
$htmlContent = $htmlContent -replace '\r?\n', '<br>'

# Wrap in paragraphs
$htmlContent = "<p>$htmlContent</p>"

# Replace content in template
$finalHtml = $htmlTemplate -replace '\{CONTENT\}', $htmlContent

Write-Host "Writing HTML file..." -ForegroundColor Yellow

# Write HTML file
$finalHtml | Out-File -FilePath $htmlFile -Encoding UTF8

Write-Host "HTML file created successfully: $htmlFile" -ForegroundColor Green

# Try to convert to PDF using different methods
Write-Host "Attempting PDF conversion..." -ForegroundColor Yellow

# Method 1: Try Chrome/Edge headless (if available)
$chromeFound = $false

# Check for Chrome
try {
    $chromePath = Get-Command chrome -ErrorAction SilentlyContinue
    if (-not $chromePath) {
        $chromePath = Get-ChildItem "C:\Program Files\Google\Chrome\Application\chrome.exe" -ErrorAction SilentlyContinue
    }
    if (-not $chromePath) {
        $chromePath = Get-ChildItem "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" -ErrorAction SilentlyContinue
    }
    if ($chromePath) {
        $chromeFound = $true
        Write-Host "Found Chrome, attempting PDF conversion..." -ForegroundColor Yellow
        & $chromePath.FullName --headless --disable-gpu --print-to-pdf="$pdfFile" --no-margins "file:///$($htmlFile -replace '\\', '/')"
        if (Test-Path $pdfFile) {
            Write-Host "PDF created successfully using Chrome: $pdfFile" -ForegroundColor Green
            return
        }
    }
} catch {
    Write-Host "Chrome conversion failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Check for Edge
if (-not $chromeFound) {
    try {
        $edgePath = Get-Command msedge -ErrorAction SilentlyContinue
        if (-not $edgePath) {
            $edgePath = Get-ChildItem "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" -ErrorAction SilentlyContinue
        }
        if ($edgePath) {
            Write-Host "Found Edge, attempting PDF conversion..." -ForegroundColor Yellow
            & $edgePath.FullName --headless --disable-gpu --print-to-pdf="$pdfFile" --no-margins "file:///$($htmlFile -replace '\\', '/')"
            if (Test-Path $pdfFile) {
                Write-Host "PDF created successfully using Edge: $pdfFile" -ForegroundColor Green
                return
            }
        }
    } catch {
        Write-Host "Edge conversion failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# If no browser-based conversion worked, provide instructions
if (-not (Test-Path $pdfFile)) {
    Write-Host ""
    Write-Host "=== PDF CONVERSION INSTRUCTIONS ===" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "The HTML file has been created successfully. To convert to PDF, you can:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Method 1 - Browser (Recommended):" -ForegroundColor White
    Write-Host "1. Open the HTML file in Chrome or Edge: $htmlFile" -ForegroundColor Gray
    Write-Host "2. Press Ctrl+P to print" -ForegroundColor Gray
    Write-Host "3. Select 'Save as PDF' destination" -ForegroundColor Gray
    Write-Host "4. Save as: AI-Threat-Detector-Documentation.pdf" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Method 2 - Online Converter:" -ForegroundColor White
    Write-Host "1. Use online HTML to PDF converter (e.g., html-pdf-converter.com)" -ForegroundColor Gray
    Write-Host "2. Upload the HTML file: $htmlFile" -ForegroundColor Gray
    Write-Host "3. Download the PDF and save to project root" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Method 3 - Install Pandoc:" -ForegroundColor White
    Write-Host "1. Install pandoc: winget install --id=JohnMacFarlane.Pandoc" -ForegroundColor Gray
    Write-Host "2. Run: pandoc COMPLETE_PROJECT_DOCUMENTATION.md -o AI-Threat-Detector-Documentation.pdf" -ForegroundColor Gray
    Write-Host ""
    Write-Host "HTML file location: $htmlFile" -ForegroundColor Green
    Write-Host "Target PDF location: $pdfFile" -ForegroundColor Green
}

Write-Host ""
Write-Host "Documentation conversion completed!" -ForegroundColor Green
