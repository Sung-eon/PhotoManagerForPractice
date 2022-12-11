Param(
    [switch] $watch,
    [switch] $clean
)

$PHOTO_MANAGER_DIR = "./PhotoManager/"
$PROJECT_DIR = $PHOTO_MANAGER_DIR + "PhotoManager.csproj"

$env:PHOTO_ROOT_DIR = "/Users/sungeon/Pictures"

$env:DefaultConnection = "DataSource=app.db;Cache=Shared"


If($clean)
{
    dotnet clean
}

If($watch)
{
    dotnet watch run --project $PROJECT_DIR
}
else 
{
    dotnet run --project $PROJECT_DIR
}
