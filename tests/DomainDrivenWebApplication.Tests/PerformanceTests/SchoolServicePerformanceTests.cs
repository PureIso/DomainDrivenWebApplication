using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Channels;
using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Services;
using DomainDrivenWebApplication.Tests.Fixtures;
using ErrorOr;

namespace DomainDrivenWebApplication.Tests.PerformanceTests;

/// <summary>
/// Performance tests for the SchoolService class to evaluate operation times under various scenarios.
/// </summary>
public class SchoolServicePerformanceTests(SchoolFixture fixture)
    : IClassFixture<SchoolFixture>, IDisposable, IAsyncLifetime
{
    /// <summary>
    /// Logs the test results to the CSV file.
    /// </summary>
    private static void LogResultToCsv(string testName, string header, string row)
    {
        string resultsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "results");
        if (!Directory.Exists(resultsDirectory))
        {
            Directory.CreateDirectory(resultsDirectory);
        }
        string uniqueFileName = Path.Combine(resultsDirectory, $"{testName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        if (!File.Exists(uniqueFileName) || new FileInfo(uniqueFileName).Length == 0)
        {
            File.WriteAllText(uniqueFileName, $"sep=,\n{header}\n");
        }
        File.AppendAllText(uniqueFileName, $"{row}\n");
    }

    [Fact]
    public async Task PerformanceTest_CRUDSequenceAsync()
    {
        const int iterations = 3000;
        SchoolServiceCommandQuery schoolService = fixture.SchoolServiceCommandQuery;
        const string testName = nameof(PerformanceTest_CRUDSequenceAsync);
        const string header = "\"TestName\",\"Operation\",\"Count\",\"MinMs\",\"AvgMs\",\"MaxMs\",\"TotalMs\",\"Passed\",\"Failed\",\"OverallMinMs\",\"OverallAvgMs\",\"OverallMaxMs\",\"OverallTotalMs\"";

        Dictionary<string, List<long>> operationDurations = new()
        {
            { "Create", new List<long>() },
            { "Read", new List<long>() },
            { "Update", new List<long>() },
            { "Delete", new List<long>() }
        };

        Dictionary<string, int> operationPassCount = new()
        {
            { "Create", 0 },
            { "Read", 0 },
            { "Update", 0 },
            { "Delete", 0 }
        };

        Dictionary<string, int> operationFailCount = new()
        {
            { "Create", 0 },
            { "Read", 0 },
            { "Update", 0 },
            { "Delete", 0 }
        };

        for (int i = 0; i < iterations; i++)
        {
            Stopwatch? createStopwatch = null;
            Stopwatch? readStopwatch = null;
            Stopwatch? updateStopwatch = null;
            Stopwatch? deleteStopwatch = null;

            try
            {
                School newSchool = new School
                {
                    Name = $"TestSchool_{Guid.NewGuid()}",
                    Address = "123 Test Lane",
                    PrincipalName = "Test Principal",
                    CreatedAt = DateTime.UtcNow
                };

                // Create operation
                createStopwatch = Stopwatch.StartNew();
                ErrorOr<bool> createResult = await schoolService.AddSchoolAsync(newSchool);
                createStopwatch.Stop();
                operationDurations["Create"].Add(createStopwatch.ElapsedMilliseconds);
                if (createResult.IsError) operationFailCount["Create"]++;
                else operationPassCount["Create"]++;

                // Read operation
                readStopwatch = Stopwatch.StartNew();
                ErrorOr<List<School>> readResult = await schoolService.GetAllSchoolsAsync();
                readStopwatch.Stop();
                operationDurations["Read"].Add(readStopwatch.ElapsedMilliseconds);
                if (readResult.IsError || readResult.Value.All(s => s.Name != newSchool.Name)) operationFailCount["Read"]++;
                else operationPassCount["Read"]++;

                // Update operation
                updateStopwatch = Stopwatch.StartNew();
                newSchool.Address = "Updated Test Lane";
                ErrorOr<bool> updateResult = await schoolService.UpdateSchoolAsync(newSchool);
                updateStopwatch.Stop();
                operationDurations["Update"].Add(updateStopwatch.ElapsedMilliseconds);
                if (updateResult.IsError) operationFailCount["Update"]++;
                else operationPassCount["Update"]++;

                // Delete operation
                deleteStopwatch = Stopwatch.StartNew();
                ErrorOr<bool> deleteResult = await schoolService.DeleteSchoolAsync(newSchool.Id);
                deleteStopwatch.Stop();
                operationDurations["Delete"].Add(deleteStopwatch.ElapsedMilliseconds);
                if (deleteResult.IsError) operationFailCount["Delete"]++;
                else operationPassCount["Delete"]++;
            }
            catch (Exception)
            {
                if (createStopwatch?.IsRunning == true) operationFailCount["Create"]++;
                if (readStopwatch?.IsRunning == true) operationFailCount["Read"]++;
                if (updateStopwatch?.IsRunning == true) operationFailCount["Update"]++;
                if (deleteStopwatch?.IsRunning == true) operationFailCount["Delete"]++;
            }
        }

        long overallTotalTime = operationDurations.Values.SelectMany(durations => durations).Sum();
        double overallAverageTime = operationDurations.Values.SelectMany(durations => durations).Average();
        long overallMinTime = operationDurations.Values.SelectMany(durations => durations).Min();
        long overallMaxTime = operationDurations.Values.SelectMany(durations => durations).Max();

        foreach ((string operation, List<long> durations) in operationDurations)
        {
            string count = durations.Count.ToString();
            string min = durations.Min().ToString();
            string average = durations.Average().ToString(CultureInfo.InvariantCulture);
            string max = durations.Max().ToString();
            string total = durations.Sum().ToString();
            string passed = operationPassCount[operation].ToString();
            string failed = operationFailCount[operation].ToString();

            string row = $"" +
                         $"{testName}," +
                         $"{operation}," +
                         $"{count}," +
                         $"{min}," +
                         $"{average}," +
                         $"{max}," +
                         $"{total}," +
                         $"{passed}," +
                         $"{failed}," +
                         $"{overallMinTime}," +
                         $"{overallAverageTime.ToString(CultureInfo.InvariantCulture)}," +
                         $"{overallMaxTime}," +
                         $"{overallTotalTime}";
            LogResultToCsv(testName, header, row);
        }
    }

    [Fact]
    public async Task PerformanceTest_CRUDSequenceNormalAsync()
    {
        const int iterations = 3000;
        SchoolService schoolService = fixture.SchoolService;
        const string testName = nameof(PerformanceTest_CRUDSequenceNormalAsync);
        const string header = "\"TestName\",\"Operation\",\"Count\",\"MinMs\",\"AvgMs\",\"MaxMs\",\"TotalMs\",\"Passed\",\"Failed\",\"OverallMinMs\",\"OverallAvgMs\",\"OverallMaxMs\",\"OverallTotalMs\"";

        Dictionary<string, List<long>> operationDurations = new()
        {
            { "Create", new List<long>() },
            { "Read", new List<long>() },
            { "Update", new List<long>() },
            { "Delete", new List<long>() }
        };

        Dictionary<string, int> operationPassCount = new()
        {
            { "Create", 0 },
            { "Read", 0 },
            { "Update", 0 },
            { "Delete", 0 }
        };

        Dictionary<string, int> operationFailCount = new()
        {
            { "Create", 0 },
            { "Read", 0 },
            { "Update", 0 },
            { "Delete", 0 }
        };

        for (int i = 0; i < iterations; i++)
        {
            Stopwatch? createStopwatch = null;
            Stopwatch? readStopwatch = null;
            Stopwatch? updateStopwatch = null;
            Stopwatch? deleteStopwatch = null;

            try
            {
                School newSchool = new School
                {
                    Name = $"TestSchool_{Guid.NewGuid()}",
                    Address = "123 Test Lane",
                    PrincipalName = "Test Principal",
                    CreatedAt = DateTime.UtcNow
                };

                // Create operation
                createStopwatch = Stopwatch.StartNew();
                ErrorOr<bool> createResult = await schoolService.AddSchoolAsync(newSchool);
                createStopwatch.Stop();
                operationDurations["Create"].Add(createStopwatch.ElapsedMilliseconds);
                if (createResult.IsError) operationFailCount["Create"]++;
                else operationPassCount["Create"]++;

                // Read operation
                readStopwatch = Stopwatch.StartNew();
                ErrorOr<List<School>> readResult = await schoolService.GetAllSchoolsAsync();
                readStopwatch.Stop();
                operationDurations["Read"].Add(readStopwatch.ElapsedMilliseconds);
                if (readResult.IsError || readResult.Value.All(s => s.Name != newSchool.Name)) operationFailCount["Read"]++;
                else operationPassCount["Read"]++;

                // Update operation
                updateStopwatch = Stopwatch.StartNew();
                newSchool.Address = "Updated Test Lane";
                ErrorOr<bool> updateResult = await schoolService.UpdateSchoolAsync(newSchool);
                updateStopwatch.Stop();
                operationDurations["Update"].Add(updateStopwatch.ElapsedMilliseconds);
                if (updateResult.IsError) operationFailCount["Update"]++;
                else operationPassCount["Update"]++;

                // Delete operation
                deleteStopwatch = Stopwatch.StartNew();
                ErrorOr<bool> deleteResult = await schoolService.DeleteSchoolAsync(newSchool.Id);
                deleteStopwatch.Stop();
                operationDurations["Delete"].Add(deleteStopwatch.ElapsedMilliseconds);
                if (deleteResult.IsError) operationFailCount["Delete"]++;
                else operationPassCount["Delete"]++;
            }
            catch (Exception)
            {
                if (createStopwatch?.IsRunning == true) operationFailCount["Create"]++;
                if (readStopwatch?.IsRunning == true) operationFailCount["Read"]++;
                if (updateStopwatch?.IsRunning == true) operationFailCount["Update"]++;
                if (deleteStopwatch?.IsRunning == true) operationFailCount["Delete"]++;
            }
        }

        long overallTotalTime = operationDurations.Values.SelectMany(durations => durations).Sum();
        double overallAverageTime = operationDurations.Values.SelectMany(durations => durations).Average();
        long overallMinTime = operationDurations.Values.SelectMany(durations => durations).Min();
        long overallMaxTime = operationDurations.Values.SelectMany(durations => durations).Max();

        foreach ((string operation, List<long> durations) in operationDurations)
        {
            string count = durations.Count.ToString();
            string min = durations.Min().ToString();
            string average = durations.Average().ToString(CultureInfo.InvariantCulture);
            string max = durations.Max().ToString();
            string total = durations.Sum().ToString();
            string passed = operationPassCount[operation].ToString();
            string failed = operationFailCount[operation].ToString();

            string row = $"" +
                         $"{testName}," +
                         $"{operation}," +
                         $"{count}," +
                         $"{min}," +
                         $"{average}," +
                         $"{max}," +
                         $"{total}," +
                         $"{passed}," +
                         $"{failed}," +
                         $"{overallMinTime}," +
                         $"{overallAverageTime.ToString(CultureInfo.InvariantCulture)}," +
                         $"{overallMaxTime}," +
                         $"{overallTotalTime}";
            LogResultToCsv(testName, header, row);
        }
    }

    [Fact]
    public async Task PerformanceTest_CRUDSequenceAsync_Realistic()
    {
        // Initialize service
        SchoolServiceCommandQuery schoolService = fixture.SchoolServiceCommandQuery;

        // Test parameters
        const int iterations = 3000;
        const string testName = nameof(PerformanceTest_CRUDSequenceAsync_Realistic);
        const string header = "\"TestName\",\"Operation\",\"Count\",\"MinMs\",\"AvgMs\",\"MaxMs\",\"TotalMs\",\"Passed\",\"Failed\",\"OverallMinMs\",\"OverallAvgMs\",\"OverallMaxMs\",\"OverallTotalMs\"";

        // Operation tracking
        Dictionary<string, ConcurrentBag<long>> operationDurations = new()
        {
            ["Create"] = new ConcurrentBag<long>(),
            ["Read_All"] = new ConcurrentBag<long>(),
            ["Read_ById"] = new ConcurrentBag<long>(),
            ["Update"] = new ConcurrentBag<long>(),
            ["Delete"] = new ConcurrentBag<long>()
        };

        Dictionary<string, int> operationPassCount = new()
        {
            ["Create"] = 0,
            ["Read_All"] = 0,
            ["Read_ById"] = 0,
            ["Update"] = 0,
            ["Delete"] = 0
        };

        Dictionary<string, int> operationFailCount = new()
        {
            ["Create"] = 0,
            ["Read_All"] = 0,
            ["Read_ById"] = 0,
            ["Update"] = 0,
            ["Delete"] = 0
        };

        // Channels
        Channel<School> createChannel = Channel.CreateUnbounded<School>();
        Channel<School> readChannel = Channel.CreateUnbounded<School>();
        Channel<School> updateChannel = Channel.CreateUnbounded<School>();
        Channel<School> deleteChannel = Channel.CreateUnbounded<School>();

        // Start all workers
        Task[] tasks =
        [
            Task.Run(CreateWorker), 
            Task.Run(ReadWorker), 
            Task.Run(UpdateWorker), 
            Task.Run(DeleteWorker)
        ];

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Aggregate results
        long overallTotalTime = operationDurations.Values.SelectMany(bag => bag).Sum();
        double overallAverageTime = operationDurations.Values.SelectMany(bag => bag).Average();
        long overallMinTime = operationDurations.Values.SelectMany(bag => bag).Min();
        long overallMaxTime = operationDurations.Values.SelectMany(bag => bag).Max();

        StringBuilder stringBuilder = new StringBuilder();
        foreach ((string operation, ConcurrentBag<long> durations) in operationDurations)
        {
            string count = durations.Count.ToString();
            string min = durations.Min().ToString();
            string average = durations.Average().ToString(CultureInfo.InvariantCulture);
            string max = durations.Max().ToString();
            string total = durations.Sum().ToString();
            string passed = operationPassCount[operation].ToString();
            string failed = operationFailCount[operation].ToString();

            string row =
                $"{testName},{operation},{count},{min},{average},{max},{total},{passed},{failed},{overallMinTime},{overallAverageTime.ToString(CultureInfo.InvariantCulture)},{overallMaxTime},{overallTotalTime}";
            stringBuilder.AppendLine(row);
        }
        LogResultToCsv(testName, header, stringBuilder.ToString());
        return;

        async Task DeleteWorker()
        {
            try
            {
                await foreach (School school in updateChannel.Reader.ReadAllAsync())
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    ErrorOr<bool> deleteResult = await schoolService.DeleteSchoolAsync(school.Id);
                    stopwatch.Stop();
                    operationDurations["Delete"].Add(stopwatch.ElapsedMilliseconds);

                    if (deleteResult.IsError)
                    {
                        operationFailCount["Delete"]++;
                    }
                    else
                    {
                        operationPassCount["Delete"]++;
                    }
                }
            }
            finally
            {
                deleteChannel.Writer.Complete();
            }
        }

        async Task UpdateWorker()
        {
            try
            {
                await foreach (School school in readChannel.Reader.ReadAllAsync())
                {
                    school.PrincipalName = $"Updated {Guid.NewGuid().ToString("N").Substring(0, 32)}";
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    ErrorOr<bool> updateResult = await schoolService.UpdateSchoolAsync(school);
                    stopwatch.Stop();
                    operationDurations["Update"].Add(stopwatch.ElapsedMilliseconds);

                    if (updateResult.IsError)
                    {
                        operationFailCount["Update"]++;
                    }
                    else
                    {
                        operationPassCount["Update"]++;
                        await updateChannel.Writer.WriteAsync(school);
                    }
                }
            }
            finally
            {
                updateChannel.Writer.Complete();
            }
        }

        async Task ReadWorker()
        {
            try
            {
                await foreach (School school in createChannel.Reader.ReadAllAsync())
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    ErrorOr<List<School>> readAllResult = await schoolService.GetAllSchoolsAsync();
                    stopwatch.Stop();
                    operationDurations["Read_All"].Add(stopwatch.ElapsedMilliseconds);

                    if (readAllResult.IsError || readAllResult.Value.All(s => s.Name != school.Name))
                    {
                        operationFailCount["Read_All"]++;
                    }
                    else
                    {
                        operationPassCount["Read_All"]++;

                        Stopwatch stopwatchById = Stopwatch.StartNew();
                        ErrorOr<School> readByIdResult = await schoolService.GetSchoolByIdAsync(school.Id);
                        stopwatchById.Stop();
                        operationDurations["Read_ById"].Add(stopwatchById.ElapsedMilliseconds);

                        if (readByIdResult.IsError)
                        {
                            operationFailCount["Read_ById"]++;
                        }
                        else
                        {
                            operationPassCount["Read_ById"]++;
                            await readChannel.Writer.WriteAsync(school);
                        }
                    }
                }
            }
            finally
            {
                readChannel.Writer.Complete();
            }
        }

        async Task CreateWorker()
        {
            try
            {
                for (int i = 0; i < iterations; i++)
                {
                    School newSchool = new School
                    {
                        Name = $"TestSchool_{Guid.NewGuid()}",
                        Address = "123 Test Lane",
                        PrincipalName = "Test Principal",
                        CreatedAt = DateTime.UtcNow
                    };

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    ErrorOr<bool> createResult = await schoolService.AddSchoolAsync(newSchool);
                    stopwatch.Stop();
                    operationDurations["Create"].Add(stopwatch.ElapsedMilliseconds);

                    if (createResult.IsError)
                    {
                        operationFailCount["Create"]++;
                    }
                    else
                    {
                        operationPassCount["Create"]++;
                        await createChannel.Writer.WriteAsync(newSchool);
                    }
                }
            }
            finally
            {
                createChannel.Writer.Complete();
            }
        }
    }

    [Fact]
    public async Task PerformanceTest_CRUDSequenceAsync_NormalRealistic()
    {
        // Initialize service
        SchoolService schoolService = fixture.SchoolService;

        // Test parameters
        const int iterations = 3000;
        const string testName = nameof(PerformanceTest_CRUDSequenceAsync_NormalRealistic);
        const string header = "\"TestName\",\"Operation\",\"Count\",\"MinMs\",\"AvgMs\",\"MaxMs\",\"TotalMs\",\"Passed\",\"Failed\",\"OverallMinMs\",\"OverallAvgMs\",\"OverallMaxMs\",\"OverallTotalMs\"";

        // Operation tracking
        Dictionary<string, ConcurrentBag<long>> operationDurations = new()
        {
            ["Create"] = new ConcurrentBag<long>(),
            ["Read_All"] = new ConcurrentBag<long>(),
            ["Read_ById"] = new ConcurrentBag<long>(),
            ["Update"] = new ConcurrentBag<long>(),
            ["Delete"] = new ConcurrentBag<long>()
        };

        Dictionary<string, int> operationPassCount = new()
        {
            ["Create"] = 0,
            ["Read_All"] = 0,
            ["Read_ById"] = 0,
            ["Update"] = 0,
            ["Delete"] = 0
        };

        Dictionary<string, int> operationFailCount = new()
        {
            ["Create"] = 0,
            ["Read_All"] = 0,
            ["Read_ById"] = 0,
            ["Update"] = 0,
            ["Delete"] = 0
        };

        // Channels
        Channel<School> createChannel = Channel.CreateUnbounded<School>();
        Channel<School> readChannel = Channel.CreateUnbounded<School>();
        Channel<School> updateChannel = Channel.CreateUnbounded<School>();
        Channel<School> deleteChannel = Channel.CreateUnbounded<School>();

        // Start all workers
        Task[] tasks =
        [
            Task.Run(CreateWorker),
            Task.Run(ReadWorker),
            Task.Run(UpdateWorker),
            Task.Run(DeleteWorker)
        ];

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Aggregate results
        long overallTotalTime = operationDurations.Values.SelectMany(bag => bag).Sum();
        double overallAverageTime = operationDurations.Values.SelectMany(bag => bag).Average();
        long overallMinTime = operationDurations.Values.SelectMany(bag => bag).Min();
        long overallMaxTime = operationDurations.Values.SelectMany(bag => bag).Max();

        StringBuilder stringBuilder = new StringBuilder();
        foreach ((string operation, ConcurrentBag<long> durations) in operationDurations)
        {
            string count = durations.Count.ToString();
            string min = durations.Min().ToString();
            string average = durations.Average().ToString(CultureInfo.InvariantCulture);
            string max = durations.Max().ToString();
            string total = durations.Sum().ToString();
            string passed = operationPassCount[operation].ToString();
            string failed = operationFailCount[operation].ToString();

            string row =
                $"{testName},{operation},{count},{min},{average},{max},{total},{passed},{failed},{overallMinTime},{overallAverageTime.ToString(CultureInfo.InvariantCulture)},{overallMaxTime},{overallTotalTime}";
            stringBuilder.AppendLine(row);
        }
        LogResultToCsv(testName, header, stringBuilder.ToString());
        return;

        async Task DeleteWorker()
        {
            try
            {
                await foreach (School school in updateChannel.Reader.ReadAllAsync())
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    ErrorOr<bool> deleteResult = await schoolService.DeleteSchoolAsync(school.Id);
                    stopwatch.Stop();
                    operationDurations["Delete"].Add(stopwatch.ElapsedMilliseconds);

                    if (deleteResult.IsError)
                    {
                        operationFailCount["Delete"]++;
                    }
                    else
                    {
                        operationPassCount["Delete"]++;
                    }
                }
            }
            finally
            {
                deleteChannel.Writer.Complete();
            }
        }

        async Task UpdateWorker()
        {
            try
            {
                await foreach (School school in readChannel.Reader.ReadAllAsync())
                {
                    school.PrincipalName = $"Updated {Guid.NewGuid().ToString("N").Substring(0, 32)}";
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    ErrorOr<bool> updateResult = await schoolService.UpdateSchoolAsync(school);
                    stopwatch.Stop();
                    operationDurations["Update"].Add(stopwatch.ElapsedMilliseconds);

                    if (updateResult.IsError)
                    {
                        operationFailCount["Update"]++;
                    }
                    else
                    {
                        operationPassCount["Update"]++;
                        await updateChannel.Writer.WriteAsync(school);
                    }
                }
            }
            finally
            {
                updateChannel.Writer.Complete();
            }
        }

        async Task ReadWorker()
        {
            try
            {
                await foreach (School school in createChannel.Reader.ReadAllAsync())
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    ErrorOr<List<School>> readAllResult = await schoolService.GetAllSchoolsAsync();
                    stopwatch.Stop();
                    operationDurations["Read_All"].Add(stopwatch.ElapsedMilliseconds);

                    if (readAllResult.IsError || readAllResult.Value.All(s => s.Name != school.Name))
                    {
                        operationFailCount["Read_All"]++;
                    }
                    else
                    {
                        operationPassCount["Read_All"]++;

                        Stopwatch stopwatchById = Stopwatch.StartNew();
                        ErrorOr<School> readByIdResult = await schoolService.GetSchoolByIdAsync(school.Id);
                        stopwatchById.Stop();
                        operationDurations["Read_ById"].Add(stopwatchById.ElapsedMilliseconds);

                        if (readByIdResult.IsError)
                        {
                            operationFailCount["Read_ById"]++;
                        }
                        else
                        {
                            operationPassCount["Read_ById"]++;
                            await readChannel.Writer.WriteAsync(school);
                        }
                    }
                }
            }
            finally
            {
                readChannel.Writer.Complete();
            }
        }

        async Task CreateWorker()
        {
            try
            {
                for (int i = 0; i < iterations; i++)
                {
                    School newSchool = new School
                    {
                        Name = $"TestSchool_{Guid.NewGuid()}",
                        Address = "123 Test Lane",
                        PrincipalName = "Test Principal",
                        CreatedAt = DateTime.UtcNow
                    };

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    ErrorOr<bool> createResult = await schoolService.AddSchoolAsync(newSchool);
                    stopwatch.Stop();
                    operationDurations["Create"].Add(stopwatch.ElapsedMilliseconds);

                    if (createResult.IsError)
                    {
                        operationFailCount["Create"]++;
                    }
                    else
                    {
                        operationPassCount["Create"]++;
                        await createChannel.Writer.WriteAsync(newSchool);
                    }
                }
            }
            finally
            {
                createChannel.Writer.Complete();
            }
        }
    }

    public async Task InitializeAsync()
    {
        await fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await fixture.DisposeAsync();
    }

    public void Dispose()
    {
        fixture.DisposeAsync().GetAwaiter().GetResult();
    }
}
