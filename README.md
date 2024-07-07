# DomainDrivenWebApplication Solution

This is the README file for the DomainDrivenWebApplication solution. It provides an overview of the solution structure and instructions for running the application.

## Solution Structure

The solution consists of the following projects:

- `DomainDrivenWebApplication`: The main project that contains the domain entities, services, and repositories.
- `DomainDrivenWebApplication.Infrastructure`: The infrastructure project that provides implementations for the domain repositories.
- `DomainDrivenWebApplication.Tests`: The test project that contains unit tests and integration tests for the solution.

## Running the Application

To run the application, follow these steps:

1. Clone the repository to your local machine.
2. Open the solution in Visual Studio.
3. Build the solution to restore NuGet packages and compile the code.
4. Set the `DomainDrivenWebApplication` project as the startup project.
5. Press F5 to run the application.

## Running the Tests

To run the tests, follow these steps:

1. Open the solution in Visual Studio.
2. Build the solution to restore NuGet packages and compile the code.
3. Open the Test Explorer window (go to `Test` > `Windows` > `Test Explorer`).
4. Click on the `Run All Tests` button in the Test Explorer window.

### Integration Tests

To run the integration tests, you will need the following requirements:

- .NET Core SDK
- Docker Desktop

Follow these additional steps to run the integration tests:

1. Make sure Docker Desktop is running.
2. Open the solution in Visual Studio.
3. Build the solution to restore NuGet packages and compile the code.
4. Open the Test Explorer window (go to `Test` > `Windows` > `Test Explorer`).
5. Click on the `Run All Tests` button in the Test Explorer window.

## Contributing

If you would like to contribute to this project, please follow these guidelines:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and commit them to your branch.
4. Push your branch to your forked repository.
5. Open a pull request to merge your changes into the main repository.

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for more information.
