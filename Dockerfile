# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HseBackend.csproj", "./"]
RUN dotnet restore "HseBackend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "HseBackend.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "HseBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
# Copy the database if it exists in the source, though ideally use a volume or let it be created
# If hse.db is not in /app/publish (which it won't be by default), we might need to copy it manually if you want initial data
# COPY hse.db . 
# The above line is commented; the app will create a new DB if missing via EnsureCreated()

ENTRYPOINT ["dotnet", "HseBackend.dll"]
