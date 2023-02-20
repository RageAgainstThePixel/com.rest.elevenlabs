# Copyright (c) Stephen Hodgson. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

$InputAuthor = Read-Host "Set Author name: (i.e. your GitHub username)"
$ProjectAuthor = "ProjectAuthor"

$InputName = Read-Host "Enter a name for your new project"
$ProjectName = "ProjectName"

$InputScope = Read-Host "Enter a scope for your new project (optional)"

if(-not [String]::IsNullOrWhiteSpace($InputScope)) {
  $InputScope = "$InputScope."
}

$ProjectScope = "ProjectScope."

Write-Host "Your new com.$($InputScope.ToLower())$($InputName.ToLower()) project is being created..."
Remove-Item -Path ".\Readme.md"
Copy-Item -Path ".\$ProjectScope$ProjectName\Packages\com.$($ProjectScope.ToLower())$($ProjectName.ToLower())\Documentation~\Readme.md" `
          -Destination ".\Readme.md"

# Rename any directories before we crawl the folders
Rename-Item -Path ".\$ProjectScope$ProjectName\Packages\com.$($ProjectScope.ToLower())$($ProjectName.ToLower())\Runtime\$ProjectScope$ProjectName.asmdef" `
            -NewName "$InputScope$InputName.asmdef"
Rename-Item -Path ".\$ProjectScope$ProjectName\Packages\com.$($ProjectScope.ToLower())$($ProjectName.ToLower())\Editor\$ProjectScope$ProjectName.Editor.asmdef" `
            -NewName "$InputScope$InputName.Editor.asmdef"
Rename-Item -Path ".\$ProjectScope$ProjectName\Packages\com.$($ProjectScope.ToLower())$($ProjectName.ToLower())\Tests\$ProjectScope$ProjectName.Tests.asmdef" `
            -NewName "$InputScope$InputName.Tests.asmdef"
Rename-Item -Path ".\$ProjectScope$ProjectName\Packages\com.$($ProjectScope.ToLower())$($ProjectName.ToLower())" `
            -NewName "com.$($InputScope.ToLower())$($InputName.ToLower())"
Rename-Item -Path ".\$ProjectScope$ProjectName" `
            -NewName ".\$InputScope$InputName"

$excludes = @('*Library*', '*Obj*','*InitializeTemplate*')
Get-ChildItem -Path "*"-File -Recurse -Exclude $excludes | ForEach-Object -Process {
  $isValid = $true

  foreach ($exclude in $excludes) {
    if ((Split-Path -Path $_.FullName -Parent) -ilike $exclude) {
      $isValid = $false
      break
    }
  }

  if ($isValid) {
    Get-ChildItem -Path $_ -File | ForEach-Object -Process {
      $updated = $false;

      $fileContent = Get-Content $($_.FullName) -Raw

      # Rename all PascalCase instances
      if ($fileContent -cmatch $ProjectName) {
        $fileContent -creplace $ProjectName, $InputName | Set-Content $($_.FullName) -NoNewline
        $updated = $true
      }

      $fileContent = Get-Content $($_.FullName) -Raw

      if ($fileContent -cmatch $ProjectScope) {
        $fileContent -creplace $ProjectScope, $InputScope | Set-Content $($_.FullName) -NoNewline
        $updated = $true
      }

      $fileContent = Get-Content $($_.FullName) -Raw

      if ($fileContent -cmatch $ProjectAuthor) {
        $fileContent -creplace $ProjectAuthor, $InputAuthor | Set-Content $($_.FullName) -NoNewline
        $updated = $true
      }

      $fileContent = Get-Content $($_.FullName) -Raw

      $StephenHodgson = "StephenHodgson"

      if ($fileContent -cmatch $StephenHodgson) {
        $fileContent -creplace $StephenHodgson, $InputAuthor | Set-Content $($_.FullName) -NoNewline
        $updated = $true
      }

      $fileContent = Get-Content $($_.FullName) -Raw

      # Rename all lowercase instances
      if ($fileContent -cmatch $ProjectName.ToLower()) {
        $fileContent -creplace $ProjectName.ToLower(), $InputName.ToLower() | Set-Content $($_.FullName) -NoNewline
        $updated = $true
      }

      $fileContent = Get-Content $($_.FullName) -Raw

      if ($fileContent -cmatch $ProjectScope.ToLower()) {
        $fileContent -creplace $ProjectScope.ToLower(), $InputScope.ToLower() | Set-Content $($_.FullName) -NoNewline
        $updated = $true
      }

      $fileContent = Get-Content $($_.FullName) -Raw

      # Rename all UPPERCASE instances
      if ($fileContent -cmatch $ProjectName.ToUpper()) {
        $fileContent -creplace $ProjectName.ToUpper(), $InputName.ToUpper() | Set-Content $($_.FullName) -NoNewline
        $updated = $true
      }

      $fileContent = Get-Content $($_.FullName) -Raw

      if ($fileContent -cmatch $ProjectScope.ToUpper()) {
        $fileContent -creplace $ProjectScope.ToUpper(), $InputScope.ToUpper() | Set-Content $($_.FullName) -NoNewline
        $updated = $true
      }

      $fileContent = Get-Content $($_.FullName) -Raw

      # Update guids
      if ($fileContent -match "#INSERT_GUID_HERE#") {
        $fileContent -replace "#INSERT_GUID_HERE#", [guid]::NewGuid() | Set-Content $($_.FullName) -NoNewline
        $updated = $true
      }

      $fileContent = Get-Content $($_.FullName) -Raw

      # Update year
      if ($fileContent -match "#CURRENT_YEAR#") {
        $fileContent -replace "#CURRENT_YEAR#", (Get-Date).year | Set-Content $($_.FullName) -NoNewline
        $updated = $true
      }

      # Rename files
      if ($_.Name -match $ProjectName) {
        Rename-Item -LiteralPath $_.FullName -NewName ($_.Name -replace ($ProjectName, $InputName))
        $updated = $true
      }

      if ($updated) {
        Write-Host $_.Name
      }
    }
  }
}

Remove-Item -Path "InitializeTemplate.ps1"
