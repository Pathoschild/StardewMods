. $PSScriptRoot/SemanticVersion.ps1
. $PSScriptRoot/ModProject.ps1

<#
.SYNOPSIS
Get all C# mod project files found in a given path.

.PARAMETER Path
The root path to search for project files.

.PARAMETER OnlyModZipped
Whether to only select mod projects which create a release zip.
#>
function Get-ModProjects {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$false)] [string] $Path = ".",
        [Parameter(Mandatory=$false)] [switch] $OnlyModZipped = $false
    )

    process {
        $projects = Get-ChildItem -Path $Path -Recurse -Filter *.csproj
        Select-ModProjects -Projects $projects -OnlyModZipped:$OnlyModZipped
    }
}

<#
.SYNOPSIS
Given a list of C# project files, returns metadata parsed from the project file relevant to
automated build pipelines. This doesn't support inherited build properties.

.PARAMETER Projects
The project files to read.

.PARAMETER OnlyModZipped
Whether to only select mod projects which create a release zip.
#>
function Select-ModProjects {
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline=$true)] [System.IO.FileInfo[]] $Projects,
        [Parameter(Mandatory=$false)] [switch] $OnlyModZipped = $false
    )

    process {
        foreach ($file in $Projects) {
            $xml = [xml](Get-Content $file.FullName)
            $versionNode = Select-Xml -Xml $xml -XPath '//Version' | Select-Object -Last 1
            $enableModZipNode = Select-Xml -Xml $xml -XPath '//EnableModZip' | Select-Object -Last 1

            $version = $versionNode ? $versionNode.Node.InnerText : $null
            $enableModZip = if ($enableModZipNode) { [bool]::Parse($enableModZipNode.Node.InnerText) } else { $true }

            if ($OnlyModZipped -and -not $enableModZip) {
                continue
            }

            [ModProject]::new($file, $xml, $version, $enableModZip)
        }
    }
}

<#
.SYNOPSIS
Increment the versions of C# mod project files to a timestamped prerelease version.

.PARAMETER ProjectPaths
The paths to the project files to set.
#>
function Set-PrereleaseModVersions {
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline=$true)] $Projects # note: can't set type to [ModProject[]] because the parameter is bound before the separate type is loaded from the separate file
    )

    process {
        $timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddHHmm')

        foreach ($project in $Projects) {
            if (!($project -is [ModProject])) {
                throw "Expected a ModProject, got $($project.GetType().FullName)"
            }

            # validate version
            if (!$project.RawVersion) {
                Write-Warning "Skipped $($project): no <Version> found."
                continue
            }
            if (!$project.ParsedVersion) {
                Write-Warning "Skipped $($project): version is set to non-semantic value '$($project.RawVersion)'."
                continue;
            }

            # build new version
            $oldVersion = $project.ParsedVersion
            $newPatch = $oldVersion.Patch
            if (!$oldVersion.Prerelease) {
                $newPatch += 1
            }
            $newVersion = "$($oldVersion.Major).$($oldVersion.Minor).$newPatch-alpha.$timestamp"

            # update project
            $project.FileXml.SelectNodes( '//Version' ) | Select-Object -Last 1 | ForEach-Object { $_.InnerText = $newVersion }
            $project.FileXml.Save($project.File.FullName)

            Write-Host "Updated $($project) -> '$newVersion'"
        }
    }
}

<#
.SYNOPSIS
Export a GitHub Actions output variable containing the base names for each zip file in a folder.
This can be used to run a job for each release zip.

.PARAMETER ReleasesPath
The path containing the release zip files.

.PARAMETER OutputName
The name of the GitHub Actions output variable to write.
#>
function Export-ModZipList {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$false)] [string] $ReleasesPath = "_releases",
        [Parameter(Mandatory=$false)] [string] $OutputName = "files"
    )

    if (-not (Test-Path $ReleasesPath)) {
        Write-Warning "Releases folder '$ReleasesPath' doesn't exist."
        return
    }

    # get base names
    $baseFileNames = Get-ChildItem -Path $ReleasesPath -Filter *.zip | ForEach-Object { $_.BaseName }

    # export to GitHub Actions output
    $json = $baseFileNames | ConvertTo-Json -Compress
    "$OutputName=$json" | Out-File -FilePath $env:GITHUB_OUTPUT -Append

    Write-Host "Exported $($baseFileNames.Count) release zips to GitHub Actions output '$OutputName'."
}

Export-ModuleMember -Function Get-ModProjects, Select-ModProjects, Set-PrereleaseModVersions, Export-ModZipList
