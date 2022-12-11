#!/usr/bin/env bash

AUTH_MIGRATION_FOLDER="./Data/AuthMigrations"
PHOTO_APP_MIGRATION="./Data/PhotoMigrations"

echo "Deleting Migration folder for PhotoDbContext"
if [ -d $AUTH_MIGRATION_FOLDER ]; then
    rm -rf $AUTH_MIGRATION_FOLDER
fi

if [ -d $PHOTO_APP_MIGRATION ]; then
    rm -rf $PHOTO_APP_MIGRATION
fi


echo "Cleaning thumbnail folder"
rm -rf ./thumbnail/*

PHOTO_DB="photo.db"
if [ -e $PHOTO_DB ]; then
    echo "Deleting sqlite database file for PhotoDbContext"
    rm $PHOTO_DB
fi

echo "Creating migration files for PhotoDbContext"
dotnet ef migrations add InitialCreate --output-dir $PHOTO_APP_MIGRATION --context PhotoDbContext
#echo "Creating migration files for ASP.NET Identity"
#dotnet ef migrations add InitialCreate --output-dir $AUTH_MIGRATION_FOLDER --context ApplicationDbContext

echo "Applying migration file - PhotoDbContext"
dotnet ef database update --context PhotoDbContext
#echo "Applying migration file - ASP.NET Identity"
#dotnet ef database update --context ApplicationDbContext