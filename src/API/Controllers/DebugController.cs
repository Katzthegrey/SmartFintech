using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog; // Add this for NLog
using SmartFintechFinancial.API.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartFintechFinancial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly ILogger<DebugController> _logger;
    private readonly IHostEnvironment _environment;
    private static readonly NLog.ILogger _nlogger = LogManager.GetCurrentClassLogger(); // Add NLog logger

    public DebugController(
        ILogger<DebugController> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    [HttpGet("test-logs")]
    public IActionResult TestLogs()
    {
        try
        {
            // Microsoft ILogger logs
            _logger.LogTrace("TRACE level message - most detailed");
            _logger.LogDebug("DEBUG level message - for debugging");
            _logger.LogInformation("INFORMATION level message - normal flow");
            _logger.LogWarning("WARNING level message - something unusual");
            _logger.LogError("ERROR level message - something failed");
            _logger.LogCritical("CRITICAL level message - application failure");

            // NLog direct logs
            _nlogger.Debug("NLog DEBUG: Test log from controller");
            _nlogger.Info("NLog INFO: Test log from controller");
            _nlogger.Warn("NLog WARN: Test log from controller");
            _nlogger.Error("NLog ERROR: Test log from controller");

            // Log with structured data
            _logger.LogInformation("Test log with parameters: {Timestamp}, {RandomValue}",
                DateTime.Now, Random.Shared.Next(1, 1000));

            // Log specifically from your validator
            _logger.LogInformation("=== TESTING VALIDATOR LOGGING ===");
            _logger.LogInformation("If you see this, logging is working");

            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");

            // Safe file enumeration
            var files = new List<object>();
            if (Directory.Exists(logDirectory))
            {
                try
                {
                    files = Directory.GetFiles(logDirectory, "*.txt")
                        .Concat(Directory.GetFiles(logDirectory, "*.json"))
                        .Select(f => {
                            try
                            {
                                var fileInfo = new FileInfo(f);
                                var lines = new List<string>();
                                try
                                {
                                    lines = System.IO.File.ReadAllLines(f)
                                        .TakeLast(10)
                                        .ToList();
                                }
                                catch { }

                                return new
                                {
                                    Name = Path.GetFileName(f),
                                    Size = fileInfo.Length,
                                    LastModified = fileInfo.LastWriteTime,
                                    Preview = lines,
                                    HasError = false
                                };
                            }
                            catch
                            {
                                return new
                                {
                                    Name = Path.GetFileName(f),
                                    Size = 0L,
                                    LastModified = DateTime.MinValue,
                                    Preview = new List<string>(),
                                    HasError = true
                                };
                            }
                        })
                        .Cast<object>()
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading log files");
                    files = new List<object>();
                }
            }

            return Ok(new
            {
                message = "Logs generated successfully",
                timestamp = DateTime.Now,
                logDirectory = logDirectory,
                directoryExists = Directory.Exists(logDirectory),
                currentDirectory = Directory.GetCurrentDirectory(),
                environment = _environment.EnvironmentName,
                isDevelopment = _environment.IsDevelopment(),
                logFiles = files,
                logEntries = new
                {
                    trace = "Check logs for TRACE message",
                    debug = "Check logs for DEBUG message",
                    info = "Check logs for INFORMATION message",
                    warning = "Check logs for WARNING message",
                    error = "Check logs for ERROR message",
                    critical = "Check logs for CRITICAL message"
                },
                validatorTest = new
                {
                    message = "Your validator should log: 'Validating Email: test@example.com' etc.",
                    status = "Check logs above to see if any validator messages appear"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TestLogs endpoint");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpGet("check-filesystem")]
    public IActionResult CheckFileSystem()
    {
        try
        {
            var results = new List<object>();
            var ipAddress = HttpContext.GetClientIpAddress();

            _logger.LogInformation("Filesystem check requested from IP: {IP}", ipAddress);

            // Safely get locations - focusing on WSL/Windows paths
            var locationsList = new List<string>
            {
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), "logs"),
                _environment.ContentRootPath,
                "/tmp", // Linux/WSL temp
                "/mnt/c/Windows/Temp", // Windows temp from WSL
                "/mnt/c/Users/Public" // Public folder
            };

            foreach (var location in locationsList.Distinct())
            {
                try
                {
                    var dirExists = Directory.Exists(location);
                    var files = new List<string>();

                    if (dirExists)
                    {
                        try
                        {
                            files = Directory.GetFiles(location)
                                .Take(5)
                                .Select(Path.GetFileName)
                                .Where(f => f != null)
                                .Select(f => f!)
                                .ToList();
                        }
                        catch
                        {
                            files = new List<string>();
                        }
                    }

                    // Test write permissions
                    bool canWrite = false;
                    string testFile = null;

                    if (dirExists)
                    {
                        try
                        {
                            testFile = Path.Combine(location, $"wsl-test-{Guid.NewGuid()}.txt");
                            System.IO.File.WriteAllText(testFile, "WSL permission test");
                            canWrite = System.IO.File.Exists(testFile);
                            if (canWrite && System.IO.File.Exists(testFile))
                            {
                                System.IO.File.Delete(testFile);
                            }
                        }
                        catch
                        {
                            canWrite = false;
                        }
                    }

                    results.Add(new
                    {
                        location,
                        exists = dirExists,
                        canWrite,
                        sampleFiles = files,
                        isWritable = canWrite,
                        isWindowsPath = location.StartsWith("/mnt/c/")
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new { location, error = ex.Message });
                }
            }

            return Ok(new
            {
                success = true,
                results,
                isWSL = Directory.Exists("/mnt/c/Windows"),
                currentOS = Environment.OSVersion.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckFileSystem endpoint");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpGet("create-log-directory")]
    public IActionResult CreateLogDirectory()
    {
        try
        {
            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            var messages = new List<string>();
            var ipAddress = HttpContext.GetClientIpAddress();

            _logger.LogInformation("Create log directory requested from IP: {IP}", ipAddress);

            messages.Add($"Current directory: {Directory.GetCurrentDirectory()}");
            messages.Add($"Log directory path: {logDir}");
            messages.Add($"Directory exists before: {Directory.Exists(logDir)}");

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
                messages.Add($"✅ Created directory: {logDir}");
            }
            else
            {
                messages.Add($"✅ Directory already exists: {logDir}");
            }

            // Test write permission
            var testFile = Path.Combine(logDir, $"test-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
            System.IO.File.WriteAllText(testFile, "Test write operation");
            messages.Add($"✅ Created test file: {testFile}");

            var fileContent = System.IO.File.ReadAllText(testFile);
            messages.Add($"✅ Read test file: {fileContent}");

            System.IO.File.Delete(testFile);
            messages.Add($"✅ Deleted test file");

            // Now try to write a log message through NLog
            _nlogger.Info("Log directory test successful from IP: {IP}", ipAddress);
            messages.Add($"✅ NLog log message sent");

            return Ok(new
            {
                success = true,
                logDirectory = logDir,
                directoryExists = Directory.Exists(logDir),
                messages = messages,
                note = "If you see this, file writing works! Now check if logs appear in the directory."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateLogDirectory endpoint");
            return StatusCode(500, new
            {
                success = false,
                error = "An unexpected error occurred",
                details = ex.Message
            });
        }
    }

    [HttpGet("serilog-status")]
    public IActionResult SerilogStatus()
    {
        try
        {
            var ipAddress = HttpContext.GetClientIpAddress();
            _logger.LogInformation("Serilog status check from IP: {IP}", ipAddress);

            // Now using NLog instead
            _nlogger.Info("NLog status check from IP: {IP}", ipAddress);

            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            var logFiles = new List<object>();

            if (Directory.Exists(logDir))
            {
                try
                {
                    logFiles = Directory.GetFiles(logDir, "*.txt")
                        .Concat(Directory.GetFiles(logDir, "*.json"))
                        .Select(f => new FileInfo(f))
                        .Select(f => {
                            var content = new List<string>();
                            if (f.Length < 10240)
                            {
                                try
                                {
                                    content = System.IO.File.ReadAllLines(f.FullName)
                                        .TakeLast(10)
                                        .ToList();
                                }
                                catch { }
                            }

                            return new
                            {
                                Name = f.Name,
                                Size = f.Length,
                                Created = f.CreationTime,
                                Modified = f.LastWriteTime,
                                Preview = content
                            };
                        })
                        .Cast<object>()
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading log files");
                }
            }

            return Ok(new
            {
                success = true,
                logDirectory = logDir,
                directoryExists = Directory.Exists(logDir),
                logFiles = logFiles,
                usingNLog = true,
                note = "Now using NLog instead of Serilog"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SerilogStatus endpoint");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpGet("test-validator")]
    public IActionResult TestValidator()
    {
        try
        {
            _logger.LogInformation("=== SIMULATING VALIDATOR LOGS ===");
            _logger.LogInformation("Validating Email: test@example.com");
            _logger.LogInformation("Password validation - Length: 14, Uppercase: True, Lowercase: True, Number: True, Special: True");
            _logger.LogInformation("RegistrationType: client");
            _logger.LogInformation("ConsentGiven: True");
            _logger.LogInformation("=== END VALIDATOR SIMULATION ===");

            // Also log through NLog to the validator logger
            var validatorLogger = LogManager.GetLogger("SmartFintechFinancial.Modules.Identity.Application.Validators.RegisterRequestValidator");
            validatorLogger.Debug("=== NLOG VALIDATOR TEST ===");
            validatorLogger.Debug("Validating Email: test@example.com");
            validatorLogger.Debug("Password validation - Length: 14, Uppercase: True, Lowercase: True, Number: True, Special: True");
            validatorLogger.Debug("=== END NLOG VALIDATOR TEST ===");

            return Ok(new
            {
                message = "Test validator logs generated",
                note = "Check your logs to see if these messages appear"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TestValidator endpoint");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpGet("debug-routes")]
    public IActionResult DebugRoutes()
    {
        var routes = new List<string>();

        // Manually list your debug endpoints
        routes.Add("/api/Debug/test-logs");
        routes.Add("/api/Debug/check-filesystem");
        routes.Add("/api/Debug/create-log-directory");
        routes.Add("/api/Debug/serilog-status");
        routes.Add("/api/Debug/test-validator");
        routes.Add("/api/Debug/debug-routes");

        return Ok(new
        {
            message = "Available debug endpoints",
            baseUrl = $"http://localhost:5085",
            endpoints = routes,
            note = "Try accessing these endpoints"
        });
    }
}