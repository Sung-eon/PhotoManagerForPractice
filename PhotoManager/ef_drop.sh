#!/usr/bin/env bash

echo "Drop the database 'Photos'"
dotnet ef database drop --context PhotoDbContext