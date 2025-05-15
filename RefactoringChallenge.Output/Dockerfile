FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RefactoringChallenge.Output/RefactoringChallenge.Output.csproj", "RefactoringChallenge.Output/"]
COPY ["RefactoringChallenge.Common/RefactoringChallenge.Common.csproj", "RefactoringChallenge.Common/"]
COPY ["RefactoringChallenge.Orchestration/RefactoringChallenge.Orchestration.csproj", "RefactoringChallenge.Orchestration/"]
RUN dotnet restore "RefactoringChallenge.Output/RefactoringChallenge.Output.csproj"
COPY . .
WORKDIR "/src/RefactoringChallenge.Output"
RUN dotnet build "RefactoringChallenge.Output.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RefactoringChallenge.Output.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RefactoringChallenge.Output.dll"]
