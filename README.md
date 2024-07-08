# DomainDrivenWebApplication Solution

Welcome to the README file for the DomainDrivenWebApplication solution. This document provides an overview of the solution structure, instructions for running the application, details about running tests, contributing guidelines, and licensing information.

## Solution Structure

The solution consists of the following projects:

- `DomainDrivenWebApplication`: The main project containing domain entities, services, and business logic.
- `DomainDrivenWebApplication.Infrastructure`: Infrastructure project providing implementations for domain repositories and data access.
- `DomainDrivenWebApplication.Tests`: Project containing unit tests and integration tests for the solution.

## Frameworks and Requirements

The solution requires the following frameworks and tools:

- .NET Core SDK
- Docker Desktop (for running integration tests)

## Running the Application

To run the application locally, follow these steps:

1. Clone the repository to your local machine.
2. Open the solution in Visual Studio.
3. Build the solution to restore NuGet packages and compile the code.
4. Set the `DomainDrivenWebApplication` project as the startup project.
5. Press F5 to run the application.

## Running the Tests

### Unit Tests

To run unit tests:

1. Open the solution in Visual Studio.
2. Build the solution to restore NuGet packages and compile the code.
3. Open the Test Explorer window (`Test` > `Windows` > `Test Explorer`).
4. Click on `Run All Tests` in the Test Explorer window.

### Integration Tests

To run integration tests:

1. Ensure Docker Desktop is running on your machine.
2. Open the solution in Visual Studio.
3. Build the solution to restore NuGet packages and compile the code.
4. Open the Test Explorer window (`Test` > `Windows` > `Test Explorer`).
5. Click on `Run All Tests` in the Test Explorer window.

## Contributing

We welcome contributions to this project! If you would like to contribute, please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bug fix (`git checkout -b feature/your-feature-name`).
3. Make your changes and commit them to your branch.
4. Push your branch to your forked repository (`git push origin feature/your-feature-name`).
5. Open a pull request to merge your changes into the main repository.

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for more details.
