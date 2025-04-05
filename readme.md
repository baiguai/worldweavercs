# WorldWeaver

WorldWeaver is a **C# .NET Core Interactive Fiction Engine** designed for flexibility and cross-platform compatibility.

## Features
- **Cross-platform**: Runs on Linux, Windows, and macOS.
- **Expandable**: Designed to support custom story formats and interactive mechanics.
- **Lightweight**: Minimal dependencies with a focus on performance.

## Getting Started

### Prerequisites
Ensure you have the following installed:
- **.NET SDK 8.0+** ([Download Here](https://dotnet.microsoft.com/download))
- **Git** (optional, for cloning the repository)

### Installation
#### Clone the Repository
```sh
git clone https://github.com/baiguai/worldweavercs.git
cd worldweavercs
```

> Note:\
You want to be one level up from the WorldWeaver directory when calling GIT commands, but\
within it when doing dotnet run or build etc.\
This is due to the game files and other artifacts that are outside the code base.

#### Install Dependencies
```sh
dotnet restore ./WorldWeaver
```

---

#### Build the Project
CD into the WorldWeaver directory.

* For Windows:
```
dotnet publish -c Release -r win-x64 --self-contained true
```
* For Linux:
```
dotnet publish -c Release -r linux-x64 --self-contained true
```
* For Mac:
### Mac 64
```
dotnet publish -c Release -r osx-x64 --self-contained true
```
### Apple Silicon (M1/M2/M3 ARM-based)
```
dotnet publish -c Release -r osx-arm64 --self-contained true
```


#### Run the Engine
```sh
dotnet run --project ./WorldWeaver
```

## Development Environment Setup

### **Linux (VSCode)**
1. Install **VSCode** ([Download Here](https://code.visualstudio.com/))
2. Install the **C# Dev Kit** and **.NET Core SDK**
3. Open the folder in VSCode:
   ```sh
   code worldweavercs
   ```
4. Use the built-in terminal for **building and running**:
   ```sh
   dotnet build ./WorldWeaver
   dotnet run --project ./WorldWeaver
   ```

### **Windows (Visual Studio)**
1. Install **Visual Studio 2022+** ([Download Here](https://visualstudio.microsoft.com/))
2. During installation, select:
   - `.NET Core cross-platform development`
3. Open `WorldWeaver.sln` in Visual Studio.
4. Press `Ctrl + F5` to build and run.

### **macOS**
1. Install **.NET SDK** via Homebrew:
   ```sh
   brew install dotnet
   ```
2. Install **VSCode** or **Rider** for IDE support.
3. Navigate to the project folder and run:
   ```sh
   dotnet run --project ./WorldWeaver
   ```

## Contribution Guidelines
1. Fork the repository.
2. Create a feature branch.
3. Commit changes and push to your fork.
4. Submit a Pull Request.
