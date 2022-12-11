Param(
    [switch] $DROP
)

If($DROP)
{
    Write-Host "Drop the database 'Photos'"
    dotnet ef database drop --context PhotoDbContext --force
}

$PHOTO_APP_MIGRATION = "./Data/PhotoMigrations"
$THUMBNAIL = "./thumbnail"
$PHOTO_DB_SQLITE_FILE = "photo.db"

Write-Host "Doing delete Migration Folders if exists"

If(Test-Path -Path $PHOTO_APP_MIGRATION)
{
    Remove-Item $PHOTO_APP_MIGRATION -Recurse
}

Write-Host "Doing delete thumbnail folder if exists"

If(Test-Path -Path $THUMBNAIL)
{
    Remove-Item $THUMBNAIL -Recurse
    New-Item -Path . -Name "thumbnail" -ItemType "directory"
}

If(Test-Path -Path $PHOTO_DB_SQLITE_FILE -PathType leaf)
{
    Write-Host "Doing delete photo.db"
    Remove-Item $PHOTO_DB_SQLITE_FILE
}

dotnet ef migrations add InitialCreate --output-dir $PHOTO_APP_MIGRATION --context PhotoDbContext

dotnet ef database update --context PhotoDbContext