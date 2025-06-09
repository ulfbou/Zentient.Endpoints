# Contributing to Zentient.Endpoints

First off, thank you for considering contributing to Zentient.Endpoints — your time and effort are greatly appreciated.

## 🧭 Project Goals

Zentient.Endpoints is a high-quality, modular library that standardizes endpoint result adaptation across transport layers (HTTP, gRPC, Messaging, etc.). It builds on the principles of **predictable execution**, **composability**, and **functional-style error modeling** powered by [Zentient.Results](https://www.nuget.org/packages/Zentient.Results).

Our goals are:
- 🔒 Type-safe and protocol-agnostic result mapping
- 💡 Seamless developer experience
- 📦 CI/CD rigor and reproducibility

## ⚙️ How to Contribute

### 1. Fork the Repository

Click the "Fork" button on GitHub, then clone your fork:

```bash
git clone https://github.com/<your-username>/Zentient.Endpoints.git
cd Zentient.Endpoints
````

### 2. Install Prerequisites

Ensure you have the following installed:

* [.NET SDK 8 and 9](https://dotnet.microsoft.com/en-us/download)
* [GitVersion CLI](https://gitversion.net/docs/)
* [pre-commit](https://pre-commit.com/#installation) (for local formatting checks)

```bash
pip install pre-commit
pre-commit install
```

### 3. Create a Feature Branch

```bash
git checkout -b feature/my-new-feature
```

### 4. Make Your Changes

* Follow the `.editorconfig` and code style rules.
* Run `dotnet format` before committing (automatically enforced if you installed pre-commit).
* Make sure `dotnet build -warnaserror` and `dotnet test` pass locally.

### 5. Commit Your Work

```bash
git commit -m "Add: meaningful commit message"
git push origin feature/my-new-feature
```

### 6. Submit a Pull Request

Open a pull request against the `develop` branch with a clear description of your changes.

---

## ✅ Contribution Requirements

All pull requests **must** meet the following:

* ✅ All code passes CI (formatting, analyzers, tests).
* 🧪 New functionality includes tests.
* 🧼 Public APIs include XML docs.
* 🧭 Behavior aligns with functional result modeling and the Zentient design philosophy.

---

## 📚 Resources

* [Zentient.Results](https://www.nuget.org/packages/Zentient.Results)
* [Zentient.Endpoints README](./README.md)
* [GitVersion](https://gitversion.net/docs/)
* [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)

---

## 🗣 Join the Conversation

* Open a [Discussion](https://github.com/zentient/zentient.endpoints/discussions) to propose ideas or ask questions.
* File a [GitHub Issue](https://github.com/zentient/zentient.endpoints/issues) to report bugs or suggest enhancements.

Thank you for helping build the future of transport-agnostic endpoint standardization.
