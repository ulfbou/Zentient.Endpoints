# Contributing to the Zentient Framework

We welcome and deeply appreciate your contributions to the Zentient Framework! By contributing, you play a vital role in building a robust, predictable, and developer-first ecosystem for the entire .NET community. This document outlines the guidelines and processes for contributing across all Zentient projects.

## Code of Conduct

To ensure a welcoming, inclusive, and professional environment, all contributors are expected to adhere to our [Code of Conduct](CODE_OF_CONDUCT.md). Please review it before contributing.

## How Can I Contribute?

Your contributions can take many forms:

### 1. Reporting Bugs

If you discover a bug in any Zentient library, please open an issue on the relevant project's GitHub Issues page.
When reporting, please provide:
* A clear and concise description of the bug.
* Detailed steps to reproduce the behavior.
* The expected vs. actual behavior.
* Screenshots or minimal code snippets if helpful.
* Your .NET SDK version(s) and operating system.

### 2. Suggesting Enhancements

Have an idea for a new feature, API improvement, or better documentation? We'd love to hear it!
Open an issue on the relevant project's GitHub Issues page and describe your suggestion, including:
* A clear and concise description of the proposed enhancement.
* The specific problem it solves or the benefit it provides.
* Any potential alternatives or considerations you've explored.

### 3. Writing Code

We highly value contributions of code for bug fixes, new features, or refactorings. Before starting any significant work:

* **Check existing issues:** See if your contribution is already being discussed or worked on.
* **Open an issue:** For new features, significant changes, or complex bug fixes, please open an issue first to discuss your idea with the maintainers. This ensures alignment, prevents duplicate efforts, and helps us guide you.
* **Fork the repository:** Create a fork of the Zentient Framework's main repository on GitHub.

### 4. Improving Documentation

High-quality, developer-first documentation is crucial for Zentient. If you find typos, inaccuracies, or areas that could be explained more clearly in any `README.md`, conceptual guides (`/docs/`), or code comments, please consider contributing. You can submit a pull request with your changes or open an issue.

## Getting Started with Development

To set up your local development environment for the Zentient Framework:

1.  **Prerequisites:**
    * .NET SDK (version 8.0 or newer). We recommend using the latest stable version of .NET 9.0.
    * [GitVersion CLI](https://gitversion.net/docs/): For consistent versioning across the framework.
    * [pre-commit](https://pre-commit.com/#installation): For local code formatting and linting checks before committing. Install it via pip (`pip install pre-commit`) and then run `pre-commit install` in the repository root.

2.  **Clone the Repository:**
    ```bash
    git clone [https://github.com/ulfbou/Zentient.Framework.git](https://github.com/ulfbou/Zentient.Framework.git) # Adjust if main repo name is different
    cd Zentient.Framework
    ```

3.  **Build the Project:**
    ```bash
    dotnet build # This will build all projects in the solution
    ```

4.  **Run Tests:**
    ```bash
    dotnet test # This will run tests for all test projects
    ```

## Your First Code Contribution (Workflow)

Once you've set up your development environment:

1.  **Create a New Branch:**
    * For new features: `git checkout -b feat/your-feature-name`
    * For bug fixes: `git checkout -b fix/issue-number-short-description`
    * For documentation: `git checkout -b docs/clarify-api-usage`
    * For refactoring/chore: `git checkout -b chore/refactor-module-x`

2.  **Make Your Changes:** Implement your feature, bug fix, or documentation update.

3.  **Adhere to Framework Conventions:**
    * **Coding Style:** Follow standard .NET coding conventions. We use a `.editorconfig` file to enforce consistent formatting; your IDE should pick this up automatically. Run `dotnet format` locally before committing (`pre-commit` will also enforce this).
    * **Zentient Design Principles:** Ensure your changes align with the framework's core philosophies:
        * **Async-First:** All public APIs designed for I/O or long-running operations must be asynchronous (`Task<T>`, `ValueTask<T>`) and omit the `Async` suffix if no synchronous counterpart exists. Avoid `.Result` or `.Wait()`.
        * **Developer-First:** Prioritize predictability, clear API surface, and compiler-guided correctness.
        * **Interface-First:** Public contracts (method return types, parameters) should rely on interfaces (e.g., `IResult<T>`, `IEndpointOutcome`), not concrete types.
        * **Immutability:** All result, outcome, and metadata types (like `ErrorInfo`, `Result`, `EndpointOutcome`, `TransportMetadata`) must be immutable (using `init` setters).
        * **Naming Conventions:** Adhere strictly to the guidelines in `/docs/conventions/naming-conventions.md` (e.g., `I` prefix for interfaces, `Extensions` suffix for extension classes, `With`/`Set`/`As`/`From` patterns).
        * **Cancellation Support:** Include `CancellationToken` for methods performing I/O or potentially long-running operations.

4.  **Write Tests:**
    * All new features must be accompanied by comprehensive unit tests.
    * Bug fixes should include a regression test that fails without your fix and passes with it.
    * Ensure all existing tests pass after your changes (`dotnet test`).
    * Aim for high code coverage, especially for core logic.

5.  **Commit Your Changes:**
    Write clear, concise commit messages. We encourage using [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) (e.g., `feat(endpoints): add new result type`, `fix(results): resolve serialization issue`, `docs: update async guidelines`).
    ```bash
    git add .
    git commit -m "feat(module): descriptive commit message"
    ```

6.  **Push to Your Fork:**
    ```bash
    git push origin feat/my-new-feature
    ```

7.  **Create a Pull Request (PR):**
    * Go to the Zentient Framework GitHub repository (or your fork) and you should see a prompt to create a new pull request from your pushed branch.
    * **Target the `develop` branch.**
    * Provide a clear title and detailed description for your PR.
    * Reference any related issues (e.g., `Closes #123` or `Fixes #123`).
    * Explain the changes you've made, why they are necessary, and how they align with Zentient's conventions.
    * Ensure all CI checks (formatting, analyzers, tests) pass.
    * Be prepared for constructive feedback and discussions during the review process.

## License

By contributing to the Zentient Framework, you agree that your contributions will be licensed under the [MIT License](https://github.com/ulfbou/Zentient.Framework/blob/main/LICENSE) (adjust path if different).

## Support & Contact

If you have any questions or need clarification on the contributing process, please don't hesitate to open a [GitHub Issue](https://github.com/ulfbou/Zentient.Framework/issues) or join our [Discussions board](https://github.com/ulfbou/Zentient.Framework/discussions) (if applicable).

Thank you for helping us build the future of predictable .NET outcomes!

---

## 📚 Resources & Related Documents

* [Zentient.Results NuGet](https://www.nuget.org/packages/Zentient.Results)
* [Zentient.Endpoints NuGet](https://www.nuget.org/packages/Zentient.Endpoints)
* [GitVersion Documentation](https://gitversion.net/docs/)
* [dotnet format Documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)
* `/docs/conventions/async-guidelines.md`
* `/docs/conventions/developer-first.md`
* `/docs/conventions/naming-conventions.md`
* `/docs/architecture/transport-pipeline.md`
* `/docs/analyzers/rules.md`
